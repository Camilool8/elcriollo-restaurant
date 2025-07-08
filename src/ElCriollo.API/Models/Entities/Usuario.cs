using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los usuarios del sistema (login y autenticación)
/// </summary>
[Table("Usuarios")]
public class Usuario
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UsuarioID { get; set; }

    /// <summary>
    /// Nombre de usuario para login (único en el sistema)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string UsuarioNombre { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña hasheada del usuario (nunca almacenar en texto plano)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ContrasenaHash { get; set; } = string.Empty;

    /// <summary>
    /// Rol asignado al usuario (FK a Roles)
    /// </summary>
    [Required]
    [ForeignKey("Rol")]
    public int RolID { get; set; }

    /// <summary>
    /// Email del usuario para notificaciones
    /// </summary>
    [StringLength(70)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Fecha y hora de creación del usuario
    /// </summary>
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora del último acceso al sistema
    /// </summary>
    public DateTime? UltimoAcceso { get; set; }

    /// <summary>
    /// Indica si el usuario está activo en el sistema
    /// </summary>
    [Required]
    public bool Estado { get; set; } = true;

    /// <summary>
    /// Indica si el usuario debe cambiar su contraseña en el próximo login
    /// </summary>
    [Required]
    public bool RequiereCambioContrasena { get; set; } = false;

    /// <summary>
    /// Token de refresco para JWT
    /// </summary>
    [StringLength(500)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Fecha de expiración del refresh token
    /// </summary>
    public DateTime? RefreshTokenExpiry { get; set; }

    /// <summary>
    /// ID del empleado asociado (para facilitar el acceso)
    /// </summary>
    [ForeignKey("Empleado")]
    public int? EmpleadoID { get; set; }

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Rol asignado al usuario
    /// </summary>
    public virtual Rol Rol { get; set; } = null!;

    /// <summary>
    /// Empleado asociado a este usuario (si aplica)
    /// </summary>
    public virtual Empleado? Empleado { get; set; }

    // ============================================================================
    // PROPIEDADES ALIAS PARA COMPATIBILIDAD
    // ============================================================================

    /// <summary>
    /// Alias para Id (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public int Id => UsuarioID;

    /// <summary>
    /// Alias para Username (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string Username => UsuarioNombre;

    /// <summary>
    /// Alias para PasswordHash (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string PasswordHash 
    { 
        get => ContrasenaHash;
        set => ContrasenaHash = value;
    }

    /// <summary>
    /// Alias para EsActivo (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public bool EsActivo 
    { 
        get => Estado;
        set => Estado = value;
    }

    /// <summary>
    /// Alias para UltimoLogin (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public DateTime? UltimoLogin 
    { 
        get => UltimoAcceso;
        set => UltimoAcceso = value;
    }

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Verifica si el usuario es administrador
    /// </summary>
    public bool EsAdministrador()
    {
        return Rol?.EsAdministrador() ?? false;
    }

    /// <summary>
    /// Actualiza la fecha de último acceso
    /// </summary>
    public void ActualizarUltimoAcceso()
    {
        UltimoAcceso = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    public bool TieneRol(string nombreRol)
    {
        return Rol?.NombreRol.Equals(nombreRol, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Obtiene el nombre completo del usuario (si tiene empleado asociado)
    /// </summary>
    public string ObtenerNombreCompleto()
    {
        if (Empleado != null)
        {
            return $"{Empleado.Nombre} {Empleado.Apellido}";
        }
        return UsuarioNombre;
    }

    /// <summary>
    /// Representación en string del usuario
    /// </summary>
    public override string ToString()
    {
        return $"{UsuarioNombre} ({Rol?.NombreRol}) - {(Estado ? "Activo" : "Inactivo")}";
    }
}