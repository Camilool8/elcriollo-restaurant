using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Servicio de facturación para el restaurante El Criollo
    /// Maneja facturación con ITBIS dominicano (18%) y liberación automática de mesas
    /// </summary>
    public class FacturaService : IFacturaService
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IMesaService _mesaService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<FacturaService> _logger;

        // Constantes dominicanas
        private const decimal ITBIS_DOMINICANO = 0.18m; // 18% ITBIS República Dominicana
        private const string PREFIJO_FACTURA = "FACT";

        public FacturaService(
            IFacturaRepository facturaRepository,
            IOrdenRepository ordenRepository,
            IMesaRepository mesaRepository,
            IMesaService mesaService,
            IEmailService emailService,
            IMapper mapper,
            ILogger<FacturaService> logger)
        {
            _facturaRepository = facturaRepository;
            _ordenRepository = ordenRepository;
            _mesaRepository = mesaRepository;
            _mesaService = mesaService;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // CREACIÓN Y GESTIÓN BÁSICA DE FACTURAS
        // ============================================================================

        public async Task<FacturaDto> CrearFacturaAsync(CrearFacturaRequest crearFacturaRequest)
        {
            try
            {
                _logger.LogInformation("🧾 Iniciando creación de factura para orden {OrdenId}", crearFacturaRequest.OrdenId);

                // Validar que la orden puede ser facturada
                var validacion = await ValidarOrdenParaFacturacionAsync(crearFacturaRequest.OrdenId);
                if (!validacion.EsValida)
                {
                    var errores = string.Join(", ", validacion.Errores);
                    _logger.LogWarning("❌ Validación fallida para orden {OrdenId}: {Errores}", crearFacturaRequest.OrdenId, errores);
                    throw new InvalidOperationException($"No se puede facturar la orden: {errores}");
                }

                // Obtener la orden completa
                var orden = await _ordenRepository.GetByIdWithIncludesAsync(crearFacturaRequest.OrdenId);
                if (orden == null)
                {
                    _logger.LogError("❌ Orden {OrdenId} no encontrada", crearFacturaRequest.OrdenId);
                    throw new ArgumentException($"Orden con ID {crearFacturaRequest.OrdenId} no encontrada");
                }

                // Calcular totales con ITBIS dominicano
                var totales = await CalcularTotalesOrdenAsync(
                    crearFacturaRequest.OrdenId, 
                    crearFacturaRequest.Descuento, 
                    crearFacturaRequest.Propina);

                // Generar número de factura único
                var numeroFactura = await GenerarNumeroFacturaUnicoAsync();

                // Crear la factura
                var nuevaFactura = new Factura
                {
                    NumeroFactura = numeroFactura,
                    OrdenID = crearFacturaRequest.OrdenId,
                    ClienteID = orden.ClienteID ?? 1,
                    EmpleadoID = orden.EmpleadoID,
                    FechaFactura = DateTime.Now,
                    Subtotal = totales.Subtotal,
                    Descuento = totales.Descuento,
                    Impuesto = totales.ITBIS,
                    Propina = totales.Propina,
                    Total = totales.TotalFinal,
                    MetodoPago = crearFacturaRequest.MetodoPago ?? "Efectivo",
                    Estado = "Pagada", // Marcar automáticamente como pagada
                    ObservacionesPago = crearFacturaRequest.Observaciones,
                    FechaPago = DateTime.Now // Establecer fecha de pago
                };

                // Guardar en base de datos
                var facturaCreada = await _facturaRepository.AddAsync(nuevaFactura);

                // Actualizar estado de la orden
                await ActualizarEstadoOrdenPostFacturacionAsync(crearFacturaRequest.OrdenId);

                // NO liberar mesa automáticamente - solo se liberará cuando todas las órdenes estén pagadas
                if (orden.MesaID.HasValue)
                {
                    _logger.LogInformation("🪑 Mesa {MesaId} - factura creada, mesa permanece ocupada hasta que todas las órdenes estén pagadas", orden.MesaID.Value);
                }

                // Enviar factura por email automáticamente
                try
                {
                    string emailDestino;
                    if (!string.IsNullOrEmpty(orden.Cliente?.Email))
                    {
                        emailDestino = orden.Cliente.Email;
                        _logger.LogInformation("📧 Enviando factura a cliente registrado: {Email}", emailDestino);
                    }
                    else
                    {
                        // Usar email por defecto para clientes anónimos
                        emailDestino = _emailService.GetDefaultEmailForAnonymousClients();
                        _logger.LogInformation("📧 Enviando factura a cliente anónimo usando email por defecto: {Email}", emailDestino);
                    }

                    await EnviarFacturaPorEmailAsync(facturaCreada.FacturaID, emailDestino);
                    _logger.LogInformation("📧 Factura enviada automáticamente a {Email}", emailDestino);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "⚠️ Error al enviar factura automática para factura {FacturaId}", facturaCreada.FacturaID);
                    // No falla la operación principal si hay error en el email
                }

                _logger.LogInformation("✅ Factura {NumeroFactura} creada exitosamente para orden {OrdenId}", 
                    numeroFactura, crearFacturaRequest.OrdenId);

                return _mapper.Map<FacturaDto>(facturaCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear factura para orden {OrdenId}", crearFacturaRequest.OrdenId);
                throw;
            }
        }

        public async Task<FacturaDto> CrearFacturaGrupalAsync(int mesaId, string metodoPago = "Efectivo", decimal descuento = 0, decimal propina = 0)
        {
            try
            {
                _logger.LogInformation("🧾👥 Iniciando facturación grupal para mesa {MesaId}", mesaId);

                // Validar que la mesa puede ser facturada grupalmente
                var validacion = await ValidarMesaParaFacturacionGrupalAsync(mesaId);
                if (!validacion.EsValida)
                {
                    var errores = string.Join(", ", validacion.Errores);
                    _logger.LogWarning("❌ Validación grupal fallida para mesa {MesaId}: {Errores}", mesaId, errores);
                    throw new InvalidOperationException($"No se puede facturar la mesa grupalmente: {errores}");
                }

                // Obtener todas las órdenes activas de la mesa
                var ordenes = await _ordenRepository.GetOrdenesPorMesaAsync(mesaId);
                var ordenesActivas = ordenes.Where(o => o.Estado == "Entregada" || o.Estado == "Pendiente").ToList();

                if (!ordenesActivas.Any())
                {
                    throw new InvalidOperationException("No hay órdenes entregadas o pendientes para facturar en esta mesa");
                }

                decimal subtotalTotal = 0;
                decimal totalFinalAcumulado = 0;

                // Calcular totales acumulados de todas las órdenes
                foreach (var orden in ordenesActivas)
                {
                    var totalesOrden = await CalcularTotalesOrdenAsync(orden.OrdenID, 0, 0);
                    subtotalTotal += totalesOrden.Subtotal;
                }

                // Aplicar descuento y propina al total grupal
                var subtotalConDescuento = subtotalTotal - descuento;
                var itbisTotal = CalcularITBIS(subtotalConDescuento);
                totalFinalAcumulado = subtotalConDescuento + itbisTotal + propina;

                // Generar número de factura único
                var numeroFactura = await GenerarNumeroFacturaUnicoAsync();

                // Crear factura grupal (usando la primera orden como referencia)
                var primeraOrden = ordenesActivas.First();
                var facturaGrupal = new Factura
                {
                    NumeroFactura = numeroFactura,
                    OrdenID = primeraOrden.OrdenID, // Orden principal
                    ClienteID = primeraOrden.ClienteID ?? 1,
                    EmpleadoID = primeraOrden.EmpleadoID, // CORREGIDO: Asignar EmpleadoID desde la orden
                    FechaFactura = DateTime.Now,
                    Subtotal = subtotalTotal,
                    Descuento = descuento,
                    Impuesto = itbisTotal,
                    Propina = propina,
                    Total = totalFinalAcumulado,
                    MetodoPago = metodoPago,
                    Estado = "Pendiente",
                    ObservacionesPago = $"Factura grupal mesa {mesaId} - {ordenesActivas.Count} órdenes"
                };

                // Guardar factura grupal
                var facturaCreada = await _facturaRepository.AddAsync(facturaGrupal);

                // Actualizar estado de todas las órdenes
                foreach (var orden in ordenesActivas)
                {
                    await ActualizarEstadoOrdenPostFacturacionAsync(orden.OrdenID);
                }

                // NO liberar mesa automáticamente - solo se liberará cuando todas las órdenes estén pagadas
                _logger.LogInformation("🪑 Mesa {MesaId} - facturación grupal completada, mesa permanece ocupada hasta que todas las órdenes estén pagadas", mesaId);

                _logger.LogInformation("✅ Factura grupal {NumeroFactura} creada para mesa {MesaId} con {CantidadOrdenes} órdenes", 
                    numeroFactura, mesaId, ordenesActivas.Count);

                return _mapper.Map<FacturaDto>(facturaCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear factura grupal para mesa {MesaId}", mesaId);
                throw;
            }
        }

        public async Task<FacturaDto?> GetFacturaByIdAsync(int facturaId)
        {
            try
            {
                var factura = await _facturaRepository.GetByIdWithIncludesAsync(facturaId);
                return factura != null ? _mapper.Map<FacturaDto>(factura) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener factura {FacturaId}", facturaId);
                throw;
            }
        }

        public async Task<FacturaDto?> GetFacturaByNumeroAsync(string numeroFactura)
        {
            try
            {
                var factura = await _facturaRepository.GetByNumeroFacturaAsync(numeroFactura);
                return factura != null ? _mapper.Map<FacturaDto>(factura) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener factura por número {NumeroFactura}", numeroFactura);
                throw;
            }
        }

        public async Task<bool> MarcarFacturaPagadaAsync(int facturaId, string metodoPago)
        {
            try
            {
                var factura = await _facturaRepository.GetByIdWithIncludesAsync(facturaId);
                if (factura == null)
                {
                    _logger.LogWarning("⚠️ Factura {FacturaId} no encontrada", facturaId);
                    return false;
                }

                if (factura.Estado == "Pagada")
                {
                    _logger.LogWarning("⚠️ Factura {FacturaId} ya está pagada", facturaId);
                    return false;
                }

                factura.Estado = "Pagada";
                factura.MetodoPago = metodoPago;
                factura.FechaPago = DateTime.Now;

                await _facturaRepository.UpdateAsync(factura);

                // Enviar comprobante de pago por email automáticamente
                try
                {
                    string emailDestino;
                    if (!string.IsNullOrEmpty(factura.Cliente?.Email))
                    {
                        emailDestino = factura.Cliente.Email;
                        _logger.LogInformation("📧 Enviando comprobante de pago a cliente registrado: {Email}", emailDestino);
                    }
                    else
                    {
                        // Usar email por defecto para clientes anónimos
                        emailDestino = _emailService.GetDefaultEmailForAnonymousClients();
                        _logger.LogInformation("📧 Enviando comprobante de pago a cliente anónimo usando email por defecto: {Email}", emailDestino);
                    }

                    await _emailService.EnviarComprobantePagoAsync(factura);
                    _logger.LogInformation("📧 Comprobante de pago enviado automáticamente a {Email}", emailDestino);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "⚠️ Error al enviar comprobante de pago automático para factura {FacturaId}", facturaId);
                    // No falla la operación principal si hay error en el email
                }

                _logger.LogInformation("✅ Factura {FacturaId} marcada como pagada", facturaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al marcar factura {FacturaId} como pagada", facturaId);
                return false;
            }
        }

        public async Task<bool> CancelarFacturaAsync(int facturaId, string motivoCancelacion)
        {
            try
            {
                var factura = await _facturaRepository.GetByIdAsync(facturaId);
                if (factura == null)
                {
                    _logger.LogWarning("⚠️ Factura {FacturaId} no encontrada", facturaId);
                    return false;
                }

                if (factura.Estado == "Pagada")
                {
                    _logger.LogWarning("⚠️ No se puede cancelar factura {FacturaId} ya pagada", facturaId);
                    return false;
                }

                factura.Estado = "Anulada";
                factura.ObservacionesPago = $"ANULADA: {motivoCancelacion}";

                await _facturaRepository.UpdateAsync(factura);

                _logger.LogInformation("✅ Factura {FacturaId} cancelada: {Motivo}", facturaId, motivoCancelacion);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cancelar factura {FacturaId}", facturaId);
                return false;
            }
        }

        // ============================================================================
        // CÁLCULOS Y OPERACIONES MATEMÁTICAS
        // ============================================================================

        public async Task<CalculoTotalesResult> CalcularTotalesOrdenAsync(int ordenId, decimal descuento = 0, decimal propina = 0)
        {
            try
            {
                var orden = await _ordenRepository.GetByIdWithIncludesAsync(ordenId);
                if (orden == null)
                {
                    throw new ArgumentException($"Orden con ID {ordenId} no encontrada");
                }

                decimal subtotal = 0;
                var desglosePorCategoria = new Dictionary<string, decimal>();

                // Calcular subtotal de todos los detalles
                if (orden.DetalleOrdenes != null) // Cambiar DetallesOrden por DetalleOrdenes
                {
                    foreach (var detalle in orden.DetalleOrdenes) // Cambiar DetallesOrden por DetalleOrdenes
                    {
                        var totalDetalle = detalle.Cantidad * detalle.PrecioUnitario;
                        subtotal += totalDetalle;

                        // Desglose por categoría
                        var categoria = detalle.Producto?.Categoria?.Nombre ?? "Sin categoría"; // Cambiar NombreCategoria por Nombre
                        if (!desglosePorCategoria.ContainsKey(categoria))
                            desglosePorCategoria[categoria] = 0;
                        desglosePorCategoria[categoria] += totalDetalle;
                    }
                }

                var subtotalConDescuento = subtotal - descuento;
                var itbis = CalcularITBIS(subtotalConDescuento);
                var totalFinal = subtotalConDescuento + itbis + propina;

                return new CalculoTotalesResult
                {
                    Subtotal = subtotal,
                    Descuento = descuento,
                    SubtotalConDescuento = subtotalConDescuento,
                    ITBIS = itbis,
                    Propina = propina,
                    TotalFinal = totalFinal,
                    MetodoPago = "Efectivo",
                    DesglosePorCategoria = desglosePorCategoria
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al calcular totales para orden {OrdenId}", ordenId);
                throw;
            }
        }

        public decimal CalcularITBIS(decimal subtotal)
        {
            return Math.Round(subtotal * ITBIS_DOMINICANO, 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalcularTotalFinal(decimal subtotal, decimal descuento = 0, decimal propina = 0)
        {
            var subtotalConDescuento = subtotal - descuento;
            var itbis = CalcularITBIS(subtotalConDescuento);
            return subtotalConDescuento + itbis + propina;
        }

        // ============================================================================
        // CONSULTAS POR ESTADO Y FECHA
        // ============================================================================

        public async Task<IEnumerable<FacturaDto>> GetFacturasPorEstadoAsync(string estado)
        {
            try
            {
                var facturas = await _facturaRepository.GetByEstadoAsync(estado);
                return _mapper.Map<IEnumerable<FacturaDto>>(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas por estado {Estado}", estado);
                throw;
            }
        }

        public async Task<IEnumerable<FacturaDto>> GetFacturasPendientesAsync()
        {
            return await GetFacturasPorEstadoAsync("Pendiente");
        }

        public async Task<IEnumerable<FacturaDto>> GetFacturasHoyAsync()
        {
            try
            {
                var facturas = await _facturaRepository.GetFacturasHoyAsync();
                return _mapper.Map<IEnumerable<FacturaDto>>(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas de hoy");
                throw;
            }
        }

        public async Task<IEnumerable<FacturaDto>> GetFacturasPorFechaAsync(DateTime fecha)
        {
            try
            {
                var facturas = await _facturaRepository.GetFacturasPorFechaAsync(fecha);
                return _mapper.Map<IEnumerable<FacturaDto>>(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas de fecha {Fecha}", fecha);
                throw;
            }
        }

        public async Task<IEnumerable<FacturaDto>> GetFacturasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var facturas = await _facturaRepository.GetFacturasPorRangoFechasAsync(fechaInicio, fechaFin);
                return _mapper.Map<IEnumerable<FacturaDto>>(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas por rango {FechaInicio}-{FechaFin}", fechaInicio, fechaFin);
                throw;
            }
        }

        /// <summary>
        /// Obtener facturas por orden
        /// </summary>
        public async Task<IEnumerable<FacturaResponse>> GetFacturasPorOrdenAsync(int ordenId)
        {
            try
            {
                var facturas = await _facturaRepository.GetByOrdenAsync(ordenId);
                return _mapper.Map<IEnumerable<FacturaResponse>>(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas de orden {OrdenId}", ordenId);
                throw;
            }
        }

        // ============================================================================
        // VALIDACIONES Y VERIFICACIONES
        // ============================================================================

        public async Task<ValidacionFacturaResult> ValidarOrdenParaFacturacionAsync(int ordenId)
        {
            var resultado = new ValidacionFacturaResult();

            try
            {
                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                {
                    resultado.Errores.Add("La orden no existe");
                    return resultado;
                }

                // Validar estado de la orden - permitir facturar órdenes pendientes, entregadas y facturadas
                if (orden.Estado != "Entregada" && orden.Estado != "Pendiente" && orden.Estado != "Facturada")
                {
                    resultado.Errores.Add($"La orden debe estar en estado 'Entregada', 'Pendiente' o 'Facturada', actualmente está: {orden.Estado}");
                }

                // Verificar si ya tiene factura pagada
                var facturasExistentes = await _facturaRepository.GetByOrdenAsync(ordenId);
                var facturaPagada = facturasExistentes.FirstOrDefault(f => f.Estado == "Pagada");
                if (facturaPagada != null)
                {
                    resultado.Errores.Add("La orden ya tiene una factura pagada");
                }

                // Calcular total estimado
                var totales = await CalcularTotalesOrdenAsync(ordenId);
                resultado.TotalEstimado = totales.TotalFinal;

                resultado.EsValida = !resultado.Errores.Any();
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al validar orden {OrdenId} para facturación", ordenId);
                resultado.Errores.Add("Error interno al validar la orden");
                return resultado;
            }
        }

        public async Task<bool> ExisteNumeroFacturaAsync(string numeroFactura)
        {
            try
            {
                return await _facturaRepository.NumeroFacturaExisteAsync(numeroFactura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar número de factura {NumeroFactura}", numeroFactura);
                return false;
            }
        }

        public async Task<ValidacionFacturaResult> ValidarMesaParaFacturacionGrupalAsync(int mesaId)
        {
            var resultado = new ValidacionFacturaResult();

            try
            {
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null)
                {
                    resultado.Errores.Add("La mesa no existe");
                    return resultado;
                }

                resultado.EstadoMesa = mesa.Estado;

                // Verificar estado de la mesa
                if (mesa.Estado != "Ocupada")
                {
                    resultado.Errores.Add($"La mesa debe estar ocupada para facturación grupal, actualmente está: {mesa.Estado}");
                }

                // Obtener órdenes de la mesa
                var ordenes = await _ordenRepository.GetOrdenesPorMesaAsync(mesaId);
                var ordenesFacturables = ordenes.Where(o => o.Estado == "Entregada" || o.Estado == "Pendiente").ToList();

                resultado.CantidadOrdenes = ordenesFacturables.Count;

                if (!ordenesFacturables.Any())
                {
                    resultado.Errores.Add("No hay órdenes entregadas o pendientes para facturar en esta mesa");
                }

                // Calcular total estimado
                decimal totalEstimado = 0;
                foreach (var orden in ordenesFacturables)
                {
                    var totales = await CalcularTotalesOrdenAsync(orden.OrdenID);
                    totalEstimado += totales.TotalFinal;
                }
                resultado.TotalEstimado = totalEstimado;

                resultado.EsValida = !resultado.Errores.Any();
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al validar mesa {MesaId} para facturación grupal", mesaId);
                resultado.Errores.Add("Error interno al validar la mesa");
                return resultado;
            }
        }

        // ============================================================================
        // REPORTES BÁSICOS DE FACTURACIÓN
        // ============================================================================

        public async Task<ResumenFacturacionDiaViewModel> GetResumenFacturacionHoyAsync()
        {
            try
            {
                _logger.LogInformation("📊 Generando resumen de facturación del día");
                
                var facturas = await _facturaRepository.GetFacturasHoyAsync();
                var facturasPagadas = facturas.Where(f => f.Estado == "Pagada");
                
                var totalVentas = facturasPagadas.Sum(f => f.Total);
                var totalITBIS = facturasPagadas.Sum(f => f.Impuesto);
                var totalPropinas = facturasPagadas.Sum(f => f.Propina);
                var totalFacturas = facturasPagadas.Count();
                
                // Desglose por método de pago
                var desglosePorMetodoPago = facturasPagadas
                    .GroupBy(f => f.MetodoPago)
                    .ToDictionary(g => g.Key, g => g.Sum(f => f.Total));
                
                var resumen = new ResumenFacturacionDiaViewModel
                {
                    Fecha = DateTime.Today,
                    TotalFacturas = totalFacturas,
                    TotalVentas = totalVentas,
                    TotalITBIS = totalITBIS,
                    TotalPropinas = totalPropinas,
                    DesglosePorMetodoPago = desglosePorMetodoPago
                };
                
                _logger.LogInformation("✅ Resumen de facturación generado: {TotalVentas:C}", totalVentas);
                return resumen;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar resumen de facturación del día");
                throw;
            }
        }

        public async Task<object> GetResumenVentasDelDiaAsync(DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                _logger.LogInformation("📊 Generando resumen detallado de ventas para {Fecha}", fechaConsulta.ToString("dd/MM/yyyy"));

                // Obtener facturas del día
                var facturas = await _facturaRepository.GetFacturasPorFechaAsync(fechaConsulta);
                var facturasPagadas = facturas.Where(f => f.Estado == "Pagada").ToList();

                var ventasBrutas = facturasPagadas.Sum(f => f.Subtotal);
                var totalDescuentos = facturasPagadas.Sum(f => f.Descuento);
                var totalITBIS = facturasPagadas.Sum(f => f.Impuesto);
                var ventasNetas = facturasPagadas.Sum(f => f.Total - f.Impuesto);
                var totalPropinas = facturasPagadas.Sum(f => f.Propina);
                var totalFacturas = facturasPagadas.Count;

                // Desglose por método de pago (ventas)
                var ventasPorMetodoPago = facturasPagadas
                    .GroupBy(f => f.MetodoPago)
                    .ToDictionary(g => g.Key, g => g.Sum(f => f.Total));

                // Desglose por método de pago (cantidad de facturas)
                var facturasPorMetodoPago = facturasPagadas
                    .GroupBy(f => f.MetodoPago)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Cálculos adicionales
                var ticketPromedio = totalFacturas > 0 ? ventasBrutas / totalFacturas : 0;
                var porcentajeITBIS = ventasBrutas > 0 ? (totalITBIS / ventasBrutas) * 100 : 0;

                // Comparación con el día anterior para contexto
                var ayer = fechaConsulta.AddDays(-1);
                var facturasAyer = await _facturaRepository.GetFacturasPorFechaAsync(ayer);
                var ventasAyer = facturasAyer.Where(f => f.Estado == "Pagada").Sum(f => f.Total);
                var cambioPorcentual = ventasAyer > 0 ? ((ventasBrutas - ventasAyer) / ventasAyer) * 100 : 0;

                var resumen = new
                {
                    Fecha = fechaConsulta,
                    TotalFacturas = totalFacturas,
                    VentasBrutas = ventasBrutas,
                    TotalDescuentos = totalDescuentos,
                    TotalITBIS = totalITBIS,
                    VentasNetas = ventasNetas,
                    TotalPropinas = totalPropinas,
                    VentasPorMetodoPago = ventasPorMetodoPago,
                    FacturasPorMetodoPago = facturasPorMetodoPago,
                    // Métricas adicionales
                    TicketPromedio = Math.Round(ticketPromedio, 2),
                    PorcentajeITBIS = Math.Round(porcentajeITBIS, 2),
                    CambioPorcentualDiaAnterior = Math.Round(cambioPorcentual, 2),
                    FacturasAnuladas = facturas.Count(f => f.Estado == "Anulada"),
                    FacturasPendientes = facturas.Count(f => f.Estado == "Pendiente"),
                    // Información del período
                    HoraInicioVentas = facturasPagadas.Any() ? facturasPagadas.Min(f => f.FechaFactura).ToString("HH:mm") : "N/A",
                    HoraUltimaVenta = facturasPagadas.Any() ? facturasPagadas.Max(f => f.FechaFactura).ToString("HH:mm") : "N/A",
                    VentaMaxima = facturasPagadas.Any() ? facturasPagadas.Max(f => f.Total) : 0,
                    VentaMinima = facturasPagadas.Any() ? facturasPagadas.Min(f => f.Total) : 0
                };

                _logger.LogInformation("✅ Resumen detallado de ventas generado: {VentasBrutas:C} en {TotalFacturas} facturas", 
                    ventasBrutas, totalFacturas);

                return resumen;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar resumen detallado de ventas del día");
                throw;
            }
        }

        public async Task<EstadisticasFacturacionViewModel> GetEstadisticasFacturacionAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var facturas = await GetFacturasPorRangoAsync(fechaInicio, fechaFin);
                var facturasPagadas = facturas.Where(f => f.Estado == "Pagada");

                return new EstadisticasFacturacionViewModel
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalFacturas = facturas.Count(),
                    TotalVentas = facturasPagadas.Sum(f => f.TotalNumerico),
                    TotalITBIS = facturasPagadas.Sum(f => f.ImpuestoNumerico),
                    VentaPromedioDiaria = facturasPagadas.Any() ? facturasPagadas.Average(f => f.TotalNumerico) : 0,
                    FacturaConMayorMonto = facturasPagadas.OrderByDescending(f => f.TotalNumerico).FirstOrDefault()?.TotalNumerico ?? 0,
                    FacturaConMenorMonto = facturasPagadas.OrderBy(f => f.TotalNumerico).FirstOrDefault()?.TotalNumerico ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar estadísticas de facturación");
                throw;
            }
        }

        // ============================================================================
        // INTEGRACIÓN CON OTROS SERVICIOS
        // ============================================================================

        public async Task<bool> LiberarMesaPostFacturacionAsync(int mesaId)
        {
            try
            {
                // Verificar si se puede liberar la mesa antes de hacerlo
                var puedeLiberarse = await _mesaService.PuedeLiberarseMesaAsync(mesaId);
                if (!puedeLiberarse)
                {
                    _logger.LogInformation("🪑 Mesa {MesaId} no liberada - aún hay órdenes activas o no pagadas", mesaId);
                    return false;
                }

                // Usar el servicio de mesa para liberar correctamente
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null) return false;

                mesa.Estado = "Libre";
                mesa.FechaUltimaActualizacion = DateTime.Now;

                await _mesaRepository.UpdateAsync(mesa);

                _logger.LogInformation("🪑 Mesa {MesaId} liberada automáticamente post-facturación", mesaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al liberar mesa {MesaId} post-facturación", mesaId);
                return false;
            }
        }

        public async Task<bool> ActualizarEstadoOrdenPostFacturacionAsync(int ordenId)
        {
            try
            {
                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null) return false;

                orden.Estado = "Facturada";
                orden.FechaActualizacion = DateTime.Now;

                await _ordenRepository.UpdateAsync(orden);

                _logger.LogInformation("📋 Orden {OrdenId} marcada como facturada", ordenId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar orden {OrdenId} post-facturación", ordenId);
                return false;
            }
        }

        public async Task<bool> EnviarFacturaPorEmailAsync(int facturaId, string emailDestino, bool incluirPDF = true)
        {
            try
            {
                _logger.LogInformation("📧 Enviando factura {FacturaId} a {Email}", facturaId, emailDestino);

                // Obtener la factura completa
                var factura = await GetFacturaByIdAsync(facturaId);
                if (factura == null)
                {
                    _logger.LogWarning("⚠️ Factura {FacturaId} no encontrada", facturaId);
                    return false;
                }

                // Preparar el contenido del email
                var asunto = $"Factura #{factura.NumeroFactura} - El Criollo Restaurant";
                var contenido = GenerarContenidoEmailFactura(factura);

                // Enviar email usando el método disponible
                var resultado = await _emailService.EnviarNotificacionPersonalizadaAsync(emailDestino, asunto, contenido, true);

                if (resultado)
                {
                    _logger.LogInformation("✅ Factura {FacturaId} enviada exitosamente a {Email}", facturaId, emailDestino);
                    
                    // Registrar el envío en el log de la factura
                    await RegistrarEnvioFacturaAsync(facturaId, emailDestino);
                }
                else
                {
                    _logger.LogWarning("⚠️ Error al enviar factura {FacturaId} a {Email}", facturaId, emailDestino);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar factura {FacturaId} por email", facturaId);
                return false;
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        private async Task<string> GenerarNumeroFacturaUnicoAsync()
        {
            string numeroFactura;
            bool existe;

            do
            {
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var secuencial = DateTime.Now.ToString("HHmmss");
                numeroFactura = $"{PREFIJO_FACTURA}-{fecha}-{secuencial}";
                
                existe = await ExisteNumeroFacturaAsync(numeroFactura);
                
                if (existe)
                {
                    // Esperar un milisegundo para generar un número diferente
                    await Task.Delay(1);
                }
            } while (existe);

            return numeroFactura;
        }

        private string GenerarContenidoEmailFactura(FacturaDto factura)
        {
            var contenido = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
        .header {{ background-color: #d4821a; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .total {{ font-size: 18px; font-weight: bold; color: #d4821a; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🍽️ El Criollo Restaurant</h1>
        <p>Factura #{factura.NumeroFactura}</p>
    </div>
    <div class='content'>
        <h2>Detalles de la Factura</h2>
        <p><strong>Fecha:</strong> {factura.FechaFactura:dd/MM/yyyy HH:mm}</p>
        <p><strong>Método de Pago:</strong> {factura.MetodoPago}</p>
        
        <hr>
        
        <h3>Resumen de Pago</h3>
        <p><strong>Subtotal:</strong> RD$ {factura.SubtotalNumerico:N2}</p>
        <p><strong>Descuento:</strong> RD$ {factura.DescuentoNumerico:N2}</p>
        <p><strong>ITBIS (18%):</strong> RD$ {factura.ImpuestoNumerico:N2}</p>
        <p><strong>Propina:</strong> RD$ {factura.PropinaNumerico:N2}</p>
        <p class='total'><strong>TOTAL:</strong> RD$ {factura.TotalNumerico:N2}</p>
        
        <div class='footer'>
            <p>Gracias por su visita a El Criollo Restaurant</p>
            <p>Para cualquier consulta, por favor contacte a nuestro equipo de atención al cliente.</p>
        </div>
    </div>
</body>
</html>";

            return contenido;
        }

        private async Task RegistrarEnvioFacturaAsync(int facturaId, string emailDestino)
        {
            try
            {
                // Agregar observación sobre el envío
                var factura = await _facturaRepository.GetByIdAsync(facturaId);
                if (factura != null)
                {
                    var observacionActual = factura.ObservacionesPago ?? "";
                    var nuevaObservacion = $"{observacionActual}; Email enviado a {emailDestino} el {DateTime.Now:dd/MM/yyyy HH:mm}";
                    factura.ObservacionesPago = nuevaObservacion;
                    await _facturaRepository.UpdateAsync(factura);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al registrar envío de factura {FacturaId}", facturaId);
            }
        }
    }
}