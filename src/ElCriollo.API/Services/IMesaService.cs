using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de mesas de El Criollo
    /// Maneja estados dinámicos, asignación inteligente y optimización de ocupación
    /// </summary>
    public interface IMesaService
    {
        // ============================================================================
        // GESTIÓN DE ESTADOS DE MESAS
        // ============================================================================

        /// <summary>
        /// Obtiene el estado actual de todas las mesas del restaurante
        /// </summary>
        /// <returns>Vista completa del estado de mesas</returns>
        Task<EstadoMesasViewModel> GetEstadoTodasLasMesasAsync();

        /// <summary>
        /// Obtiene información detallada de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Información completa de la mesa</returns>
        Task<MesaResponse> GetMesaDetalleAsync(int mesaId);

        /// <summary>
        /// Obtiene todas las mesas filtradas por estado
        /// </summary>
        /// <param name="estado">Estado de las mesas (Libre, Ocupada, Reservada, Mantenimiento)</param>
        /// <returns>Lista de mesas con el estado especificado</returns>
        Task<IEnumerable<MesaResponse>> GetMesasPorEstadoAsync(EstadoMesa estado);

        /// <summary>
        /// Cambia el estado de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado a asignar</param>
        /// <param name="usuarioId">ID del usuario que hace el cambio</param>
        /// <param name="motivo">Motivo del cambio (opcional)</param>
        /// <returns>Resultado del cambio de estado</returns>
        Task<CambioEstadoResult> CambiarEstadoMesaAsync(int mesaId, EstadoMesa nuevoEstado, int usuarioId, string? motivo = null);

        /// <summary>
        /// Libera una mesa automáticamente cuando se completa el servicio
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="usuarioId">ID del usuario que libera</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> LiberarMesaAsync(int mesaId, int usuarioId);

        // ============================================================================
        // ASIGNACIÓN INTELIGENTE DE MESAS
        // ============================================================================

        /// <summary>
        /// Busca y asigna automáticamente la mejor mesa disponible
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <param name="preferenciaUbicacion">Preferencia de ubicación (opcional)</param>
        /// <param name="usuarioId">ID del usuario que asigna</param>
        /// <returns>Mesa asignada o recomendaciones</returns>
        Task<AsignacionMesaResult> AsignarMesaAutomaticaAsync(int cantidadPersonas, string? preferenciaUbicacion = null, int usuarioId = 0);

        /// <summary>
        /// Busca mesas disponibles que cumplan criterios específicos
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <param name="fechaHora">Fecha y hora deseada (para reservas)</param>
        /// <param name="duracionMinutos">Duración estimada en minutos</param>
        /// <returns>Lista de mesas disponibles ordenadas por idoneidad</returns>
        Task<IEnumerable<MesaDisponibleResult>> BuscarMesasDisponiblesAsync(int cantidadPersonas, DateTime? fechaHora = null, int duracionMinutos = 120);

        /// <summary>
        /// Reserva una mesa específica para una fecha/hora
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fechaHora">Fecha y hora de la reserva</param>
        /// <param name="duracionMinutos">Duración estimada</param>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="usuarioId">ID del usuario que reserva</param>
        /// <returns>Resultado de la reserva</returns>
        Task<ReservaMesaResult> ReservarMesaAsync(int mesaId, DateTime fechaHora, int duracionMinutos, int? clienteId, int usuarioId);

        /// <summary>
        /// Optimiza la asignación de mesas para maximizar ocupación
        /// </summary>
        /// <param name="solicitudesPendientes">Lista de solicitudes pendientes</param>
        /// <returns>Recomendaciones de asignación optimizada</returns>
        Task<OptimizacionResult> OptimizarAsignacionMesasAsync(List<SolicitudMesa> solicitudesPendientes);

        // ============================================================================
        // CONTROL DE OCUPACIÓN Y ROTACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas de ocupación en tiempo real
        /// </summary>
        /// <returns>Métricas de ocupación actual</returns>
        Task<EstadisticasOcupacionResult> GetEstadisticasOcupacionAsync();

        /// <summary>
        /// Obtiene el historial de ocupación por período
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período</param>
        /// <param name="fechaFin">Fecha de fin del período</param>
        /// <returns>Datos históricos de ocupación</returns>
        Task<HistorialOcupacionResult> GetHistorialOcupacionAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Calcula el tiempo promedio de ocupación por mesa
        /// </summary>
        /// <param name="mesaId">ID de mesa específica (opcional)</param>
        /// <param name="dias">Número de días hacia atrás para calcular</param>
        /// <returns>Tiempo promedio de ocupación</returns>
        Task<TiempoPromedioResult> CalcularTiempoPromedioOcupacionAsync(int? mesaId = null, int dias = 30);

        /// <summary>
        /// Identifica mesas que requieren rotación por tiempo excesivo
        /// </summary>
        /// <param name="tiempoLimiteMinutos">Tiempo límite en minutos (opcional)</param>
        /// <returns>Mesas que requieren atención</returns>
        Task<IEnumerable<MesaAtencionResult>> GetMesasRequierenRotacionAsync(int? tiempoLimiteMinutos = null);

        /// <summary>
        /// Notifica sobre mesas que necesitan limpieza o mantenimiento
        /// </summary>
        /// <returns>Lista de mesas que requieren servicio</returns>
        Task<IEnumerable<MesaMantenimientoResult>> GetMesasRequierenMantenimientoAsync();

        // ============================================================================
        // GESTIÓN DE SERVICIOS POR MESA
        // ============================================================================

        /// <summary>
        /// Obtiene las órdenes activas de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Órdenes activas en la mesa</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesActivasPorMesaAsync(int mesaId);

        /// <summary>
        /// Asigna una orden a una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del usuario que asigna</param>
        /// <returns>Resultado de la asignación</returns>
        Task<bool> AsignarOrdenAMesaAsync(int mesaId, int ordenId, int usuarioId);

        /// <summary>
        /// Calcula el total acumulado de consumo en una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Total acumulado y desglose</returns>
        Task<ConsumoMesaResult> CalcularConsumoMesaAsync(int mesaId);

        /// <summary>
        /// Prepara una mesa para facturación (pre-cierre)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Información pre-facturación</returns>
        Task<PreFacturacionResult> PrepararMesaParaFacturacionAsync(int mesaId, int usuarioId);

        // ============================================================================
        // NOTIFICACIONES Y ALERTAS
        // ============================================================================

        /// <summary>
        /// Obtiene notificaciones activas relacionadas con mesas
        /// </summary>
        /// <param name="usuarioId">ID del usuario (para filtrar por rol)</param>
        /// <returns>Lista de notificaciones activas</returns>
        Task<IEnumerable<NotificacionMesa>> GetNotificacionesActivasAsync(int usuarioId);

        /// <summary>
        /// Marca una mesa como "requiere atención" (mesero/limpieza)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="tipoAtencion">Tipo de atención requerida</param>
        /// <param name="usuarioId">ID del usuario que reporta</param>
        /// <param name="notas">Notas adicionales</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> MarcarMesaRequiereAtencionAsync(int mesaId, TipoAtencionMesa tipoAtencion, int usuarioId, string? notas = null);

        /// <summary>
        /// Confirma que se atendió una mesa marcada
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="usuarioId">ID del usuario que atendió</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> ConfirmarAtencionMesaAsync(int mesaId, int usuarioId);

        // ============================================================================
        // CONFIGURACIÓN Y ADMINISTRACIÓN
        // ============================================================================

        /// <summary>
        /// Actualiza la configuración de una mesa (capacidad, ubicación, etc.)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="configuracion">Nueva configuración</param>
        /// <param name="usuarioId">ID del administrador</param>
        /// <returns>Mesa actualizada</returns>
        Task<MesaResponse?> ActualizarConfiguracionMesaAsync(int mesaId, ConfiguracionMesa configuracion, int usuarioId);

        /// <summary>
        /// Bloquea/desbloquea una mesa temporalmente
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="bloquear">True para bloquear, false para desbloquear</param>
        /// <param name="motivo">Motivo del bloqueo/desbloqueo</param>
        /// <param name="usuarioId">ID del administrador</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> BloquearDesbloquearMesaAsync(int mesaId, bool bloquear, string motivo, int usuarioId);

        /// <summary>
        /// Reinicia el estado de todas las mesas (para inicio de día)
        /// </summary>
        /// <param name="usuarioId">ID del administrador</param>
        /// <returns>Resultado del reinicio</returns>
        Task<ReinicioMesasResult> ReiniciarEstadoTodasMesasAsync(int usuarioId);
    }

    // ============================================================================
    // MODELOS DE RESPUESTA ESPECÍFICOS DEL SERVICIO
    // ============================================================================

    /// <summary>
    /// Estados posibles de una mesa
    /// </summary>
    public enum EstadoMesa
    {
        Libre = 1,
        Ocupada = 2,
        Reservada = 3,
        Mantenimiento = 4,
        Bloqueada = 5
    }

    /// <summary>
    /// Tipos de atención que puede requerir una mesa
    /// </summary>
    public enum TipoAtencionMesa
    {
        Limpieza = 1,
        ServicioMesero = 2,
        Mantenimiento = 3,
        CheckCliente = 4
    }

    /// <summary>
    /// Resultado de cambio de estado de mesa
    /// </summary>
    public class CambioEstadoResult
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public EstadoMesa EstadoAnterior { get; set; }
        public EstadoMesa EstadoNuevo { get; set; }
        public DateTime FechaCambio { get; set; }
        public string? Usuario { get; set; }
    }

    /// <summary>
    /// Resultado de asignación automática de mesa
    /// </summary>
    public class AsignacionMesaResult
    {
        public bool Exitoso { get; set; }
        public MesaResponse? MesaAsignada { get; set; }
        public string? Mensaje { get; set; }
        public List<MesaResponse> MesasAlternativas { get; set; } = new();
        public string? RazonAsignacion { get; set; }
    }

    /// <summary>
    /// Mesa disponible con puntuación de idoneidad
    /// </summary>
    public class MesaDisponibleResult
    {
        public MesaResponse? Mesa { get; set; }
        public int PuntuacionIdoneidad { get; set; }
        public string? RazonIdoneidad { get; set; }
        public bool RequiereEspera { get; set; }
        public int TiempoEsperaMinutos { get; set; }
    }

    /// <summary>
    /// Resultado de reserva de mesa
    /// </summary>
    public class ReservaMesaResult
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public int? ReservacionId { get; set; }
        public DateTime? FechaHoraReserva { get; set; }
        public int DuracionMinutos { get; set; }
    }

    /// <summary>
    /// Solicitud de mesa para optimización
    /// </summary>
    public class SolicitudMesa
    {
        public int CantidadPersonas { get; set; }
        public DateTime FechaHoraSolicitud { get; set; }
        public string? PreferenciaUbicacion { get; set; }
        public int Prioridad { get; set; } = 1;
        public int? ClienteId { get; set; }
    }

    /// <summary>
    /// Resultado de optimización de asignación
    /// </summary>
    public class OptimizacionResult
    {
        public List<AsignacionOptima> AsignacionesRecomendadas { get; set; } = new();
        public decimal PorcentajeOptimizacion { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Asignación óptima recomendada
    /// </summary>
    public class AsignacionOptima
    {
        public SolicitudMesa? Solicitud { get; set; }
        public MesaResponse? MesaRecomendada { get; set; }
        public int TiempoEsperaMinutos { get; set; }
        public string? Justificacion { get; set; }
    }

    /// <summary>
    /// Estadísticas de ocupación en tiempo real
    /// </summary>
    public class EstadisticasOcupacionResult
    {
        public int TotalMesas { get; set; }
        public int MesasLibres { get; set; }
        public int MesasOcupadas { get; set; }
        public int MesasReservadas { get; set; }
        public int MesasMantenimiento { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public decimal CapacidadMaxima { get; set; }
        public decimal CapacidadActual { get; set; }
        public TimeSpan TiempoPromedioOcupacion { get; set; }
    }

    /// <summary>
    /// Historial de ocupación por período
    /// </summary>
    public class HistorialOcupacionResult
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal OcupacionPromedio { get; set; }
        public int TotalRotaciones { get; set; }
        public decimal IngresosTotales { get; set; }
        public List<OcupacionDiaria> OcupacionPorDia { get; set; } = new();
    }

    /// <summary>
    /// Ocupación diaria
    /// </summary>
    public class OcupacionDiaria
    {
        public DateTime Fecha { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public int RotacionesTotales { get; set; }
        public decimal IngresosDia { get; set; }
    }

    /// <summary>
    /// Resultado de tiempo promedio de ocupación
    /// </summary>
    public class TiempoPromedioResult
    {
        public TimeSpan TiempoPromedio { get; set; }
        public TimeSpan TiempoMinimo { get; set; }
        public TimeSpan TiempoMaximo { get; set; }
        public int TotalOcupaciones { get; set; }
        public string? MesaEspecifica { get; set; }
    }

    /// <summary>
    /// Mesa que requiere atención por rotación
    /// </summary>
    public class MesaAtencionResult
    {
        public MesaResponse? Mesa { get; set; }
        public TimeSpan TiempoOcupada { get; set; }
        public string? Urgencia { get; set; }
        public string? Recomendacion { get; set; }
        public decimal ConsumoActual { get; set; }
    }

    /// <summary>
    /// Mesa que requiere mantenimiento
    /// </summary>
    public class MesaMantenimientoResult
    {
        public MesaResponse? Mesa { get; set; }
        public TipoAtencionMesa TipoAtencionRequerida { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaReporte { get; set; }
        public string? UsuarioReporte { get; set; }
        public string? Prioridad { get; set; }
    }

    /// <summary>
    /// Resultado del consumo de una mesa
    /// </summary>
    public class ConsumoMesaResult
    {
        public decimal TotalConsumo { get; set; }
        public int TotalOrdenes { get; set; }
        public List<OrdenResponse> OrdenesActivas { get; set; } = new();
        public DateTime? InicioServicio { get; set; }
        public TimeSpan? TiempoTranscurrido { get; set; }
        public decimal PromedioConsumo { get; set; }
    }

    /// <summary>
    /// Información para pre-facturación
    /// </summary>
    public class PreFacturacionResult
    {
        public bool ListaParaFacturar { get; set; }
        public decimal TotalConsumo { get; set; }
        public decimal ITBIS { get; set; }
        public decimal TotalConITBIS { get; set; }
        public List<OrdenResponse> OrdenesParaFacturar { get; set; } = new();
        public string? Observaciones { get; set; }
        public bool RequiereAtencionMesero { get; set; }
    }

    /// <summary>
    /// Notificación relacionada con mesa
    /// </summary>
    public class NotificacionMesa
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public string? NumeroMesa { get; set; }
        public string? Tipo { get; set; }
        public string? Mensaje { get; set; }
        public string? Urgencia { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
    }

    /// <summary>
    /// Configuración de mesa
    /// </summary>
    public class ConfiguracionMesa
    {
        public int? Capacidad { get; set; }
        public string? Ubicacion { get; set; }
        public string? Descripcion { get; set; }
        public bool? EstaActiva { get; set; }
    }

    /// <summary>
    /// Resultado de reinicio de mesas
    /// </summary>
    public class ReinicioMesasResult
    {
        public bool Exitoso { get; set; }
        public int MesasReiniciadas { get; set; }
        public int MesasConProblemas { get; set; }
        public List<string> Observaciones { get; set; } = new();
        public DateTime FechaReinicio { get; set; }
    }
}