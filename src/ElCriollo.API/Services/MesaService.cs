using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de mesas para El Criollo
    /// Maneja estados dinámicos, asignación inteligente y optimización de rotación
    /// </summary>
    public class MesaService : IMesaService
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MesaService> _logger;

        // Configuración específica para El Criollo
        private const int TIEMPO_LIMITE_OCUPACION_MINUTOS = 150; // 2.5 horas
        private const int TIEMPO_LIMITE_ROTACION_MINUTOS = 180; // 3 horas
        private const int CAPACIDAD_MINIMA_ASIGNACION = 2;
        private const decimal FACTOR_OPTIMIZACION_CAPACIDAD = 0.8m; // 80% de ocupación óptima

        public MesaService(
            IMesaRepository mesaRepository,
            IReservacionRepository reservacionRepository,
            IOrdenRepository ordenRepository,
            IFacturaRepository facturaRepository,
            IMapper mapper,
            ILogger<MesaService> logger)
        {
            _mesaRepository = mesaRepository;
            _reservacionRepository = reservacionRepository;
            _ordenRepository = ordenRepository;
            _facturaRepository = facturaRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================================
        // GESTIÓN DE ESTADOS DE MESAS
        // ============================================================================

        /// <summary>
        /// Obtiene el estado actual de todas las mesas del restaurante
        /// </summary>
        public async Task<EstadoMesasViewModel> GetEstadoTodasLasMesasAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo estado de todas las mesas del restaurante");

                var mesas = await _mesaRepository.GetAllWithDetallesAsync();
                var estadisticas = await GetEstadisticasOcupacionAsync();

                var estadoMesas = new EstadoMesasViewModel
                {
                    FechaConsulta = DateTime.UtcNow,
                    TotalMesas = mesas.Count(),
                    MesasLibres = mesas.Count(m => m.EstadoMesa == "Libre"),
                    MesasOcupadas = mesas.Count(m => m.EstadoMesa == "Ocupada"),
                    MesasReservadas = mesas.Count(m => m.EstadoMesa == "Reservada"),
                    MesasMantenimiento = mesas.Count(m => m.EstadoMesa == "Mantenimiento"),
                    PorcentajeOcupacion = estadisticas.PorcentajeOcupacion,
                    CapacidadTotal = mesas.Sum(m => m.Capacidad),
                    CapacidadOcupada = mesas.Where(m => m.EstadoMesa == "Ocupada").Sum(m => m.Capacidad)
                };

                // Agrupar mesas por estado para mejor visualización
                estadoMesas.MesasPorEstado = new Dictionary<string, List<MesaResponse>>
                {
                    ["Libre"] = _mapper.Map<List<MesaResponse>>(mesas.Where(m => m.EstadoMesa == "Libre").OrderBy(m => m.Numero)),
                    ["Ocupada"] = _mapper.Map<List<MesaResponse>>(mesas.Where(m => m.EstadoMesa == "Ocupada").OrderBy(m => m.Numero)),
                    ["Reservada"] = _mapper.Map<List<MesaResponse>>(mesas.Where(m => m.EstadoMesa == "Reservada").OrderBy(m => m.Numero)),
                    ["Mantenimiento"] = _mapper.Map<List<MesaResponse>>(mesas.Where(m => m.EstadoMesa == "Mantenimiento").OrderBy(m => m.Numero))
                };

                // Enriquecer información de mesas ocupadas con tiempo de ocupación
                foreach (var mesa in estadoMesas.MesasPorEstado["Ocupada"])
                {
                    var mesaEntity = mesas.FirstOrDefault(m => m.MesaID == mesa.Id);
                    if (mesaEntity?.FechaOcupacion.HasValue == true)
                    {
                        mesa.TiempoOcupada = DateTime.UtcNow - mesaEntity.FechaOcupacion.Value;
                        mesa.RequiereAtencion = mesa.TiempoOcupada > TimeSpan.FromMinutes(TIEMPO_LIMITE_OCUPACION_MINUTOS);
                    }
                }

                _logger.LogInformation("Estado de mesas obtenido: {Libres} libres, {Ocupadas} ocupadas, {Reservadas} reservadas, {Mantenimiento} en mantenimiento",
                    estadoMesas.MesasLibres, estadoMesas.MesasOcupadas, estadoMesas.MesasReservadas, estadoMesas.MesasMantenimiento);

                return estadoMesas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de todas las mesas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene información detallada de una mesa específica
        /// </summary>
        public async Task<MesaResponse> GetMesaDetalleAsync(int mesaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo detalle de mesa ID: {MesaId}", mesaId);

                var mesa = await _mesaRepository.GetByIdWithDetallesAsync(mesaId);
                if (mesa == null)
                {
                    _logger.LogWarning("Mesa no encontrada: {MesaId}", mesaId);
                    return null;
                }

                var detalle = _mapper.Map<MesaResponse>(mesa);

                // Enriquecer con información adicional
                if (mesa.EstadoMesa == "Ocupada")
                {
                    detalle.OrdenesActivas = await GetOrdenesActivasPorMesaAsync(mesaId);
                    detalle.ConsumoActual = await CalcularConsumoMesaAsync(mesaId);
                    
                    if (mesa.FechaOcupacion.HasValue)
                    {
                        detalle.TiempoOcupada = DateTime.UtcNow - mesa.FechaOcupacion.Value;
                        detalle.RequiereRotacion = detalle.TiempoOcupada > TimeSpan.FromMinutes(TIEMPO_LIMITE_ROTACION_MINUTOS);
                    }
                }
                else if (mesa.EstadoMesa == "Reservada")
                {
                    var reservacion = await _reservacionRepository.GetReservacionActivaPorMesaAsync(mesaId);
                    if (reservacion != null)
                    {
                        detalle.ReservacionActiva = _mapper.Map<ReservacionResponse>(reservacion);
                        detalle.TiempoParaReserva = reservacion.FechaHora - DateTime.UtcNow;
                    }
                }

                return detalle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de mesa {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las mesas filtradas por estado
        /// </summary>
        public async Task<IEnumerable<MesaResponse>> GetMesasPorEstadoAsync(EstadoMesa estado)
        {
            try
            {
                _logger.LogDebug("Obteniendo mesas por estado: {Estado}", estado);

                var estadoString = estado.ToString();
                var mesas = await _mesaRepository.GetMesasPorEstadoAsync(estadoString);
                var mesasResponse = _mapper.Map<List<MesaResponse>>(mesas);

                // Enriquecer información según el estado
                foreach (var mesa in mesasResponse)
                {
                    var mesaEntity = mesas.FirstOrDefault(m => m.MesaID == mesa.Id);
                    if (mesaEntity?.FechaOcupacion.HasValue == true && estado == EstadoMesa.Ocupada)
                    {
                        mesa.TiempoOcupada = DateTime.UtcNow - mesaEntity.FechaOcupacion.Value;
                        mesa.RequiereAtencion = mesa.TiempoOcupada > TimeSpan.FromMinutes(TIEMPO_LIMITE_OCUPACION_MINUTOS);
                    }
                }

                _logger.LogDebug("Encontradas {Count} mesas en estado {Estado}", mesasResponse.Count, estado);

                return mesasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas por estado {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Cambia el estado de una mesa específica
        /// </summary>
        public async Task<CambioEstadoResult> CambiarEstadoMesaAsync(int mesaId, EstadoMesa nuevoEstado, int usuarioId, string? motivo = null)
        {
            try
            {
                _logger.LogInformation("Cambiando estado de mesa {MesaId} a {NuevoEstado} por usuario {UsuarioId}", 
                    mesaId, nuevoEstado, usuarioId);

                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null)
                {
                    return new CambioEstadoResult
                    {
                        Exitoso = false,
                        Mensaje = "Mesa no encontrada"
                    };
                }

                var estadoAnterior = Enum.Parse<EstadoMesa>(mesa.EstadoMesa);

                // Validaciones específicas según el cambio de estado
                var validacion = await ValidarCambioEstadoAsync(mesa, nuevoEstado);
                if (!validacion.esValido)
                {
                    return new CambioEstadoResult
                    {
                        Exitoso = false,
                        Mensaje = validacion.mensaje,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = nuevoEstado
                    };
                }

                // Ejecutar cambio de estado
                mesa.EstadoMesa = nuevoEstado.ToString();
                
                switch (nuevoEstado)
                {
                    case EstadoMesa.Ocupada:
                        mesa.FechaOcupacion = DateTime.UtcNow;
                        break;
                    case EstadoMesa.Libre:
                        mesa.FechaOcupacion = null;
                        mesa.FechaLiberacion = DateTime.UtcNow;
                        break;
                    case EstadoMesa.Mantenimiento:
                        mesa.FechaOcupacion = null;
                        break;
                }

                mesa.FechaModificacion = DateTime.UtcNow;
                await _mesaRepository.UpdateAsync(mesa);

                var result = new CambioEstadoResult
                {
                    Exitoso = true,
                    Mensaje = $"Mesa {mesa.Numero} cambiada de {estadoAnterior} a {nuevoEstado}" + 
                             (string.IsNullOrEmpty(motivo) ? "" : $". Motivo: {motivo}"),
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = nuevoEstado,
                    FechaCambio = DateTime.UtcNow,
                    Usuario = $"Usuario ID: {usuarioId}"
                };

                _logger.LogInformation("Estado de mesa cambiado exitosamente: {MesaId} de {EstadoAnterior} a {EstadoNuevo}", 
                    mesaId, estadoAnterior, nuevoEstado);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de mesa {MesaId}", mesaId);
                return new CambioEstadoResult
                {
                    Exitoso = false,
                    Mensaje = "Error interno al cambiar estado de mesa"
                };
            }
        }

        /// <summary>
        /// Libera una mesa automáticamente cuando se completa el servicio
        /// </summary>
        public async Task<bool> LiberarMesaAsync(int mesaId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Liberando mesa {MesaId} por usuario {UsuarioId}", mesaId, usuarioId);

                // Verificar que no hay órdenes pendientes
                var ordenesActivas = await GetOrdenesActivasPorMesaAsync(mesaId);
                if (ordenesActivas.Any(o => o.Estado != "Entregada" && o.Estado != "Facturada"))
                {
                    _logger.LogWarning("No se puede liberar mesa {MesaId} - Tiene órdenes pendientes", mesaId);
                    return false;
                }

                var resultado = await CambiarEstadoMesaAsync(mesaId, EstadoMesa.Libre, usuarioId, "Liberación automática después del servicio");
                
                if (resultado.Exitoso)
                {
                    _logger.LogInformation("Mesa {MesaId} liberada exitosamente", mesaId);
                }

                return resultado.Exitoso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // ASIGNACIÓN INTELIGENTE DE MESAS
        // ============================================================================

        /// <summary>
        /// Busca y asigna automáticamente la mejor mesa disponible
        /// </summary>
        public async Task<AsignacionMesaResult> AsignarMesaAutomaticaAsync(int cantidadPersonas, string? preferenciaUbicacion = null, int usuarioId = 0)
        {
            try
            {
                _logger.LogInformation("Asignando mesa automática para {CantidadPersonas} personas con preferencia: {Preferencia}", 
                    cantidadPersonas, preferenciaUbicacion ?? "Ninguna");

                var mesasDisponibles = await BuscarMesasDisponiblesAsync(cantidadPersonas);
                
                if (!mesasDisponibles.Any())
                {
                    _logger.LogWarning("No hay mesas disponibles para {CantidadPersonas} personas", cantidadPersonas);
                    return new AsignacionMesaResult
                    {
                        Exitoso = false,
                        Mensaje = $"No hay mesas disponibles para {cantidadPersonas} personas en este momento",
                        MesasAlternativas = new List<MesaResponse>()
                    };
                }

                // Aplicar lógica de asignación inteligente
                var mejorMesa = SeleccionarMejorMesa(mesasDisponibles, cantidadPersonas, preferenciaUbicacion);

                if (mejorMesa?.Mesa != null)
                {
                    // Asignar la mesa (marcar como ocupada)
                    var cambio = await CambiarEstadoMesaAsync(mejorMesa.Mesa.Id, EstadoMesa.Ocupada, usuarioId, 
                        $"Asignación automática para {cantidadPersonas} personas");

                    if (cambio.Exitoso)
                    {
                        return new AsignacionMesaResult
                        {
                            Exitoso = true,
                            MesaAsignada = mejorMesa.Mesa,
                            Mensaje = $"Mesa {mejorMesa.Mesa.Numero} asignada exitosamente",
                            RazonAsignacion = mejorMesa.RazonIdoneidad,
                            MesasAlternativas = mesasDisponibles.Skip(1).Take(3).Select(m => m.Mesa!).ToList()
                        };
                    }
                }

                return new AsignacionMesaResult
                {
                    Exitoso = false,
                    Mensaje = "Error al asignar mesa",
                    MesasAlternativas = mesasDisponibles.Take(5).Select(m => m.Mesa!).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en asignación automática de mesa");
                return new AsignacionMesaResult
                {
                    Exitoso = false,
                    Mensaje = "Error interno en asignación de mesa"
                };
            }
        }

        /// <summary>
        /// Busca mesas disponibles que cumplan criterios específicos
        /// </summary>
        public async Task<IEnumerable<MesaDisponibleResult>> BuscarMesasDisponiblesAsync(int cantidadPersonas, DateTime? fechaHora = null, int duracionMinutos = 120)
        {
            try
            {
                _logger.LogDebug("Buscando mesas disponibles para {CantidadPersonas} personas", cantidadPersonas);

                var fechaBusqueda = fechaHora ?? DateTime.UtcNow;
                var mesasLibres = await _mesaRepository.BuscarMesasDisponiblesAsync(fechaBusqueda, cantidadPersonas, duracionMinutos);

                var resultados = new List<MesaDisponibleResult>();

                foreach (var mesa in mesasLibres)
                {
                    var puntuacion = CalcularPuntuacionIdoneidad(mesa, cantidadPersonas);
                    var razonIdoneidad = GenerarRazonIdoneidad(mesa, cantidadPersonas, puntuacion);

                    resultados.Add(new MesaDisponibleResult
                    {
                        Mesa = _mapper.Map<MesaResponse>(mesa),
                        PuntuacionIdoneidad = puntuacion,
                        RazonIdoneidad = razonIdoneidad,
                        RequiereEspera = false,
                        TiempoEsperaMinutos = 0
                    });
                }

                // Ordenar por puntuación de idoneidad (mayor a menor)
                resultados = resultados.OrderByDescending(r => r.PuntuacionIdoneidad).ToList();

                _logger.LogDebug("Encontradas {Count} mesas disponibles para {CantidadPersonas} personas", 
                    resultados.Count, cantidadPersonas);

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar mesas disponibles");
                throw;
            }
        }

        /// <summary>
        /// Reserva una mesa específica para una fecha/hora
        /// </summary>
        public async Task<ReservaMesaResult> ReservarMesaAsync(int mesaId, DateTime fechaHora, int duracionMinutos, int? clienteId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Reservando mesa {MesaId} para {FechaHora} por {Duracion} minutos", 
                    mesaId, fechaHora, duracionMinutos);

                // Verificar disponibilidad de la mesa en la fecha/hora solicitada
                var disponible = await _mesaRepository.EstaDisponibleAsync(mesaId, fechaHora, duracionMinutos);
                if (!disponible)
                {
                    return new ReservaMesaResult
                    {
                        Exitoso = false,
                        Mensaje = "La mesa no está disponible en la fecha y hora solicitada"
                    };
                }

                // Crear la reservación
                var reservacion = new Reservacion
                {
                    MesaId = mesaId,
                    ClienteId = clienteId,
                    FechaHora = fechaHora,
                    DuracionMinutos = duracionMinutos,
                    Estado = "Confirmada",
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = usuarioId
                };

                var reservacionCreada = await _reservacionRepository.CreateAsync(reservacion);

                // Cambiar estado de la mesa si la reserva es para ahora
                if (fechaHora <= DateTime.UtcNow.AddMinutes(30))
                {
                    await CambiarEstadoMesaAsync(mesaId, EstadoMesa.Reservada, usuarioId, "Reservación confirmada");
                }

                _logger.LogInformation("Reservación creada exitosamente: ID {ReservacionId}", reservacionCreada.Id);

                return new ReservaMesaResult
                {
                    Exitoso = true,
                    Mensaje = "Mesa reservada exitosamente",
                    ReservacionId = reservacionCreada.Id,
                    FechaHoraReserva = fechaHora,
                    DuracionMinutos = duracionMinutos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reservar mesa {MesaId}", mesaId);
                return new ReservaMesaResult
                {
                    Exitoso = false,
                    Mensaje = "Error interno al procesar la reservación"
                };
            }
        }

        /// <summary>
        /// Optimiza la asignación de mesas para maximizar ocupación
        /// </summary>
        public async Task<OptimizacionResult> OptimizarAsignacionMesasAsync(List<SolicitudMesa> solicitudesPendientes)
        {
            try
            {
                _logger.LogInformation("Optimizando asignación para {Count} solicitudes pendientes", solicitudesPendientes.Count);

                var resultado = new OptimizacionResult();
                var mesasDisponibles = await _mesaRepository.GetMesasLibresAsync();

                // Algoritmo de optimización: asignar primero por capacidad exacta, luego por prioridad
                var solicitudesOrdenadas = solicitudesPendientes
                    .OrderByDescending(s => s.Prioridad)
                    .ThenBy(s => s.FechaHoraSolicitud)
                    .ToList();

                foreach (var solicitud in solicitudesOrdenadas)
                {
                    var mesaOptima = mesasDisponibles
                        .Where(m => m.Capacidad >= solicitud.CantidadPersonas)
                        .OrderBy(m => m.Capacidad) // Preferir mesa con capacidad más cercana
                        .ThenBy(m => CalcularPuntuacionUbicacion(m, solicitud.PreferenciaUbicacion))
                        .FirstOrDefault();

                    if (mesaOptima != null)
                    {
                        resultado.AsignacionesRecomendadas.Add(new AsignacionOptima
                        {
                            Solicitud = solicitud,
                            MesaRecomendada = _mapper.Map<MesaResponse>(mesaOptima),
                            TiempoEsperaMinutos = 0,
                            Justificacion = $"Capacidad exacta: {mesaOptima.Capacidad} personas"
                        });

                        // Remover mesa de disponibles para próxima iteración
                        mesasDisponibles = mesasDisponibles.Where(m => m.MesaID != mesaOptima.MesaID).ToList();
                    }
                    else
                    {
                        // No hay mesa disponible, calcular tiempo de espera estimado
                        var tiempoEspera = await CalcularTiempoEsperaEstimadoAsync(solicitud.CantidadPersonas);
                        
                        resultado.AsignacionesRecomendadas.Add(new AsignacionOptima
                        {
                            Solicitud = solicitud,
                            MesaRecomendada = null,
                            TiempoEsperaMinutos = tiempoEspera,
                            Justificacion = $"Sin mesa disponible. Tiempo de espera estimado: {tiempoEspera} minutos"
                        });
                    }
                }

                var asignacionesExitosas = resultado.AsignacionesRecomendadas.Count(a => a.MesaRecomendada != null);
                resultado.PorcentajeOptimizacion = solicitudesPendientes.Any() 
                    ? (decimal)asignacionesExitosas / solicitudesPendientes.Count * 100 
                    : 100;

                resultado.Observaciones = $"Optimización completada: {asignacionesExitosas}/{solicitudesPendientes.Count} asignaciones exitosas";

                _logger.LogInformation("Optimización completada con {Porcentaje}% de éxito", resultado.PorcentajeOptimizacion);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en optimización de asignación de mesas");
                throw;
            }
        }

        // ============================================================================
        // CONTROL DE OCUPACIÓN Y ROTACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas de ocupación en tiempo real
        /// </summary>
        public async Task<EstadisticasOcupacionResult> GetEstadisticasOcupacionAsync()
        {
            try
            {
                _logger.LogDebug("Calculando estadísticas de ocupación en tiempo real");

                var mesas = await _mesaRepository.GetAllAsync();
                var totalMesas = mesas.Count();
                var mesasLibres = mesas.Count(m => m.EstadoMesa == "Libre");
                var mesasOcupadas = mesas.Count(m => m.EstadoMesa == "Ocupada");
                var mesasReservadas = mesas.Count(m => m.EstadoMesa == "Reservada");
                var mesasMantenimiento = mesas.Count(m => m.EstadoMesa == "Mantenimiento");

                var capacidadMaxima = mesas.Sum(m => m.Capacidad);
                var capacidadOcupada = mesas.Where(m => m.EstadoMesa == "Ocupada").Sum(m => m.Capacidad);

                var tiempoPromedio = await CalcularTiempoPromedioOcupacionGlobalAsync();

                var estadisticas = new EstadisticasOcupacionResult
                {
                    TotalMesas = totalMesas,
                    MesasLibres = mesasLibres,
                    MesasOcupadas = mesasOcupadas,
                    MesasReservadas = mesasReservadas,
                    MesasMantenimiento = mesasMantenimiento,
                    PorcentajeOcupacion = totalMesas > 0 ? (decimal)mesasOcupadas / totalMesas * 100 : 0,
                    CapacidadMaxima = capacidadMaxima,
                    CapacidadActual = capacidadOcupada,
                    TiempoPromedioOcupacion = tiempoPromedio
                };

                _logger.LogDebug("Estadísticas calculadas: {PorcentajeOcupacion}% ocupación, {CapacidadOcupada}/{CapacidadMaxima} capacidad", 
                    estadisticas.PorcentajeOcupacion, capacidadOcupada, capacidadMaxima);

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular estadísticas de ocupación");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial de ocupación por período
        /// </summary>
        public async Task<HistorialOcupacionResult> GetHistorialOcupacionAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                _logger.LogDebug("Obteniendo historial de ocupación desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);

                var historial = await _mesaRepository.GetHistorialOcupacionAsync(fechaInicio, fechaFin);

                var resultado = new HistorialOcupacionResult
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    OcupacionPromedio = historial.OcupacionPromedio,
                    TotalRotaciones = historial.TotalRotaciones,
                    IngresosTotales = historial.IngresosTotales,
                    OcupacionPorDia = historial.OcupacionPorDia?.Select(o => new OcupacionDiaria
                    {
                        Fecha = o.Fecha,
                        PorcentajeOcupacion = o.PorcentajeOcupacion,
                        RotacionesTotales = o.RotacionesTotales,
                        IngresosDia = o.IngresosDia
                    }).ToList() ?? new List<OcupacionDiaria>()
                };

                _logger.LogDebug("Historial obtenido: {OcupacionPromedio}% ocupación promedio, {TotalRotaciones} rotaciones totales", 
                    resultado.OcupacionPromedio, resultado.TotalRotaciones);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de ocupación");
                throw;
            }
        }

        /// <summary>
        /// Calcula el tiempo promedio de ocupación por mesa
        /// </summary>
        public async Task<TiempoPromedioResult> CalcularTiempoPromedioOcupacionAsync(int? mesaId = null, int dias = 30)
        {
            try
            {
                _logger.LogDebug("Calculando tiempo promedio de ocupación para mesa {MesaId} en {Dias} días", 
                    mesaId?.ToString() ?? "todas", dias);

                var tiempos = await _mesaRepository.GetTiemposOcupacionAsync(mesaId, dias);

                if (!tiempos.Any())
                {
                    return new TiempoPromedioResult
                    {
                        TiempoPromedio = TimeSpan.Zero,
                        TiempoMinimo = TimeSpan.Zero,
                        TiempoMaximo = TimeSpan.Zero,
                        TotalOcupaciones = 0,
                        MesaEspecifica = mesaId?.ToString()
                    };
                }

                var resultado = new TiempoPromedioResult
                {
                    TiempoPromedio = TimeSpan.FromTicks((long)tiempos.Average(t => t.Ticks)),
                    TiempoMinimo = tiempos.Min(),
                    TiempoMaximo = tiempos.Max(),
                    TotalOcupaciones = tiempos.Count(),
                    MesaEspecifica = mesaId?.ToString()
                };

                _logger.LogDebug("Tiempo promedio calculado: {TiempoPromedio} basado en {TotalOcupaciones} ocupaciones", 
                    resultado.TiempoPromedio, resultado.TotalOcupaciones);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular tiempo promedio de ocupación");
                throw;
            }
        }

        /// <summary>
        /// Identifica mesas que requieren rotación por tiempo excesivo
        /// </summary>
        public async Task<IEnumerable<MesaAtencionResult>> GetMesasRequierenRotacionAsync(int? tiempoLimiteMinutos = null)
        {
            try
            {
                var tiempoLimite = tiempoLimiteMinutos ?? TIEMPO_LIMITE_ROTACION_MINUTOS;
                _logger.LogDebug("Buscando mesas que requieren rotación (límite: {TiempoLimite} minutos)", tiempoLimite);

                var mesasOcupadas = await _mesaRepository.GetMesasOcupadasConTiempoAsync();
                var resultados = new List<MesaAtencionResult>();

                foreach (var mesa in mesasOcupadas)
                {
                    if (mesa.FechaOcupacion.HasValue)
                    {
                        var tiempoOcupada = DateTime.UtcNow - mesa.FechaOcupacion.Value;
                        
                        if (tiempoOcupada.TotalMinutes > tiempoLimite)
                        {
                            var consumo = await CalcularConsumoMesaAsync(mesa.MesaID);
                            
                            resultados.Add(new MesaAtencionResult
                            {
                                Mesa = _mapper.Map<MesaResponse>(mesa),
                                TiempoOcupada = tiempoOcupada,
                                Urgencia = tiempoOcupada.TotalMinutes > tiempoLimite * 1.5 ? "Alta" : "Media",
                                Recomendacion = GenerarRecomendacionRotacion(tiempoOcupada, consumo.TotalConsumo),
                                ConsumoActual = consumo.TotalConsumo
                            });
                        }
                    }
                }

                resultados = resultados.OrderByDescending(r => r.TiempoOcupada).ToList();

                _logger.LogDebug("Encontradas {Count} mesas que requieren rotación", resultados.Count);

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas que requieren rotación");
                throw;
            }
        }

        /// <summary>
        /// Notifica sobre mesas que necesitan limpieza o mantenimiento
        /// </summary>
        public async Task<IEnumerable<MesaMantenimientoResult>> GetMesasRequierenMantenimientoAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo mesas que requieren mantenimiento");

                var mesasMantenimiento = await _mesaRepository.GetMesasRequierenMantenimientoAsync();

                var resultados = mesasMantenimiento.Select(m => new MesaMantenimientoResult
                {
                    Mesa = _mapper.Map<MesaResponse>(m),
                    TipoAtencionRequerida = DeterminarTipoAtencion(m),
                    Descripcion = GenerarDescripcionMantenimiento(m),
                    FechaReporte = m.FechaModificacion ?? DateTime.UtcNow,
                    UsuarioReporte = "Sistema",
                    Prioridad = DeterminarPrioridadMantenimiento(m)
                });

                _logger.LogDebug("Encontradas {Count} mesas que requieren mantenimiento", resultados.Count());

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas que requieren mantenimiento");
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE SERVICIOS POR MESA
        // ============================================================================

        /// <summary>
        /// Obtiene las órdenes activas de una mesa específica
        /// </summary>
        public async Task<IEnumerable<OrdenResponse>> GetOrdenesActivasPorMesaAsync(int mesaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo órdenes activas para mesa {MesaId}", mesaId);

                var ordenes = await _ordenRepository.GetOrdenesPorMesaAsync(mesaId);
                var ordenesActivas = ordenes.Where(o => o.Estado != "Entregada" && o.Estado != "Cancelada" && o.Estado != "Facturada");

                var ordenesResponse = _mapper.Map<List<OrdenResponse>>(ordenesActivas);

                _logger.LogDebug("Encontradas {Count} órdenes activas para mesa {MesaId}", ordenesResponse.Count, mesaId);

                return ordenesResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener órdenes activas de mesa {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Asigna una orden a una mesa específica
        /// </summary>
        public async Task<bool> AsignarOrdenAMesaAsync(int mesaId, int ordenId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Asignando orden {OrdenId} a mesa {MesaId} por usuario {UsuarioId}", 
                    ordenId, mesaId, usuarioId);

                var orden = await _ordenRepository.GetByIdAsync(ordenId);
                if (orden == null)
                {
                    _logger.LogWarning("Orden no encontrada: {OrdenId}", ordenId);
                    return false;
                }

                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null)
                {
                    _logger.LogWarning("Mesa no encontrada: {MesaId}", mesaId);
                    return false;
                }

                // Verificar que la mesa esté disponible para órdenes
                if (mesa.EstadoMesa != "Ocupada" && mesa.EstadoMesa != "Libre")
                {
                    _logger.LogWarning("Mesa {MesaId} no disponible para órdenes (Estado: {Estado})", mesaId, mesa.EstadoMesa);
                    return false;
                }

                // Asignar orden a mesa
                orden.MesaId = mesaId;
                orden.FechaModificacion = DateTime.UtcNow;
                await _ordenRepository.UpdateAsync(orden);

                // Si la mesa estaba libre, marcarla como ocupada
                if (mesa.EstadoMesa == "Libre")
                {
                    await CambiarEstadoMesaAsync(mesaId, EstadoMesa.Ocupada, usuarioId, "Asignación de orden");
                }

                _logger.LogInformation("Orden {OrdenId} asignada exitosamente a mesa {MesaId}", ordenId, mesaId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar orden {OrdenId} a mesa {MesaId}", ordenId, mesaId);
                return false;
            }
        }

        /// <summary>
        /// Calcula el total acumulado de consumo en una mesa
        /// </summary>
        public async Task<ConsumoMesaResult> CalcularConsumoMesaAsync(int mesaId)
        {
            try
            {
                _logger.LogDebug("Calculando consumo de mesa {MesaId}", mesaId);

                var ordenes = await _ordenRepository.GetOrdenesPorMesaAsync(mesaId);
                var ordenesActivas = ordenes.Where(o => o.Estado != "Cancelada");

                var resultado = new ConsumoMesaResult
                {
                    TotalConsumo = ordenesActivas.Sum(o => o.Total),
                    TotalOrdenes = ordenesActivas.Count(),
                    OrdenesActivas = _mapper.Map<List<OrdenResponse>>(ordenesActivas.Where(o => o.Estado != "Entregada" && o.Estado != "Facturada")),
                    InicioServicio = ordenesActivas.Any() ? ordenesActivas.Min(o => o.FechaCreacion) : null,
                    PromedioConsumo = ordenesActivas.Any() ? ordenesActivas.Average(o => o.Total) : 0
                };

                if (resultado.InicioServicio.HasValue)
                {
                    resultado.TiempoTranscurrido = DateTime.UtcNow - resultado.InicioServicio.Value;
                }

                _logger.LogDebug("Consumo calculado para mesa {MesaId}: RD$ {TotalConsumo} en {TotalOrdenes} órdenes", 
                    mesaId, resultado.TotalConsumo, resultado.TotalOrdenes);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular consumo de mesa {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Prepara una mesa para facturación (pre-cierre)
        /// </summary>
        public async Task<PreFacturacionResult> PrepararMesaParaFacturacionAsync(int mesaId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Preparando mesa {MesaId} para facturación por usuario {UsuarioId}", mesaId, usuarioId);

                var consumo = await CalcularConsumoMesaAsync(mesaId);
                var ordenesParaFacturar = consumo.OrdenesActivas.Where(o => o.Estado == "Entregada").ToList();

                var totalFacturable = ordenesParaFacturar.Sum(o => o.Total);
                var itbis = totalFacturable * 0.18m; // ITBIS 18% dominicano
                var totalConItbis = totalFacturable + itbis;

                var resultado = new PreFacturacionResult
                {
                    ListaParaFacturar = ordenesParaFacturar.Any(),
                    TotalConsumo = totalFacturable,
                    ITBIS = itbis,
                    TotalConITBIS = totalConItbis,
                    OrdenesParaFacturar = ordenesParaFacturar,
                    RequiereAtencionMesero = consumo.OrdenesActivas.Any(o => o.Estado != "Entregada")
                };

                if (!resultado.ListaParaFacturar)
                {
                    resultado.Observaciones = "No hay órdenes listas para facturar. Verificar estado de las órdenes.";
                }
                else if (resultado.RequiereAtencionMesero)
                {
                    resultado.Observaciones = "Hay órdenes pendientes. Confirmar entrega antes de facturar.";
                }
                else
                {
                    resultado.Observaciones = "Mesa lista para facturación completa.";
                }

                _logger.LogInformation("Pre-facturación calculada para mesa {MesaId}: RD$ {Total} ({OrdenesCount} órdenes)", 
                    mesaId, totalConItbis, ordenesParaFacturar.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar mesa {MesaId} para facturación", mesaId);
                throw;
            }
        }

        // ============================================================================
        // NOTIFICACIONES Y ALERTAS
        // ============================================================================

        /// <summary>
        /// Obtiene notificaciones activas relacionadas con mesas
        /// </summary>
        public async Task<IEnumerable<NotificacionMesa>> GetNotificacionesActivasAsync(int usuarioId)
        {
            try
            {
                _logger.LogDebug("Obteniendo notificaciones activas para usuario {UsuarioId}", usuarioId);

                var notificaciones = new List<NotificacionMesa>();

                // Mesas que requieren rotación
                var mesasRotacion = await GetMesasRequierenRotacionAsync();
                notificaciones.AddRange(mesasRotacion.Select(m => new NotificacionMesa
                {
                    MesaId = m.Mesa?.Id ?? 0,
                    NumeroMesa = m.Mesa?.Numero,
                    Tipo = "Rotación",
                    Mensaje = $"Mesa ocupada por {m.TiempoOcupada.Hours}h {m.TiempoOcupada.Minutes}m",
                    Urgencia = m.Urgencia,
                    FechaCreacion = DateTime.UtcNow,
                    Leida = false
                }));

                // Mesas que requieren mantenimiento
                var mesasMantenimiento = await GetMesasRequierenMantenimientoAsync();
                notificaciones.AddRange(mesasMantenimiento.Select(m => new NotificacionMesa
                {
                    MesaId = m.Mesa?.Id ?? 0,
                    NumeroMesa = m.Mesa?.Numero,
                    Tipo = "Mantenimiento",
                    Mensaje = m.Descripcion ?? "Requiere atención",
                    Urgencia = m.Prioridad,
                    FechaCreacion = m.FechaReporte,
                    Leida = false
                }));

                // Ordenar por urgencia y fecha
                notificaciones = notificaciones
                    .OrderBy(n => n.Urgencia == "Alta" ? 0 : n.Urgencia == "Media" ? 1 : 2)
                    .ThenByDescending(n => n.FechaCreacion)
                    .ToList();

                _logger.LogDebug("Encontradas {Count} notificaciones activas", notificaciones.Count);

                return notificaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones activas");
                throw;
            }
        }

        /// <summary>
        /// Marca una mesa como "requiere atención"
        /// </summary>
        public async Task<bool> MarcarMesaRequiereAtencionAsync(int mesaId, TipoAtencionMesa tipoAtencion, int usuarioId, string? notas = null)
        {
            try
            {
                _logger.LogInformation("Marcando mesa {MesaId} como requiere atención ({TipoAtencion}) por usuario {UsuarioId}", 
                    mesaId, tipoAtencion, usuarioId);

                // TODO: Implementar sistema de alertas/notificaciones persistente
                // Por ahora solo logeamos la acción

                _logger.LogInformation("Mesa {MesaId} marcada para atención: {TipoAtencion}. Notas: {Notas}", 
                    mesaId, tipoAtencion, notas ?? "Sin notas");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar mesa {MesaId} como requiere atención", mesaId);
                return false;
            }
        }

        /// <summary>
        /// Confirma que se atendió una mesa marcada
        /// </summary>
        public async Task<bool> ConfirmarAtencionMesaAsync(int mesaId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Confirmando atención de mesa {MesaId} por usuario {UsuarioId}", mesaId, usuarioId);

                // TODO: Implementar lógica de confirmación de atención
                // Por ahora solo logeamos la acción

                _logger.LogInformation("Atención de mesa {MesaId} confirmada por usuario {UsuarioId}", mesaId, usuarioId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar atención de mesa {MesaId}", mesaId);
                return false;
            }
        }

        // ============================================================================
        // CONFIGURACIÓN Y ADMINISTRACIÓN
        // ============================================================================

        /// <summary>
        /// Actualiza la configuración de una mesa
        /// </summary>
        public async Task<MesaResponse?> ActualizarConfiguracionMesaAsync(int mesaId, ConfiguracionMesa configuracion, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando configuración de mesa {MesaId} por usuario {UsuarioId}", mesaId, usuarioId);

                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null)
                {
                    _logger.LogWarning("Mesa no encontrada para actualizar: {MesaId}", mesaId);
                    return null;
                }

                // Actualizar campos de configuración
                if (configuracion.Capacidad.HasValue)
                    mesa.Capacidad = configuracion.Capacidad.Value;
                
                if (!string.IsNullOrWhiteSpace(configuracion.Ubicacion))
                    mesa.Ubicacion = configuracion.Ubicacion;
                
                if (!string.IsNullOrWhiteSpace(configuracion.Descripcion))
                    mesa.Descripcion = configuracion.Descripcion;
                
                if (configuracion.EstaActiva.HasValue)
                    mesa.EstaActiva = configuracion.EstaActiva.Value;

                mesa.FechaModificacion = DateTime.UtcNow;

                var mesaActualizada = await _mesaRepository.UpdateAsync(mesa);

                _logger.LogInformation("Configuración de mesa {MesaId} actualizada exitosamente", mesaId);

                return _mapper.Map<MesaResponse>(mesaActualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración de mesa {MesaId}", mesaId);
                throw;
            }
        }

        /// <summary>
        /// Bloquea/desbloquea una mesa temporalmente
        /// </summary>
        public async Task<bool> BloquearDesbloquearMesaAsync(int mesaId, bool bloquear, string motivo, int usuarioId)
        {
            try
            {
                var accion = bloquear ? "Bloqueando" : "Desbloqueando";
                _logger.LogInformation("{Accion} mesa {MesaId} por usuario {UsuarioId}. Motivo: {Motivo}", 
                    accion, mesaId, usuarioId, motivo);

                var nuevoEstado = bloquear ? EstadoMesa.Bloqueada : EstadoMesa.Libre;
                var resultado = await CambiarEstadoMesaAsync(mesaId, nuevoEstado, usuarioId, motivo);

                return resultado.Exitoso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al bloquear/desbloquear mesa {MesaId}", mesaId);
                return false;
            }
        }

        /// <summary>
        /// Reinicia el estado de todas las mesas (para inicio de día)
        /// </summary>
        public async Task<ReinicioMesasResult> ReiniciarEstadoTodasMesasAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Reiniciando estado de todas las mesas por usuario {UsuarioId}", usuarioId);

                var mesas = await _mesaRepository.GetAllAsync();
                var resultado = new ReinicioMesasResult
                {
                    FechaReinicio = DateTime.UtcNow
                };

                foreach (var mesa in mesas)
                {
                    try
                    {
                        // Solo reiniciar mesas que no estén en mantenimiento
                        if (mesa.EstadoMesa != "Mantenimiento")
                        {
                            var cambio = await CambiarEstadoMesaAsync(mesa.MesaID, EstadoMesa.Libre, usuarioId, 
                                "Reinicio de día - Todas las mesas libres");
                            
                            if (cambio.Exitoso)
                            {
                                resultado.MesasReiniciadas++;
                            }
                            else
                            {
                                resultado.MesasConProblemas++;
                                resultado.Observaciones.Add($"Mesa {mesa.Numero}: {cambio.Mensaje}");
                            }
                        }
                        else
                        {
                            resultado.Observaciones.Add($"Mesa {mesa.Numero}: Mantenida en estado Mantenimiento");
                        }
                    }
                    catch (Exception ex)
                    {
                        resultado.MesasConProblemas++;
                        resultado.Observaciones.Add($"Mesa {mesa.Numero}: Error - {ex.Message}");
                        _logger.LogError(ex, "Error al reiniciar mesa {MesaId}", mesa.MesaID);
                    }
                }

                resultado.Exitoso = resultado.MesasConProblemas == 0;

                _logger.LogInformation("Reinicio completado: {MesasReiniciadas} exitosas, {MesasConProblemas} con problemas", 
                    resultado.MesasReiniciadas, resultado.MesasConProblemas);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reiniciar estado de todas las mesas");
                return new ReinicioMesasResult
                {
                    Exitoso = false,
                    FechaReinicio = DateTime.UtcNow,
                    Observaciones = new List<string> { $"Error general: {ex.Message}" }
                };
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        /// <summary>
        /// Valida si un cambio de estado es permitido
        /// </summary>
        private async Task<(bool esValido, string mensaje)> ValidarCambioEstadoAsync(Mesa mesa, EstadoMesa nuevoEstado)
        {
            var estadoActual = Enum.Parse<EstadoMesa>(mesa.EstadoMesa);

            // Validaciones específicas según el cambio
            switch (nuevoEstado)
            {
                case EstadoMesa.Ocupada:
                    if (estadoActual == EstadoMesa.Ocupada)
                        return (false, "La mesa ya está ocupada");
                    if (estadoActual == EstadoMesa.Mantenimiento)
                        return (false, "No se puede ocupar una mesa en mantenimiento");
                    break;

                case EstadoMesa.Libre:
                    if (estadoActual == EstadoMesa.Ocupada)
                    {
                        // Verificar que no haya órdenes pendientes
                        var ordenesActivas = await GetOrdenesActivasPorMesaAsync(mesa.MesaID);
                        if (ordenesActivas.Any(o => o.Estado != "Entregada" && o.Estado != "Facturada"))
                            return (false, "No se puede liberar la mesa - Hay órdenes pendientes");
                    }
                    break;

                case EstadoMesa.Reservada:
                    if (estadoActual == EstadoMesa.Ocupada)
                        return (false, "No se puede reservar una mesa ocupada");
                    break;
            }

            return (true, "Cambio de estado válido");
        }

        /// <summary>
        /// Calcula puntuación de idoneidad para asignación de mesa
        /// </summary>
        private int CalcularPuntuacionIdoneidad(Mesa mesa, int cantidadPersonas)
        {
            int puntuacion = 100;

            // Penalizar por sobrecapacidad o subcapacidad
            var diferencia = Math.Abs(mesa.Capacidad - cantidadPersonas);
            puntuacion -= diferencia * 10;

            // Bonificar si la capacidad es exacta o ligeramente mayor
            if (mesa.Capacidad == cantidadPersonas)
                puntuacion += 50;
            else if (mesa.Capacidad == cantidadPersonas + 1)
                puntuacion += 25;

            // Bonificar por ubicación preferencial
            if (mesa.Ubicacion?.Contains("Ventana") == true)
                puntuacion += 10;

            return Math.Max(0, puntuacion);
        }

        /// <summary>
        /// Genera razón de idoneidad para una mesa
        /// </summary>
        private string GenerarRazonIdoneidad(Mesa mesa, int cantidadPersonas, int puntuacion)
        {
            if (mesa.Capacidad == cantidadPersonas)
                return "Capacidad exacta - Asignación perfecta";
            
            if (mesa.Capacidad > cantidadPersonas)
                return $"Capacidad suficiente ({mesa.Capacidad} personas) - Cómoda para {cantidadPersonas}";
            
            return $"Capacidad justa ({mesa.Capacidad} personas) para {cantidadPersonas}";
        }

        /// <summary>
        /// Selecciona la mejor mesa de las disponibles
        /// </summary>
        private MesaDisponibleResult? SeleccionarMejorMesa(IEnumerable<MesaDisponibleResult> mesasDisponibles, 
            int cantidadPersonas, string? preferenciaUbicacion)
        {
            var mesas = mesasDisponibles.ToList();
            
            if (!mesas.Any())
                return null;

            // Aplicar preferencia de ubicación si existe
            if (!string.IsNullOrWhiteSpace(preferenciaUbicacion))
            {
                var mesasConPreferencia = mesas.Where(m => 
                    m.Mesa?.Ubicacion?.Contains(preferenciaUbicacion, StringComparison.OrdinalIgnoreCase) == true);
                
                if (mesasConPreferencia.Any())
                    return mesasConPreferencia.OrderByDescending(m => m.PuntuacionIdoneidad).First();
            }

            // Seleccionar la mesa con mayor puntuación
            return mesas.OrderByDescending(m => m.PuntuacionIdoneidad).First();
        }

        /// <summary>
        /// Calcula puntuación de ubicación según preferencia
        /// </summary>
        private int CalcularPuntuacionUbicacion(Mesa mesa, string? preferenciaUbicacion)
        {
            if (string.IsNullOrWhiteSpace(preferenciaUbicacion) || string.IsNullOrWhiteSpace(mesa.Ubicacion))
                return 0;

            return mesa.Ubicacion.Contains(preferenciaUbicacion, StringComparison.OrdinalIgnoreCase) ? 10 : 0;
        }

        /// <summary>
        /// Calcula tiempo de espera estimado para una capacidad específica
        /// </summary>
        private async Task<int> CalcularTiempoEsperaEstimadoAsync(int cantidadPersonas)
        {
            try
            {
                var tiempoPromedio = await CalcularTiempoPromedioOcupacionAsync(null, 7);
                
                // Estimar basado en tiempo promedio de ocupación
                var tiempoEsperaBase = (int)tiempoPromedio.TiempoPromedio.TotalMinutes / 2;
                
                // Ajustar según capacidad solicitada
                if (cantidadPersonas > 6)
                    tiempoEsperaBase += 30; // Mesas grandes tardan más en liberarse
                
                return Math.Max(15, tiempoEsperaBase); // Mínimo 15 minutos
            }
            catch
            {
                return 60; // Default: 1 hora de espera
            }
        }

        /// <summary>
        /// Calcula tiempo promedio global de ocupación
        /// </summary>
        private async Task<TimeSpan> CalcularTiempoPromedioOcupacionGlobalAsync()
        {
            try
            {
                var resultado = await CalcularTiempoPromedioOcupacionAsync(null, 7);
                return resultado.TiempoPromedio;
            }
            catch
            {
                return TimeSpan.FromMinutes(90); // Default: 1.5 horas
            }
        }

        /// <summary>
        /// Genera recomendación de rotación basada en tiempo y consumo
        /// </summary>
        private string GenerarRecomendacionRotacion(TimeSpan tiempoOcupada, decimal consumo)
        {
            if (tiempoOcupada.TotalMinutes > 240) // 4 horas
                return "Tiempo excesivo - Considerar cortés invitación a finalizar";
            
            if (tiempoOcupada.TotalMinutes > 180 && consumo < 500) // 3 horas con bajo consumo
                return "Revisar si el cliente necesita algo más o está listo para la cuenta";
            
            return "Verificar satisfacción del cliente y ofrecer servicios adicionales";
        }

        /// <summary>
        /// Determina tipo de atención requerida para mantenimiento
        /// </summary>
        private TipoAtencionMesa DeterminarTipoAtencion(Mesa mesa)
        {
            if (mesa.EstadoMesa == "Mantenimiento")
                return TipoAtencionMesa.Mantenimiento;
            
            // Lógica adicional según las reglas de negocio
            return TipoAtencionMesa.Limpieza;
        }

        /// <summary>
        /// Genera descripción de mantenimiento requerido
        /// </summary>
        private string GenerarDescripcionMantenimiento(Mesa mesa)
        {
            if (mesa.EstadoMesa == "Mantenimiento")
                return $"Mesa {mesa.Numero} requiere mantenimiento programado";
            
            return $"Mesa {mesa.Numero} requiere limpieza profunda";
        }

        /// <summary>
        /// Determina prioridad de mantenimiento
        /// </summary>
        private string DeterminarPrioridadMantenimiento(Mesa mesa)
        {
            if (mesa.EstadoMesa == "Mantenimiento")
                return "Alta";
            
            return "Media";
        }
    }
}