using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión del menú y productos del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Gestión del menú de comida dominicana, productos, combos y categorías")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        // ============================================================================
        // MENÚ DIGITAL Y CONSULTAS GENERALES
        // ============================================================================

        /// <summary>
        /// Obtener el menú digital completo del restaurante
        /// </summary>
        /// <param name="incluirNoDisponibles">Incluir productos no disponibles</param>
        /// <returns>Menú digital organizado por categorías</returns>
        /// <response code="200">Menú digital completo</response>
        [HttpGet("menu-digital")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener menú digital completo",
            Description = "Devuelve el menú completo del restaurante organizado por categorías (Platos Principales, Bebidas, Postres, etc.)",
            OperationId = "Productos.GetMenuDigital",
            Tags = new[] { "Menú Digital" }
        )]
        [ProducesResponseType(typeof(MenuDigitalViewModel), StatusCodes.Status200OK)]
        public async Task<ActionResult<MenuDigitalViewModel>> GetMenuDigital([FromQuery] bool incluirNoDisponibles = false)
        {
            try
            {
                _logger.LogInformation("Consultando menú digital, incluir no disponibles: {IncluirNoDisponibles}", incluirNoDisponibles);
                
                var menu = await _productoService.GetMenuDigitalAsync(incluirNoDisponibles);
                return Ok(menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener menú digital");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener el menú digital",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener todos los productos disponibles
        /// </summary>
        /// <returns>Lista de productos disponibles</returns>
        /// <response code="200">Lista de productos</response>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Listar productos disponibles",
            Description = "Devuelve todos los productos disponibles en el menú",
            OperationId = "Productos.GetProductosDisponibles",
            Tags = new[] { "Productos" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ProductoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductoResponse>>> GetProductosDisponibles()
        {
            try
            {
                var productos = await _productoService.GetProductosDisponiblesAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos disponibles");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener los productos",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener un producto específico por ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Datos del producto</returns>
        /// <response code="200">Producto encontrado</response>
        /// <response code="404">Producto no encontrado</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener producto por ID",
            Description = "Devuelve la información detallada de un producto específico",
            OperationId = "Productos.GetProductoById",
            Tags = new[] { "Productos" }
        )]
        [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductoResponse>> GetProductoById(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoByIdAsync(id);
                
                if (producto == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Producto no encontrado",
                        Detail = $"No se encontró un producto con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto con ID: {ProductoId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener el producto",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener productos por categoría
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <returns>Lista de productos de la categoría</returns>
        /// <response code="200">Productos de la categoría</response>
        [HttpGet("categoria/{categoriaId}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener productos por categoría",
            Description = "Devuelve todos los productos de una categoría específica",
            OperationId = "Productos.GetProductosPorCategoria",
            Tags = new[] { "Productos" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ProductoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductoResponse>>> GetProductosPorCategoria(int categoriaId)
        {
            try
            {
                var productos = await _productoService.GetProductosPorCategoriaAsync(categoriaId);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos de categoría: {CategoriaId}", categoriaId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener los productos",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Buscar productos por término
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <returns>Lista de productos que coinciden</returns>
        /// <response code="200">Productos encontrados</response>
        [HttpGet("buscar")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Buscar productos",
            Description = "Busca productos por nombre o descripción",
            OperationId = "Productos.BuscarProductos",
            Tags = new[] { "Productos" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ProductoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductoResponse>>> BuscarProductos([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Término de búsqueda requerido",
                        Detail = "Debe proporcionar un término de búsqueda",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var productos = await _productoService.BuscarProductosAsync(termino);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos con término: {Termino}", termino);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al buscar productos",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener todas las categorías disponibles
        /// </summary>
        /// <returns>Lista de categorías con información de productos</returns>
        /// <response code="200">Lista de categorías</response>
        [HttpGet("categorias")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener categorías",
            Description = "Devuelve todas las categorías disponibles con información de productos",
            OperationId = "Productos.GetCategorias",
            Tags = new[] { "Categorías" }
        )]
        [ProducesResponseType(typeof(IEnumerable<CategoriaBasicaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoriaBasicaResponse>>> GetCategorias()
        {
            try
            {
                var categorias = await _productoService.GetCategoriasAsync();
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

        // ============================================================================
        // GESTIÓN DE PRODUCTOS (ADMIN/MESERO)
        // ============================================================================

        /// <summary>
        /// Crear un nuevo producto
        /// </summary>
        /// <param name="crearProductoRequest">Datos del nuevo producto</param>
        /// <returns>Producto creado</returns>
        /// <response code="201">Producto creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Crear nuevo producto",
            Description = "Crea un nuevo producto en el menú (solo administradores)",
            OperationId = "Productos.CrearProducto",
            Tags = new[] { "Gestión de Productos" }
        )]
        [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProductoResponse>> CrearProducto([FromBody] CrearProductoRequest crearProductoRequest)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Validar el producto antes de crearlo
                var validacion = await _productoService.ValidarProductoAsync(crearProductoRequest);
                if (!validacion.EsValido)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Error de validación",
                        Detail = string.Join("; ", validacion.Errores),
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var producto = await _productoService.CrearProductoAsync(crearProductoRequest, usuarioId);

                _logger.LogInformation("Producto creado exitosamente: {ProductoNombre}", producto.Nombre);

                return CreatedAtAction(
                    nameof(GetProductoById), 
                    new { id = producto.ProductoID }, 
                    producto
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al crear producto: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al crear el producto",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Actualizar un producto existente
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="actualizarProductoRequest">Nuevos datos del producto</param>
        /// <returns>Producto actualizado</returns>
        /// <response code="200">Producto actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Actualizar producto",
            Description = "Actualiza la información de un producto existente (solo administradores)",
            OperationId = "Productos.ActualizarProducto",
            Tags = new[] { "Gestión de Productos" }
        )]
        [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProductoResponse>> ActualizarProducto(
            int id, 
            [FromBody] ActualizarProductoRequest actualizarProductoRequest)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var producto = await _productoService.ActualizarProductoAsync(id, actualizarProductoRequest, usuarioId);
                
                if (producto == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Producto no encontrado",
                        Detail = $"No se encontró un producto con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Producto actualizado exitosamente: ID {ProductoId}", id);
                return Ok(producto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al actualizar producto: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto con ID: {ProductoId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al actualizar el producto",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Cambiar disponibilidad de un producto
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="disponible">Nueva disponibilidad</param>
        /// <returns>Confirmación del cambio</returns>
        /// <response code="200">Disponibilidad actualizada</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado</response>
        [HttpPatch("{id}/disponibilidad")]
        [Authorize(Roles = "Administrador,Mesero")]
        [SwaggerOperation(
            Summary = "Cambiar disponibilidad de producto",
            Description = "Marca un producto como disponible o no disponible",
            OperationId = "Productos.CambiarDisponibilidad",
            Tags = new[] { "Gestión de Productos" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MessageResponse>> CambiarDisponibilidad(int id, [FromQuery] bool disponible)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var success = await _productoService.CambiarDisponibilidadProductoAsync(id, disponible, usuarioId);
                
                if (!success)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Producto no encontrado",
                        Detail = $"No se encontró un producto con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Disponibilidad de producto {ProductoId} cambiada a: {Disponible}", id, disponible);
                
                return Ok(new MessageResponse
                {
                    Message = $"Producto marcado como {(disponible ? "disponible" : "no disponible")}",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar disponibilidad del producto: {ProductoId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al cambiar la disponibilidad",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Eliminar un producto (desactivar)
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="200">Producto eliminado</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Eliminar producto",
            Description = "Desactiva un producto del menú (no se elimina físicamente)",
            OperationId = "Productos.EliminarProducto",
            Tags = new[] { "Gestión de Productos" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<MessageResponse>> EliminarProducto(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var success = await _productoService.EliminarProductoAsync(id, usuarioId);
                
                if (!success)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Producto no encontrado",
                        Detail = $"No se encontró un producto con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Producto eliminado exitosamente: ID {ProductoId}", id);
                
                return Ok(new MessageResponse
                {
                    Message = "Producto eliminado exitosamente",
                    Success = true
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al eliminar producto: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto: {ProductoId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al eliminar el producto",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // COMBOS ESPECIALES
        // ============================================================================

        /// <summary>
        /// Obtener todos los combos disponibles
        /// </summary>
        /// <returns>Lista de combos disponibles</returns>
        /// <response code="200">Lista de combos</response>
        [HttpGet("combos")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener combos disponibles",
            Description = "Devuelve todos los combos especiales disponibles del restaurante",
            OperationId = "Productos.GetCombosDisponibles",
            Tags = new[] { "Combos" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ComboBasicoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ComboBasicoResponse>>> GetCombosDisponibles()
        {
            try
            {
                var combos = await _productoService.GetCombosDisponiblesAsync();
                return Ok(combos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combos disponibles");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener los combos",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Obtener un combo específico
        /// </summary>
        /// <param name="id">ID del combo</param>
        /// <returns>Datos del combo</returns>
        /// <response code="200">Combo encontrado</response>
        /// <response code="404">Combo no encontrado</response>
        [HttpGet("combos/{id}")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Obtener combo por ID",
            Description = "Devuelve la información detallada de un combo específico con sus productos",
            OperationId = "Productos.GetComboById",
            Tags = new[] { "Combos" }
        )]
        [ProducesResponseType(typeof(ComboBasicoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ComboBasicoResponse>> GetComboById(int id)
        {
            try
            {
                var combo = await _productoService.GetComboByIdAsync(id);
                
                if (combo == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Combo no encontrado",
                        Detail = $"No se encontró un combo con ID {id}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(combo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combo con ID: {ComboId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al obtener el combo",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // CÁLCULOS Y VERIFICACIONES
        // ============================================================================

        /// <summary>
        /// Calcular precio total de una lista de items
        /// </summary>
        /// <param name="items">Lista de items con cantidades</param>
        /// <returns>Precio total calculado</returns>
        /// <response code="200">Cálculo realizado</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("calcular-precio")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Calcular precio total",
            Description = "Calcula el precio total de una lista de productos y/o combos",
            OperationId = "Productos.CalcularPrecioTotal",
            Tags = new[] { "Cálculos" }
        )]
        [ProducesResponseType(typeof(CalculoPrecioResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CalculoPrecioResult>> CalcularPrecioTotal([FromBody] List<ItemCalculoRequest> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Items requeridos",
                        Detail = "Debe proporcionar al menos un item para calcular",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var resultado = await _productoService.CalcularPrecioTotalAsync(items);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al calcular precio: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular precio total");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al calcular el precio",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Verificar disponibilidad de productos para orden
        /// </summary>
        /// <param name="items">Lista de items a verificar</param>
        /// <returns>Estado de disponibilidad</returns>
        /// <response code="200">Verificación realizada</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("verificar-disponibilidad")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Verificar disponibilidad",
            Description = "Verifica si los productos solicitados están disponibles en inventario",
            OperationId = "Productos.VerificarDisponibilidad",
            Tags = new[] { "Inventario" }
        )]
        [ProducesResponseType(typeof(DisponibilidadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DisponibilidadResult>> VerificarDisponibilidad([FromBody] List<ItemOrdenRequest> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Items requeridos",
                        Detail = "Debe proporcionar al menos un item para verificar",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var resultado = await _productoService.VerificarDisponibilidadAsync(items);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al verificar disponibilidad",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
} 