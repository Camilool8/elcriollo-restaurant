using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Services;
using ElCriollo.API.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de mesas del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Gestión de mesas, ocupación, disponibilidad y estadísticas")]
    public class MesasController : ControllerBase
    {
        private readonly IMesaService _mesaService;
        private readonly ILogger<MesasController> _logger;

        public MesasController(IMesaService mesaService, ILogger<MesasController> logger)
        {
            _mesaService = mesaService;
            _logger = logger;
        }

        // ============================================================================
        // ESTADO Y CONSULTA DE MESAS
        // ============================================================================

        /// <summary>
        /// Obtener el estado actual de todas las mesas
        /// </summary>
        /// <returns>Lista de todas las mesas con su estado actual</returns>
        /// <response code="200">Lista de mesas</response>
        /// <response code="401">No autorizado</response>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener todas las mesas",
            Description = "Devuelve el estado actual de todas las mesas del restaurante con información de ocupación",
            OperationId = "Mesas.GetTodasLasMesas",
            Tags = new[] { "Estado de Mesas" }
        )]
        [ProducesResponseType(typeof(IEnumerable<MesaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<MesaResponse>>> GetTodasLasMesas()
        {
            try
            {
                _logger.LogInformation("Consultando estado de todas las mesas");
                
                var mesas = await _mesaService.GetEstadoTodasLasMesasAsync();
                return Ok(mesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de mesas");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener el estado de las mesas",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener una mesa específica por ID
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <returns>Datos detallados de la mesa</returns>
        /// <response code="200">Mesa encontrada</response>
        /// <response code="404">Mesa no encontrada</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener mesa por ID",
            Description = "Devuelve información detallada de una mesa específica incluyendo orden actual si existe",
            OperationId = "Mesas.GetMesaById",
            Tags = new[] { "Estado de Mesas" }
        )]
        [ProducesResponseType(typeof(MesaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MesaResponse>> GetMesaById(int id)
        {
            try
            {
                var mesa = await _mesaService.GetMesaByIdAsync(id);
                
                if (mesa == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(mesa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesa con ID: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener mesas por estado específico
        /// </summary>
        /// <param name="estado">Estado a filtrar (Libre, Ocupada, Reservada, Mantenimiento)</param>
        /// <returns>Lista de mesas en el estado especificado</returns>
        /// <response code="200">Mesas filtradas por estado</response>
        /// <response code="400">Estado inválido</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("estado/{estado}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Filtrar mesas por estado",
            Description = "Obtiene todas las mesas que se encuentran en un estado específico",
            OperationId = "Mesas.GetMesasPorEstado",
            Tags = new[] { "Estado de Mesas" }
        )]
        [ProducesResponseType(typeof(IEnumerable<MesaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<MesaResponse>>> GetMesasPorEstado(string estado)
        {
            try
            {
                var estadosValidos = new[] { "Libre", "Ocupada", "Reservada", "Mantenimiento" };
                if (!estadosValidos.Contains(estado, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Estado inválido",
                        Detail = $"El estado debe ser uno de: {string.Join(", ", estadosValidos)}",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var mesas = await _mesaService.GetMesasPorEstadoAsync(estado);
                return Ok(mesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas por estado: {Estado}", estado);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener las mesas",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Buscar mesas disponibles
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas (opcional)</param>
        /// <returns>Lista de mesas disponibles</returns>
        /// <response code="200">Mesas disponibles</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("disponibles")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Buscar mesas disponibles",
            Description = "Busca mesas libres, opcionalmente filtradas por capacidad",
            OperationId = "Mesas.BuscarMesasDisponibles",
            Tags = new[] { "Disponibilidad" }
        )]
        [ProducesResponseType(typeof(IEnumerable<MesaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<MesaResponse>>> BuscarMesasDisponibles([FromQuery] int? cantidadPersonas = null)
        {
            try
            {
                var mesas = await _mesaService.BuscarMesasDisponiblesAsync(cantidadPersonas);
                return Ok(mesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar mesas disponibles");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al buscar mesas disponibles",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Verificar disponibilidad de una mesa específica
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <returns>Estado de disponibilidad de la mesa</returns>
        /// <response code="200">Disponibilidad verificada</response>
        /// <response code="404">Mesa no encontrada</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("{id}/disponibilidad")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Verificar disponibilidad de mesa",
            Description = "Verifica si una mesa específica está disponible para ser ocupada",
            OperationId = "Mesas.VerificarDisponibilidad",
            Tags = new[] { "Disponibilidad" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> VerificarDisponibilidadMesa(int id)
        {
            try
            {
                var mesa = await _mesaService.GetMesaByIdAsync(id);
                if (mesa == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var disponible = await _mesaService.VerificarDisponibilidadMesaAsync(id);
                
                return Ok(new
                {
                    mesaId = id,
                    disponible = disponible,
                    estado = mesa.Estado,
                    numeroMesa = mesa.NumeroMesa,
                    capacidad = mesa.Capacidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad de mesa: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al verificar la disponibilidad de la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // GESTIÓN DE ESTADOS DE MESA
        // ============================================================================

        /// <summary>
        /// Cambiar el estado de una mesa
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <param name="cambioEstadoRequest">Nuevo estado y motivo</param>
        /// <returns>Confirmación del cambio</returns>
        /// <response code="200">Estado cambiado exitosamente</response>
        /// <response code="400">Cambio de estado inválido</response>
        /// <response code="404">Mesa no encontrada</response>
        /// <response code="401">No autorizado</response>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Administrador,Recepcion,Mesero")]
        [SwaggerOperation(
            Summary = "Cambiar estado de mesa",
            Description = "Cambia el estado de una mesa con validación de transiciones permitidas",
            OperationId = "Mesas.CambiarEstadoMesa",
            Tags = new[] { "Gestión de Estados" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MessageResponse>> CambiarEstadoMesa(int id, [FromBody] CambioEstadoMesaRequest cambioEstadoRequest)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Validar el cambio de estado
                var validacion = await _mesaService.ValidarCambioEstadoAsync(id, cambioEstadoRequest.NuevoEstado);
                if (!validacion.EsValida)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Cambio de estado inválido",
                        Detail = string.Join("; ", validacion.Errores),
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var success = await _mesaService.CambiarEstadoMesaAsync(
                    id, 
                    cambioEstadoRequest.NuevoEstado, 
                    usuarioId, 
                    cambioEstadoRequest.Motivo
                );

                if (!success)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Estado de mesa {MesaId} cambiado a: {NuevoEstado}", id, cambioEstadoRequest.NuevoEstado);
                
                return Ok(new MessageResponse
                {
                    Message = $"Mesa {id} actualizada a estado {cambioEstadoRequest.NuevoEstado}",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de mesa: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al cambiar el estado de la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Ocupar una mesa para orden inmediata
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <returns>Confirmación de ocupación</returns>
        /// <response code="200">Mesa ocupada exitosamente</response>
        /// <response code="400">Mesa no disponible</response>
        /// <response code="404">Mesa no encontrada</response>
        /// <response code="401">No autorizado</response>
        [HttpPost("{id}/ocupar")]
        [Authorize(Roles = "Administrador,Mesero")]
        [SwaggerOperation(
            Summary = "Ocupar mesa",
            Description = "Marca una mesa como ocupada para atender clientes inmediatamente",
            OperationId = "Mesas.OcuparMesa",
            Tags = new[] { "Gestión de Estados" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MessageResponse>> OcuparMesa(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var disponible = await _mesaService.VerificarDisponibilidadMesaAsync(id);
                if (!disponible)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Mesa no disponible",
                        Detail = "La mesa no está disponible para ser ocupada",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var success = await _mesaService.OcuparMesaAsync(id, usuarioId);

                if (!success)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Mesa {MesaId} ocupada por usuario {UsuarioId}", id, usuarioId);
                
                return Ok(new MessageResponse
                {
                    Message = $"Mesa {id} ocupada exitosamente",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ocupar mesa: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al ocupar la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Liberar una mesa
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <returns>Confirmación de liberación</returns>
        /// <response code="200">Mesa liberada exitosamente</response>
        /// <response code="400">Mesa no puede ser liberada</response>
        /// <response code="404">Mesa no encontrada</response>
        /// <response code="401">No autorizado</response>
        [HttpPost("{id}/liberar")]
        [Authorize(Roles = "Administrador,Cajero")]
        [SwaggerOperation(
            Summary = "Liberar mesa",
            Description = "Libera una mesa después de que los clientes han terminado y pagado",
            OperationId = "Mesas.LiberarMesa",
            Tags = new[] { "Gestión de Estados" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MessageResponse>> LiberarMesa(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var puedeLiberarse = await _mesaService.PuedeLiberarseMesaAsync(id);
                if (!puedeLiberarse)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Mesa no puede ser liberada",
                        Detail = "La mesa tiene órdenes pendientes o no está ocupada",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var success = await _mesaService.LiberarMesaAsync(id, usuarioId);

                if (!success)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Mesa {MesaId} liberada por usuario {UsuarioId}", id, usuarioId);
                
                return Ok(new MessageResponse
                {
                    Message = $"Mesa {id} liberada exitosamente",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar mesa: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al liberar la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Marcar mesa para mantenimiento
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <param name="mantenimientoRequest">Motivo del mantenimiento</param>
        /// <returns>Confirmación del mantenimiento</returns>
        /// <response code="200">Mesa en mantenimiento</response>
        /// <response code="404">Mesa no encontrada</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpPost("{id}/mantenimiento")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Mesa en mantenimiento",
            Description = "Marca una mesa como en mantenimiento con el motivo especificado",
            OperationId = "Mesas.MarcarMantenimiento",
            Tags = new[] { "Gestión de Estados" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<MessageResponse>> MarcarMantenimiento(int id, [FromBody] MantenimientoMesaRequest mantenimientoRequest)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var success = await _mesaService.MarcarMesaMantenimientoAsync(
                    id, 
                    mantenimientoRequest.Motivo, 
                    usuarioId
                );

                if (!success)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Mesa {MesaId} marcada para mantenimiento: {Motivo}", id, mantenimientoRequest.Motivo);
                
                return Ok(new MessageResponse
                {
                    Message = $"Mesa {id} marcada para mantenimiento",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar mesa para mantenimiento: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al marcar la mesa para mantenimiento",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtener estadísticas básicas de ocupación
        /// </summary>
        /// <returns>Estadísticas de mesas</returns>
        /// <response code="200">Estadísticas obtenidas</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Administrador,Recepcion,Mesero,Cajero")]
        [SwaggerOperation(
            Summary = "Estadísticas de mesas",
            Description = "Obtiene estadísticas básicas de ocupación y estado de las mesas",
            OperationId = "Mesas.GetEstadisticas",
            Tags = new[] { "Estadísticas" }
        )]
        [ProducesResponseType(typeof(EstadisticasMesasBasicasViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EstadisticasMesasBasicasViewModel>> GetEstadisticas()
        {
            try
            {
                var estadisticas = await _mesaService.GetEstadisticasBasicasAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de mesas");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener las estadísticas",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener resumen de ocupación del día
        /// </summary>
        /// <param name="fecha">Fecha a consultar (por defecto hoy)</param>
        /// <returns>Resumen del día</returns>
        /// <response code="200">Resumen obtenido</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("resumen-dia")]
        [Authorize(Roles = "Administrador,Recepcion")]
        [SwaggerOperation(
            Summary = "Resumen del día",
            Description = "Obtiene el resumen de ocupación de mesas del día especificado",
            OperationId = "Mesas.GetResumenDia",
            Tags = new[] { "Estadísticas" }
        )]
        [ProducesResponseType(typeof(ResumenOcupacionDiaViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResumenOcupacionDiaViewModel>> GetResumenDia([FromQuery] DateTime? fecha = null)
        {
            try
            {
                var resumen = await _mesaService.GetResumenOcupacionDiaAsync(fecha);
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen del día");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener el resumen",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener mesas que requieren atención
        /// </summary>
        /// <param name="tiempoLimiteMinutos">Tiempo límite en minutos (default 180)</param>
        /// <returns>Lista de mesas con tiempo excedido</returns>
        /// <response code="200">Mesas que requieren atención</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("requieren-atencion")]
        [Authorize(Roles = "Administrador,Mesero")]
        [SwaggerOperation(
            Summary = "Mesas que requieren atención",
            Description = "Obtiene las mesas ocupadas por más tiempo del límite establecido",
            OperationId = "Mesas.GetMesasRequierenAtencion",
            Tags = new[] { "Monitoreo" }
        )]
        [ProducesResponseType(typeof(IEnumerable<MesaAtencionBasicaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<MesaAtencionBasicaResponse>>> GetMesasRequierenAtencion([FromQuery] int tiempoLimiteMinutos = 180)
        {
            try
            {
                var mesas = await _mesaService.GetMesasRequierenAtencionAsync(tiempoLimiteMinutos);
                return Ok(mesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas que requieren atención");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener las mesas",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // OPERACIONES ADMINISTRATIVAS
        // ============================================================================

        /// <summary>
        /// Reiniciar todas las mesas a estado Libre
        /// </summary>
        /// <returns>Confirmación del reinicio</returns>
        /// <response code="200">Mesas reiniciadas</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpPost("reiniciar-todas")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Reiniciar todas las mesas",
            Description = "Reinicia todas las mesas a estado Libre (útil al inicio del día)",
            OperationId = "Mesas.ReiniciarTodas",
            Tags = new[] { "Administración" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<MessageResponse>> ReiniciarTodasLasMesas()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var success = await _mesaService.ReiniciarTodasLasMesasAsync(usuarioId);

                if (success)
                {
                    _logger.LogWarning("Todas las mesas reiniciadas por usuario {UsuarioId}", usuarioId);
                    
                    return Ok(new MessageResponse
                    {
                        Message = "Todas las mesas han sido reiniciadas a estado Libre",
                        Success = true
                    });
                }

                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error al reiniciar mesas",
                    Detail = "No se pudieron reiniciar las mesas",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reiniciar todas las mesas");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al reiniciar las mesas",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Verificar estado detallado de una mesa
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <returns>Estado detallado de la mesa</returns>
        [HttpGet("{id}/estado-detallado")]
        [Authorize(Roles = "Administrador,Cajero")]
        [SwaggerOperation(
            Summary = "Verificar estado detallado de mesa",
            Description = "Obtiene información detallada sobre el estado de una mesa y sus órdenes",
            OperationId = "Mesas.GetEstadoDetallado",
            Tags = new[] { "Gestión de Estados" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> GetEstadoDetallado(int id)
        {
            try
            {
                var mesa = await _mesaService.GetMesaByIdAsync(id);
                if (mesa == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var puedeLiberarse = await _mesaService.PuedeLiberarseMesaAsync(id);
                
                var resultado = new
                {
                    Mesa = mesa,
                    PuedeLiberarse = puedeLiberarse,
                    FechaVerificacion = DateTime.Now
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado detallado de mesa: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener el estado detallado de la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener información detallada de órdenes de una mesa (para debugging)
        /// </summary>
        /// <param name="id">ID de la mesa</param>
        /// <returns>Información detallada de todas las órdenes de la mesa</returns>
        [HttpGet("{id}/ordenes-detalladas")]
        [Authorize(Roles = "Administrador,Cajero")]
        [SwaggerOperation(
            Summary = "Obtener órdenes detalladas de mesa",
            Description = "Obtiene información detallada de todas las órdenes de una mesa incluyendo facturas",
            OperationId = "Mesas.GetOrdenesDetalladas",
            Tags = new[] { "Debugging" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> GetOrdenesDetalladas(int id)
        {
            try
            {
                var mesa = await _mesaService.GetMesaByIdAsync(id);
                if (mesa == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Mesa no encontrada",
                        Detail = $"No se encontró una mesa con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Obtener órdenes de la mesa usando el repositorio directamente
                var ordenRepository = HttpContext.RequestServices.GetRequiredService<IOrdenRepository>();
                var facturaRepository = HttpContext.RequestServices.GetRequiredService<IFacturaRepository>();
                
                var ordenes = await ordenRepository.GetByMesaAsync(id);
                var ordenesDetalladas = new List<object>();

                foreach (var orden in ordenes)
                {
                    var facturas = await facturaRepository.GetByOrdenAsync(orden.OrdenID);
                    var facturasInfo = facturas.Select(f => new
                    {
                        f.FacturaID,
                        f.NumeroFactura,
                        f.Estado,
                        f.Total,
                        f.FechaFactura,
                        f.FechaPago
                    }).ToList();

                    ordenesDetalladas.Add(new
                    {
                        OrdenID = orden.OrdenID,
                        NumeroOrden = orden.NumeroOrden,
                        Estado = orden.Estado,
                        FechaCreacion = orden.FechaCreacion,
                        FechaActualizacion = orden.FechaActualizacion,
                        Facturas = facturasInfo
                    });
                }

                var resultado = new
                {
                    Mesa = new
                    {
                        mesa.MesaID,
                        mesa.NumeroMesa,
                        mesa.Estado,
                        mesa.Capacidad
                    },
                    Ordenes = ordenesDetalladas,
                    TotalOrdenes = ordenes.Count(),
                    FechaVerificacion = DateTime.Now
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes detalladas de mesa: {MesaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener las órdenes detalladas de la mesa",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }

    // ============================================================================
    // DTOs ADICIONALES
    // ============================================================================

    /// <summary>
    /// Request para cambiar estado de mesa
    /// </summary>
    public class CambioEstadoMesaRequest
    {
        /// <summary>
        /// Nuevo estado (Libre, Ocupada, Reservada, Mantenimiento)
        /// </summary>
        public string NuevoEstado { get; set; } = string.Empty;

        /// <summary>
        /// Motivo del cambio (opcional)
        /// </summary>
        public string? Motivo { get; set; }
    }

    /// <summary>
    /// Request para mantenimiento de mesa
    /// </summary>
    public class MantenimientoMesaRequest
    {
        /// <summary>
        /// Motivo del mantenimiento
        /// </summary>
        public string Motivo { get; set; } = string.Empty;
    }

} 