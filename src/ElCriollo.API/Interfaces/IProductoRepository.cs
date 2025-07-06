using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para operaciones con productos del menú
    /// Maneja el catálogo de comida dominicana del restaurante El Criollo
    /// </summary>
    public interface IProductoRepository : IBaseRepository<Producto>
    {
        // ============================================================================
        // CONSULTAS POR CATEGORÍA
        // ============================================================================

        /// <summary>
        /// Obtiene productos por categoría específica
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<IEnumerable<Producto>> GetByCategoriaAsync(int categoriaId);

        /// <summary>
        /// Obtiene productos por nombre de categoría
        /// </summary>
        /// <param name="nombreCategoria">Nombre de la categoría (ej: "Platos Principales")</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<IEnumerable<Producto>> GetByNombreCategoriaAsync(string nombreCategoria);

        /// <summary>
        /// Obtiene productos dominicanos auténticos
        /// Filtra solo productos que son auténtica comida dominicana
        /// </summary>
        /// <returns>Lista de productos dominicanos</returns>
        Task<IEnumerable<Producto>> GetProductosDominicanosAsync();

        // ============================================================================
        // BÚSQUEDAS Y FILTROS
        // ============================================================================

        /// <summary>
        /// Busca productos por nombre o descripción
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <returns>Lista de productos que coinciden con la búsqueda</returns>
        Task<IEnumerable<Producto>> BuscarProductosAsync(string termino);

        /// <summary>
        /// Obtiene productos en un rango de precios
        /// </summary>
        /// <param name="precioMinimo">Precio mínimo</param>
        /// <param name="precioMaximo">Precio máximo</param>
        /// <returns>Lista de productos en el rango de precios</returns>
        Task<IEnumerable<Producto>> GetByRangoPreciosAsync(decimal precioMinimo, decimal precioMaximo);

        /// <summary>
        /// Obtiene productos por tiempo de preparación
        /// </summary>
        /// <param name="tiempoMaximo">Tiempo máximo de preparación en minutos</param>
        /// <returns>Lista de productos que se preparan en el tiempo especificado o menos</returns>
        Task<IEnumerable<Producto>> GetByTiempoPreparacionAsync(int tiempoMaximo);

        // ============================================================================
        // PRODUCTOS DISPONIBLES E INVENTARIO
        // ============================================================================

        /// <summary>
        /// Obtiene productos activos únicamente
        /// </summary>
        /// <returns>Lista de productos activos</returns>
        Task<IEnumerable<Producto>> GetProductosActivosAsync();

        /// <summary>
        /// Obtiene productos disponibles (activos y con inventario)
        /// </summary>
        /// <returns>Lista de productos disponibles para ordenar</returns>
        Task<IEnumerable<Producto>> GetProductosDisponiblesAsync();

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        /// <returns>Lista de productos con inventario por debajo del mínimo</returns>
        Task<IEnumerable<Producto>> GetProductosStockBajoAsync();

        /// <summary>
        /// Verifica si un producto está disponible para ordenar
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>True si está disponible</returns>
        Task<bool> EstaDisponibleAsync(int productoId);

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene los productos más vendidos
        /// </summary>
        /// <param name="limite">Número máximo de productos a retornar</param>
        /// <param name="dias">Período en días para calcular ventas (por defecto 30)</param>
        /// <returns>Lista de productos más vendidos ordenados por cantidad</returns>
        Task<IEnumerable<object>> GetProductosMasVendidosAsync(int limite = 10, int dias = 30);

        /// <summary>
        /// Obtiene productos menos vendidos o sin ventas
        /// </summary>
        /// <param name="limite">Número máximo de productos a retornar</param>
        /// <param name="dias">Período en días para calcular ventas (por defecto 30)</param>
        /// <returns>Lista de productos menos vendidos</returns>
        Task<IEnumerable<object>> GetProductosMenosVendidosAsync(int limite = 10, int dias = 30);

        /// <summary>
        /// Obtiene productos recomendados del día
        /// Basado en ventas históricas y disponibilidad
        /// </summary>
        /// <param name="limite">Número de productos recomendados</param>
        /// <returns>Lista de productos recomendados</returns>
        Task<IEnumerable<Producto>> GetRecomendacionesDelDiaAsync(int limite = 5);

        /// <summary>
        /// Obtiene estadísticas generales de productos
        /// </summary>
        /// <returns>Objeto con estadísticas del catálogo</returns>
        Task<object> GetEstadisticasProductosAsync();

        // ============================================================================
        // GESTIÓN DE MENÚ
        // ============================================================================

        /// <summary>
        /// Obtiene el menú completo organizado por categorías
        /// </summary>
        /// <returns>Menú completo con productos agrupados por categoría</returns>
        Task<object> GetMenuCompletoAsync();

        /// <summary>
        /// Obtiene productos para carta digital
        /// Incluye solo productos activos con información completa
        /// </summary>
        /// <returns>Lista de productos formateados para carta digital</returns>
        Task<IEnumerable<object>> GetCartaDigitalAsync();

        /// <summary>
        /// Activa o desactiva un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="estado">True para activar, False para desactivar</param>
        /// <returns>True si se cambió el estado correctamente</returns>
        Task<bool> CambiarEstadoProductoAsync(int productoId, bool estado);

        // ============================================================================
        // OPERACIONES ESPECÍFICAS DOMINICANAS
        // ============================================================================

        /// <summary>
        /// Obtiene productos típicos para el desayuno dominicano
        /// </summary>
        /// <returns>Lista de productos de desayuno (mangú, tres golpes, etc.)</returns>
        Task<IEnumerable<Producto>> GetDesayunosDominicanosAsync();

        /// <summary>
        /// Obtiene productos de almuerzo típico dominicano
        /// </summary>
        /// <returns>Lista de productos de almuerzo (pollo guisado, arroz con habichuelas, etc.)</returns>
        Task<IEnumerable<Producto>> GetAlmuerzosDominicanosAsync();

        /// <summary>
        /// Obtiene bebidas típicas dominicanas
        /// </summary>
        /// <returns>Lista de bebidas dominicanas (morir soñando, mamajuana, etc.)</returns>
        Task<IEnumerable<Producto>> GetBebidasDominicanasAsync();

        /// <summary>
        /// Obtiene postres típicos dominicanos
        /// </summary>
        /// <returns>Lista de postres dominicanos (tres leches, flan de coco, etc.)</returns>
        Task<IEnumerable<Producto>> GetPostresDominicanosAsync();

        /// <summary>
        /// Obtiene frituras típicas dominicanas
        /// </summary>
        /// <returns>Lista de frituras (tostones, yuca frita, maduros, etc.)</returns>
        Task<IEnumerable<Producto>> GetFriturasDominicanasAsync();

        /// <summary>
        /// Verifica si un producto es auténticamente dominicano
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>True si es un producto típico dominicano</returns>
        Task<bool> EsProductoDominicaneoAsync(int productoId);

        // ============================================================================
        // VALIDACIONES ESPECÍFICAS
        // ============================================================================

        /// <summary>
        /// Verifica si el nombre de un producto ya existe
        /// </summary>
        /// <param name="nombre">Nombre del producto</param>
        /// <param name="excluirProductoId">ID del producto a excluir (para updates)</param>
        /// <returns>True si el nombre ya existe</returns>
        Task<bool> NombreProductoExisteAsync(string nombre, int? excluirProductoId = null);

        /// <summary>
        /// Obtiene productos que necesitan restock urgente
        /// </summary>
        /// <returns>Lista de productos con inventario crítico</returns>
        Task<IEnumerable<Producto>> GetProductosRestockUrgenteAsync();
    }
}