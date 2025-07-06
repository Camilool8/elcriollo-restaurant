namespace ElCriollo.API.Models.DTOs.Common
{
    /// <summary>
    /// Response genérico para mensajes
    /// </summary>
    public class MessageResponse
    {
        /// <summary>
        /// Mensaje de respuesta
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Datos adicionales (opcional)
        /// </summary>
        public object? Data { get; set; }
    }

    /// <summary>
    /// Response de validación de token
    /// </summary>
    public class TokenValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response de autenticación
    /// </summary>
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UsuarioResponse Usuario { get; set; } = new();
    }

    /// <summary>
    /// Response para usuario actual
    /// </summary>
    public class CurrentUserResponse
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public List<ClaimDto> Claims { get; set; } = new();
    }

    /// <summary>
    /// DTO para claims
    /// </summary>
    public class ClaimDto
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response de usuario
    /// </summary>
    public class UsuarioResponse
    {
        public int UsuarioId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public EmpleadoBasicoResponse? Empleado { get; set; }
    }

    /// <summary>
    /// Response básico de empleado
    /// </summary>
    public class EmpleadoBasicoResponse
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Departamento { get; set; }
    }

    /// <summary>
    /// Response básico de rol
    /// </summary>
    public class RolResponse
    {
        public int RolId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
} 