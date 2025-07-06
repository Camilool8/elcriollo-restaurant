using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los roles de usuario en el sistema
/// </summary>
[Table("Roles")]
public class Rol
{
    /// <summary>
    /// Identificador único del rol
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RolID { get; set; }

    /// <summary>
    /// Nombre del rol (ej: Administrador, Mesero, Cajero, Recepcion)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string NombreRol { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del rol y sus responsabilidades
    /// </summary>
    [StringLength(200)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si el rol está activo en el sistema
    /// </summary>
    [Required]
    public bool Estado { get; set; } = true;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Usuarios que tienen asignado este rol
    /// </summary>
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Verifica si el rol es de tipo administrador
    /// </summary>
    public bool EsAdministrador()
    {
        return NombreRol.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene representación en string del rol
    /// </summary>
    public override string ToString()
    {
        return $"{NombreRol} ({(Estado ? "Activo" : "Inactivo")})";
    }
}