using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los empleados del restaurante
/// </summary>
[Table("Empleados")]
public class Empleado
{
    /// <summary>
    /// Identificador único del empleado
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EmpleadoID { get; set; }

    /// <summary>
    /// Cédula de identidad del empleado (documento dominicano)
    /// </summary>
    [Required]
    [StringLength(16)]
    public string Cedula { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del empleado
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del empleado
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Sexo del empleado
    /// </summary>
    [StringLength(15)]
    public string? Sexo { get; set; }

    /// <summary>
    /// Dirección de residencia del empleado
    /// </summary>
    [StringLength(100)]
    public string? Direccion { get; set; }

    /// <summary>
    /// Teléfono de contacto del empleado
    /// </summary>
    [StringLength(50)]
    public string? Telefono { get; set; }

    /// <summary>
    /// Email personal del empleado
    /// </summary>
    [StringLength(70)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Fecha de nacimiento del empleado
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Preferencias alimentarias del empleado
    /// </summary>
    [StringLength(500)]
    public string? PreferenciasComida { get; set; }

    /// <summary>
    /// Fecha de ingreso del empleado al restaurante
    /// </summary>
    [Required]
    public DateTime FechaIngreso { get; set; } = DateTime.Now.Date;

    /// <summary>
    /// Usuario del sistema asociado al empleado (opcional)
    /// </summary>
    [ForeignKey("Usuario")]
    public int? UsuarioID { get; set; }

    /// <summary>
    /// Estado del empleado (Activo/Inactivo)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Activo";

    /// <summary>
    /// Salario del empleado (RD$)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salario { get; set; }

    /// <summary>
    /// Departamento o área del empleado
    /// </summary>
    [StringLength(50)]
    public string? Departamento { get; set; }

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Usuario del sistema asociado (si el empleado puede usar el sistema)
    /// </summary>
    public virtual Usuario? Usuario { get; set; }

    /// <summary>
    /// Órdenes creadas por este empleado
    /// </summary>
    public virtual ICollection<Orden> Ordenes { get; set; } = new List<Orden>();

    /// <summary>
    /// Facturas procesadas por este empleado
    /// </summary>
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Nombre completo del empleado
    /// </summary>
    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellido}";

    /// <summary>
    /// Años de antigüedad en el restaurante
    /// </summary>
    [NotMapped]
    public int AnosAntiguedad => DateTime.Now.Year - FechaIngreso.Year;

    /// <summary>
    /// Indica si el empleado tiene acceso al sistema
    /// </summary>
    [NotMapped]
    public bool TieneAccesoSistema => UsuarioID.HasValue && Usuario?.Estado == true;

    // ============================================================================
    // PROPIEDADES ALIAS PARA COMPATIBILIDAD
    // ============================================================================

    /// <summary>
    /// Alias para Id (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public int Id => EmpleadoID;

    /// <summary>
    /// Alias para FechaContratacion (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public DateTime FechaContratacion => FechaIngreso;

    /// <summary>
    /// Alias para EsActivo (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public bool EsActivo => Estado == "Activo";

    /// <summary>
    /// RolId del usuario asociado (si existe)
    /// </summary>
    [NotMapped]
    public int? RolId => Usuario?.RolID;

    /// <summary>
    /// Teléfono formateado dominicano
    /// </summary>
    [NotMapped]
    public string TelefonoFormateado => FormatarTelefonoDominicano();

    /// <summary>
    /// Salario formateado en pesos dominicanos
    /// </summary>
    [NotMapped]
    public string SalarioFormateado => Salario.HasValue ? $"RD$ {Salario:N2}" : "No definido";

    /// <summary>
    /// Tiempo en la empresa formateado
    /// </summary>
    [NotMapped]
    public string TiempoEnEmpresa => ObtenerTiempoEnEmpresa();

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Verifica si el empleado tiene un rol específico en el sistema
    /// </summary>
    public bool TieneRol(string nombreRol)
    {
        return Usuario?.TieneRol(nombreRol) ?? false;
    }

    /// <summary>
    /// Formatea la cédula dominicana en el formato estándar
    /// </summary>
    public string FormatearCedula()
    {
        if (string.IsNullOrEmpty(Cedula) || Cedula.Length != 11)
            return Cedula;

        // Formato dominicano: 001-1234567-8
        return $"{Cedula.Substring(0, 3)}-{Cedula.Substring(3, 7)}-{Cedula.Substring(10, 1)}";
    }

    /// <summary>
    /// Verifica si la cédula tiene el formato correcto dominicano
    /// </summary>
    public bool EsCedulaValida()
    {
        if (string.IsNullOrEmpty(Cedula))
            return false;

        // Remover guiones si los tiene
        var cedulaLimpia = Cedula.Replace("-", "");
        
        // Debe tener exactamente 11 dígitos
        return cedulaLimpia.Length == 11 && cedulaLimpia.All(char.IsDigit);
    }

    /// <summary>
    /// Obtiene el rol del empleado en el sistema
    /// </summary>
    public string? ObtenerRol()
    {
        return Usuario?.Rol?.NombreRol;
    }

    /// <summary>
    /// Formatea el teléfono al estilo dominicano
    /// </summary>
    private string FormatarTelefonoDominicano()
    {
        if (string.IsNullOrEmpty(Telefono))
            return "No registrado";
            
        var telefonoLimpio = Telefono.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
        
        if (telefonoLimpio.Length == 10)
            return $"({telefonoLimpio.Substring(0, 3)}) {telefonoLimpio.Substring(3, 3)}-{telefonoLimpio.Substring(6)}";
            
        return Telefono;
    }

    /// <summary>
    /// Obtiene el tiempo en la empresa formateado
    /// </summary>
    private string ObtenerTiempoEnEmpresa()
    {
        var anos = AnosAntiguedad;
        if (anos == 0)
            return "Menos de 1 año";
        else if (anos == 1)
            return "1 año";
        else
            return $"{anos} años";
    }

    /// <summary>
    /// Representación en string del empleado
    /// </summary>
    public override string ToString()
    {
        var estado = Estado == "Activo" ? "Activo" : "Inactivo";
        var rol = ObtenerRol() ?? "Sin acceso al sistema";
        return $"{NombreCompleto} - {rol} ({estado})";
    }
}