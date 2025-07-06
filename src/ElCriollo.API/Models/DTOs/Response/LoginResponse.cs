using ElCriollo.API.Models.DTOs.Common;

namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta para login exitoso
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Token JWT para autenticación
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token para renovar el JWT
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de expiración del token
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public UsuarioResponse Usuario { get; set; } = null!;

    /// <summary>
    /// Indica si es el primer login y debe cambiar contraseña
    /// </summary>
    public bool RequiereCambioContrasena { get; set; }

    /// <summary>
    /// Permisos del usuario en el sistema
    /// </summary>
    public List<string> Permisos { get; set; } = new List<string>();

    /// <summary>
    /// Mensaje de bienvenida personalizado
    /// </summary>
    public string MensajeBienvenida { get; set; } = string.Empty;
}