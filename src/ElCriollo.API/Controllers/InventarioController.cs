using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Services;
using ElCriollo.API.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using AutoMapper;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de inventario del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Gestión de inventario, stock, movimientos y alertas de reabastecimiento")]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(
            IInventarioRepository inventarioRepository,
            IProductoRepository productoRepository,
            IEmailService emailService,
            IMapper mapper,
            ILogger<InventarioController> logger)
        {
            _inventarioRepository = inventarioRepository;
            _productoRepository = productoRepository;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // CONSULTA DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Obtener estado actual del inventario
        /// </summary>
        /// <param name="incluirAgotados">Incluir productos agotados</param>
        /// <returns>Lista completa del inventario actual</returns>
        /// <response code="200">Estado del inventario</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener inventario actual",
            Description = "Devuelve el estado actual del inventario con cantidades, costos y alertas",
            OperationId = "Inventario.GetActual",
            Tags = new[] { "Consulta de Inventario" }
        )]
        [ProducesResponseType(typeof(IEnumerable<InventarioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventarioResponse>>> GetInventarioActual([FromQuery] bool incluirAgotados = true)
        {
            try
            {
                _logger.LogInformation("📦 Consultando estado actual del inventario");

                var inventario = await _inventarioRepository.GetWithIncludesAsync(i => i.Producto);
                
                if (!incluirAgotados)
                {
                    inventario = inventario.Where(i => i.CantidadDisponible > 0);
                }

                var response = _mapper.Map<IEnumerable<InventarioResponse>>(inventario);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener inventario de un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>Estado del inventario del producto</returns>
        /// <response code="200">Inventario del producto</response>
        /// <response code="404">Producto no encontrado</response>
        [HttpGet("producto/{productoId:int}")]
        [SwaggerOperation(
            Summary = "Obtener inventario por producto",
            Description = "Obtiene el estado del inventario de un producto específico",
            OperationId = "Inventario.GetPorProducto",
            Tags = new[] { "Consulta de Inventario" }
        )]
        [ProducesResponseType(typeof(InventarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventarioResponse>> GetInventarioPorProducto(int productoId)
        {
            try
            {
                var inventario = await _inventarioRepository.GetByProductoIdAsync(productoId);
                
                if (inventario == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Inventario no encontrado",
                        Detail = $"No se encontró inventario para el producto con ID {productoId}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var response = _mapper.Map<InventarioResponse>(inventario);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener inventario del producto {ProductoId}", productoId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener productos con stock bajo
        /// </summary>
        /// <returns>Lista de productos que necesitan reabastecimiento</returns>
        /// <response code="200">Productos con stock bajo</response>
        [HttpGet("stock-bajo")]
        [SwaggerOperation(
            Summary = "Productos con stock bajo",
            Description = "Devuelve productos que están por debajo del stock mínimo establecido",
            OperationId = "Inventario.GetStockBajo",
            Tags = new[] { "Alertas de Inventario" }
        )]
        [ProducesResponseType(typeof(IEnumerable<AlertaInventarioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AlertaInventarioResponse>>> GetProductosStockBajo()
        {
            try
            {
                _logger.LogInformation("⚠️ Consultando productos con stock bajo");

                var productosStockBajo = await _inventarioRepository.GetProductosStockBajoAsync();
                var response = _mapper.Map<IEnumerable<AlertaInventarioResponse>>(productosStockBajo);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener productos con stock bajo");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener productos agotados
        /// </summary>
        /// <returns>Lista de productos sin stock</returns>
        /// <response code="200">Productos agotados</response>
        [HttpGet("agotados")]
        [SwaggerOperation(
            Summary = "Productos agotados",
            Description = "Devuelve la lista de productos completamente agotados",
            OperationId = "Inventario.GetAgotados",
            Tags = new[] { "Alertas de Inventario" }
        )]
        [ProducesResponseType(typeof(IEnumerable<ProductoAgotadoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductoAgotadoResponse>>> GetProductosAgotados()
        {
            try
            {
                _logger.LogInformation("🚨 Consultando productos agotados");

                var productosAgotados = await _inventarioRepository.GetProductosAgotadosAsync();
                var response = _mapper.Map<IEnumerable<ProductoAgotadoResponse>>(productosAgotados);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener productos agotados");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // MOVIMIENTOS DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Registrar entrada de inventario
        /// </summary>
        /// <param name="request">Datos de la entrada</param>
        /// <returns>Confirmación de registro</returns>
        /// <response code="200">Entrada registrada exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("entrada")]
        [Authorize(Roles = "Administrador,Cajero")]
        [SwaggerOperation(
            Summary = "Registrar entrada de inventario",
            Description = "Registra una entrada de productos al inventario (compra, reabastecimiento, etc.)",
            OperationId = "Inventario.RegistrarEntrada",
            Tags = new[] { "Movimientos de Inventario" }
        )]
        [ProducesResponseType(typeof(MovimientoInventarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MovimientoInventarioResponse>> RegistrarEntrada([FromBody] EntradaInventarioRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("📥 Registrando entrada de inventario para producto {ProductoId}", request.ProductoId);

                // Verificar que el producto existe
                var producto = await _productoRepository.GetByIdAsync(request.ProductoId);
                if (producto == null)
                {
                    return BadRequest(new ValidationProblemDetails
                    {
                        Title = "Producto no encontrado",
                        Detail = $"No existe el producto con ID {request.ProductoId}",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Registrar entrada usando el nuevo método del repositorio
                var movimiento = await _inventarioRepository.RegistrarEntradaAsync(
                    request.ProductoId, 
                    (int)request.Cantidad, 
                    request.CostoUnitario,
                    User.Identity?.Name ?? "Sistema",
                    request.Proveedor,
                    request.NumeroFactura,
                    request.Observaciones
                );

                var resultado = new MovimientoInventarioResponse
                {
                    Success = true,
                    TipoMovimiento = movimiento.TipoMovimiento,
                    ProductoId = movimiento.ProductoID,
                    NombreProducto = producto.Nombre,
                    CantidadMovimiento = movimiento.Cantidad,
                    StockAnterior = movimiento.StockAnterior,
                    StockActual = movimiento.StockResultante,
                    FechaMovimiento = movimiento.FechaMovimiento,
                    Usuario = movimiento.Usuario,
                    RequiereReabastecimiento = false
                };

                _logger.LogInformation("✅ Entrada registrada exitosamente. Nuevo stock: {NuevoStock}", resultado.StockActual);
                
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al registrar entrada de inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Registrar salida de inventario
        /// </summary>
        /// <param name="request">Datos de la salida</param>
        /// <returns>Confirmación de registro</returns>
        /// <response code="200">Salida registrada exitosamente</response>
        /// <response code="400">Stock insuficiente o datos inválidos</response>
        [HttpPost("salida")]
        [Authorize(Roles = "Administrador,Cajero,Cocina")]
        [SwaggerOperation(
            Summary = "Registrar salida de inventario",
            Description = "Registra una salida de productos del inventario (venta, merma, etc.)",
            OperationId = "Inventario.RegistrarSalida",
            Tags = new[] { "Movimientos de Inventario" }
        )]
        [ProducesResponseType(typeof(MovimientoInventarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MovimientoInventarioResponse>> RegistrarSalida([FromBody] SalidaInventarioRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("📤 Registrando salida de inventario para producto {ProductoId}", request.ProductoId);

                // Registrar salida usando el nuevo método del repositorio
                var movimiento = await _inventarioRepository.RegistrarSalidaAsync(
                    request.ProductoId,
                    (int)request.Cantidad,
                    User.Identity?.Name ?? "Sistema",
                    null, // referencia
                    request.Observaciones
                );

                var resultado = new MovimientoInventarioResponse
                {
                    Success = true,
                    TipoMovimiento = movimiento.TipoMovimiento,
                    ProductoId = movimiento.ProductoID,
                    NombreProducto = movimiento.Producto?.Nombre ?? "Producto",
                    CantidadMovimiento = Math.Abs(movimiento.Cantidad),
                    StockAnterior = movimiento.StockAnterior,
                    StockActual = movimiento.StockResultante,
                    FechaMovimiento = movimiento.FechaMovimiento,
                    Usuario = movimiento.Usuario,
                    RequiereReabastecimiento = movimiento.StockResultante <= 5 // Asumiendo stock mínimo de 5
                };

                // Verificar si necesita alerta de stock bajo
                if (resultado.RequiereReabastecimiento)
                {
                    try
                    {
                        var producto = await _productoRepository.GetByIdAsync(request.ProductoId);
                        await _emailService.NotificarStockBajoAsync(producto!);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "⚠️ No se pudo enviar alerta de stock bajo");
                    }
                }

                _logger.LogInformation("✅ Salida registrada exitosamente. Stock restante: {StockActual}", resultado.StockActual);
                
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "⚠️ Operación inválida al registrar salida");
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error en la operación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al registrar salida de inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Realizar ajuste de inventario
        /// </summary>
        /// <param name="request">Datos del ajuste</param>
        /// <returns>Confirmación del ajuste</returns>
        /// <response code="200">Ajuste realizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("ajuste")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Ajustar inventario",
            Description = "Realiza un ajuste manual del inventario (corrección de stock, conteo físico, etc.)",
            OperationId = "Inventario.Ajustar",
            Tags = new[] { "Movimientos de Inventario" }
        )]
        [ProducesResponseType(typeof(MovimientoInventarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MovimientoInventarioResponse>> AjustarInventario([FromBody] AjusteInventarioRequest request)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("🔧 Realizando ajuste de inventario para producto {ProductoId}", request.ProductoId);

                // Realizar ajuste usando el nuevo método del repositorio
                var movimiento = await _inventarioRepository.AjustarInventarioAsync(
                    request.ProductoId,
                    (int)request.NuevaCantidad,
                    User.Identity?.Name ?? "Sistema",
                    request.Motivo,
                    null // observaciones adicionales
                );

                var resultado = new MovimientoInventarioResponse
                {
                    Success = true,
                    TipoMovimiento = movimiento.TipoMovimiento,
                    ProductoId = movimiento.ProductoID,
                    NombreProducto = movimiento.Producto?.Nombre ?? "Producto",
                    CantidadMovimiento = Math.Abs(movimiento.Cantidad),
                    StockAnterior = movimiento.StockAnterior,
                    StockActual = movimiento.StockResultante,
                    FechaMovimiento = movimiento.FechaMovimiento,
                    Usuario = movimiento.Usuario,
                    RequiereReabastecimiento = movimiento.StockResultante <= 5 // Asumiendo stock mínimo de 5
                };

                _logger.LogInformation("✅ Ajuste realizado. Stock ajustado a: {NuevoStock}", resultado.StockActual);
                
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "⚠️ Operación inválida al ajustar inventario");
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error en la operación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al ajustar inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener historial de movimientos
        /// </summary>
        /// <param name="productoId">ID del producto (opcional)</param>
        /// <param name="fechaInicio">Fecha inicio del historial</param>
        /// <param name="fechaFin">Fecha fin del historial</param>
        /// <returns>Lista de movimientos de inventario</returns>
        /// <response code="200">Historial de movimientos</response>
        [HttpGet("movimientos")]
        [SwaggerOperation(
            Summary = "Historial de movimientos",
            Description = "Obtiene el historial detallado de movimientos del inventario",
            OperationId = "Inventario.GetMovimientos",
            Tags = new[] { "Consulta de Inventario" }
        )]
        [ProducesResponseType(typeof(IEnumerable<MovimientoHistorialResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovimientoHistorialResponse>>> GetHistorialMovimientos(
            [FromQuery] int? productoId = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogInformation("📋 Consultando historial de movimientos");

                var movimientos = await _inventarioRepository.GetMovimientosAsync(productoId, fechaInicio, fechaFin);
                var response = movimientos.Select(m => new MovimientoHistorialResponse
                {
                    MovimientoId = m.MovimientoID,
                    Fecha = m.FechaMovimiento,
                    TipoMovimiento = m.TipoMovimiento,
                    Producto = m.Producto?.Nombre ?? "Producto",
                    Cantidad = Math.Abs(m.Cantidad),
                    StockResultante = m.StockResultante,
                    CostoUnitario = m.CostoUnitario,
                    Referencia = m.Referencia,
                    Usuario = m.Usuario,
                    Observaciones = m.Observaciones
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener historial de movimientos");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // CONFIGURACIÓN Y GESTIÓN
        // ============================================================================

        /// <summary>
        /// Actualizar stock mínimo
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="request">Nuevo stock mínimo</param>
        /// <returns>Confirmación de actualización</returns>
        /// <response code="200">Stock mínimo actualizado</response>
        /// <response code="404">Producto no encontrado</response>
        [HttpPut("producto/{productoId:int}/stock-minimo")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Actualizar stock mínimo",
            Description = "Actualiza el stock mínimo de un producto para alertas de reabastecimiento",
            OperationId = "Inventario.ActualizarStockMinimo",
            Tags = new[] { "Configuración de Inventario" }
        )]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ActualizarStockMinimo(int productoId, [FromBody] ActualizarStockMinimoRequest request)
        {
            try
            {
                _logger.LogInformation("📊 Actualizando stock mínimo del producto {ProductoId} a {NuevoStock}", 
                    productoId, request.StockMinimo);

                var inventario = await _inventarioRepository.GetByProductoIdAsync(productoId);
                if (inventario == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Inventario no encontrado",
                        Detail = $"No se encontró inventario para el producto con ID {productoId}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                inventario.CantidadMinima = (int)request.StockMinimo;
                // StockMaximo no existe en la entidad, comentando esta línea
                // inventario.StockMaximo = request.StockMaximo ?? inventario.StockMaximo;
                
                await _inventarioRepository.UpdateAsync(inventario);

                _logger.LogInformation("✅ Stock mínimo actualizado exitosamente");
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Stock mínimo actualizado exitosamente",
                    Data = new 
                    { 
                        ProductoId = productoId, 
                        StockMinimo = request.StockMinimo,
                        StockMaximo = request.StockMaximo // Mantener valor del request ya que no existe en entidad
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al actualizar stock mínimo");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Generar orden de reabastecimiento
        /// </summary>
        /// <returns>Lista de productos que necesitan reabastecimiento</returns>
        /// <response code="200">Orden de reabastecimiento generada</response>
        [HttpGet("orden-reabastecimiento")]
        [Authorize(Roles = "Administrador,Cajero")]
        [SwaggerOperation(
            Summary = "Generar orden de reabastecimiento",
            Description = "Genera una orden sugerida de reabastecimiento basada en stocks mínimos",
            OperationId = "Inventario.GenerarOrdenReabastecimiento",
            Tags = new[] { "Gestión de Compras" }
        )]
        [ProducesResponseType(typeof(OrdenReabastecimientoResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<OrdenReabastecimientoResponse>> GenerarOrdenReabastecimiento()
        {
            try
            {
                _logger.LogInformation("📋 Generando orden de reabastecimiento");

                var productosReabastecer = await _inventarioRepository.GetInventariosParaReabastecer();
                
                var response = new OrdenReabastecimientoResponse
                {
                    FechaGeneracion = DateTime.Now,
                    TotalProductos = productosReabastecer.Count(),
                    Items = productosReabastecer.Select(i => new ItemReabastecimientoResponse
                    {
                        ProductoId = i.ProductoID,
                        NombreProducto = i.Producto?.Nombre ?? "Producto",
                        Categoria = i.Producto?.Categoria?.NombreCategoria ?? "Sin categoría",
                        StockActual = i.CantidadDisponible,
                        StockMinimo = i.CantidadMinima,
                        StockMaximo = i.CantidadMinima * 3, // Estimado
                        CantidadSugerida = (i.CantidadMinima * 3) - i.CantidadDisponible,
                        UltimoCosto = i.Producto?.Precio ?? 0,
                        CostoEstimado = ((i.CantidadMinima * 3) - i.CantidadDisponible) * (i.Producto?.Precio ?? 0),
                        ProveedorSugerido = "Por determinar"
                    }).ToList()
                };

                // Calcular costo estimado
                response.CostoEstimado = response.Items.Sum(i => i.CantidadSugerida * i.UltimoCosto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar orden de reabastecimiento");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REPORTES DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Obtener valorización del inventario
        /// </summary>
        /// <returns>Valorización total del inventario actual</returns>
        /// <response code="200">Valorización del inventario</response>
        [HttpGet("valorizacion")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Valorización del inventario",
            Description = "Calcula el valor total del inventario actual basado en costos",
            OperationId = "Inventario.GetValorizacion",
            Tags = new[] { "Reportes de Inventario" }
        )]
        [ProducesResponseType(typeof(ValorizacionInventarioResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ValorizacionInventarioResponse>> GetValorizacionInventario()
        {
            try
            {
                _logger.LogInformation("💰 Calculando valorización del inventario");

                var valorizacionData = await _inventarioRepository.GetValorizacionInventarioAsync();
                var datos = (dynamic)valorizacionData;
                
                // Convertir el objeto anónimo a ValorizacionInventarioResponse
                var valorizacion = new ValorizacionInventarioResponse
                {
                    FechaCalculo = datos.FechaCalculo,
                    ValorTotal = datos.ValorTotal,
                    TotalProductos = datos.TotalProductos,
                    ProductosConStock = datos.ProductosConStock,
                    ProductosAgotados = datos.ProductosAgotados,
                    ValorPorCategoria = datos.ValorPorCategoria,
                    Top10ProductosMayorValor = ((IEnumerable<dynamic>)datos.Top10ProductosMayorValor)
                        .Select(item => new ProductoValorizadoItem
                        {
                            ProductoId = item.ProductoId,
                            NombreProducto = item.NombreProducto,
                            CantidadDisponible = item.CantidadDisponible,
                            CostoUnitario = item.CostoUnitario,
                            ValorTotal = item.ValorTotal,
                            PorcentajeDelTotal = datos.ValorTotal > 0 
                                ? (item.ValorTotal / datos.ValorTotal) * 100 
                                : 0
                        })
                        .ToList()
                };

                return Ok(valorizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al calcular valorización del inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener rotación de inventario
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del análisis</param>
        /// <param name="fechaFin">Fecha fin del análisis</param>
        /// <returns>Análisis de rotación de inventario</returns>
        /// <response code="200">Análisis de rotación</response>
        [HttpGet("rotacion")]
        [Authorize(Roles = "Administrador")]
        [SwaggerOperation(
            Summary = "Análisis de rotación de inventario",
            Description = "Analiza la rotación del inventario para identificar productos de alta y baja rotación",
            OperationId = "Inventario.GetRotacion",
            Tags = new[] { "Reportes de Inventario" }
        )]
        [ProducesResponseType(typeof(RotacionInventarioResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<RotacionInventarioResponse>> GetRotacionInventario(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogInformation("🔄 Analizando rotación de inventario");

                var analisisData = await _inventarioRepository.GetAnalisisRotacionAsync(fechaInicio, fechaFin);
                var datos = (dynamic)analisisData;

                var rotacion = new RotacionInventarioResponse
                {
                    FechaInicio = datos.FechaInicio,
                    FechaFin = datos.FechaFin,
                    RotacionPromedio = datos.RotacionPromedio,
                    AltaRotacion = ((IEnumerable<dynamic>)datos.AltaRotacion)
                        .Select(item => new ProductoRotacionItem
                        {
                            ProductoId = item.ProductoId,
                            NombreProducto = item.NombreProducto,
                            IndiceRotacion = item.IndiceRotacion,
                            DiasCubrimiento = 0, // Calculado si es necesario
                            VentasPromedioDiarias = item.VentasPromedioDiarias,
                            Recomendacion = item.Recomendacion
                        })
                        .ToList(),
                    BajaRotacion = ((IEnumerable<dynamic>)datos.BajaRotacion)
                        .Select(item => new ProductoRotacionItem
                        {
                            ProductoId = item.ProductoId,
                            NombreProducto = item.NombreProducto,
                            IndiceRotacion = item.IndiceRotacion,
                            DiasCubrimiento = 0, // Calculado si es necesario
                            VentasPromedioDiarias = item.VentasPromedioDiarias,
                            Recomendacion = item.Recomendacion
                        })
                        .ToList(),
                    SinMovimiento = ((IEnumerable<dynamic>)datos.SinMovimiento)
                        .Select(item => new ProductoRotacionItem
                        {
                            ProductoId = item.ProductoId,
                            NombreProducto = item.NombreProducto,
                            IndiceRotacion = item.IndiceRotacion,
                            DiasCubrimiento = 0, // Calculado si es necesario
                            VentasPromedioDiarias = item.VentasPromedioDiarias,
                            Recomendacion = item.Recomendacion
                        })
                        .ToList()
                };

                return Ok(rotacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al analizar rotación de inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REQUESTS Y RESPONSES ESPECÍFICOS
        // ============================================================================

        public class EntradaInventarioRequest
        {
            public int ProductoId { get; set; }
            public decimal Cantidad { get; set; }
            public decimal CostoUnitario { get; set; }
            public string? Proveedor { get; set; }
            public string? NumeroFactura { get; set; }
            public string? Observaciones { get; set; }
        }

        public class SalidaInventarioRequest
        {
            public int ProductoId { get; set; }
            public decimal Cantidad { get; set; }
            public string TipoSalida { get; set; } = "Venta"; // Venta, Merma, Ajuste
            public string? Observaciones { get; set; }
        }

        public class AjusteInventarioRequest
        {
            public int ProductoId { get; set; }
            public decimal NuevaCantidad { get; set; }
            public string Motivo { get; set; } = string.Empty;
        }

        public class ActualizarStockMinimoRequest
        {
            public decimal StockMinimo { get; set; }
            public decimal? StockMaximo { get; set; }
        }

        public class InventarioResponse
        {
            public int InventarioID { get; set; }
            public int ProductoID { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public decimal CantidadDisponible { get; set; }
            public decimal StockMinimo { get; set; }
            public decimal StockMaximo { get; set; }
            public string UnidadMedida { get; set; } = string.Empty;
            public decimal UltimoCosto { get; set; }
            public decimal ValorTotal { get; set; }
            public DateTime FechaUltimaActualizacion { get; set; }
            public string Estado { get; set; } = string.Empty;
            public bool RequiereReabastecimiento { get; set; }
        }

        public class AlertaInventarioResponse
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public decimal StockActual { get; set; }
            public decimal StockMinimo { get; set; }
            public decimal CantidadFaltante { get; set; }
            public string Urgencia { get; set; } = string.Empty; // Alta, Media, Baja
            public int DiasParaAgotarse { get; set; }
        }

        public class ProductoAgotadoResponse
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public DateTime FechaAgotamiento { get; set; }
            public int DiasAgotado { get; set; }
            public decimal PerdidaEstimada { get; set; }
        }

        public class MovimientoInventarioResponse
        {
            public bool Success { get; set; }
            public string TipoMovimiento { get; set; } = string.Empty;
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public decimal CantidadMovimiento { get; set; }
            public decimal StockAnterior { get; set; }
            public decimal StockActual { get; set; }
            public DateTime FechaMovimiento { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public bool RequiereReabastecimiento { get; set; }
        }

        public class MovimientoHistorialResponse
        {
            public int MovimientoId { get; set; }
            public DateTime Fecha { get; set; }
            public string TipoMovimiento { get; set; } = string.Empty;
            public string Producto { get; set; } = string.Empty;
            public decimal Cantidad { get; set; }
            public decimal StockResultante { get; set; }
            public decimal? CostoUnitario { get; set; }
            public string? Referencia { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string? Observaciones { get; set; }
        }

        public class OrdenReabastecimientoResponse
        {
            public DateTime FechaGeneracion { get; set; }
            public int TotalProductos { get; set; }
            public decimal CostoEstimado { get; set; }
            public List<ItemReabastecimientoResponse> Items { get; set; } = new();
        }

        public class ItemReabastecimientoResponse
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public decimal StockActual { get; set; }
            public decimal StockMinimo { get; set; }
            public decimal StockMaximo { get; set; }
            public decimal CantidadSugerida { get; set; }
            public decimal UltimoCosto { get; set; }
            public decimal CostoEstimado { get; set; }
            public string? ProveedorSugerido { get; set; }
        }

        public class ValorizacionInventarioResponse
        {
            public DateTime FechaCalculo { get; set; }
            public decimal ValorTotal { get; set; }
            public int TotalProductos { get; set; }
            public int ProductosConStock { get; set; }
            public int ProductosAgotados { get; set; }
            public Dictionary<string, decimal> ValorPorCategoria { get; set; } = new();
            public List<ProductoValorizadoItem> Top10ProductosMayorValor { get; set; } = new();
        }

        public class ProductoValorizadoItem
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public decimal CantidadDisponible { get; set; }
            public decimal CostoUnitario { get; set; }
            public decimal ValorTotal { get; set; }
            public decimal PorcentajeDelTotal { get; set; }
        }

        public class RotacionInventarioResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal RotacionPromedio { get; set; }
            public List<ProductoRotacionItem> AltaRotacion { get; set; } = new();
            public List<ProductoRotacionItem> BajaRotacion { get; set; } = new();
            public List<ProductoRotacionItem> SinMovimiento { get; set; } = new();
        }

        public class ProductoRotacionItem
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public decimal IndiceRotacion { get; set; }
            public int DiasCubrimiento { get; set; }
            public decimal VentasPromedioDiarias { get; set; }
            public string Recomendacion { get; set; } = string.Empty;
        }
    }
}