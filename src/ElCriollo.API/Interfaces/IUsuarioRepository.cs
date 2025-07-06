using ElCriollo.API.Models.Entities;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para operaciones con usuarios
    /// Extiende las operaciones base con funcionalidades específicas de autenticación y gestión de usuarios
    /// </summary>
    public interface IUsuarioRepository : IBaseRepository<Usuario>
    {
        // ============================================================================
        // OPERACIONES DE AUTENTICACIÓN
        // ============================================================================

        /// <summary>
        /// Valida las credenciales de un usuario para login
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario</param>
        /// <param name="contrasena">Contraseña en texto plano</param>
        /// <returns>Usuario si las credenciales son válidas, null en caso contrario</returns>
        Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string contrasena);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario único</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<Usuario?> GetByUsernameAsync(string nombreUsuario);

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<Usuario?> GetByEmailAsync(string email);

        /// <summary>
        /// Obtiene un usuario por su refresh token
        /// </summary>
        Task<Usuario?> GetByRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        Task<Rol?> GetRolByNombreAsync(string nombreRol);

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        Task<IEnumerable<Rol>> GetAllRolesAsync();

        /// <summary>
        /// Prueba la conexión a la base de datos
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        new Task<Usuario> CreateAsync(Usuario usuario);

        /// <summary>
        /// Registra el último acceso de un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se actualizó correctamente</returns>
        Task<bool> RegistrarUltimoAccesoAsync(int usuarioId);

        // ============================================================================
        // GESTIÓN DE CONTRASEÑAS
        // ============================================================================

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="contrasenaActual">Contraseña actual para validación</param>
        /// <param name="contrasenaNueva">Nueva contraseña</param>
        /// <returns>True si se cambió correctamente</returns>
        Task<bool> CambiarContrasenaAsync(int usuarioId, string contrasenaActual, string contrasenaNueva);

        /// <summary>
        /// Restablece la contraseña de un usuario (solo admin)
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="contrasenaNueva">Nueva contraseña</param>
        /// <param name="requiereCambio">Si debe cambiar la contraseña en el próximo login</param>
        /// <returns>True si se restableció correctamente</returns>
        Task<bool> RestablecerContrasenaAsync(int usuarioId, string contrasenaNueva, bool requiereCambio = true);

        /// <summary>
        /// Verifica si un usuario requiere cambio de contraseña
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si requiere cambio de contraseña</returns>
        Task<bool> RequiereCambioContrasenaAsync(int usuarioId);

        // ============================================================================
        // GESTIÓN DE USUARIOS
        // ============================================================================

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="rolId">ID del rol</param>
        /// <returns>Lista de usuarios del rol especificado</returns>
        Task<IEnumerable<Usuario>> GetByRolAsync(int rolId);

        /// <summary>
        /// Obtiene usuarios activos únicamente
        /// </summary>
        /// <returns>Lista de usuarios activos</returns>
        Task<IEnumerable<Usuario>> GetUsuariosActivosAsync();

        /// <summary>
        /// Obtiene usuarios con sus roles incluidos
        /// </summary>
        /// <returns>Lista de usuarios con información del rol</returns>
        Task<IEnumerable<Usuario>> GetUsuariosConRolesAsync();

        /// <summary>
        /// Activa o desactiva un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="estado">True para activar, False para desactivar</param>
        /// <returns>True si se cambió el estado correctamente</returns>
        Task<bool> CambiarEstadoUsuarioAsync(int usuarioId, bool estado);

        // ============================================================================
        // VALIDACIONES Y VERIFICACIONES
        // ============================================================================

        /// <summary>
        /// Verifica si un nombre de usuario ya existe
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar</param>
        /// <param name="excluirUsuarioId">ID de usuario a excluir en la verificación (para updates)</param>
        /// <returns>True si el nombre de usuario ya existe</returns>
        Task<bool> NombreUsuarioExisteAsync(string nombreUsuario, int? excluirUsuarioId = null);

        /// <summary>
        /// Verifica si un email ya está registrado
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excluirUsuarioId">ID de usuario a excluir en la verificación (para updates)</param>
        /// <returns>True si el email ya está registrado</returns>
        Task<bool> EmailExisteAsync(string email, int? excluirUsuarioId = null);

        /// <summary>
        /// Verifica si un usuario tiene un rol específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="nombreRol">Nombre del rol a verificar</param>
        /// <returns>True si el usuario tiene el rol especificado</returns>
        Task<bool> TieneRolAsync(int usuarioId, string nombreRol);

        // ============================================================================
        // OPERACIONES DE AUDITORÍA
        // ============================================================================

        /// <summary>
        /// Obtiene historial de accesos de un usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="dias">Número de días hacia atrás (por defecto 30)</param>
        /// <returns>Lista de fechas de acceso</returns>
        Task<IEnumerable<DateTime>> GetHistorialAccesosAsync(int usuarioId, int dias = 30);

        /// <summary>
        /// Obtiene estadísticas de usuarios
        /// </summary>
        /// <returns>Objeto con estadísticas de usuarios del sistema</returns>
        Task<object> GetEstadisticasUsuariosAsync();

        // ============================================================================
        // OPERACIONES ESPECÍFICAS PARA EL CRIOLLO
        // ============================================================================

        /// <summary>
        /// Obtiene usuarios por tipo de empleado (meseros, cajeros, etc.)
        /// </summary>
        /// <param name="tipoEmpleado">Tipo de empleado basado en el rol</param>
        /// <returns>Lista de usuarios empleados del tipo especificado</returns>
        Task<IEnumerable<Usuario>> GetUsuariosPorTipoEmpleadoAsync(string tipoEmpleado);

        /// <summary>
        /// Crea el usuario administrador inicial del sistema
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario (por defecto: thecuevas0123_)</param>
        /// <param name="contrasena">Contraseña (por defecto: thepikachu0123_)</param>
        /// <param name="email">Email del administrador</param>
        /// <returns>Usuario administrador creado</returns>
        Task<Usuario> CrearAdministradorInicialAsync(
            string nombreUsuario = "thecuevas0123_",
            string contrasena = "thepikachu0123_",
            string email = "admin@elcriollo.com");

        /// <summary>
        /// Verifica si ya existe un administrador en el sistema
        /// </summary>
        /// <returns>True si ya existe un administrador</returns>
        Task<bool> ExisteAdministradorAsync();
    }
}