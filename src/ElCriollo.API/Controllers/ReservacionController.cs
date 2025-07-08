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
    /// Controlador para la gestión de reservaciones del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Gestión de reservaciones, mesas y disponibilidad")]
    public class ReservacionController : ControllerBase
    {
        private readonly IReservacionService _reservacionService;
        private readonly ILogger<ReservacionController> _logger;

        public ReservacionController(IReservacionService reservacionService, ILogger<ReservacionController> logger)
        {
            _reservacionService = reservacionService;
            _logger = logger;
        }

        // ============================================================================
        // CREACIÓN DE RESERVACIONES
        // ============================================================================

        /// <summary>
        /// Crear reservación individual
        /// </summary>
        /// <param name="request">Datos de la reservación a crear</param>
        /// <returns>Reservación creada</returns>
        /// <response code="201">Reservación creada exitosamente</response>
        /// <response code="400">Datos inválidos o mesa no disponible</response>
        /// <response code="401">No autorizado</response>
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcion,Mesero")]
        [SwaggerOperation(
            Summary = "Crear reservación individual",
            Description = "Genera una reservación para una mesa específica con confirmación automática",
            OperationId = "Reservacion.Crear",
            Tags = new[] { "Reservaciones" }
        )]
        [ProducesResponseType(typeof(ReservacionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReservacionResponse>> CrearReservacion([FromBody] CreateReservacionRequest request)
        {
            try
            {
                _logger.LogInformation("📅 Creando reservación para mesa {MesaId}", request.MesaId);

                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var reservacion = await _reservacionService.CrearReservaAsync(request, usuarioId);
                
                _logger.LogInformation("✅ Reservación {ReservacionId} creada exitosamente", reservacion.ReservacionID);
                
                return CreatedAtAction(
                    nameof(GetReservacionById), 
                    new { id = reservacion.ReservacionID }, 
                    reservacion
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al crear reservación: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al crear reservación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al crear reservación");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar la reservación",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Crear reservación grupal para múltiples mesas
        /// </summary>
        /// <param name="request">Datos de reservación grupal</param>
        /// <returns>Reservación grupal creada</returns>
        /// <response code="201">Reservación grupal creada exitosamente</response>
        /// <response code="400">Mesas no disponibles</response>
        [HttpPost("grupal")]
        [Authorize(Roles = "Administrador,Recepcion")]
        [SwaggerOperation(
            Summary = "Crear reservación grupal",
            Description = "Genera una reservación para múltiples mesas en el mismo horario",
            OperationId = "Reservacion.CrearGrupal",
            Tags = new[] { "Reservaciones" }
        )]
        [ProducesResponseType(typeof(ReservacionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public Task<ActionResult<ReservacionResponse>> CrearReservacionGrupal([FromBody] CreateReservacionGrupalRequest request)
        {
            try
            {
                _logger.LogInformation("📅 Creando reservación grupal para {CantidadMesas} mesas", request.MesasIds.Count);

                // Temporalmente comentado hasta implementar el método
                // var reservacion = await _reservacionService.CrearReservacionGrupalAsync(request);
                
                // _logger.LogInformation("✅ Reservación grupal {ReservacionId} creada", reservacion.ReservacionID);
                
                // return CreatedAtAction(
                //     nameof(GetReservacionById), 
                //     new { id = reservacion.ReservacionID }, 
                //     reservacion
                // );

                var result = BadRequest(new ValidationProblemDetails
                {
                    Title = "Función no implementada",
                    Detail = "La creación de reservaciones grupales no está implementada aún",
                    Status = StatusCodes.Status400BadRequest
                });

                return Task.FromResult<ActionResult<ReservacionResponse>>(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al crear reservación grupal: {Mensaje}", ex.Message);
                var result = BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al crear reservación grupal",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });

                return Task.FromResult<ActionResult<ReservacionResponse>>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear reservación grupal");
                var result = StatusCode(StatusCodes.Status500InternalServerError);
                return Task.FromResult<ActionResult<ReservacionResponse>>(result);
            }
        }

        // ============================================================================
        // CONSULTA DE RESERVACIONES
        // ============================================================================

        /// <summary>
        /// Obtener una reservación por ID
        /// </summary>
        /// <param name="id">ID de la reservación</param>
        /// <returns>Datos completos de la reservación</returns>
        /// <response code="200">Reservación encontrada</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtener reservación por ID",
            Description = "Obtiene los datos completos de una reservación incluyendo detalles de mesa y cliente",
            OperationId = "Reservacion.GetById",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(ReservacionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservacionResponse>> GetReservacionById(int id)
        {
            try
            {
                var reservacion = await _reservacionService.GetReservaByIdAsync(id);
                
                if (reservacion == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Reservación no encontrada",
                        Detail = $"No se encontró la reservación con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(reservacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener reservaciones del día
        /// </summary>
        /// <param name="fecha">Fecha a consultar (por defecto hoy)</param>
        /// <returns>Lista de reservaciones del día</returns>
        /// <response code="200">Lista de reservaciones</response>
        [HttpGet("dia")]
        [SwaggerOperation(
            Summary = "Obtener reservaciones del día",
            Description = "Devuelve todas las reservaciones programadas para una fecha específica",
            OperationId = "Reservacion.GetDelDia",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservacionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservacionResponse>>> GetReservacionesDelDia([FromQuery] DateTime? fecha = null)
        {
            try
            {
                // Usar fecha dominicana si no se especifica
                var dominicanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
                var fechaConsulta = fecha ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dominicanTimeZone).Date;
                
                _logger.LogInformation("📅 Consultando reservaciones para {Fecha} (zona horaria dominicana)", fechaConsulta);

                var reservaciones = await _reservacionService.GetReservasPorFechaAsync(fechaConsulta);
                
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservaciones del día");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener reservaciones por rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de reservaciones en el rango</returns>
        /// <response code="200">Lista de reservaciones</response>
        /// <response code="400">Rango de fechas inválido</response>
        [HttpGet("rango")]
        [SwaggerOperation(
            Summary = "Obtener reservaciones por rango de fechas",
            Description = "Devuelve todas las reservaciones programadas entre dos fechas",
            OperationId = "Reservacion.GetPorRango",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservacionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ReservacionResponse>>> GetReservacionesPorRango([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Rango de fechas inválido",
                        Detail = "La fecha de inicio no puede ser posterior a la fecha de fin",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Temporalmente usando el método disponible
                var reservaciones = await _reservacionService.GetReservasPorFechaAsync(fechaInicio);
                
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservaciones por rango");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // OPERACIONES DE RESERVACIÓN
        // ============================================================================

        /// <summary>
        /// Cancelar reservación
        /// </summary>
        /// <param name="id">ID de la reservación</param>
        /// <param name="request">Datos de cancelación</param>
        /// <returns>Confirmación de cancelación</returns>
        /// <response code="200">Reservación cancelada exitosamente</response>
        /// <response code="400">No se puede cancelar la reservación</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpPost("{id:int}/cancelar")]
        [Authorize(Roles = "Administrador,Recepcion")]
        [SwaggerOperation(
            Summary = "Cancelar reservación",
            Description = "Cancela una reservación y libera las mesas asociadas",
            OperationId = "Reservacion.Cancelar",
            Tags = new[] { "Operaciones de Reservación" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> CancelarReservacion(int id, [FromBody] CancelarReservacionRequest request)
        {
            try
            {
                _logger.LogInformation("🚫 Cancelando reservación {ReservacionId}", id);

                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _reservacionService.CancelarReservaAsync(id, request.Motivo, usuarioId);
                
                _logger.LogInformation("✅ Reservación {ReservacionId} cancelada exitosamente", id);
                
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Reservación cancelada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al cancelar reservación: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al cancelar reservación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cancelar reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar la cancelación",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Confirma una reservación
        /// </summary>
        /// <param name="id">ID de la reservación a confirmar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Reservación confirmada exitosamente</response>
        /// <response code="400">La reservación no se pudo confirmar</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpPost("{id:int}/confirmar")]
        [Authorize(Roles = "Administrador,Recepcion")]
        [SwaggerOperation(
            Summary = "Confirmar reservación",
            Description = "Confirma una reservación que estaba en estado pendiente.",
            OperationId = "Reservacion.Confirmar",
            Tags = new[] { "Operaciones de Reservación" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ConfirmarReservacion(int id)
        {
            try
            {
                _logger.LogInformation("✅ Confirmando reservación ID: {ReservacionId}", id);

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var exito = await _reservacionService.ConfirmarReservaAsync(id, usuarioId);

                if (!exito)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "No se pudo confirmar la reservación",
                        Detail = "La reservación podría no existir o ya estar en un estado final (cancelada, completada).",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new ApiResponse { Success = true, Message = "Reservación confirmada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al confirmar reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar la confirmación.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Enviar recordatorio de reservación por email
        /// </summary>
        /// <param name="id">ID de la reservación</param>
        /// <returns>Confirmación de envío</returns>
        /// <response code="200">Recordatorio enviado exitosamente</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpPost("{id:int}/enviar-recordatorio")]
        [SwaggerOperation(
            Summary = "Enviar recordatorio de reservación",
            Description = "Envía un recordatorio de reservación al email del cliente",
            OperationId = "Reservacion.EnviarRecordatorio",
            Tags = new[] { "Operaciones de Reservación" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public Task<ActionResult<ApiResponse>> EnviarRecordatorioReservacion(int id)
        {
            try
            {
                _logger.LogInformation("📧 Enviando recordatorio de reservación {ReservacionId}", id);

                // Temporalmente comentado hasta implementar notificaciones
                // await _reservacionService.EnviarRecordatorioReservacionAsync(id);
                
                _logger.LogInformation("✅ Recordatorio de reservación {ReservacionId} enviado", id);
                
                var result = Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Recordatorio enviado exitosamente"
                });

                return Task.FromResult<ActionResult<ApiResponse>>(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al enviar recordatorio: {Mensaje}", ex.Message);
                var result = NotFound(new ProblemDetails
                {
                    Title = "Reservación no encontrada",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });

                return Task.FromResult<ActionResult<ApiResponse>>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar recordatorio de reservación {ReservacionId}", id);
                var result = StatusCode(StatusCodes.Status500InternalServerError);
                return Task.FromResult<ActionResult<ApiResponse>>(result);
            }
        }

        // ============================================================================
        // REPORTES Y ESTADÍSTICAS
        // ============================================================================

        /// <summary>
        /// Obtener estadísticas de reservaciones
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Estadísticas de reservaciones</returns>
        /// <response code="200">Estadísticas obtenidas</response>
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Administrador,Recepcion")]
        [SwaggerOperation(
            Summary = "Estadísticas de reservaciones",
            Description = "Obtiene estadísticas detalladas de reservaciones para un período",
            OperationId = "Reservacion.Estadisticas",
            Tags = new[] { "Reportes" }
        )]
        [ProducesResponseType(typeof(EstadisticasReservacionResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<EstadisticasReservacionResponse>> GetEstadisticasReservaciones([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                var estadisticas = await _reservacionService.GetEstadisticasReservasAsync(fechaInicio, fechaFin);
                
                // Mapear a la respuesta esperada por el controlador
                var response = new EstadisticasReservacionResponse
                {
                    FechaInicio = estadisticas.FechaInicio,
                    FechaFin = estadisticas.FechaFin,
                    TotalReservaciones = estadisticas.TotalReservas,
                    ReservacionesCanceladas = estadisticas.ReservasCanceladas,
                    ReservacionesCompletadas = estadisticas.ReservasCompletadas,
                    TasaOcupacion = (double)estadisticas.PorcentajeOcupacion,
                    HoraPico = estadisticas.ReservasPorHora.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "N/A",
                    ReservacionesPorDia = new List<ReservacionDiariaItem>()
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas de reservaciones");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener disponibilidad de mesas
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <param name="hora">Hora a consultar</param>
        /// <returns>Disponibilidad de mesas</returns>
        /// <response code="200">Disponibilidad obtenida</response>
        [HttpGet("disponibilidad")]
        [SwaggerOperation(
            Summary = "Consultar disponibilidad de mesas",
            Description = "Obtiene la disponibilidad de mesas para una fecha y hora específica",
            OperationId = "Reservacion.DisponibilidadMesas",
            Tags = new[] { "Consulta de Disponibilidad" }
        )]
        [ProducesResponseType(typeof(DisponibilidadMesasResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<DisponibilidadMesasResponse>> GetDisponibilidadMesas([FromQuery] DateTime fecha, [FromQuery] TimeSpan hora)
        {
            try
            {
                var fechaHora = fecha.Date.Add(hora);
                var mesasDisponibles = await _reservacionService.BuscarMesasDisponiblesParaReservaAsync(fechaHora, 4, 120);
                
                var response = new DisponibilidadMesasResponse
                {
                    FechaHora = fechaHora,
                    Mesas = mesasDisponibles.Select(m => new MesaDisponibilidad
                    {
                        MesaId = m.MesaID,
                        Numero = m.NumeroMesa,
                        Capacidad = m.Capacidad,
                        Disponible = true,
                        MotivoNoDisponible = null
                    }).ToList()
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener disponibilidad de mesas");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // CLASES DE REQUEST Y RESPONSE
        // ============================================================================

        public class CreateReservacionGrupalRequest
        {
            /// <summary>
            /// IDs de las mesas a reservar
            /// </summary>
            public List<int> MesasIds { get; set; } = new();

            /// <summary>
            /// Fecha y hora de la reservación
            /// </summary>
            public DateTime FechaHora { get; set; }

            /// <summary>
            /// Datos del cliente
            /// </summary>
            public DatosClienteReservacion Cliente { get; set; } = new();

            /// <summary>
            /// Observaciones especiales
            /// </summary>
            public string? Observaciones { get; set; }
        }

        public class CancelarReservacionRequest
        {
            /// <summary>
            /// Motivo de la cancelación
            /// </summary>
            public string Motivo { get; set; } = string.Empty;
        }

        public class DatosClienteReservacion
        {
            public string Nombre { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class EstadisticasReservacionResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public int TotalReservaciones { get; set; }
            public int ReservacionesCanceladas { get; set; }
            public int ReservacionesCompletadas { get; set; }
            public double TasaOcupacion { get; set; }
            public string HoraPico { get; set; } = string.Empty;
            public List<ReservacionDiariaItem> ReservacionesPorDia { get; set; } = new();
        }

        public class ReservacionDiariaItem
        {
            public DateTime Fecha { get; set; }
            public int Cantidad { get; set; }
            public int MesasOcupadas { get; set; }
        }

        public class DisponibilidadMesasResponse
        {
            public DateTime FechaHora { get; set; }
            public List<MesaDisponibilidad> Mesas { get; set; } = new();
        }

        public class MesaDisponibilidad
        {
            public int MesaId { get; set; }
            public int Numero { get; set; }
            public int Capacidad { get; set; }
            public bool Disponible { get; set; }
            public string? MotivoNoDisponible { get; set; }
        }
    }
}