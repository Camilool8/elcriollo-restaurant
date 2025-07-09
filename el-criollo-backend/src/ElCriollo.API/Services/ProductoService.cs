using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.Entities;
using ElCriollo.API.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Servicio de gestión de productos para el restaurante El Criollo
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(
            IProductoRepository productoRepository,
            IInventarioRepository inventarioRepository,
            ILogger<ProductoService> logger)
        {
            _productoRepository = productoRepository;
            _inventarioRepository = inventarioRepository;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DE PRODUCTOS
        // ============================================================================

        /// <summary>
        /// Obtiene el menú digital completo con productos dominicanos
        /// </summary>
        public async Task<MenuDigitalViewModel> GetMenuDigitalAsync(bool incluirNoDisponibles = false)
        {
            try
            {
                _logger.LogDebug("Obteniendo menú digital. Incluir no disponibles: {IncluirNoDisponibles}", incluirNoDisponibles);

                var productos = incluirNoDisponibles 
                    ? await _productoRepository.GetProductosActivosAsync()
                    : await _productoRepository.GetProductosDisponiblesAsync();

                var menuDigital = new MenuDigitalViewModel
                {
                    FechaActualizacion = DateTime.Now,
                    TotalProductos = productos.Count(),
                    Categorias = productos
                        .GroupBy(p => p.Categoria?.Nombre ?? "Sin Categoría")
                        .Select(g => new CategoriaMenuViewModel
                        {
                            Nombre = g.Key,
                            Productos = g.Select(MapToProductoResponse).ToList()
                        })
                        .OrderBy(c => c.Nombre)
                        .ToList()
                };

                _logger.LogDebug("Menú digital obtenido con {Count} categorías", menuDigital.Categorias.Count);
                return menuDigital;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener menú digital");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un producto específico por ID
        /// </summary>
        public async Task<ProductoResponse?> GetProductoByIdAsync(int productoId)
        {
            try
            {
                _logger.LogDebug("Obteniendo producto por ID: {ProductoId}", productoId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado: {ProductoId}", productoId);
                    return null;
                }

                return MapToProductoResponse(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los productos disponibles
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetProductosDisponiblesAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos disponibles");

                var productos = await _productoRepository.GetProductosDisponiblesAsync();
                var productosResponse = productos.Select(MapToProductoResponse).ToList();

                _logger.LogDebug("Se obtuvieron {Count} productos disponibles", productosResponse.Count);
                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos disponibles");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo producto básico
        /// </summary>
        public async Task<ProductoResponse> CrearProductoAsync(CrearProductoRequest crearProductoRequest, int usuarioId)
        {
            try
            {
                _logger.LogDebug("Creando nuevo producto. Usuario: {UsuarioId}", usuarioId);

                // Validar que el nombre no exista
                if (await _productoRepository.NombreProductoExisteAsync(crearProductoRequest.Nombre))
                {
                    throw new InvalidOperationException($"Ya existe un producto con el nombre '{crearProductoRequest.Nombre}'");
                }

                // Generar stock aleatorio entre 4 y 100
                var random = new Random();
                var stockAleatorio = random.Next(4, 101); // 4 a 100 inclusive

                var producto = new Producto
                {
                    Nombre = crearProductoRequest.Nombre,
                    Descripcion = crearProductoRequest.Descripcion,
                    CategoriaID = crearProductoRequest.CategoriaId,
                    Precio = crearProductoRequest.Precio,
                    TiempoPreparacion = crearProductoRequest.TiempoPreparacion,
                    Imagen = crearProductoRequest.Imagen,
                    Estado = true // Siempre disponible
                };

                var productoGuardado = await _productoRepository.AddAsync(producto);
                await _productoRepository.SaveChangesAsync();

                // Crear inventario con stock aleatorio
                var inventario = new Inventario
                {
                    ProductoID = productoGuardado.ProductoID,
                    CantidadDisponible = stockAleatorio,
                    CantidadMinima = 4,
                    UltimaActualizacion = DateTime.UtcNow
                };

                await _inventarioRepository.CreateAsync(inventario);
                await _inventarioRepository.SaveChangesAsync();

                _logger.LogInformation("Producto creado exitosamente. ID: {ProductoId}, Stock: {Stock}", productoGuardado.ProductoID, stockAleatorio);
                return MapToProductoResponse(productoGuardado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                throw;
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        public async Task<ProductoResponse?> ActualizarProductoAsync(int productoId, ActualizarProductoRequest actualizarRequest, int usuarioId)
        {
            try
            {
                _logger.LogDebug("Actualizando producto {ProductoId}. Usuario: {UsuarioId}", productoId, usuarioId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado para actualizar: {ProductoId}", productoId);
                    return null;
                }

                // Actualizar campos si se proporcionan
                if (!string.IsNullOrEmpty(actualizarRequest.Nombre))
                {
                    if (await _productoRepository.NombreProductoExisteAsync(actualizarRequest.Nombre, productoId))
                    {
                        throw new InvalidOperationException($"Ya existe otro producto con el nombre '{actualizarRequest.Nombre}'");
                    }
                    producto.Nombre = actualizarRequest.Nombre;
                }

                if (actualizarRequest.Descripcion != null)
                    producto.Descripcion = actualizarRequest.Descripcion;

                if (actualizarRequest.Precio.HasValue)
                    producto.ActualizarPrecio(actualizarRequest.Precio.Value);

                if (actualizarRequest.CategoriaId.HasValue)
                    producto.CategoriaID = actualizarRequest.CategoriaId.Value;

                // No permitir cambiar la disponibilidad - siempre debe estar disponible
                // if (actualizarRequest.Disponible.HasValue)
                // {
                //     if (actualizarRequest.Disponible.Value)
                //         producto.Activar();
                //     else
                //         producto.Desactivar();
                // }

                await _productoRepository.SaveChangesAsync();

                _logger.LogInformation("Producto actualizado exitosamente. ID: {ProductoId}", productoId);
                return MapToProductoResponse(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Activa/desactiva la disponibilidad de un producto
        /// </summary>
        public async Task<bool> CambiarDisponibilidadProductoAsync(int productoId, bool disponible, int usuarioId)
        {
            try
            {
                _logger.LogDebug("Cambiando disponibilidad del producto {ProductoId} a {Disponible}. Usuario: {UsuarioId}", 
                    productoId, disponible, usuarioId);

                var resultado = await _productoRepository.CambiarEstadoProductoAsync(productoId, disponible);

                if (resultado)
                {
                    _logger.LogInformation("Disponibilidad del producto {ProductoId} cambiada a {Disponible}", productoId, disponible);
                }
                else
                {
                    _logger.LogWarning("No se pudo cambiar la disponibilidad del producto {ProductoId}", productoId);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar disponibilidad del producto {ProductoId}", productoId);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS
        // ============================================================================

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetProductosPorCategoriaAsync(int categoriaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo productos por categoría: {CategoriaId}", categoriaId);

                var productos = await _productoRepository.GetByCategoriaAsync(categoriaId);
                var productosResponse = productos.Select(MapToProductoResponse).ToList();

                _logger.LogDebug("Se obtuvieron {Count} productos de la categoría {CategoriaId}", productosResponse.Count, categoriaId);
                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría {CategoriaId}", categoriaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las categorías disponibles
        /// </summary>
        public async Task<IEnumerable<CategoriaBasicaResponse>> GetCategoriasAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo categorías disponibles");

                var productos = await _productoRepository.GetProductosActivosAsync();
                var categorias = productos
                    .GroupBy(p => p.Categoria)
                    .Where(g => g.Key != null)
                    .Select(g => new CategoriaBasicaResponse
                    {
                        CategoriaID = g.Key!.CategoriaID,
                        NombreCategoria = g.Key.Nombre,
                        Descripcion = g.Key.Descripcion,
                        CantidadProductos = g.Count(),
                        ProductosDisponibles = g.Count(p => p.EstaDisponible)
                    })
                    .OrderBy(c => c.NombreCategoria)
                    .ToList();

                _logger.LogDebug("Se obtuvieron {Count} categorías", categorias.Count);
                return categorias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                throw;
            }
        }

        /// <summary>
        /// Busca productos por nombre (búsqueda simple)
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> BuscarProductosPorNombreAsync(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    _logger.LogWarning("Término de búsqueda vacío");
                    return Enumerable.Empty<ProductoResponse>();
                }

                _logger.LogDebug("Buscando productos por nombre: {Nombre}", nombre);

                var productos = await _productoRepository.BuscarProductosAsync(nombre, null); // Agregar segundo parámetro
                var productosResponse = productos.Select(MapToProductoResponse).ToList();

                _logger.LogDebug("Se encontraron {Count} productos con el nombre '{Nombre}'", productosResponse.Count, nombre);
                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos por nombre: {Nombre}", nombre);
                return new List<ProductoResponse>(); // Retornar lista vacía en lugar de throw
            }
        }

        /// <summary>
        /// Busca productos por término en nombre o descripción
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> BuscarProductosAsync(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    _logger.LogWarning("Término de búsqueda vacío");
                    return Enumerable.Empty<ProductoResponse>();
                }

                _logger.LogDebug("Buscando productos por término: {Termino}", termino);

                // Buscar en nombre y descripción
                var productos = await _productoRepository.BuscarProductosAsync(termino, null);
                var productosResponse = productos.Select(MapToProductoResponse).ToList();

                _logger.LogInformation("Se encontraron {Count} productos con el término '{Termino}'", productosResponse.Count, termino);
                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos por término: {Termino}", termino);
                return new List<ProductoResponse>();
            }
        }

        /// <summary>
        /// Elimina (desactiva) un producto
        /// </summary>
        public async Task<bool> EliminarProductoAsync(int productoId, int usuarioId)
        {
            try
            {
                _logger.LogDebug("Eliminando producto {ProductoId}. Usuario: {UsuarioId}", productoId, usuarioId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado para eliminar: {ProductoId}", productoId);
                    return false;
                }

                // Verificar si el producto tiene órdenes activas
                if (producto.DetalleOrdenes.Any(d => d.Orden.Estado != "Facturada" && d.Orden.Estado != "Cancelada"))
                {
                    throw new InvalidOperationException("No se puede eliminar un producto con órdenes activas");
                }

                // Desactivar el producto
                producto.Desactivar();
                await _productoRepository.SaveChangesAsync();

                _logger.LogInformation("Producto {ProductoId} eliminado (desactivado) exitosamente", productoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {ProductoId}", productoId);
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Verifica disponibilidad de un producto
        /// </summary>
        public async Task<bool> VerificarDisponibilidadAsync(int productoId, int cantidad = 1)
        {
            try
            {
                _logger.LogDebug("Verificando disponibilidad del producto {ProductoId}, cantidad: {Cantidad}", productoId, cantidad);

                var estaDisponible = await _productoRepository.EstaDisponibleAsync(productoId);
                if (!estaDisponible)
                {
                    _logger.LogDebug("Producto {ProductoId} no está disponible", productoId);
                    return false;
                }

                // Verificar stock si es necesario
                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto?.Inventario != null)
                {
                    var hayStock = producto.Inventario.PuedeSatisfacerOrden(cantidad);
                    _logger.LogDebug("Producto {ProductoId} - Stock disponible: {Stock}, Cantidad requerida: {Cantidad}, Hay stock: {HayStock}", 
                        productoId, producto.Inventario.CantidadDisponible, cantidad, hayStock);
                    return hayStock;
                }

                return true; // Si no tiene inventario, se considera disponible
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Verifica disponibilidad de una lista de productos
        /// </summary>
        public async Task<DisponibilidadResult> VerificarDisponibilidadAsync(List<ItemOrdenRequest> items)
        {
            try
            {
                _logger.LogDebug("Verificando disponibilidad de {Count} items", items.Count);

                var resultado = new DisponibilidadResult
                {
                    TodoDisponible = true,
                    ProductosNoDisponibles = new List<string>(),
                    ProductosStockBajo = new List<string>()
                };

                foreach (var item in items)
                {
                    var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                    {
                        resultado.TodoDisponible = false;
                        resultado.ProductosNoDisponibles.Add($"Producto ID {item.ProductoId} no encontrado");
                        continue;
                    }

                    if (!producto.EstaDisponible)
                    {
                        resultado.TodoDisponible = false;
                        resultado.ProductosNoDisponibles.Add($"{producto.Nombre} no está disponible");
                        continue;
                    }

                    // Verificar stock si existe inventario
                    if (producto.Inventario != null)
                    {
                        if (!producto.Inventario.PuedeSatisfacerOrden(item.Cantidad))
                        {
                            resultado.TodoDisponible = false;
                            resultado.ProductosNoDisponibles.Add($"{producto.Nombre} - Stock insuficiente (disponible: {producto.Inventario.CantidadDisponible}, requerido: {item.Cantidad})");
                        }
                        else if (producto.Inventario.StockBajo)
                        {
                            resultado.ProductosStockBajo.Add($"{producto.Nombre} - Stock bajo ({producto.Inventario.CantidadDisponible} unidades)");
                        }
                    }
                }

                _logger.LogDebug("Verificación de disponibilidad completada. Todo disponible: {TodoDisponible}", resultado.TodoDisponible);
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad de items");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el stock actual de un producto
        /// </summary>
        public async Task<StockProductoResult> GetStockProductoAsync(int productoId)
        {
            try
            {
                _logger.LogDebug("Obteniendo stock del producto {ProductoId}", productoId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    throw new InvalidOperationException($"Producto {productoId} no encontrado");
                }

                var inventario = producto.Inventario;
                var resultado = new StockProductoResult
                {
                    ProductoID = productoId,
                    Nombre = producto.Nombre,
                    CantidadDisponible = inventario?.CantidadDisponible ?? 0,
                    CantidadMinima = inventario?.CantidadMinima ?? 0,
                    RequiereRestock = inventario?.NecesitaReabastecimiento ?? false, // Cambiar () por solo propiedad
                    EstadoStock = inventario?.ObtenerEstadoStock() ?? "Sin inventario"
                };

                _logger.LogDebug("Stock obtenido para producto {ProductoId}: {Disponible}/{Minimo}", 
                    productoId, resultado.CantidadDisponible, resultado.CantidadMinima);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stock del producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto
        /// </summary>
        public async Task<bool> ActualizarStockAsync(int productoId, int nuevaCantidad, int usuarioId)
        {
            try
            {
                _logger.LogDebug("Actualizando stock del producto {ProductoId} a {NuevaCantidad}. Usuario: {UsuarioId}", 
                    productoId, nuevaCantidad, usuarioId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado para actualizar stock: {ProductoId}", productoId);
                    return false;
                }

                if (producto.Inventario == null)
                {
                    // Crear inventario si no existe
                    producto.Inventario = new Inventario
                    {
                        ProductoID = productoId,
                        CantidadDisponible = nuevaCantidad,
                        CantidadMinima = 5,
                        UltimaActualizacion = DateTime.UtcNow
                    };
                }
                else
                {
                    producto.Inventario.ActualizarCantidad(nuevaCantidad);
                }

                await _productoRepository.SaveChangesAsync();

                _logger.LogInformation("Stock del producto {ProductoId} actualizado a {NuevaCantidad}", productoId, nuevaCantidad);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        public async Task<IEnumerable<ProductoStockBajoResponse>> GetProductosStockBajoAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos con stock bajo");

                var productos = await _productoRepository.GetProductosStockBajoAsync();
                var productosResponse = productos.Select(p => new ProductoStockBajoResponse
                {
                    ProductoID = p.ProductoID,
                    Nombre = p.Nombre,
                    Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                    CantidadDisponible = p.Inventario?.CantidadDisponible ?? 0,
                    CantidadMinima = p.Inventario?.CantidadMinima ?? 0,
                    EsCritico = (p.Inventario?.CantidadDisponible ?? 0) <= ((p.Inventario?.CantidadMinima ?? 0) * 0.5), // Corregir comparación
                    Recomendacion = p.Inventario?.ObtenerRecomendacionReorden() ?? "Sin recomendación"
                }).ToList();

                _logger.LogDebug("Se encontraron {Count} productos con stock bajo", productosResponse.Count);
                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock bajo");
                return new List<ProductoStockBajoResponse>(); // Retornar lista vacía en lugar de throw
            }
        }

        // ============================================================================
        // CÁLCULOS
        // ============================================================================

        /// <summary>
        /// Calcula el precio total de una lista de productos
        /// </summary>
        public async Task<CalculoPrecioResult> CalcularPrecioTotalAsync(List<ItemCalculoRequest> items)
        {
            try
            {
                _logger.LogDebug("Calculando precio total para {Count} items", items.Count);

                var resultado = new CalculoPrecioResult
                {
                    Detalles = new List<DetalleCalculoItem>()
                };

                foreach (var item in items)
                {
                    var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                    {
                        _logger.LogWarning("Producto {ProductoId} no encontrado para cálculo", item.ProductoId);
                        continue;
                    }

                    var subtotal = producto.Precio * item.Cantidad;
                    resultado.Subtotal += subtotal;
                    resultado.TotalItems += item.Cantidad;

                    resultado.Detalles.Add(new DetalleCalculoItem
                    {
                        ProductoId = item.ProductoId,
                        Nombre = producto.Nombre,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotal
                    });
                }

                _logger.LogDebug("Precio total calculado: {Subtotal}", resultado.SubtotalFormateado);
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular precio total");
                throw;
            }
        }

        /// <summary>
        /// Calcula el precio con descuento si aplica
        /// </summary>
        public async Task<decimal> CalcularPrecioConDescuentoAsync(int productoId, int cantidad)
        {
            try
            {
                _logger.LogDebug("Calculando precio con descuento para producto {ProductoId}, cantidad: {Cantidad}", productoId, cantidad);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    throw new InvalidOperationException($"Producto {productoId} no encontrado");
                }

                var precioTotal = producto.Precio * cantidad;
                // Por ahora no hay descuentos implementados, pero se puede extender
                var precioConDescuento = precioTotal;

                _logger.LogDebug("Precio con descuento calculado: {PrecioConDescuento}", precioConDescuento);
                return precioConDescuento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular precio con descuento para producto {ProductoId}", productoId);
                throw;
            }
        }

        // ============================================================================
        // COMBOS
        // ============================================================================

        /// <summary>
        /// Obtiene todos los combos disponibles
        /// </summary>
        public async Task<IEnumerable<ComboBasicoResponse>> GetCombosDisponiblesAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo combos disponibles");

                // Por ahora retornamos una lista vacía ya que no hay implementación de combos
                // Esto se puede implementar cuando se agregue el repositorio de combos
                var combos = new List<ComboBasicoResponse>();

                _logger.LogDebug("Se obtuvieron {Count} combos disponibles", combos.Count);
                return await Task.FromResult(combos); // Agregar await
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combos disponibles");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un combo específico con sus productos
        /// </summary>
        public async Task<ComboBasicoResponse?> GetComboByIdAsync(int comboId)
        {
            try
            {
                _logger.LogDebug("Obteniendo combo por ID: {ComboId}", comboId);

                // Por ahora retornamos null ya que no hay implementación de combos
                _logger.LogWarning("Funcionalidad de combos no implementada");
                return await Task.FromResult<ComboBasicoResponse?>(null); // Agregar await
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combo por ID: {ComboId}", comboId);
                throw;
            }
        }

        /// <summary>
        /// Verifica disponibilidad de un combo
        /// </summary>
        public async Task<bool> VerificarDisponibilidadComboAsync(int comboId)
        {
            try
            {
                _logger.LogDebug("Verificando disponibilidad del combo {ComboId}", comboId);

                // Por ahora retornamos false ya que no hay implementación de combos
                _logger.LogWarning("Funcionalidad de combos no implementada");
                return await Task.FromResult(false); // Agregar await
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad del combo {ComboId}", comboId);
                throw;
            }
        }

        // ============================================================================
        // VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Valida que un producto puede ser creado
        /// </summary>
        public async Task<ValidacionProductoResult> ValidarProductoAsync(CrearProductoRequest crearProductoRequest)
        {
            try
            {
                _logger.LogDebug("Validando producto para crear: {Nombre}", crearProductoRequest.Nombre);

                var resultado = new ValidacionProductoResult();

                // Validar nombre
                if (string.IsNullOrWhiteSpace(crearProductoRequest.Nombre))
                {
                    resultado.Errores.Add("El nombre del producto es requerido");
                }
                else if (crearProductoRequest.Nombre.Length > 100)
                {
                    resultado.Errores.Add("El nombre del producto no puede exceder 100 caracteres");
                }

                // Validar precio
                if (crearProductoRequest.Precio <= 0)
                {
                    resultado.Errores.Add("El precio debe ser mayor a 0");
                }
                else if (crearProductoRequest.Precio > 999999.99m)
                {
                    resultado.Errores.Add("El precio no puede exceder RD$ 999,999.99");
                }

                // Validar categoría
                if (crearProductoRequest.CategoriaId <= 0)
                {
                    resultado.Errores.Add("Debe seleccionar una categoría válida");
                }

                // Validar tiempo de preparación
                if (crearProductoRequest.TiempoPreparacion.HasValue && 
                    (crearProductoRequest.TiempoPreparacion < 1 || crearProductoRequest.TiempoPreparacion > 999))
                {
                    resultado.Errores.Add("El tiempo de preparación debe estar entre 1 y 999 minutos");
                }

                // Verificar si el nombre ya existe
                if (!string.IsNullOrWhiteSpace(crearProductoRequest.Nombre) &&
                    await _productoRepository.NombreProductoExisteAsync(crearProductoRequest.Nombre))
                {
                    resultado.Errores.Add($"Ya existe un producto con el nombre '{crearProductoRequest.Nombre}'");
                }

                resultado.EsValido = !resultado.Errores.Any();

                _logger.LogDebug("Validación de producto completada. Es válido: {EsValido}, Errores: {Count}", 
                    resultado.EsValido, resultado.Errores.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar producto");
                throw;
            }
        }

        /// <summary>
        /// Valida que una lista de productos es válida para una orden
        /// </summary>
        public async Task<ValidacionProductosResult> ValidarListaProductosAsync(List<ItemCalculoRequest> items)
        {
            try
            {
                _logger.LogDebug("Validando lista de productos para orden. Items: {Count}", items.Count);

                var resultado = new ValidacionProductosResult();

                foreach (var item in items)
                {
                    var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                    {
                        resultado.ProductosInvalidos.Add($"Producto ID {item.ProductoId} no encontrado");
                        continue;
                    }

                    if (!producto.Estado)
                    {
                        resultado.ProductosInvalidos.Add($"Producto '{producto.Nombre}' no está activo");
                        continue;
                    }

                    if (!producto.EstaDisponible)
                    {
                        resultado.ProductosNoDisponibles.Add($"Producto '{producto.Nombre}' no está disponible");
                        continue;
                    }

                    if (item.Cantidad <= 0)
                    {
                        resultado.ProductosInvalidos.Add($"Cantidad inválida para producto '{producto.Nombre}'");
                        continue;
                    }

                    if (item.Cantidad > 99)
                    {
                        resultado.ProductosInvalidos.Add($"Cantidad máxima excedida para producto '{producto.Nombre}'");
                        continue;
                    }

                    // Verificar stock si es necesario
                    if (producto.Inventario != null && !producto.Inventario.PuedeSatisfacerOrden(item.Cantidad))
                    {
                        resultado.ProductosNoDisponibles.Add($"Stock insuficiente para producto '{producto.Nombre}' (disponible: {producto.Inventario.CantidadDisponible}, requerido: {item.Cantidad})");
                    }
                }

                resultado.TodosValidos = !resultado.ProductosInvalidos.Any() && !resultado.ProductosNoDisponibles.Any();

                _logger.LogDebug("Validación de lista de productos completada. Todos válidos: {TodosValidos}", resultado.TodosValidos);
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar lista de productos");
                throw;
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS
        // ============================================================================

        /// <summary>
        /// Mapea una entidad Producto a ProductoResponse
        /// </summary>
        private ProductoResponse MapToProductoResponse(Producto producto)
        {
            return new ProductoResponse
            {
                ProductoID = producto.ProductoID,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Categoria = producto.Categoria != null ? new CategoriaBasicaResponse
                {
                    CategoriaID = producto.Categoria.CategoriaID,
                    NombreCategoria = producto.Categoria.Nombre,
                    Descripcion = producto.Categoria.Descripcion
                } : null,
                Precio = producto.PrecioFormateado,
                PrecioNumerico = producto.Precio,
                TiempoPreparacion = producto.TiempoPreparacionFormateado,
                Imagen = producto.Imagen,
                EstaDisponible = producto.EstaDisponible,
                Inventario = producto.Inventario != null ? new InventarioBasicoResponse
                {
                    CantidadDisponible = producto.Inventario.CantidadDisponible,
                    NivelStock = producto.Inventario.ObtenerEstadoStock(),
                    ColorIndicador = producto.Inventario.ObtenerColorIndicador(),
                    StockBajo = producto.TieneStockBajo
                } : null,
                EsPlatoDominicano = producto.EsPlatoDominicano(),
                InformacionNutricional = producto.ObtenerInformacionNutricional()
            };
        }
    }

    // ============================================================================
    // VIEWMODELS ADICIONALES
    // ============================================================================

    /// <summary>
    /// ViewModel para el menú digital
    /// </summary>
    public class MenuDigitalViewModel
    {
        public DateTime FechaActualizacion { get; set; }
        public int TotalProductos { get; set; }
        public List<CategoriaMenuViewModel> Categorias { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para categoría en el menú
    /// </summary>
    public class CategoriaMenuViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public List<ProductoResponse> Productos { get; set; } = new();
    }
}
