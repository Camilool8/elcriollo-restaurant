using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear una nueva reservación
/// </summary>
public class CreateReservacionRequest
{
    /// <summary>
    /// Mesa que se desea reservar (opcional, se puede asignar automáticamente)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una mesa válida")]
    public int? MesaId { get; set; }

    /// <summary>
    /// Cliente que hace la reservación (opcional para walk-ins)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido")]
    public int? ClienteId { get; set; }

    /// <summary>
    /// Cantidad de personas para la reservación
    /// </summary>
    [Required(ErrorMessage = "La cantidad de personas es requerida")]
    [Range(1, 20, ErrorMessage = "La cantidad de personas debe estar entre 1 y 20")]
    public int CantidadPersonas { get; set; }

    /// <summary>
    /// Fecha y hora de la reservación
    /// </summary>
    [Required(ErrorMessage = "La fecha y hora son requeridas")]
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Duración estimada en minutos
    /// </summary>
    [Range(30, 480, ErrorMessage = "La duración debe estar entre 30 minutos y 8 horas")]
    public int? DuracionMinutos { get; set; } = 120;

    /// <summary>
    /// Observaciones especiales de la reservación
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? NotasEspeciales { get; set; }

    /// <summary>
    /// Valida que la fecha de reservación sea válida
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();

        // Usar zona horaria dominicana para validaciones
        var dominicanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
        var ahoraDominicana = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dominicanTimeZone);

        // Validar que la fecha sea futura
        if (FechaHora <= ahoraDominicana.AddMinutes(30))
        {
            errors.Add(new ValidationResult(
                "La reservación debe ser con al menos 30 minutos de anticipación",
                new[] { nameof(FechaHora) }));
        }

        // Validar que no sea muy lejana
        if (FechaHora > ahoraDominicana.AddDays(30))
        {
            errors.Add(new ValidationResult(
                "No se pueden hacer reservaciones con más de 30 días de anticipación",
                new[] { nameof(FechaHora) }));
        }

        return errors;
    }
}