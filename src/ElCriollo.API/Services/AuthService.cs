using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using BCrypt.Net;
using ElCriollo.API.Configuration;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.DTOs.Common;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación para El Criollo
    /// Maneja JWT, seguridad, validaciones y operaciones específicas dominicanas
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        // Configuración específica para El Criollo
        private const string ADMIN_USERNAME = "thecuevas0123_";
        private const string ADMIN_PASSWORD = "thepikachu0123_";
        private const string ADMIN_EMAIL = "admin@elcriollo.com";
        private const string ADMIN_NOMBRE = "Administrador";
        private const string ADMIN_APELLIDO = "El Criollo";

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IEmpleadoRepository empleadoRepository,
            IEmailService emailService,
            IMapper mapper,
            JwtSettings jwtSettings,
            ILogger<AuthService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _empleadoRepository = empleadoRepository;
            _emailService = emailService;
            _mapper = mapper;
            _jwtSettings = jwtSettings;
            _logger = logger;
        }

        // ============================================================================
        // OPERACIONES DE AUTENTICACIÓN PRINCIPAL
        // ============================================================================

        /// <summary>
        /// Autentica un usuario con sus credenciales y genera tokens JWT
        /// </summary>
        public async Task<AuthResponse?> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("Intento de login para usuario: {Username}", loginRequest.Username);

                // Validar datos de entrada
                if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    _logger.LogWarning("Intento de login con credenciales vacías");
                    await LogLoginAttemptAsync(loginRequest.Username ?? "N/A", false, null, null);
                    return null;
                }

                // Buscar usuario por nombre de usuario o email
                var usuario = await _usuarioRepository.GetByUsernameAsync(loginRequest.Username) ??
                             await _usuarioRepository.GetByEmailAsync(loginRequest.Username);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Username}", loginRequest.Username);
                    await LogLoginAttemptAsync(loginRequest.Username, false, null, null);
                    return null;
                }

                // Verificar si el usuario está activo
                if (!usuario.EsActivo)
                {
                    _logger.LogWarning("Intento de login con usuario inactivo: {Username}", loginRequest.Username);
                    await LogLoginAttemptAsync(loginRequest.Username, false, null, null);
                    return null;
                }

                // Validar contraseña con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, usuario.PasswordHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Username}", loginRequest.Username);
                    await LogLoginAttemptAsync(loginRequest.Username, false, null, null);
                    return null;
                }

                // Actualizar último login
                usuario.UltimoLogin = DateTime.UtcNow;
                await _usuarioRepository.UpdateAsync(usuario);

                // Generar tokens JWT
                var (token, refreshToken, expiresAt) = await GenerateJwtTokenAsync(usuario);

                // Log exitoso
                _logger.LogInformation("Login exitoso para usuario: {Username} - Rol: {Rol}", 
                    usuario.Username, usuario.Rol?.Nombre);
                await LogLoginAttemptAsync(loginRequest.Username, true, null, null);

                // Mapear usuario a response
                var usuarioResponse = _mapper.Map<UsuarioResponse>(usuario);

                return new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    Usuario = usuarioResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de login para usuario: {Username}", loginRequest.Username);
                await LogLoginAttemptAsync(loginRequest.Username ?? "N/A", false, null, null);
                return null;
            }
        }

        /// <summary>
        /// Renueva un token JWT usando el refresh token
        /// </summary>
        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogDebug("Intento de renovación de token con refresh token");

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return null;
                }

                // Buscar usuario por refresh token
                var usuario = await _usuarioRepository.GetByRefreshTokenAsync(refreshToken);
                if (usuario == null)
                {
                    _logger.LogWarning("Refresh token no válido o expirado");
                    return null;
                }

                // Verificar que el usuario esté activo
                if (!usuario.EsActivo)
                {
                    _logger.LogWarning("Intento de renovación con usuario inactivo: {Username}", usuario.Username);
                    return null;
                }

                // Generar nuevos tokens
                var (newToken, newRefreshToken, expiresAt) = await GenerateJwtTokenAsync(usuario);

                _logger.LogInformation("Token renovado exitosamente para usuario: {Username}", usuario.Username);

                var usuarioResponse = _mapper.Map<UsuarioResponse>(usuario);

                return new AuthResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = expiresAt,
                    Usuario = usuarioResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la renovación de token");
                return null;
            }
        }

        /// <summary>
        /// Invalida los tokens de un usuario (logout)
        /// </summary>
        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Logout para usuario ID: {UserId}", userId);

                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para logout: {UserId}", userId);
                    return false;
                }

                // Invalidar refresh token
                usuario.RefreshToken = null;
                usuario.RefreshTokenExpiry = null;
                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Logout exitoso para usuario: {Username}", usuario.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante logout para usuario ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Valida si un token JWT es válido y activo
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters();

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                // Verificar que el usuario siga activo
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var usuario = await _usuarioRepository.GetByIdAsync(userId);
                    return usuario?.EsActivo == true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Token inválido: {Error}", ex.Message);
                return false;
            }
        }

        // ============================================================================
        // GESTIÓN DE USUARIOS Y ROLES
        // ============================================================================

        /// <summary>
        /// Crea el usuario administrador inicial del sistema
        /// </summary>
        public async Task<UsuarioResponse> CreateAdminUserAsync()
        {
            try
            {
                _logger.LogInformation("Creando usuario administrador inicial del sistema");

                // Verificar si ya existe el usuario admin
                var existingAdmin = await _usuarioRepository.GetByUsernameAsync(ADMIN_USERNAME);
                if (existingAdmin != null)
                {
                    _logger.LogInformation("Usuario administrador ya existe: {Username}", ADMIN_USERNAME);
                    return _mapper.Map<UsuarioResponse>(existingAdmin);
                }

                // Obtener rol de administrador
                var adminRole = await _usuarioRepository.GetRolByNombreAsync("Administrador");
                if (adminRole == null)
                {
                    _logger.LogError("Rol de Administrador no encontrado en la base de datos");
                    throw new InvalidOperationException("Rol de Administrador no configurado");
                }

                // Crear empleado administrador
                var empleadoAdmin = new Empleado
                {
                    Nombre = ADMIN_NOMBRE,
                    Apellido = ADMIN_APELLIDO,
                    Email = ADMIN_EMAIL,
                    Telefono = "809-555-0001",
                    Cedula = "00100000001",
                    FechaIngreso = DateTime.UtcNow,
                    Salario = 100000, // Salario simbólico
                    Estado = "Activo"
                };

                var empleadoCreado = await _empleadoRepository.CreateAsync(empleadoAdmin);

                // Crear usuario administrador
                var usuarioAdmin = new Usuario
                {
                    UsuarioNombre = ADMIN_USERNAME,
                    Email = ADMIN_EMAIL,
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(ADMIN_PASSWORD),
                    Estado = true,
                    FechaCreacion = DateTime.UtcNow,
                    RolID = adminRole.Id,
                    EmpleadoID = empleadoCreado.Id
                };

                var usuarioCreado = await _usuarioRepository.CreateAsync(usuarioAdmin);

                _logger.LogInformation("Usuario administrador creado exitosamente: {Username}", ADMIN_USERNAME);

                return _mapper.Map<UsuarioResponse>(usuarioCreado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario administrador inicial");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        public async Task<UsuarioResponse> CreateUserAsync(CreateUsuarioRequest crearUsuarioRequest, int createdByUserId)
        {
            try
            {
                _logger.LogInformation("Creando nuevo usuario y empleado: {Username} - {NombreCompleto} por usuario ID: {CreatedBy}", 
                    crearUsuarioRequest.Username, crearUsuarioRequest.NombreCompleto, createdByUserId);

                // ============================================================================
                // VALIDACIONES INICIALES
                // ============================================================================

                // Verificar que el usuario no exista
                var existingUser = await _usuarioRepository.GetByUsernameAsync(crearUsuarioRequest.Username);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"Ya existe un usuario con el nombre: {crearUsuarioRequest.Username}");
                }

                // Verificar email único
                var existingEmail = await _usuarioRepository.GetByEmailAsync(crearUsuarioRequest.Email);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException($"Ya existe un usuario con el email: {crearUsuarioRequest.Email}");
                }

                // Verificar cédula única
                var existingCedula = await _empleadoRepository.GetByCedulaAsync(crearUsuarioRequest.Cedula);
                if (existingCedula != null)
                {
                    throw new InvalidOperationException($"Ya existe un empleado con la cédula: {crearUsuarioRequest.Cedula}");
                }

                // ============================================================================
                // CREAR EMPLEADO PRIMERO
                // ============================================================================

                var nuevoEmpleado = _mapper.Map<Empleado>(crearUsuarioRequest);
                var empleadoCreado = await _empleadoRepository.CreateAsync(nuevoEmpleado);
                
                _logger.LogInformation("Empleado creado exitosamente: {EmpleadoId} - {NombreCompleto}", 
                    empleadoCreado.Id, empleadoCreado.NombreCompleto);

                // ============================================================================
                // CREAR USUARIO ASOCIADO
                // ============================================================================

                var nuevoUsuario = _mapper.Map<Usuario>(crearUsuarioRequest);
                nuevoUsuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(crearUsuarioRequest.Password);
                nuevoUsuario.Estado = true;
                nuevoUsuario.FechaCreacion = DateTime.UtcNow;
                nuevoUsuario.EmpleadoID = empleadoCreado.Id;

                var usuarioCreado = await _usuarioRepository.CreateAsync(nuevoUsuario);

                // ============================================================================
                // ACTUALIZAR EMPLEADO CON REFERENCIA AL USUARIO
                // ============================================================================

                empleadoCreado.UsuarioID = usuarioCreado.Id;
                await _empleadoRepository.UpdateAsync(empleadoCreado);

                _logger.LogInformation("Usuario creado y vinculado exitosamente: {Username} - ID: {UserId} - Empleado: {EmpleadoId}", 
                    usuarioCreado.Username, usuarioCreado.Id, empleadoCreado.Id);

                // ============================================================================
                // OBTENER USUARIO CON RELACIONES COMPLETAS
                // ============================================================================

                var usuarioConRelaciones = await _usuarioRepository.GetByIdAsync(usuarioCreado.Id);

                // ============================================================================
                // ENVIAR EMAIL DE BIENVENIDA
                // ============================================================================

                try
                {
                    if (usuarioConRelaciones != null)
                    {
                        var emailEnviado = await _emailService.EnviarBienvenidaUsuarioAsync(usuarioConRelaciones);
                        if (emailEnviado)
                        {
                            _logger.LogInformation("✅ Email de bienvenida enviado a: {Email}", usuarioConRelaciones.Email);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No se pudo enviar email de bienvenida a: {Email}", usuarioConRelaciones.Email);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No se pudo cargar usuario con relaciones para enviar email");
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "❌ Error al enviar email de bienvenida para usuario: {Username}", usuarioCreado.Username);
                    // No fallar todo el proceso si solo falla el email
                }

                return _mapper.Map<UsuarioResponse>(usuarioConRelaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario y empleado: {Username}", crearUsuarioRequest.Username);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los claims específicos de un usuario para JWT
        /// </summary>
        public Task<IEnumerable<Claim>> GetUserClaimsAsync(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Username ?? string.Empty),
                new(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new("jti", Guid.NewGuid().ToString()),
                new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Agregar rol
            if (usuario.Rol != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.NombreRol ?? "SinRol"));
                claims.Add(new Claim("rol_id", usuario.Rol.Id.ToString()));
            }

            // Agregar datos del empleado si existe
            if (usuario.Empleado != null)
            {
                claims.Add(new Claim("empleado_id", usuario.Empleado.Id.ToString()));
                claims.Add(new Claim("empleado_nombre", $"{usuario.Empleado.Nombre} {usuario.Empleado.Apellido}"));
            }

            // Claims específicos dominicanos
            claims.Add(new Claim("restaurante", "El Criollo"));
            claims.Add(new Claim("pais", "Republica Dominicana"));
            claims.Add(new Claim("timezone", "America/Santo_Domingo"));

            _logger.LogDebug("Claims generados para usuario: {Username} - Cantidad: {ClaimsCount}", 
                usuario.Username, claims.Count);

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }

        /// <summary>
        /// Valida si un usuario tiene permisos para una operación específica
        /// </summary>
        public async Task<bool> ValidateUserPermissionAsync(int userId, string permiso)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario?.Rol == null || !usuario.EsActivo)
                    return false;

                // Administrador tiene todos los permisos
                if (usuario.Rol.NombreRol == "Administrador")
                    return true;

                // Validar permisos específicos por rol
                return usuario.Rol.NombreRol switch
                {
                    "Recepcion" => permiso.Contains("reserva") || permiso.Contains("mesa") || permiso.Contains("cliente"),
                    "Mesero" => permiso.Contains("orden") || permiso.Contains("mesa") || permiso.Contains("producto"),
                    "Cajero" => permiso.Contains("factura") || permiso.Contains("pago") || permiso.Contains("reporte"),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando permisos para usuario ID: {UserId} - Permiso: {Permiso}", 
                    userId, permiso);
                return false;
            }
        }

        // ============================================================================
        // OPERACIONES DE SEGURIDAD AVANZADAS
        // ============================================================================

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                // Verificar contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.PasswordHash))
                {
                    _logger.LogWarning("Intento de cambio de contraseña con contraseña actual incorrecta para usuario ID: {UserId}", userId);
                    return false;
                }

                // Actualizar contraseña
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {Username}", usuario.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Restablece la contraseña de un usuario (solo administradores)
        /// </summary>
        public async Task<bool> ResetPasswordAsync(int userId, string newPassword, int adminUserId)
        {
            try
            {
                // Validar que quien hace el reset sea administrador
                var admin = await _usuarioRepository.GetByIdAsync(adminUserId);
                if (admin?.Rol?.NombreRol != "Administrador")
                {
                    _logger.LogWarning("Intento de reset de contraseña por usuario no administrador ID: {AdminId}", adminUserId);
                    return false;
                }

                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                // Restablecer contraseña
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Contraseña restablecida por admin {AdminUsername} para usuario: {Username}", 
                    admin.Username, usuario.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña para usuario ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Bloquea/desbloquea un usuario
        /// </summary>
        public async Task<bool> ToggleUserBlockAsync(int userId, bool isBlocked, int adminUserId)
        {
            try
            {
                // Validar que quien hace el cambio sea administrador
                var admin = await _usuarioRepository.GetByIdAsync(adminUserId);
                if (admin?.Rol?.NombreRol != "Administrador")
                    return false;

                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                    return false;

                usuario.Estado = !isBlocked;
                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Usuario {Action} por admin {AdminUsername}: {Username}", 
                    isBlocked ? "bloqueado" : "desbloqueado", admin.Username, usuario.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de bloqueo para usuario ID: {UserId}", userId);
                return false;
            }
        }

        // ============================================================================
        // OPERACIONES DE AUDITORÍA Y SEGURIDAD
        // ============================================================================

        /// <summary>
        /// Registra un intento de login para auditoría
        /// </summary>
        public Task LogLoginAttemptAsync(string username, bool success, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                // Por ahora solo logging, posteriormente se puede crear tabla de auditoría
                _logger.LogInformation("Login attempt - Usuario: {Username}, Exitoso: {Success}, IP: {IP}, UserAgent: {UserAgent}", 
                    username, success, ipAddress ?? "N/A", userAgent ?? "N/A");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando intento de login");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Obtiene el historial de logins de un usuario
        /// </summary>
        public Task<IEnumerable<LoginAttempt>> GetLoginHistoryAsync(int userId, int days = 30)
        {
            // Implementación futura con tabla de auditoría
            // Por ahora retornamos lista vacía
            return Task.FromResult<IEnumerable<LoginAttempt>>(new List<LoginAttempt>());
        }

        /// <summary>
        /// Valida si el sistema está configurado correctamente
        /// </summary>
        public async Task<SystemValidationResult> ValidateSystemHealthAsync()
        {
            var result = new SystemValidationResult
            {
                ComponentStatus = new Dictionary<string, bool>()
            };

            try
            {
                // Validar conexión a base de datos
                var dbHealthy = await _usuarioRepository.TestConnectionAsync();
                result.ComponentStatus["Database"] = dbHealthy;

                // Validar configuración JWT
                var jwtHealthy = !string.IsNullOrEmpty(_jwtSettings.Key) && 
                                !string.IsNullOrEmpty(_jwtSettings.Issuer) && 
                                !string.IsNullOrEmpty(_jwtSettings.Audience);
                result.ComponentStatus["JWT_Configuration"] = jwtHealthy;

                // Validar usuario administrador
                var adminExists = await _usuarioRepository.GetByUsernameAsync(ADMIN_USERNAME) != null;
                result.ComponentStatus["Admin_User"] = adminExists;

                // Validar roles básicos
                var rolesExist = await _usuarioRepository.GetAllRolesAsync();
                var hasBasicRoles = rolesExist.Any(r => r.Nombre == "Administrador") &&
                                   rolesExist.Any(r => r.Nombre == "Mesero") &&
                                   rolesExist.Any(r => r.Nombre == "Cajero") &&
                                   rolesExist.Any(r => r.Nombre == "Recepcion");
                result.ComponentStatus["Basic_Roles"] = hasBasicRoles;

                result.IsHealthy = result.ComponentStatus.Values.All(status => status);
                result.Message = result.IsHealthy ? 
                    "Sistema El Criollo funcionando correctamente" : 
                    "Algunos componentes del sistema requieren atención";

                _logger.LogInformation("Validación de sistema completada - Saludable: {IsHealthy}", result.IsHealthy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante validación de sistema");
                result.IsHealthy = false;
                result.Message = "Error durante validación del sistema";
            }

            return result;
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        /// <summary>
        /// Genera token JWT y refresh token para un usuario
        /// </summary>
        private async Task<(string token, string refreshToken, DateTime expiresAt)> GenerateJwtTokenAsync(Usuario usuario)
        {
            var claims = await GetUserClaimsAsync(usuario);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Generar y guardar refresh token
            var refreshToken = Guid.NewGuid().ToString();
            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // 7 días
            await _usuarioRepository.UpdateAsync(usuario);

            return (tokenString, refreshToken, expiresAt);
        }

        /// <summary>
        /// Obtiene parámetros de validación para tokens JWT
        /// </summary>
        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}