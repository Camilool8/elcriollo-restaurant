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
    }
} 