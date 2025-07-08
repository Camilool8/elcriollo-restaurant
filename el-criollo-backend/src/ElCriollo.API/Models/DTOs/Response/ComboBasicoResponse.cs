namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta básica para combos
/// </summary>
public class ComboBasicoResponse
{
    /// <summary>
    /// ID del combo
    /// </summary>
    public int ComboID { get; set; }

    /// <summary>
    /// Nombre del combo
    /// </summary>
    public string NombreCombo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del combo
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Precio del combo
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Precio formateado en pesos dominicanos
    /// </summary>
    public string PrecioFormateado => $"RD$ {Precio:N2}";

    /// <summary>
    /// Indica si el combo está disponible
    /// </summary>
    public bool Disponible { get; set; }

    /// <summary>
    /// Lista básica de productos incluidos
    /// </summary>
    public List<ProductoComboBasico> Productos { get; set; } = new();
}

/// <summary>
/// Información básica de producto en combo
/// </summary>
public class ProductoComboBasico
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public int ProductoID { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad incluida en el combo
    /// </summary>
    public int Cantidad { get; set; }
} 