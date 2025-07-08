namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para categorías
/// </summary>
public class CategoriaResponse
{
    /// <summary>
    /// ID de la categoría
    /// </summary>
    public int CategoriaID { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Total de productos en la categoría
    /// </summary>
    public int TotalProductos { get; set; }

    /// <summary>
    /// Cantidad de productos activos/disponibles
    /// </summary>
    public int ProductosActivos { get; set; }

    /// <summary>
    /// Rango de precios de los productos (ej: "RD$ 100 - RD$ 500")
    /// </summary>
    public string RangoPrecios { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la categoría
    /// </summary>
    public bool Estado { get; set; }
} 