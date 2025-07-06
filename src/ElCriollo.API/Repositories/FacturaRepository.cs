using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación específica para operaciones con facturas
    /// Maneja facturación, pagos y reportes financieros del restaurante
    /// </summary>
    public class FacturaRepository : BaseRepository<Factura>, IFacturaRepository
    {
        public FacturaRepository(ElCriolloDbContext context, ILogger<FacturaRepository> logger)
            : base(context, logger)
        {
        }

        // ============================================================================
        // GESTIÓN DE FACTURAS
        // ============================================================================

        /// <summary>
        /// Genera el próximo número de factura automáticamente
        /// </summary>
        public async Task<string> GenerarNumeroFacturaAsync()
        {
            try
            {
                var fechaHoy = DateTime.Now.ToString("yyyyMMdd");
                var prefijo = $"FAC-{fechaHoy}-";

                // Obtener el último número del día
                var ultimaFactura = await _dbSet
                    .Where(f => f.NumeroFactura.StartsWith(prefijo))
                    .OrderByDescending(f => f.NumeroFactura)
                    .FirstOrDefaultAsync();

                int siguienteNumero = 1;
                if (ultimaFactura != null)
                {
                    var ultimoNumeroStr = ultimaFactura.NumeroFactura.Substring(prefijo.Length);
                    if (int.TryParse(ultimoNumeroStr, out int ultimoNumero))
                    {
                        siguienteNumero = ultimoNumero + 1;
                    }
                }

                var numeroFactura = $"{prefijo}{siguienteNumero:D6}";
                
                _logger.LogDebug("Número de factura generado: {NumeroFactura}", numeroFactura);
                return numeroFactura;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de factura");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una factura por su número de factura
        /// </summary>
        public async Task<Factura?> GetByNumeroFacturaAsync(string numeroFactura)
        {
            try
            {
                var factura = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Empleado)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.DetalleOrdenes)
                            .ThenInclude(d => d.Producto)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.DetalleOrdenes)
                            .ThenInclude(d => d.Combo)
                    .FirstOrDefaultAsync(f => f.NumeroFactura == numeroFactura);

                return factura;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener factura por número: {NumeroFactura}", numeroFactura);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un número de factura ya existe
        /// </summary>
        public async Task<bool> NumeroFacturaExisteAsync(string numeroFactura)
        {
            try
            {
                return await _dbSet.AnyAsync(f => f.NumeroFactura == numeroFactura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de número de factura: {NumeroFactura}", numeroFactura);
                throw;
            }
        }

        /// <summary>
        /// Crea una factura para una orden específica
        /// </summary>
        public async Task<Factura> CrearFacturaParaOrdenAsync(int ordenId, string metodoPago = "Efectivo", decimal impuesto = 0.18m, decimal descuento = 0, decimal propina = 0)
        {
            try
            {
                _logger.LogDebug("Creando factura para orden ID: {OrdenId}", ordenId);

                return await ExecuteInTransactionAsync(async () =>
                {
                    // Obtener orden con detalles
                    var orden = await _context.Ordenes
                        .Include(o => o.DetalleOrdenes)
                            .ThenInclude(d => d.Producto)
                        .Include(o => o.DetalleOrdenes)
                            .ThenInclude(d => d.Combo)
                        .Include(o => o.Mesa)
                        .FirstOrDefaultAsync(o => o.OrdenID == ordenId);

                    if (orden == null)
                    {
                        throw new ArgumentException($"Orden con ID {ordenId} no encontrada");
                    }

                    if (orden.Estado != "Lista" && orden.Estado != "Entregada")
                    {
                        throw new InvalidOperationException($"No se puede facturar una orden en estado {orden.Estado}");
                    }

                    // Calcular subtotal de la orden
                    var subtotal = orden.DetalleOrdenes.Sum(d => d.Subtotal);

                    // Calcular impuesto (ITBIS 18% en República Dominicana)
                    var montoImpuesto = subtotal * impuesto;

                    // Calcular total
                    var total = subtotal + montoImpuesto - descuento + propina;

                    // Crear factura
                    var factura = new Factura
                    {
                        NumeroFactura = await GenerarNumeroFacturaAsync(),
                        OrdenID = ordenId,
                        FechaFactura = DateTime.UtcNow,
                        Subtotal = subtotal,
                        Impuesto = montoImpuesto,
                        Descuento = descuento,
                        Propina = propina,
                        Total = total,
                        MetodoPago = metodoPago,
                        Estado = "Pagada"
                    };

                    await _dbSet.AddAsync(factura);

                    // Actualizar estado de orden
                    orden.Estado = "Entregada";

                    // Liberar mesa si estaba ocupada
                    if (orden.Mesa != null && orden.Mesa.Estado == "Ocupada")
                    {
                        orden.Mesa.Estado = "Libre";
                        await _context.Mesas
                            .Where(m => m.MesaID == orden.Mesa.MesaID)
                            .ExecuteUpdateAsync(m => m.SetProperty(mesa => mesa.Estado, "Libre"));
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Factura creada exitosamente. Número: {NumeroFactura} para orden ID: {OrdenId}", 
                        factura.NumeroFactura, ordenId);

                    return factura;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear factura para orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR ESTADO Y MÉTODO DE PAGO
        // ============================================================================

        /// <summary>
        /// Obtiene facturas por estado específico
        /// </summary>
        public async Task<IEnumerable<Factura>> GetByEstadoAsync(string estado)
        {
            try
            {
                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.Estado == estado)
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas por estado: {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas pagadas
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasPagadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.Estado == "Pagada");

                if (fechaInicio.HasValue)
                    query = query.Where(f => f.FechaFactura >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(f => f.FechaFactura <= fechaFin.Value);

                var facturas = await query
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas pagadas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas pendientes de pago
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasPendientesAsync()
        {
            return await GetByEstadoAsync("Pendiente");
        }

        /// <summary>
        /// Obtiene facturas anuladas
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasAnuladasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.Estado == "Anulada");

                if (fechaInicio.HasValue)
                    query = query.Where(f => f.FechaFactura >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(f => f.FechaFactura <= fechaFin.Value);

                var facturas = await query
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas anuladas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas por método de pago
        /// </summary>
        public async Task<IEnumerable<Factura>> GetByMetodoPagoAsync(string metodoPago, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.MetodoPago == metodoPago && f.Estado != "Anulada");

                if (fechaInicio.HasValue)
                    query = query.Where(f => f.FechaFactura >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(f => f.FechaFactura <= fechaFin.Value);

                var facturas = await query
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas por método de pago: {MetodoPago}", metodoPago);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR FECHA Y TIEMPO
        // ============================================================================

        /// <summary>
        /// Obtiene facturas del día actual
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasHoyAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.FechaFactura >= hoy && f.FechaFactura < mañana)
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas de una fecha específica
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasPorFechaAsync(DateTime fecha)
        {
            try
            {
                var inicioFecha = fecha.Date;
                var finFecha = inicioFecha.AddDays(1);

                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.FechaFactura >= inicioFecha && f.FechaFactura < finFecha)
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas de fecha: {Fecha}", fecha);
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas en un rango de fechas
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin)
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas por rango de fechas: {Inicio} - {Fin}", fechaInicio, fechaFin);
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas del mes actual
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasMesActualAsync()
        {
            try
            {
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                return await GetFacturasPorRangoFechasAsync(inicioMes, finMes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas del mes actual");
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas de un mes específico
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasPorMesAsync(int año, int mes)
        {
            try
            {
                var inicioMes = new DateTime(año, mes, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                return await GetFacturasPorRangoFechasAsync(inicioMes, finMes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas del mes: {Año}-{Mes}", año, mes);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR ORDEN Y CLIENTE
        // ============================================================================

        /// <summary>
        /// Obtiene facturas de una orden específica
        /// </summary>
        public async Task<IEnumerable<Factura>> GetByOrdenAsync(int ordenId)
        {
            try
            {
                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.OrdenID == ordenId)
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas de orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene la factura principal de una orden
        /// </summary>
        public async Task<Factura?> GetFacturaPrincipalOrdenAsync(int ordenId)
        {
            try
            {
                var factura = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.DetalleOrdenes)
                            .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(f => f.OrdenID == ordenId && f.Estado == "Pagada");

                return factura;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener factura principal de orden ID: {OrdenId}", ordenId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene facturas de un cliente específico
        /// </summary>
        public async Task<IEnumerable<Factura>> GetByClienteAsync(int clienteId)
        {
            try
            {
                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Where(f => f.Orden!.ClienteID == clienteId)
                    .OrderByDescending(f => f.FechaFactura)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas de cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de facturas de un cliente
        /// </summary>
        public async Task<IEnumerable<Factura>> GetHistorialClienteAsync(int clienteId, int limite = 20)
        {
            try
            {
                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Where(f => f.Orden!.ClienteID == clienteId)
                    .OrderByDescending(f => f.FechaFactura)
                    .Take(limite)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES DE FACTURACIÓN
        // ============================================================================

        /// <summary>
        /// Procesa el pago de una factura
        /// </summary>
        public async Task<bool> ProcesarPagoAsync(int facturaId, string metodoPago, decimal montoPagado)
        {
            try
            {
                _logger.LogDebug("Procesando pago de factura ID: {FacturaId}, Método: {MetodoPago}, Monto: {Monto}", 
                    facturaId, metodoPago, montoPagado);

                var factura = await _dbSet.FindAsync(facturaId);
                if (factura == null)
                {
                    _logger.LogWarning("Factura no encontrada para procesar pago: {FacturaId}", facturaId);
                    return false;
                }

                if (factura.Estado == "Pagada")
                {
                    _logger.LogWarning("Factura ID: {FacturaId} ya está pagada", facturaId);
                    return false;
                }

                if (montoPagado < factura.Total)
                {
                    _logger.LogWarning("Monto pagado {Monto} es insuficiente para factura {FacturaId} con total {Total}", 
                        montoPagado, facturaId, factura.Total);
                    return false;
                }

                factura.Estado = "Pagada";
                factura.MetodoPago = metodoPago;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pago procesado exitosamente para factura ID: {FacturaId}", facturaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago de factura ID: {FacturaId}", facturaId);
                throw;
            }
        }

        /// <summary>
        /// Anula una factura
        /// </summary>
        public async Task<bool> AnularFacturaAsync(int facturaId, string razon)
        {
            try
            {
                _logger.LogDebug("Anulando factura ID: {FacturaId}, Razón: {Razon}", facturaId, razon);

                return await ExecuteInTransactionAsync(async () =>
                {
                    var factura = await _dbSet
                        .Include(f => f.Orden)
                        .FirstOrDefaultAsync(f => f.FacturaID == facturaId);

                    if (factura == null)
                    {
                        _logger.LogWarning("Factura no encontrada para anular: {FacturaId}", facturaId);
                        return false;
                    }

                    if (factura.Estado == "Anulada")
                    {
                        _logger.LogWarning("Factura ID: {FacturaId} ya está anulada", facturaId);
                        return false;
                    }

                    factura.Estado = "Anulada";

                    // Revertir estado de orden si es necesario
                    if (factura.Orden != null)
                    {
                        factura.Orden.Estado = "Lista"; // Volver a estado anterior
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Factura ID: {FacturaId} anulada exitosamente. Razón: {Razon}", facturaId, razon);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular factura ID: {FacturaId}", facturaId);
                throw;
            }
        }

        /// <summary>
        /// Divide una factura en varias (para grupos)
        /// </summary>
        public async Task<IEnumerable<Factura>> DividirFacturaAsync(int facturaOriginalId, int divisiones)
        {
            try
            {
                _logger.LogDebug("Dividiendo factura ID: {FacturaId} en {Divisiones} partes", facturaOriginalId, divisiones);

                return await ExecuteInTransactionAsync(async () =>
                {
                    var facturaOriginal = await _dbSet
                        .Include(f => f.Orden)
                            .ThenInclude(o => o!.DetalleOrdenes)
                        .FirstOrDefaultAsync(f => f.FacturaID == facturaOriginalId);

                    if (facturaOriginal == null)
                    {
                        throw new ArgumentException($"Factura con ID {facturaOriginalId} no encontrada");
                    }

                    if (divisiones < 2)
                    {
                        throw new ArgumentException("El número de divisiones debe ser mayor a 1");
                    }

                    // Anular factura original
                    facturaOriginal.Estado = "Anulada";

                    var nuevasFacturas = new List<Factura>();

                    // Calcular montos divididos
                    var subtotalPorDivision = Math.Round(facturaOriginal.Subtotal / divisiones, 2);
                    var impuestoPorDivision = Math.Round(facturaOriginal.Impuesto / divisiones, 2);
                    var descuentoPorDivision = Math.Round(facturaOriginal.Descuento / divisiones, 2);
                    var propinaPorDivision = Math.Round(facturaOriginal.Propina / divisiones, 2);
                    var totalPorDivision = Math.Round(facturaOriginal.Total / divisiones, 2);

                    // Crear facturas divididas
                    for (int i = 0; i < divisiones; i++)
                    {
                        var nuevaFactura = new Factura
                        {
                            NumeroFactura = await GenerarNumeroFacturaAsync(),
                            OrdenID = facturaOriginal.OrdenID,
                            FechaFactura = DateTime.UtcNow,
                            Subtotal = subtotalPorDivision,
                            Impuesto = impuestoPorDivision,
                            Descuento = descuentoPorDivision,
                            Propina = propinaPorDivision,
                            Total = totalPorDivision,
                            MetodoPago = facturaOriginal.MetodoPago,
                            Estado = "Pagada"
                        };

                        await _dbSet.AddAsync(nuevaFactura);
                        nuevasFacturas.Add(nuevaFactura);
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Factura ID: {FacturaId} dividida exitosamente en {Divisiones} facturas", 
                        facturaOriginalId, divisiones);

                    return nuevasFacturas;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al dividir factura ID: {FacturaId}", facturaOriginalId);
                throw;
            }
        }

        /// <summary>
        /// Aplica descuento a una factura
        /// </summary>
        public async Task<bool> AplicarDescuentoAsync(int facturaId, decimal descuento, bool esPorcentaje = false)
        {
            try
            {
                _logger.LogDebug("Aplicando descuento a factura ID: {FacturaId}, Descuento: {Descuento}, Es porcentaje: {EsPorcentaje}", 
                    facturaId, descuento, esPorcentaje);

                var factura = await _dbSet.FindAsync(facturaId);
                if (factura == null)
                {
                    _logger.LogWarning("Factura no encontrada para aplicar descuento: {FacturaId}", facturaId);
                    return false;
                }

                if (factura.Estado == "Anulada")
                {
                    throw new InvalidOperationException("No se puede aplicar descuento a una factura anulada");
                }

                decimal montoDescuento;
                if (esPorcentaje)
                {
                    montoDescuento = factura.Subtotal * (descuento / 100);
                }
                else
                {
                    montoDescuento = descuento;
                }

                factura.Descuento += montoDescuento;
                factura.Total = factura.Subtotal + factura.Impuesto - factura.Descuento + factura.Propina;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Descuento de {Descuento} aplicado a factura ID: {FacturaId}", montoDescuento, facturaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar descuento a factura ID: {FacturaId}", facturaId);
                throw;
            }
        }

        // ============================================================================
        // REPORTES FINANCIEROS
        // ============================================================================

        /// <summary>
        /// Calcula el total de ventas del día
        /// </summary>
        public async Task<decimal> GetVentasDelDiaAsync(DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                var inicioFecha = fechaConsulta.Date;
                var finFecha = inicioFecha.AddDays(1);

                var totalVentas = await _dbSet
                    .Where(f => f.FechaFactura >= inicioFecha && 
                               f.FechaFactura < finFecha && 
                               f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                return totalVentas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular ventas del día");
                throw;
            }
        }

        /// <summary>
        /// Calcula el total de ventas del mes
        /// </summary>
        public async Task<decimal> GetVentasDelMesAsync(int año, int mes)
        {
            try
            {
                var inicioMes = new DateTime(año, mes, 1);
                var finMes = inicioMes.AddMonths(1);

                var totalVentas = await _dbSet
                    .Where(f => f.FechaFactura >= inicioMes && 
                               f.FechaFactura < finMes && 
                               f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                return totalVentas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular ventas del mes: {Año}-{Mes}", año, mes);
                throw;
            }
        }

        /// <summary>
        /// Calcula ventas por período
        /// </summary>
        public async Task<decimal> GetVentasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var totalVentas = await _dbSet
                    .Where(f => f.FechaFactura >= fechaInicio && 
                               f.FechaFactura <= fechaFin && 
                               f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                return totalVentas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular ventas por período: {Inicio} - {Fin}", fechaInicio, fechaFin);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reporte de ventas diarias para un período
        /// </summary>
        public async Task<IEnumerable<object>> GetReporteVentasDiariasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasDiarias = await _dbSet
                    .Where(f => f.FechaFactura >= fechaInicio && 
                               f.FechaFactura <= fechaFin && 
                               f.Estado == "Pagada")
                    .GroupBy(f => f.FechaFactura.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key.ToString("yyyy-MM-dd"),
                        CantidadFacturas = g.Count(),
                        TotalVentas = g.Sum(f => f.Total),
                        TotalImpuestos = g.Sum(f => f.Impuesto),
                        TotalDescuentos = g.Sum(f => f.Descuento),
                        TotalPropinas = g.Sum(f => f.Propina),
                        TicketPromedio = g.Average(f => f.Total)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToListAsync();

                return ventasDiarias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de ventas diarias");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reporte de ventas por método de pago
        /// </summary>
        public async Task<IEnumerable<object>> GetReporteVentasPorMetodoPagoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasPorMetodo = await _dbSet
                    .Where(f => f.FechaFactura >= fechaInicio && 
                               f.FechaFactura <= fechaFin && 
                               f.Estado == "Pagada")
                    .GroupBy(f => f.MetodoPago)
                    .Select(g => new
                    {
                        MetodoPago = g.Key,
                        CantidadFacturas = g.Count(),
                        TotalVentas = g.Sum(f => f.Total),
                        PorcentajeVentas = 0.0 // Se calculará después
                    })
                    .OrderByDescending(v => v.TotalVentas)
                    .ToListAsync();

                // Calcular porcentajes
                var totalGeneral = ventasPorMetodo.Sum(v => v.TotalVentas);
                if (totalGeneral > 0)
                {
                    ventasPorMetodo = ventasPorMetodo.Select(v => new
                    {
                        v.MetodoPago,
                        v.CantidadFacturas,
                        v.TotalVentas,
                        PorcentajeVentas = Math.Round((double)(v.TotalVentas / totalGeneral) * 100, 2)
                    }).ToList();
                }

                return ventasPorMetodo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de ventas por método de pago");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las facturas más altas del período
        /// </summary>
        public async Task<IEnumerable<Factura>> GetFacturasMasAltasAsync(int limite = 10, int dias = 30)
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                var facturas = await _dbSet
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Mesa)
                    .Include(f => f.Orden)
                        .ThenInclude(o => o!.Cliente)
                    .Where(f => f.FechaFactura >= fechaLimite && f.Estado == "Pagada")
                    .OrderByDescending(f => f.Total)
                    .Take(limite)
                    .ToListAsync();

                return facturas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturas más altas");
                throw;
            }
        }

        // ============================================================================
        // ESTADÍSTICAS Y ANÁLISIS
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas financieras del día
        /// </summary>
        public async Task<object> GetEstadisticasFinancierasDelDiaAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var facturasHoy = await _dbSet
                    .Where(f => f.FechaFactura >= hoy && f.FechaFactura < mañana && f.Estado == "Pagada")
                    .ToListAsync();

                var totalFacturas = facturasHoy.Count;
                var totalVentas = facturasHoy.Sum(f => f.Total);
                var totalImpuestos = facturasHoy.Sum(f => f.Impuesto);
                var totalDescuentos = facturasHoy.Sum(f => f.Descuento);
                var totalPropinas = facturasHoy.Sum(f => f.Propina);
                var ticketPromedio = totalFacturas > 0 ? totalVentas / totalFacturas : 0;

                var ventasPorMetodo = facturasHoy
                    .GroupBy(f => f.MetodoPago)
                    .Select(g => new { MetodoPago = g.Key, Total = g.Sum(f => f.Total) })
                    .ToList();

                return new
                {
                    Fecha = hoy.ToString("yyyy-MM-dd"),
                    TotalFacturas = totalFacturas,
                    TotalVentas = Math.Round(totalVentas, 2),
                    TotalImpuestos = Math.Round(totalImpuestos, 2),
                    TotalDescuentos = Math.Round(totalDescuentos, 2),
                    TotalPropinas = Math.Round(totalPropinas, 2),
                    TicketPromedio = Math.Round(ticketPromedio, 2),
                    VentasPorMetodoPago = ventasPorMetodo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas financieras del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas financieras del mes
        /// </summary>
        public async Task<object> GetEstadisticasFinancierasDelMesAsync()
        {
            try
            {
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1);

                var facturasMes = await _dbSet
                    .Where(f => f.FechaFactura >= inicioMes && f.FechaFactura < finMes && f.Estado == "Pagada")
                    .ToListAsync();

                var totalFacturas = facturasMes.Count;
                var totalVentas = facturasMes.Sum(f => f.Total);
                var totalImpuestos = facturasMes.Sum(f => f.Impuesto);
                var totalDescuentos = facturasMes.Sum(f => f.Descuento);
                var totalPropinas = facturasMes.Sum(f => f.Propina);

                var ventasPorDia = facturasMes
                    .GroupBy(f => f.FechaFactura.Day)
                    .Select(g => new { Dia = g.Key, Total = g.Sum(f => f.Total) })
                    .OrderBy(v => v.Dia)
                    .ToList();

                return new
                {
                    Mes = inicioMes.ToString("yyyy-MM"),
                    TotalFacturas = totalFacturas,
                    TotalVentas = Math.Round(totalVentas, 2),
                    TotalImpuestos = Math.Round(totalImpuestos, 2),
                    TotalDescuentos = Math.Round(totalDescuentos, 2),
                    TotalPropinas = Math.Round(totalPropinas, 2),
                    VentasPorDia = ventasPorDia
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas financieras del mes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el ticket promedio
        /// </summary>
        public async Task<decimal> GetTicketPromedioAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet.Where(f => f.Estado == "Pagada");

                if (fechaInicio.HasValue)
                    query = query.Where(f => f.FechaFactura >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(f => f.FechaFactura <= fechaFin.Value);

                var facturas = await query.ToListAsync();

                return facturas.Any() ? facturas.Average(f => f.Total) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ticket promedio");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el total de impuestos recaudados
        /// </summary>
        public async Task<decimal> GetTotalImpuestosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var totalImpuestos = await _dbSet
                    .Where(f => f.FechaFactura >= fechaInicio && 
                               f.FechaFactura <= fechaFin && 
                               f.Estado == "Pagada")
                    .SumAsync(f => f.Impuesto);

                return totalImpuestos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de impuestos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el total de propinas recibidas
        /// </summary>
        public async Task<decimal> GetTotalPropinasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var totalPropinas = await _dbSet
                    .Where(f => f.FechaFactura >= fechaInicio && 
                               f.FechaFactura <= fechaFin && 
                               f.Estado == "Pagada")
                    .SumAsync(f => f.Propina);

                return totalPropinas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de propinas");
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES ESPECIALES DOMINICANAS
        // ============================================================================

        /// <summary>
        /// Calcula el ITBIS (Impuesto dominicano 18%) para un período
        /// </summary>
        public async Task<decimal> GetTotalITBISAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // En República Dominicana, el ITBIS es del 18%
                return await GetTotalImpuestosAsync(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de ITBIS");
                throw;
            }
        }

        /// <summary>
        /// Genera reporte fiscal para declaraciones en República Dominicana
        /// </summary>
        public async Task<object> GenerarReporteFiscalAsync(int mes, int año)
        {
            try
            {
                var inicioMes = new DateTime(año, mes, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                var facturas = await _dbSet
                    .Where(f => f.FechaFactura >= inicioMes && 
                               f.FechaFactura <= finMes && 
                               f.Estado == "Pagada")
                    .ToListAsync();

                var totalVentasGravadas = facturas.Sum(f => f.Subtotal);
                var totalITBIS = facturas.Sum(f => f.Impuesto);
                var totalVentasConImpuesto = facturas.Sum(f => f.Total - f.Descuento - f.Propina);
                var totalFacturas = facturas.Count;

                var ventasPorDia = facturas
                    .GroupBy(f => f.FechaFactura.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key.ToString("yyyy-MM-dd"),
                        CantidadFacturas = g.Count(),
                        VentasGravadas = g.Sum(f => f.Subtotal),
                        ITBIS = g.Sum(f => f.Impuesto),
                        TotalVentas = g.Sum(f => f.Total - f.Descuento - f.Propina)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToList();

                return new
                {
                    Periodo = $"{año}-{mes:D2}",
                    TotalFacturas = totalFacturas,
                    TotalVentasGravadas = Math.Round(totalVentasGravadas, 2),
                    TotalITBIS = Math.Round(totalITBIS, 2),
                    TotalVentasConImpuesto = Math.Round(totalVentasConImpuesto, 2),
                    TasaImpuesto = "18%",
                    MonedaLocal = "RD$",
                    DetallesPorDia = ventasPorDia,
                    FechaGeneracion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Restaurante = "El Criollo - Comida Dominicana"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte fiscal para {Mes}/{Año}", mes, año);
                throw;
            }
        }
    }
}