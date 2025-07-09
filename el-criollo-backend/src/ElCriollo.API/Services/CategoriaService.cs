using Microsoft.Extensions.Logging;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services;

/// <summary>
/// Servicio para la gestión de categorías
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ILogger<CategoriaService> _logger;

    public CategoriaService(
        ICategoriaRepository categoriaRepository,
        ILogger<CategoriaService> logger)
    {
        _categoriaRepository = categoriaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las categorías
    /// </summary>
    public async Task<IEnumerable<CategoriaResponse>> GetCategoriasAsync()
    {
        try
        {
            _logger.LogDebug("Obteniendo todas las categorías");

            var categorias = await _categoriaRepository.GetCategoriasConProductosAsync();
            var categoriasResponse = categorias.Select(MapToCategoriaResponse).ToList();

            _logger.LogDebug("Se obtuvieron {Count} categorías", categoriasResponse.Count);
            return categoriasResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías");
            throw;
        }
    }

    /// <summary>
    /// Obtiene una categoría por ID
    /// </summary>
    public async Task<CategoriaResponse?> GetCategoriaByIdAsync(int categoriaId)
    {
        try
        {
            _logger.LogDebug("Obteniendo categoría con ID: {CategoriaId}", categoriaId);

            var categoria = await _categoriaRepository.GetByIdWithProductosAsync(categoriaId);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada con ID: {CategoriaId}", categoriaId);
                return null;
            }

            var categoriaResponse = MapToCategoriaResponse(categoria);
            _logger.LogDebug("Categoría obtenida exitosamente: {CategoriaNombre}", categoria.Nombre);
            return categoriaResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría con ID: {CategoriaId}", categoriaId);
            throw;
        }
    }

    /// <summary>
    /// Crea una nueva categoría
    /// </summary>
    public async Task<CategoriaResponse> CrearCategoriaAsync(CrearCategoriaRequest request, int usuarioId)
    {
        try
        {
            _logger.LogDebug("Creando nueva categoría: {Nombre}. Usuario: {UsuarioId}", request.Nombre, usuarioId);

            // Validar que no exista una categoría con el mismo nombre
            if (await _categoriaRepository.ExistePorNombreAsync(request.Nombre))
            {
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{request.Nombre}'");
            }

            var categoria = new Categoria
            {
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                Estado = true
            };

            await _categoriaRepository.AddAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            _logger.LogInformation("Categoría creada exitosamente: {CategoriaNombre}", categoria.Nombre);
            return MapToCategoriaResponse(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría: {Nombre}", request.Nombre);
            throw;
        }
    }

    /// <summary>
    /// Actualiza una categoría existente
    /// </summary>
    public async Task<CategoriaResponse?> ActualizarCategoriaAsync(int categoriaId, ActualizarCategoriaRequest request, int usuarioId)
    {
        try
        {
            _logger.LogDebug("Actualizando categoría {CategoriaId}. Usuario: {UsuarioId}", categoriaId, usuarioId);

            var categoria = await _categoriaRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada para actualizar: {CategoriaId}", categoriaId);
                return null;
            }

            // Validar que no exista otra categoría con el mismo nombre
            if (await _categoriaRepository.ExistePorNombreAsync(request.Nombre, categoriaId))
            {
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{request.Nombre}'");
            }

            categoria.Nombre = request.Nombre.Trim();
            categoria.Descripcion = request.Descripcion?.Trim();
            categoria.Estado = request.Estado;

            await _categoriaRepository.SaveChangesAsync();

            _logger.LogInformation("Categoría actualizada exitosamente: {CategoriaNombre}", categoria.Nombre);
            return MapToCategoriaResponse(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría {CategoriaId}", categoriaId);
            throw;
        }
    }

    /// <summary>
    /// Elimina una categoría
    /// </summary>
    public async Task<bool> EliminarCategoriaAsync(int categoriaId, int usuarioId)
    {
        try
        {
            _logger.LogDebug("Eliminando categoría {CategoriaId}. Usuario: {UsuarioId}", categoriaId, usuarioId);

            var categoria = await _categoriaRepository.GetByIdWithProductosAsync(categoriaId);
            if (categoria == null)
            {
                _logger.LogWarning("Categoría no encontrada para eliminar: {CategoriaId}", categoriaId);
                return false;
            }

            // Verificar si la categoría tiene productos
            if (await _categoriaRepository.TieneProductosAsync(categoriaId))
            {
                throw new InvalidOperationException("No se puede eliminar una categoría que tiene productos asignados");
            }

            await _categoriaRepository.DeleteAsync(categoriaId);
            await _categoriaRepository.SaveChangesAsync();

            _logger.LogInformation("Categoría eliminada exitosamente: {CategoriaNombre}", categoria.Nombre);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar categoría {CategoriaId}", categoriaId);
            throw;
        }
    }

    /// <summary>
    /// Valida que una categoría puede ser creada
    /// </summary>
    public async Task<ValidacionCategoriaResult> ValidarCategoriaAsync(CrearCategoriaRequest request)
    {
        var resultado = new ValidacionCategoriaResult { EsValido = true };

        try
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                resultado.EsValido = false;
                resultado.Errores.Add("El nombre de la categoría es requerido");
            }
            else if (request.Nombre.Length > 50)
            {
                resultado.EsValido = false;
                resultado.Errores.Add("El nombre no puede exceder 50 caracteres");
            }

            // Validar descripción
            if (!string.IsNullOrWhiteSpace(request.Descripcion) && request.Descripcion.Length > 200)
            {
                resultado.EsValido = false;
                resultado.Errores.Add("La descripción no puede exceder 200 caracteres");
            }

            // Verificar si ya existe una categoría con el mismo nombre
            if (resultado.EsValido && await _categoriaRepository.ExistePorNombreAsync(request.Nombre))
            {
                resultado.EsValido = false;
                resultado.Errores.Add($"Ya existe una categoría con el nombre '{request.Nombre}'");
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar categoría");
            resultado.EsValido = false;
            resultado.Errores.Add("Error interno durante la validación");
            return resultado;
        }
    }

    /// <summary>
    /// Valida que una categoría puede ser actualizada
    /// </summary>
    public async Task<ValidacionCategoriaResult> ValidarActualizacionCategoriaAsync(int categoriaId, ActualizarCategoriaRequest request)
    {
        var resultado = new ValidacionCategoriaResult { EsValido = true };

        try
        {
            // Verificar que la categoría existe
            var categoria = await _categoriaRepository.GetByIdAsync(categoriaId);
            if (categoria == null)
            {
                resultado.EsValido = false;
                resultado.Errores.Add($"No se encontró una categoría con ID {categoriaId}");
                return resultado;
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                resultado.EsValido = false;
                resultado.Errores.Add("El nombre de la categoría es requerido");
            }
            else if (request.Nombre.Length > 50)
            {
                resultado.EsValido = false;
                resultado.Errores.Add("El nombre no puede exceder 50 caracteres");
            }

            // Validar descripción
            if (!string.IsNullOrWhiteSpace(request.Descripcion) && request.Descripcion.Length > 200)
            {
                resultado.EsValido = false;
                resultado.Errores.Add("La descripción no puede exceder 200 caracteres");
            }

            // Verificar si ya existe otra categoría con el mismo nombre
            if (resultado.EsValido && await _categoriaRepository.ExistePorNombreAsync(request.Nombre, categoriaId))
            {
                resultado.EsValido = false;
                resultado.Errores.Add($"Ya existe una categoría con el nombre '{request.Nombre}'");
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar actualización de categoría");
            resultado.EsValido = false;
            resultado.Errores.Add("Error interno durante la validación");
            return resultado;
        }
    }

    /// <summary>
    /// Mapea una entidad Categoria a CategoriaResponse
    /// </summary>
    private CategoriaResponse MapToCategoriaResponse(Categoria categoria)
    {
        // Como solo incluimos productos activos en el Include, todos los productos en la colección son activos
        var totalProductos = categoria.Productos?.Count ?? 0;
        
        return new CategoriaResponse
        {
            CategoriaID = categoria.CategoriaID,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            TotalProductos = totalProductos,
            ProductosActivos = totalProductos, // Todos los productos incluidos son activos
            RangoPrecios = categoria.ObtenerRangoPrecios(),
            Estado = categoria.Estado
        };
    }
} 