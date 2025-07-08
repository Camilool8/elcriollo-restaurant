namespace ElCriollo.API.Models.DTOs.Response;

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