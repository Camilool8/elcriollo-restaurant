using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación base genérica para operaciones CRUD comunes
    /// Proporciona funcionalidad estándar que será heredada por todos los repositorios específicos
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a manejar</typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ElCriolloDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<BaseRepository<T>> _logger;

        public BaseRepository(ElCriolloDbContext context, ILogger<BaseRepository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<T>();
        }

        // ============================================================================
        // OPERACIONES BÁSICAS CRUD
        // ============================================================================

        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("Obteniendo entidad {EntityType} con ID: {Id}", typeof(T).Name, id);
                
                var entity = await _dbSet.FindAsync(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("No se encontró entidad {EntityType} con ID: {Id}", typeof(T).Name, id);
                }
                
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entidad {EntityType} con ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo todas las entidades {EntityType}", typeof(T).Name);
                
                var entities = await _dbSet.ToListAsync();
                
                _logger.LogDebug("Se obtuvieron {Count} entidades {EntityType}", entities.Count, typeof(T).Name);
                
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las entidades {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Obtiene entidades con paginación
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Límite máximo por seguridad

                _logger.LogDebug("Obteniendo entidades {EntityType} paginadas. Página: {Page}, Tamaño: {PageSize}", 
                    typeof(T).Name, page, pageSize);

                var entities = await _dbSet
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} entidades {EntityType} para página {Page}", 
                    entities.Count, typeof(T).Name, page);

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entidades {EntityType} paginadas", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Obtiene entidades que cumplan una condición
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Obteniendo entidades {EntityType} con filtro", typeof(T).Name);
                
                var entities = await _dbSet.Where(predicate).ToListAsync();
                
                _logger.LogDebug("Se obtuvieron {Count} entidades {EntityType} con filtro", entities.Count, typeof(T).Name);
                
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entidades {EntityType} con filtro", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Obtiene la primera entidad que cumpla una condición
        /// </summary>
        public virtual async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Obteniendo primera entidad {EntityType} con filtro", typeof(T).Name);
                
                var entity = await _dbSet.FirstOrDefaultAsync(predicate);
                
                if (entity == null)
                {
                    _logger.LogWarning("No se encontró entidad {EntityType} que cumpla el filtro", typeof(T).Name);
                }
                
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener primera entidad {EntityType} con filtro", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva entidad
        /// </summary>
        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _logger.LogDebug("Creando nueva entidad {EntityType}", typeof(T).Name);

                var entry = await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad {EntityType} creada exitosamente", typeof(T).Name);

                return entry.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entidad {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _logger.LogDebug("Actualizando entidad {EntityType}", typeof(T).Name);

                _dbSet.Update(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad {EntityType} actualizada exitosamente", typeof(T).Name);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar entidad {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Elimina una entidad por su ID
        /// </summary>
        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogDebug("Eliminando entidad {EntityType} con ID: {Id}", typeof(T).Name, id);

                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("No se encontró entidad {EntityType} con ID: {Id} para eliminar", typeof(T).Name, id);
                    return false;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad {EntityType} con ID: {Id} eliminada exitosamente", typeof(T).Name, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entidad {EntityType} con ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una entidad específica
        /// </summary>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _logger.LogDebug("Eliminando entidad {EntityType}", typeof(T).Name);

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entidad {EntityType} eliminada exitosamente", typeof(T).Name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entidad {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Agrega una nueva entidad (alias de CreateAsync para compatibilidad)
        /// </summary>
        public virtual async Task<T> AddAsync(T entity)
        {
            return await CreateAsync(entity);
        }

        // ============================================================================
        // OPERACIONES DE CONSULTA
        // ============================================================================

        /// <summary>
        /// Verifica si existe una entidad con el ID especificado
        /// </summary>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                _logger.LogDebug("Verificando existencia de entidad {EntityType} con ID: {Id}", typeof(T).Name, id);
                
                var exists = await _dbSet.FindAsync(id) != null;
                
                _logger.LogDebug("Entidad {EntityType} con ID: {Id} {Exists}", 
                    typeof(T).Name, id, exists ? "existe" : "no existe");
                
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de entidad {EntityType} con ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla la condición
        /// </summary>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Verificando existencia de entidad {EntityType} con filtro", typeof(T).Name);
                
                var exists = await _dbSet.AnyAsync(predicate);
                
                _logger.LogDebug("Entidad {EntityType} con filtro {Exists}", 
                    typeof(T).Name, exists ? "existe" : "no existe");
                
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de entidad {EntityType} con filtro", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Cuenta total de entidades
        /// </summary>
        public virtual async Task<int> CountAsync()
        {
            try
            {
                _logger.LogDebug("Contando entidades {EntityType}", typeof(T).Name);
                
                var count = await _dbSet.CountAsync();
                
                _logger.LogDebug("Total de entidades {EntityType}: {Count}", typeof(T).Name, count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar entidades {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Cuenta entidades que cumplan una condición
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogDebug("Contando entidades {EntityType} con filtro", typeof(T).Name);
                
                var count = await _dbSet.CountAsync(predicate);
                
                _logger.LogDebug("Total de entidades {EntityType} con filtro: {Count}", typeof(T).Name, count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar entidades {EntityType} con filtro", typeof(T).Name);
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES DE TRANSACCIONES
        // ============================================================================

        /// <summary>
        /// Guarda todos los cambios pendientes en el contexto
        /// </summary>
        public virtual async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.LogDebug("Guardando cambios en el contexto");
                
                var result = await _context.SaveChangesAsync();
                
                _logger.LogDebug("Se guardaron {Count} cambios en el contexto", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en el contexto");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una operación dentro de una transacción
        /// </summary>
        public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation)
        {
            try
            {
                _logger.LogDebug("Iniciando transacción para operación");

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var result = await operation();
                    await transaction.CommitAsync();
                    
                    _logger.LogDebug("Transacción completada exitosamente");
                    
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Transacción revertida debido a error");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transacción");
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES AVANZADAS
        // ============================================================================

        /// <summary>
        /// Obtiene entidades con relaciones incluidas
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetWithIncludesAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            try
            {
                _logger.LogDebug("Obteniendo entidades {EntityType} con relaciones incluidas", typeof(T).Name);

                IQueryable<T> query = _dbSet;

                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }

                var entities = await query.ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} entidades {EntityType} con relaciones incluidas", 
                    entities.Count, typeof(T).Name);

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entidades {EntityType} con relaciones incluidas", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Obtiene entidades con filtro y relaciones incluidas
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetWhereWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includeProperties)
        {
            try
            {
                _logger.LogDebug("Obteniendo entidades {EntityType} con filtro y relaciones incluidas", typeof(T).Name);

                IQueryable<T> query = _dbSet;

                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }

                var entities = await query.Where(predicate).ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} entidades {EntityType} con filtro y relaciones incluidas", 
                    entities.Count, typeof(T).Name);

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entidades {EntityType} con filtro y relaciones incluidas", typeof(T).Name);
                throw;
            }
        }
    }
}