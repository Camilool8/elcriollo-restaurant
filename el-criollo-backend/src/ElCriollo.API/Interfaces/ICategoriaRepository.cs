using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces;

/// <summary>
/// Interfaz para el repositorio de categorías
/// </summary>
public interface ICategoriaRepository : IBaseRepository<Categoria>
{
    /// <summary>
    /// Obtiene todas las categorías activas
    /// </summary>
    /// <returns>Lista de categorías activas</returns>
    Task<IEnumerable<Categoria>> GetCategoriasActivasAsync();

    /// <summary>
    /// Obtiene una categoría por nombre
    /// </summary>
    /// <param name="nombre">Nombre de la categoría</param>
    /// <returns>Categoría encontrada o null</returns>
    Task<Categoria?> GetByNombreAsync(string nombre);

    /// <summary>
    /// Verifica si existe una categoría con el nombre especificado
    /// </summary>
    /// <param name="nombre">Nombre de la categoría</param>
    /// <param name="excludeId">ID a excluir (para actualizaciones)</param>
    /// <returns>True si existe</returns>
    Task<bool> ExistePorNombreAsync(string nombre, int? excludeId = null);

    /// <summary>
    /// Obtiene categorías con información de productos
    /// </summary>
    /// <returns>Lista de categorías con productos</returns>
    Task<IEnumerable<Categoria>> GetCategoriasConProductosAsync();

    /// <summary>
    /// Verifica si una categoría tiene productos
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <returns>True si tiene productos</returns>
    Task<bool> TieneProductosAsync(int categoriaId);

    /// <summary>
    /// Obtiene una categoría con sus productos
    /// </summary>
    /// <param name="id">ID de la categoría</param>
    /// <returns>Categoría con productos</returns>
    Task<Categoria?> GetByIdWithProductosAsync(int id);
} 