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
    /// Categoría del producto
    /// </summary>
    public CategoriaBasicaResponse Categoria { get; set; } = null!;

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
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }
}

/// <summary>
/// DTO de respuesta básica para inventario
/// </summary>
public class InventarioBasicoResponse
{
    /// <summary>
    /// Cantidad disponible
    /// </summary>
    public int CantidadDisponible { get; set; }

    /// <summary>
    /// Nivel de stock
    /// </summary>
    public string NivelStock { get; set; } = string.Empty;

    /// <summary>
    /// Color del indicador
    /// </summary>
    public string ColorIndicador { get; set; } = string.Empty;

    /// <summary>
    /// Indica si tiene stock bajo
    /// </summary>
    public bool StockBajo { get; set; }
}