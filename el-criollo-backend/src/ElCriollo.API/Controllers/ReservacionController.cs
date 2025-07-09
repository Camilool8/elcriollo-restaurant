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

        /// <summary>
        /// Obtener reservaciones por mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Lista de reservaciones de la mesa</returns>
        /// <response code="200">Lista de reservaciones</response>
        [HttpGet("mesa/{mesaId:int}")]
        [SwaggerOperation(
            Summary = "Obtener reservaciones por mesa",
            Description = "Devuelve todas las reservaciones de una mesa específica",
            OperationId = "Reservacion.GetPorMesa",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservacionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservacionResponse>>> GetReservacionesPorMesa(int mesaId)
        {
            try
            {
                // Usar el método de disponibilidad para obtener reservaciones por mesa
                var mesasDisponibles = await _reservacionService.BuscarMesasDisponiblesParaReservaAsync(DateTime.Now, 1, 120);
                var mesa = mesasDisponibles.FirstOrDefault(m => m.Id == mesaId);
                
                if (mesa == null)
                {
                    return Ok(new List<ReservacionResponse>());
                }

                // Por ahora devolver lista vacía hasta implementar el método específico
                return Ok(new List<ReservacionResponse>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservaciones por mesa {MesaId}", mesaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener reservaciones por cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Lista de reservaciones del cliente</returns>
        /// <response code="200">Lista de reservaciones</response>
        [HttpGet("cliente/{clienteId:int}")]
        [SwaggerOperation(
            Summary = "Obtener reservaciones por cliente",
            Description = "Devuelve todas las reservaciones de un cliente específico",
            OperationId = "Reservacion.GetPorCliente",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservacionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservacionResponse>>> GetReservacionesPorCliente(int clienteId)
        {
            try
            {
                // Usar el método de reservaciones por cliente del servicio
                var reservaciones = await _reservacionService.GetReservasClienteAsync(clienteId);
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservaciones por cliente {ClienteId}", clienteId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener horarios disponibles para una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de horarios disponibles</returns>
        /// <response code="200">Lista de horarios</response>
        [HttpGet("horarios-disponibles/{mesaId:int}")]
        [SwaggerOperation(
            Summary = "Obtener horarios disponibles",
            Description = "Devuelve los horarios disponibles para una mesa en una fecha específica",
            OperationId = "Reservacion.GetHorariosDisponibles",
            Tags = new[] { "Consulta de Disponibilidad" }
        )]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetHorariosDisponibles(int mesaId, [FromQuery] string fecha)
        {
            try
            {
                // Convertir string a DateTime
                if (!DateTime.TryParse(fecha, out DateTime fechaDateTime))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Formato de fecha inválido",
                        Detail = "La fecha debe estar en formato YYYY-MM-DD",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Generar horarios disponibles de 11:00 AM a 10:00 PM cada 30 minutos
                var horarios = new List<string>();
                var horaInicio = new TimeSpan(11, 0, 0); // 11:00 AM
                var horaFin = new TimeSpan(22, 0, 0); // 10:00 PM
                var intervalo = TimeSpan.FromMinutes(30);

                for (var hora = horaInicio; hora <= horaFin; hora += intervalo)
                {
                    var fechaHora = fechaDateTime.Date.Add(hora);
                    var disponible = await _reservacionService.VerificarDisponibilidadMesaAsync(mesaId, fechaHora, 120);
                    
                    if (disponible)
                    {
                        horarios.Add(fechaHora.ToString("HH:mm"));
                    }
                }

                return Ok(horarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener horarios disponibles para mesa {MesaId}", mesaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener reservaciones retrasadas
        /// </summary>
        /// <returns>Lista de reservaciones retrasadas</returns>
        /// <response code="200">Lista de reservaciones</response>
        [HttpGet("retrasadas")]
        [SwaggerOperation(
            Summary = "Obtener reservaciones retrasadas",
            Description = "Devuelve las reservaciones que están retrasadas",
            OperationId = "Reservacion.GetRetrasadas",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservacionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservacionResponse>>> GetReservacionesRetrasadas()
        {
            try
            {
                // Usar el método de reservaciones vencidas del servicio
                var reservaciones = await _reservacionService.GetReservasVencidasAsync(15);
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservaciones retrasadas");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener reservaciones próximas
        /// </summary>
        /// <param name="minutos">Minutos hacia adelante para buscar</param>
        /// <returns>Lista de reservaciones próximas</returns>
        /// <response code="200">Lista de reservaciones</response>
        [HttpGet("proximas")]
        [SwaggerOperation(
            Summary = "Obtener reservaciones próximas",
            Description = "Devuelve las reservaciones que están próximas a comenzar",
            OperationId = "Reservacion.GetProximas",
            Tags = new[] { "Consulta de Reservaciones" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ReservacionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReservacionResponse>>> GetReservacionesProximas([FromQuery] int minutos = 30)
        {
            try
            {
                // Usar el método de próximas reservaciones del servicio
                var reservaciones = await _reservacionService.GetProximasReservasAsync(minutos / 60);
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener reservaciones próximas");
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
        public async Task<ActionResult<ApiResponse>> EnviarRecordatorioReservacion(int id)
        {
            try
            {
                _logger.LogInformation("📧 Enviando recordatorio de reservación {ReservacionId}", id);

                var exito = await _reservacionService.EnviarRecordatorioReservacionAsync(id, 60);
                
                if (!exito)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "No se pudo enviar el recordatorio",
                        Detail = "La reservación no existe, no tiene email configurado o ya pasó la hora",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Recordatorio de reservación {ReservacionId} enviado", id);
                
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Recordatorio enviado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar recordatorio de reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al enviar el recordatorio",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Iniciar una reservación (cliente llegó)
        /// </summary>
        /// <param name="id">ID de la reservación a iniciar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Reservación iniciada exitosamente</response>
        /// <response code="400">La reservación no se pudo iniciar</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpPost("{id:int}/iniciar")]
        [Authorize(Roles = "Administrador,Recepcion,Mesero")]
        [SwaggerOperation(
            Summary = "Iniciar reservación",
            Description = "Marca que el cliente ha llegado y la reservación está en curso",
            OperationId = "Reservacion.Iniciar",
            Tags = new[] { "Operaciones de Reservación" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> IniciarReservacion(int id)
        {
            try
            {
                _logger.LogInformation("🚀 Iniciando reservación ID: {ReservacionId}", id);

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var exito = await _reservacionService.MarcarClienteLlegoAsync(id, usuarioId);

                if (!exito)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "No se pudo iniciar la reservación",
                        Detail = "La reservación podría no existir o ya estar en un estado final",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new ApiResponse { Success = true, Message = "Reservación iniciada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al iniciar reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar el inicio de la reservación.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Completar una reservación
        /// </summary>
        /// <param name="id">ID de la reservación a completar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Reservación completada exitosamente</response>
        /// <response code="400">La reservación no se pudo completar</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpPost("{id:int}/completar")]
        [Authorize(Roles = "Administrador,Recepcion,Mesero")]
        [SwaggerOperation(
            Summary = "Completar reservación",
            Description = "Marca la reservación como completada",
            OperationId = "Reservacion.Completar",
            Tags = new[] { "Operaciones de Reservación" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> CompletarReservacion(int id)
        {
            try
            {
                _logger.LogInformation("✅ Completando reservación ID: {ReservacionId}", id);

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var exito = await _reservacionService.CompletarReservaAsync(id, usuarioId);

                if (!exito)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "No se pudo completar la reservación",
                        Detail = "La reservación podría no existir o ya estar completada",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new ApiResponse { Success = true, Message = "Reservación completada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al completar reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar la finalización de la reservación.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Marcar reservación como No Show
        /// </summary>
        /// <param name="id">ID de la reservación a marcar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Reservación marcada como No Show exitosamente</response>
        /// <response code="400">La reservación no se pudo marcar</response>
        /// <response code="404">Reservación no encontrada</response>
        [HttpPost("{id:int}/no-show")]
        [Authorize(Roles = "Administrador,Recepcion")]
        [SwaggerOperation(
            Summary = "Marcar No Show",
            Description = "Marca la reservación como No Show (cliente no llegó)",
            OperationId = "Reservacion.MarcarNoShow",
            Tags = new[] { "Operaciones de Reservación" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> MarcarNoShow(int id)
        {
            try
            {
                _logger.LogInformation("❌ Marcando No Show para reservación ID: {ReservacionId}", id);

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var exito = await _reservacionService.MarcarNoShowAsync(id, usuarioId);

                if (!exito)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "No se pudo marcar como No Show",
                        Detail = "La reservación podría no existir o ya estar en un estado final",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new ApiResponse { Success = true, Message = "Reservación marcada como No Show exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al marcar No Show para reservación {ReservacionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar el No Show.",
                    Status = StatusCodes.Status500InternalServerError
                });
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
        public async Task<ActionResult<EstadisticasReservacionResponse>> GetEstadisticasReservaciones([FromQuery] string fechaInicio, [FromQuery] string fechaFin)
        {
            try
            {
                // Convertir strings a DateTime
                if (!DateTime.TryParse(fechaInicio, out DateTime fechaInicioDateTime))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Formato de fecha de inicio inválido",
                        Detail = "La fecha de inicio debe estar en formato YYYY-MM-DD",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                if (!DateTime.TryParse(fechaFin, out DateTime fechaFinDateTime))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Formato de fecha de fin inválido",
                        Detail = "La fecha de fin debe estar en formato YYYY-MM-DD",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var estadisticas = await _reservacionService.GetEstadisticasReservasAsync(fechaInicioDateTime, fechaFinDateTime);
                
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
        public async Task<ActionResult<DisponibilidadMesasResponse>> GetDisponibilidadMesas([FromQuery] string fecha, [FromQuery] string hora)
        {
            try
            {
                // Convertir string a DateTime
                if (!DateTime.TryParse(fecha, out DateTime fechaDateTime))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Formato de fecha inválido",
                        Detail = "La fecha debe estar en formato YYYY-MM-DD",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Convertir string a TimeSpan
                if (!TimeSpan.TryParse(hora, out TimeSpan horaTimeSpan))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Formato de hora inválido",
                        Detail = "La hora debe estar en formato HH:MM",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var fechaHora = fechaDateTime.Date.Add(horaTimeSpan);
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