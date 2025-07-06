namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para reservaciones
/// </summary>
public class ReservacionResponse
{
    /// <summary>
    /// ID de la reservación
    /// </summary>
    public int ReservacionID { get; set; }

    /// <summary>
    /// Cliente que realizó la reservación
    /// </summary>
    public ClienteBasicoResponse Cliente { get; set; } = null!;

    /// <summary>
    /// Mesa reservada
    /// </summary>
    public MesaBasicaResponse Mesa { get; set; } = null!;

    /// <summary>
    /// Fecha y hora de la reservación
    /// </summary>
    public DateTime FechaYHora { get; set; }

    /// <summary>
    /// Cantidad de personas
    /// </summary>
    public int CantidadPersonas { get; set; }

    /// <summary>
    /// Estado de la reservación (Pendiente, Confirmada, Cancelada, etc.)
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones o solicitudes especiales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Horario formateado para mostrar
    /// </summary>
    public string Horario { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo restante hasta la reservación
    /// </summary>
    public string? TiempoHastaReservacion { get; set; }

    /// <summary>
    /// Indica si la reservación puede ser modificada
    /// </summary>
    public bool PuedeModificar { get; set; }

    /// <summary>
    /// Indica si la reservación puede ser cancelada
    /// </summary>
    public bool PuedeCancelar { get; set; }

    /// <summary>
    /// Minutos para que llegue el cliente (si ya es la hora)
    /// </summary>
    public int? TiempoParaLlegar { get; set; }

    /// <summary>
    /// Fecha de creación de la reservación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Duración estimada en minutos
    /// </summary>
    public int DuracionMinutos { get; set; } = 120;

    /// <summary>
    /// Alias para Id (compatibilidad con servicios)
    /// </summary>
    public int Id => ReservacionID;
} 