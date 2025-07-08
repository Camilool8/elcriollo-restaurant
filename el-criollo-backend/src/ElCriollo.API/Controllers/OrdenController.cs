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
    /// Controlador para la gestión de órdenes/comandas del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Gestión de órdenes, comandas y pedidos del restaurante")]
    public class OrdenController : ControllerBase
    {
        private readonly IOrdenService _ordenService;
        private readonly ILogger<OrdenController> _logger;

        public OrdenController(IOrdenService ordenService, ILogger<OrdenController> logger)
        {
            _ordenService = ordenService;
            _logger = logger;
        }

        // ============================================================================
        // CREACIÓN Y GESTIÓN DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Crear una nueva orden/comanda
        /// </summary>
        /// <param name="request">Datos de la nueva orden</param>
        /// <returns>Orden creada con número único</returns>
        /// <response code="201">Orden creada exitosamente</response>
        /// <response code="400">Datos inválidos o mesa no disponible</response>
        /// <response code="401">No autorizado</response>
        [HttpPost]
        [Authorize(Roles = "Administrador,Mesero,Recepción")]
        [SwaggerOperation(
            Summary = "Crear nueva orden",
            Description = "Crea una nueva orden/comanda para mesa o para llevar. Valida disponibilidad de mesa y productos",
            OperationId = "Orden.Crear",
            Tags = new[] { "Gestión de Órdenes" }
        )]
        [ProducesResponseType(typeof(OrdenResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OrdenResponse>> CrearOrden([FromBody] CreateOrdenRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("🍽️ Creando nueva orden por usuario {UsuarioId}", usuarioId);

                var orden = await _ordenService.CrearOrdenAsync(request, usuarioId);
                
                _logger.LogInformation("✅ Orden {NumeroOrden} creada exitosamente", orden.NumeroOrden);
                
                return CreatedAtAction(
                    nameof(GetOrdenById), 
                    new { id = orden.OrdenID }, 
                    orden
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al crear orden: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al crear orden",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al crear orden");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno",
                    Detail = "Ocurrió un error al procesar la orden",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // CONSULTA DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Obtener una orden específica por ID
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Datos completos de la orden</returns>
        /// <response code="200">Orden encontrada</response>
        /// <response code="404">Orden no encontrada</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtener orden por ID",
            Description = "Obtiene los datos completos de una orden específica incluyendo items y totales",
            OperationId = "Orden.GetById",
            Tags = new[] { "Consulta de Órdenes" }
        )]
        [ProducesResponseType(typeof(OrdenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenResponse>> GetOrdenById(int id)
        {
            try
            {
                var orden = await _ordenService.GetOrdenByIdAsync(id);
                
                if (orden == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Orden no encontrada",
                        Detail = $"No se encontró la orden con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(orden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener orden {OrdenId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener órdenes activas del día
        /// </summary>
        /// <returns>Lista de órdenes activas (no facturadas ni canceladas)</returns>
        /// <response code="200">Lista de órdenes activas</response>
        [HttpGet("activas")]
        [SwaggerOperation(
            Summary = "Obtener órdenes activas",
            Description = "Devuelve todas las órdenes activas del día (Pendiente, EnPreparacion, Lista, Entregada)",
            OperationId = "Orden.GetActivas",
            Tags = new[] { "Consulta de Órdenes" }
        )]
        [ProducesResponseType(typeof(IEnumerable<OrdenResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrdenResponse>>> GetOrdenesActivas()
        {
            try
            {
                _logger.LogInformation("📋 Consultando órdenes activas del día");
                var ordenes = await _ordenService.GetOrdenesActivasAsync();
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes activas");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener órdenes por mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Lista de órdenes de la mesa</returns>
        /// <response code="200">Lista de órdenes de la mesa</response>
        [HttpGet("mesa/{mesaId:int}")]
        [SwaggerOperation(
            Summary = "Obtener órdenes por mesa",
            Description = "Devuelve todas las órdenes activas de una mesa específica",
            OperationId = "Orden.GetByMesa",
            Tags = new[] { "Consulta de Órdenes" }
        )]
        [ProducesResponseType(typeof(IEnumerable<OrdenResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrdenResponse>>> GetOrdenesPorMesa(int mesaId)
        {
            try
            {
                _logger.LogInformation("🪑 Consultando órdenes de mesa {MesaId}", mesaId);
                var ordenes = await _ordenService.GetOrdenesPorMesaAsync(mesaId);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes de mesa {MesaId}", mesaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener órdenes por estado
        /// </summary>
        /// <param name="estado">Estado de las órdenes (Pendiente, EnPreparacion, Lista, etc.)</param>
        /// <returns>Lista de órdenes en el estado especificado</returns>
        /// <response code="200">Lista de órdenes</response>
        /// <response code="400">Estado inválido</response>
        [HttpGet("estado/{estado}")]
        [SwaggerOperation(
            Summary = "Obtener órdenes por estado",
            Description = "Filtra órdenes por estado: Pendiente, EnPreparacion, Lista, Entregada, Facturada, Cancelada",
            OperationId = "Orden.GetByEstado",
            Tags = new[] { "Consulta de Órdenes" }
        )]
        [ProducesResponseType(typeof(IEnumerable<OrdenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrdenResponse>>> GetOrdenesPorEstado(string estado)
        {
            try
            {
                var estadosValidos = new[] { "Pendiente", "EnPreparacion", "Lista", "Entregada", "Facturada", "Cancelada" };
                if (!estadosValidos.Contains(estado))
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Estado inválido",
                        Detail = $"Los estados válidos son: {string.Join(", ", estadosValidos)}",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var ordenes = await _ordenService.GetOrdenesPorEstadoAsync(estado);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener órdenes por estado {Estado}", estado);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // ACTUALIZACIÓN DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Actualizar estado de una orden
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="request">Nuevo estado y observaciones</param>
        /// <returns>Orden actualizada</returns>
        /// <response code="200">Orden actualizada exitosamente</response>
        /// <response code="400">Estado inválido o transición no permitida</response>
        /// <response code="404">Orden no encontrada</response>
        [HttpPut("{id:int}/estado")]
        [Authorize(Roles = "Administrador,Mesero,Cocina")]
        [SwaggerOperation(
            Summary = "Actualizar estado de orden",
            Description = "Cambia el estado de una orden siguiendo el flujo: Pendiente → EnPreparacion → Lista → Entregada",
            OperationId = "Orden.ActualizarEstado",
            Tags = new[] { "Gestión de Órdenes" }
        )]
        [ProducesResponseType(typeof(OrdenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenResponse>> ActualizarEstadoOrden(int id, [FromBody] ActualizarEstadoOrdenRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("📝 Actualizando estado de orden {OrdenId} a {NuevoEstado}", id, request.NuevoEstado);

                // Usar CambiarEstadoOrdenAsync en lugar de ActualizarEstadoOrdenAsync
                var resultado = await _ordenService.CambiarEstadoOrdenAsync(id, request.NuevoEstado, usuarioId);
                
                OrdenResponse? ordenActualizada = null;
                if (resultado)
                {
                    ordenActualizada = await _ordenService.GetOrdenByIdAsync(id);
                }

                if (ordenActualizada == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Orden no encontrada",
                        Detail = $"No se encontró la orden con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Estado de orden {OrdenId} actualizado exitosamente", id);
                return Ok(ordenActualizada);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al actualizar estado: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al actualizar estado",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar estado de orden {OrdenId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Agregar items a una orden existente
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="request">Items a agregar</param>
        /// <returns>Orden actualizada con nuevos items</returns>
        /// <response code="200">Items agregados exitosamente</response>
        /// <response code="400">Orden no puede ser modificada</response>
        /// <response code="404">Orden no encontrada</response>
        [HttpPost("{id:int}/items")]
        [Authorize(Roles = "Administrador,Mesero")]
        [SwaggerOperation(
            Summary = "Agregar items a orden",
            Description = "Agrega nuevos productos o combos a una orden existente. Solo permitido en estados Pendiente o EnPreparacion",
            OperationId = "Orden.AgregarItems",
            Tags = new[] { "Gestión de Órdenes" }
        )]
        [ProducesResponseType(typeof(OrdenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenResponse>> AgregarItemsOrden(int id, [FromBody] AgregarItemsOrdenRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("➕ Agregando items a orden {OrdenId}", id);

                var ordenActualizada = await _ordenService.AgregarItemsOrdenAsync(id, request.Items, usuarioId);

                if (ordenActualizada == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Orden no encontrada",
                        Detail = $"No se encontró la orden con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Items agregados exitosamente a orden {OrdenId}", id);
                return Ok(ordenActualizada);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al agregar items: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al agregar items",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al agregar items a orden {OrdenId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Cancelar una orden
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <param name="request">Motivo de cancelación</param>
        /// <returns>Confirmación de cancelación</returns>
        /// <response code="200">Orden cancelada exitosamente</response>
        /// <response code="400">Orden no puede ser cancelada</response>
        /// <response code="404">Orden no encontrada</response>
        [HttpPost("{id:int}/cancelar")]
        [Authorize(Roles = "Administrador,Mesero")]
        [SwaggerOperation(
            Summary = "Cancelar orden",
            Description = "Cancela una orden activa. No se pueden cancelar órdenes ya facturadas",
            OperationId = "Orden.Cancelar",
            Tags = new[] { "Gestión de Órdenes" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> CancelarOrden(int id, [FromBody] CancelarOrdenRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("❌ Cancelando orden {OrdenId}. Motivo: {Motivo}", id, request.Motivo);

                var resultado = await _ordenService.CancelarOrdenAsync(id, request.Motivo, usuarioId);

                if (!resultado)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Orden no encontrada",
                        Detail = $"No se encontró la orden con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Orden {OrdenId} cancelada exitosamente", id);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Orden cancelada exitosamente",
                    Data = new { OrdenId = id, Estado = "Cancelada", Motivo = request.Motivo }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al cancelar orden: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al cancelar orden",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cancelar orden {OrdenId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // VISTAS Y REPORTES DE COCINA
        // ============================================================================

        /// <summary>
        /// Obtener vista de cocina
        /// </summary>
        /// <returns>Órdenes pendientes de preparación organizadas por prioridad</returns>
        /// <response code="200">Vista de cocina con órdenes pendientes</response>
        [HttpGet("cocina")]
        [Authorize(Roles = "Administrador,Cocina,Mesero")]
        [SwaggerOperation(
            Summary = "Vista de cocina",
            Description = "Muestra todas las órdenes pendientes de preparación organizadas por tiempo y prioridad",
            OperationId = "Orden.VistaCocina",
            Tags = new[] { "Vista Cocina" }
        )]
        [ProducesResponseType(typeof(IEnumerable<VistaCocinaViewModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VistaCocinaViewModel>>> GetVistaCocina()
        {
            try
            {
                _logger.LogInformation("👨‍🍳 Consultando vista de cocina");
                // Usar GetColaCocinaBasicaAsync en lugar de GetVistaCocinaAsync
                var ordenesBasicas = await _ordenService.GetColaCocinaBasicaAsync();
                
                // Convertir a formato VistaCocinaViewModel
                var vistacocina = ordenesBasicas.Select(o => new VistaCocinaViewModel
                {
                    OrdenID = o.OrdenID,
                    NumeroOrden = o.NumeroOrden,
                    NumeroMesa = o.NumeroMesa,
                    TiempoEspera = o.TiempoEspera,
                    CantidadItems = o.CantidadItems,
                    Estado = o.Estado,
                    ProductosResumen = o.ProductosResumen,
                    EsUrgente = o.EsUrgente,
                    NotasEspeciales = o.NotasEspeciales
                });
                return Ok(vistacocina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener vista de cocina");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Marcar orden como lista
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <returns>Orden actualizada</returns>
        /// <response code="200">Orden marcada como lista</response>
        /// <response code="400">Orden no está en preparación</response>
        /// <response code="404">Orden no encontrada</response>
        [HttpPost("{id:int}/lista")]
        [Authorize(Roles = "Administrador,Cocina")]
        [SwaggerOperation(
            Summary = "Marcar orden como lista",
            Description = "Marca una orden en preparación como lista para servir",
            OperationId = "Orden.MarcarLista",
            Tags = new[] { "Vista Cocina" }
        )]
        [ProducesResponseType(typeof(OrdenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenResponse>> MarcarOrdenLista(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("✅ Marcando orden {OrdenId} como lista", id);

                // Usar CambiarEstadoOrdenAsync
                var resultado = await _ordenService.CambiarEstadoOrdenAsync(id, "Lista", usuarioId);
                
                OrdenResponse? ordenActualizada = null;
                if (resultado)
                {
                    ordenActualizada = await _ordenService.GetOrdenByIdAsync(id);
                }

                if (ordenActualizada == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Orden no encontrada",
                        Detail = $"No se encontró la orden con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("✅ Orden {OrdenId} marcada como lista", id);
                return Ok(ordenActualizada);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error al marcar orden como lista: {Mensaje}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al actualizar orden",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al marcar orden {OrdenId} como lista", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REQUESTS ESPECÍFICOS DEL CONTROLADOR
        // ============================================================================

        /// <summary>
        /// Request para actualizar estado de orden
        /// </summary>
        public class ActualizarEstadoOrdenRequest
        {
            /// <summary>
            /// Nuevo estado de la orden
            /// </summary>
            public string NuevoEstado { get; set; } = string.Empty;

            /// <summary>
            /// Observaciones sobre el cambio (opcional)
            /// </summary>
            public string? Observaciones { get; set; }
        }

        /// <summary>
        /// Request para agregar items a una orden
        /// </summary>
        public class AgregarItemsOrdenRequest
        {
            /// <summary>
            /// Lista de items a agregar
            /// </summary>
            public List<ItemOrdenRequest> Items { get; set; } = new();
        }

        /// <summary>
        /// Request para cancelar orden
        /// </summary>
        public class CancelarOrdenRequest
        {
            /// <summary>
            /// Motivo de la cancelación
            /// </summary>
            public string Motivo { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Vista modelo para cocina
        /// </summary>
        public class VistaCocinaViewModel
        {
            public int OrdenID { get; set; }
            public string NumeroOrden { get; set; } = string.Empty;
            public string? NumeroMesa { get; set; }
            public string TiempoEspera { get; set; } = string.Empty;
            public int CantidadItems { get; set; }
            public string Estado { get; set; } = string.Empty;
            public List<string> ProductosResumen { get; set; } = new();
            public bool EsUrgente { get; set; }
            public string? NotasEspeciales { get; set; }
        }
    }
}