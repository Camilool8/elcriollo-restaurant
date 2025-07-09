using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación específica para operaciones con órdenes del restaurante
    /// Maneja comandas, estados de preparación y flujo de cocina
    /// </summary>
    public class OrdenRepository : BaseRepository<Orden>, IOrdenRepository
    {
        public OrdenRepository(ElCriolloDbContext context, ILogger<OrdenRepository> logger)
            : base(context, logger)
        {
        }

        // ============================================================================
        // GESTIÓN DE ESTADOS DE ORDEN
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes por estado específico
        /// </summary>
        public async Task<IEnumerable<Orden>> GetByEstadoAsync(string estado)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes por estado: {Estado}", estado);

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Combo)
                    .Where(o => o.Estado == estado)
                    .OrderBy(o => o.FechaCreacion)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} órdenes en estado: {Estado}", ordenes.Count, estado);
                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes por estado: {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes pendientes de preparación
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesPendientesAsync()
        {
            return await GetByEstadoAsync("Pendiente");
        }

        /// <summary>
        /// Obtiene órdenes en preparación
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesEnPreparacionAsync()
        {
            return await GetByEstadoAsync("EnPreparacion");
        }

        /// <summary>
        /// Obtiene órdenes listas para entregar
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesListasAsync()
        {
            return await GetByEstadoAsync("Lista");
        }

        /// <summary>
        /// Obtiene órdenes entregadas
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesEntregadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.Estado == "Entregada");

                if (fechaInicio.HasValue)
                    query = query.Where(o => o.FechaCreacion >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(o => o.FechaCreacion <= fechaFin.Value);

                var ordenes = await query
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes entregadas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes canceladas
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesCanceladasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.Estado == "Cancelada");

                if (fechaInicio.HasValue)
                    query = query.Where(o => o.FechaCreacion >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(o => o.FechaCreacion <= fechaFin.Value);

                var ordenes = await query
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes canceladas");
                throw;
            }
        }

        /// <summary>
        /// Cambia el estado de una orden
        /// </summary>
        public async Task<bool> CambiarEstadoOrdenAsync(int ordenId, string nuevoEstado, string? observaciones = null)
        {
            try
            {
                _logger.LogDebug("Cambiando estado de orden ID: {OrdenId} a {Estado}", ordenId, nuevoEstado);

                var orden = await _dbSet.FindAsync(ordenId);
                if (orden == null)
                {
                    _logger.LogWarning("Orden no encontrada para cambio de estado: {OrdenId}", ordenId);
                    return false;
                }

                var estadosValidos = new[] { "Pendiente", "EnPreparacion", "Lista", "Entregada", "Cancelada" };
                if (!estadosValidos.Contains(nuevoEstado))
                {
                    _logger.LogWarning("Estado inválido para orden: {Estado}", nuevoEstado);
                    return false;
                }

                orden.Estado = nuevoEstado;
                if (!string.IsNullOrEmpty(observaciones))
                {
                    orden.Observaciones = $"{orden.Observaciones}\n{DateTime.Now:yyyy-MM-dd HH:mm}: {observaciones}".Trim();
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de orden ID: {OrdenId} cambiado a {Estado}", ordenId, nuevoEstado);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR TIPO DE ORDEN
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes por tipo específico
        /// </summary>
        public async Task<IEnumerable<Orden>> GetByTipoOrdenAsync(string tipoOrden)
        {
            try
            {
                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.TipoOrden == tipoOrden)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes por tipo: {Tipo}", tipoOrden);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes para mesa
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesMesaAsync()
        {
            return await GetByTipoOrdenAsync("Mesa");
        }

        /// <summary>
        /// Obtiene órdenes para llevar
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesLlevarAsync()
        {
            return await GetByTipoOrdenAsync("Llevar");
        }

        /// <summary>
        /// Obtiene órdenes para delivery
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesDeliveryAsync()
        {
            return await GetByTipoOrdenAsync("Delivery");
        }

        // ============================================================================
        // CONSULTAS POR MESA Y EMPLEADO
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes de una mesa específica
        /// </summary>
        public async Task<IEnumerable<Orden>> GetByMesaAsync(int mesaId)
        {
            try
            {
                var ordenes = await _dbSet
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Where(o => o.MesaID == mesaId)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de mesa ID: {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene la orden activa de una mesa
        /// </summary>
        public async Task<Orden?> GetOrdenActivaMesaAsync(int mesaId)
        {
            try
            {
                var orden = await _dbSet
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Combo)
                    .FirstOrDefaultAsync(o => o.MesaID == mesaId && 
                                            o.Estado != "Entregada" && 
                                            o.Estado != "Cancelada");

                return orden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden activa de mesa ID: {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes atendidas por un empleado
        /// </summary>
        public async Task<IEnumerable<Orden>> GetByEmpleadoAsync(int empleadoId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Where(o => o.EmpleadoID == empleadoId);

                if (fechaInicio.HasValue)
                    query = query.Where(o => o.FechaCreacion >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(o => o.FechaCreacion <= fechaFin.Value);

                var ordenes = await query
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de empleado ID: {EmpleadoId}", empleadoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes de un cliente específico
        /// </summary>
        public async Task<IEnumerable<Orden>> GetByClienteAsync(int clienteId)
        {
            try
            {
                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Empleado)
                    .Where(o => o.ClienteID == clienteId)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE NÚMEROS DE ORDEN
        // ============================================================================

        /// <summary>
        /// Genera el próximo número de orden automáticamente
        /// </summary>
        public async Task<string> GenerarNumeroOrdenAsync()
        {
            try
            {
                var fechaHoy = DateTime.Now.ToString("yyyyMMdd");
                var prefijo = $"ORD-{fechaHoy}-";

                // Obtener el último número del día
                var ultimaOrden = await _dbSet
                    .Where(o => o.NumeroOrden.StartsWith(prefijo))
                    .OrderByDescending(o => o.NumeroOrden)
                    .FirstOrDefaultAsync();

                int siguienteNumero = 1;
                if (ultimaOrden != null)
                {
                    var ultimoNumeroStr = ultimaOrden.NumeroOrden.Substring(prefijo.Length);
                    if (int.TryParse(ultimoNumeroStr, out int ultimoNumero))
                    {
                        siguienteNumero = ultimoNumero + 1;
                    }
                }

                var numeroOrden = $"{prefijo}{siguienteNumero:D4}";
                
                _logger.LogDebug("Número de orden generado: {NumeroOrden}", numeroOrden);
                return numeroOrden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de orden");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una orden por su número de orden
        /// </summary>
        public async Task<Orden?> GetByNumeroOrdenAsync(string numeroOrden)
        {
            try
            {
                var orden = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Combo)
                    .FirstOrDefaultAsync(o => o.NumeroOrden == numeroOrden);

                return orden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden por número: {NumeroOrden}", numeroOrden);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un número de orden ya existe
        /// </summary>
        public async Task<bool> NumeroOrdenExisteAsync(string numeroOrden)
        {
            try
            {
                return await _dbSet.AnyAsync(o => o.NumeroOrden == numeroOrden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de número de orden: {NumeroOrden}", numeroOrden);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR FECHA Y TIEMPO
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes del día actual
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesHoyAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.FechaCreacion >= hoy && o.FechaCreacion < mañana)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes de una fecha específica
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesPorFechaAsync(DateTime fecha)
        {
            try
            {
                var inicioFecha = fecha.Date;
                var finFecha = inicioFecha.AddDays(1);

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.FechaCreacion >= inicioFecha && o.FechaCreacion < finFecha)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes de fecha: {Fecha}", fecha);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes en un rango de fechas
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes por rango de fechas: {Inicio} - {Fin}", fechaInicio, fechaFin);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes con tiempo de preparación excedido
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesConTiempoExcedidoAsync(int tiempoLimite = 30)
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddMinutes(-tiempoLimite);

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Where(o => (o.Estado == "Pendiente" || o.Estado == "EnPreparacion") && 
                               o.FechaCreacion <= fechaLimite)
                    .OrderBy(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes con tiempo excedido");
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES CON DETALLES DE ORDEN
        // ============================================================================

        /// <summary>
        /// Obtiene una orden con todos sus detalles incluidos
        /// </summary>
        public async Task<Orden?> GetConDetallesAsync(int ordenId)
        {
            try
            {
                var orden = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p!.Categoria)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Combo)
                    .Include(o => o.Facturas)
                    .FirstOrDefaultAsync(o => o.OrdenID == ordenId);

                return orden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden con detalles ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Agrega un producto a una orden existente
        /// </summary>
        public async Task<DetalleOrden> AgregarProductoAOrdenAsync(int ordenId, int? productoId, int? comboId, int cantidad, string? observaciones = null)
        {
            try
            {
                _logger.LogDebug("Agregando producto a orden ID: {OrdenId}, Producto: {ProductoId}, Combo: {ComboId}, Cantidad: {Cantidad}", 
                    ordenId, productoId, comboId, cantidad);

                var orden = await _dbSet.FindAsync(ordenId);
                if (orden == null)
                {
                    throw new ArgumentException($"Orden con ID {ordenId} no encontrada");
                }

                if (orden.Estado == "Entregada" || orden.Estado == "Cancelada")
                {
                    throw new InvalidOperationException($"No se puede agregar productos a una orden en estado {orden.Estado}");
                }

                decimal precio = 0;
                if (productoId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(productoId.Value);
                    precio = producto?.Precio ?? 0;
                }
                else if (comboId.HasValue)
                {
                    var combo = await _context.Combos.FindAsync(comboId.Value);
                    precio = combo?.Precio ?? 0;
                }

                var detalleOrden = new DetalleOrden
                {
                    OrdenID = ordenId,
                    ProductoID = productoId,
                    ComboID = comboId,
                    Cantidad = cantidad,
                    PrecioUnitario = precio,
                    Observaciones = observaciones
                };

                _context.DetalleOrdenes.Add(detalleOrden);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Producto agregado exitosamente a orden ID: {OrdenId}", ordenId);
                return detalleOrden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto a orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Remueve un producto de una orden
        /// </summary>
        public async Task<bool> RemoverProductoDeOrdenAsync(int ordenId, int detalleOrdenId)
        {
            try
            {
                _logger.LogDebug("Removiendo producto de orden ID: {OrdenId}, Detalle: {DetalleId}", ordenId, detalleOrdenId);

                var detalleOrden = await _context.DetalleOrdenes
                    .FirstOrDefaultAsync(d => d.DetalleOrdenID == detalleOrdenId && d.OrdenID == ordenId);

                if (detalleOrden == null)
                {
                    _logger.LogWarning("Detalle de orden no encontrado: {DetalleId} en orden: {OrdenId}", detalleOrdenId, ordenId);
                    return false;
                }

                var orden = await _dbSet.FindAsync(ordenId);
                if (orden?.Estado == "Entregada" || orden?.Estado == "Cancelada")
                {
                    throw new InvalidOperationException($"No se puede remover productos de una orden en estado {orden.Estado}");
                }

                _context.DetalleOrdenes.Remove(detalleOrden);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Producto removido exitosamente de orden ID: {OrdenId}", ordenId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al remover producto de orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Calcula el total de una orden
        /// </summary>
        public async Task<decimal> CalcularTotalOrdenAsync(int ordenId)
        {
            try
            {
                var total = await _context.DetalleOrdenes
                    .Where(d => d.OrdenID == ordenId)
                    .SumAsync(d => d.Subtotal);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular total de orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas de órdenes del día
        /// </summary>
        public async Task<object> GetEstadisticasDelDiaAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var ordenesHoy = await _dbSet
                    .Where(o => o.FechaCreacion >= hoy && o.FechaCreacion < mañana)
                    .ToListAsync();

                var totalOrdenes = ordenesHoy.Count;
                var ordenesPendientes = ordenesHoy.Count(o => o.Estado == "Pendiente");
                var ordenesEnPreparacion = ordenesHoy.Count(o => o.Estado == "EnPreparacion");
                var ordenesListas = ordenesHoy.Count(o => o.Estado == "Lista");
                var ordenesEntregadas = ordenesHoy.Count(o => o.Estado == "Entregada");
                var ordenesCanceladas = ordenesHoy.Count(o => o.Estado == "Cancelada");

                var ordenesPorTipo = ordenesHoy
                    .GroupBy(o => o.TipoOrden)
                    .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                    .ToList();

                return new
                {
                    Fecha = hoy.ToString("yyyy-MM-dd"),
                    TotalOrdenes = totalOrdenes,
                    OrdenesPendientes = ordenesPendientes,
                    OrdenesEnPreparacion = ordenesEnPreparacion,
                    OrdenesListas = ordenesListas,
                    OrdenesEntregadas = ordenesEntregadas,
                    OrdenesCanceladas = ordenesCanceladas,
                    OrdenesPorTipo = ordenesPorTipo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de órdenes del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de órdenes por período
        /// </summary>
        public async Task<object> GetEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ordenes = await _dbSet
                    .Where(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin)
                    .ToListAsync();

                var totalOrdenes = ordenes.Count;
                var ordenesPorEstado = ordenes
                    .GroupBy(o => o.Estado)
                    .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                    .ToList();

                var ordenesPorTipo = ordenes
                    .GroupBy(o => o.TipoOrden)
                    .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                    .ToList();

                var ordenesPorDia = ordenes
                    .GroupBy(o => o.FechaCreacion.Date)
                    .Select(g => new { Fecha = g.Key.ToString("yyyy-MM-dd"), Cantidad = g.Count() })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                return new
                {
                    FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                    FechaFin = fechaFin.ToString("yyyy-MM-dd"),
                    TotalOrdenes = totalOrdenes,
                    OrdenesPorEstado = ordenesPorEstado,
                    OrdenesPorTipo = ordenesPorTipo,
                    OrdenesPorDia = ordenesPorDia
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de órdenes por período");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el tiempo promedio de preparación
        /// </summary>
        public async Task<double> GetTiempoPromedioPreparacionAsync(int dias = 7)
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                var ordenesEntregadas = await _dbSet
                    .Include(o => o.Facturas)
                    .Where(o => o.Estado == "Entregada" && 
                           o.FechaCreacion >= fechaLimite &&
                           o.Facturas.Any())
                    .ToListAsync();

                if (!ordenesEntregadas.Any())
                    return 0;

                var tiempos = ordenesEntregadas
                    .Where(o => o.Facturas.Any())
                    .Select(o => 
                    {
                        var factura = o.Facturas.OrderBy(f => f.FechaFactura).First();
                        return (factura.FechaFactura - o.FechaCreacion).TotalMinutes;
                    })
                    .Where(t => t > 0 && t < 300); // Filtrar tiempos razonables (menos de 5 horas)

                return tiempos.Any() ? tiempos.Average() : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tiempo promedio de preparación");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las órdenes más grandes del día
        /// </summary>
        public async Task<IEnumerable<object>> GetOrdenesMasGrandesAsync(int limite = 10)
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var ordenesGrandes = await _context.DetalleOrdenes
                    .Include(d => d.Orden)
                    .Where(d => d.Orden.FechaCreacion >= hoy && d.Orden.FechaCreacion < mañana)
                    .GroupBy(d => new { d.OrdenID, d.Orden.NumeroOrden })
                    .Select(g => new
                    {
                        OrdenId = g.Key.OrdenID,
                        NumeroOrden = g.Key.NumeroOrden,
                        TotalItems = g.Sum(d => d.Cantidad),
                        TotalValor = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(o => o.TotalValor)
                    .Take(limite)
                    .ToListAsync();

                return ordenesGrandes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes más grandes");
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES ESPECIALES
        // ============================================================================

        /// <summary>
        /// Cancela una orden y libera recursos asociados
        /// </summary>
        public async Task<bool> CancelarOrdenAsync(int ordenId, string razon)
        {
            try
            {
                _logger.LogDebug("Cancelando orden ID: {OrdenId}, Razón: {Razon}", ordenId, razon);

                return await ExecuteInTransactionAsync(async () =>
                {
                    var orden = await _dbSet
                        .Include(o => o.Mesa)
                        .FirstOrDefaultAsync(o => o.OrdenID == ordenId);

                    if (orden == null)
                    {
                        _logger.LogWarning("Orden no encontrada para cancelar: {OrdenId}", ordenId);
                        return false;
                    }

                    if (orden.Estado == "Entregada")
                    {
                        throw new InvalidOperationException("No se puede cancelar una orden ya entregada");
                    }

                    // Cambiar estado de la orden
                    orden.Estado = "Cancelada";
                    orden.Observaciones = $"{orden.Observaciones}\n{DateTime.Now:yyyy-MM-dd HH:mm}: CANCELADA - {razon}".Trim();

                    // Liberar mesa si estaba ocupada
                    if (orden.Mesa != null && orden.Mesa.Estado == "Ocupada")
                    {
                        orden.Mesa.Estado = "Libre";
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Orden ID: {OrdenId} cancelada exitosamente", ordenId);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Duplica una orden existente (para repetir pedido)
        /// </summary>
        public async Task<Orden> DuplicarOrdenAsync(int ordenId, int? nuevaMesaId = null)
        {
            try
            {
                _logger.LogDebug("Duplicando orden ID: {OrdenId}", ordenId);

                return await ExecuteInTransactionAsync(async () =>
                {
                    var ordenOriginal = await _dbSet
                        .Include(o => o.DetalleOrdenes)
                            .ThenInclude(d => d.Producto)
                        .Include(o => o.DetalleOrdenes)
                            .ThenInclude(d => d.Combo)
                        .FirstOrDefaultAsync(o => o.OrdenID == ordenId);

                    if (ordenOriginal == null)
                    {
                        throw new ArgumentException($"Orden con ID {ordenId} no encontrada");
                    }

                    // Crear nueva orden
                    var nuevaOrden = new Orden
                    {
                        NumeroOrden = await GenerarNumeroOrdenAsync(),
                        MesaID = nuevaMesaId ?? ordenOriginal.MesaID,
                        ClienteID = ordenOriginal.ClienteID,
                        EmpleadoID = ordenOriginal.EmpleadoID,
                        FechaCreacion = DateTime.UtcNow,
                        Estado = "Pendiente",
                        TipoOrden = ordenOriginal.TipoOrden,
                        Observaciones = $"Duplicada de orden {ordenOriginal.NumeroOrden}"
                    };

                    _dbSet.Add(nuevaOrden);
                    await _context.SaveChangesAsync();

                    // Duplicar detalles de orden
                    foreach (var detalle in ordenOriginal.DetalleOrdenes)
                    {
                        var nuevoDetalle = new DetalleOrden
                        {
                            OrdenID = nuevaOrden.OrdenID,
                            ProductoID = detalle.ProductoID,
                            ComboID = detalle.ComboID,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = detalle.PrecioUnitario,
                            Observaciones = detalle.Observaciones
                        };

                        _context.DetalleOrdenes.Add(nuevoDetalle);
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Orden ID: {OrdenId} duplicada exitosamente como orden ID: {NuevaOrdenId}", 
                        ordenId, nuevaOrden.OrdenID);

                    return nuevaOrden;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al duplicar orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes que necesitan seguimiento
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesQueNecesitanSeguimientoAsync()
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddMinutes(-20); // Órdenes de más de 20 minutos

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Where(o => (o.Estado == "Pendiente" || o.Estado == "EnPreparacion") && 
                               o.FechaCreacion <= fechaLimite)
                    .OrderBy(o => o.FechaCreacion)
                    .ToListAsync();

                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes que necesitan seguimiento");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una orden con todos sus includes
        /// </summary>
        public async Task<Orden?> GetByIdWithIncludesAsync(int ordenId)
        {
            try
            {
                _logger.LogDebug("Obteniendo orden con includes ID: {OrdenId}", ordenId);

                var orden = await _context.Ordenes
                    .AsNoTracking()
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)!
                        .ThenInclude(d => d.Producto)!
                            .ThenInclude(p => p!.Categoria)
                    .Include(o => o.DetalleOrdenes)!
                        .ThenInclude(d => d.Combo)
                    .FirstOrDefaultAsync(o => o.OrdenID == ordenId);

                if (orden == null)
                {
                    _logger.LogWarning($"No se encontró la orden con ID: {ordenId}");
                }
                
                return orden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener orden con includes ID: {ordenId}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes activas (no terminadas)
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesActivasAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes activas");

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Where(o => o.Estado != "Entregada" && 
                               o.Estado != "Cancelada" && 
                               o.Estado != "Facturada" && 
                               o.Estado != "Completada")
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} órdenes activas", ordenes.Count);
                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes activas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene órdenes por mesa actualmente activas
        /// </summary>
        public async Task<IEnumerable<Orden>> GetOrdenesPorMesaAsync(int mesaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes activas de mesa ID: {MesaId}", mesaId);

                var ordenes = await _dbSet
                    .Include(o => o.Mesa)
                    .Include(o => o.Cliente)
                    .Include(o => o.Empleado)
                    .Include(o => o.DetalleOrdenes)
                        .ThenInclude(d => d.Producto)
                    .Where(o => o.MesaID == mesaId && 
                               o.Estado != "Entregada" && 
                               o.Estado != "Cancelada" && 
                               o.Estado != "Facturada" && 
                               o.Estado != "Completada")
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} órdenes activas para mesa {MesaId}", ordenes.Count, mesaId);
                return ordenes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes por mesa ID: {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Agrega un detalle de orden
        /// </summary>
        public async Task<DetalleOrden> AddDetalleOrdenAsync(DetalleOrden detalle)
        {
            try
            {
                _logger.LogDebug("Agregando detalle de orden para orden ID: {OrdenId}", detalle.OrdenID);

                if (detalle == null)
                    throw new ArgumentNullException(nameof(detalle));

                // Validar que la orden existe
                var orden = await _dbSet.FindAsync(detalle.OrdenID);
                if (orden == null)
                {
                    throw new ArgumentException($"Orden con ID {detalle.OrdenID} no encontrada");
                }

                if (orden.Estado == "Entregada" || orden.Estado == "Cancelada")
                {
                    throw new InvalidOperationException($"No se puede agregar detalles a una orden en estado {orden.Estado}");
                }

                _context.DetalleOrdenes.Add(detalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Detalle de orden agregado exitosamente a orden ID: {OrdenId}", detalle.OrdenID);
                return detalle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar detalle de orden");
                throw;
            }
        }

        public async Task RemoveDetalleOrdenAsync(DetalleOrden detalle)
        {
            _context.DetalleOrdenes.Remove(detalle);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Agrega una nueva orden (alias de CreateAsync)
        /// </summary>
        public new async Task<Orden> AddAsync(Orden orden)
        {
            return await base.AddAsync(orden);
        }
    }
}