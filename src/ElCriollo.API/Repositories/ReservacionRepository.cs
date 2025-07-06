using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación específica para operaciones con reservaciones
    /// Maneja el sistema de reservas de mesas con control de tiempo y disponibilidad
    /// </summary>
    public class ReservacionRepository : BaseRepository<Reservacion>, IReservacionRepository
    {
        public ReservacionRepository(ElCriolloDbContext context, ILogger<ReservacionRepository> logger)
            : base(context, logger)
        {
        }

        // ============================================================================
        // GESTIÓN DE ESTADOS DE RESERVACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones por estado específico
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetByEstadoAsync(string estado)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservaciones por estado: {Estado}", estado);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == estado)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} reservaciones en estado: {Estado}", reservaciones.Count, estado);
                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones por estado: {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones pendientes de confirmación
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesPendientesAsync()
        {
            return await GetByEstadoAsync("Pendiente");
        }

        /// <summary>
        /// Obtiene reservaciones confirmadas
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesConfirmadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Confirmada");

                if (fechaInicio.HasValue)
                    query = query.Where(r => r.FechaYHora >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(r => r.FechaYHora <= fechaFin.Value);

                var reservaciones = await query
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones confirmadas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones completadas
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesCompletadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Completada");

                if (fechaInicio.HasValue)
                    query = query.Where(r => r.FechaYHora >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(r => r.FechaYHora <= fechaFin.Value);

                var reservaciones = await query
                    .OrderByDescending(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones completadas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones canceladas
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesCanceladasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Cancelada");

                if (fechaInicio.HasValue)
                    query = query.Where(r => r.FechaYHora >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(r => r.FechaYHora <= fechaFin.Value);

                var reservaciones = await query
                    .OrderByDescending(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones canceladas");
                throw;
            }
        }

        /// <summary>
        /// Cambia el estado de una reservación
        /// </summary>
        public async Task<bool> CambiarEstadoReservacionAsync(int reservacionId, string nuevoEstado, string? observaciones = null)
        {
            try
            {
                _logger.LogDebug("Cambiando estado de reservación ID: {ReservacionId} a {Estado}", reservacionId, nuevoEstado);

                var reservacion = await _dbSet
                    .Include(r => r.Mesa)
                    .FirstOrDefaultAsync(r => r.ReservacionID == reservacionId);

                if (reservacion == null)
                {
                    _logger.LogWarning("Reservación no encontrada para cambio de estado: {ReservacionId}", reservacionId);
                    return false;
                }

                var estadosValidos = new[] { "Pendiente", "Confirmada", "Completada", "Cancelada" };
                if (!estadosValidos.Contains(nuevoEstado))
                {
                    _logger.LogWarning("Estado inválido para reservación: {Estado}", nuevoEstado);
                    return false;
                }

                var estadoAnterior = reservacion.Estado;
                reservacion.Estado = nuevoEstado;

                if (!string.IsNullOrEmpty(observaciones))
                {
                    reservacion.Observaciones = $"{reservacion.Observaciones}\n{DateTime.Now:yyyy-MM-dd HH:mm}: {observaciones}".Trim();
                }

                // Gestionar estado de mesa según el cambio de estado
                if (reservacion.Mesa != null)
                {
                    if (nuevoEstado == "Confirmada" && estadoAnterior == "Pendiente")
                    {
                        // Solo cambiar a reservada si la mesa está libre
                        if (reservacion.Mesa.Estado == "Libre")
                        {
                            reservacion.Mesa.Estado = "Reservada";
                        }
                    }
                    else if (nuevoEstado == "Cancelada" || nuevoEstado == "Completada")
                    {
                        // Liberar mesa si estaba reservada para esta reservación
                        if (reservacion.Mesa.Estado == "Reservada")
                        {
                            reservacion.Mesa.Estado = "Libre";
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estado de reservación ID: {ReservacionId} cambiado a {Estado}", reservacionId, nuevoEstado);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de reservación ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR FECHA Y TIEMPO
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones del día actual
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesHoyAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaYHora >= hoy && r.FechaYHora < mañana)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones de una fecha específica
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesPorFechaAsync(DateTime fecha)
        {
            try
            {
                var inicioFecha = fecha.Date;
                var finFecha = inicioFecha.AddDays(1);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaYHora >= inicioFecha && r.FechaYHora < finFecha)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones de fecha: {Fecha}", fecha);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones en un rango de fechas
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaYHora >= fechaInicio && r.FechaYHora <= fechaFin)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones por rango de fechas: {Inicio} - {Fin}", fechaInicio, fechaFin);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones próximas (siguientes horas)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesProximasAsync(int horas = 2)
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var fechaLimite = ahora.AddHours(horas);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaYHora >= ahora && 
                               r.FechaYHora <= fechaLimite &&
                               (r.Estado == "Confirmada" || r.Estado == "Pendiente"))
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones próximas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones vencidas (no confirmadas a tiempo)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesVencidasAsync()
        {
            try
            {
                var ahora = DateTime.UtcNow;

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaYHora < ahora && r.Estado == "Pendiente")
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones vencidas");
                throw;
            }
        }

        // ============================================================================
        // CONSULTAS POR MESA Y CLIENTE
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones de una mesa específica
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetByMesaAsync(int mesaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var query = _dbSet
                    .Include(r => r.Cliente)
                    .Where(r => r.MesaID == mesaId);

                if (fechaInicio.HasValue)
                    query = query.Where(r => r.FechaYHora >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(r => r.FechaYHora <= fechaFin.Value);

                var reservaciones = await query
                    .OrderByDescending(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones de mesa ID: {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene la reservación activa de una mesa
        /// </summary>
        public async Task<Reservacion?> GetReservacionActivaMesaAsync(int mesaId)
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var reservacion = await _dbSet
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.MesaID == mesaId && 
                                            r.Estado == "Confirmada" &&
                                            r.FechaYHora <= ahora &&
                                            r.FechaYHora.AddMinutes(r.DuracionEstimada) >= ahora);

                return reservacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservación activa de mesa ID: {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones de un cliente específico
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetByClienteAsync(int clienteId)
        {
            try
            {
                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Where(r => r.ClienteID == clienteId)
                    .OrderByDescending(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones de cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de reservaciones de un cliente
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetHistorialClienteAsync(int clienteId, int limite = 10)
        {
            try
            {
                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Where(r => r.ClienteID == clienteId)
                    .OrderByDescending(r => r.FechaYHora)
                    .Take(limite)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de cliente ID: {ClienteId}", clienteId);
                throw;
            }
        }

        // ============================================================================
        // VERIFICACIÓN DE DISPONIBILIDAD
        // ============================================================================

        /// <summary>
        /// Verifica si una mesa está disponible para una fecha/hora específica
        /// </summary>
        public async Task<bool> MesaDisponibleParaReservacionAsync(int mesaId, DateTime fechaYHora, int duracionMinutos, int? excluirReservacionId = null)
        {
            try
            {
                var inicioReservacion = fechaYHora;
                var finReservacion = fechaYHora.AddMinutes(duracionMinutos);

                var query = _dbSet.Where(r => r.MesaID == mesaId && 
                                             (r.Estado == "Confirmada" || r.Estado == "Pendiente") &&
                                             (
                                                 // Conflicto: nueva reservación inicia durante una existente
                                                 (inicioReservacion >= r.FechaYHora && inicioReservacion < r.FechaYHora.AddMinutes(r.DuracionEstimada)) ||
                                                 // Conflicto: nueva reservación termina durante una existente
                                                 (finReservacion > r.FechaYHora && finReservacion <= r.FechaYHora.AddMinutes(r.DuracionEstimada)) ||
                                                 // Conflicto: nueva reservación envuelve una existente
                                                 (inicioReservacion <= r.FechaYHora && finReservacion >= r.FechaYHora.AddMinutes(r.DuracionEstimada))
                                             ));

                if (excluirReservacionId.HasValue)
                {
                    query = query.Where(r => r.ReservacionID != excluirReservacionId.Value);
                }

                var conflictos = await query.AnyAsync();
                return !conflictos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad de mesa ID: {MesaId} para fecha: {Fecha}", mesaId, fechaYHora);
                throw;
            }
        }

        /// <summary>
        /// Busca mesas disponibles para una fecha/hora y capacidad específica
        /// </summary>
        public async Task<IEnumerable<Mesa>> BuscarMesasDisponiblesAsync(DateTime fechaYHora, int cantidadPersonas, int duracionMinutos = 120)
        {
            try
            {
                _logger.LogDebug("Buscando mesas disponibles para {Personas} personas el {Fecha} por {Duracion} minutos", 
                    cantidadPersonas, fechaYHora, duracionMinutos);

                // Obtener todas las mesas con capacidad suficiente
                var mesasConCapacidad = await _context.Mesas
                    .Where(m => m.Capacidad >= cantidadPersonas && m.Estado != "Mantenimiento")
                    .ToListAsync();

                var mesasDisponibles = new List<Mesa>();

                // Verificar disponibilidad para cada mesa
                foreach (var mesa in mesasConCapacidad)
                {
                    var disponible = await MesaDisponibleParaReservacionAsync(mesa.MesaID, fechaYHora, duracionMinutos);
                    if (disponible)
                    {
                        mesasDisponibles.Add(mesa);
                    }
                }

                // Ordenar por capacidad (priorizar mesas con capacidad más cercana)
                var mesasOrdenadas = mesasDisponibles
                    .OrderBy(m => m.Capacidad)
                    .ThenBy(m => m.NumeroMesa)
                    .ToList();

                _logger.LogDebug("Se encontraron {Count} mesas disponibles", mesasOrdenadas.Count);
                return mesasOrdenadas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar mesas disponibles");
                throw;
            }
        }

        /// <summary>
        /// Verifica conflictos de horario para una mesa
        /// </summary>
        public async Task<IEnumerable<Reservacion>> VerificarConflictosHorarioAsync(int mesaId, DateTime fechaYHora, int duracionMinutos)
        {
            try
            {
                var inicioReservacion = fechaYHora;
                var finReservacion = fechaYHora.AddMinutes(duracionMinutos);

                var conflictos = await _dbSet
                    .Include(r => r.Cliente)
                    .Where(r => r.MesaID == mesaId && 
                               (r.Estado == "Confirmada" || r.Estado == "Pendiente") &&
                               (
                                   (inicioReservacion >= r.FechaYHora && inicioReservacion < r.FechaYHora.AddMinutes(r.DuracionEstimada)) ||
                                   (finReservacion > r.FechaYHora && finReservacion <= r.FechaYHora.AddMinutes(r.DuracionEstimada)) ||
                                   (inicioReservacion <= r.FechaYHora && finReservacion >= r.FechaYHora.AddMinutes(r.DuracionEstimada))
                               ))
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return conflictos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar conflictos de horario para mesa ID: {MesaId}", mesaId);
                throw;
            }
        }

        // ============================================================================
        // OPERACIONES DE RESERVACIÓN
        // ============================================================================

        /// <summary>
        /// Crea una nueva reservación validando disponibilidad
        /// </summary>
        public async Task<Reservacion?> CrearReservacionAsync(Reservacion reservacion)
        {
            try
            {
                _logger.LogDebug("Creando nueva reservación para mesa ID: {MesaId} el {Fecha}", 
                    reservacion.MesaID, reservacion.FechaYHora);

                return await ExecuteInTransactionAsync(async () =>
                {
                    // Validar disponibilidad
                    var disponible = await MesaDisponibleParaReservacionAsync(
                        reservacion.MesaID, 
                        reservacion.FechaYHora, 
                        reservacion.DuracionEstimada);

                    if (!disponible)
                    {
                        _logger.LogWarning("Mesa ID: {MesaId} no está disponible para la fecha: {Fecha}", 
                            reservacion.MesaID, reservacion.FechaYHora);
                        return null;
                    }

                    // Establecer valores por defecto
                    reservacion.Estado = "Pendiente";
                    reservacion.FechaCreacion = DateTime.UtcNow;

                    await _dbSet.AddAsync(reservacion);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Reservación creada exitosamente ID: {ReservacionId}", reservacion.ReservacionID);
                    return reservacion;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reservación");
                throw;
            }
        }

        /// <summary>
        /// Confirma una reservación pendiente
        /// </summary>
        public async Task<bool> ConfirmarReservacionAsync(int reservacionId)
        {
            try
            {
                return await CambiarEstadoReservacionAsync(reservacionId, "Confirmada", "Reservación confirmada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar reservación ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        /// <summary>
        /// Cancela una reservación
        /// </summary>
        public async Task<bool> CancelarReservacionAsync(int reservacionId, string razon)
        {
            try
            {
                return await CambiarEstadoReservacionAsync(reservacionId, "Cancelada", $"Cancelada: {razon}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reservación ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        /// <summary>
        /// Marca una reservación como completada
        /// </summary>
        public async Task<bool> CompletarReservacionAsync(int reservacionId)
        {
            try
            {
                return await CambiarEstadoReservacionAsync(reservacionId, "Completada", "Reservación completada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar reservación ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        /// <summary>
        /// Modifica una reservación existente
        /// </summary>
        public async Task<bool> ModificarReservacionAsync(int reservacionId, DateTime? nuevaFechaYHora = null, int? nuevaMesaId = null, int? nuevaCantidadPersonas = null)
        {
            try
            {
                _logger.LogDebug("Modificando reservación ID: {ReservacionId}", reservacionId);

                return await ExecuteInTransactionAsync(async () =>
                {
                    var reservacion = await _dbSet
                        .Include(r => r.Mesa)
                        .FirstOrDefaultAsync(r => r.ReservacionID == reservacionId);

                    if (reservacion == null)
                    {
                        _logger.LogWarning("Reservación no encontrada para modificar: {ReservacionId}", reservacionId);
                        return false;
                    }

                    if (reservacion.Estado == "Completada" || reservacion.Estado == "Cancelada")
                    {
                        throw new InvalidOperationException($"No se puede modificar una reservación en estado {reservacion.Estado}");
                    }

                    // Liberar mesa actual si está reservada
                    if (reservacion.Mesa?.Estado == "Reservada")
                    {
                        reservacion.Mesa.Estado = "Libre";
                    }

                    // Aplicar cambios
                    if (nuevaFechaYHora.HasValue)
                        reservacion.FechaYHora = nuevaFechaYHora.Value;

                    if (nuevaMesaId.HasValue)
                        reservacion.MesaID = nuevaMesaId.Value;

                    if (nuevaCantidadPersonas.HasValue)
                        reservacion.CantidadPersonas = nuevaCantidadPersonas.Value;

                    // Validar nueva disponibilidad
                    var disponible = await MesaDisponibleParaReservacionAsync(
                        reservacion.MesaID, 
                        reservacion.FechaYHora, 
                        reservacion.DuracionEstimada,
                        reservacionId);

                    if (!disponible)
                    {
                        throw new InvalidOperationException("La nueva configuración de reservación no está disponible");
                    }

                    reservacion.Observaciones = $"{reservacion.Observaciones}\n{DateTime.Now:yyyy-MM-dd HH:mm}: Reservación modificada".Trim();

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Reservación ID: {ReservacionId} modificada exitosamente", reservacionId);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar reservación ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas de reservaciones del día
        /// </summary>
        public async Task<object> GetEstadisticasDelDiaAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var reservacionesHoy = await _dbSet
                    .Where(r => r.FechaYHora >= hoy && r.FechaYHora < mañana)
                    .ToListAsync();

                var totalReservaciones = reservacionesHoy.Count;
                var reservacionesPendientes = reservacionesHoy.Count(r => r.Estado == "Pendiente");
                var reservacionesConfirmadas = reservacionesHoy.Count(r => r.Estado == "Confirmada");
                var reservacionesCompletadas = reservacionesHoy.Count(r => r.Estado == "Completada");
                var reservacionesCanceladas = reservacionesHoy.Count(r => r.Estado == "Cancelada");

                var personasTotal = reservacionesHoy
                    .Where(r => r.Estado != "Cancelada")
                    .Sum(r => r.CantidadPersonas);

                return new
                {
                    Fecha = hoy.ToString("yyyy-MM-dd"),
                    TotalReservaciones = totalReservaciones,
                    ReservacionesPendientes = reservacionesPendientes,
                    ReservacionesConfirmadas = reservacionesConfirmadas,
                    ReservacionesCompletadas = reservacionesCompletadas,
                    ReservacionesCanceladas = reservacionesCanceladas,
                    PersonasTotal = personasTotal,
                    TasaCancelacion = totalReservaciones > 0 ? Math.Round((double)reservacionesCanceladas / totalReservaciones * 100, 2) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de reservaciones del día");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de reservaciones por período
        /// </summary>
        public async Task<object> GetEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var reservaciones = await _dbSet
                    .Where(r => r.FechaYHora >= fechaInicio && r.FechaYHora <= fechaFin)
                    .ToListAsync();

                var totalReservaciones = reservaciones.Count;
                var reservacionesPorEstado = reservaciones
                    .GroupBy(r => r.Estado)
                    .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                    .ToList();

                var reservacionesPorDia = reservaciones
                    .GroupBy(r => r.FechaYHora.Date)
                    .Select(g => new { Fecha = g.Key.ToString("yyyy-MM-dd"), Cantidad = g.Count() })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                var personasPromedio = reservaciones.Any() ? reservaciones.Average(r => r.CantidadPersonas) : 0;
                var duracionPromedio = reservaciones.Any() ? reservaciones.Average(r => r.DuracionEstimada) : 0;

                return new
                {
                    FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                    FechaFin = fechaFin.ToString("yyyy-MM-dd"),
                    TotalReservaciones = totalReservaciones,
                    ReservacionesPorEstado = reservacionesPorEstado,
                    ReservacionesPorDia = reservacionesPorDia,
                    PersonasPromedio = Math.Round(personasPromedio, 1),
                    DuracionPromedio = Math.Round(duracionPromedio, 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de reservaciones por período");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las horas más populares para reservaciones
        /// </summary>
        public async Task<IEnumerable<object>> GetHorasPopularesAsync(int dias = 30)
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                var horasPopulares = await _dbSet
                    .Where(r => r.FechaYHora >= fechaLimite && r.Estado != "Cancelada")
                    .GroupBy(r => r.FechaYHora.Hour)
                    .Select(g => new
                    {
                        Hora = g.Key,
                        CantidadReservaciones = g.Count(),
                        PersonasTotal = g.Sum(r => r.CantidadPersonas)
                    })
                    .OrderByDescending(h => h.CantidadReservaciones)
                    .ToListAsync();

                return horasPopulares;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener horas populares para reservaciones");
                throw;
            }
        }

        /// <summary>
        /// Obtiene los clientes más frecuentes
        /// </summary>
        public async Task<IEnumerable<object>> GetClientesMasFrecuentesAsync(int limite = 10, int dias = 90)
        {
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                var clientesFrecuentes = await _dbSet
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaYHora >= fechaLimite && r.Estado != "Cancelada")
                    .GroupBy(r => new { r.ClienteID, r.Cliente.Nombre, r.Cliente.Apellido })
                    .Select(g => new
                    {
                        ClienteId = g.Key.ClienteID,
                        NombreCompleto = $"{g.Key.Nombre} {g.Key.Apellido}",
                        CantidadReservaciones = g.Count(),
                        UltimaReservacion = g.Max(r => r.FechaYHora)
                    })
                    .OrderByDescending(c => c.CantidadReservaciones)
                    .Take(limite)
                    .ToListAsync();

                return clientesFrecuentes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes más frecuentes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la tasa de ocupación por reservaciones
        /// </summary>
        public async Task<double> GetTasaOcupacionReservacionesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var reservacionesConfirmadas = await _dbSet
                    .Where(r => r.FechaYHora >= fechaInicio && 
                               r.FechaYHora <= fechaFin && 
                               (r.Estado == "Confirmada" || r.Estado == "Completada"))
                    .ToListAsync();

                if (!reservacionesConfirmadas.Any())
                    return 0;

                var totalMinutosReservados = reservacionesConfirmadas.Sum(r => r.DuracionEstimada);
                var diasEnPeriodo = (fechaFin - fechaInicio).TotalDays + 1;
                var totalMesas = await _context.Mesas.CountAsync();
                
                // Asumiendo 12 horas de operación por día (720 minutos)
                var totalMinutosDisponibles = diasEnPeriodo * 720 * totalMesas;
                
                var tasaOcupacion = (totalMinutosReservados / totalMinutosDisponibles) * 100;
                
                return Math.Round(tasaOcupacion, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tasa de ocupación por reservaciones");
                throw;
            }
        }

        // ============================================================================
        // NOTIFICACIONES Y RECORDATORIOS
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones que necesitan recordatorio
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesParaRecordatorioAsync(int horasAntes = 2)
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var fechaLimite = ahora.AddHours(horasAntes);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Confirmada" &&
                               r.FechaYHora >= ahora &&
                               r.FechaYHora <= fechaLimite)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones para recordatorio");
                throw;
            }
        }

        /// <summary>
        /// Marca una reservación como recordada
        /// </summary>
        public async Task<bool> MarcarComoRecordadaAsync(int reservacionId)
        {
            try
            {
                var reservacion = await _dbSet.FindAsync(reservacionId);
                if (reservacion == null)
                    return false;

                reservacion.Observaciones = $"{reservacion.Observaciones}\n{DateTime.Now:yyyy-MM-dd HH:mm}: Recordatorio enviado".Trim();
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar como recordada la reservación ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservaciones que llegaron tarde
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesTardiasAsync(int minutosTolerancia = 15)
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var fechaLimite = ahora.AddMinutes(-minutosTolerancia);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Confirmada" &&
                               r.FechaYHora <= fechaLimite)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservaciones tardías");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una reservación con todos sus detalles incluidos
        /// </summary>
        public async Task<Reservacion?> GetByIdWithDetallesAsync(int reservacionId)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservación con detalles ID: {ReservacionId}", reservacionId);

                var reservacion = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.ReservacionID == reservacionId);

                if (reservacion == null)
                {
                    _logger.LogWarning("No se encontró reservación con ID: {ReservacionId}", reservacionId);
                }

                return reservacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservación con detalles ID: {ReservacionId}", reservacionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas por fecha (alias de GetReservacionesPorFechaAsync)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasPorFechaAsync(DateTime fecha)
        {
            return await GetReservacionesPorFechaAsync(fecha);
        }

        /// <summary>
        /// Obtiene reservas por estado (alias de GetByEstadoAsync)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasPorEstadoAsync(string estado)
        {
            return await GetByEstadoAsync(estado);
        }

        /// <summary>
        /// Obtiene reservas en rango de tiempo para una mesa
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasEnRangoAsync(int mesaId, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservas de mesa {MesaId} en rango {FechaInicio} - {FechaFin}", 
                    mesaId, fechaInicio, fechaFin);

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.MesaID == mesaId &&
                               r.FechaYHora >= fechaInicio &&
                               r.FechaYHora <= fechaFin)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                _logger.LogDebug("Se encontraron {Count} reservas", reservaciones.Count);
                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas en rango");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas para recordatorio con fecha específica
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasParaRecordatorioAsync(DateTime fechaLimite)
        {
            try
            {
                var ahora = DateTime.UtcNow;

                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Confirmada" &&
                               r.FechaYHora >= ahora &&
                               r.FechaYHora <= fechaLimite)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas para recordatorio");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas vencidas con fecha específica
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasVencidasAsync(DateTime fechaLimite)
        {
            try
            {
                var reservaciones = await _dbSet
                    .Include(r => r.Mesa)
                    .Include(r => r.Cliente)
                    .Where(r => r.Estado == "Confirmada" &&
                               r.FechaYHora <= fechaLimite)
                    .OrderBy(r => r.FechaYHora)
                    .ToListAsync();

                return reservaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas vencidas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas por rango de fecha (alias de GetReservacionesPorRangoFechasAsync)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasPorRangoFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await GetReservacionesPorRangoFechasAsync(fechaInicio, fechaFin);
        }

        /// <summary>
        /// Obtiene reservas por cliente (alias de GetByClienteAsync)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservasPorClienteAsync(int clienteId)
        {
            return await GetByClienteAsync(clienteId);
        }

        /// <summary>
        /// Obtiene reservaciones por mesa (simplificado)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesPorMesaAsync(int mesaId)
        {
            return await GetByMesaAsync(mesaId, null, null);
        }

        /// <summary>
        /// Obtiene reservaciones por estado (alias de GetByEstadoAsync)
        /// </summary>
        public async Task<IEnumerable<Reservacion>> GetReservacionesPorEstadoAsync(string estado)
        {
            return await GetByEstadoAsync(estado);
        }
    }
}