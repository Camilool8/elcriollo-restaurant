namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta básica para categorías
/// </summary>
public class CategoriaBasicaResponse
{
    /// <summary>
    /// ID de la categoría
    /// </summary>
    public int CategoriaID { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string NombreCategoria { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Cantidad total de productos en la categoría
    /// </summary>
    public int CantidadProductos { get; set; }

    /// <summary>
    /// Cantidad de productos disponibles/activos
    /// </summary>
    public int ProductosDisponibles { get; set; }
} 