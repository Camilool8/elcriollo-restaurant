using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa las reservaciones de mesas del restaurante
/// </summary>
[Table("Reservaciones")]
public class Reservacion
{
    /// <summary>
    /// Identificador único de la reservación
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReservacionID { get; set; }

    /// <summary>
    /// Mesa reservada (FK a Mesas)
    /// </summary>
    [Required]
    [ForeignKey("Mesa")]
    public int MesaID { get; set; }

    /// <summary>
    /// Cliente que realizó la reservación (FK a Clientes)
    /// </summary>
    [Required]
    [ForeignKey("Cliente")]
    public int ClienteID { get; set; }

    /// <summary>
    /// Cantidad de personas para la reservación
    /// </summary>
    [Required]
    [Range(1, 20, ErrorMessage = "La cantidad de personas debe estar entre 1 y 20")]
    public int CantidadPersonas { get; set; }

    /// <summary>
    /// Fecha y hora de la reservación
    /// </summary>
    [Required]
    public DateTime FechaYHora { get; set; }

    /// <summary>
    /// Duración estimada de la reservación en minutos
    /// </summary>
    [Required]
    [Range(30, 480, ErrorMessage = "La duración debe estar entre 30 minutos y 8 horas")]
    public int DuracionEstimada { get; set; } = 120;

    /// <summary>
    /// Observaciones especiales de la reservación
    /// </summary>
    [StringLength(500)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Estado de la reservación (Pendiente, Confirmada, Completada, Cancelada)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Fecha y hora de creación de la reservación
    /// </summary>
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Mesa reservada
    /// </summary>
    public virtual Mesa Mesa { get; set; } = null!;

    /// <summary>
    /// Cliente que realizó la reservación
    /// </summary>
    public virtual Cliente Cliente { get; set; } = null!;

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Fecha y hora de finalización de la reservación
    /// </summary>
    [NotMapped]
    public DateTime FechaHoraFin => FechaYHora.AddMinutes(DuracionEstimada);

    /// <summary>
    /// Indica si la reservación está pendiente
    /// </summary>
    [NotMapped]
    public bool EstaPendiente => Estado == "Pendiente";

    /// <summary>
    /// Indica si la reservación está confirmada
    /// </summary>
    [NotMapped]
    public bool EstaConfirmada => Estado == "Confirmada";

    /// <summary>
    /// Indica si la reservación está completada
    /// </summary>
    [NotMapped]
    public bool EstaCompletada => Estado == "Completada";

    /// <summary>
    /// Indica si la reservación está cancelada
    /// </summary>
    [NotMapped]
    public bool EstaCancelada => Estado == "Cancelada";

    /// <summary>
    /// Indica si la reservación es para hoy
    /// </summary>
    [NotMapped]
    public bool EsParaHoy => FechaYHora.Date == DateTime.Now.Date;

    /// <summary>
    /// Indica si la reservación ya ha pasado
    /// </summary>
    [NotMapped]
    public bool YaPaso => FechaHoraFin < DateTime.Now;

    /// <summary>
    /// Indica si la reservación está activa (confirmada y en su horario)
    /// </summary>
    [NotMapped]
    public bool EstaActiva => EstaConfirmada && 
                             DateTime.Now >= FechaYHora && 
                             DateTime.Now <= FechaHoraFin;

    /// <summary>
    /// Tiempo restante hasta la reservación
    /// </summary>
    [NotMapped]
    public TimeSpan TiempoHastaReservacion => FechaYHora > DateTime.Now 
        ? FechaYHora - DateTime.Now 
        : TimeSpan.Zero;

    /// <summary>
    /// Tiempo restante de la reservación (si está activa)
    /// </summary>
    [NotMapped]
    public TimeSpan TiempoRestante => EstaActiva 
        ? FechaHoraFin - DateTime.Now 
        : TimeSpan.Zero;

    /// <summary>
    /// Indica si la reservación necesita confirmación (menos de 2 horas)
    /// </summary>
    [NotMapped]
    public bool NecesitaConfirmacion => EstaPendiente && 
                                       TiempoHastaReservacion.TotalHours <= 2;

    // ============================================================================
    // PROPIEDADES ALIAS PARA COMPATIBILIDAD
    // ============================================================================

    /// <summary>
    /// Alias para Id (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public int Id => ReservacionID;

    /// <summary>
    /// Alias para ClienteId (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public int ClienteId 
    { 
        get => ClienteID;
        set => ClienteID = value;
    }

    /// <summary>
    /// Alias para MesaId (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public int MesaId 
    { 
        get => MesaID;
        set => MesaID = value;
    }

    /// <summary>
    /// Alias para FechaHora (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public DateTime FechaHora 
    { 
        get => FechaYHora;
        set => FechaYHora = value;
    }

    /// <summary>
    /// Alias para DuracionMinutos (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public int DuracionMinutos 
    { 
        get => DuracionEstimada;
        set => DuracionEstimada = value;
    }

    /// <summary>
    /// Alias para NotasEspeciales (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string? NotasEspeciales 
    { 
        get => Observaciones;
        set => Observaciones = value;
    }

    /// <summary>
    /// Alias para ObservacionesEspeciales (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string? ObservacionesEspeciales 
    { 
        get => Observaciones;
        set => Observaciones = value;
    }

    /// <summary>
    /// Usuario que creó la reservación (para compatibilidad)
    /// </summary>
    public int? UsuarioCreacion { get; set; }

    /// <summary>
    /// Fecha de modificación (para compatibilidad)
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Verifica si puede ser modificada (menos de 2 horas antes)
    /// </summary>
    [NotMapped]
    public bool PuedeModificar => !EstaCancelada && !EstaCompletada && TiempoHastaReservacion.TotalHours > 2;

    /// <summary>
    /// Verifica si puede ser cancelada (más de 1 hora antes)
    /// </summary>
    [NotMapped]
    public bool PuedeCancelar => !EstaCancelada && !EstaCompletada && TiempoHastaReservacion.TotalHours > 1;

    /// <summary>
    /// Tiempo para llegar en minutos
    /// </summary>
    [NotMapped]
    public int? TiempoParaLlegar => TiempoHastaReservacion.TotalMinutes > 0 
        ? (int)TiempoHastaReservacion.TotalMinutes 
        : null;

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Confirma la reservación
    /// </summary>
    public void Confirmar()
    {
        if (EstaCancelada)
            throw new InvalidOperationException("No se puede confirmar una reservación cancelada");
            
        if (YaPaso)
            throw new InvalidOperationException("No se puede confirmar una reservación que ya pasó");

        Estado = "Confirmada";
    }

    /// <summary>
    /// Cancela la reservación
    /// </summary>
    public void Cancelar()
    {
        if (EstaCompletada)
            throw new InvalidOperationException("No se puede cancelar una reservación completada");

        Estado = "Cancelada";
    }

    /// <summary>
    /// Marca la reservación como completada
    /// </summary>
    public void Completar()
    {
        if (!EstaConfirmada)
            throw new InvalidOperationException("Solo se pueden completar reservaciones confirmadas");

        Estado = "Completada";
    }

    /// <summary>
    /// Verifica si la mesa puede acomodar la cantidad de personas
    /// </summary>
    public bool MesaPuedeAcomodar()
    {
        return Mesa?.Capacidad >= CantidadPersonas;
    }

    /// <summary>
    /// Verifica si hay conflicto con otra reservación
    /// </summary>
    public bool TieneConflictoHorario(IEnumerable<Reservacion> otrasReservaciones)
    {
        return otrasReservaciones.Any(r => 
            r.ReservacionID != ReservacionID &&
            r.MesaID == MesaID &&
            r.EstaConfirmada &&
            ((FechaYHora >= r.FechaYHora && FechaYHora < r.FechaHoraFin) ||
             (FechaHoraFin > r.FechaYHora && FechaHoraFin <= r.FechaHoraFin) ||
             (FechaYHora <= r.FechaYHora && FechaHoraFin >= r.FechaHoraFin)));
    }

    /// <summary>
    /// Valida que la reservación sea válida
    /// </summary>
    public List<string> ValidarReservacion()
    {
        var errores = new List<string>();

        if (FechaYHora <= DateTime.Now.AddMinutes(30))
            errores.Add("La reservación debe ser con al menos 30 minutos de anticipación");

        if (FechaYHora > DateTime.Now.AddDays(30))
            errores.Add("No se pueden hacer reservaciones con más de 30 días de anticipación");

        if (CantidadPersonas <= 0)
            errores.Add("La cantidad de personas debe ser mayor a 0");

        if (Mesa != null && CantidadPersonas > Mesa.Capacidad)
            errores.Add($"La mesa {Mesa.NumeroMesa} solo tiene capacidad para {Mesa.Capacidad} personas");

        if (DuracionEstimada < 30)
            errores.Add("La duración mínima de una reservación es 30 minutos");

        if (DuracionEstimada > 480)
            errores.Add("La duración máxima de una reservación es 8 horas");

        return errores;
    }

    /// <summary>
    /// Obtiene el formato de hora para mostrar
    /// </summary>
    public string ObtenerHorarioFormateado()
    {
        return $"{FechaYHora:dd/MM/yyyy HH:mm} - {FechaHoraFin:HH:mm}";
    }

    /// <summary>
    /// Obtiene la duración en formato legible
    /// </summary>
    public string ObtenerDuracionFormateada()
    {
        var horas = DuracionEstimada / 60;
        var minutos = DuracionEstimada % 60;

        if (horas > 0 && minutos > 0)
            return $"{horas}h {minutos}m";
        else if (horas > 0)
            return $"{horas}h";
        else
            return $"{minutos}m";
    }

    /// <summary>
    /// Representación en string de la reservación
    /// </summary>
    public override string ToString()
    {
        var cliente = Cliente?.NombreCompleto ?? "Cliente desconocido";
        var mesa = Mesa?.NumeroMesa.ToString() ?? "Mesa desconocida";
        var horario = ObtenerHorarioFormateado();
        
        return $"{cliente} - Mesa {mesa} - {horario} ({CantidadPersonas} personas) - {Estado}";
    }
}