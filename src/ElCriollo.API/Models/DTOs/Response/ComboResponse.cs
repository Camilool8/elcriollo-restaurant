namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para combos
/// </summary>
public class ComboResponse
{
    /// <summary>
    /// ID del combo
    /// </summary>
    public int ComboID { get; set; }

    /// <summary>
    /// Nombre del combo
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del combo
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio formateado del combo
    /// </summary>
    public string Precio { get; set; } = string.Empty;

    /// <summary>
    /// Precio numérico para cálculos
    /// </summary>
    public decimal PrecioNumerico { get; set; }

    /// <summary>
    /// Descuento formateado aplicado
    /// </summary>
    public string Descuento { get; set; } = string.Empty;

    /// <summary>
    /// Ahorro total formateado
    /// </summary>
    public string Ahorro { get; set; } = string.Empty;

    /// <summary>
    /// Porcentaje de descuento
    /// </summary>
    public decimal PorcentajeDescuento { get; set; }

    /// <summary>
    /// Cantidad de productos diferentes en el combo
    /// </summary>
    public int CantidadProductos { get; set; }

    /// <summary>
    /// Total de items considerando cantidades
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Disponibilidad del combo
    /// </summary>
    public bool EstaDisponible { get; set; }

    /// <summary>
    /// Tiempo de preparación estimado
    /// </summary>
    public string TiempoPreparacion { get; set; } = string.Empty;

    /// <summary>
    /// Indica si es un combo de comida dominicana
    /// </summary>
    public bool EsComboDominicano { get; set; }

    /// <summary>
    /// Estado del combo (activo/inactivo)
    /// </summary>
    public bool Estado { get; set; }

    /// <summary>
    /// Lista de productos incluidos en el combo
    /// </summary>
    public List<ComboProductoResponse> Productos { get; set; } = new List<ComboProductoResponse>();
} 