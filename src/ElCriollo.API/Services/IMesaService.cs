using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz simplificada para el servicio de gestión de mesas de El Criollo
    /// </summary>
    public interface IMesaService
    {
        // ============================================================================
        // GESTIÓN DE MESAS
        // ============================================================================

        /// <summary>
        /// Obtiene el estado actual de todas las mesas
        /// </summary>
        /// <returns>Lista completa de mesas con su estado</returns>
        Task<IEnumerable<MesaResponse>> GetEstadoTodasLasMesasAsync();

        /// <summary>
        /// Obtiene una mesa específica por ID
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Datos de la mesa</returns>
        Task<MesaResponse?> GetMesaByIdAsync(int mesaId);

        /// <summary>
        /// Obtiene mesas por estado específico
        /// </summary>
        /// <param name="estado">Estado a filtrar (Libre, Ocupada, Reservada, Mantenimiento)</param>
        /// <returns>Lista de mesas en el estado especificado</returns>
        Task<IEnumerable<MesaResponse>> GetMesasPorEstadoAsync(string estado);

        /// <summary>
        /// Cambia el estado de una mesa manualmente
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="usuarioId">ID del usuario que hace el cambio</param>
        /// <param name="motivo">Motivo del cambio (opcional)</param>
        /// <returns>True si se cambió exitosamente</returns>
        Task<bool> CambiarEstadoMesaAsync(int mesaId, string nuevoEstado, int usuarioId, string? motivo = null);

        /// <summary>
        /// Libera una mesa automáticamente (post-facturación)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se liberó exitosamente</returns>
        Task<bool> LiberarMesaAsync(int mesaId, int usuarioId);

        // ============================================================================
        // BÚSQUEDA Y DISPONIBILIDAD
        // ============================================================================

        /// <summary>
        /// Busca mesas disponibles (libres) básico
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas (opcional para filtrar por capacidad)</param>
        /// <returns>Lista de mesas disponibles</returns>
        Task<IEnumerable<MesaResponse>> BuscarMesasDisponiblesAsync(int? cantidadPersonas = null);

        /// <summary>
        /// Obtiene la primera mesa disponible para un número de personas
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <returns>Primera mesa disponible o null</returns>
        Task<MesaResponse?> GetPrimeraMesaDisponibleAsync(int cantidadPersonas);

        /// <summary>
        /// Verifica si una mesa específica está disponible
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si está disponible</returns>
        Task<bool> VerificarDisponibilidadMesaAsync(int mesaId);

        // ============================================================================
        // RESERVAS Y OCUPACIÓN
        // ============================================================================

        /// <summary>
        /// Ocupa una mesa (para orden inmediata)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se ocupó exitosamente</returns>
        Task<bool> OcuparMesaAsync(int mesaId, int usuarioId);

        /// <summary>
        /// Reserva una mesa para una reservación
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se reservó exitosamente</returns>
        Task<bool> ReservarMesaAsync(int mesaId, int reservacionId, int usuarioId);

        /// <summary>
        /// Libera una reserva (cuando llega el cliente o se cancela)
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="activarOcupacion">True si el cliente llegó (ocupar), false si se cancela (liberar)</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se procesó exitosamente</returns>
        Task<bool> LiberarReservaMesaAsync(int mesaId, bool activarOcupacion, int usuarioId);

        // ============================================================================
        // ESTADÍSTICAS
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas básicas de ocupación
        /// </summary>
        /// <returns>Estadísticas simples de mesas</returns>
        Task<EstadisticasMesasBasicasViewModel> GetEstadisticasBasicasAsync();

        /// <summary>
        /// Obtiene el resumen de ocupación del día
        /// </summary>
        /// <param name="fecha">Fecha a consultar (por defecto hoy)</param>
        /// <returns>Resumen básico del día</returns>
        Task<ResumenOcupacionDiaViewModel> GetResumenOcupacionDiaAsync(DateTime? fecha = null);

        /// <summary>
        /// Obtiene las mesas con más tiempo ocupadas (para rotación)
        /// </summary>
        /// <param name="tiempoLimiteMinutos">Tiempo límite en minutos (por defecto 180 min)</param>
        /// <returns>Lista de mesas que requieren atención</returns>
        Task<IEnumerable<MesaAtencionBasicaResponse>> GetMesasRequierenAtencionAsync(int tiempoLimiteMinutos = 180);

        // ============================================================================
        // VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Valida que una mesa puede cambiar a un estado específico
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado deseado</param>
        /// <returns>Resultado de validación</returns>
        Task<ValidacionMesaResult> ValidarCambioEstadoAsync(int mesaId, string nuevoEstado);

        /// <summary>
        /// Valida que una mesa puede ser liberada
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si puede ser liberada</returns>
        Task<bool> PuedeLiberarseMesaAsync(int mesaId);

        // ============================================================================
        // UTILIDADES
        // ============================================================================

        /// <summary>
        /// Reinicia el estado de todas las mesas a "Libre" (para inicio de día)
        /// </summary>
        /// <param name="usuarioId">ID del administrador</param>
        /// <returns>Resultado del reinicio</returns>
        Task<bool> ReiniciarTodasLasMesasAsync(int usuarioId);

        /// <summary>
        /// Marca una mesa para mantenimiento
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="motivo">Motivo del mantenimiento</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se marcó exitosamente</returns>
        Task<bool> MarcarMesaMantenimientoAsync(int mesaId, string motivo, int usuarioId);

        /// <summary>
        /// Completa el mantenimiento y libera la mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se completó exitosamente</returns>
        Task<bool> CompletarMantenimientoMesaAsync(int mesaId, int usuarioId);
    }

    // ============================================================================
    // MODELOS BÁSICOS PARA RESULTADOS
    // ============================================================================

    /// <summary>
    /// Estadísticas básicas de mesas
    /// </summary>
    public class EstadisticasMesasBasicasViewModel
    {
        public int TotalMesas { get; set; }
        public int MesasLibres { get; set; }
        public int MesasOcupadas { get; set; }
        public int MesasReservadas { get; set; }
        public int MesasMantenimiento { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public string HorarioPico { get; set; } = "12:00 - 14:00";
    }

    /// <summary>
    /// Resumen de ocupación del día
    /// </summary>
    public class ResumenOcupacionDiaViewModel
    {
        public DateTime Fecha { get; set; } = DateTime.Today;
        public int MesasOcupadasMaximo { get; set; }
        public decimal PorcentajeOcupacionPromedio { get; set; }
        public int TotalClientesAtendidos { get; set; }
        public string TiempoPromedioOcupacion { get; set; } = "0 min";
        public int VecesRotacionPromedio { get; set; }
    }

    /// <summary>
    /// Mesa que requiere atención básica
    /// </summary>
    public class MesaAtencionBasicaResponse
    {
        public int MesaID { get; set; }
        public int NumeroMesa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TiempoOcupada { get; set; } = string.Empty;
        public int MinutosOcupada { get; set; }
        public bool RequiereRotacion { get; set; }
        public string? UltimaOrden { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Resultado de validación de mesa
    /// </summary>
    public class ValidacionMesaResult
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
        public string EstadoActual { get; set; } = string.Empty;
        public string EstadoDeseado { get; set; } = string.Empty;
    }
}