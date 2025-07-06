using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de órdenes para El Criollo
    /// Maneja el flujo completo de comandas, validaciones y integración con cocina
    /// </summary>
    public class OrdenService : IOrdenService
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProductoService _productoService;
        private readonly IMesaService _mesaService;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdenService> _logger;

        // Configuración específica para El Criollo
        private const decimal ITBIS_DOMINICANO = 0.18m; // 18%
        private const int TIEMPO_LIMITE_MODIFICACION_MINUTOS = 10;
        private const int DESCUENTO_VOLUMEN_MINIMO = 1000; // RD$ 1,000
        private const decimal DESCUENTO_VOLUMEN_PORCENTAJE = 0.05m; // 5%

        // Tiempos promedio de preparación por categoría (minutos)
        private readonly Dictionary<string, int> TIEMPOS_PREPARACION = new()
        {
            ["Platos Principales"] = 25,
            ["Sopas"] = 20,
            ["Mariscos"] = 30,
            ["Frituras"] = 15,
            ["Acompañamientos"] = 10,
            ["Bebidas"] = 5,
            ["Postres"] = 10,
            ["Desayunos"] = 20
        };

        public OrdenService(
            IOrdenRepository ordenRepository,
            IProductoService productoService,
            IMesaService mesaService,
            IInventarioRepository inventarioRepository,
            IMapper mapper,
            ILogger<OrdenService> logger)
        {
            _ordenRepository = ordenRepository;
            _productoService = productoService;
            _mesaService = mesaService;
            _inventarioRepository = inventarioRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // CREACIÓN Y GESTIÓN DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Crea una nueva orden con validación completa de productos y stock
        /// </summary>
        public async Task<OrdenResponse> CrearOrdenAsync(CrearOrdenRequest crearOrdenRequest, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando nueva orden para mesa {MesaId} por usuario {UsuarioId}", 
                    crearOrdenRequest.MesaId, usuarioId);

                // Validar la orden completa
                var validacion = await ValidarOrdenAsync(crearOrdenRequest);
                if (!validacion.EsValida)
                {
                    var errores = string.Join(", ", validacion.Errores);
                    throw new InvalidOperationException($"Orden inválida: {errores}");
                }

                // Verificar disponibilidad de productos
                var disponibilidad = await VerificarDisponibilidadOrdenAsync(crearOrdenRequest.Items);
                if (!disponibilidad.TodoDisponible)
                {
                    throw new InvalidOperationException($"Productos no disponibles: {string.Join(", ", disponibilidad.ProductosNoDisponibles)}");
                }

                // Calcular totales
                var calculo = await CalcularTotalOrdenAsync(crearOrdenRequest.Items, true);

                // Crear la orden principal
                var nuevaOrden = new Orden
                {
                    NumeroOrden = await GenerarNumeroOrdenAsync(),
                    MesaId = crearOrdenRequest.MesaId,
                    ClienteId = crearOrdenRequest.ClienteId,
                    TipoOrden = crearOrdenRequest.TipoOrden.ToString(),
                    Estado = EstadoOrden.Pendiente.ToString(),
                    Subtotal = calculo.Subtotal,
                    Descuentos = calculo.Descuentos,
                    ITBIS = calculo.ITBIS,
                    Total = calculo.Total,
                    NotasEspeciales = crearOrdenRequest.NotasEspeciales,
                    TiempoEstimadoPreparacion = await EstimarTiempoPreparacionAsync(crearOrdenRequest.Items),
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = usuarioId
                };

                var ordenCreada = await _ordenRepository.CreateAsync(nuevaOrden);

                // Crear detalles de la orden
                foreach (var item in crearOrdenRequest.Items)
                {
                    var detalleOrden = new DetalleOrden
                    {
                        OrdenId = ordenCreada.Id,
                        ProductoId = item.ProductoId,
                        ComboId = item.ComboId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario ?? await ObtenerPrecioProductoAsync(item.ProductoId, item.ComboId),
                        NotasEspeciales = item.NotasEspeciales,
                        FechaCreacion = DateTime.UtcNow
                    };

                    detalleOrden.PrecioTotal = detalleOrden.PrecioUnitario * detalleOrden.Cantidad;
                    await _ordenRepository.CreateDetalleOrdenAsync(detalleOrden);
                }

                // Actualizar estado de la mesa si es necesario
                if (crearOrdenRequest.MesaId.HasValue)
                {
                    await _mesaService.AsignarOrdenAMesaAsync(crearOrdenRequest.MesaId.Value, ordenCreada.Id, usuarioId);
                }

                // Reservar stock temporalmente
                var itemsStock = crearOrdenRequest.Items.Select(i => new ItemStock 
                { 
                    ProductoId = i.ProductoId, 
                    Cantidad = i.Cantidad 
                }).ToList();
                
                await _productoService.ReservarStockTemporalAsync(itemsStock);

                _logger.LogInformation("Orden creada exitosamente: {OrdenId} - {NumeroOrden} - Total: RD$ {Total}", 
                    ordenCreada.Id, ordenCreada.NumeroOrden, ordenCreada.Total);

                // Notificar a cocina
                await NotificarCambioEstadoAsync(ordenCreada.Id, EstadoOrden.Pendiente, usuarioId);

                return _mapper.Map<OrdenResponse>(ordenCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden para mesa {MesaId}", crearOrdenRequest.MesaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una orden específica con todos sus detalles
        /// </summary>
        public async Task<OrdenResponse?> GetOrdenByIdAsync(int ordenId)
        {
            try
            {
                _logger.LogDebug("Obteniendo orden ID: {OrdenId}", ordenId);

                var orden = await _ordenRepository.GetByIdWithDetallesAsync(ordenId);
                if (orden == null)
                {
                    _logger.LogWarning("Orden no encontrada: {OrdenId}", ordenId);
                    return null;
                }

                var ordenResponse = _mapper.Map<OrdenResponse>(orden);

                // Enriquecer con información adicional
                if (orden.MesaId.HasValue)
                {
                    var mesaDetalle = await _mesaService.GetMesaDetalleAsync(orden.MesaId.Value);
                    ordenResponse.Mesa = mesaDetalle?.Mesa;
                }

                // Calcular tiempo transcurrido
                ordenResponse.TiempoTranscurrido = DateTime.UtcNow - orden.FechaCreacion;

                // Calcular progreso de preparación
                ordenResponse.ProgresoPreparacion = CalcularProgresoPreparacion(orden);

                return ordenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los items de una orden existente
        /// </summary>
        public async Task<OrdenResponse?> ActualizarItemsOrdenAsync(int ordenId, List<ItemOrdenRequest> nuevosItems, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando items de orden {OrdenId} por usuario {UsuarioId}", ordenId, usuarioId);

                var orden = await _ordenRepository.GetByIdWithDetallesAsync(ordenId);
                if (orden == null)
                {
                    _logger.LogWarning("Orden no encontrada para actualizar: {OrdenId}", ordenId);
                    return null;
                }

                // Verificar que la orden se puede modificar
                if (!PuedeModificarOrden(orden))
                {
                    throw new InvalidOperationException($"No se puede modificar la orden en estado {orden.Estado}");
                }

                // Verificar tiempo límite de modificación
                var tiempoTranscurrido = DateTime.UtcNow - orden.FechaCreacion;
                if (tiempoTranscurrido.TotalMinutes > TIEMPO_LIMITE_MODIFICACION_MINUTOS)
                {
                    throw new InvalidOperationException($"No se puede modificar la orden después de {TIEMPO_LIMITE_MODIFICACION_MINUTOS} minutos");
                }

                // Eliminar detalles actuales
                await _ordenRepository.EliminarDetallesOrdenAsync(ordenId);

                // Liberar stock de items anteriores
                // TODO: Implementar liberación de stock reservado

                // Recalcular totales con nuevos items
                var calculo = await CalcularTotalOrdenAsync(nuevosItems, true);

                // Actualizar la orden
                orden.Subtotal = calculo.Subtotal;
                orden.Descuentos = calculo.Descuentos;
                orden.ITBIS = calculo.ITBIS;
                orden.Total = calculo.Total;
                orden.TiempoEstimadoPreparacion = await EstimarTiempoPreparacionAsync(nuevosItems);
                orden.FechaModificacion = DateTime.UtcNow;

                await _ordenRepository.UpdateAsync(orden);

                // Crear nuevos detalles
                foreach (var item in nuevosItems)
                {
                    var detalleOrden = new DetalleOrden
                    {
                        OrdenId = ordenId,
                        ProductoId = item.ProductoId,
                        ComboId = item.ComboId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario ?? await ObtenerPrecioProductoAsync(item.ProductoId, item.ComboId),
                        NotasEspeciales = item.NotasEspeciales,
                        FechaCreacion = DateTime.UtcNow
                    };

                    detalleOrden.PrecioTotal = detalleOrden.PrecioUnitario * detalleOrden.Cantidad;
                    await _ordenRepository.CreateDetalleOrdenAsync(detalleOrden);
                }

                // Reservar nuevo stock
                var itemsStock = nuevosItems.Select(i => new ItemStock 
                { 
                    ProductoId = i.ProductoId, 
                    Cantidad = i.Cantidad 
                }).ToList();
                
                await _productoService.ReservarStockTemporalAsync(itemsStock);

                _logger.LogInformation("Items de orden actualizados exitosamente: {OrdenId} - Nuevo total: RD$ {Total}", 
                    ordenId, orden.Total);

                return _mapper.Map<OrdenResponse>(orden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar items de orden {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Cancela una orden completa
        /// </summary>
        public async Task<CancelacionOrdenResult> CancelarOrdenAsync(int ordenId, string motivo, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Cancelando orden {OrdenId} por usuario {UsuarioId}. Motivo: {Motivo}", 
                    ordenId, usuarioId, motivo);

                var orden = await _ordenRepository.GetByIdWithDetallesAsync(ordenId);
                if (orden == null)
                {
                    return new CancelacionOrdenResult
                    {
                        Exitoso = false,
                        Mensaje = "Orden no encontrada"
                    };
                }

                // Verificar que la orden se puede cancelar
                if (!PuedeCancelarOrden(orden))
                {
                    return new CancelacionOrdenResult
                    {
                        Exitoso = false,
                        Mensaje = $"No se puede cancelar la orden en estado {orden.Estado}"
                    };
                }

                // Calcular reembolso si aplica
                var montoReembolso = orden.Estado == "Pendiente" ? orden.Total : 0;

                // Cancelar la orden
                orden.Estado = EstadoOrden.Cancelada.ToString();
                orden.NotasEspeciales = $"{orden.NotasEspeciales}\nCANCELADA: {motivo}";
                orden.FechaModificacion = DateTime.UtcNow;

                await _ordenRepository.UpdateAsync(orden);

                // Restaurar stock si la orden no había iniciado preparación
                bool stockRestaurado = false;
                if (orden.Estado == "Pendiente")
                {
                    stockRestaurado = await RestaurarStockOrdenAsync(orden);
                }

                // Liberar mesa si es necesario
                if (orden.MesaId.HasValue)
                {
                    var otrasOrdenes = await GetOrdenesPorMesaAsync(orden.MesaId.Value);
                    if (!otrasOrdenes.Any(o => o.Estado != "Cancelada" && o.Estado != "Facturada"))
                    {
                        await _mesaService.LiberarMesaAsync(orden.MesaId.Value, usuarioId);
                    }
                }

                var resultado = new CancelacionOrdenResult
                {
                    Exitoso = true,
                    Mensaje = $"Orden {orden.NumeroOrden} cancelada exitosamente",
                    MontoReembolso = montoReembolso,
                    StockRestaurado = stockRestaurado,
                    FechaCancelacion = DateTime.UtcNow
                };

                _logger.LogInformation("Orden cancelada exitosamente: {OrdenId} - Reembolso: RD$ {MontoReembolso}", 
                    ordenId, montoReembolso);

                // Notificar cancelación
                await NotificarCambioEstadoAsync(ordenId, EstadoOrden.Cancelada, usuarioId);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar orden {OrdenId}", ordenId);
                return new CancelacionOrdenResult
                {
                    Exitoso = false,
                    Mensaje = "Error interno al cancelar orden"
                };
            }
        }

        /// <summary>
        /// Duplica una orden existente para repetir pedido
        /// </summary>
        public async Task<OrdenResponse> DuplicarOrdenAsync(int ordenIdOriginal, int? mesaId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Duplicando orden {OrdenIdOriginal} por usuario {UsuarioId}", ordenIdOriginal, usuarioId);

                var ordenOriginal = await _ordenRepository.GetByIdWithDetallesAsync(ordenIdOriginal);
                if (ordenOriginal == null)
                {
                    throw new InvalidOperationException("Orden original no encontrada");
                }

                // Crear items basados en la orden original
                var itemsOrigenales = ordenOriginal.DetalleOrdenes?.Select(d => new ItemOrdenRequest
                {
                    ProductoId = d.ProductoId,
                    ComboId = d.ComboId,
                    Cantidad = d.Cantidad,
                    NotasEspeciales = d.NotasEspeciales,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList() ?? new List<ItemOrdenRequest>();

                // Crear nueva orden con los mismos datos
                var crearOrdenRequest = new CrearOrdenRequest
                {
                    MesaId = mesaId ?? ordenOriginal.MesaId,
                    ClienteId = ordenOriginal.ClienteId,
                    TipoOrden = Enum.Parse<TipoOrden>(ordenOriginal.TipoOrden),
                    Items = itemsOrigenales,
                    NotasEspeciales = $"Duplicada de orden {ordenOriginal.NumeroOrden}"
                };

                var nuevaOrden = await CrearOrdenAsync(crearOrdenRequest, usuarioId);

                _logger.LogInformation("Orden duplicada exitosamente: Original {OrdenIdOriginal} → Nueva {NuevaOrdenId}", 
                    ordenIdOriginal, nuevaOrden.Id);

                return nuevaOrden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al duplicar orden {OrdenIdOriginal}", ordenIdOriginal);
                throw;
            }
        }

        // ============================================================================
        // FLUJO DE ESTADOS DE ORDEN
        // ============================================================================

        /// <summary>
        /// Cambia el estado de una orden siguiendo el flujo definido
        /// </summary>
        public async Task<CambioEstadoOrdenResult> CambiarEstadoOrdenAsync(int ordenId, EstadoOrden nuevoEstado, int usuarioId, string? notas = null)
        {
            try
            {
                _logger.LogInformation("Cambiando estado de orden {OrdenId} a {NuevoEstado} por usuario {UsuarioId}", 
                    ordenId, nuevoEstado, usuarioId);

                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                {
                    return new CambioEstadoOrdenResult
                    {
                        Exitoso = false,
                        Mensaje = "Orden no encontrada"
                    };
                }

                var estadoAnterior = Enum.Parse<EstadoOrden>(orden.Estado);

                // Validar transición de estado
                var validacion = ValidarCambioEstado(estadoAnterior, nuevoEstado);
                if (!validacion.esValido)
                {
                    return new CambioEstadoOrdenResult
                    {
                        Exitoso = false,
                        Mensaje = validacion.mensaje,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = nuevoEstado
                    };
                }

                // Ejecutar acciones específicas según el nuevo estado
                await EjecutarAccionesEstadoAsync(orden, nuevoEstado, usuarioId);

                // Actualizar estado
                orden.Estado = nuevoEstado.ToString();
                orden.FechaModificacion = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(notas))
                {
                    orden.NotasEspeciales = $"{orden.NotasEspeciales}\n{DateTime.UtcNow:HH:mm}: {notas}";
                }

                await _ordenRepository.UpdateAsync(orden);

                var resultado = new CambioEstadoOrdenResult
                {
                    Exitoso = true,
                    Mensaje = $"Orden {orden.NumeroOrden} cambiada de {estadoAnterior} a {nuevoEstado}",
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = nuevoEstado,
                    FechaCambio = DateTime.UtcNow,
                    Usuario = $"Usuario ID: {usuarioId}",
                    TiempoEstimadoMinutos = nuevoEstado == EstadoOrden.EnPreparacion ? orden.TiempoEstimadoPreparacion : null
                };

                _logger.LogInformation("Estado de orden cambiado exitosamente: {OrdenId} de {EstadoAnterior} a {EstadoNuevo}", 
                    ordenId, estadoAnterior, nuevoEstado);

                // Notificar cambio
                await NotificarCambioEstadoAsync(ordenId, nuevoEstado, usuarioId);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de orden {OrdenId}", ordenId);
                return new CambioEstadoOrdenResult
                {
                    Exitoso = false,
                    Mensaje = "Error interno al cambiar estado"
                };
            }
        }

        /// <summary>
        /// Marca una orden como "En Preparación"
        /// </summary>
        public async Task<bool> IniciarPreparacionAsync(int ordenId, int usuarioId, int? tiempoEstimadoMinutos = null)
        {
            try
            {
                _logger.LogInformation("Iniciando preparación de orden {OrdenId} por usuario {UsuarioId}", ordenId, usuarioId);

                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                    return false;

                // Actualizar tiempo estimado si se proporciona
                if (tiempoEstimadoMinutos.HasValue)
                {
                    orden.TiempoEstimadoPreparacion = tiempoEstimadoMinutos.Value;
                }

                // Confirmar reserva de stock (descontar del inventario)
                await ConfirmarStockOrdenAsync(orden);

                var resultado = await CambiarEstadoOrdenAsync(ordenId, EstadoOrden.EnPreparacion, usuarioId, 
                    $"Iniciada preparación. Tiempo estimado: {orden.TiempoEstimadoPreparacion} minutos");

                return resultado.Exitoso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar preparación de orden {OrdenId}", ordenId);
                return false;
            }
        }

        /// <summary>
        /// Marca una orden como "Lista"
        /// </summary>
        public async Task<bool> MarcarOrdenListaAsync(int ordenId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Marcando orden {OrdenId} como lista por usuario {UsuarioId}", ordenId, usuarioId);

                var resultado = await CambiarEstadoOrdenAsync(ordenId, EstadoOrden.Lista, usuarioId, "Orden lista para servir");

                return resultado.Exitoso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar orden {OrdenId} como lista", ordenId);
                return false;
            }
        }

        /// <summary>
        /// Marca una orden como "Entregada"
        /// </summary>
        public async Task<bool> MarcarOrdenEntregadaAsync(int ordenId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Marcando orden {OrdenId} como entregada por usuario {UsuarioId}", ordenId, usuarioId);

                var resultado = await CambiarEstadoOrdenAsync(ordenId, EstadoOrden.Entregada, usuarioId, "Orden entregada al cliente");

                // Verificar si todas las órdenes de la mesa están entregadas para preparar facturación
                if (resultado.Exitoso)
                {
                    var orden = await _ordenRepository.GetByIdAsync(ordenId);
                    if (orden?.MesaId.HasValue == true)
                    {
                        await VerificarEstadoMesaParaFacturacionAsync(orden.MesaId.Value);
                    }
                }

                return resultado.Exitoso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar orden {OrdenId} como entregada", ordenId);
                return false;
            }
        }

        /// <summary>
        /// Obtiene todas las órdenes filtradas por estado
        /// </summary>
        public async Task<IEnumerable<OrdenResponse>> GetOrdenesPorEstadoAsync(EstadoOrden estado, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes por estado: {Estado}", estado);

                var estadoString = estado.ToString();
                var ordenes = await _ordenRepository.GetOrdenesPorEstadoAsync(estadoString, fechaInicio, fechaFin);

                var ordenesResponse = _mapper.Map<List<OrdenResponse>>(ordenes);

                // Enriquecer con información adicional
                foreach (var orden in ordenesResponse)
                {
                    var ordenEntity = ordenes.FirstOrDefault(o => o.Id == orden.Id);
                    if (ordenEntity != null)
                    {
                        orden.TiempoTranscurrido = DateTime.UtcNow - ordenEntity.FechaCreacion;
                        orden.ProgresoPreparacion = CalcularProgresoPreparacion(ordenEntity);
                    }
                }

                _logger.LogDebug("Encontradas {Count} órdenes en estado {Estado}", ordenesResponse.Count, estado);

                return ordenesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes por estado {Estado}", estado);
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE ÓRDENES POR MESA
        // ============================================================================

        /// <summary>
        /// Obtiene todas las órdenes activas de una mesa específica
        /// </summary>
        public async Task<IEnumerable<OrdenResponse>> GetOrdenesPorMesaAsync(int mesaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes activas para mesa {MesaId}", mesaId);

                var ordenes = await _ordenRepository.GetOrdenesPorMesaAsync(mesaId);
                var ordenesActivas = ordenes.Where(o => o.Estado != "Cancelada");

                var ordenesResponse = _mapper.Map<List<OrdenResponse>>(ordenesActivas);

                // Enriquecer con información de tiempo
                foreach (var orden in ordenesResponse)
                {
                    var ordenEntity = ordenesActivas.FirstOrDefault(o => o.Id == orden.Id);
                    if (ordenEntity != null)
                    {
                        orden.TiempoTranscurrido = DateTime.UtcNow - ordenEntity.FechaCreacion;
                        orden.ProgresoPreparacion = CalcularProgresoPreparacion(ordenEntity);
                    }
                }

                _logger.LogDebug("Encontradas {Count} órdenes activas para mesa {MesaId}", ordenesResponse.Count, mesaId);

                return ordenesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de mesa {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Crea una orden grupal para múltiples personas en una mesa
        /// </summary>
        public async Task<OrdenGrupalResult> CrearOrdenGrupalAsync(int mesaId, List<CrearOrdenRequest> ordenesIndividuales, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando orden grupal para mesa {MesaId} con {Count} órdenes individuales", 
                    mesaId, ordenesIndividuales.Count);

                var resultado = new OrdenGrupalResult();

                // Crear orden principal consolidada
                var todosLosItems = ordenesIndividuales.SelectMany(o => o.Items).ToList();
                var itemsConsolidados = ConsolidarItems(todosLosItems);

                var ordenPrincipalRequest = new CrearOrdenRequest
                {
                    MesaId = mesaId,
                    TipoOrden = TipoOrden.Mesa,
                    Items = itemsConsolidados,
                    NotasEspeciales = $"Orden grupal - {ordenesIndividuales.Count} personas"
                };

                resultado.OrdenPrincipal = await CrearOrdenAsync(ordenPrincipalRequest, usuarioId);

                // Crear órdenes individuales como referencia
                foreach (var ordenIndividual in ordenesIndividuales)
                {
                    ordenIndividual.MesaId = mesaId;
                    var ordenCreada = await CrearOrdenAsync(ordenIndividual, usuarioId);
                    resultado.OrdenesIndividuales.Add(ordenCreada);
                }

                resultado.TotalGrupal = resultado.OrdenPrincipal.Total + resultado.OrdenesIndividuales.Sum(o => o.Total);
                resultado.Exitoso = true;
                resultado.Mensaje = $"Orden grupal creada exitosamente para {ordenesIndividuales.Count} personas";

                _logger.LogInformation("Orden grupal creada: Mesa {MesaId} - Total: RD$ {Total}", 
                    mesaId, resultado.TotalGrupal);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden grupal para mesa {MesaId}", mesaId);
                return new OrdenGrupalResult
                {
                    Exitoso = false,
                    Mensaje = "Error al crear orden grupal"
                };
            }
        }

        /// <summary>
        /// Divide una orden grupal en órdenes individuales
        /// </summary>
        public async Task<DivisionOrdenResult> DividirOrdenGrupalAsync(int ordenGrupalId, DivisionOrdenRequest division, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Dividiendo orden grupal {OrdenGrupalId} por usuario {UsuarioId}", ordenGrupalId, usuarioId);

                var ordenGrupal = await _ordenRepository.GetByIdWithDetallesAsync(ordenGrupalId);
                if (ordenGrupal == null)
                {
                    return new DivisionOrdenResult
                    {
                        Exitoso = false,
                        Mensaje = "Orden grupal no encontrada"
                    };
                }

                var resultado = new DivisionOrdenResult
                {
                    TotalOriginal = ordenGrupal.Total
                };

                if (division.DividirEquitativamente)
                {
                    // División equitativa
                    var cantidadPersonas = division.Divisiones.Count;
                    var montoPorPersona = ordenGrupal.Total / cantidadPersonas;

                    foreach (var persona in division.Divisiones)
                    {
                        var ordenIndividual = await CrearOrdenIndividualDividida(
                            ordenGrupal, persona.NombrePersona, montoPorPersona, usuarioId);
                        resultado.OrdenesIndividuales.Add(ordenIndividual);
                    }
                }
                else
                {
                    // División por items específicos
                    foreach (var persona in division.Divisiones)
                    {
                        var itemsPersona = ordenGrupal.DetalleOrdenes?
                            .Where(d => persona.ItemsIds.Contains(d.Id))
                            .ToList() ?? new List<DetalleOrden>();

                        var montoPersona = persona.MontoFijo ?? itemsPersona.Sum(i => i.PrecioTotal);

                        var ordenIndividual = await CrearOrdenIndividualDividida(
                            ordenGrupal, persona.NombrePersona, montoPersona, usuarioId);
                        resultado.OrdenesIndividuales.Add(ordenIndividual);
                    }
                }

                resultado.TotalDividido = resultado.OrdenesIndividuales.Sum(o => o.Total);
                resultado.Exitoso = true;
                resultado.Mensaje = $"Orden dividida en {resultado.OrdenesIndividuales.Count} facturas individuales";

                // Marcar orden original como facturada
                await CambiarEstadoOrdenAsync(ordenGrupalId, EstadoOrden.Facturada, usuarioId, "Dividida en facturas individuales");

                _logger.LogInformation("Orden grupal dividida exitosamente: {OrdenGrupalId} → {Count} órdenes individuales", 
                    ordenGrupalId, resultado.OrdenesIndividuales.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al dividir orden grupal {OrdenGrupalId}", ordenGrupalId);
                return new DivisionOrdenResult
                {
                    Exitoso = false,
                    Mensaje = "Error al dividir orden grupal"
                };
            }
        }

        /// <summary>
        /// Consolida múltiples órdenes de una mesa en una sola factura
        /// </summary>
        public async Task<OrdenConsolidadaResult> ConsolidarOrdenesAsync(int mesaId, List<int> ordenesIds, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Consolidando {Count} órdenes de mesa {MesaId} por usuario {UsuarioId}", 
                    ordenesIds.Count, mesaId, usuarioId);

                var ordenes = new List<Orden>();
                foreach (var ordenId in ordenesIds)
                {
                    var orden = await _ordenRepository.GetByIdWithDetallesAsync(ordenId);
                    if (orden != null)
                        ordenes.Add(orden);
                }

                if (!ordenes.Any())
                {
                    return new OrdenConsolidadaResult
                    {
                        Exitoso = false,
                        Mensaje = "No se encontraron órdenes válidas para consolidar"
                    };
                }

                // Consolidar todos los items
                var todosLosItems = ordenes.SelectMany(o => o.DetalleOrdenes ?? new List<DetalleOrden>()).ToList();
                var itemsConsolidados = ConsolidarItemsDetalles(todosLosItems);

                // Crear orden consolidada
                var ordenConsolidada = new Orden
                {
                    NumeroOrden = await GenerarNumeroOrdenAsync(),
                    MesaId = mesaId,
                    TipoOrden = "Mesa",
                    Estado = EstadoOrden.Entregada.ToString(),
                    Subtotal = ordenes.Sum(o => o.Subtotal),
                    Descuentos = ordenes.Sum(o => o.Descuentos),
                    ITBIS = ordenes.Sum(o => o.ITBIS),
                    Total = ordenes.Sum(o => o.Total),
                    NotasEspeciales = $"Consolidación de órdenes: {string.Join(", ", ordenes.Select(o => o.NumeroOrden))}",
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = usuarioId
                };

                var ordenCreada = await _ordenRepository.CreateAsync(ordenConsolidada);

                // Crear detalles consolidados
                foreach (var item in itemsConsolidados)
                {
                    await _ordenRepository.CreateDetalleOrdenAsync(new DetalleOrden
                    {
                        OrdenId = ordenCreada.Id,
                        ProductoId = item.ProductoId,
                        ComboId = item.ComboId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        PrecioTotal = item.PrecioTotal,
                        NotasEspeciales = item.NotasEspeciales,
                        FechaCreacion = DateTime.UtcNow
                    });
                }

                // Marcar órdenes originales como facturadas
                foreach (var orden in ordenes)
                {
                    await CambiarEstadoOrdenAsync(orden.Id, EstadoOrden.Facturada, usuarioId, "Consolidada en factura única");
                }

                var resultado = new OrdenConsolidadaResult
                {
                    Exitoso = true,
                    OrdenConsolidada = _mapper.Map<OrdenResponse>(ordenCreada),
                    OrdenesOriginalesIds = ordenesIds,
                    TotalConsolidado = ordenCreada.Total,
                    Mensaje = $"Órdenes consolidadas exitosamente en factura {ordenCreada.NumeroOrden}"
                };

                _logger.LogInformation("Órdenes consolidadas exitosamente: Mesa {MesaId} - Total: RD$ {Total}", 
                    mesaId, resultado.TotalConsolidado);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consolidar órdenes de mesa {MesaId}", mesaId);
                return new OrdenConsolidadaResult
                {
                    Exitoso = false,
                    Mensaje = "Error al consolidar órdenes"
                };
            }
        }

        // ============================================================================
        // DASHBOARD Y MONITOREO DE COCINA
        // ============================================================================

        /// <summary>
        /// Obtiene el dashboard de cocina con órdenes en preparación
        /// </summary>
        public async Task<DashboardCocinaViewModel> GetDashboardCocinaAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo dashboard de cocina");

                var ordenesPendientes = await GetOrdenesPorEstadoAsync(EstadoOrden.Pendiente);
                var ordenesEnPreparacion = await GetOrdenesPorEstadoAsync(EstadoOrden.EnPreparacion);
                var ordenesListas = await GetOrdenesPorEstadoAsync(EstadoOrden.Lista);

                var estadisticas = await GetEstadisticasCocinaAsync();

                var dashboard = new DashboardCocinaViewModel
                {
                    FechaActualizacion = DateTime.UtcNow,
                    OrdenesPendientes = ordenesPendientes.Count(),
                    OrdenesEnPreparacion = ordenesEnPreparacion.Count(),
                    OrdenesListasParaServir = ordenesListas.Count(),
                    TiempoPromedioPreparacion = estadisticas.TiempoPromedioPreparacion,
                    TiempoEsperaMaximo = estadisticas.TiempoEsperaMaximo,
                    PorcentajeEficiencia = estadisticas.PorcentajeEficiencia,
                    ColaOrdenes = await GetColaOrdensCocinaAsync(),
                    OrdenesListas = ordenesListas.Take(10).ToList(),
                    ProductosMasSolicitados = estadisticas.ProductosMasSolicitados.Take(5).ToList()
                };

                _logger.LogDebug("Dashboard de cocina generado: {Pendientes} pendientes, {EnPreparacion} en preparación, {Listas} listas", 
                    dashboard.OrdenesPendientes, dashboard.OrdenesEnPreparacion, dashboard.OrdenesListasParaServir);

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard de cocina");
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes pendientes de preparación (cola de cocina)
        /// </summary>
        public async Task<IEnumerable<OrdenCocinaResponse>> GetColaOrdensCocinaAsync(TipoOrden? filtrarPorTipo = null)
        {
            try
            {
                _logger.LogDebug("Obteniendo cola de órdenes para cocina");

                var ordenesPendientes = await GetOrdenesPorEstadoAsync(EstadoOrden.Pendiente);
                var ordenesEnPreparacion = await GetOrdenesPorEstadoAsync(EstadoOrden.EnPreparacion);

                var todasLasOrdenes = ordenesPendientes.Concat(ordenesEnPreparacion);

                if (filtrarPorTipo.HasValue)
                {
                    todasLasOrdenes = todasLasOrdenes.Where(o => o.TipoOrden == filtrarPorTipo.ToString());
                }

                var cola = todasLasOrdenes.Select(o => new OrdenCocinaResponse
                {
                    Id = o.Id,
                    NumeroOrden = o.NumeroOrden,
                    MesaId = o.MesaId,
                    NumeroMesa = o.Mesa?.Numero,
                    Tipo = Enum.Parse<TipoOrden>(o.TipoOrden),
                    Prioridad = DeterminarPrioridad(o),
                    FechaCreacion = o.FechaCreacion,
                    TiempoEsperaMinutos = (int)(DateTime.UtcNow - o.FechaCreacion).TotalMinutes,
                    TiempoEstimadoPreparacion = o.TiempoEstimadoPreparacion ?? 30,
                    Items = GenerarItemsCocina(o.Items),
                    NotasEspeciales = o.NotasEspeciales
                }).OrderBy(o => o.Prioridad).ThenBy(o => o.FechaCreacion);

                _logger.LogDebug("Cola de cocina: {Count} órdenes", cola.Count());

                return cola;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cola de órdenes de cocina");
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes listas para servir
        /// </summary>
        public async Task<IEnumerable<OrdenResponse>> GetOrdenesListasParaServirAsync(int? meseroId = null)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes listas para servir");

                var ordenesListas = await GetOrdenesPorEstadoAsync(EstadoOrden.Lista);

                // Filtrar por mesero si se especifica
                if (meseroId.HasValue)
                {
                    // TODO: Implementar filtro por mesero asignado
                }

                // Ordenar por tiempo en estado "Lista" (más antiguas primero)
                var ordenesOrdenadas = ordenesListas.OrderBy(o => o.FechaModificacion ?? o.FechaCreacion);

                _logger.LogDebug("Encontradas {Count} órdenes listas para servir", ordenesOrdenadas.Count());

                return ordenesOrdenadas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes listas para servir");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de tiempo de preparación en tiempo real
        /// </summary>
        public async Task<EstadisticasCocinaResult> GetEstadisticasCocinaAsync()
        {
            try
            {
                _logger.LogDebug("Calculando estadísticas de cocina");

                var hoy = DateTime.Today;
                var ordenesHoy = await _ordenRepository.GetOrdenesPorFechaAsync(hoy, DateTime.Now);

                var ordenesPendientes = ordenesHoy.Count(o => o.Estado == "Pendiente");
                var ordenesEnPreparacion = ordenesHoy.Count(o => o.Estado == "EnPreparacion");
                var ordenesListas = ordenesHoy.Count(o => o.Estado == "Lista");
                var ordenesCompletadas = ordenesHoy.Count(o => o.Estado == "Entregada" || o.Estado == "Facturada");

                var tiemposPreparacion = ordenesHoy
                    .Where(o => o.Estado == "Entregada" || o.Estado == "Facturada")
                    .Where(o => o.FechaModificacion.HasValue)
                    .Select(o => o.FechaModificacion!.Value - o.FechaCreacion)
                    .ToList();

                var tiempoPromedio = tiemposPreparacion.Any() 
                    ? TimeSpan.FromTicks((long)tiemposPreparacion.Average(t => t.Ticks))
                    : TimeSpan.Zero;

                var tiempoMaximo = tiemposPreparacion.Any() 
                    ? tiemposPreparacion.Max()
                    : TimeSpan.Zero;

                var totalOrdenes = ordenesHoy.Count();
                var eficiencia = totalOrdenes > 0 ? (decimal)ordenesCompletadas / totalOrdenes * 100 : 100;

                var productosPopulares = await GetProductosMasOrdenadosAsync(hoy, DateTime.Now, 5);

                var estadisticas = new EstadisticasCocinaResult
                {
                    OrdenesEnCola = ordenesPendientes,
                    OrdenesEnPreparacion = ordenesEnPreparacion,
                    OrdenesListasParaServir = ordenesListas,
                    TiempoPromedioPreparacion = tiempoPromedio,
                    TiempoEsperaMaximo = tiempoMaximo,
                    PorcentajeEficiencia = eficiencia,
                    ProductosMasSolicitados = productosPopulares.ToList()
                };

                _logger.LogDebug("Estadísticas de cocina calculadas: Eficiencia {Eficiencia}%, Tiempo promedio {TiempoPromedio}", 
                    eficiencia, tiempoPromedio);

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular estadísticas de cocina");
                throw;
            }
        }

        /// <summary>
        /// Notifica a cocina sobre una nueva orden urgente
        /// </summary>
        public async Task<bool> NotificarOrdenUrgenteAsync(int ordenId, PrioridadOrden prioridad, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Notificando orden urgente {OrdenId} con prioridad {Prioridad} por usuario {UsuarioId}", 
                    ordenId, prioridad, usuarioId);

                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                    return false;

                // Agregar nota de urgencia
                var notaUrgencia = $"🚨 ORDEN {prioridad.ToString().ToUpper()} - Notificada por usuario {usuarioId}";
                orden.NotasEspeciales = $"{orden.NotasEspeciales}\n{DateTime.UtcNow:HH:mm}: {notaUrgencia}";
                orden.FechaModificacion = DateTime.UtcNow;

                await _ordenRepository.UpdateAsync(orden);

                // TODO: Implementar notificación real a cocina (SignalR, etc.)
                _logger.LogInformation("Orden {OrdenId} marcada como {Prioridad} y notificada a cocina", ordenId, prioridad);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar orden urgente {OrdenId}", ordenId);
                return false;
            }
        }

        // ============================================================================
        // CÁLCULOS Y VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Calcula el total de una orden con descuentos aplicables
        /// </summary>
        public async Task<CalculoOrdenResult> CalcularTotalOrdenAsync(List<ItemOrdenRequest> items, bool aplicarDescuentos = true)
        {
            try
            {
                _logger.LogDebug("Calculando total de orden con {Count} items", items.Count);

                var resultado = new CalculoOrdenResult();

                // Calcular subtotal
                foreach (var item in items)
                {
                    var precio = item.PrecioUnitario ?? await ObtenerPrecioProductoAsync(item.ProductoId, item.ComboId);
                    resultado.Subtotal += precio * item.Cantidad;
                }

                // Aplicar descuentos si está habilitado
                if (aplicarDescuentos)
                {
                    // Descuento por volumen
                    if (resultado.Subtotal >= DESCUENTO_VOLUMEN_MINIMO)
                    {
                        var descuentoVolumen = resultado.Subtotal * DESCUENTO_VOLUMEN_PORCENTAJE;
                        resultado.Descuentos += descuentoVolumen;
                        resultado.DescuentosDetalle.Add(new DescuentoAplicado
                        {
                            Tipo = "Volumen",
                            Descripcion = $"Descuento por compra mayor a RD$ {DESCUENTO_VOLUMEN_MINIMO:N0}",
                            Monto = descuentoVolumen,
                            Porcentaje = DESCUENTO_VOLUMEN_PORCENTAJE * 100
                        });
                    }

                    // TODO: Agregar más tipos de descuentos (cliente frecuente, combos, etc.)
                }

                // Calcular ITBIS dominicano (18%)
                var baseImponible = resultado.Subtotal - resultado.Descuentos;
                resultado.ITBIS = baseImponible * ITBIS_DOMINICANO;

                // Calcular total final
                resultado.Total = baseImponible + resultado.ITBIS;

                resultado.Observaciones = resultado.Descuentos > 0 
                    ? $"Se aplicaron descuentos por RD$ {resultado.Descuentos:N2}"
                    : "Sin descuentos aplicados";

                _logger.LogDebug("Total calculado - Subtotal: RD$ {Subtotal}, Descuentos: RD$ {Descuentos}, ITBIS: RD$ {ITBIS}, Total: RD$ {Total}", 
                    resultado.Subtotal, resultado.Descuentos, resultado.ITBIS, resultado.Total);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular total de orden");
                throw;
            }
        }

        /// <summary>
        /// Valida una orden completa antes de crearla
        /// </summary>
        public async Task<ValidacionOrdenResult> ValidarOrdenAsync(CrearOrdenRequest crearOrdenRequest)
        {
            try
            {
                _logger.LogDebug("Validando orden completa");

                var resultado = new ValidacionOrdenResult();

                // Validaciones básicas
                if (!crearOrdenRequest.Items.Any())
                {
                    resultado.Errores.Add("La orden debe contener al menos un producto");
                }

                if (crearOrdenRequest.MesaId.HasValue)
                {
                    var mesa = await _mesaService.GetMesaDetalleAsync(crearOrdenRequest.MesaId.Value);
                    if (mesa == null)
                    {
                        resultado.Errores.Add("Mesa no encontrada");
                    }
                    else if (mesa.Estado != "Libre" && mesa.Estado != "Ocupada")
                    {
                        resultado.Advertencias.Add($"Mesa en estado {mesa.Estado} - Verificar disponibilidad");
                    }
                }

                // Validar productos
                foreach (var item in crearOrdenRequest.Items)
                {
                    if (item.Cantidad <= 0)
                    {
                        resultado.Errores.Add($"Cantidad inválida para producto ID {item.ProductoId}");
                    }

                    var disponibilidad = await _productoService.VerificarDisponibilidadAsync(item.ProductoId, item.Cantidad);
                    if (!disponibilidad.EstaDisponible)
                    {
                        resultado.Errores.Add($"Producto no disponible: {disponibilidad.Mensaje}");
                    }
                }

                // Calcular totales estimados
                if (!resultado.Errores.Any())
                {
                    var calculo = await CalcularTotalOrdenAsync(crearOrdenRequest.Items, true);
                    resultado.TotalEstimado = calculo.Total;
                    resultado.TiempoEstimadoMinutos = await EstimarTiempoPreparacionAsync(crearOrdenRequest.Items);

                    if (calculo.Descuentos > 0)
                    {
                        resultado.Sugerencias.Add($"Se aplicarán descuentos por RD$ {calculo.Descuentos:N2}");
                    }
                }

                resultado.EsValida = !resultado.Errores.Any();

                _logger.LogDebug("Validación completada - Válida: {EsValida}, Errores: {Errores}, Advertencias: {Advertencias}", 
                    resultado.EsValida, resultado.Errores.Count, resultado.Advertencias.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar orden");
                throw;
            }
        }

        /// <summary>
        /// Estima el tiempo total de preparación de una orden
        /// </summary>
        public async Task<int> EstimarTiempoPreparacionAsync(List<ItemOrdenRequest> items)
        {
            try
            {
                _logger.LogDebug("Estimando tiempo de preparación para {Count} items", items.Count);

                var tiempoTotal = 0;
                var categoriasProcesadas = new HashSet<string>();

                foreach (var item in items)
                {
                    // Obtener categoría del producto
                    var categoria = await ObtenerCategoriaProductoAsync(item.ProductoId);
                    
                    if (!string.IsNullOrEmpty(categoria) && !categoriasProcesadas.Contains(categoria))
                    {
                        // Solo agregar tiempo si no hemos procesado esta categoría
                        if (TIEMPOS_PREPARACION.TryGetValue(categoria, out var tiempoCategoria))
                        {
                            tiempoTotal += tiempoCategoria;
                            categoriasProcesadas.Add(categoria);
                        }
                        else
                        {
                            tiempoTotal += 15; // Tiempo por defecto
                        }
                    }
                }

                // Agregar tiempo base y factor de complejidad
                var tiempoBase = 5; // 5 minutos base
                var factorComplejidad = Math.Min(items.Count * 2, 20); // Máximo 20 minutos adicionales
                
                var tiempoEstimado = tiempoBase + tiempoTotal + factorComplejidad;

                _logger.LogDebug("Tiempo estimado de preparación: {TiempoEstimado} minutos", tiempoEstimado);

                return tiempoEstimado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al estimar tiempo de preparación");
                return 30; // Tiempo por defecto en caso de error
            }
        }

        /// <summary>
        /// Verifica disponibilidad de todos los productos de una orden
        /// </summary>
        public async Task<DisponibilidadOrdenResult> VerificarDisponibilidadOrdenAsync(List<ItemOrdenRequest> items)
        {
            try
            {
                _logger.LogDebug("Verificando disponibilidad de {Count} items", items.Count);

                var resultado = new DisponibilidadOrdenResult();

                foreach (var item in items)
                {
                    var disponibilidad = await _productoService.VerificarDisponibilidadAsync(item.ProductoId, item.Cantidad);
                    
                    if (!disponibilidad.EstaDisponible)
                    {
                        var nombreProducto = await ObtenerNombreProductoAsync(item.ProductoId);
                        resultado.ProductosNoDisponibles.Add(nombreProducto);
                        
                        if (disponibilidad.Alternativas?.Any() == true)
                        {
                            resultado.Alternativas.AddRange(disponibilidad.Alternativas);
                        }
                    }
                    else if (disponibilidad.StockActual <= 10) // Stock bajo
                    {
                        var nombreProducto = await ObtenerNombreProductoAsync(item.ProductoId);
                        resultado.ProductosStockBajo.Add(nombreProducto);
                    }
                }

                resultado.TodoDisponible = !resultado.ProductosNoDisponibles.Any();

                if (resultado.TodoDisponible)
                {
                    resultado.Mensaje = resultado.ProductosStockBajo.Any() 
                        ? $"Disponible, pero algunos productos tienen stock bajo: {string.Join(", ", resultado.ProductosStockBajo)}"
                        : "Todos los productos están disponibles";
                }
                else
                {
                    resultado.Mensaje = $"Productos no disponibles: {string.Join(", ", resultado.ProductosNoDisponibles)}";
                }

                _logger.LogDebug("Verificación de disponibilidad completada - Todo disponible: {TodoDisponible}", resultado.TodoDisponible);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad de orden");
                throw;
            }
        }

        // ============================================================================
        // HISTORIAL Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene el historial de órdenes de un cliente específico
        /// </summary>
        public async Task<IEnumerable<OrdenResponse>> GetHistorialClienteAsync(int clienteId, int limit = 10)
        {
            try
            {
                _logger.LogDebug("Obteniendo historial de cliente {ClienteId} (limit: {Limit})", clienteId, limit);

                var ordenes = await _ordenRepository.GetOrdenesPorClienteAsync(clienteId, limit);
                var ordenesResponse = _mapper.Map<List<OrdenResponse>>(ordenes);

                _logger.LogDebug("Encontradas {Count} órdenes en historial de cliente {ClienteId}", ordenesResponse.Count, clienteId);

                return ordenesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de cliente {ClienteId}", clienteId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes recientes del día actual
        /// </summary>
        public async Task<IEnumerable<OrdenResponse>> GetOrdenesDelDiaAsync(bool soloActivas = false)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes del día (solo activas: {SoloActivas})", soloActivas);

                var hoy = DateTime.Today;
                var ordenes = await _ordenRepository.GetOrdenesPorFechaAsync(hoy, DateTime.Now);

                if (soloActivas)
                {
                    ordenes = ordenes.Where(o => o.Estado != "Entregada" && o.Estado != "Facturada" && o.Estado != "Cancelada");
                }

                var ordenesResponse = _mapper.Map<List<OrdenResponse>>(ordenes.OrderByDescending(o => o.FechaCreacion));

                _logger.LogDebug("Encontradas {Count} órdenes del día", ordenesResponse.Count);

                return ordenesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos más ordenados en un período
        /// </summary>
        public async Task<IEnumerable<ProductoMasOrdenadoResult>> GetProductosMasOrdenadosAsync(DateTime fechaInicio, DateTime fechaFin, int limit = 10)
        {
            try
            {
                _logger.LogDebug("Obteniendo productos más ordenados desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);

                var productos = await _ordenRepository.GetProductosMasOrdenadosAsync(fechaInicio, fechaFin, limit);

                var resultado = productos.Select(p => new ProductoMasOrdenadoResult
                {
                    ProductoId = p.ProductoId,
                    NombreProducto = p.NombreProducto,
                    CantidadOrdenada = p.CantidadOrdenada,
                    MontoTotal = p.MontoTotal,
                    VecesOrdenado = p.VecesOrdenado,
                    Porcentaje = p.Porcentaje
                });

                _logger.LogDebug("Encontrados {Count} productos más ordenados", resultado.Count());

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más ordenados");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de órdenes por período
        /// </summary>
        public async Task<EstadisticasOrdenesResult> GetEstadisticasOrdenesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                _logger.LogDebug("Obteniendo estadísticas de órdenes desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);

                var ordenes = await _ordenRepository.GetOrdenesPorFechaAsync(fechaInicio, fechaFin);

                var totalOrdenes = ordenes.Count();
                var ordenesCompletadas = ordenes.Count(o => o.Estado == "Entregada" || o.Estado == "Facturada");
                var ordenesCanceladas = ordenes.Count(o => o.Estado == "Cancelada");
                var montoTotalVentas = ordenes.Where(o => o.Estado != "Cancelada").Sum(o => o.Total);
                var ticketPromedio = ordenesCompletadas > 0 ? montoTotalVentas / ordenesCompletadas : 0;

                var tiemposPreparacion = ordenes
                    .Where(o => o.Estado == "Entregada" || o.Estado == "Facturada")
                    .Where(o => o.FechaModificacion.HasValue)
                    .Select(o => o.FechaModificacion!.Value - o.FechaCreacion)
                    .ToList();

                var tiempoPromedio = tiemposPreparacion.Any() 
                    ? TimeSpan.FromTicks((long)tiemposPreparacion.Average(t => t.Ticks))
                    : TimeSpan.Zero;

                var topProductos = await GetProductosMasOrdenadosAsync(fechaInicio, fechaFin, 10);

                var ordenesPorHora = ordenes
                    .GroupBy(o => o.FechaCreacion.Hour)
                    .ToDictionary(g => $"{g.Key:00}:00", g => g.Count());

                var estadisticas = new EstadisticasOrdenesResult
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalOrdenes = totalOrdenes,
                    OrdenesCompletadas = ordenesCompletadas,
                    OrdenesCanceladas = ordenesCanceladas,
                    MontoTotalVentas = montoTotalVentas,
                    TicketPromedio = ticketPromedio,
                    TiempoPromedioPreparacion = tiempoPromedio,
                    TopProductos = topProductos.ToList(),
                    OrdenesPorHora = ordenesPorHora
                };

                _logger.LogDebug("Estadísticas calculadas: {TotalOrdenes} órdenes, RD$ {MontoTotal} en ventas", 
                    totalOrdenes, montoTotalVentas);

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de órdenes");
                throw;
            }
        }

        // ============================================================================
        // NOTIFICACIONES Y COMUNICACIÓN
        // ============================================================================

        /// <summary>
        /// Envía notificación de cambio de estado a personal relevante
        /// </summary>
        public async Task<bool> NotificarCambioEstadoAsync(int ordenId, EstadoOrden nuevoEstado, int usuarioId)
        {
            try
            {
                _logger.LogDebug("Enviando notificación de cambio de estado para orden {OrdenId}: {Estado}", ordenId, nuevoEstado);

                // TODO: Implementar sistema de notificaciones real (SignalR, push notifications, etc.)
                // Por ahora solo logeamos la notificación

                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                var mensaje = GenerarMensajeNotificacion(orden, nuevoEstado);

                _logger.LogInformation("Notificación enviada - Orden: {NumeroOrden}, Estado: {Estado}, Mensaje: {Mensaje}", 
                    orden?.NumeroOrden, nuevoEstado, mensaje);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación para orden {OrdenId}", ordenId);
                return false;
            }
        }

        /// <summary>
        /// Obtiene notificaciones activas para un usuario específico
        /// </summary>
        public async Task<IEnumerable<NotificacionOrden>> GetNotificacionesUsuarioAsync(int usuarioId, bool soloNoLeidas = true)
        {
            try
            {
                _logger.LogDebug("Obteniendo notificaciones para usuario {UsuarioId} (solo no leídas: {SoloNoLeidas})", 
                    usuarioId, soloNoLeidas);

                // TODO: Implementar sistema de notificaciones persistente
                // Por ahora retornamos lista vacía
                return new List<NotificacionOrden>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones para usuario {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Marca notificaciones como leídas
        /// </summary>
        public async Task<bool> MarcarNotificacionesLeidasAsync(List<int> notificacionesIds, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Marcando {Count} notificaciones como leídas para usuario {UsuarioId}", 
                    notificacionesIds.Count, usuarioId);

                // TODO: Implementar marcado de notificaciones como leídas
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificaciones como leídas para usuario {UsuarioId}", usuarioId);
                return false;
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        /// <summary>
        /// Genera número único de orden
        /// </summary>
        private async Task<string> GenerarNumeroOrdenAsync()
        {
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var ultimo = await _ordenRepository.GetUltimoNumeroOrdenAsync(fecha);
            var numero = ultimo + 1;
            return $"ORD-{fecha}-{numero:D4}";
        }

        /// <summary>
        /// Obtiene el precio de un producto o combo
        /// </summary>
        private async Task<decimal> ObtenerPrecioProductoAsync(int productoId, int? comboId = null)
        {
            if (comboId.HasValue)
            {
                var combo = await _productoService.GetComboByIdAsync(comboId.Value);
                return combo?.Precio ?? 0;
            }
            else
            {
                // TODO: Obtener precio del producto
                return 100; // Precio por defecto temporal
            }
        }

        /// <summary>
        /// Obtiene la categoría de un producto
        /// </summary>
        private async Task<string> ObtenerCategoriaProductoAsync(int productoId)
        {
            // TODO: Implementar obtención de categoría del producto
            return "Platos Principales"; // Categoría por defecto temporal
        }

        /// <summary>
        /// Obtiene el nombre de un producto
        /// </summary>
        private async Task<string> ObtenerNombreProductoAsync(int productoId)
        {
            // TODO: Implementar obtención del nombre del producto
            return $"Producto {productoId}"; // Nombre por defecto temporal
        }

        /// <summary>
        /// Verifica si una orden puede ser modificada
        /// </summary>
        private bool PuedeModificarOrden(Orden orden)
        {
            return orden.Estado == "Pendiente";
        }

        /// <summary>
        /// Verifica si una orden puede ser cancelada
        /// </summary>
        private bool PuedeCancelarOrden(Orden orden)
        {
            return orden.Estado != "Entregada" && orden.Estado != "Facturada" && orden.Estado != "Cancelada";
        }

        /// <summary>
        /// Calcula el progreso de preparación de una orden
        /// </summary>
        private decimal CalcularProgresoPreparacion(Orden orden)
        {
            return orden.Estado switch
            {
                "Pendiente" => 0,
                "EnPreparacion" => 50,
                "Lista" => 80,
                "Entregada" => 100,
                "Facturada" => 100,
                "Cancelada" => 0,
                _ => 0
            };
        }

        /// <summary>
        /// Valida transición de estado de orden
        /// </summary>
        private (bool esValido, string mensaje) ValidarCambioEstado(EstadoOrden estadoActual, EstadoOrden nuevoEstado)
        {
            // Definir transiciones válidas
            var transicionesValidas = new Dictionary<EstadoOrden, List<EstadoOrden>>
            {
                [EstadoOrden.Pendiente] = new() { EstadoOrden.EnPreparacion, EstadoOrden.Cancelada },
                [EstadoOrden.EnPreparacion] = new() { EstadoOrden.Lista, EstadoOrden.Cancelada },
                [EstadoOrden.Lista] = new() { EstadoOrden.Entregada },
                [EstadoOrden.Entregada] = new() { EstadoOrden.Facturada },
                [EstadoOrden.Facturada] = new() { }, // Estado final
                [EstadoOrden.Cancelada] = new() { } // Estado final
            };

            if (transicionesValidas.TryGetValue(estadoActual, out var estadosPermitidos))
            {
                if (estadosPermitidos.Contains(nuevoEstado))
                {
                    return (true, "Transición válida");
                }
            }

            return (false, $"No se puede cambiar de {estadoActual} a {nuevoEstado}");
        }

        /// <summary>
        /// Ejecuta acciones específicas al cambiar estado
        /// </summary>
        private async Task EjecutarAccionesEstadoAsync(Orden orden, EstadoOrden nuevoEstado, int usuarioId)
        {
            switch (nuevoEstado)
            {
                case EstadoOrden.EnPreparacion:
                    // Confirmar stock
                    await ConfirmarStockOrdenAsync(orden);
                    break;
                
                case EstadoOrden.Entregada:
                    // Verificar si se puede preparar facturación
                    if (orden.MesaId.HasValue)
                    {
                        await VerificarEstadoMesaParaFacturacionAsync(orden.MesaId.Value);
                    }
                    break;
                
                case EstadoOrden.Facturada:
                    // Liberar mesa si es la última orden
                    if (orden.MesaId.HasValue)
                    {
                        var otrasOrdenes = await GetOrdenesPorMesaAsync(orden.MesaId.Value);
                        if (!otrasOrdenes.Any(o => o.Estado != "Facturada" && o.Estado != "Cancelada"))
                        {
                            await _mesaService.LiberarMesaAsync(orden.MesaId.Value, usuarioId);
                        }
                    }
                    break;
                
                case EstadoOrden.Cancelada:
                    // Restaurar stock
                    await RestaurarStockOrdenAsync(orden);
                    break;
            }
        }

        /// <summary>
        /// Confirma el stock de una orden (descuenta del inventario)
        /// </summary>
        private async Task<bool> ConfirmarStockOrdenAsync(Orden orden)
        {
            try
            {
                // TODO: Implementar confirmación de stock real
                _logger.LogDebug("Confirmando stock para orden {OrdenId}", orden.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar stock de orden {OrdenId}", orden.Id);
                return false;
            }
        }

        /// <summary>
        /// Restaura el stock de una orden cancelada
        /// </summary>
        private async Task<bool> RestaurarStockOrdenAsync(Orden orden)
        {
            try
            {
                // TODO: Implementar restauración de stock real
                _logger.LogDebug("Restaurando stock para orden cancelada {OrdenId}", orden.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar stock de orden {OrdenId}", orden.Id);
                return false;
            }
        }

        /// <summary>
        /// Verifica si una mesa está lista para facturación
        /// </summary>
        private async Task VerificarEstadoMesaParaFacturacionAsync(int mesaId)
        {
            try
            {
                var ordenes = await GetOrdenesPorMesaAsync(mesaId);
                var todasEntregadas = ordenes.All(o => o.Estado == "Entregada" || o.Estado == "Facturada");
                
                if (todasEntregadas)
                {
                    _logger.LogInformation("Mesa {MesaId} lista para facturación - Todas las órdenes entregadas", mesaId);
                    // TODO: Notificar al cajero que la mesa está lista para facturar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar estado de mesa para facturación {MesaId}", mesaId);
            }
        }

        /// <summary>
        /// Consolida items duplicados
        /// </summary>
        private List<ItemOrdenRequest> ConsolidarItems(List<ItemOrdenRequest> items)
        {
            return items
                .GroupBy(i => new { i.ProductoId, i.ComboId, i.NotasEspeciales })
                .Select(g => new ItemOrdenRequest
                {
                    ProductoId = g.Key.ProductoId,
                    ComboId = g.Key.ComboId,
                    Cantidad = g.Sum(i => i.Cantidad),
                    NotasEspeciales = g.Key.NotasEspeciales,
                    PrecioUnitario = g.First().PrecioUnitario
                })
                .ToList();
        }

        /// <summary>
        /// Consolida detalles de orden
        /// </summary>
        private List<DetalleOrden> ConsolidarItemsDetalles(List<DetalleOrden> detalles)
        {
            return detalles
                .GroupBy(d => new { d.ProductoId, d.ComboId, d.NotasEspeciales })
                .Select(g => new DetalleOrden
                {
                    ProductoId = g.Key.ProductoId,
                    ComboId = g.Key.ComboId,
                    Cantidad = g.Sum(d => d.Cantidad),
                    PrecioUnitario = g.First().PrecioUnitario,
                    PrecioTotal = g.Sum(d => d.PrecioTotal),
                    NotasEspeciales = g.Key.NotasEspeciales,
                    FechaCreacion = DateTime.UtcNow
                })
                .ToList();
        }

        /// <summary>
        /// Crea orden individual para división
        /// </summary>
        private async Task<OrdenResponse> CrearOrdenIndividualDividida(Orden ordenOriginal, string? nombrePersona, decimal monto, int usuarioId)
        {
            var ordenIndividual = new Orden
            {
                NumeroOrden = await GenerarNumeroOrdenAsync(),
                MesaId = ordenOriginal.MesaId,
                TipoOrden = ordenOriginal.TipoOrden,
                Estado = EstadoOrden.Entregada.ToString(),
                Subtotal = monto / (1 + ITBIS_DOMINICANO),
                ITBIS = monto * ITBIS_DOMINICANO / (1 + ITBIS_DOMINICANO),
                Total = monto,
                NotasEspeciales = $"División individual - {nombrePersona ?? "Cliente"}",
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioId
            };

            var ordenCreada = await _ordenRepository.CreateAsync(ordenIndividual);
            return _mapper.Map<OrdenResponse>(ordenCreada);
        }

        /// <summary>
        /// Determina prioridad de orden para cocina
        /// </summary>
        private PrioridadOrden DeterminarPrioridad(OrdenResponse orden)
        {
            var tiempoEspera = DateTime.UtcNow - orden.FechaCreacion;
            
            if (tiempoEspera.TotalMinutes > 45)
                return PrioridadOrden.Urgente;
            
            if (tiempoEspera.TotalMinutes > 30)
                return PrioridadOrden.Alta;
            
            return PrioridadOrden.Normal;
        }

        /// <summary>
        /// Genera items para cocina
        /// </summary>
        private List<ItemCocinaResponse> GenerarItemsCocina(List<ItemOrdenResponse> items)
        {
            return items.Select(i => new ItemCocinaResponse
            {
                NombreProducto = i.NombreProducto,
                Cantidad = i.Cantidad,
                NotasEspeciales = i.NotasEspeciales,
                RequierePrecaucion = EsProductoConPrecaucion(i.NombreProducto),
                TiempoPreparacionMinutos = ObtenerTiempoPreparacionProducto(i.NombreProducto)
            }).ToList();
        }

        /// <summary>
        /// Verifica si un producto requiere precaución especial
        /// </summary>
        private bool EsProductoConPrecaucion(string nombreProducto)
        {
            var productosConPrecaucion = new[] { "Mariscos", "Pollo", "Cerdo" };
            return productosConPrecaucion.Any(p => nombreProducto.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtiene tiempo de preparación específico del producto
        /// </summary>
        private int ObtenerTiempoPreparacionProducto(string nombreProducto)
        {
            // Lógica específica para productos dominicanos
            if (nombreProducto.Contains("Sancocho", StringComparison.OrdinalIgnoreCase))
                return 45;
            
            if (nombreProducto.Contains("Mangú", StringComparison.OrdinalIgnoreCase))
                return 15;
            
            if (nombreProducto.Contains("Tostones", StringComparison.OrdinalIgnoreCase))
                return 10;
            
            return 20; // Tiempo por defecto
        }

        /// <summary>
        /// Genera mensaje de notificación según estado
        /// </summary>
        private string GenerarMensajeNotificacion(Orden? orden, EstadoOrden estado)
        {
            if (orden == null) return "Notificación de orden";

            return estado switch
            {
                EstadoOrden.Pendiente => $"Nueva orden {orden.NumeroOrden} pendiente de preparación",
                EstadoOrden.EnPreparacion => $"Orden {orden.NumeroOrden} en preparación",
                EstadoOrden.Lista => $"Orden {orden.NumeroOrden} lista para servir",
                EstadoOrden.Entregada => $"Orden {orden.NumeroOrden} entregada al cliente",
                EstadoOrden.Facturada => $"Orden {orden.NumeroOrden} facturada",
                EstadoOrden.Cancelada => $"Orden {orden.NumeroOrden} cancelada",
                _ => $"Orden {orden.NumeroOrden} - Estado actualizado"
            };
        }
    }
}