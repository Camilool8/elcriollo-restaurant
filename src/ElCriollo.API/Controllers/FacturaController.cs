using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de facturación del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Gestión de facturación, pagos y comprobantes fiscales con ITBIS dominicano")]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturaService _facturaService;
        private readonly ILogger<FacturaController> _logger;

        public FacturaController(IFacturaService facturaService, ILogger<FacturaController> logger)
        {
            _facturaService = facturaService;
            _logger = logger;
        }

        // ============================================================================
        // CREACIÓN DE FACTURAS
        // ============================================================================

        /// <summary>
        /// Crear factura individual para una orden
        /// </summary>
        /// <param name="request">Datos de la factura a crear</param>
        /// <returns>Factura creada con cálculos de ITBIS</returns>
        /// <response code="201">Factura creada exitosamente</response>
        /// <response code="400">Datos inválidos o orden ya facturada</response>
        /// <response code="401">No autorizado</response>
        [HttpPost]
        [Authorize(Roles = "Administrador,Cajero,Mesero")]
        [SwaggerOperation(
            Summary = "Crear factura individual",
            Description = "Genera una factura para una orden específica con cálculo automático de ITBIS (18%) y libera la mesa si aplica",
            OperationId = "Factura.Crear",
            Tags = new[] { "Facturación" }
        )]
        [ProducesResponseType(typeof(FacturaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<FacturaDto>> CrearFactura([FromBody] CrearFacturaRequest request)
        {
            try
            {
                _logger.LogInformation("💳 Creando factura para orden {OrdenId}", request.OrdenId);

                var factura = await _facturaService.CrearFacturaAsync(request);
                
                _logger.LogInformation("✅ Factura {NumeroFactura} creada exitosamente", factura.NumeroFactura);
                
                return CreatedAtAction(
                    nameof(GetFacturaById), 
                    new { id = factura.FacturaID }, 
                    factura
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al crear factura: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al crear factura",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al crear factura");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar la factura",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Crear factura grupal para todas las órdenes de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa a facturar</param>
        /// <param name="request">Datos de pago y descuentos</param>
        /// <returns>Factura grupal con totales consolidados</returns>
        /// <response code="201">Factura grupal creada exitosamente</response>
        /// <response code="400">Mesa sin órdenes pendientes</response>
        [HttpPost("mesa/{mesaId:int}/factura-grupal")]
        [Authorize(Roles = "Administrador,Cajero,Mesero")]
        [SwaggerOperation(
            Summary = "Crear factura grupal por mesa",
            Description = "Genera una factura única para todas las órdenes activas de una mesa. Libera la mesa automáticamente",
            OperationId = "Factura.CrearGrupal",
            Tags = new[] { "Facturación" }
        )]
        [ProducesResponseType(typeof(FacturaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FacturaDto>> CrearFacturaGrupal(int mesaId, [FromBody] CrearFacturaGrupalRequest request)
        {
            try
            {
                _logger.LogInformation("💳 Creando factura grupal para mesa {MesaId}", mesaId);

                var factura = await _facturaService.CrearFacturaGrupalAsync(
                    mesaId, 
                    request.MetodoPago, 
                    request.Descuento ?? 0,
                    request.Propina ?? 0
                );
                
                _logger.LogInformation("✅ Factura grupal {NumeroFactura} creada para mesa {MesaId}", factura.NumeroFactura, mesaId);
                
                return CreatedAtAction(
                    nameof(GetFacturaById), 
                    new { id = factura.FacturaID }, 
                    factura
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al crear factura grupal: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al crear factura grupal",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear factura grupal para mesa {MesaId}", mesaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // CONSULTA DE FACTURAS
        // ============================================================================

        /// <summary>
        /// Obtener una factura por ID
        /// </summary>
        /// <param name="id">ID de la factura</param>
        /// <returns>Datos completos de la factura</returns>
        /// <response code="200">Factura encontrada</response>
        /// <response code="404">Factura no encontrada</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtener factura por ID",
            Description = "Obtiene los datos completos de una factura incluyendo detalles, totales e ITBIS",
            OperationId = "Factura.GetById",
            Tags = new[] { "Consulta de Facturas" }
        )]
        [ProducesResponseType(typeof(FacturaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FacturaDto>> GetFacturaById(int id)
        {
            try
            {
                var factura = await _facturaService.GetFacturaByIdAsync(id);
                
                if (factura == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Factura no encontrada",
                        Detail = $"No se encontró la factura con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(factura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener factura {FacturaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener facturas del día
        /// </summary>
        /// <param name="fecha">Fecha a consultar (por defecto hoy)</param>
        /// <returns>Lista de facturas del día</returns>
        /// <response code="200">Lista de facturas</response>
        [HttpGet("dia")]
        [SwaggerOperation(
            Summary = "Obtener facturas del día",
            Description = "Devuelve todas las facturas generadas en una fecha específica",
            OperationId = "Factura.GetDelDia",
            Tags = new[] { "Consulta de Facturas" }
        )]
        [ProducesResponseType(typeof(IEnumerable<FacturaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FacturaDto>>> GetFacturasDelDia([FromQuery] DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                _logger.LogInformation("📅 Consultando facturas del día {Fecha}", fechaConsulta.ToShortDateString());
                
                // GetFacturasPorRangoAsync para obtener facturas de un día completo
                var facturas = await _facturaService.GetFacturasPorRangoAsync(fechaConsulta, fechaConsulta.AddDays(1).AddSeconds(-1));
                return Ok(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas del día");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener facturas por rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del rango</param>
        /// <param name="fechaFin">Fecha fin del rango</param>
        /// <returns>Lista de facturas en el rango</returns>
        /// <response code="200">Lista de facturas</response>
        /// <response code="400">Rango de fechas inválido</response>
        [HttpGet("rango")]
        [SwaggerOperation(
            Summary = "Obtener facturas por rango de fechas",
            Description = "Devuelve todas las facturas generadas entre dos fechas",
            OperationId = "Factura.GetPorRango",
            Tags = new[] { "Consulta de Facturas" }
        )]
        [ProducesResponseType(typeof(IEnumerable<FacturaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<FacturaDto>>> GetFacturasPorRango([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Rango de fechas inválido",
                        Detail = "La fecha de inicio debe ser anterior a la fecha fin",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation("📅 Consultando facturas desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);
                
                var facturas = await _facturaService.GetFacturasPorRangoAsync(fechaInicio, fechaFin);
                return Ok(facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener facturas por rango");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // OPERACIONES DE FACTURA
        // ============================================================================

        /// <summary>
        /// Anular una factura
        /// </summary>
        /// <param name="id">ID de la factura</param>
        /// <param name="request">Motivo de anulación</param>
        /// <returns>Confirmación de anulación</returns>
        /// <response code="200">Factura anulada exitosamente</response>
        /// <response code="400">Factura no puede ser anulada</response>
        /// <response code="404">Factura no encontrada</response>
        [HttpPost("{id:int}/anular")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Anular factura",
            Description = "Anula una factura emitida. Solo administradores pueden anular facturas",
            OperationId = "Factura.Anular",
            Tags = new[] { "Operaciones de Factura" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> AnularFactura(int id, [FromBody] AnularFacturaRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("🚫 Anulando factura {FacturaId}. Motivo: {Motivo}", id, request.Motivo);

                // CancelarFacturaAsync en lugar de AnularFacturaAsync
                var resultado = await _facturaService.CancelarFacturaAsync(id, request.Motivo);

                if (!resultado)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Factura no encontrada",
                        Detail = $"No se encontró la factura con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Factura {FacturaId} anulada exitosamente", id);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Factura anulada exitosamente",
                    Data = new { FacturaId = id, Estado = "Anulada", Motivo = request.Motivo }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al anular factura: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al anular factura",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al anular factura {FacturaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Enviar factura por email
        /// </summary>
        /// <param name="id">ID de la factura</param>
        /// <param name="request">Email del destinatario</param>
        /// <returns>Confirmación de envío</returns>
        /// <response code="200">Factura enviada exitosamente</response>
        /// <response code="404">Factura no encontrada</response>
        [HttpPost("{id:int}/enviar-email")]
        [SwaggerOperation(
            Summary = "Enviar factura por email",
            Description = "Envía una copia de la factura al email especificado",
            OperationId = "Factura.EnviarEmail",
            Tags = new[] { "Operaciones de Factura" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> EnviarFacturaPorEmail(int id, [FromBody] EnviarFacturaEmailRequest request)
        {
            try
            {
                _logger.LogInformation("📧 Enviando factura {FacturaId} a {Email}", id, request.Email);

                var resultado = await _facturaService.EnviarFacturaPorEmailAsync(id, request.Email, true);

                if (!resultado)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Error al enviar factura",
                        Detail = $"No se pudo enviar la factura con ID {id}. Verifique que la factura exista y que el email sea válido.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Factura {FacturaId} enviada exitosamente a {Email}", id, request.Email);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Factura enviada exitosamente por email",
                    Data = new { FacturaId = id, Email = request.Email }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar factura {FacturaId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtener resumen de ventas del día
        /// </summary>
        /// <param name="fecha">Fecha a consultar (por defecto hoy)</param>
        /// <returns>Resumen de ventas con totales</returns>
        /// <response code="200">Resumen de ventas</response>
        [HttpGet("resumen-ventas")]
        [Authorize(Roles = "Administrador,Cajero")]
        [SwaggerOperation(
            Summary = "Resumen de ventas del día",
            Description = "Obtiene un resumen de las ventas del día con totales, ITBIS y métodos de pago",
            OperationId = "Factura.ResumenVentas",
            Tags = new[] { "Reportes" }
        )]
        [ProducesResponseType(typeof(ResumenVentasResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenVentasResponse>> GetResumenVentas([FromQuery] DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                _logger.LogInformation("📊 Generando resumen de ventas para {Fecha}", fechaConsulta.ToShortDateString());
                
                // Usar el método implementado GetResumenVentasDelDiaAsync
                var resumenDetallado = await _facturaService.GetResumenVentasDelDiaAsync(fechaConsulta);
                var datosResumen = (dynamic)resumenDetallado;
                
                var resumen = new ResumenVentasResponse
                {
                    Fecha = datosResumen.Fecha,
                    TotalFacturas = datosResumen.TotalFacturas,
                    VentasBrutas = datosResumen.VentasBrutas,
                    TotalDescuentos = datosResumen.TotalDescuentos,
                    TotalITBIS = datosResumen.TotalITBIS,
                    VentasNetas = datosResumen.VentasNetas,
                    TotalPropinas = datosResumen.TotalPropinas,
                    VentasPorMetodoPago = ((IDictionary<string, object>)datosResumen.VentasPorMetodoPago)
                        .ToDictionary(kvp => kvp.Key, kvp => Convert.ToDecimal(kvp.Value)),
                    FacturasPorMetodoPago = ((IDictionary<string, object>)datosResumen.FacturasPorMetodoPago)
                        .ToDictionary(kvp => kvp.Key, kvp => Convert.ToInt32(kvp.Value))
                };
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar resumen de ventas");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener estadísticas de facturación
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estadísticas detalladas de facturación</returns>
        /// <response code="200">Estadísticas de facturación</response>
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Estadísticas de facturación",
            Description = "Obtiene estadísticas detalladas de facturación para un período",
            OperationId = "Factura.Estadisticas",
            Tags = new[] { "Reportes" }
        )]
        [ProducesResponseType(typeof(EstadisticasFacturacionResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<EstadisticasFacturacionResponse>> GetEstadisticasFacturacion([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando estadísticas de facturación desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);
                
                var estadisticas = await _facturaService.GetEstadisticasFacturacionAsync(fechaInicio, fechaFin);
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar estadísticas de facturación");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REQUESTS ESPECÍFICOS DEL CONTROLADOR
        // ============================================================================

        /// <summary>
        /// Request para crear factura grupal
        /// </summary>
        public class CrearFacturaGrupalRequest
        {
            /// <summary>
            /// Método de pago
            /// </summary>
            public string MetodoPago { get; set; } = "Efectivo";

            /// <summary>
            /// Descuento aplicado (opcional)
            /// </summary>
            public decimal? Descuento { get; set; }

            /// <summary>
            /// Propina incluida (opcional)
            /// </summary>
            public decimal? Propina { get; set; }
        }

        /// <summary>
        /// Request para anular factura
        /// </summary>
        public class AnularFacturaRequest
        {
            /// <summary>
            /// Motivo de la anulación
            /// </summary>
            public string Motivo { get; set; } = string.Empty;
        }

        /// <summary>
        /// Request para enviar factura por email
        /// </summary>
        public class EnviarFacturaEmailRequest
        {
            /// <summary>
            /// Email del destinatario
            /// </summary>
            public string Email { get; set; } = string.Empty;
        }

        /// <summary>
        /// Response de resumen de ventas
        /// </summary>
        public class ResumenVentasResponse
        {
            public DateTime Fecha { get; set; }
            public int TotalFacturas { get; set; }
            public decimal VentasBrutas { get; set; }
            public decimal TotalDescuentos { get; set; }
            public decimal TotalITBIS { get; set; }
            public decimal VentasNetas { get; set; }
            public decimal TotalPropinas { get; set; }
            public Dictionary<string, decimal> VentasPorMetodoPago { get; set; } = new();
            public Dictionary<string, int> FacturasPorMetodoPago { get; set; } = new();
        }

        /// <summary>
        /// Response de estadísticas de facturación
        /// </summary>
        public class EstadisticasFacturacionResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public int TotalFacturas { get; set; }
            public int FacturasAnuladas { get; set; }
            public decimal PromedioVentaDiaria { get; set; }
            public decimal PromedioFactura { get; set; }
            public decimal TotalVentas { get; set; }
            public decimal TotalITBIS { get; set; }
            public string DiaMayorVenta { get; set; } = string.Empty;
            public string DiaMenorVenta { get; set; } = string.Empty;
            public List<EstadisticaVentaDiariaItem> VentasPorDia { get; set; } = new();
        }

        /// <summary>
        /// Item de estadística de venta diaria para facturación
        /// </summary>
        public class EstadisticaVentaDiariaItem
        {
            public DateTime Fecha { get; set; }
            public decimal Total { get; set; }
            public int CantidadFacturas { get; set; }
        }
    }
}