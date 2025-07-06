using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Models.Entities;
using System.Threading.Tasks;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación de El Criollo
    /// Maneja toda la lógica de seguridad, JWT y gestión de usuarios
    /// </summary>
    public interface IAuthService
    {
        // ============================================================================
        // OPERACIONES DE AUTENTICACIÓN PRINCIPAL
        // ============================================================================

        /// <summary>
        /// Autentica un usuario con sus credenciales y genera tokens JWT
        /// </summary>
        /// <param name="loginRequest">Credenciales del usuario</param>
        /// <returns>Response con token JWT y datos del usuario</returns>
        Task<AuthResponse?> LoginAsync(LoginRequest loginRequest);

        /// <summary>
        /// Renueva un token JWT usando el refresh token
        /// </summary>
        /// <param name="refreshToken">Token de renovación</param>
        /// <returns>Nuevos tokens JWT</returns>
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Invalida los tokens de un usuario (logout)
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        Task<bool> LogoutAsync(int userId);

        /// <summary>
        /// Valida si un token JWT es válido y activo
        /// </summary>
        /// <param name="token">Token JWT a validar</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidateTokenAsync(string token);

        // ============================================================================
        // GESTIÓN DE USUARIOS Y ROLES
        // ============================================================================

        /// <summary>
        /// Crea el usuario administrador inicial del sistema
        /// Usuario: thecuevas0123_ | Contraseña: thepikachu0123_
        /// </summary>
        /// <returns>Usuario administrador creado</returns>
        Task<UsuarioResponse> CreateAdminUserAsync();

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        /// <param name="crearUsuarioRequest">Datos del nuevo usuario</param>
        /// <param name="createdByUserId">ID del usuario que crea (para auditoría)</param>
        /// <returns>Usuario creado</returns>
        Task<UsuarioResponse> CreateUserAsync(CreateUsuarioRequest crearUsuarioRequest, int createdByUserId);

        /// <summary>
        /// Obtiene los claims específicos de un usuario para JWT
        /// Incluye roles dominicanos y permisos especiales
        /// </summary>
        /// <param name="usuario">Usuario para generar claims</param>
        /// <returns>Lista de claims</returns>
        Task<IEnumerable<System.Security.Claims.Claim>> GetUserClaimsAsync(Usuario usuario);

        /// <summary>
        /// Valida si un usuario tiene permisos para una operación específica
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permiso">Permiso requerido</param>
        /// <returns>True si tiene el permiso</returns>
        Task<bool> ValidateUserPermissionAsync(int userId, string permiso);

        // ============================================================================
        // OPERACIONES DE SEGURIDAD AVANZADAS
        // ============================================================================

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="currentPassword">Contraseña actual</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>Resultado de la operación</returns>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        /// <summary>
        /// Restablece la contraseña de un usuario (solo administradores)
        /// </summary>
        /// <param name="userId">ID del usuario a restablecer</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <param name="adminUserId">ID del administrador que hace el cambio</param>
        /// <returns>Resultado de la operación</returns>
        Task<bool> ResetPasswordAsync(int userId, string newPassword, int adminUserId);

        /// <summary>
        /// Bloquea/desbloquea un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="isBlocked">True para bloquear, false para desbloquear</param>
        /// <param name="adminUserId">ID del administrador</param>
        /// <returns>Resultado de la operación</returns>
        Task<bool> ToggleUserBlockAsync(int userId, bool isBlocked, int adminUserId);

        // ============================================================================
        // OPERACIONES DE AUDITORÍA Y SEGURIDAD
        // ============================================================================

        /// <summary>
        /// Registra un intento de login (exitoso o fallido) para auditoría
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="success">Si fue exitoso</param>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="userAgent">User agent del navegador</param>
        /// <returns>Task</returns>
        Task LogLoginAttemptAsync(string username, bool success, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Obtiene el historial de logins de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="days">Días hacia atrás (default: 30)</param>
        /// <returns>Lista de intentos de login</returns>
        Task<IEnumerable<LoginAttempt>> GetLoginHistoryAsync(int userId, int days = 30);

        /// <summary>
        /// Valida si el sistema está configurado correctamente
        /// Verifica conexión BD, configuración JWT, usuario admin, etc.
        /// </summary>
        /// <returns>Resultado de la validación del sistema</returns>
        Task<SystemValidationResult> ValidateSystemHealthAsync();
    }

    // ============================================================================
    // MODELOS DE RESPUESTA ESPECÍFICOS DE AUTENTICACIÓN
    // ============================================================================

    /// <summary>
    /// Registro de intento de login para auditoría
    /// </summary>
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTime AttemptDate { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Resultado de validación del sistema
    /// </summary>
    public class SystemValidationResult
    {
        public bool IsHealthy { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, bool> ComponentStatus { get; set; } = new();
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}