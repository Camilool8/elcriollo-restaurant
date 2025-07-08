namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta básica para facturas
/// </summary>
public class FacturaBasicaResponse
{
    /// <summary>
    /// ID de la factura
    /// </summary>
    public int FacturaID { get; set; }

    /// <summary>
    /// Número único de factura
    /// </summary>
    public string NumeroFactura { get; set; } = string.Empty;

    /// <summary>
    /// Total formateado
    /// </summary>
    public string Total { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string ClienteNombre { get; set; } = string.Empty;

    /// <summary>
    /// Fecha formateada
    /// </summary>
    public string FechaFormateada { get; set; } = string.Empty;

    /// <summary>
    /// Método de pago
    /// </summary>
    public string MetodoPago { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la factura
    /// </summary>
    public string Estado { get; set; } = string.Empty;
} 