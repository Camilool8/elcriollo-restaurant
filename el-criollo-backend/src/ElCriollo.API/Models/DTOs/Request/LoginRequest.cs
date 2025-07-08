using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para solicitud de login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Nombre de usuario para autenticación
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario quiere mantener la sesión activa
    /// </summary>
    public bool RecordarSesion { get; set; } = false;
}