using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de empleados
    /// </summary>
    public interface IEmpleadoRepository : IBaseRepository<Empleado>
    {
        /// <summary>
        /// Obtiene un empleado por su cédula
        /// </summary>
        Task<Empleado?> GetByCedulaAsync(string cedula);

        /// <summary>
        /// Obtiene un empleado por su usuario ID
        /// </summary>
        Task<Empleado?> GetByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtiene empleados por rol
        /// </summary>
        Task<IEnumerable<Empleado>> GetByRolAsync(string rol);

        /// <summary>
        /// Obtiene empleados activos
        /// </summary>
        Task<IEnumerable<Empleado>> GetEmpleadosActivosAsync();

        /// <summary>
        /// Obtiene empleados por departamento
        /// </summary>
        Task<IEnumerable<Empleado>> GetByDepartamentoAsync(string departamento);

        /// <summary>
        /// Obtiene el historial de compras de un empleado
        /// </summary>
        Task<IEnumerable<dynamic>> GetHistorialComprasAsync(int empleadoId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene estadísticas detalladas de un empleado
        /// </summary>
        Task<dynamic> GetEstadisticasEmpleadoAsync(int empleadoId);

        /// <summary>
        /// Obtiene empleados frecuentes basado en visitas
        /// </summary>
        Task<IEnumerable<dynamic>> GetEmpleadosFrecuentesAsync(int minVisitas = 5);

        /// <summary>
        /// Obtiene empleados que cumplen años en el mes especificado
        /// </summary>
        Task<IEnumerable<dynamic>> GetEmpleadosCumpleanosAsync(int mes);
    }
} 