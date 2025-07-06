using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;
using Microsoft.Extensions.Logging;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Servicio de gestión de mesas para El Criollo
    /// </summary>
    public class MesaService : IMesaService
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MesaService> _logger;

        // Constantes básicas para El Criollo
        private const int TIEMPO_LIMITE_OCUPACION_MINUTOS = 180; // 3 horas
        private readonly string[] ESTADOS_VALIDOS = { "Libre", "Ocupada", "Reservada", "Mantenimiento" };

        public MesaService(
            IMesaRepository mesaRepository,
            IOrdenRepository ordenRepository,
            IReservacionRepository reservacionRepository,
            IMapper mapper,
            ILogger<MesaService> logger)
        {
            _mesaRepository = mesaRepository;
            _ordenRepository = ordenRepository;
            _reservacionRepository = reservacionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DE MESAS
        // ============================================================================

        public async Task<IEnumerable<MesaResponse>> GetEstadoTodasLasMesasAsync()
        {
            try
            {
                _logger.LogInformation("🪑 Obteniendo estado de todas las mesas");

                var mesas = await _mesaRepository.GetAllWithIncludesAsync();
                var mesasResponse = _mapper.Map<IEnumerable<MesaResponse>>(mesas);

                // Agregar información adicional básica
                foreach (var mesa in mesasResponse)
                {
                    await EnriquecerInformacionMesaAsync(mesa);
                }

                _logger.LogInformation("✅ Estado de {CantidadMesas} mesas obtenido", mesasResponse.Count());
                return mesasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estado de todas las mesas");
                throw;
            }
        }

        public async Task<MesaResponse?> GetMesaByIdAsync(int mesaId)
        {
            try
            {
                var mesa = await _mesaRepository.GetByIdWithIncludesAsync(mesaId);
                if (mesa == null) return null;

                var mesaResponse = _mapper.Map<MesaResponse>(mesa);
                await EnriquecerInformacionMesaAsync(mesaResponse);

                return mesaResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener mesa {MesaId}", mesaId);
                throw;
            }
        }

        public async Task<IEnumerable<MesaResponse>> GetMesasPorEstadoAsync(string estado)
        {
            try
            {
                if (!ESTADOS_VALIDOS.Contains(estado))
                {
                    _logger.LogWarning("⚠️ Estado inválido solicitado: {Estado}", estado);
                    return Enumerable.Empty<MesaResponse>();
                }

                var mesas = await _mesaRepository.GetByEstadoAsync(estado);
                return _mapper.Map<IEnumerable<MesaResponse>>(mesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener mesas por estado {Estado}", estado);
                throw;
            }
        }

        public async Task<bool> CambiarEstadoMesaAsync(int mesaId, string nuevoEstado, int usuarioId, string? motivo = null)
        {
            try
            {
                if (!ESTADOS_VALIDOS.Contains(nuevoEstado))
                {
                    _logger.LogWarning("⚠️ Estado inválido: {Estado}", nuevoEstado);
                    return false;
                }

                // Validar cambio de estado
                var validacion = await ValidarCambioEstadoAsync(mesaId, nuevoEstado);
                if (!validacion.EsValida)
                {
                    var errores = string.Join(", ", validacion.Errores);
                    _logger.LogWarning("⚠️ Cambio de estado inválido para mesa {MesaId}: {Errores}", mesaId, errores);
                    return false;
                }

                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null) return false;

                var estadoAnterior = mesa.Estado;
                mesa.Estado = nuevoEstado;
                mesa.FechaUltimaActualizacion = DateTime.Now;

                await _mesaRepository.UpdateAsync(mesa);

                _logger.LogInformation("✅ Mesa {MesaId} cambió de {EstadoAnterior} a {NuevoEstado}. Motivo: {Motivo}", 
                    mesaId, estadoAnterior, nuevoEstado, motivo ?? "Sin especificar");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar estado de mesa {MesaId}", mesaId);
                return false;
            }
        }

        public async Task<bool> LiberarMesaAsync(int mesaId, int usuarioId)
        {
            try
            {
                // Verificar que no hay órdenes activas
                var puedeLiberarse = await PuedeLiberarseMesaAsync(mesaId);
                if (!puedeLiberarse)
                {
                    _logger.LogWarning("⚠️ Mesa {MesaId} no puede liberarse - tiene órdenes activas", mesaId);
                    return false;
                }

                return await CambiarEstadoMesaAsync(mesaId, "Libre", usuarioId, "Liberación automática post-facturación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al liberar mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // BÚSQUEDA Y DISPONIBILIDAD
        // ============================================================================

        public async Task<IEnumerable<MesaResponse>> BuscarMesasDisponiblesAsync(int? cantidadPersonas = null)
        {
            try
            {
                var mesasLibres = await _mesaRepository.GetByEstadoAsync("Libre");
                var mesasDisponibles = mesasLibres.AsEnumerable();

                // Filtrar por capacidad si se especifica
                if (cantidadPersonas.HasValue)
                {
                    mesasDisponibles = mesasDisponibles.Where(m => m.Capacidad >= cantidadPersonas.Value);
                }

                var resultado = mesasDisponibles.OrderBy(m => m.NumeroMesa);
                return _mapper.Map<IEnumerable<MesaResponse>>(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar mesas disponibles");
                throw;
            }
        }

        public async Task<MesaResponse?> GetPrimeraMesaDisponibleAsync(int cantidadPersonas)
        {
            try
            {
                var mesasDisponibles = await BuscarMesasDisponiblesAsync(cantidadPersonas);
                return mesasDisponibles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener primera mesa disponible");
                throw;
            }
        }

        public async Task<bool> VerificarDisponibilidadMesaAsync(int mesaId)
        {
            try
            {
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                return mesa != null && mesa.Estado == "Libre";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar disponibilidad de mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // RESERVAS Y OCUPACIÓN
        // ============================================================================

        public async Task<bool> OcuparMesaAsync(int mesaId, int usuarioId)
        {
            try
            {
                var disponible = await VerificarDisponibilidadMesaAsync(mesaId);
                if (!disponible)
                {
                    _logger.LogWarning("⚠️ Mesa {MesaId} no está disponible para ocupar", mesaId);
                    return false;
                }

                return await CambiarEstadoMesaAsync(mesaId, "Ocupada", usuarioId, "Cliente sentado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al ocupar mesa {MesaId}", mesaId);
                return false;
            }
        }

        public async Task<bool> ReservarMesaAsync(int mesaId, int reservacionId, int usuarioId)
        {
            try
            {
                var disponible = await VerificarDisponibilidadMesaAsync(mesaId);
                if (!disponible)
                {
                    _logger.LogWarning("⚠️ Mesa {MesaId} no está disponible para reservar", mesaId);
                    return false;
                }

                return await CambiarEstadoMesaAsync(mesaId, "Reservada", usuarioId, $"Reservación #{reservacionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al reservar mesa {MesaId}", mesaId);
                return false;
            }
        }

        public async Task<bool> LiberarReservaMesaAsync(int mesaId, bool activarOcupacion, int usuarioId)
        {
            try
            {
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null || mesa.Estado != "Reservada")
                {
                    _logger.LogWarning("⚠️ Mesa {MesaId} no está reservada", mesaId);
                    return false;
                }

                var nuevoEstado = activarOcupacion ? "Ocupada" : "Libre";
                var motivo = activarOcupacion ? "Cliente llegó - activando ocupación" : "Reserva cancelada";

                return await CambiarEstadoMesaAsync(mesaId, nuevoEstado, usuarioId, motivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al liberar reserva de mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // ESTADÍSTICAS
        // ============================================================================

        public async Task<EstadisticasMesasBasicasViewModel> GetEstadisticasBasicasAsync()
        {
            try
            {
                var todasLasMesas = await _mesaRepository.GetAllAsync();
                var totalMesas = todasLasMesas.Count();

                var estadisticas = new EstadisticasMesasBasicasViewModel
                {
                    TotalMesas = totalMesas,
                    MesasLibres = todasLasMesas.Count(m => m.Estado == "Libre"),
                    MesasOcupadas = todasLasMesas.Count(m => m.Estado == "Ocupada"),
                    MesasReservadas = todasLasMesas.Count(m => m.Estado == "Reservada"),
                    MesasMantenimiento = todasLasMesas.Count(m => m.Estado == "Mantenimiento")
                };

                estadisticas.PorcentajeOcupacion = totalMesas > 0 ? 
                    Math.Round((decimal)estadisticas.MesasOcupadas / totalMesas * 100, 2) : 0;

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas básicas");
                throw;
            }
        }

        public async Task<ResumenOcupacionDiaViewModel> GetResumenOcupacionDiaAsync(DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                var estadisticasActuales = await GetEstadisticasBasicasAsync();

                // Obtener órdenes del día para calcular clientes atendidos
                var ordenesDelDia = await _ordenRepository.GetOrdenesPorFechaAsync(fechaConsulta);
                var clientesAtendidos = ordenesDelDia.Select(o => o.ClienteID).Distinct().Count();

                return new ResumenOcupacionDiaViewModel
                {
                    Fecha = fechaConsulta,
                    MesasOcupadasMaximo = estadisticasActuales.MesasOcupadas, // Simplificado
                    PorcentajeOcupacionPromedio = estadisticasActuales.PorcentajeOcupacion,
                    TotalClientesAtendidos = clientesAtendidos,
                    TiempoPromedioOcupacion = "90 min", // Simplificado
                    VecesRotacionPromedio = 3 // Simplificado
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener resumen de ocupación del día");
                throw;
            }
        }

        public async Task<IEnumerable<MesaAtencionBasicaResponse>> GetMesasRequierenAtencionAsync(int tiempoLimiteMinutos = 180)
        {
            try
            {
                var mesasOcupadas = await _mesaRepository.GetByEstadoAsync("Ocupada");
                var mesasAtencion = new List<MesaAtencionBasicaResponse>();

                foreach (var mesa in mesasOcupadas)
                {
                    var tiempoOcupada = mesa.FechaUltimaActualizacion.HasValue 
                        ? DateTime.Now - mesa.FechaUltimaActualizacion.Value 
                        : TimeSpan.Zero; // Manejar nullable DateTime
                    if (tiempoOcupada.TotalMinutes > tiempoLimiteMinutos)
                    {
                        mesasAtencion.Add(new MesaAtencionBasicaResponse
                        {
                            MesaID = mesa.MesaID,
                            NumeroMesa = mesa.NumeroMesa,
                            Estado = mesa.Estado,
                            TiempoOcupada = $"{Math.Round(tiempoOcupada.TotalMinutes)} min",
                            MinutosOcupada = (int)tiempoOcupada.TotalMinutes,
                            RequiereRotacion = tiempoOcupada.TotalMinutes > tiempoLimiteMinutos,
                            Observaciones = "Mesa ocupada por tiempo prolongado"
                        });
                    }
                }

                return mesasAtencion.OrderByDescending(m => m.MinutosOcupada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener mesas que requieren atención");
                throw;
            }
        }

        // ============================================================================
        // VALIDACIONES
        // ============================================================================

        public async Task<ValidacionMesaResult> ValidarCambioEstadoAsync(int mesaId, string nuevoEstado)
        {
            var resultado = new ValidacionMesaResult { EstadoDeseado = nuevoEstado };

            try
            {
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null)
                {
                    resultado.Errores.Add("Mesa no encontrada");
                    return resultado;
                }

                resultado.EstadoActual = mesa.Estado;

                // Validaciones básicas de transición
                if (mesa.Estado == nuevoEstado)
                {
                    resultado.Advertencias.Add("La mesa ya está en ese estado");
                }

                // Validar transiciones específicas
                switch (nuevoEstado)
                {
                    case "Ocupada":
                        if (mesa.Estado != "Libre" && mesa.Estado != "Reservada")
                        {
                            resultado.Errores.Add("Solo se puede ocupar una mesa libre o reservada");
                        }
                        break;

                    case "Libre":
                        // Verificar que no hay órdenes activas
                        var ordenesActivas = await _ordenRepository.GetByMesaAsync(mesaId);
                        if (ordenesActivas.Any(o => o.Estado != "Facturada" && o.Estado != "Cancelada"))
                        {
                            resultado.Errores.Add("No se puede liberar mesa con órdenes activas");
                        }
                        break;

                    case "Reservada":
                        if (mesa.Estado != "Libre")
                        {
                            resultado.Errores.Add("Solo se puede reservar una mesa libre");
                        }
                        break;

                    case "Mantenimiento":
                        if (mesa.Estado == "Ocupada")
                        {
                            resultado.Errores.Add("No se puede poner en mantenimiento una mesa ocupada");
                        }
                        break;
                }

                resultado.EsValida = !resultado.Errores.Any();
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al validar cambio de estado de mesa {MesaId}", mesaId);
                resultado.Errores.Add("Error interno al validar");
                return resultado;
            }
        }

        public async Task<bool> PuedeLiberarseMesaAsync(int mesaId)
        {
            try
            {
                var ordenesActivas = await _ordenRepository.GetByMesaAsync(mesaId);
                return !ordenesActivas.Any(o => o.Estado != "Facturada" && o.Estado != "Cancelada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar si puede liberarse mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // UTILIDADES
        // ============================================================================

        public async Task<bool> ReiniciarTodasLasMesasAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("🔄 Reiniciando estado de todas las mesas");

                var todasLasMesas = await _mesaRepository.GetAllAsync();
                var mesasReiniciadas = 0;

                foreach (var mesa in todasLasMesas)
                {
                    // Solo reiniciar mesas que no están en mantenimiento
                    if (mesa.Estado != "Mantenimiento")
                    {
                        mesa.Estado = "Libre";
                        mesa.FechaUltimaActualizacion = DateTime.Now;
                        await _mesaRepository.UpdateAsync(mesa);
                        mesasReiniciadas++;
                    }
                }

                _logger.LogInformation("✅ {MesasReiniciadas} mesas reiniciadas exitosamente", mesasReiniciadas);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al reiniciar todas las mesas");
                return false;
            }
        }

        public async Task<bool> MarcarMesaMantenimientoAsync(int mesaId, string motivo, int usuarioId)
        {
            try
            {
                return await CambiarEstadoMesaAsync(mesaId, "Mantenimiento", usuarioId, $"Mantenimiento: {motivo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al marcar mesa {MesaId} para mantenimiento", mesaId);
                return false;
            }
        }

        public async Task<bool> CompletarMantenimientoMesaAsync(int mesaId, int usuarioId)
        {
            try
            {
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null || mesa.Estado != "Mantenimiento")
                {
                    _logger.LogWarning("⚠️ Mesa {MesaId} no está en mantenimiento", mesaId);
                    return false;
                }

                return await CambiarEstadoMesaAsync(mesaId, "Libre", usuarioId, "Mantenimiento completado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al completar mantenimiento de mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        private async Task EnriquecerInformacionMesaAsync(MesaResponse mesaResponse)
        {
            try
            {
                // Nota: las propiedades RequiereAtencion y TiempoHastaReserva son de solo lectura
                // y deben ser asignadas durante el mapeo en AutoMapper o venir del DTO original.
                // Solo podemos actualizar la propiedad TiempoOcupada si es necesario.
                if (mesaResponse.Estado == "Ocupada")
                {
                    var ordenesActivas = await _ordenRepository.GetByMesaAsync(mesaResponse.MesaID);
                    var ultimaOrden = ordenesActivas.OrderByDescending(o => o.FechaCreacion).FirstOrDefault();
                    
                    if (ultimaOrden != null)
                    {
                        var tiempoOcupada = DateTime.Now - ultimaOrden.FechaCreacion;
                        mesaResponse.TiempoOcupada = $"{Math.Round(tiempoOcupada.TotalMinutes)} min";
                        // RequiereAtencion es readonly - se debe calcular en el DTO o mapeo
                    }
                }
                // TiempoHastaReserva es readonly - se debe calcular en el DTO o mapeo
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error al enriquecer información de mesa {MesaId}", mesaResponse.MesaID);
                // No lanzar excepción, solo log de advertencia
            }
        }
    }
}