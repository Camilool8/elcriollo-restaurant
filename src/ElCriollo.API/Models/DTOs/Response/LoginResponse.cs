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

/// <summary>
/// DTO de respuesta con información del usuario
/// </summary>
public class UsuarioResponse
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public int UsuarioID { get; set; }

    /// <summary>
    /// Nombre de usuario
    /// </summary>
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Rol del usuario
    /// </summary>
    public RolResponse Rol { get; set; } = null!;

    /// <summary>
    /// Empleado asociado (si aplica)
    /// </summary>
    public EmpleadoBasicoResponse? Empleado { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Último acceso al sistema
    /// </summary>
    public DateTime? UltimoAcceso { get; set; }

    /// <summary>
    /// Estado del usuario
    /// </summary>
    public bool Estado { get; set; }
}

/// <summary>
/// DTO de respuesta con información básica del rol
/// </summary>
public class RolResponse
{
    /// <summary>
    /// ID del rol
    /// </summary>
    public int RolID { get; set; }

    /// <summary>
    /// Nombre del rol
    /// </summary>
    public string NombreRol { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del rol
    /// </summary>
    public string? Descripcion { get; set; }
}

/// <summary>
/// DTO de respuesta con información básica del empleado
/// </summary>
public class EmpleadoBasicoResponse
{
    /// <summary>
    /// ID del empleado
    /// </summary>
    public int EmpleadoID { get; set; }

    /// <summary>
    /// Nombre completo del empleado
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Cédula del empleado
    /// </summary>
    public string Cedula { get; set; } = string.Empty;

    /// <summary>
    /// Email del empleado
    /// </summary>
    public string? Email { get; set; }
}