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
                    .Where(i => i.CantidadDisponible <= i.CantidadMinima && i.CantidadDisponible > 0)
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
        /// Obtiene productos completamente agotados
        /// </summary>
        public async Task<IEnumerable<Inventario>> GetProductosAgotadosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo productos agotados");

                var inventarios = await _dbSet
                    .Include(i => i.Producto)
                    .ThenInclude(p => p.Categoria)
                    .Where(i => i.CantidadDisponible == 0)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} productos agotados", inventarios.Count);

                return inventarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos agotados");
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
                    .ThenInclude(p => p.Categoria)
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

        // ============================================================================
        // GESTIÓN DE MOVIMIENTOS
        // ============================================================================

        /// <summary>
        /// Registra una entrada de inventario con historial
        /// </summary>
        public async Task<MovimientoInventario> RegistrarEntradaAsync(int productoId, int cantidad, decimal? costoUnitario, 
            string usuario, string? proveedor = null, string? referencia = null, string? observaciones = null)
        {
            try
            {
                _logger.LogDebug("Registrando entrada: Producto {ProductoId}, Cantidad {Cantidad}", productoId, cantidad);

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Obtener o crear inventario
                var inventario = await _dbSet.FirstOrDefaultAsync(i => i.ProductoID == productoId);
                if (inventario == null)
                {
                    // Crear nuevo inventario
                    inventario = new Inventario
                    {
                        ProductoID = productoId,
                        CantidadDisponible = 0,
                        CantidadMinima = 10,
                        UltimaActualizacion = DateTime.UtcNow
                    };
                    _dbSet.Add(inventario);
                    await _context.SaveChangesAsync();
                }

                var stockAnterior = inventario.CantidadDisponible;

                // Crear movimiento
                var movimiento = MovimientoInventario.CrearEntrada(
                    productoId, cantidad, stockAnterior, costoUnitario, usuario, proveedor, referencia, observaciones);

                // Actualizar inventario
                inventario.CantidadDisponible += cantidad;
                inventario.UltimaActualizacion = DateTime.UtcNow;

                // Guardar movimiento
                _context.Set<MovimientoInventario>().Add(movimiento);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Entrada registrada exitosamente: Producto {ProductoId}, Stock: {StockAnterior} -> {StockNuevo}", 
                    productoId, stockAnterior, inventario.CantidadDisponible);

                return movimiento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar entrada para producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Registra una salida de inventario con historial
        /// </summary>
        public async Task<MovimientoInventario> RegistrarSalidaAsync(int productoId, int cantidad, string usuario, 
            string? referencia = null, string? observaciones = null)
        {
            try
            {
                _logger.LogDebug("Registrando salida: Producto {ProductoId}, Cantidad {Cantidad}", productoId, cantidad);

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Verificar inventario existente
                var inventario = await _dbSet.FirstOrDefaultAsync(i => i.ProductoID == productoId);
                if (inventario == null)
                {
                    throw new InvalidOperationException($"No existe inventario para el producto {productoId}");
                }

                if (inventario.CantidadDisponible < cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente. Disponible: {inventario.CantidadDisponible}, Solicitado: {cantidad}");
                }

                var stockAnterior = inventario.CantidadDisponible;

                // Crear movimiento
                var movimiento = MovimientoInventario.CrearSalida(
                    productoId, cantidad, stockAnterior, usuario, referencia, observaciones);

                // Actualizar inventario
                inventario.CantidadDisponible -= cantidad;
                inventario.UltimaActualizacion = DateTime.UtcNow;

                // Guardar movimiento
                _context.Set<MovimientoInventario>().Add(movimiento);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Salida registrada exitosamente: Producto {ProductoId}, Stock: {StockAnterior} -> {StockNuevo}", 
                    productoId, stockAnterior, inventario.CantidadDisponible);

                return movimiento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar salida para producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Realiza un ajuste de inventario con historial
        /// </summary>
        public async Task<MovimientoInventario> AjustarInventarioAsync(int productoId, int nuevaCantidad, string usuario, 
            string motivo, string? observaciones = null)
        {
            try
            {
                _logger.LogDebug("Ajustando inventario: Producto {ProductoId}, Nueva cantidad {NuevaCantidad}", productoId, nuevaCantidad);

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Verificar inventario existente
                var inventario = await _dbSet.FirstOrDefaultAsync(i => i.ProductoID == productoId);
                if (inventario == null)
                {
                    throw new InvalidOperationException($"No existe inventario para el producto {productoId}");
                }

                var stockAnterior = inventario.CantidadDisponible;

                // Crear movimiento
                var movimiento = MovimientoInventario.CrearAjuste(
                    productoId, stockAnterior, nuevaCantidad, usuario, motivo, observaciones);

                // Actualizar inventario
                inventario.CantidadDisponible = nuevaCantidad;
                inventario.UltimaActualizacion = DateTime.UtcNow;

                // Guardar movimiento
                _context.Set<MovimientoInventario>().Add(movimiento);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Ajuste realizado exitosamente: Producto {ProductoId}, Stock: {StockAnterior} -> {StockNuevo}", 
                    productoId, stockAnterior, nuevaCantidad);

                return movimiento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar inventario para producto ID: {ProductoId}", productoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de movimientos de inventario
        /// </summary>
        public async Task<IEnumerable<MovimientoInventario>> GetMovimientosAsync(int? productoId = null, 
            DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogDebug("Obteniendo historial de movimientos. Producto: {ProductoId}, Desde: {FechaInicio}, Hasta: {FechaFin}", 
                    productoId, fechaInicio, fechaFin);

                var query = _context.Set<MovimientoInventario>()
                    .Include(m => m.Producto)
                    .AsQueryable();

                if (productoId.HasValue)
                {
                    query = query.Where(m => m.ProductoID == productoId.Value);
                }

                if (fechaInicio.HasValue)
                {
                    query = query.Where(m => m.FechaMovimiento >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    query = query.Where(m => m.FechaMovimiento <= fechaFin.Value);
                }

                var movimientos = await query
                    .OrderByDescending(m => m.FechaMovimiento)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} movimientos", movimientos.Count);

                return movimientos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de movimientos");
                throw;
            }
        }

        // ============================================================================
        // REPORTES Y ANÁLISIS
        // ============================================================================

        /// <summary>
        /// Obtiene la valorización total del inventario
        /// </summary>
        public async Task<object> GetValorizacionInventarioAsync()
        {
            try
            {
                _logger.LogDebug("Calculando valorización del inventario");

                var inventarios = await _dbSet
                    .Include(i => i.Producto)
                    .ThenInclude(p => p.Categoria)
                    .ToListAsync();

                var valorizacion = new
                {
                    FechaCalculo = DateTime.Now,
                    ValorTotal = inventarios.Sum(i => i.CantidadDisponible * (decimal)(i.Producto?.Precio ?? 0)),
                    TotalProductos = inventarios.Count,
                    ProductosConStock = inventarios.Count(i => i.CantidadDisponible > 0),
                    ProductosAgotados = inventarios.Count(i => i.CantidadDisponible == 0),
                    ValorPorCategoria = inventarios
                        .GroupBy(i => i.Producto?.Categoria?.NombreCategoria ?? "Sin categoría")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Sum(i => i.CantidadDisponible * (decimal)(i.Producto?.Precio ?? 0))
                        ),
                    Top10ProductosMayorValor = inventarios
                        .Where(i => i.CantidadDisponible > 0)
                        .Select(i => new
                        {
                            ProductoId = i.ProductoID,
                            NombreProducto = i.Producto?.Nombre ?? "Producto",
                            CantidadDisponible = i.CantidadDisponible,
                            CostoUnitario = (decimal)(i.Producto?.Precio ?? 0),
                            ValorTotal = i.CantidadDisponible * (decimal)(i.Producto?.Precio ?? 0)
                        })
                        .OrderByDescending(x => x.ValorTotal)
                        .Take(10)
                        .ToList()
                };

                _logger.LogDebug("Valorización calculada: Valor total {ValorTotal:C}", valorizacion.ValorTotal);

                return valorizacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular valorización del inventario");
                throw;
            }
        }

        /// <summary>
        /// Obtiene análisis de rotación del inventario
        /// </summary>
        public async Task<object> GetAnalisisRotacionAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var inicio = fechaInicio ?? DateTime.Now.AddMonths(-3);
                var fin = fechaFin ?? DateTime.Now;

                _logger.LogDebug("Analizando rotación de inventario desde {Inicio} hasta {Fin}", inicio, fin);

                // Obtener movimientos del período
                var movimientos = await _context.Set<MovimientoInventario>()
                    .Include(m => m.Producto)
                    .Where(m => m.FechaMovimiento >= inicio && m.FechaMovimiento <= fin && m.TipoMovimiento == "Salida")
                    .ToListAsync();

                // Calcular rotación por producto
                var rotacionPorProducto = movimientos
                    .GroupBy(m => new { m.ProductoID, m.Producto?.Nombre })
                    .Select(g => new
                    {
                        ProductoId = g.Key.ProductoID,
                        NombreProducto = g.Key.Nombre ?? "Producto",
                        TotalVendido = Math.Abs(g.Sum(m => m.Cantidad)),
                        Frecuencia = g.Count(),
                        PromedioVentasDiarias = Math.Abs(g.Sum(m => m.Cantidad)) / (decimal)(fin - inicio).TotalDays
                    })
                    .ToList();

                var rotacionPromedio = rotacionPorProducto.Any() ? 
                    rotacionPorProducto.Average(r => r.PromedioVentasDiarias) : 0;

                var analisis = new
                {
                    FechaInicio = inicio,
                    FechaFin = fin,
                    RotacionPromedio = rotacionPromedio,
                    AltaRotacion = rotacionPorProducto
                        .Where(r => r.PromedioVentasDiarias > rotacionPromedio * 1.5m)
                        .Select(r => new
                        {
                            r.ProductoId,
                            r.NombreProducto,
                            IndiceRotacion = r.PromedioVentasDiarias,
                            VentasPromedioDiarias = r.PromedioVentasDiarias,
                            Recomendacion = "Producto de alta demanda - Mantener stock elevado"
                        })
                        .ToList(),
                    BajaRotacion = rotacionPorProducto
                        .Where(r => r.PromedioVentasDiarias < rotacionPromedio * 0.5m)
                        .Select(r => new
                        {
                            r.ProductoId,
                            r.NombreProducto,
                            IndiceRotacion = r.PromedioVentasDiarias,
                            VentasPromedioDiarias = r.PromedioVentasDiarias,
                            Recomendacion = "Producto de baja demanda - Reducir stock o promocionar"
                        })
                        .ToList(),
                    SinMovimiento = await _dbSet
                        .Include(i => i.Producto)
                        .Where(i => !movimientos.Any(m => m.ProductoID == i.ProductoID))
                        .Select(i => new
                        {
                            ProductoId = i.ProductoID,
                            NombreProducto = i.Producto!.Nombre,
                            IndiceRotacion = 0m,
                            VentasPromedioDiarias = 0m,
                            Recomendacion = "Sin ventas en el período - Considerar descontinuar o promover"
                        })
                        .ToListAsync()
                };

                _logger.LogDebug("Análisis de rotación completado. Productos analizados: {Total}", rotacionPorProducto.Count);

                return analisis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar rotación del inventario");
                throw;
            }
        }
    }
} 