using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los clientes del restaurante
/// </summary>
[Table("Clientes")]
public class Cliente
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClienteID { get; set; }

    /// <summary>
    /// Cédula de identidad del cliente (opcional para clientes eventuales)
    /// </summary>
    [StringLength(16)]
    public string? Cedula { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del cliente
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto del cliente
    /// </summary>
    [StringLength(50)]
    public string? Telefono { get; set; }

    /// <summary>
    /// Email del cliente para envío de facturas y confirmaciones
    /// </summary>
    [StringLength(70)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Fecha de registro del cliente en el sistema
    /// </summary>
    [Required]
    public DateTime FechaRegistro { get; set; } = DateTime.Now.Date;

    /// <summary>
    /// Indica si el cliente está activo en el sistema
    /// </summary>
    [Required]
    public bool Estado { get; set; } = true;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Mesas asignadas al cliente (historial)
    /// </summary>
    public virtual ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();

    /// <summary>
    /// Reservaciones realizadas por el cliente
    /// </summary>
    public virtual ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();

    /// <summary>
    /// Órdenes del cliente
    /// </summary>
    public virtual ICollection<Orden> Ordenes { get; set; } = new List<Orden>();

    /// <summary>
    /// Facturas del cliente
    /// </summary>
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellido}";

    /// <summary>
    /// Indica si el cliente es frecuente (más de 5 órdenes)
    /// </summary>
    [NotMapped]
    public bool EsClienteFrecuente => Ordenes?.Count >= 5;

    /// <summary>
    /// Total de órdenes del cliente
    /// </summary>
    [NotMapped]
    public int TotalOrdenes => Ordenes?.Count ?? 0;

    /// <summary>
    /// Fecha de última visita
    /// </summary>
    [NotMapped]
    public DateTime? UltimaVisita => Ordenes?
        .Where(o => o.FechaCreacion != default)
        .Max(o => o.FechaCreacion);

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Verifica si la cédula tiene el formato correcto dominicano
    /// </summary>
    public bool EsCedulaValida()
    {
        if (string.IsNullOrEmpty(Cedula))
            return true; // La cédula es opcional

        // Remover guiones si los tiene
        var cedulaLimpia = Cedula.Replace("-", "");
        
        // Debe tener exactamente 11 dígitos
        return cedulaLimpia.Length == 11 && cedulaLimpia.All(char.IsDigit);
    }

    /// <summary>
    /// Formatea la cédula dominicana en el formato estándar
    /// </summary>
    public string? FormatearCedula()
    {
        if (string.IsNullOrEmpty(Cedula) || Cedula.Length != 11)
            return Cedula;

        // Formato dominicano: 001-1234567-8
        return $"{Cedula.Substring(0, 3)}-{Cedula.Substring(3, 7)}-{Cedula.Substring(10, 1)}";
    }

    /// <summary>
    /// Verifica si el cliente tiene información de contacto completa
    /// </summary>
    public bool TieneContactoCompleto()
    {
        return !string.IsNullOrEmpty(Telefono) || !string.IsNullOrEmpty(Email);
    }

    /// <summary>
    /// Calcula el total gastado por el cliente
    /// </summary>
    public decimal CalcularTotalGastado()
    {
        return Facturas?.Where(f => f.Estado == "Pagada").Sum(f => f.Total) ?? 0;
    }

    /// <summary>
    /// Obtiene la categoría del cliente basada en su frecuencia
    /// </summary>
    public string ObtenerCategoriaCliente()
    {
        var totalOrdenes = TotalOrdenes;
        return totalOrdenes switch
        {
            >= 20 => "VIP",
            >= 10 => "Frecuente",
            >= 5 => "Regular",
            _ => "Nuevo"
        };
    }

    /// <summary>
    /// Verifica si el cliente puede recibir notificaciones por email
    /// </summary>
    public bool PuedeRecibirEmails()
    {
        return !string.IsNullOrEmpty(Email) && Estado;
    }

    /// <summary>
    /// Representación en string del cliente
    /// </summary>
    public override string ToString()
    {
        var categoria = ObtenerCategoriaCliente();
        var contacto = TieneContactoCompleto() ? "Con contacto" : "Sin contacto";
        return $"{NombreCompleto} - {categoria} ({contacto})";
    }
}