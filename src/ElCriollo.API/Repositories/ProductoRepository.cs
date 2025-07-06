using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación específica para operaciones con productos del menú
    /// Maneja el catálogo de comida dominicana del restaurante El Criollo
    /// </summary>
    public class ProductoRepository : BaseRepository<Producto>, IProductoRepository
    {
        // Lista de productos típicamente dominicanos para validaciones
        private readonly string[] _productosDominicanos = {
            "Pollo Guisado", "Res Guisada", "Cerdo Guisado", "Pescao Frito", "Chicharrón", "Pernil",
            "Mangú", "Tres Golpes", "Huevos Rancheros", "Casabe", "Yuca Hervida", "Yautía",
            "Arroz Blanco", "Habichuelas Rojas", "Moro de Guandules", "Locrio", "Ensalada Verde",
            "Tostones", "Yuca Frita", "Maduros", "Chicharrones", "Arepitas", "Bollitos de Yuca",
            "Morir Soñando", "Jugo de Chinola", "Mamajuana", "Cerveza Presidente", "Malta Morena",
            "Tres Leches", "Flan de Coco", "Majarete", "Dulce de Leche", "Cake de Ron",
            "Sancocho", "Sopa de Pollo", "Mondongo", "Asopao", "Caldo de Pollo"
        };

        public ProductoRepository(ElCriolloDbContext context, ILogger<ProductoRepository> logger)
            : base(context, logger)
        {
        }

        // ============================================================================
        // CONSULTAS POR CATEGORÍA
        // ============================================================================

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByCategoriaAsync(int categoriaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo productos por categoría ID: {CategoriaId}", categoriaId);

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.CategoriaID == categoriaId && p.Estado)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos en categoría ID: {CategoriaId}", productos.Count, categoriaId);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría ID: {CategoriaId}", categoriaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por nombre de categoría
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByNombreCategoriaAsync(string nombreCategoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreCategoria))
                {
                    _logger.LogWarning("Nombre de categoría es null o vacío");
                    return Enumerable.Empty<Producto>();
                }

                _logger.LogDebug("Obteniendo productos por nombre de categoría: {Categoria}", nombreCategoria);

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Categoria != null && p.Categoria.Nombre == nombreCategoria && p.Estado)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos en categoría: {Categoria}", productos.Count, nombreCategoria);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por nombre de categoría: {Categoria}", nombreCategoria);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos dominicanos auténticos
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosDominicanosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos dominicanos auténticos");

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => _productosDominicanos.Contains(p.Nombre) && p.Estado)
                    .OrderBy(p => p.Categoria != null ? p.Categoria.Nombre : string.Empty)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos dominicanos auténticos", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos dominicanos auténticos");
                throw;
            }
        }

        // ============================================================================
        // BÚSQUEDAS Y FILTROS
        // ============================================================================

        /// <summary>
        /// Busca productos por nombre o descripción
        /// </summary>
        public async Task<IEnumerable<Producto>> BuscarProductosAsync(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    _logger.LogWarning("Término de búsqueda es null o vacío");
                    return Enumerable.Empty<Producto>();
                }

                _logger.LogDebug("Buscando productos con término: {Termino}", termino);

                // Convertir a lista primero para evitar problemas con LINQ to Entities
                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado)
                    .ToListAsync();

                var resultados = productos.Where(p => 
                    (p.Nombre?.Contains(termino, StringComparison.OrdinalIgnoreCase) == true) || 
                    (p.Descripcion?.Contains(termino, StringComparison.OrdinalIgnoreCase) == true))
                    .OrderBy(p => p.Nombre)
                    .ToList();

                _logger.LogDebug("Se encontraron {Count} productos con término: {Termino}", resultados.Count, termino);
                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos con término: {Termino}", termino);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos en un rango de precios
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByRangoPreciosAsync(decimal precioMinimo, decimal precioMaximo)
        {
            try
            {
                if (precioMinimo > precioMaximo)
                {
                    _logger.LogWarning("Precio mínimo ({Min}) es mayor que precio máximo ({Max})", precioMinimo, precioMaximo);
                    (precioMinimo, precioMaximo) = (precioMaximo, precioMinimo);
                }

                _logger.LogDebug("Obteniendo productos en rango de precios: {Min} - {Max}", precioMinimo, precioMaximo);

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && p.Precio >= precioMinimo && p.Precio <= precioMaximo)
                    .OrderBy(p => p.Precio)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos en rango de precios: {Min} - {Max}", productos.Count, precioMinimo, precioMaximo);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por rango de precios: {Min} - {Max}", precioMinimo, precioMaximo);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por tiempo de preparación
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByTiempoPreparacionAsync(int tiempoMaximo)
        {
            try
            {
                if (tiempoMaximo < 0)
                {
                    _logger.LogWarning("Tiempo máximo ({Tiempo}) es negativo", tiempoMaximo);
                    return Enumerable.Empty<Producto>();
                }

                _logger.LogDebug("Obteniendo productos con tiempo de preparación máximo: {Tiempo} minutos", tiempoMaximo);

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && (p.TiempoPreparacion == null || p.TiempoPreparacion <= tiempoMaximo))
                    .OrderBy(p => p.TiempoPreparacion ?? 0)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos con tiempo de preparación máximo: {Tiempo} minutos", productos.Count, tiempoMaximo);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por tiempo de preparación: {Tiempo}", tiempoMaximo);
                throw;
            }
        }

        // ============================================================================
        // PRODUCTOS DISPONIBLES E INVENTARIO
        // ============================================================================

        /// <summary>
        /// Obtiene productos activos únicamente
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosActivosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos activos");

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado)
                    .OrderBy(p => p.Categoria != null ? p.Categoria.Nombre : string.Empty)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos activos", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos activos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos disponibles (activos y con inventario)
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosDisponiblesAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos disponibles (activos y con inventario)");

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && 
                           (p.Inventario == null || p.Inventario.CantidadDisponible > 0))
                    .OrderBy(p => p.Categoria != null ? p.Categoria.Nombre : string.Empty)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos disponibles", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos disponibles");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosStockBajoAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos con stock bajo");

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && 
                           p.Inventario != null && 
                           p.Inventario.CantidadDisponible <= p.Inventario.CantidadMinima)
                    .OrderBy(p => p.Inventario!.CantidadDisponible)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos con stock bajo", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock bajo");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un producto está disponible para ordenar
        /// </summary>
        public async Task<bool> EstaDisponibleAsync(int productoId)
        {
            try
            {
                var producto = await _dbSet
                    .Include(p => p.Inventario)
                    .FirstOrDefaultAsync(p => p.ProductoID == productoId);

                if (producto == null || !producto.Estado)
                    return false;

                // Si no tiene inventario asociado, se considera disponible
                if (producto.Inventario == null)
                    return true;

                // Si tiene inventario, verificar que haya cantidad disponible
                return producto.Inventario.CantidadDisponible > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad del producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene los productos más vendidos
        /// </summary>
        public async Task<IEnumerable<object>> GetProductosMasVendidosAsync(int limite = 10, int dias = 30)
        {
            try
            {
                if (limite <= 0) limite = 10;
                if (dias <= 0) dias = 30;

                _logger.LogDebug("Obteniendo productos más vendidos. Límite: {Limite}, Días: {Dias}", limite, dias);

                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                var productosVendidos = await _context.DetalleOrdenes
                    .Include(d => d.Producto)
                    .Include(d => d.Orden)
                    .Where(d => d.ProductoID != null && 
                           d.Producto != null &&
                           d.Orden != null &&
                           d.Orden.FechaCreacion >= fechaLimite &&
                           d.Orden.Estado != "Cancelada")
                    .GroupBy(d => new { d.ProductoID, d.Producto!.Nombre })
                    .Select(g => new 
                    {
                        ProductoId = g.Key.ProductoID,
                        Nombre = g.Key.Nombre,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(limite)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} productos más vendidos", productosVendidos.Count);
                return productosVendidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos menos vendidos o sin ventas
        /// </summary>
        public async Task<IEnumerable<object>> GetProductosMenosVendidosAsync(int limite = 10, int dias = 30)
        {
            try
            {
                if (limite <= 0) limite = 10;
                if (dias <= 0) dias = 30;

                _logger.LogDebug("Obteniendo productos menos vendidos. Límite: {Limite}, Días: {Dias}", limite, dias);

                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                var productosVendidos = await _context.DetalleOrdenes
                    .Include(d => d.Producto)
                    .Include(d => d.Orden)
                    .Where(d => d.ProductoID != null && 
                           d.Producto != null &&
                           d.Orden != null &&
                           d.Orden.FechaCreacion >= fechaLimite &&
                           d.Orden.Estado != "Cancelada")
                    .GroupBy(d => new { d.ProductoID, d.Producto!.Nombre })
                    .Select(g => new 
                    {
                        ProductoId = g.Key.ProductoID,
                        Nombre = g.Key.Nombre,
                        CantidadVendida = g.Sum(d => d.Cantidad)
                    })
                    .OrderBy(p => p.CantidadVendida)
                    .Take(limite)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} productos menos vendidos", productosVendidos.Count);
                return productosVendidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos menos vendidos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos recomendados del día
        /// </summary>
        public async Task<IEnumerable<Producto>> GetRecomendacionesDelDiaAsync(int limite = 5)
        {
            try
            {
                if (limite <= 0) limite = 5;

                _logger.LogDebug("Obteniendo recomendaciones del día. Límite: {Limite}", limite);

                // Algoritmo simple: productos dominicanos disponibles con buenas ventas
                var fechaLimite = DateTime.UtcNow.AddDays(-7);

                var recomendaciones = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && 
                           _productosDominicanos.Contains(p.Nombre) &&
                           (p.Inventario == null || p.Inventario.CantidadDisponible > 0))
                    .OrderBy(p => Guid.NewGuid()) // Orden aleatorio
                    .Take(limite)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} recomendaciones del día", recomendaciones.Count);
                return recomendaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recomendaciones del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas generales de productos
        /// </summary>
        public async Task<object> GetEstadisticasProductosAsync()
        {
            try
            {
                var totalProductos = await _dbSet.CountAsync();
                var productosActivos = await _dbSet.CountAsync(p => p.Estado);
                var productosInactivos = totalProductos - productosActivos;
                var productosDominicanos = await _dbSet.CountAsync(p => _productosDominicanos.Contains(p.Nombre));

                var productosPorCategoria = await _dbSet
                    .Include(p => p.Categoria)
                    .GroupBy(p => p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría")
                    .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                    .ToListAsync();

                var precioPromedio = await _dbSet.Where(p => p.Estado).AverageAsync(p => p.Precio);

                return new
                {
                    TotalProductos = totalProductos,
                    ProductosActivos = productosActivos,
                    ProductosInactivos = productosInactivos,
                    ProductosDominicanos = productosDominicanos,
                    ProductosPorCategoria = productosPorCategoria,
                    PrecioPromedio = Math.Round(precioPromedio, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de productos");
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE MENÚ
        // ============================================================================

        /// <summary>
        /// Obtiene el menú completo organizado por categorías
        /// </summary>
        public async Task<object> GetMenuCompletoAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo menú completo organizado por categorías");

                var menu = await _context.Categorias
                    .Include(c => c.Productos.Where(p => p.Estado))
                    .ThenInclude(p => p.Inventario)
                    .Where(c => c.Estado)
                    .Select(c => new
                    {
                        CategoriaId = c.CategoriaID,
                        NombreCategoria = c.Nombre,
                        DescripcionCategoria = c.Descripcion,
                        Productos = c.Productos.Select(p => new
                        {
                            ProductoId = p.ProductoID,
                            Nombre = p.Nombre,
                            Descripcion = p.Descripcion,
                            Precio = p.Precio,
                            TiempoPreparacion = p.TiempoPreparacion,
                            Imagen = p.Imagen,
                            Disponible = p.Inventario == null || p.Inventario.CantidadDisponible > 0,
                            EsDominicano = _productosDominicanos.Contains(p.Nombre)
                        }).OrderBy(p => p.Nombre)
                    })
                    .OrderBy(c => c.NombreCategoria)
                    .ToListAsync();

                _logger.LogDebug("Menú completo obtenido con {Count} categorías", menu.Count);
                return menu;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener menú completo");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos para carta digital
        /// </summary>
        public async Task<IEnumerable<object>> GetCartaDigitalAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos para carta digital");

                var cartaDigital = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado)
                    .Select(p => new
                    {
                        ProductoId = p.ProductoID,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría",
                        Precio = p.Precio,
                        TiempoPreparacion = p.TiempoPreparacion,
                        Imagen = p.Imagen,
                        Disponible = p.Inventario == null || p.Inventario.CantidadDisponible > 0,
                        EsDominicano = _productosDominicanos.Contains(p.Nombre),
                        PrecioFormateado = $"RD$ {p.Precio:N2}"
                    })
                    .OrderBy(p => p.Categoria)
                    .ThenBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} productos para carta digital", cartaDigital.Count);
                return cartaDigital;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos para carta digital");
                throw;
            }
        }

        /// <summary>
        /// Activa o desactiva un producto
        /// </summary>
        public async Task<bool> CambiarEstadoProductoAsync(int productoId, bool estado)
        {
            try
            {
                _logger.LogDebug("Cambiando estado de producto ID: {ProductoId} a {Estado}", productoId, estado);

                var producto = await _dbSet.FindAsync(productoId);
                if (producto == null)
                {
                    _logger.LogWarning("Producto no encontrado para cambio de estado: {ProductoId}", productoId);
                    return false;
                }

                producto.Estado = estado;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de producto ID: {ProductoId} cambiado a {Estado}", productoId, estado);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES ESPECÍFICAS DOMINICANAS
        // ============================================================================

        /// <summary>
        /// Obtiene productos típicos para el desayuno dominicano
        /// </summary>
        public async Task<IEnumerable<Producto>> GetDesayunosDominicanosAsync()
        {
            try
            {
                var desayunosDominicanos = new[] { "Mangú", "Tres Golpes", "Huevos Rancheros", "Casabe", "Yuca Hervida" };

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && desayunosDominicanos.Contains(p.Nombre))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener desayunos dominicanos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos de almuerzo típico dominicano
        /// </summary>
        public async Task<IEnumerable<Producto>> GetAlmuerzosDominicanosAsync()
        {
            try
            {
                var almuerzosDominicanos = new[] { "Pollo Guisado", "Res Guisada", "Arroz Blanco", "Habichuelas Rojas", "Moro de Guandules" };

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && almuerzosDominicanos.Contains(p.Nombre))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener almuerzos dominicanos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene bebidas típicas dominicanas
        /// </summary>
        public async Task<IEnumerable<Producto>> GetBebidasDominicanasAsync()
        {
            try
            {
                var bebidasDominicanas = new[] { "Morir Soñando", "Jugo de Chinola", "Mamajuana", "Cerveza Presidente", "Malta Morena" };

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && bebidasDominicanas.Contains(p.Nombre))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener bebidas dominicanas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene postres típicos dominicanos
        /// </summary>
        public async Task<IEnumerable<Producto>> GetPostresDominicanosAsync()
        {
            try
            {
                var postresDominicanos = new[] { "Tres Leches", "Flan de Coco", "Majarete", "Dulce de Leche", "Cake de Ron" };

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && postresDominicanos.Contains(p.Nombre))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener postres dominicanos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene frituras típicas dominicanas
        /// </summary>
        public async Task<IEnumerable<Producto>> GetFriturasDominicanasAsync()
        {
            try
            {
                var friturasDominicanas = new[] { "Tostones", "Yuca Frita", "Maduros", "Chicharrones", "Arepitas", "Bollitos de Yuca" };

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && friturasDominicanas.Contains(p.Nombre))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener frituras dominicanas");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un producto es auténticamente dominicano
        /// </summary>
        public async Task<bool> EsProductoDominicaneoAsync(int productoId)
        {
            try
            {
                var producto = await _dbSet.FindAsync(productoId);
                return producto != null && _productosDominicanos.Contains(producto.Nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si producto ID: {ProductoId} es dominicano", productoId);
                throw;
            }
        }

        // ============================================================================
        // VALIDACIONES ESPECÍFICAS
        // ============================================================================

        /// <summary>
        /// Verifica si el nombre de un producto ya existe
        /// </summary>
        public async Task<bool> NombreProductoExisteAsync(string nombre, int? excluirProductoId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return false;
                }

                var query = _dbSet.Where(p => p.Nombre == nombre);

                if (excluirProductoId.HasValue)
                {
                    query = query.Where(p => p.ProductoID != excluirProductoId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de nombre de producto: {Nombre}", nombre);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos que necesitan restock urgente
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosRestockUrgenteAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos que necesitan restock urgente");

                var productos = await _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && 
                           p.Inventario != null && 
                           p.Inventario.CantidadDisponible <= (p.Inventario.CantidadMinima * 0.5)) // 50% del mínimo
                    .OrderBy(p => p.Inventario!.CantidadDisponible)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos que necesitan restock urgente", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos que necesitan restock urgente");
                throw;
            }
        }

        /// <summary>
        /// Busca productos por término y categoría opcional
        /// </summary>
        public async Task<IEnumerable<Producto>> BuscarProductosAsync(string? termino, int? categoriaId)
        {
            try
            {
                _logger.LogDebug("Buscando productos con término: {Termino} y categoría: {CategoriaId}", termino, categoriaId);

                var query = _dbSet
                    .Include(p => p.Categoria)
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado);

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    termino = termino.ToLower();
                    query = query.Where(p => 
                        p.Nombre.ToLower().Contains(termino) || 
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(termino)));
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(p => p.CategoriaID == categoriaId.Value);
                }

                var productos = await query
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos");
                throw;
            }
        }

        /// <summary>
        /// Agrega un nuevo producto (alias de CreateAsync)
        /// </summary>
        public new async Task<Producto> AddAsync(Producto producto)
        {
            return await CreateAsync(producto);
        }
    }
}