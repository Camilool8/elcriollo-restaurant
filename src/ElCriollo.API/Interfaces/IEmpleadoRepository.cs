using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de empleados
    /// </summary>
    public interface IEmpleadoRepository : IBaseRepository<Empleado>
    {
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
    }
} 