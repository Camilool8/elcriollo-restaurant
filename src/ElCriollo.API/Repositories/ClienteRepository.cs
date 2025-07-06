using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación del repositorio para operaciones con clientes
    /// </summary>
    public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ElCriolloDbContext context, ILogger<ClienteRepository> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Obtiene un cliente por su email
        /// </summary>
        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogDebug("Obteniendo cliente por email: {Email}", email);

                var cliente = await _dbSet
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (cliente == null)
                {
                    _logger.LogWarning("No se encontró cliente con email: {Email}", email);
                }

                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente por email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un cliente por su teléfono
        /// </summary>
        public async Task<Cliente?> GetByTelefonoAsync(string telefono)
        {
            try
            {
                _logger.LogDebug("Obteniendo cliente por teléfono: {Telefono}", telefono);

                var cliente = await _dbSet
                    .FirstOrDefaultAsync(c => c.Telefono == telefono);

                if (cliente == null)
                {
                    _logger.LogWarning("No se encontró cliente con teléfono: {Telefono}", telefono);
                }

                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente por teléfono: {Telefono}", telefono);
                throw;
            }
        }

        /// <summary>
        /// Busca clientes por nombre o apellido
        /// </summary>
        public async Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string searchTerm)
        {
            try
            {
                _logger.LogDebug("Buscando clientes por nombre: {SearchTerm}", searchTerm);

                var clientes = await _dbSet
                    .Where(c => c.Nombre.Contains(searchTerm) || c.Apellido.Contains(searchTerm))
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} clientes con el término de búsqueda: {SearchTerm}", clientes.Count, searchTerm);

                return clientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes por nombre: {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Obtiene clientes frecuentes
        /// </summary>
        public async Task<IEnumerable<Cliente>> GetClientesFrecuentesAsync(int minOrdenes = 5)
        {
            try
            {
                _logger.LogDebug("Obteniendo clientes frecuentes con mínimo {MinOrdenes} órdenes", minOrdenes);

                var clientes = await _dbSet
                    .Include(c => c.Reservaciones)
                    .Where(c => c.Reservaciones.Count >= minOrdenes)
                    .OrderByDescending(c => c.Reservaciones.Count)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} clientes frecuentes", clientes.Count);

                return clientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes frecuentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene clientes con reservas pendientes
        /// </summary>
        public async Task<IEnumerable<Cliente>> GetClientesConReservasPendientesAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo clientes con reservas pendientes");

                var clientes = await _dbSet
                    .Include(c => c.Reservaciones)
                    .Where(c => c.Reservaciones.Any(r => r.Estado == "Pendiente" || r.Estado == "Confirmada"))
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} clientes con reservas pendientes", clientes.Count);

                return clientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes con reservas pendientes");
                throw;
            }
        }
    }
} 