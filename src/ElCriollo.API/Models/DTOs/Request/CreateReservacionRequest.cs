using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear una nueva reservación
/// </summary>
public class CreateReservacionRequest
{
    /// <summary>
    /// Mesa que se desea reservar
    /// </summary>
    [Required(ErrorMessage = "La mesa es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una mesa válida")]
    public int MesaID { get; set; }

    /// <summary>
    /// Cliente que hace la reservación
    /// </summary>
    [Required(ErrorMessage = "El cliente es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido")]
    public int ClienteID { get; set; }

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
    public DateTime FechaYHora { get; set; }

    /// <summary>
    /// Duración estimada en minutos
    /// </summary>
    [Range(30, 480, ErrorMessage = "La duración debe estar entre 30 minutos y 8 horas")]
    public int DuracionEstimada { get; set; } = 120;

    /// <summary>
    /// Observaciones especiales de la reservación
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Valida que la fecha de reservación sea válida
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();

        // Validar que la fecha sea futura
        if (FechaYHora <= DateTime.Now.AddMinutes(30))
        {
            errors.Add(new ValidationResult(
                "La reservación debe ser con al menos 30 minutos de anticipación",
                new[] { nameof(FechaYHora) }));
        }

        // Validar que no sea muy lejana
        if (FechaYHora > DateTime.Now.AddDays(30))
        {
            errors.Add(new ValidationResult(
                "No se pueden hacer reservaciones con más de 30 días de anticipación",
                new[] { nameof(FechaYHora) }));
        }

        // Validar horario de operación (11:00 AM - 11:00 PM)
        var hora = FechaYHora.TimeOfDay;
        if (hora < TimeSpan.FromHours(11) || hora > TimeSpan.FromHours(23))
        {
            errors.Add(new ValidationResult(
                "Las reservaciones solo se pueden hacer entre 11:00 AM y 11:00 PM",
                new[] { nameof(FechaYHora) }));
        }

        return errors;
    }
}