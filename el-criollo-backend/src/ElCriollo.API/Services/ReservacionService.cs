using AutoMapper;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Implementación básica del servicio de reservas para El Criollo
    /// </summary>
    public class ReservacionService : IReservacionService
    {
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservacionService> _logger;

        // Configuración básica para El Criollo
        private const int DURACION_DEFAULT_MINUTOS = 120; // 2 horas por defecto
        private const int TOLERANCIA_DEFAULT_MINUTOS = 15; // 15 minutos de tolerancia
        private const int RECORDATORIO_DEFAULT_MINUTOS = 60; // 1 hora antes
        
        // Zona horaria de República Dominicana (UTC-4)
        private static readonly TimeZoneInfo DominicanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");

        public ReservacionService(
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IClienteRepository clienteRepository,
            IEmailService emailService,
            IMapper mapper,
            ILogger<ReservacionService> logger)
        {
            _reservacionRepository = reservacionRepository;
            _mesaRepository = mesaRepository;
            _clienteRepository = clienteRepository;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la fecha/hora actual en zona horaria dominicana
        /// </summary>
        private DateTime GetDominicanNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DominicanTimeZone);
        }

        /// <summary>
        /// Obtiene la fecha actual en zona horaria dominicana
        /// </summary>
        private DateTime GetDominicanToday()
        {
            return GetDominicanNow().Date;
        }

        /// <summary>
        /// Convierte una fecha local dominicana a UTC
        /// </summary>
        private DateTime ConvertToUtc(DateTime dominicanTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dominicanTime, DominicanTimeZone);
        }

        // ============================================================================
        // GESTIÓN BÁSICA DE RESERVAS
        // ============================================================================

        /// <summary>
        /// Crea una nueva reserva validando disponibilidad
        /// </summary>
        public async Task<ReservacionResponse> CrearReservaAsync(CreateReservacionRequest crearReservaRequest, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando nueva reserva para {CantidadPersonas} personas el {FechaHora} por usuario {UsuarioId}", 
                    crearReservaRequest.CantidadPersonas, crearReservaRequest.FechaHora, usuarioId);

                // Validar la solicitud
                var validacion = await ValidarSolicitudReservaAsync(crearReservaRequest);
                if (!validacion.EsValida)
                {
                    var errores = string.Join(", ", validacion.Errores);
                    throw new InvalidOperationException($"Reserva inválida: {errores}");
                }

                // Buscar mesa si no se especificó una
                int mesaId = crearReservaRequest.MesaId ?? 0;
                if (mesaId == 0)
                {
                    var mesasDisponibles = await BuscarMesasDisponiblesParaReservaAsync(
                        crearReservaRequest.FechaHora, 
                        crearReservaRequest.CantidadPersonas, 
                        crearReservaRequest.DuracionMinutos ?? DURACION_DEFAULT_MINUTOS);

                    var mejorMesa = mesasDisponibles.FirstOrDefault();
                    if (mejorMesa == null)
                    {
                        throw new InvalidOperationException("No hay mesas disponibles para la fecha y hora solicitada");
                    }
                    mesaId = mejorMesa.Id;
                }

                // Validar que se proporcione un cliente
                if (!crearReservaRequest.ClienteId.HasValue)
                {
                    throw new InvalidOperationException("Se debe especificar un cliente para la reservación");
                }

                // Crear la reserva
                var nuevaReserva = new Reservacion
                {
                    ClienteId = crearReservaRequest.ClienteId.Value,
                    MesaId = mesaId,
                    FechaHora = crearReservaRequest.FechaHora,
                    CantidadPersonas = crearReservaRequest.CantidadPersonas,
                    DuracionMinutos = crearReservaRequest.DuracionMinutos ?? DURACION_DEFAULT_MINUTOS,
                    Estado = "Pendiente",
                    NotasEspeciales = crearReservaRequest.NotasEspeciales,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = usuarioId
                };

                var reservaCreada = await _reservacionRepository.CreateAsync(nuevaReserva);

                _logger.LogInformation("Reserva creada exitosamente: ID {ReservaId} para mesa {MesaId}", 
                    reservaCreada.Id, mesaId);

                return _mapper.Map<ReservacionResponse>(reservaCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una reserva por su ID
        /// </summary>
        public async Task<ReservacionResponse?> GetReservaByIdAsync(int reservaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo reserva ID: {ReservaId}", reservaId);

                var reserva = await _reservacionRepository.GetByIdWithDetallesAsync(reservaId);
                if (reserva == null)
                {
                    _logger.LogWarning("Reserva no encontrada: {ReservaId}", reservaId);
                    return null;
                }

                return _mapper.Map<ReservacionResponse>(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva {ReservaId}", reservaId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las reservas de una fecha específica
        /// </summary>
        public async Task<IEnumerable<ReservacionResponse>> GetReservasPorFechaAsync(DateTime fecha)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservas para fecha: {Fecha}", fecha.ToString("yyyy-MM-dd"));

                // Convertir la fecha a UTC para consultar correctamente
                var fechaUtcInicio = ConvertToUtc(fecha.Date);
                var fechaUtcFin = ConvertToUtc(fecha.Date.AddDays(1));

                var reservas = await _reservacionRepository.GetReservacionesPorRangoFechasAsync(fechaUtcInicio, fechaUtcFin);
                var reservasResponse = _mapper.Map<List<ReservacionResponse>>(reservas);

                _logger.LogDebug("Encontradas {Count} reservas para {Fecha} (UTC: {FechaUtcInicio} - {FechaUtcFin})", 
                    reservasResponse.Count, fecha.ToString("yyyy-MM-dd"), fechaUtcInicio, fechaUtcFin);

                return reservasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas por fecha {Fecha}", fecha);
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas por estado
        /// </summary>
        public async Task<IEnumerable<ReservacionResponse>> GetReservasPorEstadoAsync(string estado)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservas por estado: {Estado}", estado);

                var reservas = await _reservacionRepository.GetReservasPorEstadoAsync(estado);
                var reservasResponse = _mapper.Map<List<ReservacionResponse>>(reservas);

                _logger.LogDebug("Encontradas {Count} reservas en estado {Estado}", reservasResponse.Count, estado);

                return reservasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas por estado {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Actualiza una reserva existente
        /// </summary>
        public async Task<ReservacionResponse?> ActualizarReservaAsync(int reservaId, ActualizarReservacionRequest actualizarRequest, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando reserva {ReservaId} por usuario {UsuarioId}", reservaId, usuarioId);

                var reserva = await _reservacionRepository.GetByIdAsync(reservaId);
                if (reserva == null)
                {
                    _logger.LogWarning("Reserva no encontrada para actualizar: {ReservaId}", reservaId);
                    return null;
                }

                // Solo permitir actualizar reservas pendientes o confirmadas
                if (reserva.Estado == "Completada" || reserva.Estado == "Cancelada" || reserva.Estado == "NoShow")
                {
                    throw new InvalidOperationException($"No se puede actualizar una reserva en estado {reserva.Estado}");
                }

                // Actualizar campos si se proporcionan
                if (actualizarRequest.FechaHora.HasValue)
                {
                    // Validar nueva fecha/hora
                    var disponible = await VerificarDisponibilidadMesaAsync(
                        reserva.MesaId, 
                        actualizarRequest.FechaHora.Value, 
                        actualizarRequest.DuracionMinutos ?? reserva.DuracionMinutos);
                    
                    if (!disponible)
                    {
                        throw new InvalidOperationException("La mesa no está disponible en la nueva fecha/hora");
                    }
                    
                    reserva.FechaHora = actualizarRequest.FechaHora.Value;
                }

                if (actualizarRequest.CantidadPersonas.HasValue)
                    reserva.CantidadPersonas = actualizarRequest.CantidadPersonas.Value;

                if (actualizarRequest.MesaId.HasValue)
                {
                    // Validar disponibilidad de nueva mesa
                    var disponible = await VerificarDisponibilidadMesaAsync(
                        actualizarRequest.MesaId.Value, 
                        reserva.FechaHora, 
                        reserva.DuracionMinutos);
                    
                    if (!disponible)
                    {
                        throw new InvalidOperationException("La nueva mesa no está disponible");
                    }
                    
                    reserva.MesaId = actualizarRequest.MesaId.Value;
                }

                if (!string.IsNullOrWhiteSpace(actualizarRequest.NotasEspeciales))
                    reserva.NotasEspeciales = actualizarRequest.NotasEspeciales;

                if (actualizarRequest.DuracionMinutos.HasValue)
                    reserva.DuracionMinutos = actualizarRequest.DuracionMinutos.Value;

                reserva.FechaModificacion = DateTime.UtcNow;

                var reservaActualizada = await _reservacionRepository.UpdateAsync(reserva);

                _logger.LogInformation("Reserva actualizada exitosamente: {ReservaId}", reservaId);

                return _mapper.Map<ReservacionResponse>(reservaActualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva {ReservaId}", reservaId);
                throw;
            }
        }

        /// <summary>
        /// Cancela una reserva
        /// </summary>
        public async Task<bool> CancelarReservaAsync(int reservaId, string motivo, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Cancelando reserva {ReservaId} por usuario {UsuarioId}. Motivo: {Motivo}", 
                    reservaId, usuarioId, motivo);

                var reserva = await _reservacionRepository.GetByIdAsync(reservaId);
                if (reserva == null)
                {
                    _logger.LogWarning("Reserva no encontrada para cancelar: {ReservaId}", reservaId);
                    return false;
                }

                // Solo permitir cancelar reservas que no estén completadas
                if (reserva.Estado == "Completada" || reserva.Estado == "Cancelada")
                {
                    _logger.LogWarning("No se puede cancelar reserva en estado {Estado}", reserva.Estado);
                    return false;
                }

                reserva.Estado = "Cancelada";
                reserva.NotasEspeciales = $"{reserva.NotasEspeciales}\nCANCELADA: {motivo}";
                reserva.FechaModificacion = DateTime.UtcNow;

                await _reservacionRepository.UpdateAsync(reserva);

                _logger.LogInformation("Reserva cancelada exitosamente: {ReservaId}", reservaId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva {ReservaId}", reservaId);
                return false;
            }
        }

        // ============================================================================
        // VALIDACIONES Y DISPONIBILIDAD
        // ============================================================================

        /// <summary>
        /// Verifica si una mesa está disponible en fecha/hora específica
        /// </summary>
        public async Task<bool> VerificarDisponibilidadMesaAsync(int mesaId, DateTime fechaHora, int duracionMinutos)
        {
            try
            {
                _logger.LogDebug("Verificando disponibilidad mesa {MesaId} para {FechaHora}", mesaId, fechaHora);

                // Verificar que la mesa exista y esté activa
                var mesa = await _mesaRepository.GetByIdAsync(mesaId);
                if (mesa == null || !mesa.EstaActiva)
                {
                    return false;
                }

                // Verificar que no hay conflicto con otras reservas
                var fechaInicio = fechaHora;
                var fechaFin = fechaHora.AddMinutes(duracionMinutos);

                var reservasConflicto = await _reservacionRepository.GetReservasEnRangoAsync(
                    mesaId, fechaInicio, fechaFin);

                // Filtrar solo reservas activas (no canceladas)
                var reservasActivas = reservasConflicto.Where(r => 
                    r.Estado != "Cancelada" && r.Estado != "NoShow");

                var hayConflicto = reservasActivas.Any();

                _logger.LogDebug("Disponibilidad mesa {MesaId}: {Disponible} (Conflictos: {Conflictos})", 
                    mesaId, !hayConflicto, reservasActivas.Count());

                return !hayConflicto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad mesa {MesaId}", mesaId);
                return false;
            }
        }

        /// <summary>
        /// Busca mesas disponibles para una fecha/hora y número de personas
        /// </summary>
        public async Task<IEnumerable<MesaResponse>> BuscarMesasDisponiblesParaReservaAsync(DateTime fechaHora, int cantidadPersonas, int duracionMinutos = 120)
        {
            try
            {
                _logger.LogDebug("Buscando mesas disponibles para {CantidadPersonas} personas el {FechaHora}", 
                    cantidadPersonas, fechaHora);

                // Obtener todas las mesas que puedan acomodar la cantidad de personas
                var todasLasMesas = await _mesaRepository.GetMesasActivasAsync();
                var mesasAdecuadas = todasLasMesas.Where(m => m.Capacidad >= cantidadPersonas);

                var mesasDisponibles = new List<Mesa>();

                // Verificar disponibilidad de cada mesa
                foreach (var mesa in mesasAdecuadas)
                {
                    var disponible = await VerificarDisponibilidadMesaAsync(mesa.MesaID, fechaHora, duracionMinutos);
                    if (disponible)
                    {
                        mesasDisponibles.Add(mesa);
                    }
                }

                // Ordenar por capacidad (preferir mesas con capacidad más cercana)
                var mesasOrdenadas = mesasDisponibles
                    .OrderBy(m => m.Capacidad)
                    .ThenBy(m => m.NumeroMesa);

                var resultado = _mapper.Map<List<MesaResponse>>(mesasOrdenadas);

                _logger.LogDebug("Encontradas {Count} mesas disponibles para {CantidadPersonas} personas", 
                    resultado.Count, cantidadPersonas);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar mesas disponibles");
                throw;
            }
        }

        /// <summary>
        /// Valida una solicitud de reserva
        /// </summary>
        public async Task<ValidacionReservaResult> ValidarSolicitudReservaAsync(CreateReservacionRequest request)
        {
            try
            {
                _logger.LogDebug("Validando solicitud de reserva");

                var resultado = new ValidacionReservaResult();

                // Validaciones básicas
                if (request.FechaHora <= DateTime.UtcNow)
                {
                    resultado.Errores.Add("La fecha de reserva debe ser futura");
                }

                if (request.CantidadPersonas <= 0)
                {
                    resultado.Errores.Add("La cantidad de personas debe ser mayor a cero");
                }

                if (request.CantidadPersonas > 12) // Capacidad máxima del restaurante
                {
                    resultado.Errores.Add("La cantidad máxima de personas por reserva es 12");
                }

                // Validar cliente si se especifica
                if (request.ClienteId.HasValue)
                {
                    var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId.Value);
                    if (cliente == null)
                    {
                        resultado.Errores.Add("Cliente no encontrado");
                    }
                }

                // Validar disponibilidad
                if (!resultado.Errores.Any())
                {
                    if (request.MesaId.HasValue)
                    {
                        // Si se especifica una mesa, validar solo esa
                        var mesa = await _mesaRepository.GetByIdAsync(request.MesaId.Value);
                        if (mesa == null)
                        {
                            resultado.Errores.Add("La mesa especificada no existe.");
                        }
                        else if (mesa.Capacidad < request.CantidadPersonas)
                        {
                            resultado.Errores.Add($"La mesa {mesa.NumeroMesa} no tiene capacidad para {request.CantidadPersonas} personas.");
                        }
                        else
                        {
                            var disponible = await VerificarDisponibilidadMesaAsync(
                                request.MesaId.Value,
                                request.FechaHora,
                                request.DuracionMinutos ?? DURACION_DEFAULT_MINUTOS
                            );

                            if (!disponible)
                            {
                                resultado.Errores.Add($"La mesa {mesa.NumeroMesa} no está disponible en el horario solicitado.");
                            }
                        }
                    }
                    else
                    {
                        // Si no se especifica mesa, buscar una disponible
                        var mesasDisponibles = await BuscarMesasDisponiblesParaReservaAsync(
                            request.FechaHora,
                            request.CantidadPersonas,
                            request.DuracionMinutos ?? DURACION_DEFAULT_MINUTOS);

                        if (!mesasDisponibles.Any())
                        {
                            resultado.Errores.Add("No hay mesas disponibles para la fecha y hora solicitada");

                            // Buscar alternativas en horas cercanas
                            var alternativas = await BuscarMesasAlternativasAsync(request.FechaHora, request.CantidadPersonas);
                            resultado.MesasAlternativas = alternativas.ToList();
                        }
                    }
                }

                resultado.EsValida = !resultado.Errores.Any();

                if (resultado.EsValida)
                {
                    resultado.Mensaje = "Solicitud de reserva válida";
                }
                else
                {
                    resultado.Mensaje = $"Errores encontrados: {string.Join(", ", resultado.Errores)}";
                }

                _logger.LogDebug("Validación completada - Válida: {EsValida}, Errores: {Errores}", 
                    resultado.EsValida, resultado.Errores.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar solicitud de reserva");
                throw;
            }
        }

        // ============================================================================
        // GESTIÓN DE ESTADOS
        // ============================================================================

        /// <summary>
        /// Confirma una reserva
        /// </summary>
        public async Task<bool> ConfirmarReservaAsync(int reservaId, int usuarioId)
        {
            return await CambiarEstadoReservaAsync(reservaId, "Confirmada", usuarioId, "Reserva confirmada");
        }

        /// <summary>
        /// Marca una reserva como "Cliente Llegó"
        /// </summary>
        public async Task<bool> MarcarClienteLlegoAsync(int reservaId, int usuarioId)
        {
            return await CambiarEstadoReservaAsync(reservaId, "ClienteLlego", usuarioId, "Cliente llegó");
        }

        /// <summary>
        /// Marca una reserva como "No Show"
        /// </summary>
        public async Task<bool> MarcarNoShowAsync(int reservaId, int usuarioId)
        {
            return await CambiarEstadoReservaAsync(reservaId, "NoShow", usuarioId, "Cliente no se presentó");
        }

        /// <summary>
        /// Completa una reserva
        /// </summary>
        public async Task<bool> CompletarReservaAsync(int reservaId, int usuarioId)
        {
            return await CambiarEstadoReservaAsync(reservaId, "Completada", usuarioId, "Reserva completada");
        }

        // ============================================================================
        // NOTIFICACIONES Y RECORDATORIOS
        // ============================================================================

        /// <summary>
        /// Obtiene reservas que requieren recordatorio
        /// </summary>
        public async Task<IEnumerable<ReservacionResponse>> GetReservasParaRecordatorioAsync(int minutosAntes = 60)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservas para recordatorio ({MinutosAntes} minutos antes)", minutosAntes);

                var fechaLimite = DateTime.UtcNow.AddMinutes(minutosAntes);
                var reservas = await _reservacionRepository.GetReservasParaRecordatorioAsync(fechaLimite);

                var reservasResponse = _mapper.Map<List<ReservacionResponse>>(reservas);

                _logger.LogDebug("Encontradas {Count} reservas para recordatorio", reservasResponse.Count);

                return reservasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas para recordatorio");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas vencidas
        /// </summary>
        public async Task<IEnumerable<ReservacionResponse>> GetReservasVencidasAsync(int minutosTolerancia = 15)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservas vencidas (tolerancia: {MinutosTolerancia} minutos)", minutosTolerancia);

                var fechaLimite = DateTime.UtcNow.AddMinutes(-minutosTolerancia);
                var reservas = await _reservacionRepository.GetReservasVencidasAsync(fechaLimite);

                var reservasResponse = _mapper.Map<List<ReservacionResponse>>(reservas);

                _logger.LogDebug("Encontradas {Count} reservas vencidas", reservasResponse.Count);

                return reservasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas vencidas");
                throw;
            }
        }

        /// <summary>
        /// Libera automáticamente reservas vencidas
        /// </summary>
        public async Task<int> LiberarReservasVencidasAsync(int minutosTolerancia = 15)
        {
            try
            {
                _logger.LogInformation("Liberando reservas vencidas (tolerancia: {MinutosTolerancia} minutos)", minutosTolerancia);

                var reservasVencidas = await GetReservasVencidasAsync(minutosTolerancia);
                int liberadas = 0;

                foreach (var reserva in reservasVencidas)
                {
                    var exito = await MarcarNoShowAsync(reserva.Id, 0); // Usuario sistema
                    if (exito)
                    {
                        liberadas++;
                    }
                }

                _logger.LogInformation("Liberadas {Liberadas} reservas vencidas", liberadas);

                return liberadas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar reservas vencidas");
                return 0;
            }
        }

        /// <summary>
        /// Envía recordatorio por email para una reservación específica
        /// </summary>
        public async Task<bool> EnviarRecordatorioReservacionAsync(int reservacionId, int minutosAntes = 60)
        {
            try
            {
                _logger.LogInformation("📧 Enviando recordatorio para reservación {ReservacionId} ({MinutosAntes} minutos antes)", 
                    reservacionId, minutosAntes);

                // Obtener la reservación con detalles del cliente
                var reservacion = await _reservacionRepository.GetByIdWithDetallesAsync(reservacionId);
                if (reservacion == null)
                {
                    _logger.LogWarning("⚠️ Reservación {ReservacionId} no encontrada para enviar recordatorio", reservacionId);
                    return false;
                }

                // Verificar que la reservación esté confirmada y no haya pasado
                if (reservacion.Estado != "Confirmada" && reservacion.Estado != "Pendiente")
                {
                    _logger.LogWarning("⚠️ Reservación {ReservacionId} no está en estado válido para recordatorio: {Estado}", 
                        reservacionId, reservacion.Estado);
                    return false;
                }

                if (reservacion.FechaHora <= DateTime.UtcNow)
                {
                    _logger.LogWarning("⚠️ Reservación {ReservacionId} ya pasó, no se puede enviar recordatorio", reservacionId);
                    return false;
                }

                // Verificar que el cliente tenga email
                if (reservacion.Cliente?.Email == null)
                {
                    _logger.LogWarning("⚠️ Cliente de reservación {ReservacionId} no tiene email configurado", reservacionId);
                    return false;
                }

                // Enviar el recordatorio usando el EmailService
                var exito = await _emailService.EnviarRecordatorioReservaAsync(reservacion, minutosAntes);

                if (exito)
                {
                    _logger.LogInformation("✅ Recordatorio enviado exitosamente para reservación {ReservacionId}", reservacionId);
                }
                else
                {
                    _logger.LogWarning("⚠️ No se pudo enviar recordatorio para reservación {ReservacionId}", reservacionId);
                }

                return exito;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar recordatorio para reservación {ReservacionId}", reservacionId);
                return false;
            }
        }

        /// <summary>
        /// Envía recordatorios automáticos para todas las reservaciones que lo requieren
        /// </summary>
        public async Task<int> EnviarRecordatoriosAutomaticosAsync(int minutosAntes = 60)
        {
            try
            {
                _logger.LogInformation("📧 Enviando recordatorios automáticos ({MinutosAntes} minutos antes)", minutosAntes);

                var reservasParaRecordatorio = await GetReservasParaRecordatorioAsync(minutosAntes);
                int enviados = 0;

                foreach (var reserva in reservasParaRecordatorio)
                {
                    var exito = await EnviarRecordatorioReservacionAsync(reserva.Id, minutosAntes);
                    if (exito)
                    {
                        enviados++;
                    }
                }

                _logger.LogInformation("✅ Enviados {Enviados} recordatorios automáticos", enviados);

                return enviados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar recordatorios automáticos");
                return 0;
            }
        }

        // ============================================================================
        // REPORTES Y ESTADÍSTICAS BÁSICAS
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas básicas de reservas
        /// </summary>
        public async Task<EstadisticasReservasResult> GetEstadisticasReservasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                _logger.LogDebug("Calculando estadísticas de reservas desde {FechaInicio} hasta {FechaFin}", fechaInicio, fechaFin);

                var reservas = await _reservacionRepository.GetReservasPorRangoFechaAsync(fechaInicio, fechaFin);

                var totalReservas = reservas.Count();
                var reservasConfirmadas = reservas.Count(r => r.Estado == "Confirmada" || r.Estado == "ClienteLlego" || r.Estado == "Completada");
                var reservasCanceladas = reservas.Count(r => r.Estado == "Cancelada");
                var reservasNoShow = reservas.Count(r => r.Estado == "NoShow");
                var reservasCompletadas = reservas.Count(r => r.Estado == "Completada");

                var porcentajeOcupacion = totalReservas > 0 ? (decimal)reservasCompletadas / totalReservas * 100 : 0;
                var porcentajeNoShow = totalReservas > 0 ? (decimal)reservasNoShow / totalReservas * 100 : 0;

                var duraciones = reservas.Where(r => r.Estado == "Completada").Select(r => TimeSpan.FromMinutes(r.DuracionMinutos));
                var tiempoPromedio = duraciones.Any() ? TimeSpan.FromTicks((long)duraciones.Average(d => d.Ticks)) : TimeSpan.Zero;

                var reservasPorHora = reservas
                    .GroupBy(r => r.FechaHora.Hour)
                    .ToDictionary(g => $"{g.Key:00}:00", g => g.Count());

                var estadisticas = new EstadisticasReservasResult
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalReservas = totalReservas,
                    ReservasConfirmadas = reservasConfirmadas,
                    ReservasCanceladas = reservasCanceladas,
                    ReservasNoShow = reservasNoShow,
                    ReservasCompletadas = reservasCompletadas,
                    PorcentajeOcupacion = porcentajeOcupacion,
                    PorcentajeNoShow = porcentajeNoShow,
                    TiempoPromedioReserva = tiempoPromedio,
                    ReservasPorHora = reservasPorHora
                };

                _logger.LogDebug("Estadísticas calculadas: {TotalReservas} reservas, {PorcentajeOcupacion}% ocupación", 
                    totalReservas, porcentajeOcupacion);

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular estadísticas de reservas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene reservas de un cliente específico
        /// </summary>
        public async Task<IEnumerable<ReservacionResponse>> GetReservasClienteAsync(int clienteId, int limit = 10)
        {
            try
            {
                _logger.LogDebug("Obteniendo reservas de cliente {ClienteId} (limit: {Limit})", clienteId, limit);

                var reservas = await _reservacionRepository.GetReservasPorClienteAsync(clienteId); // Quitar el parámetro limit
                var reservasOrdenadas = reservas.OrderByDescending(r => r.FechaHora).Take(limit);
                var reservasResponse = _mapper.Map<List<ReservacionResponse>>(reservasOrdenadas);

                _logger.LogDebug("Encontradas {Count} reservas para cliente {ClienteId}", reservasResponse.Count, clienteId);

                return reservasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas de cliente {ClienteId}", clienteId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene las próximas reservas del día
        /// </summary>
        public async Task<IEnumerable<ReservacionResponse>> GetProximasReservasAsync(int horasAdelante = 4)
        {
            try
            {
                _logger.LogDebug("Obteniendo próximas reservas ({HorasAdelante} horas adelante)", horasAdelante);

                var fechaInicio = DateTime.UtcNow;
                var fechaFin = DateTime.UtcNow.AddHours(horasAdelante);

                var reservas = await _reservacionRepository.GetReservasPorRangoFechaAsync(fechaInicio, fechaFin);
                var reservasActivas = reservas.Where(r => r.Estado != "Cancelada" && r.Estado != "NoShow");

                var reservasResponse = _mapper.Map<List<ReservacionResponse>>(reservasActivas.OrderBy(r => r.FechaHora));

                _logger.LogDebug("Encontradas {Count} próximas reservas", reservasResponse.Count);

                return reservasResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener próximas reservas");
                throw;
            }
        }

        // ============================================================================
        // MÉTODOS AUXILIARES BÁSICOS
        // ============================================================================

        /// <summary>
        /// Cambia el estado de una reserva
        /// </summary>
        private async Task<bool> CambiarEstadoReservaAsync(int reservaId, string nuevoEstado, int usuarioId, string nota)
        {
            try
            {
                _logger.LogInformation("Cambiando estado de reserva {ReservaId} a {NuevoEstado} por usuario {UsuarioId}", 
                    reservaId, nuevoEstado, usuarioId);

                var reserva = await _reservacionRepository.GetByIdAsync(reservaId);
                if (reserva == null)
                {
                    _logger.LogWarning("Reserva no encontrada: {ReservaId}", reservaId);
                    return false;
                }

                reserva.Estado = nuevoEstado;
                reserva.NotasEspeciales = $"{reserva.NotasEspeciales}\n{DateTime.UtcNow:HH:mm}: {nota}";
                reserva.FechaModificacion = DateTime.UtcNow;

                await _reservacionRepository.UpdateAsync(reserva);

                _logger.LogInformation("Estado de reserva cambiado exitosamente: {ReservaId} → {NuevoEstado}", reservaId, nuevoEstado);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de reserva {ReservaId}", reservaId);
                return false;
            }
        }

        /// <summary>
        /// Busca mesas alternativas en horarios cercanos
        /// </summary>
        private async Task<IEnumerable<MesaResponse>> BuscarMesasAlternativasAsync(DateTime fechaHoraOriginal, int cantidadPersonas)
        {
            try
            {
                var alternativas = new List<Mesa>();

                // Buscar en horarios alternativos (±2 horas)
                var horariosAlternativos = new[]
                {
                    fechaHoraOriginal.AddHours(-2),
                    fechaHoraOriginal.AddHours(-1),
                    fechaHoraOriginal.AddHours(1),
                    fechaHoraOriginal.AddHours(2)
                };

                foreach (var horario in horariosAlternativos)
                {
                    var mesasDisponibles = await BuscarMesasDisponiblesParaReservaAsync(horario, cantidadPersonas);
                    alternativas.AddRange(_mapper.Map<List<Mesa>>(mesasDisponibles));
                }

                return _mapper.Map<List<MesaResponse>>(alternativas.Take(5)); // Top 5 alternativas
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar mesas alternativas");
                return new List<MesaResponse>();
            }
        }
    }
}