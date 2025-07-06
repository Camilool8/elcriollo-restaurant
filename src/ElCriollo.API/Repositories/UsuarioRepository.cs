using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación específica para operaciones con usuarios
    /// Maneja autenticación, gestión de contraseñas y operaciones específicas de usuarios
    /// </summary>
    public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ElCriolloDbContext context, ILogger<UsuarioRepository> logger)
            : base(context, logger)
        {
        }

        // ============================================================================
        // OPERACIONES DE AUTENTICACIÓN
        // ============================================================================

        /// <summary>
        /// Valida las credenciales de un usuario para login
        /// </summary>
        public async Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string contrasena)
        {
            try
            {
                _logger.LogDebug("Validando credenciales para usuario: {Usuario}", nombreUsuario);

                var usuario = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .FirstOrDefaultAsync(u => u.UsuarioNombre == nombreUsuario && u.Estado);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado o inactivo: {Usuario}", nombreUsuario);
                    return null;
                }

                // Verificar contraseña usando BCrypt
                if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Usuario}", nombreUsuario);
                    return null;
                }

                _logger.LogInformation("Credenciales válidas para usuario: {Usuario}", nombreUsuario);
                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar credenciales para usuario: {Usuario}", nombreUsuario);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        public async Task<Usuario?> GetByUsernameAsync(string nombreUsuario)
        {
            try
            {
                _logger.LogDebug("Obteniendo usuario por nombre: {Usuario}", nombreUsuario);

                var usuario = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .FirstOrDefaultAsync(u => u.UsuarioNombre == nombreUsuario);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Usuario}", nombreUsuario);
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por nombre: {Usuario}", nombreUsuario);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogDebug("Obteniendo usuario por email: {Email}", email);

                var usuario = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con email: {Email}", email);
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Registra el último acceso de un usuario
        /// </summary>
        public async Task<bool> RegistrarUltimoAccesoAsync(int usuarioId)
        {
            try
            {
                _logger.LogDebug("Registrando último acceso para usuario ID: {UsuarioId}", usuarioId);

                var usuario = await _dbSet.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para registrar acceso: {UsuarioId}", usuarioId);
                    return false;
                }

                usuario.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogDebug("Último acceso registrado para usuario ID: {UsuarioId}", usuarioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar último acceso para usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE CONTRASEÑAS
        // ============================================================================

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public async Task<bool> CambiarContrasenaAsync(int usuarioId, string contrasenaActual, string contrasenaNueva)
        {
            try
            {
                _logger.LogDebug("Cambiando contraseña para usuario ID: {UsuarioId}", usuarioId);

                var usuario = await _dbSet.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para cambio de contraseña: {UsuarioId}", usuarioId);
                    return false;
                }

                // Verificar contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(contrasenaActual, usuario.ContrasenaHash))
                {
                    _logger.LogWarning("Contraseña actual incorrecta para usuario ID: {UsuarioId}", usuarioId);
                    return false;
                }

                // Generar hash de nueva contraseña
                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasenaNueva);
                usuario.RequiereCambioContrasena = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario ID: {UsuarioId}", usuarioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Restablece la contraseña de un usuario (solo admin)
        /// </summary>
        public async Task<bool> RestablecerContrasenaAsync(int usuarioId, string contrasenaNueva, bool requiereCambio = true)
        {
            try
            {
                _logger.LogDebug("Restableciendo contraseña para usuario ID: {UsuarioId}", usuarioId);

                var usuario = await _dbSet.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para restablecer contraseña: {UsuarioId}", usuarioId);
                    return false;
                }

                // Generar hash de nueva contraseña
                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasenaNueva);
                usuario.RequiereCambioContrasena = requiereCambio;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña restablecida exitosamente para usuario ID: {UsuarioId}", usuarioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña para usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un usuario requiere cambio de contraseña
        /// </summary>
        public async Task<bool> RequiereCambioContrasenaAsync(int usuarioId)
        {
            try
            {
                var usuario = await _dbSet.FindAsync(usuarioId);
                return usuario?.RequiereCambioContrasena ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar requerimiento de cambio de contraseña para usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE USUARIOS
        // ============================================================================

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetByRolAsync(int rolId)
        {
            try
            {
                _logger.LogDebug("Obteniendo usuarios por rol ID: {RolId}", rolId);

                var usuarios = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .Where(u => u.RolID == rolId)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} usuarios con rol ID: {RolId}", usuarios.Count, rolId);
                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por rol ID: {RolId}", rolId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene usuarios activos únicamente
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetUsuariosActivosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo usuarios activos");

                var usuarios = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .Where(u => u.Estado)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} usuarios activos", usuarios.Count);
                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene usuarios con sus roles incluidos
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetUsuariosConRolesAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo usuarios con roles incluidos");

                var usuarios = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} usuarios con roles incluidos", usuarios.Count);
                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios con roles incluidos");
                throw;
            }
        }

        /// <summary>
        /// Activa o desactiva un usuario
        /// </summary>
        public async Task<bool> CambiarEstadoUsuarioAsync(int usuarioId, bool estado)
        {
            try
            {
                _logger.LogDebug("Cambiando estado de usuario ID: {UsuarioId} a {Estado}", usuarioId, estado);

                var usuario = await _dbSet.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para cambio de estado: {UsuarioId}", usuarioId);
                    return false;
                }

                usuario.Estado = estado;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de usuario ID: {UsuarioId} cambiado a {Estado}", usuarioId, estado);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        // ============================================================================
        // VALIDACIONES Y VERIFICACIONES
        // ============================================================================

        /// <summary>
        /// Verifica si un nombre de usuario ya existe
        /// </summary>
        public async Task<bool> NombreUsuarioExisteAsync(string nombreUsuario, int? excluirUsuarioId = null)
        {
            try
            {
                var query = _dbSet.Where(u => u.UsuarioNombre == nombreUsuario);

                if (excluirUsuarioId.HasValue)
                {
                    query = query.Where(u => u.UsuarioID != excluirUsuarioId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de nombre de usuario: {Usuario}", nombreUsuario);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un email ya está registrado
        /// </summary>
        public async Task<bool> EmailExisteAsync(string email, int? excluirUsuarioId = null)
        {
            try
            {
                var query = _dbSet.Where(u => u.Email == email);

                if (excluirUsuarioId.HasValue)
                {
                    query = query.Where(u => u.UsuarioID != excluirUsuarioId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un usuario tiene un rol específico
        /// </summary>
        public async Task<bool> TieneRolAsync(int usuarioId, string nombreRol)
        {
            try
            {
                var usuario = await _dbSet
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

                return usuario?.Rol?.NombreRol?.Equals(nombreRol, StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar rol de usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES DE AUDITORÍA
        // ============================================================================

        /// <summary>
        /// Obtiene historial de accesos de un usuario
        /// </summary>
        public async Task<IEnumerable<DateTime>> GetHistorialAccesosAsync(int usuarioId, int dias = 30)
        {
            try
            {
                // Nota: En una implementación real, esto requeriría una tabla de auditoría
                // Por ahora, retornamos el último acceso si existe
                var usuario = await _dbSet.FindAsync(usuarioId);
                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                if (usuario?.UltimoAcceso != null && usuario.UltimoAcceso > fechaLimite)
                {
                    return new[] { usuario.UltimoAcceso.Value };
                }

                return new List<DateTime>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de accesos para usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de usuarios
        /// </summary>
        public async Task<object> GetEstadisticasUsuariosAsync()
        {
            try
            {
                var totalUsuarios = await _dbSet.CountAsync();
                var usuariosActivos = await _dbSet.CountAsync(u => u.Estado);
                var usuariosInactivos = totalUsuarios - usuariosActivos;

                var usuariosPorRol = await _dbSet
                    .Include(u => u.Rol)
                    .GroupBy(u => u.Rol.NombreRol)
                    .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                    .ToListAsync();

                return new
                {
                    TotalUsuarios = totalUsuarios,
                    UsuariosActivos = usuariosActivos,
                    UsuariosInactivos = usuariosInactivos,
                    UsuariosPorRol = usuariosPorRol
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de usuarios");
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES ESPECÍFICAS PARA EL CRIOLLO
        // ============================================================================

        /// <summary>
        /// Obtiene usuarios por tipo de empleado
        /// </summary>
        public async Task<IEnumerable<Usuario>> GetUsuariosPorTipoEmpleadoAsync(string tipoEmpleado)
        {
            try
            {
                var usuarios = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .Where(u => u.Rol.NombreRol == tipoEmpleado && u.Estado)
                    .ToListAsync();

                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por tipo de empleado: {Tipo}", tipoEmpleado);
                throw;
            }
        }

        /// <summary>
        /// Crea el usuario administrador inicial del sistema
        /// </summary>
        public async Task<Usuario> CrearAdministradorInicialAsync(
            string nombreUsuario = "thecuevas0123_",
            string contrasena = "thepikachu0123_",
            string email = "admin@elcriollo.com")
        {
            try
            {
                _logger.LogInformation("Creando usuario administrador inicial");

                // Verificar si ya existe un administrador
                var adminExiste = await ExisteAdministradorAsync();
                if (adminExiste)
                {
                    throw new InvalidOperationException("Ya existe un usuario administrador en el sistema");
                }

                // Obtener el rol de administrador
                var rolAdmin = await _context.Roles
                    .FirstOrDefaultAsync(r => r.NombreRol == "Administrador");

                if (rolAdmin == null)
                {
                    throw new InvalidOperationException("No existe el rol de Administrador en el sistema");
                }

                // Crear usuario administrador
                var admin = new Usuario
                {
                    UsuarioNombre = nombreUsuario,
                    ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasena),
                    Email = email,
                    RolID = rolAdmin.RolID,
                    FechaCreacion = DateTime.UtcNow,
                    Estado = true,
                    RequiereCambioContrasena = false
                };

                await _dbSet.AddAsync(admin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario administrador inicial creado exitosamente");

                return admin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario administrador inicial");
                throw;
            }
        }

        /// <summary>
        /// Verifica si ya existe un administrador en el sistema
        /// </summary>
        public async Task<bool> ExisteAdministradorAsync()
        {
            try
            {
                return await _dbSet
                    .Include(u => u.Rol)
                    .AnyAsync(u => u.Rol.NombreRol == "Administrador" && u.Estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de administrador");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su refresh token
        /// </summary>
        public async Task<Usuario?> GetByRefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogDebug("Obteniendo usuario por refresh token");

                // Nota: Esta implementación asume que el refresh token se almacena en una propiedad adicional
                // En una implementación real, podrías tener una tabla separada para tokens
                var usuario = await _dbSet
                    .Include(u => u.Rol)
                    .Include(u => u.Empleado)
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.Estado);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con refresh token especificado");
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por refresh token");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        public async Task<Rol?> GetRolByNombreAsync(string nombreRol)
        {
            try
            {
                _logger.LogDebug("Obteniendo rol por nombre: {NombreRol}", nombreRol);

                var rol = await _context.Roles
                    .FirstOrDefaultAsync(r => r.NombreRol == nombreRol);

                if (rol == null)
                {
                    _logger.LogWarning("Rol no encontrado: {NombreRol}", nombreRol);
                }

                return rol;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol por nombre: {NombreRol}", nombreRol);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        public async Task<IEnumerable<Rol>> GetAllRolesAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo todos los roles");

                var roles = await _context.Roles
                    .OrderBy(r => r.NombreRol)
                    .ToListAsync();

                _logger.LogDebug("Se obtuvieron {Count} roles", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                throw;
            }
        }

        /// <summary>
        /// Verifica la conexión a la base de datos
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogDebug("Probando conexión a la base de datos");

                // Intenta hacer una consulta simple
                _ = await _context.Database.CanConnectAsync();
                
                // Intenta contar usuarios (operación simple)
                _ = await _dbSet.CountAsync();

                _logger.LogDebug("Conexión a la base de datos exitosa");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión a la base de datos");
                return false;
            }
        }
    }
}