using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de inventario
    /// </summary>
    public interface IInventarioRepository : IBaseRepository<Inventario>
    {
        /// <summary>
        /// Obtiene el inventario de un producto específico
        /// </summary>
        Task<Inventario?> GetByProductoIdAsync(int productoId);

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        Task<IEnumerable<Inventario>> GetProductosStockBajoAsync();

        /// <summary>
        /// Obtiene productos completamente agotados
        /// </summary>
        Task<IEnumerable<Inventario>> GetProductosAgotadosAsync();

        /// <summary>
        /// Actualiza la cantidad disponible
        /// </summary>
        Task<bool> ActualizarCantidadAsync(int inventarioId, int cantidad);

        /// <summary>
        /// Reduce el stock de un producto
        /// </summary>
        Task<bool> ReducirStockAsync(int productoId, int cantidad);

        /// <summary>
        /// Aumenta el stock de un producto
        /// </summary>
        Task<bool> AumentarStockAsync(int productoId, int cantidad);

        /// <summary>
        /// Obtiene inventarios que necesitan reabastecimiento
        /// </summary>
        Task<IEnumerable<Inventario>> GetInventariosParaReabastecer();

        // ============================================================================
        // GESTIÓN DE MOVIMIENTOS
        // ============================================================================

        /// <summary>
        /// Registra una entrada de inventario con historial
        /// </summary>
        Task<MovimientoInventario> RegistrarEntradaAsync(int productoId, int cantidad, decimal? costoUnitario, 
            string usuario, string? proveedor = null, string? referencia = null, string? observaciones = null);

        /// <summary>
        /// Registra una salida de inventario con historial
        /// </summary>
        Task<MovimientoInventario> RegistrarSalidaAsync(int productoId, int cantidad, string usuario, 
            string? referencia = null, string? observaciones = null);

        /// <summary>
        /// Realiza un ajuste de inventario con historial
        /// </summary>
        Task<MovimientoInventario> AjustarInventarioAsync(int productoId, int nuevaCantidad, string usuario, 
            string motivo, string? observaciones = null);

        /// <summary>
        /// Obtiene el historial de movimientos de inventario
        /// </summary>
        Task<IEnumerable<MovimientoInventario>> GetMovimientosAsync(int? productoId = null, 
            DateTime? fechaInicio = null, DateTime? fechaFin = null);

        // ============================================================================
        // REPORTES Y ANÁLISIS
        // ============================================================================

        /// <summary>
        /// Obtiene la valorización total del inventario
        /// </summary>
        Task<object> GetValorizacionInventarioAsync();

        /// <summary>
        /// Obtiene análisis de rotación del inventario
        /// </summary>
        Task<object> GetAnalisisRotacionAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
    }
} 