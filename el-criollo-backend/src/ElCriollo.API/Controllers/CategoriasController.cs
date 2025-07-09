using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de categorías de productos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Gestión de categorías de productos del menú")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(ICategoriaService categoriaService, ILogger<CategoriasController> logger)
        {
            _categoriaService = categoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las categorías
        /// </summary>
        /// <returns>Lista de categorías</returns>
        /// <response code="200">Lista de categorías</response>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener categorías",
            Description = "Devuelve todas las categorías disponibles con información de productos",
            OperationId = "Categorias.GetCategorias",
            Tags = new[] { "Categorías" }
        )]
        [ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoriaResponse>>> GetCategorias()
        {
            try
            {
                var categorias = await _categoriaService.GetCategoriasAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener las categorías",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener una categoría específica por ID
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Datos de la categoría</returns>
        /// <response code="200">Categoría encontrada</response>
        /// <response code="404">Categoría no encontrada</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener categoría por ID",
            Description = "Devuelve la información detallada de una categoría específica",
            OperationId = "Categorias.GetCategoriaById",
            Tags = new[] { "Categorías" }
        )]
        [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoriaResponse>> GetCategoriaById(int id)
        {
            try
            {
                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
                
                if (categoria == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Categoría no encontrada",
                        Detail = $"No se encontró una categoría con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría con ID: {CategoriaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener la categoría",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Crear una nueva categoría
        /// </summary>
        /// <param name="crearCategoriaRequest">Datos de la nueva categoría</param>
        /// <returns>Categoría creada</returns>
        /// <response code="201">Categoría creada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Crear nueva categoría",
            Description = "Crea una nueva categoría de productos (solo administradores)",
            OperationId = "Categorias.CrearCategoria",
            Tags = new[] { "Gestión de Categorías" }
        )]
        [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CategoriaResponse>> CrearCategoria([FromBody] CrearCategoriaRequest crearCategoriaRequest)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Validar la categoría antes de crearla
                var validacion = await _categoriaService.ValidarCategoriaAsync(crearCategoriaRequest);
                if (!validacion.EsValido)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Error de validación",
                        Detail = string.Join("; ", validacion.Errores),
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var categoria = await _categoriaService.CrearCategoriaAsync(crearCategoriaRequest, usuarioId);

                _logger.LogInformation("Categoría creada exitosamente: {CategoriaNombre}", categoria.Nombre);

                return CreatedAtAction(
                    nameof(GetCategoriaById), 
                    new { id = categoria.CategoriaID }, 
                    categoria
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al crear categoría: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al crear la categoría",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Actualizar una categoría existente
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <param name="actualizarCategoriaRequest">Nuevos datos de la categoría</param>
        /// <returns>Categoría actualizada</returns>
        /// <response code="200">Categoría actualizada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Categoría no encontrada</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Actualizar categoría",
            Description = "Actualiza la información de una categoría existente (solo administradores)",
            OperationId = "Categorias.ActualizarCategoria",
            Tags = new[] { "Gestión de Categorías" }
        )]
        [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CategoriaResponse>> ActualizarCategoria(
            int id, 
            [FromBody] ActualizarCategoriaRequest actualizarCategoriaRequest)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Validar la actualización
                var validacion = await _categoriaService.ValidarActualizacionCategoriaAsync(id, actualizarCategoriaRequest);
                if (!validacion.EsValido)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Error de validación",
                        Detail = string.Join("; ", validacion.Errores),
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var categoria = await _categoriaService.ActualizarCategoriaAsync(id, actualizarCategoriaRequest, usuarioId);
                
                if (categoria == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Categoría no encontrada",
                        Detail = $"No se encontró una categoría con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Categoría actualizada exitosamente: ID {CategoriaId}", id);
                return Ok(categoria);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al actualizar categoría: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría con ID: {CategoriaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al actualizar la categoría",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Eliminar una categoría
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="200">Categoría eliminada exitosamente</response>
        /// <response code="400">No se puede eliminar (tiene productos)</response>
        /// <response code="404">Categoría no encontrada</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Eliminar categoría",
            Description = "Elimina una categoría (solo si no tiene productos asignados)",
            OperationId = "Categorias.EliminarCategoria",
            Tags = new[] { "Gestión de Categorías" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<MessageResponse>> EliminarCategoria(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var eliminado = await _categoriaService.EliminarCategoriaAsync(id, usuarioId);
                
                if (!eliminado)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Categoría no encontrada",
                        Detail = $"No se encontró una categoría con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Categoría eliminada exitosamente: ID {CategoriaId}", id);
                return Ok(new MessageResponse
                {
                    Message = "Categoría eliminada exitosamente",
                    Success = true
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al eliminar categoría: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "No se puede eliminar",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría con ID: {CategoriaId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al eliminar la categoría",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
} 