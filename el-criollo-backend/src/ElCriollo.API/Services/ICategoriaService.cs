using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;

namespace ElCriollo.API.Services;

/// <summary>
/// Interfaz para el servicio de gestión de categorías
/// </summary>
public interface ICategoriaService
{
    /// <summary>
    /// Obtiene todas las categorías
    /// </summary>
    /// <returns>Lista de categorías</returns>
    Task<IEnumerable<CategoriaResponse>> GetCategoriasAsync();

    /// <summary>
    /// Obtiene una categoría por ID
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <returns>Categoría encontrada</returns>
    Task<CategoriaResponse?> GetCategoriaByIdAsync(int categoriaId);

    /// <summary>
    /// Crea una nueva categoría
    /// </summary>
    /// <param name="request">Datos de la categoría</param>
    /// <param name="usuarioId">ID del usuario que crea</param>
    /// <returns>Categoría creada</returns>
    Task<CategoriaResponse> CrearCategoriaAsync(CrearCategoriaRequest request, int usuarioId);

    /// <summary>
    /// Actualiza una categoría existente
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="request">Datos a actualizar</param>
    /// <param name="usuarioId">ID del usuario que actualiza</param>
    /// <returns>Categoría actualizada</returns>
    Task<CategoriaResponse?> ActualizarCategoriaAsync(int categoriaId, ActualizarCategoriaRequest request, int usuarioId);

    /// <summary>
    /// Elimina una categoría
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="usuarioId">ID del usuario que elimina</param>
    /// <returns>True si se eliminó exitosamente</returns>
    Task<bool> EliminarCategoriaAsync(int categoriaId, int usuarioId);

    /// <summary>
    /// Valida que una categoría puede ser creada
    /// </summary>
    /// <param name="request">Datos de la categoría</param>
    /// <returns>Resultado de validación</returns>
    Task<ValidacionCategoriaResult> ValidarCategoriaAsync(CrearCategoriaRequest request);

    /// <summary>
    /// Valida que una categoría puede ser actualizada
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="request">Datos a actualizar</param>
    /// <returns>Resultado de validación</returns>
    Task<ValidacionCategoriaResult> ValidarActualizacionCategoriaAsync(int categoriaId, ActualizarCategoriaRequest request);
}

/// <summary>
/// Resultado de validación para categorías
/// </summary>
public class ValidacionCategoriaResult
{
    public bool EsValido { get; set; }
    public List<string> Errores { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
} 