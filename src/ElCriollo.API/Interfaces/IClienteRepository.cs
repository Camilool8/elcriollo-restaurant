using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de clientes
    /// </summary>
    public interface IClienteRepository : IBaseRepository<Cliente>
    {
        /// <summary>
        /// Obtiene un cliente por su email
        /// </summary>
        Task<Cliente?> GetByEmailAsync(string email);

        /// <summary>
        /// Obtiene un cliente por su teléfono
        /// </summary>
        Task<Cliente?> GetByTelefonoAsync(string telefono);

        /// <summary>
        /// Busca clientes por nombre o apellido
        /// </summary>
        Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string searchTerm);

        /// <summary>
        /// Obtiene clientes frecuentes
        /// </summary>
        Task<IEnumerable<Cliente>> GetClientesFrecuentesAsync(int minOrdenes = 5);

        /// <summary>
        /// Obtiene clientes con reservas pendientes
        /// </summary>
        Task<IEnumerable<Cliente>> GetClientesConReservasPendientesAsync();
    }
} 