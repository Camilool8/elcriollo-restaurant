namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta para productos dentro de un combo
/// </summary>
public class ComboProductoResponse
{
    /// <summary>
    /// ID de la relación combo-producto
    /// </summary>
    public int ComboProductoID { get; set; }

    /// <summary>
    /// Información completa del producto
    /// </summary>
    public ProductoResponse Producto { get; set; } = null!;

    /// <summary>
    /// Cantidad de este producto en el combo
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio total de este producto en el combo
    /// </summary>
    public string PrecioTotal { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el producto está disponible
    /// </summary>
    public bool EstaDisponible { get; set; }
} 