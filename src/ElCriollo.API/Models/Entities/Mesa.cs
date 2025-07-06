using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa las mesas del restaurante
/// </summary>
[Table("Mesas")]
public class Mesa
{
    /// <summary>
    /// Identificador único de la mesa
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MesaID { get; set; }

    /// <summary>
    /// Número de la mesa (visible para los clientes y empleados)
    /// </summary>
    [Required]
    public int NumeroMesa { get; set; }

    /// <summary>
    /// Capacidad máxima de personas que puede acomodar la mesa
    /// </summary>
    [Required]
    [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20 personas")]
    public int Capacidad { get; set; }

    /// <summary>
    /// Ubicación de la mesa en el restaurante
    /// </summary>
    [StringLength(50)]
    public string? Ubicacion { get; set; }

    /// <summary>
    /// Estado actual de la mesa (Libre, Ocupada, Reservada, Mantenimiento)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Libre";

    /// <summary>
    /// Fecha y hora de la última limpieza de la mesa
    /// </summary>
    public DateTime? FechaUltimaLimpieza { get; set; }

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Reservaciones de esta mesa
    /// </summary>
    public virtual ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();

    /// <summary>
    /// Órdenes de esta mesa
    /// </summary>
    public virtual ICollection<Orden> Ordenes { get; set; } = new List<Orden>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si la mesa está disponible
    /// </summary>
    [NotMapped]
    public bool EstaDisponible => Estado == "Libre";

    /// <summary>
    /// Indica si la mesa está ocupada
    /// </summary>
    [NotMapped]
    public bool EstaOcupada => Estado == "Ocupada";

    /// <summary>
    /// Indica si la mesa está reservada
    /// </summary>
    [NotMapped]
    public bool EstaReservada => Estado == "Reservada";

    /// <summary>
    /// Indica si la mesa está en mantenimiento
    /// </summary>
    [NotMapped]
    public bool EstaEnMantenimiento => Estado == "Mantenimiento";

    /// <summary>
    /// Orden actual de la mesa (si está ocupada)
    /// </summary>
    [NotMapped]
    public Orden? OrdenActual => Ordenes?
        .Where(o => o.Estado != "Completada" && o.Estado != "Cancelada")
        .FirstOrDefault();

    /// <summary>
    /// Reservación actual de la mesa (si está reservada)
    /// </summary>
    [NotMapped]
    public Reservacion? ReservacionActual => Reservaciones?
        .Where(r => r.Estado == "Confirmada" && 
                   r.FechaYHora <= DateTime.Now.AddHours(2) &&
                   r.FechaYHora >= DateTime.Now)
        .FirstOrDefault();

    /// <summary>
    /// Tiempo desde la última limpieza
    /// </summary>
    [NotMapped]
    public TimeSpan? TiempoDesdeLimpieza => FechaUltimaLimpieza.HasValue 
        ? DateTime.Now - FechaUltimaLimpieza.Value 
        : null;

    /// <summary>
    /// Indica si la mesa necesita limpieza (más de 4 horas)
    /// </summary>
    [NotMapped]
    public bool NecesitaLimpieza => TiempoDesdeLimpieza?.TotalHours > 4;

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Establece el estado de la mesa
    /// </summary>
    public void CambiarEstado(string nuevoEstado)
    {
        var estadosValidos = new[] { "Libre", "Ocupada", "Reservada", "Mantenimiento" };
        
        if (!estadosValidos.Contains(nuevoEstado))
            throw new ArgumentException($"Estado inválido: {nuevoEstado}");

        Estado = nuevoEstado;
    }

    /// <summary>
    /// Marca la mesa como libre
    /// </summary>
    public void Liberar()
    {
        Estado = "Libre";
    }

    /// <summary>
    /// Marca la mesa como ocupada
    /// </summary>
    public void Ocupar()
    {
        Estado = "Ocupada";
    }

    /// <summary>
    /// Marca la mesa como reservada
    /// </summary>
    public void Reservar()
    {
        Estado = "Reservada";
    }

    /// <summary>
    /// Marca la mesa en mantenimiento
    /// </summary>
    public void PonerEnMantenimiento()
    {
        Estado = "Mantenimiento";
    }

    /// <summary>
    /// Registra la limpieza de la mesa
    /// </summary>
    public void RegistrarLimpieza()
    {
        FechaUltimaLimpieza = DateTime.Now;
    }

    /// <summary>
    /// Verifica si la mesa puede acomodar un número específico de personas
    /// </summary>
    public bool PuedeAcomodar(int numeroPersonas)
    {
        return numeroPersonas <= Capacidad && EstaDisponible;
    }

    /// <summary>
    /// Obtiene el cliente actual de la mesa (si está ocupada)
    /// </summary>
    public Cliente? ObtenerClienteActual()
    {
        return OrdenActual?.Cliente;
    }

    /// <summary>
    /// Calcula el tiempo que la mesa ha estado ocupada
    /// </summary>
    public TimeSpan? TiempoOcupada()
    {
        if (!EstaOcupada || OrdenActual == null)
            return null;

        return DateTime.Now - OrdenActual.FechaCreacion;
    }

    /// <summary>
    /// Verifica si la mesa ha estado ocupada por mucho tiempo (más de 3 horas)
    /// </summary>
    public bool EstaOcupadaPorMuchoTiempo()
    {
        var tiempoOcupada = TiempoOcupada();
        return tiempoOcupada?.TotalHours > 3;
    }

    /// <summary>
    /// Obtiene una descripción completa de la mesa
    /// </summary>
    public string ObtenerDescripcion()
    {
        var descripcion = $"Mesa {NumeroMesa} ({Capacidad} personas)";
        
        if (!string.IsNullOrEmpty(Ubicacion))
            descripcion += $" - {Ubicacion}";
            
        return descripcion;
    }

    /// <summary>
    /// Representación en string de la mesa
    /// </summary>
    public override string ToString()
    {
        var descripcion = ObtenerDescripcion();
        return $"{descripcion} - {Estado}";
    }
}