using System.Linq.Expressions;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz base genérica para operaciones CRUD comunes
    /// Proporciona un contrato estándar para todos los repositorios del sistema
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a manejar</typeparam>
    public interface IBaseRepository<T> where T : class
    {
        // ============================================================================
        // OPERACIONES BÁSICAS CRUD
        // ============================================================================

        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <returns>Lista de todas las entidades</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Obtiene entidades con paginación
        /// </summary>
        /// <param name="page">Número de página (inicia en 1)</param>
        /// <param name="pageSize">Cantidad de elementos por página</param>
        /// <returns>Lista paginada de entidades</returns>
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Obtiene entidades que cumplan una condición
        /// </summary>
        /// <param name="predicate">Expresión de filtro</param>
        /// <returns>Lista de entidades filtradas</returns>
        Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Obtiene la primera entidad que cumpla una condición
        /// </summary>
        /// <param name="predicate">Expresión de filtro</param>
        /// <returns>Primera entidad encontrada o null</returns>
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Crea una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a crear</param>
        /// <returns>Entidad creada con ID asignado</returns>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad con datos actualizados</param>
        /// <returns>Entidad actualizada</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Elimina una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Elimina una entidad específica
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Agrega una nueva entidad (alias de CreateAsync para compatibilidad)
        /// </summary>
        Task<T> AddAsync(T entity);

        // ============================================================================
        // OPERACIONES DE CONSULTA
        // ============================================================================

        /// <summary>
        /// Verifica si existe una entidad con el ID especificado
        /// </summary>
        /// <param name="id">ID a verificar</param>
        /// <returns>True si existe</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla la condición
        /// </summary>
        /// <param name="predicate">Expresión de filtro</param>
        /// <returns>True si existe</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Cuenta total de entidades
        /// </summary>
        /// <returns>Número total de entidades</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Cuenta entidades que cumplan una condición
        /// </summary>
        /// <param name="predicate">Expresión de filtro</param>
        /// <returns>Número de entidades que cumplen la condición</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // ============================================================================
        // OPERACIONES DE TRANSACCIONES
        // ============================================================================

        /// <summary>
        /// Guarda todos los cambios pendientes en el contexto
        /// </summary>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Ejecuta una operación dentro de una transacción
        /// </summary>
        /// <param name="operation">Operación a ejecutar</param>
        /// <returns>Resultado de la operación</returns>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);

        // ============================================================================
        // OPERACIONES AVANZADAS
        // ============================================================================

        /// <summary>
        /// Obtiene entidades con relaciones incluidas
        /// </summary>
        /// <param name="includeProperties">Propiedades de navegación a incluir</param>
        /// <returns>Lista de entidades con relaciones cargadas</returns>
        Task<IEnumerable<T>> GetWithIncludesAsync(params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Obtiene entidades con filtro y relaciones incluidas
        /// </summary>
        /// <param name="predicate">Expresión de filtro</param>
        /// <param name="includeProperties">Propiedades de navegación a incluir</param>
        /// <returns>Lista de entidades filtradas con relaciones cargadas</returns>
        Task<IEnumerable<T>> GetWhereWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Limpia el caché del contexto de Entity Framework
        /// </summary>
        void ClearChangeTracker();
    }
}