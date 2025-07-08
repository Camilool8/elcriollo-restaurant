using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Services;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;


namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador de autenticación y gestión de usuarios para El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Endpoints de autenticación, gestión de usuarios y seguridad del sistema")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // ============================================================================
        // ENDPOINTS DE AUTENTICACIÓN
        // ============================================================================

        /// <summary>
        /// Iniciar sesión en el sistema
        /// </summary>
        /// <param name="loginRequest">Credenciales del usuario</param>
        /// <returns>Token JWT y datos del usuario</returns>
        /// <response code="200">Login exitoso con token JWT</response>
        /// <response code="401">Credenciales inválidas</response>
        /// <response code="400">Datos de entrada inválidos</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Login de usuario",
            Description = "Autentica un usuario con sus credenciales y devuelve un token JWT para acceder a los endpoints protegidos",
            OperationId = "Auth.Login",
            Tags = new[] { "Autenticación" }
        )]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("Intento de login para usuario: {Username}", loginRequest.Username);

                var result = await _authService.LoginAsync(loginRequest);
                
                if (result == null)
                {
                    _logger.LogWarning("Login fallido para usuario: {Username}", loginRequest.Username);
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Credenciales inválidas",
                        Detail = "El usuario o contraseña son incorrectos",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                _logger.LogInformation("Login exitoso para usuario: {Username}", loginRequest.Username);
                
                // Registrar intento exitoso
                await _authService.LogLoginAttemptAsync(
                    loginRequest.Username, 
                    true, 
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                
                // Registrar intento fallido
                await _authService.LogLoginAttemptAsync(
                    loginRequest.Username, 
                    false, 
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Renovar token JWT usando refresh token
        /// </summary>
        /// <param name="request">Request con el token de renovación</param>
        /// <returns>Nuevos tokens JWT</returns>
        /// <response code="200">Token renovado exitosamente</response>
        /// <response code="401">Refresh token inválido o expirado</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Renovar token JWT",
            Description = "Genera un nuevo token JWT usando un refresh token válido",
            OperationId = "Auth.RefreshToken",
            Tags = new[] { "Autenticación" }
        )]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                
                if (result == null)
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Token inválido",
                        Detail = "El refresh token es inválido o ha expirado",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar token");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Cerrar sesión del usuario actual
        /// </summary>
        /// <returns>Confirmación de logout</returns>
        /// <response code="200">Logout exitoso</response>
        /// <response code="401">No autorizado</response>
        [HttpPost("logout")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Cerrar sesión",
            Description = "Invalida los tokens del usuario actual",
            OperationId = "Auth.Logout",
            Tags = new[] { "Autenticación" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MessageResponse>> Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var username = User.Identity?.Name;

                var success = await _authService.LogoutAsync(userId);

                if (success)
                {
                    _logger.LogInformation("Logout exitoso para usuario: {Username}", username);
                    return Ok(new MessageResponse 
                    { 
                        Message = "Sesión cerrada exitosamente",
                        Success = true
                    });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Error al cerrar sesión",
                    Detail = "No se pudo cerrar la sesión correctamente",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // GESTIÓN DE USUARIOS
        // ============================================================================

        /// <summary>
        /// Registrar nuevo usuario con empleado asociado
        /// </summary>
        /// <param name="createUsuarioRequest">Datos del nuevo usuario y empleado</param>
        /// <returns>Usuario creado con su empleado asociado</returns>
        /// <response code="201">Usuario y empleado creados exitosamente</response>
        /// <response code="400">Datos inválidos, usuario ya existe o cédula duplicada</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para crear usuarios</response>
        [HttpPost("register")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Registrar nuevo usuario con empleado",
            Description = "Crea un nuevo usuario y su empleado asociado en el sistema automáticamente (solo administradores)",
            OperationId = "Auth.Register",
            Tags = new[] { "Gestión de Usuarios" }
        )]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UsuarioResponse>> Register([FromBody] CreateUsuarioRequest createUsuarioRequest)
        {
            try
            {
                var createdByUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var usuario = await _authService.CreateUserAsync(createUsuarioRequest, createdByUserId);

                _logger.LogInformation("Usuario creado exitosamente: {Username}", createUsuarioRequest.Username);

                return CreatedAtAction(
                    nameof(GetUsuarioById), 
                    new { id = usuario.UsuarioId }, 
                    usuario
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al crear usuario: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Cambiar contraseña del usuario actual
        /// </summary>
        /// <param name="changePasswordRequest">Contraseña actual y nueva</param>
        /// <returns>Confirmación del cambio</returns>
        /// <response code="200">Contraseña cambiada exitosamente</response>
        /// <response code="400">Contraseña actual incorrecta o nueva contraseña inválida</response>
        /// <response code="401">No autorizado</response>
        [HttpPost("change-password")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Cambiar contraseña",
            Description = "Permite al usuario cambiar su propia contraseña",
            OperationId = "Auth.ChangePassword",
            Tags = new[] { "Gestión de Usuarios" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MessageResponse>> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var success = await _authService.ChangePasswordAsync(
                    userId, 
                    changePasswordRequest.CurrentPassword, 
                    changePasswordRequest.NewPassword
                );

                if (success)
                {
                    _logger.LogInformation("Contraseña cambiada exitosamente para usuario ID: {UserId}", userId);
                    return Ok(new MessageResponse 
                    { 
                        Message = "Contraseña actualizada exitosamente",
                        Success = true
                    });
                }

                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error al cambiar contraseña",
                    Detail = "La contraseña actual es incorrecta",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Restablecer contraseña de un usuario (solo administradores)
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="resetPasswordRequest">Nueva contraseña</param>
        /// <returns>Confirmación del restablecimiento</returns>
        /// <response code="200">Contraseña restablecida exitosamente</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Sin permisos para restablecer contraseñas</response>
        [HttpPost("{userId}/reset-password")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Restablecer contraseña de usuario",
            Description = "Permite a un administrador restablecer la contraseña de cualquier usuario",
            OperationId = "Auth.ResetPassword",
            Tags = new[] { "Gestión de Usuarios" }
        )]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<MessageResponse>> ResetPassword(
            [FromRoute] int userId, 
            [FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            try
            {
                var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var success = await _authService.ResetPasswordAsync(
                    userId, 
                    resetPasswordRequest.NewPassword, 
                    adminUserId
                );

                if (success)
                {
                    _logger.LogInformation("Contraseña restablecida para usuario ID: {UserId} por admin ID: {AdminId}", 
                        userId, adminUserId);
                    return Ok(new MessageResponse 
                    { 
                        Message = "Contraseña restablecida exitosamente",
                        Success = true
                    });
                }

                return NotFound(new ProblemDetails
                {
                    Title = "Usuario no encontrado",
                    Detail = $"No se encontró un usuario con ID {userId}",
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // ENDPOINTS INFORMATIVOS
        // ============================================================================

        /// <summary>
        /// Obtener información del usuario actual
        /// </summary>
        /// <returns>Datos del usuario autenticado</returns>
        /// <response code="200">Datos del usuario</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("me")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Obtener usuario actual",
            Description = "Devuelve la información del usuario autenticado actualmente",
            OperationId = "Auth.GetCurrentUser",
            Tags = new[] { "Información de Usuario" }
        )]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public ActionResult<CurrentUserResponse> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var username = User.Identity?.Name;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;

                return Ok(new CurrentUserResponse
                {
                    UsuarioId = userId,
                    Username = username ?? "",
                    Email = email ?? "",
                    Rol = role ?? "",
                    Claims = User.Claims.Select(c => new ClaimDto
                    {
                        Type = c.Type,
                        Value = c.Value
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Validar si un token es válido
        /// </summary>
        /// <param name="tokenRequest">Token a validar</param>
        /// <returns>Estado del token</returns>
        /// <response code="200">Token válido</response>
        /// <response code="401">Token inválido</response>
        [HttpPost("validate-token")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Validar token JWT",
            Description = "Verifica si un token JWT es válido y no ha expirado",
            OperationId = "Auth.ValidateToken",
            Tags = new[] { "Autenticación" }
        )]
        [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenValidationResponse>> ValidateToken([FromBody] ValidateTokenRequest tokenRequest)
        {
            try
            {
                var isValid = await _authService.ValidateTokenAsync(tokenRequest.Token);

                if (isValid)
                {
                    return Ok(new TokenValidationResponse
                    {
                        IsValid = true,
                        Message = "Token válido"
                    });
                }

                return Unauthorized(new ProblemDetails
                {
                    Title = "Token inválido",
                    Detail = "El token proporcionado es inválido o ha expirado",
                    Status = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Crear usuario administrador inicial
        /// </summary>
        /// <returns>Usuario administrador creado</returns>
        /// <response code="201">Admin creado exitosamente</response>
        /// <response code="400">Admin ya existe</response>
        [HttpPost("create-admin")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Crear administrador inicial",
            Description = "Crea el usuario administrador inicial del sistema (solo si no existe)",
            OperationId = "Auth.CreateAdmin",
            Tags = new[] { "Configuración Inicial" }
        )]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioResponse>> CreateAdmin()
        {
            try
            {
                var admin = await _authService.CreateAdminUserAsync();
                
                _logger.LogInformation("Usuario administrador creado exitosamente");
                
                return CreatedAtAction(
                    nameof(GetUsuarioById), 
                    new { id = admin.UsuarioId }, 
                    admin
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Error al crear admin: {Error}", ex.Message);
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Error de validación",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario administrador");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // ============================================================================
        // ENDPOINTS AUXILIARES (NO DOCUMENTADOS EN SWAGGER)
        // ============================================================================

        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult<UsuarioResponse> GetUsuarioById(int id)
        {
            // Este método existe solo para el Location header de CreatedAtAction
            return NotFound();
        }
    }

    // ============================================================================
    // DTOs ADICIONALES PARA REQUESTS/RESPONSES
    // ============================================================================

    /// <summary>
    /// Request para renovar token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Refresh token actual
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para cambiar contraseña
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Contraseña actual del usuario
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña (mínimo 6 caracteres)
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para restablecer contraseña
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Nueva contraseña para el usuario
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para validar token
    /// </summary>
    public class ValidateTokenRequest
    {
        /// <summary>
        /// Token JWT a validar
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }


} 