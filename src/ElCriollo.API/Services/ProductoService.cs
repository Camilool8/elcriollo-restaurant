using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de productos para El Criollo
    /// Maneja menú dominicano, inventario, combos y lógica de negocio específica
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IComboRepository _comboRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductoService> _logger;

        // Configuración específica para El Criollo
        private const int STOCK_MINIMO_DEFAULT = 5;
        private const int RESERVA_DURACION_MINUTOS = 15;
        private const decimal DESCUENTO_COMBO_DEFAULT = 0.15m; // 15%

        // Categorías típicamente dominicanas
        private readonly string[] CATEGORIAS_DOMINICANAS = {
            "Platos Principales", "Acompañamientos", "Frituras", 
            "Desayunos", "Sopas", "Mariscos"
        };

        public ProductoService(
            IProductoRepository productoRepository,
            IInventarioRepository inventarioRepository,
            ICategoriaRepository categoriaRepository,
            IComboRepository comboRepository,
            IMapper mapper,
            ILogger<ProductoService> logger)
        {
            _productoRepository = productoRepository;
            _inventarioRepository = inventarioRepository;
            _categoriaRepository = categoriaRepository;
            _comboRepository = comboRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DEL MENÚ DIGITAL DOMINICANO
        // ============================================================================

        /// <summary>
        /// Obtiene el menú digital completo con productos dominicanos categorizados
        /// </summary>
        public async Task<MenuDigitalViewModel> GetMenuDigitalAsync(bool incluirNoDisponibles = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo menú digital completo - Incluir no disponibles: {IncluirNoDisponibles}", incluirNoDisponibles);

                // Obtener todas las categorías con productos
                var categorias = await _categoriaRepository.GetCategoriasConProductosAsync();
                
                var menuDigital = new MenuDigitalViewModel
                {
                    FechaActualizacion = DateTime.UtcNow,
                    TotalCategorias = categorias.Count(),
                    Categorias = new List<CategoriaMenuViewModel>()
                };

                foreach (var categoria in categorias.OrderBy(c => c.Nombre))
                {
                    var productos = incluirNoDisponibles 
                        ? categoria.Productos?.ToList() ?? new List<Producto>()
                        : categoria.Productos?.Where(p => p.EstaDisponible && p.Inventario?.CantidadActual > 0).ToList() ?? new List<Producto>();

                    if (!productos.Any() && !incluirNoDisponibles)
                        continue;

                    var categoriaMenu = new CategoriaMenuViewModel
                    {
                        Id = categoria.Id,
                        Nombre = categoria.Nombre,
                        Descripcion = categoria.Descripcion,
                        EsDominicana = CATEGORIAS_DOMINICANAS.Contains(categoria.Nombre),
                        TotalProductos = productos.Count,
                        ProductosDisponibles = productos.Count(p => p.EstaDisponible && p.Inventario?.CantidadActual > 0),
                        PrecioMinimo = productos.Any() ? productos.Min(p => p.Precio) : 0,
                        PrecioMaximo = productos.Any() ? productos.Max(p => p.Precio) : 0,
                        Productos = _mapper.Map<List<ProductoResponse>>(productos.OrderBy(p => p.Precio))
                    };

                    menuDigital.Categorias.Add(categoriaMenu);
                }

                // Obtener combos disponibles
                var combos = await GetCombosDisponiblesAsync(incluirNoDisponibles);
                menuDigital.CombosEspeciales = combos.ToList();

                // Estadísticas generales
                menuDigital.TotalProductos = menuDigital.Categorias.Sum(c => c.TotalProductos);
                menuDigital.ProductosDisponibles = menuDigital.Categorias.Sum(c => c.ProductosDisponibles);
                menuDigital.ProductosTradicionales = await GetProductosTradicionalsDominicanosAsync();

                _logger.LogInformation("Menú digital generado: {TotalCategorias} categorías, {TotalProductos} productos", 
                    menuDigital.TotalCategorias, menuDigital.TotalProductos);

                return menuDigital;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener menú digital");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetProductosPorCategoriaAsync(int categoriaId, bool incluirNoDisponibles = false)
        {
            try
            {
                _logger.LogDebug("Obteniendo productos de categoría ID: {CategoriaId}", categoriaId);

                var productos = incluirNoDisponibles
                    ? await _productoRepository.GetByCategoriaAsync(categoriaId)
                    : await _productoRepository.GetDisponiblesByCategoriaAsync(categoriaId);

                var productosResponse = _mapper.Map<List<ProductoResponse>>(productos);

                _logger.LogDebug("Encontrados {Count} productos en categoría {CategoriaId}", 
                    productosResponse.Count, categoriaId);

                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría {CategoriaId}", categoriaId);
                throw;
            }
        }

        /// <summary>
        /// Busca productos por término, categoría o características
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> BuscarProductosAsync(string termino, int? categoriaId = null, decimal? precioMin = null, decimal? precioMax = null)
        {
            try
            {
                _logger.LogInformation("Búsqueda de productos - Término: '{Termino}', Categoría: {CategoriaId}, Precio: {PrecioMin}-{PrecioMax}", 
                    termino, categoriaId, precioMin, precioMax);

                if (string.IsNullOrWhiteSpace(termino) && !categoriaId.HasValue)
                {
                    return await GetProductosPopularesAsync();
                }

                var productos = await _productoRepository.BuscarProductosAvanzadaAsync(
                    termino?.Trim(), categoriaId, precioMin, precioMax);

                var productosResponse = _mapper.Map<List<ProductoResponse>>(productos);

                _logger.LogInformation("Búsqueda completada: {Count} productos encontrados", productosResponse.Count);

                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de productos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos típicamente dominicanos
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetProductosTradicionalsDominicanosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos tradicionales dominicanos");

                var productos = await _productoRepository.GetProductosDominicanosAsync();
                var productosResponse = _mapper.Map<List<ProductoResponse>>(productos);

                _logger.LogDebug("Encontrados {Count} productos tradicionales dominicanos", productosResponse.Count);

                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos tradicionales dominicanos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos más populares/vendidos
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetProductosPopularesAsync(int limit = 10)
        {
            try
            {
                _logger.LogDebug("Obteniendo productos populares (limit: {Limit})", limit);

                var productos = await _productoRepository.GetMasVendidosAsync(limit);
                var productosResponse = _mapper.Map<List<ProductoResponse>>(productos);

                _logger.LogDebug("Encontrados {Count} productos populares", productosResponse.Count);

                return productosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos populares");
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE COMBOS ESPECIALES
        // ============================================================================

        /// <summary>
        /// Obtiene todos los combos disponibles
        /// </summary>
        public async Task<IEnumerable<ComboResponse>> GetCombosDisponiblesAsync(bool incluirNoDisponibles = false)
        {
            try
            {
                _logger.LogDebug("Obteniendo combos disponibles - Incluir no disponibles: {IncluirNoDisponibles}", incluirNoDisponibles);

                var combos = incluirNoDisponibles
                    ? await _comboRepository.GetAllWithProductosAsync()
                    : await _comboRepository.GetDisponiblesAsync();

                var combosResponse = _mapper.Map<List<ComboResponse>>(combos);

                // Enriquecer información de combos
                foreach (var combo in combosResponse)
                {
                    var comboEntity = combos.FirstOrDefault(c => c.Id == combo.Id);
                    if (comboEntity != null)
                    {
                        combo.EsComboDominicano = comboEntity.EsComboDominicano();
                        combo.TiempoPreparacionTotal = comboEntity.TiempoPreparacionTotal;
                        combo.Ahorro = comboEntity.Ahorro;
                        combo.PorcentajeDescuento = comboEntity.PorcentajeDescuento;
                    }
                }

                _logger.LogDebug("Encontrados {Count} combos", combosResponse.Count);

                return combosResponse;
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
        public async Task<ComboResponse?> GetComboByIdAsync(int comboId)
        {
            try
            {
                _logger.LogDebug("Obteniendo combo ID: {ComboId}", comboId);

                var combo = await _comboRepository.GetByIdWithProductosAsync(comboId);
                if (combo == null)
                {
                    _logger.LogWarning("Combo no encontrado: {ComboId}", comboId);
                    return null;
                }

                var comboResponse = _mapper.Map<ComboResponse>(combo);
                
                // Enriquecer información específica
                comboResponse.EsComboDominicano = combo.EsComboDominicano();
                comboResponse.TiempoPreparacionTotal = combo.TiempoPreparacionTotal;
                comboResponse.Ahorro = combo.Ahorro;
                comboResponse.PorcentajeDescuento = combo.PorcentajeDescuento;

                return comboResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combo {ComboId}", comboId);
                throw;
            }
        }

        /// <summary>
        /// Sugiere combos basados en productos seleccionados
        /// </summary>
        public async Task<IEnumerable<ComboResponse>> SugerirCombosAsync(List<int> productosIds)
        {
            try
            {
                _logger.LogDebug("Sugiriendo combos para productos: {ProductosIds}", string.Join(", ", productosIds));

                if (!productosIds.Any())
                    return new List<ComboResponse>();

                var combos = await _comboRepository.GetCombosConProductosAsync(productosIds);
                var combosResponse = _mapper.Map<List<ComboResponse>>(combos);

                // Ordenar por número de productos coincidentes y descuento
                combosResponse = combosResponse
                    .OrderByDescending(c => c.ProductosCoincidentes)
                    .ThenByDescending(c => c.PorcentajeDescuento)
                    .ToList();

                _logger.LogDebug("Encontrados {Count} combos sugeridos", combosResponse.Count);

                return combosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sugerir combos");
                throw;
            }
        }

        /// <summary>
        /// Valida si un combo tiene suficiente stock
        /// </summary>
        public async Task<ComboValidationResult> ValidarStockComboAsync(int comboId, int cantidad = 1)
        {
            try
            {
                _logger.LogDebug("Validando stock de combo {ComboId} para cantidad {Cantidad}", comboId, cantidad);

                var combo = await _comboRepository.GetByIdWithProductosAsync(comboId);
                if (combo == null)
                {
                    return new ComboValidationResult
                    {
                        TieneStockSuficiente = false,
                        Mensaje = "Combo no encontrado"
                    };
                }

                var result = new ComboValidationResult
                {
                    CantidadMaximaPosible = int.MaxValue
                };

                foreach (var comboProducto in combo.ComboProductos ?? new List<ComboProducto>())
                {
                    var cantidadRequerida = comboProducto.Cantidad * cantidad;
                    var stockActual = comboProducto.Producto?.Inventario?.CantidadActual ?? 0;

                    if (stockActual < cantidadRequerida)
                    {
                        result.ProductosSinStock.Add(comboProducto.Producto?.Nombre ?? "Producto desconocido");
                        var maxPosible = stockActual / comboProducto.Cantidad;
                        result.CantidadMaximaPosible = Math.Min(result.CantidadMaximaPosible, maxPosible);
                    }
                    else if (stockActual < cantidadRequerida + STOCK_MINIMO_DEFAULT)
                    {
                        result.ProductosStockBajo.Add(comboProducto.Producto?.Nombre ?? "Producto desconocido");
                    }
                }

                result.TieneStockSuficiente = !result.ProductosSinStock.Any();

                if (result.TieneStockSuficiente)
                {
                    result.Mensaje = result.ProductosStockBajo.Any() 
                        ? $"Disponible, pero algunos productos tienen stock bajo: {string.Join(", ", result.ProductosStockBajo)}"
                        : "Stock suficiente para el combo";
                }
                else
                {
                    result.Mensaje = $"Stock insuficiente. Productos agotados: {string.Join(", ", result.ProductosSinStock)}";
                    if (result.CantidadMaximaPosible > 0)
                    {
                        result.Mensaje += $". Cantidad máxima posible: {result.CantidadMaximaPosible}";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar stock de combo {ComboId}", comboId);
                throw;
            }
        }

        // ============================================================================
        // CONTROL DE INVENTARIO Y STOCK
        // ============================================================================

        /// <summary>
        /// Verifica disponibilidad de un producto específico
        /// </summary>
        public async Task<StockValidationResult> VerificarDisponibilidadAsync(int productoId, int cantidadRequerida = 1)
        {
            try
            {
                _logger.LogDebug("Verificando disponibilidad producto {ProductoId} cantidad {Cantidad}", productoId, cantidadRequerida);

                var producto = await _productoRepository.GetByIdWithInventarioAsync(productoId);
                if (producto == null)
                {
                    return new StockValidationResult
                    {
                        EstaDisponible = false,
                        Mensaje = "Producto no encontrado"
                    };
                }

                var stockActual = producto.Inventario?.CantidadActual ?? 0;
                var cantidadDisponible = Math.Max(0, stockActual);

                var result = new StockValidationResult
                {
                    StockActual = stockActual,
                    CantidadRequerida = cantidadRequerida,
                    CantidadDisponible = cantidadDisponible,
                    EstaDisponible = stockActual >= cantidadRequerida && producto.EstaDisponible
                };

                if (result.EstaDisponible)
                {
                    result.Mensaje = stockActual < cantidadRequerida + STOCK_MINIMO_DEFAULT 
                        ? "Disponible, pero stock bajo"
                        : "Stock suficiente";
                }
                else
                {
                    result.Mensaje = stockActual <= 0 
                        ? "Producto agotado"
                        : $"Stock insuficiente. Disponible: {stockActual}, Requerido: {cantidadRequerida}";

                    // Buscar alternativas
                    if (producto.CategoriaId.HasValue)
                    {
                        var alternativas = await _productoRepository.GetAlternativasAsync(productoId, producto.CategoriaId.Value);
                        result.Alternativas = _mapper.Map<List<ProductoResponse>>(alternativas.Take(3));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        public async Task<IEnumerable<ProductoStockAlert>> GetProductosStockBajoAsync(int? umbralMinimo = null)
        {
            try
            {
                var umbral = umbralMinimo ?? STOCK_MINIMO_DEFAULT;
                _logger.LogDebug("Obteniendo productos con stock bajo (umbral: {Umbral})", umbral);

                var productos = await _inventarioRepository.GetProductosStockBajoAsync(umbral);

                var alertas = productos.Select(p => new ProductoStockAlert
                {
                    ProductoId = p.ProductoID,
                    NombreProducto = p.Nombre,
                    StockActual = p.Inventario?.CantidadActual ?? 0,
                    StockMinimo = p.Inventario?.PuntoReorden ?? umbral,
                    Categoria = p.Categoria?.Nombre,
                    Urgencia = (p.Inventario?.CantidadActual ?? 0) switch
                    {
                        0 => "Crítico",
                        <= 2 => "Muy Bajo",
                        <= 5 => "Bajo",
                        _ => "Advertencia"
                    }
                }).OrderBy(a => a.StockActual);

                _logger.LogInformation("Encontrados {Count} productos con stock bajo", alertas.Count());

                return alertas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock bajo");
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
                _logger.LogInformation("Actualizando stock producto {ProductoId} a {NuevaCantidad} por usuario {UsuarioId}", 
                    productoId, nuevaCantidad, usuarioId);

                var inventario = await _inventarioRepository.GetByProductoIdAsync(productoId);
                if (inventario == null)
                {
                    _logger.LogWarning("Inventario no encontrado para producto {ProductoId}", productoId);
                    return false;
                }

                var stockAnterior = inventario.CantidadActual;
                inventario.CantidadActual = nuevaCantidad;
                inventario.FechaModificacion = DateTime.UtcNow;

                await _inventarioRepository.UpdateAsync(inventario);

                _logger.LogInformation("Stock actualizado exitosamente. Producto {ProductoId}: {StockAnterior} → {StockNuevo}", 
                    productoId, stockAnterior, nuevaCantidad);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock producto {ProductoId}", productoId);
                return false;
            }
        }

        /// <summary>
        /// Reserva stock temporalmente para una orden
        /// </summary>
        public async Task<StockReservationResult> ReservarStockTemporalAsync(List<ItemStock> items)
        {
            try
            {
                _logger.LogDebug("Reservando stock temporal para {Count} items", items.Count);

                var result = new StockReservationResult
                {
                    ReservaId = Guid.NewGuid().ToString(),
                    VenceEn = DateTime.UtcNow.AddMinutes(RESERVA_DURACION_MINUTOS)
                };

                // Validar disponibilidad de todos los items
                foreach (var item in items)
                {
                    var validacion = await VerificarDisponibilidadAsync(item.ProductoId, item.Cantidad);
                    if (!validacion.EstaDisponible)
                    {
                        var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                        result.ProductosNoDisponibles.Add(producto?.Nombre ?? $"Producto ID: {item.ProductoId}");
                    }
                }

                if (result.ProductosNoDisponibles.Any())
                {
                    result.Exitoso = false;
                    result.Mensaje = $"Productos no disponibles: {string.Join(", ", result.ProductosNoDisponibles)}";
                    return result;
                }

                // TODO: Implementar lógica de reserva temporal en cache/memoria
                // Por ahora simulamos una reserva exitosa
                result.Exitoso = true;
                result.Mensaje = $"Stock reservado temporalmente por {RESERVA_DURACION_MINUTOS} minutos";

                _logger.LogInformation("Stock reservado temporalmente: {ReservaId}", result.ReservaId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reservar stock temporal");
                return new StockReservationResult
                {
                    Exitoso = false,
                    Mensaje = "Error interno al reservar stock"
                };
            }
        }

        /// <summary>
        /// Confirma reserva de stock (descuenta del inventario)
        /// </summary>
        public async Task<bool> ConfirmarReservaStockAsync(string reservaId)
        {
            try
            {
                _logger.LogInformation("Confirmando reserva de stock: {ReservaId}", reservaId);

                // TODO: Implementar lógica de confirmación con base de datos
                // Por ahora simulamos confirmación exitosa
                
                _logger.LogInformation("Reserva de stock confirmada: {ReservaId}", reservaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar reserva de stock {ReservaId}", reservaId);
                return false;
            }
        }

        /// <summary>
        /// Libera reserva de stock (devuelve al inventario)
        /// </summary>
        public async Task<bool> LiberarReservaStockAsync(string reservaId)
        {
            try
            {
                _logger.LogInformation("Liberando reserva de stock: {ReservaId}", reservaId);

                // TODO: Implementar lógica de liberación
                // Por ahora simulamos liberación exitosa

                _logger.LogInformation("Reserva de stock liberada: {ReservaId}", reservaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar reserva de stock {ReservaId}", reservaId);
                return false;
            }
        }

        // ============================================================================
        // GESTIÓN DE PRODUCTOS (CRUD)
        // ============================================================================

        /// <summary>
        /// Crea un nuevo producto en el menú
        /// </summary>
        public async Task<ProductoResponse> CrearProductoAsync(CrearProductoRequest crearProductoRequest, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando nuevo producto: {Nombre} por usuario {UsuarioId}", 
                    crearProductoRequest.Nombre, usuarioId);

                // Validar que no existe producto con el mismo nombre
                var existeProducto = await _productoRepository.ExistsByNombreAsync(crearProductoRequest.Nombre);
                if (existeProducto)
                {
                    throw new InvalidOperationException($"Ya existe un producto con el nombre: {crearProductoRequest.Nombre}");
                }

                var nuevoProducto = _mapper.Map<Producto>(crearProductoRequest);
                nuevoProducto.FechaCreacion = DateTime.UtcNow;
                nuevoProducto.Estado = true;

                var productoCreado = await _productoRepository.CreateAsync(nuevoProducto);

                // Crear inventario inicial
                var inventario = new Inventario
                {
                    ProductoID = productoCreado.ProductoID,
                    CantidadActual = crearProductoRequest.StockInicial ?? 0,
                    PuntoReorden = STOCK_MINIMO_DEFAULT,
                    FechaCreacion = DateTime.UtcNow,
                    FechaModificacion = DateTime.UtcNow
                };

                await _inventarioRepository.CreateAsync(inventario);

                _logger.LogInformation("Producto creado exitosamente: {ProductoId} - {Nombre}", 
                    productoCreado.ProductoID, productoCreado.Nombre);

                return _mapper.Map<ProductoResponse>(productoCreado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto: {Nombre}", crearProductoRequest.Nombre);
                throw;
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        public async Task<ProductoResponse?> ActualizarProductoAsync(int productoId, ActualizarProductoRequest actualizarProductoRequest, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando producto {ProductoId} por usuario {UsuarioId}", productoId, usuarioId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado para actualizar: {ProductoId}", productoId);
                    return null;
                }

                // Actualizar campos modificables
                if (!string.IsNullOrWhiteSpace(actualizarProductoRequest.Nombre))
                    producto.Nombre = actualizarProductoRequest.Nombre;
                
                if (!string.IsNullOrWhiteSpace(actualizarProductoRequest.Descripcion))
                    producto.Descripcion = actualizarProductoRequest.Descripcion;
                
                if (actualizarProductoRequest.Precio.HasValue)
                    producto.Precio = actualizarProductoRequest.Precio.Value;
                
                if (actualizarProductoRequest.CategoriaId.HasValue)
                    producto.CategoriaId = actualizarProductoRequest.CategoriaId.Value;
                
                if (actualizarProductoRequest.Estado.HasValue)
                    producto.Estado = actualizarProductoRequest.Estado.Value;
                
                if (actualizarProductoRequest.TiempoPreparacion.HasValue)
                    producto.TiempoPreparacion = actualizarProductoRequest.TiempoPreparacion.Value;

                producto.FechaModificacion = DateTime.UtcNow;

                var productoActualizado = await _productoRepository.UpdateAsync(producto);

                _logger.LogInformation("Producto actualizado exitosamente: {ProductoId}", productoId);

                return _mapper.Map<ProductoResponse>(productoActualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Desactiva un producto
        /// </summary>
        public async Task<bool> DesactivarProductoAsync(int productoId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Desactivando producto {ProductoId} por usuario {UsuarioId}", productoId, usuarioId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                    return false;

                producto.Estado = false;
                producto.FechaModificacion = DateTime.UtcNow;

                await _productoRepository.UpdateAsync(producto);

                _logger.LogInformation("Producto desactivado: {ProductoId} - {Nombre}", productoId, producto.Nombre);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar producto {ProductoId}", productoId);
                return false;
            }
        }

        /// <summary>
        /// Reactiva un producto previamente desactivado
        /// </summary>
        public async Task<bool> ReactivarProductoAsync(int productoId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Reactivando producto {ProductoId} por usuario {UsuarioId}", productoId, usuarioId);

                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto == null)
                    return false;

                producto.Estado = true;
                producto.FechaModificacion = DateTime.UtcNow;

                await _productoRepository.UpdateAsync(producto);

                _logger.LogInformation("Producto reactivado: {ProductoId} - {Nombre}", productoId, producto.Nombre);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar producto {ProductoId}", productoId);
                return false;
            }
        }

        // ============================================================================
        // ANÁLISIS Y RECOMENDACIONES
        // ============================================================================

        /// <summary>
        /// Obtiene recomendaciones de productos basadas en un producto seleccionado
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetRecomendacionesAsync(int productoId, int limit = 5)
        {
            try
            {
                _logger.LogDebug("Obteniendo recomendaciones para producto {ProductoId}", productoId);

                var recomendaciones = await _productoRepository.GetRecomendacionesAsync(productoId, limit);
                var recomendacionesResponse = _mapper.Map<List<ProductoResponse>>(recomendaciones);

                _logger.LogDebug("Encontradas {Count} recomendaciones para producto {ProductoId}", 
                    recomendacionesResponse.Count, productoId);

                return recomendacionesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recomendaciones para producto {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos que típicamente se ordenan juntos
        /// </summary>
        public async Task<IEnumerable<ProductoResponse>> GetProductosComplementariosAsync(List<int> productosIds)
        {
            try
            {
                _logger.LogDebug("Obteniendo productos complementarios para: {ProductosIds}", string.Join(", ", productosIds));

                if (!productosIds.Any())
                    return new List<ProductoResponse>();

                var complementarios = await _productoRepository.GetComplementariosAsync(productosIds);
                var complementariosResponse = _mapper.Map<List<ProductoResponse>>(complementarios);

                _logger.LogDebug("Encontrados {Count} productos complementarios", complementariosResponse.Count);

                return complementariosResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos complementarios");
                throw;
            }
        }

        /// <summary>
        /// Calcula el precio total con descuentos aplicables
        /// </summary>
        public async Task<PrecioCalculationResult> CalcularPrecioTotalAsync(List<ItemPrecio> items, bool aplicarDescuentos = true)
        {
            try
            {
                _logger.LogDebug("Calculando precio total para {Count} items", items.Count);

                var result = new PrecioCalculationResult();

                foreach (var item in items)
                {
                    if (item.ComboId.HasValue)
                    {
                        var combo = await _comboRepository.GetByIdAsync(item.ComboId.Value);
                        result.Subtotal += (combo?.Precio ?? 0) * item.Cantidad;
                    }
                    else
                    {
                        var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                        result.Subtotal += (producto?.Precio ?? 0) * item.Cantidad;
                    }
                }

                if (aplicarDescuentos)
                {
                    // Aplicar descuentos automáticos (lógica personalizable)
                    if (result.Subtotal > 1000) // Descuento por volumen
                    {
                        var descuento = result.Subtotal * 0.05m; // 5%
                        result.Descuentos += descuento;
                        result.DescuentosDetalle.Add(new DescuentoAplicado
                        {
                            Tipo = "Volumen",
                            Descripcion = "Descuento por compra mayor a RD$ 1,000",
                            Monto = descuento,
                            Porcentaje = 5
                        });
                    }
                }

                result.Total = result.Subtotal - result.Descuentos;

                _logger.LogDebug("Precio calculado - Subtotal: {Subtotal}, Descuentos: {Descuentos}, Total: {Total}", 
                    result.Subtotal, result.Descuentos, result.Total);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular precio total");
                throw;
            }
        }

        /// <summary>
        /// Valida una orden completa antes de procesarla
        /// </summary>
        public async Task<OrdenValidationResult> ValidarOrdenAsync(List<ItemOrden> items)
        {
            try
            {
                _logger.LogDebug("Validando orden con {Count} items", items.Count);

                var result = new OrdenValidationResult();

                if (!items.Any())
                {
                    result.Errores.Add("La orden debe contener al menos un producto");
                    result.EsValida = false;
                    return result;
                }

                decimal totalEstimado = 0;
                int tiempoMaximo = 0;

                foreach (var item in items)
                {
                    if (item.ComboId.HasValue)
                    {
                        var validacionCombo = await ValidarStockComboAsync(item.ComboId.Value, item.Cantidad);
                        if (!validacionCombo.TieneStockSuficiente)
                        {
                            result.Errores.Add($"Combo no disponible: {validacionCombo.Mensaje}");
                        }

                        var combo = await _comboRepository.GetByIdAsync(item.ComboId.Value);
                        totalEstimado += (combo?.Precio ?? 0) * item.Cantidad;
                        tiempoMaximo = Math.Max(tiempoMaximo, combo?.TiempoPreparacionTotal ?? 0);
                    }
                    else
                    {
                        var validacionProducto = await VerificarDisponibilidadAsync(item.ProductoId, item.Cantidad);
                        if (!validacionProducto.EstaDisponible)
                        {
                            result.Errores.Add($"Producto no disponible: {validacionProducto.Mensaje}");
                        }

                        var producto = await _productoRepository.GetByIdAsync(item.ProductoId);
                        totalEstimado += (producto?.Precio ?? 0) * item.Cantidad;
                        tiempoMaximo = Math.Max(tiempoMaximo, producto?.TiempoPreparacion ?? 0);
                    }
                }

                result.TotalEstimado = totalEstimado;
                result.TiempoPreparacionMinutos = tiempoMaximo;
                result.EsValida = !result.Errores.Any();

                if (result.EsValida)
                {
                    result.Sugerencias.Add("Orden válida y lista para procesar");
                    
                    if (totalEstimado > 1000)
                    {
                        result.Sugerencias.Add("¡Califica para descuento por volumen!");
                    }
                }

                _logger.LogDebug("Validación de orden completada - Válida: {EsValida}, Errores: {Errores}", 
                    result.EsValida, result.Errores.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar orden");
                throw;
            }
        }

        // ============================================================================
        // CATEGORÍAS Y ORGANIZACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene todas las categorías de productos disponibles
        /// </summary>
        public async Task<IEnumerable<CategoriaResponse>> GetCategoriasAsync(bool incluirVacias = false)
        {
            try
            {
                _logger.LogDebug("Obteniendo categorías - Incluir vacías: {IncluirVacias}", incluirVacias);

                var categorias = incluirVacias
                    ? await _categoriaRepository.GetAllAsync()
                    : await _categoriaRepository.GetCategoriasConProductosAsync();

                var categoriasResponse = _mapper.Map<List<CategoriaResponse>>(categorias);

                // Marcar categorías dominicanas
                foreach (var categoria in categoriasResponse)
                {
                    categoria.EsDominicana = CATEGORIAS_DOMINICANAS.Contains(categoria.Nombre);
                }

                _logger.LogDebug("Encontradas {Count} categorías", categoriasResponse.Count);

                return categoriasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de una categoría específica
        /// </summary>
        public async Task<CategoriaStatsResult> GetEstadisticasCategoriaAsync(int categoriaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo estadísticas de categoría {CategoriaId}", categoriaId);

                var productos = await _productoRepository.GetByCategoriaAsync(categoriaId);

                var stats = new CategoriaStatsResult
                {
                    TotalProductos = productos.Count(),
                    ProductosDisponibles = productos.Count(p => p.EstaDisponible && p.Inventario?.CantidadActual > 0),
                    ProductosAgotados = productos.Count(p => p.Inventario?.CantidadActual <= 0),
                    PrecioPromedio = productos.Any() ? productos.Average(p => p.Precio) : 0,
                    PrecioMinimo = productos.Any() ? productos.Min(p => p.Precio) : 0,
                    PrecioMaximo = productos.Any() ? productos.Max(p => p.Precio) : 0
                };

                // TODO: Implementar lógica de productos más/menos vendidos cuando tengamos datos de ventas

                _logger.LogDebug("Estadísticas generadas para categoría {CategoriaId}", categoriaId);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de categoría {CategoriaId}", categoriaId);
                throw;
            }
        }
    }
}