namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta para productos del menú
/// </summary>
public class ProductoResponse
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
    /// Descripción del producto
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Categoría a la que pertenece el producto
    /// </summary>
    public CategoriaBasicaResponse? Categoria { get; set; }

    /// <summary>
    /// Precio formateado
    /// </summary>
    public string Precio { get; set; } = string.Empty;

    /// <summary>
    /// Precio numérico para cálculos
    /// </summary>
    public decimal PrecioNumerico { get; set; }

    /// <summary>
    /// Tiempo de preparación formateado
    /// </summary>
    public string TiempoPreparacion { get; set; } = string.Empty;

    /// <summary>
    /// URL de la imagen
    /// </summary>
    public string? Imagen { get; set; }

    /// <summary>
    /// Disponibilidad del producto
    /// </summary>
    public bool EstaDisponible { get; set; }

    /// <summary>
    /// Información de inventario
    /// </summary>
    public InventarioBasicoResponse? Inventario { get; set; }

    /// <summary>
    /// Indica si es un plato típico dominicano
    /// </summary>
    public bool EsPlatoDominicano { get; set; }

    /// <summary>
    /// Información nutricional básica
    /// </summary>
    public object? InformacionNutricional { get; set; }
}