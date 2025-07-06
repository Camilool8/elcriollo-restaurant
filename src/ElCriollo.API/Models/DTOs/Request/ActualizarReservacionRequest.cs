using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para actualizar una reservación existente
/// </summary>
public class ActualizarReservacionRequest
{
    /// <summary>
    /// Nueva fecha y hora de la reservación (opcional)
    /// </summary>
    public DateTime? FechaHora { get; set; }

    /// <summary>
    /// Nueva cantidad de personas (opcional)
    /// </summary>
    [Range(1, 20, ErrorMessage = "La cantidad de personas debe estar entre 1 y 20")]
    public int? CantidadPersonas { get; set; }

    /// <summary>
    /// ID de la nueva mesa (opcional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una mesa válida")]
    public int? MesaId { get; set; }

    /// <summary>
    /// Nuevas notas especiales o requerimientos (opcional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? NotasEspeciales { get; set; }

    /// <summary>
    /// Nueva duración estimada en minutos (opcional)
    /// </summary>
    [Range(30, 300, ErrorMessage = "La duración debe estar entre 30 y 300 minutos")]
    public int? DuracionMinutos { get; set; }
} 