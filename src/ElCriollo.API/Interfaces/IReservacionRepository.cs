using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para operaciones con reservaciones
    /// Maneja el sistema de reservas de mesas con control de tiempo y disponibilidad
    /// </summary>
    public interface IReservacionRepository : IBaseRepository<Reservacion>
    {
        // ============================================================================
        // GESTIÓN DE ESTADOS DE RESERVACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones por estado específico
        /// </summary>
        /// <param name="estado">Estado de la reservación (Pendiente, Confirmada, Completada, Cancelada)</param>
        /// <returns>Lista de reservaciones en el estado especificado</returns>
        Task<IEnumerable<Reservacion>> GetByEstadoAsync(string estado);

        /// <summary>
        /// Obtiene reservaciones pendientes de confirmación
        /// </summary>
        /// <returns>Lista de reservaciones pendientes</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesPendientesAsync();

        /// <summary>
        /// Obtiene reservaciones confirmadas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de reservaciones confirmadas</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesConfirmadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene reservaciones completadas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de reservaciones completadas</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesCompletadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene reservaciones canceladas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de reservaciones canceladas</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesCanceladasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Cambia el estado de una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="observaciones">Observaciones del cambio (opcional)</param>
        /// <returns>True si se cambió correctamente</returns>
        Task<bool> CambiarEstadoReservacionAsync(int reservacionId, string nuevoEstado, string? observaciones = null);

        // ============================================================================
        // CONSULTAS POR FECHA Y TIEMPO
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones del día actual
        /// </summary>
        /// <returns>Lista de reservaciones de hoy</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesHoyAsync();

        /// <summary>
        /// Obtiene reservaciones de una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de reservaciones de la fecha</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Obtiene reservaciones en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Lista de reservaciones en el rango</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene reservaciones próximas (siguientes horas)
        /// </summary>
        /// <param name="horas">Número de horas hacia adelante (por defecto 2)</param>
        /// <returns>Lista de reservaciones próximas</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesProximasAsync(int horas = 2);

        /// <summary>
        /// Obtiene reservaciones vencidas (no confirmadas a tiempo)
        /// </summary>
        /// <returns>Lista de reservaciones vencidas</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesVencidasAsync();

        // ============================================================================
        // CONSULTAS POR MESA Y CLIENTE
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de reservaciones de la mesa</returns>
        Task<IEnumerable<Reservacion>> GetByMesaAsync(int mesaId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene la reservación activa de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Reservación activa de la mesa o null</returns>
        Task<Reservacion?> GetReservacionActivaMesaAsync(int mesaId);

        /// <summary>
        /// Obtiene reservaciones de un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Lista de reservaciones del cliente</returns>
        Task<IEnumerable<Reservacion>> GetByClienteAsync(int clienteId);

        /// <summary>
        /// Obtiene el historial de reservaciones de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="limite">Número máximo de reservaciones (por defecto 10)</param>
        /// <returns>Historial de reservaciones del cliente</returns>
        Task<IEnumerable<Reservacion>> GetHistorialClienteAsync(int clienteId, int limite = 10);

        // ============================================================================
        // VERIFICACIÓN DE DISPONIBILIDAD
        // ============================================================================

        /// <summary>
        /// Verifica si una mesa está disponible para una fecha/hora específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fechaYHora">Fecha y hora de la reservación</param>
        /// <param name="duracionMinutos">Duración estimada en minutos</param>
        /// <param name="excluirReservacionId">ID de reservación a excluir (para updates)</param>
        /// <returns>True si está disponible</returns>
        Task<bool> MesaDisponibleParaReservacionAsync(int mesaId, DateTime fechaYHora, int duracionMinutos, int? excluirReservacionId = null);

        /// <summary>
        /// Busca mesas disponibles para una fecha/hora y capacidad específica
        /// </summary>
        /// <param name="fechaYHora">Fecha y hora deseada</param>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <param name="duracionMinutos">Duración estimada en minutos (por defecto 120)</param>
        /// <returns>Lista de mesas disponibles</returns>
        Task<IEnumerable<Mesa>> BuscarMesasDisponiblesAsync(DateTime fechaYHora, int cantidadPersonas, int duracionMinutos = 120);

        /// <summary>
        /// Verifica conflictos de horario para una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fechaYHora">Fecha y hora de la nueva reservación</param>
        /// <param name="duracionMinutos">Duración en minutos</param>
        /// <returns>Lista de reservaciones en conflicto</returns>
        Task<IEnumerable<Reservacion>> VerificarConflictosHorarioAsync(int mesaId, DateTime fechaYHora, int duracionMinutos);

        // ============================================================================
        // OPERACIONES DE RESERVACIÓN
        // ============================================================================

        /// <summary>
        /// Crea una nueva reservación validando disponibilidad
        /// </summary>
        /// <param name="reservacion">Datos de la reservación</param>
        /// <returns>Reservación creada o null si no está disponible</returns>
        Task<Reservacion?> CrearReservacionAsync(Reservacion reservacion);

        /// <summary>
        /// Confirma una reservación pendiente
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <returns>True si se confirmó correctamente</returns>
        Task<bool> ConfirmarReservacionAsync(int reservacionId);

        /// <summary>
        /// Cancela una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="razon">Razón de la cancelación</param>
        /// <returns>True si se canceló correctamente</returns>
        Task<bool> CancelarReservacionAsync(int reservacionId, string razon);

        /// <summary>
        /// Marca una reservación como completada
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <returns>True si se completó correctamente</returns>
        Task<bool> CompletarReservacionAsync(int reservacionId);

        /// <summary>
        /// Modifica una reservación existente
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="nuevaFechaYHora">Nueva fecha y hora (opcional)</param>
        /// <param name="nuevaMesaId">Nueva mesa (opcional)</param>
        /// <param name="nuevaCantidadPersonas">Nueva cantidad de personas (opcional)</param>
        /// <returns>True si se modificó correctamente</returns>
        Task<bool> ModificarReservacionAsync(int reservacionId, DateTime? nuevaFechaYHora = null, int? nuevaMesaId = null, int? nuevaCantidadPersonas = null);

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas de reservaciones del día
        /// </summary>
        /// <returns>Estadísticas del día actual</returns>
        Task<object> GetEstadisticasDelDiaAsync();

        /// <summary>
        /// Obtiene estadísticas de reservaciones por período
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estadísticas del período</returns>
        Task<object> GetEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene las horas más populares para reservaciones
        /// </summary>
        /// <param name="dias">Número de días hacia atrás (por defecto 30)</param>
        /// <returns>Lista de horas con número de reservaciones</returns>
        Task<IEnumerable<object>> GetHorasPopularesAsync(int dias = 30);

        /// <summary>
        /// Obtiene los clientes más frecuentes
        /// </summary>
        /// <param name="limite">Número máximo de clientes (por defecto 10)</param>
        /// <param name="dias">Período en días (por defecto 90)</param>
        /// <returns>Lista de clientes más frecuentes</returns>
        Task<IEnumerable<object>> GetClientesMasFrecuentesAsync(int limite = 10, int dias = 90);

        /// <summary>
        /// Obtiene la tasa de ocupación por reservaciones
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Porcentaje de ocupación por reservaciones</returns>
        Task<double> GetTasaOcupacionReservacionesAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // NOTIFICACIONES Y RECORDATORIOS
        // ============================================================================

        /// <summary>
        /// Obtiene reservaciones que necesitan recordatorio
        /// </summary>
        /// <param name="horasAntes">Horas antes de la reservación (por defecto 2)</param>
        /// <returns>Lista de reservaciones para recordar</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesParaRecordatorioAsync(int horasAntes = 2);

        /// <summary>
        /// Marca una reservación como recordada
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <returns>True si se marcó correctamente</returns>
        Task<bool> MarcarComoRecordadaAsync(int reservacionId);

        /// <summary>
        /// Obtiene reservaciones que llegaron tarde
        /// </summary>
        /// <param name="minutosTolerancia">Minutos de tolerancia (por defecto 15)</param>
        /// <returns>Lista de reservaciones tardías</returns>
        Task<IEnumerable<Reservacion>> GetReservacionesTardiasAsync(int minutosTolerancia = 15);
    }
}