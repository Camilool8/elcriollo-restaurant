namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta para facturas (sinónimo de FacturaResponse usado en servicios)
/// </summary>
public class FacturaDto : FacturaResponse
{
    // Hereda todas las propiedades de FacturaResponse
    // Se usa como sinónimo en los servicios para mantener compatibilidad
    
    // Propiedades numéricas adicionales para cálculos
    /// <summary>
    /// Total numérico para cálculos
    /// </summary>
    public decimal TotalNumerico { get; set; }
    
    /// <summary>
    /// Impuesto numérico para cálculos
    /// </summary>
    public decimal ImpuestoNumerico { get; set; }
    
    /// <summary>
    /// Propina numérica para cálculos
    /// </summary>
    public decimal PropinaNumerico { get; set; }
    
    /// <summary>
    /// Descuento numérico para cálculos
    /// </summary>
    public decimal DescuentoNumerico { get; set; }
    
    /// <summary>
    /// Subtotal numérico para cálculos
    /// </summary>
    public decimal SubtotalNumerico { get; set; }

    /// <summary>
    /// Fecha y hora del pago (cuando se completó el pago) - numérico para cálculos
    /// </summary>
    public DateTime? FechaPagoNumerico { get; set; }
} 