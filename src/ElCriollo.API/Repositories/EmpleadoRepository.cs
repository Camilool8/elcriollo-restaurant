using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación del repositorio para operaciones con empleados
    /// </summary>
    public class EmpleadoRepository : BaseRepository<Empleado>, IEmpleadoRepository
    {
        public EmpleadoRepository(ElCriolloDbContext context, ILogger<EmpleadoRepository> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Obtiene un empleado por su cédula
        /// </summary>
        public async Task<Empleado?> GetByCedulaAsync(string cedula)
        {
            try
            {
                _logger.LogDebug("Obteniendo empleado por cédula: {Cedula}", cedula);

                var empleado = await _dbSet
                    .Include(e => e.Usuario)
                    .ThenInclude(u => u!.Rol)
                    .FirstOrDefaultAsync(e => e.Cedula == cedula);

                if (empleado == null)
                {
                    _logger.LogWarning("No se encontró empleado con cédula: {Cedula}", cedula);
                }

                return empleado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado por cédula: {Cedula}", cedula);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un empleado por su usuario ID
        /// </summary>
        public async Task<Empleado?> GetByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                _logger.LogDebug("Obteniendo empleado por usuario ID: {UsuarioId}", usuarioId);

                var empleado = await _dbSet
                    .Include(e => e.Usuario)
                    .ThenInclude(u => u!.Rol)
                    .FirstOrDefaultAsync(e => e.UsuarioID == usuarioId);

                if (empleado == null)
                {
                    _logger.LogWarning("No se encontró empleado con usuario ID: {UsuarioId}", usuarioId);
                }

                return empleado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado por usuario ID: {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene empleados por rol
        /// </summary>
        public async Task<IEnumerable<Empleado>> GetByRolAsync(string rol)
        {
            try
            {
                _logger.LogDebug("Obteniendo empleados por rol: {Rol}", rol);

                var empleados = await _dbSet
                    .Include(e => e.Usuario)
                    .ThenInclude(u => u!.Rol)
                    .Where(e => e.Usuario!.Rol!.NombreRol == rol)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} empleados con rol: {Rol}", empleados.Count, rol);

                return empleados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados por rol: {Rol}", rol);
                throw;
            }
        }

        /// <summary>
        /// Obtiene empleados activos
        /// </summary>
        public async Task<IEnumerable<Empleado>> GetEmpleadosActivosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo empleados activos");

                var empleados = await _dbSet
                    .Include(e => e.Usuario)
                    .ThenInclude(u => u!.Rol)
                    .Where(e => e.Estado && e.Usuario!.Estado)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} empleados activos", empleados.Count);

                return empleados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados activos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene empleados por departamento
        /// </summary>
        public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(string departamento)
        {
            try
            {
                _logger.LogDebug("Obteniendo empleados por departamento: {Departamento}", departamento);

                var empleados = await _dbSet
                    .Include(e => e.Usuario)
                    .ThenInclude(u => u!.Rol)
                    .Where(e => e.Departamento == departamento && e.Estado)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} empleados en departamento: {Departamento}", empleados.Count, departamento);

                return empleados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados por departamento: {Departamento}", departamento);
                throw;
            }
        }
    }
} 