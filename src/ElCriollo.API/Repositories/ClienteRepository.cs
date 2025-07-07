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

        /// <summary>
        /// Obtiene únicamente clientes activos
        /// </summary>
        public async Task<IEnumerable<Cliente>> GetClientesActivosAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo clientes activos");

                var clientes = await _dbSet
                    .Where(c => c.Estado == "Activo")
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} clientes activos", clientes.Count);

                return clientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes activos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de compras de un cliente
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetHistorialComprasAsync(int clienteId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogDebug("Obteniendo historial de compras para cliente ID: {ClienteId}", clienteId);

                var query = from f in _context.Facturas
                            join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                            where o.ClienteID == clienteId && f.Estado == "Pagada"
                            select new
                            {
                                Fecha = f.FechaFactura,
                                NumeroFactura = f.NumeroFactura,
                                Monto = f.Total,
                                MetodoPago = f.MetodoPago,
                                ProductosComprados = (from d in _context.DetalleOrdenes
                                                     join p in _context.Productos on d.ProductoID equals p.ProductoID
                                                     where d.OrdenID == o.OrdenID
                                                     select p.Nombre).ToList()
                            };

                if (fechaInicio.HasValue)
                    query = query.Where(f => f.Fecha >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(f => f.Fecha <= fechaFin.Value);

                var historial = await query.OrderByDescending(f => f.Fecha).ToListAsync();

                _logger.LogDebug("Se encontraron {Count} compras para cliente ID: {ClienteId}", historial.Count, clienteId);

                return historial;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de compras para cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas detalladas de un cliente
        /// </summary>
        public async Task<dynamic> GetEstadisticasClienteAsync(int clienteId)
        {
            try
            {
                _logger.LogDebug("Obteniendo estadísticas para cliente ID: {ClienteId}", clienteId);

                var estadisticas = await (from c in _dbSet
                                         where c.ClienteID == clienteId
                                         select new
                                         {
                                             ClienteId = c.ClienteID,
                                             NombreCliente = c.NombreCompleto,
                                             TotalVisitas = (from f in _context.Facturas
                                                           join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                           where o.ClienteID == clienteId && f.Estado == "Pagada"
                                                           select f).Count(),
                                             TotalGastado = (from f in _context.Facturas
                                                           join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                           where o.ClienteID == clienteId && f.Estado == "Pagada"
                                                           select f.Total).Sum(),
                                             ProductoFavorito = (from d in _context.DetalleOrdenes
                                                               join p in _context.Productos on d.ProductoID equals p.ProductoID
                                                               join o in _context.Ordenes on d.OrdenID equals o.OrdenID
                                                               where o.ClienteID == clienteId
                                                               group d by p.Nombre into g
                                                               orderby g.Sum(x => x.Cantidad) descending
                                                               select g.Key).FirstOrDefault(),
                                             CategoriaFavorita = (from d in _context.DetalleOrdenes
                                                                join p in _context.Productos on d.ProductoID equals p.ProductoID
                                                                join cat in _context.Categorias on p.CategoriaID equals cat.CategoriaID
                                                                join o in _context.Ordenes on d.OrdenID equals o.OrdenID
                                                                where o.ClienteID == clienteId
                                                                group d by cat.NombreCategoria into g
                                                                orderby g.Sum(x => x.Cantidad) descending
                                                                select g.Key).FirstOrDefault(),
                                             UltimaVisita = (from f in _context.Facturas
                                                           join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                           where o.ClienteID == clienteId && f.Estado == "Pagada"
                                                           orderby f.FechaFactura descending
                                                           select f.FechaFactura).FirstOrDefault()
                                         }).FirstOrDefaultAsync();

                if (estadisticas != null)
                {
                    var diasDesdeUltimaVisita = estadisticas.UltimaVisita != default(DateTime) 
                        ? (int)(DateTime.Now - estadisticas.UltimaVisita).TotalDays 
                        : 0;

                    var ticketPromedio = estadisticas.TotalVisitas > 0 
                        ? estadisticas.TotalGastado / estadisticas.TotalVisitas 
                        : 0;

                    // Obtener el cliente para determinar el nivel de fidelidad
                    var cliente = await _dbSet.FirstOrDefaultAsync(c => c.ClienteID == clienteId);
                    var nivelFidelidad = cliente?.ObtenerCategoriaCliente() ?? "Regular";

                    return new
                    {
                        estadisticas.ClienteId,
                        estadisticas.NombreCliente,
                        estadisticas.TotalVisitas,
                        estadisticas.TotalGastado,
                        TicketPromedio = ticketPromedio,
                        estadisticas.ProductoFavorito,
                        estadisticas.CategoriaFavorita,
                        DiaSemanaFrecuente = "Por determinar",
                        HoraFrecuente = "Por determinar",
                        DiasDesdeUltimaVisita = diasDesdeUltimaVisita,
                        NivelFidelidad = nivelFidelidad
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas para cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene clientes que cumplen años en el mes especificado
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetClientesCumpleanosAsync(int mes)
        {
            try
            {
                _logger.LogDebug("Obteniendo clientes que cumplen años en mes: {Mes}", mes);

                var cumpleaneros = await (from c in _dbSet
                                        where c.Estado == "Activo" && 
                                              c.FechaNacimiento.HasValue &&
                                              c.FechaNacimiento.Value.Month == mes
                                        orderby c.FechaNacimiento.Value.Day
                                        select new
                                        {
                                            ClienteId = c.ClienteID,
                                            NombreCompleto = c.NombreCompleto,
                                            Email = c.Email,
                                            Telefono = c.Telefono,
                                            FechaNacimiento = c.FechaNacimiento.Value,
                                            DiaCumpleanos = c.FechaNacimiento.Value.Day,
                                            Edad = DateTime.Now.Year - c.FechaNacimiento.Value.Year
                                        }).ToListAsync();

                _logger.LogDebug("Se encontraron {Count} clientes que cumplen años en mes: {Mes}", cumpleaneros.Count, mes);

                return cumpleaneros;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes que cumplen años en mes: {Mes}", mes);
                throw;
            }
        }
    }
} 