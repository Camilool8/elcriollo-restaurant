using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación del repositorio para operaciones con inventario
    /// </summary>
    public class InventarioRepository : BaseRepository<Inventario>, IInventarioRepository
    {
        public InventarioRepository(ElCriolloDbContext context, ILogger<InventarioRepository> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Obtiene el inventario de un producto específico
        /// </summary>
        public async Task<Inventario?> GetByProductoIdAsync(int productoId)
        {
            try
            {
                _logger.LogDebug("Obteniendo inventario para producto ID: {ProductoId}", productoId);

                var inventario = await _dbSet
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.ProductoID == productoId);

                if (inventario == null)
                {
                    _logger.LogWarning("No se encontró inventario para producto ID: {ProductoId}", productoId);
                }

                return inventario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario para producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        public async Task<IEnumerable<Inventario>> GetProductosStockBajoAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos con stock bajo");

                var inventarios = await _dbSet
                    .Include(i => i.Producto)
                    .Where(i => i.CantidadDisponible <= i.CantidadMinima)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos con stock bajo", inventarios.Count);

                return inventarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock bajo");
                throw;
            }
        }

        /// <summary>
        /// Actualiza la cantidad disponible
        /// </summary>
        public async Task<bool> ActualizarCantidadAsync(int inventarioId, int cantidad)
        {
            try
            {
                _logger.LogDebug("Actualizando cantidad para inventario ID: {InventarioId}, nueva cantidad: {Cantidad}", inventarioId, cantidad);

                var inventario = await _dbSet.FindAsync(inventarioId);
                if (inventario == null)
                {
                    _logger.LogWarning("No se encontró inventario con ID: {InventarioId}", inventarioId);
                    return false;
                }

                inventario.CantidadDisponible = cantidad;
                inventario.UltimaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cantidad actualizada exitosamente para inventario ID: {InventarioId}", inventarioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cantidad para inventario ID: {InventarioId}", inventarioId);
                throw;
            }
        }

        /// <summary>
        /// Reduce el stock de un producto
        /// </summary>
        public async Task<bool> ReducirStockAsync(int productoId, int cantidad)
        {
            try
            {
                _logger.LogDebug("Reduciendo stock para producto ID: {ProductoId}, cantidad: {Cantidad}", productoId, cantidad);

                var inventario = await _dbSet.FirstOrDefaultAsync(i => i.ProductoID == productoId);
                if (inventario == null)
                {
                    _logger.LogWarning("No se encontró inventario para producto ID: {ProductoId}", productoId);
                    return false;
                }

                if (inventario.CantidadDisponible < cantidad)
                {
                    _logger.LogWarning("Stock insuficiente para producto ID: {ProductoId}. Disponible: {Disponible}, Solicitado: {Solicitado}", 
                        productoId, inventario.CantidadDisponible, cantidad);
                    return false;
                }

                inventario.CantidadDisponible -= cantidad;
                inventario.UltimaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock reducido exitosamente para producto ID: {ProductoId}", productoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reducir stock para producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Aumenta el stock de un producto
        /// </summary>
        public async Task<bool> AumentarStockAsync(int productoId, int cantidad)
        {
            try
            {
                _logger.LogDebug("Aumentando stock para producto ID: {ProductoId}, cantidad: {Cantidad}", productoId, cantidad);

                var inventario = await _dbSet.FirstOrDefaultAsync(i => i.ProductoID == productoId);
                if (inventario == null)
                {
                    _logger.LogWarning("No se encontró inventario para producto ID: {ProductoId}", productoId);
                    return false;
                }

                inventario.CantidadDisponible += cantidad;
                inventario.UltimaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock aumentado exitosamente para producto ID: {ProductoId}", productoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aumentar stock para producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene inventarios que necesitan reabastecimiento
        /// </summary>
        public async Task<IEnumerable<Inventario>> GetInventariosParaReabastecer()
        {
            try
            {
                _logger.LogDebug("Obteniendo inventarios que necesitan reabastecimiento");

                var inventarios = await _dbSet
                    .Include(i => i.Producto)
                    .Where(i => i.CantidadDisponible < i.CantidadMinima)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} inventarios que necesitan reabastecimiento", inventarios.Count);

                return inventarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventarios que necesitan reabastecimiento");
                throw;
            }
        }
    }
} 