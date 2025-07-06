using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de reservas de El Criollo
    /// Maneja reservas de mesas con validación de disponibilidad y confirmaciones
    /// </summary>
    public interface IReservacionService
    {
        // ============================================================================
        // GESTIÓN BÁSICA DE RESERVAS
        // ============================================================================

        /// <summary>
        /// Crea una nueva reserva validando disponibilidad
        /// </summary>
        /// <param name="crearReservaRequest">Datos de la reserva</param>
        /// <param name="usuarioId">ID del usuario que crea</param>
        /// <returns>Reserva creada</returns>
        Task<ReservacionResponse> CrearReservaAsync(CrearReservacionRequest crearReservaRequest, int usuarioId);

        /// <summary>
        /// Obtiene una reserva por su ID
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <returns>Datos de la reserva</returns>
        Task<ReservacionResponse?> GetReservaByIdAsync(int reservaId);

        /// <summary>
        /// Obtiene todas las reservas de una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de reservas del día</returns>
        Task<IEnumerable<ReservacionResponse>> GetReservasPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Obtiene reservas por estado
        /// </summary>
        /// <param name="estado">Estado de las reservas</param>
        /// <returns>Lista de reservas</returns>
        Task<IEnumerable<ReservacionResponse>> GetReservasPorEstadoAsync(string estado);

        /// <summary>
        /// Actualiza una reserva existente
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <param name="actualizarRequest">Nuevos datos</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Reserva actualizada</returns>
        Task<ReservacionResponse?> ActualizarReservaAsync(int reservaId, ActualizarReservacionRequest actualizarRequest, int usuarioId);

        /// <summary>
        /// Cancela una reserva
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <param name="motivo">Motivo de cancelación</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Resultado de la cancelación</returns>
        Task<bool> CancelarReservaAsync(int reservaId, string motivo, int usuarioId);

        // ============================================================================
        // VALIDACIONES Y DISPONIBILIDAD
        // ============================================================================

        /// <summary>
        /// Verifica si una mesa está disponible en fecha/hora específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fechaHora">Fecha y hora</param>
        /// <param name="duracionMinutos">Duración en minutos</param>
        /// <returns>True si está disponible</returns>
        Task<bool> VerificarDisponibilidadMesaAsync(int mesaId, DateTime fechaHora, int duracionMinutos);

        /// <summary>
        /// Busca mesas disponibles para una fecha/hora y número de personas
        /// </summary>
        /// <param name="fechaHora">Fecha y hora deseada</param>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <param name="duracionMinutos">Duración estimada</param>
        /// <returns>Lista de mesas disponibles</returns>
        Task<IEnumerable<MesaResponse>> BuscarMesasDisponiblesParaReservaAsync(DateTime fechaHora, int cantidadPersonas, int duracionMinutos = 120);

        /// <summary>
        /// Valida una solicitud de reserva
        /// </summary>
        /// <param name="request">Solicitud a validar</param>
        /// <returns>Resultado de la validación</returns>
        Task<ValidacionReservaResult> ValidarSolicitudReservaAsync(CrearReservacionRequest request);

        // ============================================================================
        // GESTIÓN DE ESTADOS
        // ============================================================================

        /// <summary>
        /// Confirma una reserva (cambia estado a Confirmada)
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> ConfirmarReservaAsync(int reservaId, int usuarioId);

        /// <summary>
        /// Marca una reserva como "Cliente Llegó"
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> MarcarClienteLlegoAsync(int reservaId, int usuarioId);

        /// <summary>
        /// Marca una reserva como "No Show" (cliente no llegó)
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> MarcarNoShowAsync(int reservaId, int usuarioId);

        /// <summary>
        /// Completa una reserva (cliente atendido)
        /// </summary>
        /// <param name="reservaId">ID de la reserva</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> CompletarReservaAsync(int reservaId, int usuarioId);

        // ============================================================================
        // NOTIFICACIONES Y RECORDATORIOS
        // ============================================================================

        /// <summary>
        /// Obtiene reservas que requieren recordatorio
        /// </summary>
        /// <param name="minutosAntes">Minutos antes de la reserva</param>
        /// <returns>Lista de reservas a recordar</returns>
        Task<IEnumerable<ReservacionResponse>> GetReservasParaRecordatorioAsync(int minutosAntes = 60);

        /// <summary>
        /// Obtiene reservas que están vencidas (pasó la hora y no llegó)
        /// </summary>
        /// <param name="minutosTolerancia">Minutos de tolerancia</param>
        /// <returns>Lista de reservas vencidas</returns>
        Task<IEnumerable<ReservacionResponse>> GetReservasVencidasAsync(int minutosTolerancia = 15);

        /// <summary>
        /// Libera automáticamente reservas vencidas
        /// </summary>
        /// <param name="minutosTolerancia">Minutos de tolerancia</param>
        /// <returns>Número de reservas liberadas</returns>
        Task<int> LiberarReservasVencidasAsync(int minutosTolerancia = 15);

        // ============================================================================
        // REPORTES Y ESTADÍSTICAS BÁSICAS
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas básicas de reservas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estadísticas del período</returns>
        Task<EstadisticasReservasResult> GetEstadisticasReservasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene reservas de un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="limit">Número máximo de reservas</param>
        /// <returns>Historial de reservas del cliente</returns>
        Task<IEnumerable<ReservacionResponse>> GetReservasClienteAsync(int clienteId, int limit = 10);

        /// <summary>
        /// Obtiene las próximas reservas del día
        /// </summary>
        /// <param name="horasAdelante">Horas hacia adelante</param>
        /// <returns>Próximas reservas</returns>
        Task<IEnumerable<ReservacionResponse>> GetProximasReservasAsync(int horasAdelante = 4);
    }

    // ============================================================================
    // MODELOS DE RESPUESTA ESPECÍFICOS
    // ============================================================================

    /// <summary>
    /// Resultado de validación de reserva
    /// </summary>
    public class ValidacionReservaResult
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
        public List<MesaResponse> MesasAlternativas { get; set; } = new();
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Estadísticas básicas de reservas
    /// </summary>
    public class EstadisticasReservasResult
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int ReservasCanceladas { get; set; }
        public int ReservasNoShow { get; set; }
        public int ReservasCompletadas { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public decimal PorcentajeNoShow { get; set; }
        public TimeSpan TiempoPromedioReserva { get; set; }
        public Dictionary<string, int> ReservasPorHora { get; set; } = new();
    }
}