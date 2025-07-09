using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;
using Microsoft.Extensions.Logging;
using ElCriollo.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Servicio simplificado de gestión de órdenes para El Criollo
    /// </summary>
    public class OrdenService : IOrdenService
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly ElCriolloDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdenService> _logger;

        // Constantes básicas dominicanas
        private const decimal ITBIS_DOMINICANO = 0.18m; // 18%
        private const string PREFIJO_ORDEN = "ORD";
        private readonly string[] ESTADOS_VALIDOS = { "Pendiente", "EnPreparacion", "Lista", "Entregada", "Facturada", "Cancelada" };

        public OrdenService(
            IOrdenRepository ordenRepository,
            IProductoRepository productoRepository,
            IMesaRepository mesaRepository,
            IInventarioRepository inventarioRepository,
            ElCriolloDbContext context,
            IMapper mapper,
            ILogger<OrdenService> logger)
        {
            _ordenRepository = ordenRepository;
            _productoRepository = productoRepository;
            _mesaRepository = mesaRepository;
            _inventarioRepository = inventarioRepository;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Crea una nueva orden básica
        /// </summary>
        public async Task<OrdenResponse> CrearOrdenAsync(CreateOrdenRequest crearOrdenRequest, int usuarioId)
        {
            try
            {
                _logger.LogInformation("📋 Creando orden básica para mesa {MesaId} por usuario {UsuarioId}", 
                    crearOrdenRequest.MesaId, usuarioId);

                // Validación básica
                var validacion = await ValidarOrdenAsync(crearOrdenRequest);
                if (!validacion.EsValida)
                {
                    var errores = string.Join(", ", validacion.Errores);
                    throw new InvalidOperationException($"Orden inválida: {errores}");
                }

                // Verificar disponibilidad básica
                var disponibilidad = await VerificarDisponibilidadAsync(crearOrdenRequest.Items);
                if (!disponibilidad.TodoDisponible)
                {
                    var noDisponibles = string.Join(", ", disponibilidad.ProductosNoDisponibles);
                    throw new InvalidOperationException($"Productos no disponibles: {noDisponibles}");
                }

                // Generar número de orden único
                var numeroOrden = await GenerarNumeroOrdenAsync();

                // Crear la orden
                var nuevaOrden = new Orden
                {
                    NumeroOrden = numeroOrden,
                    MesaID = crearOrdenRequest.MesaId,
                    ClienteID = crearOrdenRequest.ClienteId,
                    EmpleadoID = usuarioId,
                    TipoOrden = crearOrdenRequest.TipoOrden ?? "Mesa",
                    FechaCreacion = DateTime.Now,
                    Estado = "Pendiente",
                    Observaciones = crearOrdenRequest.Observaciones
                };

                // Guardar orden
                var ordenCreada = await _ordenRepository.AddAsync(nuevaOrden);

                // Crear detalles de orden
                foreach (var item in crearOrdenRequest.Items)
                {
                    var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                    if (producto == null) continue;

                    var detalle = new DetalleOrden
                    {
                        OrdenID = ordenCreada.OrdenID,
                        ProductoID = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = item.Cantidad * producto.Precio,
                        NotasEspeciales = item.NotasEspeciales
                    };

                    await _ordenRepository.AddDetalleOrdenAsync(detalle);
                }

                // Recalcular totales
                await RecalcularTotalesAsync(ordenCreada.OrdenID);

                // Cambiar estado de mesa si es necesario
                if (crearOrdenRequest.MesaId.HasValue)
                {
                    await CambiarEstadoMesaAsync(crearOrdenRequest.MesaId.Value, "Ocupada");
                }

                _logger.LogInformation("✅ Orden {NumeroOrden} creada exitosamente", numeroOrden);

                var ordenCompleta = await _ordenRepository.GetByIdWithIncludesAsync(ordenCreada.OrdenID);
                return _mapper.Map<OrdenResponse>(ordenCompleta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear orden");
                throw;
            }
        }

        public async Task<OrdenResponse> ActualizarOrdenAsync(ActualizarOrdenRequest request, int usuarioId)
        {
            try
            {
                _logger.LogInformation("🔄 Actualizando orden {OrdenId} por usuario {UsuarioId}", request.OrdenID, usuarioId);

                var orden = await _ordenRepository.GetByIdWithIncludesAsync(request.OrdenID);
                if (orden == null)
                    throw new KeyNotFoundException($"Orden con ID {request.OrdenID} no encontrada.");

                if (orden.Estado != "Pendiente" && orden.Estado != "EnPreparacion")
                    throw new InvalidOperationException($"No se puede modificar una orden en estado '{orden.Estado}'.");

                // Actualizar observaciones de la orden
                orden.Observaciones = request.Observaciones;
                orden.FechaActualizacion = DateTime.Now;

                // Crear un diccionario de items actuales para comparación
                var itemsActuales = orden.DetalleOrdenes.ToDictionary(d => d.ProductoID ?? -1);
                var itemsRequest = request.Items.ToDictionary(i => i.ProductoId);

                // Eliminar items que ya no están en la request
                var itemsAEliminar = orden.DetalleOrdenes
                    .Where(d => d.ProductoID.HasValue && !itemsRequest.ContainsKey(d.ProductoID.Value))
                    .ToList();

                _logger.LogInformation("🗑️ Eliminando {Count} items de la orden {OrdenId}", itemsAEliminar.Count, request.OrdenID);
                foreach (var item in itemsAEliminar)
                {
                    orden.DetalleOrdenes.Remove(item);
                    _context.DetalleOrdenes.Remove(item);
                }

                // Actualizar items existentes
                var itemsActualizados = 0;
                foreach (var itemActual in orden.DetalleOrdenes.Where(d => d.ProductoID.HasValue))
                {
                    if (itemsRequest.ContainsKey(itemActual.ProductoID.Value))
                    {
                        var itemNuevo = itemsRequest[itemActual.ProductoID.Value];
                        
                        // Actualizar cantidad y recalcular subtotal
                        itemActual.Cantidad = itemNuevo.Cantidad;
                        itemActual.Subtotal = itemActual.Cantidad * itemActual.PrecioUnitario;
                        itemActual.NotasEspeciales = itemNuevo.NotasEspeciales;
                        
                        // Marcar como modificado
                        _context.Entry(itemActual).State = EntityState.Modified;
                        itemsActualizados++;
                    }
                }
                _logger.LogInformation("✏️ Actualizando {Count} items existentes de la orden {OrdenId}", itemsActualizados, request.OrdenID);
                
                // Agregar nuevos items
                var itemsNuevos = 0;
                foreach (var itemNuevo in request.Items)
                {
                    if (!itemsActuales.ContainsKey(itemNuevo.ProductoId))
                    {
                        var producto = await _productoRepository.GetByIdAsync(itemNuevo.ProductoId);
                        if (producto == null) continue;

                        var nuevoDetalle = new DetalleOrden
                        {
                            OrdenID = orden.OrdenID,
                            ProductoID = itemNuevo.ProductoId,
                            Cantidad = itemNuevo.Cantidad,
                            PrecioUnitario = producto.Precio,
                            NotasEspeciales = itemNuevo.NotasEspeciales,
                        };
                        
                        // Calcular subtotal manualmente para nuevos items
                        nuevoDetalle.Subtotal = nuevoDetalle.Cantidad * nuevoDetalle.PrecioUnitario;
                        
                        // Agregar a la colección de la orden y al contexto
                        orden.DetalleOrdenes.Add(nuevoDetalle);
                        _context.DetalleOrdenes.Add(nuevoDetalle);
                        itemsNuevos++;
                    }
                }
                _logger.LogInformation("➕ Agregando {Count} nuevos items a la orden {OrdenId}", itemsNuevos, request.OrdenID);

                // Marcar la orden como modificada
                _context.Entry(orden).State = EntityState.Modified;

                // Guardar todos los cambios de una vez
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("✅ Orden {OrdenId} actualizada correctamente.", request.OrdenID);
                
                // Recargar la orden con los cambios
                var ordenActualizada = await _ordenRepository.GetByIdWithIncludesAsync(request.OrdenID);
                return _mapper.Map<OrdenResponse>(ordenActualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar la orden {OrdenId}", request.OrdenID);
                throw;
            }
        }

        public async Task<OrdenResponse?> GetOrdenByIdAsync(int ordenId)
        {
            try
            {
                var orden = await _ordenRepository.GetByIdWithIncludesAsync(ordenId);
                if (orden == null) return null;

                // Recalcular totales para asegurar que estén actualizados
                await RecalcularTotalesAsync(ordenId);
                
                // Obtener la orden actualizada con totales recalculados
                var ordenActualizada = await _ordenRepository.GetByIdWithIncludesAsync(ordenId);
                return _mapper.Map<OrdenResponse>(ordenActualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener orden {OrdenId}", ordenId);
                throw;
            }
        }

        public async Task<OrdenResponse?> GetOrdenByNumeroAsync(string numeroOrden)
        {
            try
            {
                var orden = await _ordenRepository.GetByNumeroOrdenAsync(numeroOrden);
                return orden != null ? _mapper.Map<OrdenResponse>(orden) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener orden por número {NumeroOrden}", numeroOrden);
                throw;
            }
        }

        public async Task<bool> CancelarOrdenAsync(int ordenId, string motivo, int usuarioId)
        {
            try
            {
                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                {
                    _logger.LogWarning("⚠️ Orden {OrdenId} no encontrada", ordenId);
                    return false;
                }

                // Solo se puede cancelar si está en estado Pendiente
                if (orden.Estado != "Pendiente")
                {
                    _logger.LogWarning("⚠️ No se puede cancelar orden {OrdenId} en estado {Estado}", ordenId, orden.Estado);
                    return false;
                }

                orden.Estado = "Cancelada";
                orden.ObservacionesEspeciales = $"CANCELADA: {motivo}";
                orden.FechaActualizacion = DateTime.Now;

                await _ordenRepository.UpdateAsync(orden);

                // Liberar mesa si es necesario
                if (orden.MesaID.HasValue)
                {
                    await LiberarMesaSiEsNecesarioAsync(orden.MesaID.Value);
                }

                _logger.LogInformation("✅ Orden {OrdenId} cancelada: {Motivo}", ordenId, motivo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cancelar orden {OrdenId}", ordenId);
                return false;
            }
        }

        // ============================================================================
        // GESTIÓN DE ESTADOS
        // ============================================================================

        public async Task<bool> CambiarEstadoOrdenAsync(int ordenId, string nuevoEstado, int usuarioId)
        {
            try
            {
                if (!ESTADOS_VALIDOS.Contains(nuevoEstado))
                {
                    _logger.LogWarning("⚠️ Estado inválido: {Estado}", nuevoEstado);
                    return false;
                }

                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                {
                    _logger.LogWarning("⚠️ Orden {OrdenId} no encontrada", ordenId);
                    return false;
                }

                var estadoAnterior = orden.Estado;

                // Validar transición de estado básica
                if (!ValidarTransicionEstado(estadoAnterior, nuevoEstado))
                {
                    _logger.LogWarning("⚠️ Transición inválida de {EstadoActual} a {NuevoEstado}", estadoAnterior, nuevoEstado);
                    return false;
                }

                orden.Estado = nuevoEstado;
                orden.FechaActualizacion = DateTime.Now;

                await _ordenRepository.UpdateAsync(orden);

                _logger.LogInformation("✅ Orden {OrdenId} cambió de {EstadoAnterior} a {NuevoEstado}", 
                    ordenId, estadoAnterior, nuevoEstado);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar estado de orden {OrdenId}", ordenId);
                return false;
            }
        }

        public async Task<bool> IniciarPreparacionAsync(int ordenId, int usuarioId)
        {
            return await CambiarEstadoOrdenAsync(ordenId, "EnPreparacion", usuarioId);
        }

        public async Task<bool> MarcarOrdenListaAsync(int ordenId, int usuarioId)
        {
            return await CambiarEstadoOrdenAsync(ordenId, "Lista", usuarioId);
        }

        public async Task<bool> MarcarOrdenEntregadaAsync(int ordenId, int usuarioId)
        {
            return await CambiarEstadoOrdenAsync(ordenId, "Entregada", usuarioId);
        }

        // ============================================================================
        // CONSULTAS
        // ============================================================================

        public async Task<IEnumerable<OrdenResponse>> GetOrdenesActivasAsync()
        {
            try
            {
                var ordenes = await _ordenRepository.GetOrdenesActivasAsync();
                return _mapper.Map<IEnumerable<OrdenResponse>>(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes activas");
                throw;
            }
        }

        public async Task<IEnumerable<OrdenResponse>> GetOrdenesPorEstadoAsync(string estado)
        {
            try
            {
                var ordenes = await _ordenRepository.GetByEstadoAsync(estado);
                return _mapper.Map<IEnumerable<OrdenResponse>>(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes por estado {Estado}", estado);
                throw;
            }
        }

        public async Task<IEnumerable<OrdenResponse>> GetOrdenesPorMesaAsync(int mesaId)
        {
            try
            {
                var ordenes = await _ordenRepository.GetByMesaAsync(mesaId);
                return _mapper.Map<IEnumerable<OrdenResponse>>(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes de mesa {MesaId}", mesaId);
                throw;
            }
        }

        public async Task<IEnumerable<OrdenResponse>> GetOrdenesPorFechaAsync(DateTime fecha)
        {
            try
            {
                var ordenes = await _ordenRepository.GetOrdenesPorFechaAsync(fecha);
                return _mapper.Map<IEnumerable<OrdenResponse>>(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes de fecha {Fecha}", fecha);
                throw;
            }
        }

        public async Task<IEnumerable<OrdenResponse>> GetHistorialClienteAsync(int clienteId, int limite = 10)
        {
            try
            {
                var ordenes = await _ordenRepository.GetByClienteAsync(clienteId);
                var historial = ordenes.OrderByDescending(o => o.FechaCreacion).Take(limite);
                return _mapper.Map<IEnumerable<OrdenResponse>>(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener historial del cliente {ClienteId}", clienteId);
                throw;
            }
        }

        // ============================================================================
        // CÁLCULOS Y TOTALES
        // ============================================================================

        public async Task<CalculoOrdenResult> CalcularTotalOrdenAsync(int ordenId)
        {
            try
            {
                var orden = await _ordenRepository.GetByIdWithIncludesAsync(ordenId);
                if (orden == null)
                {
                    throw new ArgumentException($"Orden {ordenId} no encontrada");
                }

                var subtotal = orden.DetalleOrdenes?.Sum(d => d.Subtotal) ?? 0;
                var itbis = Math.Round(subtotal * ITBIS_DOMINICANO, 2);
                var total = subtotal + itbis;

                return new CalculoOrdenResult
                {
                    Subtotal = subtotal,
                    ITBIS = itbis,
                    Total = total,
                    CantidadItems = orden.DetalleOrdenes?.Sum(d => d.Cantidad) ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al calcular total de orden {OrdenId}", ordenId);
                throw;
            }
        }

        public async Task<bool> RecalcularTotalesAsync(int ordenId)
        {
            try
            {
                var calculo = await CalcularTotalOrdenAsync(ordenId);
                
                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null) return false;

                orden.Subtotal = calculo.Subtotal;
                orden.Impuesto = calculo.ITBIS;
                orden.Total = calculo.Total;

                await _ordenRepository.UpdateAsync(orden);

                _logger.LogInformation("✅ Totales recalculados para orden {OrdenId}: {Total}", ordenId, calculo.TotalFormateado);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al recalcular totales de orden {OrdenId}", ordenId);
                return false;
            }
        }

        // ============================================================================
        // VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Valida que una orden puede ser creada
        /// </summary>
        public async Task<ValidacionOrdenResult> ValidarOrdenAsync(CreateOrdenRequest crearOrdenRequest)
        {
            var resultado = new ValidacionOrdenResult { EsValida = true };

            try
            {
                // Validar que tenga items
                if (crearOrdenRequest.Items == null || !crearOrdenRequest.Items.Any())
                {
                    resultado.Errores.Add("La orden debe tener al menos un producto");
                    resultado.EsValida = false;
                }

                // Validar mesa si es necesario
                if (crearOrdenRequest.MesaId.HasValue)
                {
                    var mesa = await _mesaRepository.GetByIdAsync(crearOrdenRequest.MesaId.Value);
                    if (mesa == null)
                    {
                        resultado.Errores.Add("Mesa no encontrada");
                        resultado.EsValida = false;
                    }
                    else if (mesa.Estado == "Mantenimiento")
                    {
                        resultado.Errores.Add("Mesa en mantenimiento");
                        resultado.EsValida = false;
                    }
                }

                // Validar productos
                if (crearOrdenRequest.Items != null)
                {
                    foreach (var item in crearOrdenRequest.Items)
                    {
                        if (item.Cantidad <= 0)
                        {
                            resultado.Errores.Add($"Cantidad inválida para producto {item.ProductoId}");
                            resultado.EsValida = false;
                        }

                        var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                        if (producto == null)
                        {
                            resultado.Errores.Add($"Producto {item.ProductoId} no encontrado");
                            resultado.EsValida = false;
                        }
                        else if (!producto.Disponible)
                        {
                            resultado.Errores.Add($"Producto {producto.Nombre} no disponible");
                            resultado.EsValida = false;
                        }
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al validar orden");
                resultado.EsValida = false;
                resultado.Errores.Add("Error interno al validar orden");
                return resultado;
            }
        }

        public async Task<DisponibilidadResult> VerificarDisponibilidadAsync(List<ItemOrdenRequest> items)
        {
            var resultado = new DisponibilidadResult { TodoDisponible = true };

            try
            {
                foreach (var item in items)
                {
                    var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                    {
                        resultado.ProductosNoDisponibles.Add($"Producto ID {item.ProductoId}");
                        resultado.TodoDisponible = false;
                        continue;
                    }

                    if (!producto.Disponible)
                    {
                        resultado.ProductosNoDisponibles.Add(producto.Nombre);
                        resultado.TodoDisponible = false;
                        continue;
                    }

                    // Verificar stock básico
                    if (producto.Inventario != null)
                    {
                        if (producto.Inventario.CantidadDisponible < item.Cantidad)
                        {
                            resultado.ProductosNoDisponibles.Add($"{producto.Nombre} (Stock insuficiente)");
                            resultado.TodoDisponible = false;
                        }
                        else if (producto.Inventario.CantidadDisponible <= producto.Inventario.CantidadMinima)
                        {
                            resultado.ProductosStockBajo.Add(producto.Nombre);
                        }
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar disponibilidad");
                resultado.TodoDisponible = false;
                return resultado;
            }
        }

        // ============================================================================
        // DASHBOARD PARA COCINA
        // ============================================================================

        public async Task<DashboardCocinaBasicoViewModel> GetDashboardCocinaBasicoAsync()
        {
            try
            {
                var ordenesActivas = await _ordenRepository.GetOrdenesActivasAsync();
                
                var dashboard = new DashboardCocinaBasicoViewModel
                {
                    OrdenesPendientes = ordenesActivas.Count(o => o.Estado == "Pendiente"),
                    OrdenesEnPreparacion = ordenesActivas.Count(o => o.Estado == "EnPreparacion"),
                    OrdenesListas = ordenesActivas.Count(o => o.Estado == "Lista"),
                    TiempoPromedioPreparacion = "25 min" // Simplificado
                };

                // Órdenes urgentes (más de 30 minutos)
                var ordenesUrgentes = ordenesActivas
                    .Where(o => (DateTime.Now - o.FechaCreacion).TotalMinutes > 30 && o.Estado != "Lista")
                    .Take(5)
                    .Select(o => new OrdenCocinaBasicaResponse
                    {
                        OrdenID = o.OrdenID,
                        NumeroOrden = o.NumeroOrden,
                        NumeroMesa = o.Mesa?.NumeroMesa.ToString() ?? "S/M",
                        TiempoEspera = $"{Math.Round((DateTime.Now - o.FechaCreacion).TotalMinutes)} min",
                        Estado = o.Estado,
                        EsUrgente = true,
                        CantidadItems = o.DetalleOrdenes?.Sum(d => d.Cantidad) ?? 0
                    })
                    .ToList();

                dashboard.OrdenesUrgentes = ordenesUrgentes;

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar dashboard de cocina");
                throw;
            }
        }

        public async Task<IEnumerable<OrdenCocinaBasicaResponse>> GetColaCocinaBasicaAsync()
        {
            try
            {
                var ordenesPendientes = await _ordenRepository.GetByEstadoAsync("Pendiente");
                var ordenesEnPreparacion = await _ordenRepository.GetByEstadoAsync("EnPreparacion");
                
                var todasOrdenes = ordenesPendientes.Concat(ordenesEnPreparacion);

                return todasOrdenes
                    .OrderBy(o => o.FechaCreacion)
                    .Select(o => new OrdenCocinaBasicaResponse
                    {
                        OrdenID = o.OrdenID,
                        NumeroOrden = o.NumeroOrden,
                        NumeroMesa = o.Mesa?.NumeroMesa.ToString() ?? "Para llevar",
                        TiempoEspera = $"{Math.Round((DateTime.Now - o.FechaCreacion).TotalMinutes)} min",
                        Estado = o.Estado,
                        CantidadItems = o.DetalleOrdenes?.Sum(d => d.Cantidad) ?? 0,
                        EsUrgente = (DateTime.Now - o.FechaCreacion).TotalMinutes > 30,
                        NotasEspeciales = o.ObservacionesEspeciales,
                        ProductosResumen = o.DetalleOrdenes?.Take(3).Select(d => d.Producto?.Nombre ?? "Producto").ToList() ?? new List<string>()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener cola de cocina");
                throw;
            }
        }

        /// <summary>
        /// Agrega items a una orden existente
        /// </summary>
        public async Task<OrdenResponse> AgregarItemsOrdenAsync(int ordenId, List<ItemOrdenRequest> items, int usuarioId)
        {
            try
            {
                _logger.LogInformation("➕ Agregando {Count} items a orden {OrdenId}", items.Count, ordenId);

                // Verificar que la orden existe y está en estado válido
                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                {
                    throw new ArgumentException($"Orden con ID {ordenId} no encontrada");
                }

                if (orden.Estado != "Pendiente" && orden.Estado != "EnPreparacion")
                {
                    throw new InvalidOperationException($"No se pueden agregar items a una orden en estado {orden.Estado}");
                }

                // Validar disponibilidad de productos
                var disponibilidad = await VerificarDisponibilidadAsync(items);
                if (!disponibilidad.TodoDisponible)
                {
                    var noDisponibles = string.Join(", ", disponibilidad.ProductosNoDisponibles);
                    throw new InvalidOperationException($"Productos no disponibles: {noDisponibles}");
                }

                // Agregar cada item
                foreach (var item in items)
                {
                    var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                    if (producto == null) continue;

                    // Crear nuevo detalle siempre (simplificado para evitar duplicados complejos)
                    var nuevoDetalle = new DetalleOrden
                    {
                        OrdenID = ordenId,
                        ProductoID = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = item.Cantidad * producto.Precio,
                        NotasEspeciales = item.NotasEspeciales
                    };

                    await _ordenRepository.AddDetalleOrdenAsync(nuevoDetalle);
                }

                // Recalcular totales
                await RecalcularTotalesAsync(ordenId);

                // Obtener la orden actualizada
                var ordenActualizada = await _ordenRepository.GetByIdWithIncludesAsync(ordenId);
                var response = _mapper.Map<OrdenResponse>(ordenActualizada);

                _logger.LogInformation("✅ Items agregados exitosamente a orden {OrdenId}", ordenId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al agregar items a orden {OrdenId}", ordenId);
                throw;
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        private async Task<string> GenerarNumeroOrdenAsync()
        {
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var ordenes = await _ordenRepository.GetOrdenesPorFechaAsync(DateTime.Today);
            var secuencial = (ordenes.Count() + 1).ToString("D3");
            return $"{PREFIJO_ORDEN}-{fecha}-{secuencial}";
        }

        private bool ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            // Transiciones válidas simplificadas
            return estadoActual switch
            {
                "Pendiente" => nuevoEstado is "EnPreparacion" or "Cancelada",
                "EnPreparacion" => nuevoEstado is "Lista" or "Cancelada",
                "Lista" => nuevoEstado is "Entregada",
                "Entregada" => nuevoEstado is "Facturada",
                _ => false
            };
        }

        private async Task<bool> CambiarEstadoMesaAsync(int mesaId, string nuevoEstado)
        {
            try
            {
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null) return false;

                mesa.Estado = nuevoEstado;
                mesa.FechaUltimaActualizacion = DateTime.Now;
                await _mesaRepository.UpdateAsync(mesa);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> LiberarMesaSiEsNecesarioAsync(int mesaId)
        {
            try
            {
                var ordenesActivas = await _ordenRepository.GetByMesaAsync(mesaId);
                var tieneOrdenesActivas = ordenesActivas.Any(o => o.Estado != "Facturada" && o.Estado != "Cancelada");
                
                if (!tieneOrdenesActivas)
                {
                    await CambiarEstadoMesaAsync(mesaId, "Libre");
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}