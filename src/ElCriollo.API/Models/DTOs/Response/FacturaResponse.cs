namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para facturas
/// </summary>
public class FacturaResponse
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
    /// ID de la orden facturada
    /// </summary>
    public int OrdenID { get; set; }

    /// <summary>
    /// Cliente de la factura
    /// </summary>
    public ClienteBasicoResponse Cliente { get; set; } = null!;

    /// <summary>
    /// Empleado que procesó la factura
    /// </summary>
    public EmpleadoBasicoResponse Empleado { get; set; } = null!;

    /// <summary>
    /// Mesa donde se originó la factura (si aplica)
    /// </summary>
    public MesaBasicaResponse? Mesa { get; set; }

    /// <summary>
    /// Fecha y hora de la factura
    /// </summary>
    public DateTime FechaFactura { get; set; }

    /// <summary>
    /// Subtotal formateado
    /// </summary>
    public string Subtotal { get; set; } = string.Empty;

    /// <summary>
    /// Impuesto (ITBIS) formateado
    /// </summary>
    public string Impuesto { get; set; } = string.Empty;

    /// <summary>
    /// Descuento aplicado formateado
    /// </summary>
    public string Descuento { get; set; } = string.Empty;

    /// <summary>
    /// Propina formateada
    /// </summary>
    public string Propina { get; set; } = string.Empty;

    /// <summary>
    /// Total formateado
    /// </summary>
    public string Total { get; set; } = string.Empty;

    /// <summary>
    /// Método de pago utilizado
    /// </summary>
    public string MetodoPago { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la factura
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Porcentaje de impuesto aplicado
    /// </summary>
    public decimal PorcentajeImpuesto { get; set; }

    /// <summary>
    /// Porcentaje de descuento aplicado
    /// </summary>
    public decimal PorcentajeDescuento { get; set; }

    /// <summary>
    /// Porcentaje de propina
    /// </summary>
    public decimal PorcentajePropina { get; set; }

    /// <summary>
    /// Observaciones de pago
    /// </summary>
    public string? ObservacionesPago { get; set; }
} 