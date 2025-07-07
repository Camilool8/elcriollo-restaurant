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
            IEmailService emailService,
            IMapper mapper,
            ILogger<FacturaService> logger)
        {
            _facturaRepository = facturaRepository;
            _ordenRepository = ordenRepository;
            _mesaRepository = mesaRepository;
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
                    ClienteID = orden.ClienteID ?? 1, // Cliente por defecto si no hay
                    FechaFactura = DateTime.Now,
                    Subtotal = totales.Subtotal,
                    Descuento = totales.Descuento,
                    Impuesto = totales.ITBIS,
                    Propina = totales.Propina,
                    Total = totales.TotalFinal,
                    MetodoPago = crearFacturaRequest.MetodoPago ?? "Efectivo",
                    Estado = "Pendiente",
                    ObservacionesPago = crearFacturaRequest.Observaciones
                };

                // Guardar en base de datos
                var facturaCreada = await _facturaRepository.AddAsync(nuevaFactura);

                // Actualizar estado de la orden
                await ActualizarEstadoOrdenPostFacturacionAsync(crearFacturaRequest.OrdenId);

                // Liberar mesa si es necesario
                if (orden.MesaID.HasValue)
                {
                    await LiberarMesaPostFacturacionAsync(orden.MesaID.Value);
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
                var ordenesActivas = ordenes.Where(o => o.Estado == "Entregada").ToList();

                if (!ordenesActivas.Any())
                {
                    throw new InvalidOperationException("No hay órdenes activas para facturar en esta mesa");
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

                // Liberar la mesa
                await LiberarMesaPostFacturacionAsync(mesaId);

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
                var factura = await _facturaRepository.GetByIdAsync(facturaId);
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

                // Validar estado de la orden
                if (orden.Estado != "Entregada")
                {
                    resultado.Errores.Add($"La orden debe estar en estado 'Entregada', actualmente está: {orden.Estado}");
                }

                // Verificar si ya tiene factura
                var facturaExistente = await _facturaRepository.GetAllAsync();
                if (facturaExistente.Any(f => f.OrdenID == ordenId && f.Estado != "Anulada"))
                {
                    resultado.Errores.Add("La orden ya tiene una factura asociada");
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
                var ordenesFacturables = ordenes.Where(o => o.Estado == "Entregada").ToList();

                resultado.CantidadOrdenes = ordenesFacturables.Count;

                if (!ordenesFacturables.Any())
                {
                    resultado.Errores.Add("No hay órdenes entregadas para facturar en esta mesa");
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
                var facturasHoy = await GetFacturasHoyAsync();
                var facturasPagadas = facturasHoy.Where(f => f.Estado == "Pagada");

                return new ResumenFacturacionDiaViewModel
                {
                    Fecha = DateTime.Today,
                    TotalFacturas = facturasHoy.Count(),
                    FacturasPagadas = facturasPagadas.Count(),
                    FacturasPendientes = facturasHoy.Count(f => f.Estado == "Pendiente"),
                    TotalVentas = facturasPagadas.Sum(f => f.TotalNumerico),
                    TotalITBIS = facturasPagadas.Sum(f => f.ImpuestoNumerico),
                    TotalPropinas = facturasPagadas.Sum(f => f.PropinaNumerico),
                    PromedioVentaPorFactura = facturasPagadas.Any() ? facturasPagadas.Average(f => f.TotalNumerico) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar resumen de facturación del día");
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