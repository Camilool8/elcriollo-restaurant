using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear una nueva factura
/// </summary>
public class CrearFacturaRequest
{
    /// <summary>
    /// ID de la orden a facturar
    /// </summary>
    [Required(ErrorMessage = "El ID de la orden es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de la orden debe ser válido")]
    public int OrdenId { get; set; }

    /// <summary>
    /// Método de pago (Efectivo, Tarjeta, Transferencia)
    /// </summary>
    [StringLength(20, ErrorMessage = "El método de pago no puede exceder 20 caracteres")]
    public string? MetodoPago { get; set; } = "Efectivo";

    /// <summary>
    /// Descuento aplicado en pesos dominicanos
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "El descuento debe estar entre 0 y 999,999.99")]
    public decimal Descuento { get; set; } = 0;

    /// <summary>
    /// Propina en pesos dominicanos
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "La propina debe estar entre 0 y 999,999.99")]
    public decimal Propina { get; set; } = 0;

    /// <summary>
    /// Observaciones adicionales de pago
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
} 