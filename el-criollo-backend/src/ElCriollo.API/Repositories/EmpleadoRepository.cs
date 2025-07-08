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
                    .Where(e => e.Estado == "Activo" && e.Usuario!.Estado == true)
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
                    .Where(e => e.Departamento == departamento && e.Estado == "Activo")
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

        /// <summary>
        /// Obtiene el historial de compras de un empleado
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetHistorialComprasAsync(int empleadoId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogDebug("Obteniendo historial de compras para empleado ID: {EmpleadoId}", empleadoId);

                var query = from f in _context.Facturas
                            join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                            join c in _context.Clientes on o.ClienteID equals c.ClienteID
                            where c.ClienteID == empleadoId && f.Estado == "Pagada"
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

                _logger.LogDebug("Se encontraron {Count} compras para empleado ID: {EmpleadoId}", historial.Count, empleadoId);

                return historial;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de compras para empleado ID: {EmpleadoId}", empleadoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas detalladas de un empleado
        /// </summary>
        public async Task<dynamic> GetEstadisticasEmpleadoAsync(int empleadoId)
        {
            try
            {
                _logger.LogDebug("Obteniendo estadísticas para empleado ID: {EmpleadoId}", empleadoId);

                var estadisticas = await (from e in _dbSet
                                         where e.EmpleadoID == empleadoId
                                         select new
                                         {
                                             EmpleadoId = e.EmpleadoID,
                                             NombreEmpleado = e.NombreCompleto,
                                             TotalVisitas = (from f in _context.Facturas
                                                           join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                           join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                           where c.ClienteID == empleadoId && f.Estado == "Pagada"
                                                           select f).Count(),
                                             TotalGastado = (from f in _context.Facturas
                                                           join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                           join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                           where c.ClienteID == empleadoId && f.Estado == "Pagada"
                                                           select f.Total).Sum(),
                                             ProductoFavorito = (from d in _context.DetalleOrdenes
                                                               join p in _context.Productos on d.ProductoID equals p.ProductoID
                                                               join o in _context.Ordenes on d.OrdenID equals o.OrdenID
                                                               join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                               where c.ClienteID == empleadoId
                                                               group d by p.Nombre into g
                                                               orderby g.Sum(x => x.Cantidad) descending
                                                               select g.Key).FirstOrDefault(),
                                             DiasDesdeUltimaVisita = (from f in _context.Facturas
                                                                    join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                                    join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                                    where c.ClienteID == empleadoId && f.Estado == "Pagada"
                                                                    orderby f.FechaFactura descending
                                                                    select f.FechaFactura).FirstOrDefault()
                                         }).FirstOrDefaultAsync();

                if (estadisticas != null)
                {
                    var diasDesdeUltimaVisita = estadisticas.DiasDesdeUltimaVisita != default(DateTime) 
                        ? (int)(DateTime.Now - estadisticas.DiasDesdeUltimaVisita).TotalDays 
                        : 0;

                    var ticketPromedio = estadisticas.TotalVisitas > 0 
                        ? estadisticas.TotalGastado / estadisticas.TotalVisitas 
                        : 0;

                    return new
                    {
                        estadisticas.EmpleadoId,
                        estadisticas.NombreEmpleado,
                        estadisticas.TotalVisitas,
                        estadisticas.TotalGastado,
                        TicketPromedio = ticketPromedio,
                        estadisticas.ProductoFavorito,
                        CategoriaFavorita = "Por determinar",
                        DiaSemanaFrecuente = "Por determinar",
                        HoraFrecuente = "Por determinar",
                        DiasDesdeUltimaVisita = diasDesdeUltimaVisita,
                        NivelFidelidad = "Empleado"
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas para empleado ID: {EmpleadoId}", empleadoId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene empleados frecuentes basado en visitas
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetEmpleadosFrecuentesAsync(int minVisitas = 5)
        {
            try
            {
                _logger.LogDebug("Obteniendo empleados frecuentes con mínimo {MinVisitas} visitas", minVisitas);

                var empleadosFrecuentes = await (from e in _dbSet
                                               where e.Estado == "Activo"
                                               let totalVisitas = (from f in _context.Facturas
                                                                 join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                                 join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                                 where c.ClienteID == e.EmpleadoID && f.Estado == "Pagada"
                                                                 select f).Count()
                                               let totalGastado = (from f in _context.Facturas
                                                                 join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                                 join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                                 where c.ClienteID == e.EmpleadoID && f.Estado == "Pagada"
                                                                 select f.Total).Sum()
                                               let ultimaVisita = (from f in _context.Facturas
                                                                 join o in _context.Ordenes on f.OrdenID equals o.OrdenID
                                                                 join c in _context.Clientes on o.ClienteID equals c.ClienteID
                                                                 where c.ClienteID == e.EmpleadoID && f.Estado == "Pagada"
                                                                 orderby f.FechaFactura descending
                                                                 select f.FechaFactura).FirstOrDefault()
                                               where totalVisitas >= minVisitas
                                               orderby totalVisitas descending
                                               select new
                                               {
                                                   EmpleadoId = e.EmpleadoID,
                                                   NombreCompleto = e.NombreCompleto,
                                                   Email = e.Email,
                                                   Telefono = e.Telefono,
                                                   TotalVisitas = totalVisitas,
                                                   TotalGastado = totalGastado,
                                                   UltimaVisita = ultimaVisita,
                                                   TicketPromedio = totalVisitas > 0 ? totalGastado / totalVisitas : 0
                                               }).ToListAsync();

                _logger.LogDebug("Se encontraron {Count} empleados frecuentes", empleadosFrecuentes.Count);

                return empleadosFrecuentes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados frecuentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene empleados que cumplen años en el mes especificado
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetEmpleadosCumpleanosAsync(int mes)
        {
            try
            {
                _logger.LogDebug("Obteniendo empleados que cumplen años en mes: {Mes}", mes);

                var cumpleaneros = await (from e in _dbSet
                                        where e.Estado == "Activo" && 
                                              e.FechaNacimiento.HasValue &&
                                              e.FechaNacimiento.Value.Month == mes
                                        orderby e.FechaNacimiento.Value.Day
                                        select new
                                        {
                                            EmpleadoId = e.EmpleadoID,
                                            NombreCompleto = e.NombreCompleto,
                                            Email = e.Email,
                                            Telefono = e.Telefono,
                                            FechaNacimiento = e.FechaNacimiento.Value,
                                            DiaCumpleanos = e.FechaNacimiento.Value.Day,
                                            Edad = DateTime.Now.Year - e.FechaNacimiento.Value.Year
                                        }).ToListAsync();

                _logger.LogDebug("Se encontraron {Count} empleados que cumplen años en mes: {Mes}", cumpleaneros.Count, mes);

                return cumpleaneros;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados que cumplen años en mes: {Mes}", mes);
                throw;
            }
        }
    }
} 