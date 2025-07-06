using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear un nuevo usuario
/// </summary>
public class CreateUsuarioRequest
{
    /// <summary>
    /// Nombre de usuario único
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números y guiones bajos")]
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "La contraseña debe contener al menos: 1 minúscula, 1 mayúscula, 1 número y 1 carácter especial")]
    public string Contrasena { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de contraseña
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(70, ErrorMessage = "El email no puede exceder 70 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Rol que se asignará al usuario
    /// </summary>
    [Required(ErrorMessage = "El rol es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido")]
    public int RolID { get; set; }

    /// <summary>
    /// ID del empleado asociado (opcional)
    /// </summary>
    public int? EmpleadoID { get; set; }

    /// <summary>
    /// Indica si el usuario debe cambiar la contraseña en el primer login
    /// </summary>
    public bool RequiereCambioContrasena { get; set; } = true;
}