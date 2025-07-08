namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para inventario
/// </summary>
public class InventarioResponse
{
    /// <summary>
    /// ID del inventario
    /// </summary>
    public int InventarioID { get; set; }

    /// <summary>
    /// ID del producto asociado
    /// </summary>
    public int ProductoID { get; set; }

    /// <summary>
    /// Información del producto
    /// </summary>
    public ProductoResponse? Producto { get; set; }

    /// <summary>
    /// Cantidad disponible actual
    /// </summary>
    public int CantidadDisponible { get; set; }

    /// <summary>
    /// Cantidad mínima requerida
    /// </summary>
    public int CantidadMinima { get; set; }

    /// <summary>
    /// Cantidad máxima permitida
    /// </summary>
    public int CantidadMaxima { get; set; }

    /// <summary>
    /// Unidad de medida (unidades, libras, etc.)
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// Nivel de stock (Crítico, Bajo, Normal, Alto)
    /// </summary>
    public string NivelStock { get; set; } = string.Empty;

    /// <summary>
    /// Color del indicador (rojo, amarillo, verde)
    /// </summary>
    public string ColorIndicador { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el stock está bajo
    /// </summary>
    public bool StockBajo { get; set; }

    /// <summary>
    /// Días estimados para reabastecer
    /// </summary>
    public int? DiasParaReabastecer { get; set; }

    /// <summary>
    /// Valor total del inventario formateado
    /// </summary>
    public string ValorInventario { get; set; } = string.Empty;

    /// <summary>
    /// Porcentaje de stock disponible
    /// </summary>
    public decimal PorcentajeStock { get; set; }

    /// <summary>
    /// Indica si necesita reabastecimiento urgente
    /// </summary>
    public bool NecesitaReabastecimiento { get; set; }
} 