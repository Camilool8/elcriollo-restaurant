namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta para transacciones de email
/// </summary>
public class EmailTransaccionResponse
{
    /// <summary>
    /// ID de la transacción de email
    /// </summary>
    public int EmailTransaccionID { get; set; }

    /// <summary>
    /// Tipo de email enviado
    /// </summary>
    public string TipoEmail { get; set; } = string.Empty;

    /// <summary>
    /// Email del destinatario
    /// </summary>
    public string EmailDestinatario { get; set; } = string.Empty;

    /// <summary>
    /// Asunto del email
    /// </summary>
    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de envío
    /// </summary>
    public DateTime FechaEnvio { get; set; }

    /// <summary>
    /// Indica si el envío fue exitoso
    /// </summary>
    public bool FueExitoso { get; set; }

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? MensajeError { get; set; }

    /// <summary>
    /// Número de intentos de envío
    /// </summary>
    public int IntentosEnvio { get; set; }

    /// <summary>
    /// Tiempo transcurrido desde el envío
    /// </summary>
    public TimeSpan TiempoTranscurrido { get; set; }

    /// <summary>
    /// Indica si requiere reintento
    /// </summary>
    public bool RequiereReintento { get; set; }
} 