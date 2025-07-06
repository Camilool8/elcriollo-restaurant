using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para operaciones con mesas del restaurante
    /// Maneja estados, disponibilidad y control de ocupación
    /// </summary>
    public interface IMesaRepository : IBaseRepository<Mesa>
    {
        // ============================================================================
        // GESTIÓN DE ESTADOS DE MESA
        // ============================================================================

        /// <summary>
        /// Obtiene mesas por estado específico
        /// </summary>
        /// <param name="estado">Estado de la mesa (Libre, Ocupada, Reservada, Mantenimiento)</param>
        /// <returns>Lista de mesas en el estado especificado</returns>
        Task<IEnumerable<Mesa>> GetByEstadoAsync(string estado);

        /// <summary>
        /// Obtiene todas las mesas libres
        /// </summary>
        /// <returns>Lista de mesas disponibles</returns>
        Task<IEnumerable<Mesa>> GetMesasLibresAsync();

        /// <summary>
        /// Obtiene todas las mesas ocupadas
        /// </summary>
        /// <returns>Lista de mesas ocupadas</returns>
        Task<IEnumerable<Mesa>> GetMesasOcupadasAsync();

        /// <summary>
        /// Obtiene todas las mesas reservadas
        /// </summary>
        /// <returns>Lista de mesas reservadas</returns>
        Task<IEnumerable<Mesa>> GetMesasReservadasAsync();

        /// <summary>
        /// Obtiene mesas en mantenimiento
        /// </summary>
        /// <returns>Lista de mesas en mantenimiento</returns>
        Task<IEnumerable<Mesa>> GetMesasEnMantenimientoAsync();

        /// <summary>
        /// Cambia el estado de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado (Libre, Ocupada, Reservada, Mantenimiento)</param>
        /// <returns>True si se cambió correctamente</returns>
        Task<bool> CambiarEstadoMesaAsync(int mesaId, string nuevoEstado);

        // ============================================================================
        // DISPONIBILIDAD POR CAPACIDAD
        // ============================================================================

        /// <summary>
        /// Obtiene mesas disponibles para una cantidad específica de personas
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <returns>Lista de mesas libres con capacidad suficiente</returns>
        Task<IEnumerable<Mesa>> GetMesasDisponiblesParaCapacidadAsync(int cantidadPersonas);

        /// <summary>
        /// Obtiene la mejor mesa disponible para una cantidad de personas
        /// Prioriza mesas con capacidad exacta o cercana
        /// </summary>
        /// <param name="cantidadPersonas">Número de personas</param>
        /// <returns>Mesa más adecuada o null si no hay disponible</returns>
        Task<Mesa?> GetMejorMesaDisponibleAsync(int cantidadPersonas);

        /// <summary>
        /// Obtiene mesas por capacidad específica
        /// </summary>
        /// <param name="capacidad">Capacidad exacta de la mesa</param>
        /// <returns>Lista de mesas con la capacidad especificada</returns>
        Task<IEnumerable<Mesa>> GetByCapacidadAsync(int capacidad);

        /// <summary>
        /// Obtiene mesas por rango de capacidad
        /// </summary>
        /// <param name="capacidadMinima">Capacidad mínima</param>
        /// <param name="capacidadMaxima">Capacidad máxima</param>
        /// <returns>Lista de mesas en el rango de capacidad</returns>
        Task<IEnumerable<Mesa>> GetByRangoCapacidadAsync(int capacidadMinima, int capacidadMaxima);

        // ============================================================================
        // GESTIÓN DE UBICACIONES
        // ============================================================================

        /// <summary>
        /// Obtiene mesas por ubicación
        /// </summary>
        /// <param name="ubicacion">Ubicación de la mesa (ej: "Terraza", "Interior", "VIP")</param>
        /// <returns>Lista de mesas en la ubicación especificada</returns>
        Task<IEnumerable<Mesa>> GetByUbicacionAsync(string ubicacion);

        /// <summary>
        /// Obtiene todas las ubicaciones disponibles
        /// </summary>
        /// <returns>Lista de ubicaciones únicas en el restaurante</returns>
        Task<IEnumerable<string>> GetUbicacionesDisponiblesAsync();

        /// <summary>
        /// Obtiene mesas disponibles en una ubicación específica
        /// </summary>
        /// <param name="ubicacion">Ubicación deseada</param>
        /// <param name="cantidadPersonas">Número de personas (opcional)</param>
        /// <returns>Lista de mesas libres en la ubicación</returns>
        Task<IEnumerable<Mesa>> GetMesasDisponiblesEnUbicacionAsync(string ubicacion, int? cantidadPersonas = null);

        // ============================================================================
        // OPERACIONES DE OCUPACIÓN
        // ============================================================================

        /// <summary>
        /// Ocupa una mesa para una orden
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="ordenId">ID de la orden asociada</param>
        /// <returns>True si se ocupó correctamente</returns>
        Task<bool> OcuparMesaAsync(int mesaId, int? ordenId = null);

        /// <summary>
        /// Libera una mesa cuando se termina la orden
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si se liberó correctamente</returns>
        Task<bool> LiberarMesaAsync(int mesaId);

        /// <summary>
        /// Reserva una mesa para una fecha/hora específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <returns>True si se reservó correctamente</returns>
        Task<bool> ReservarMesaAsync(int mesaId, int reservacionId);

        /// <summary>
        /// Pone una mesa en mantenimiento
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="razon">Razón del mantenimiento</param>
        /// <returns>True si se puso en mantenimiento correctamente</returns>
        Task<bool> PonerEnMantenimientoAsync(int mesaId, string? razon = null);

        // ============================================================================
        // VERIFICACIONES Y VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Verifica si una mesa está disponible
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si la mesa está libre</returns>
        Task<bool> EstaDisponibleAsync(int mesaId);

        /// <summary>
        /// Verifica si una mesa está ocupada
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si la mesa está ocupada</returns>
        Task<bool> EstaOcupadaAsync(int mesaId);

        /// <summary>
        /// Verifica si una mesa está reservada
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si la mesa está reservada</returns>
        Task<bool> EstaReservadaAsync(int mesaId);

        /// <summary>
        /// Verifica si un número de mesa ya existe
        /// </summary>
        /// <param name="numeroMesa">Número de mesa a verificar</param>
        /// <param name="excluirMesaId">ID de mesa a excluir (para updates)</param>
        /// <returns>True si el número ya existe</returns>
        Task<bool> NumeroMesaExisteAsync(int numeroMesa, int? excluirMesaId = null);

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas generales de mesas
        /// </summary>
        /// <returns>Objeto con estadísticas de ocupación y disponibilidad</returns>
        Task<object> GetEstadisticasMesasAsync();

        /// <summary>
        /// Obtiene el historial de ocupación de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="dias">Número de días hacia atrás (por defecto 30)</param>
        /// <returns>Historial de ocupación de la mesa</returns>
        Task<IEnumerable<object>> GetHistorialOcupacionAsync(int mesaId, int dias = 30);

        /// <summary>
        /// Obtiene las mesas más utilizadas
        /// </summary>
        /// <param name="limite">Número máximo de mesas a retornar</param>
        /// <param name="dias">Período en días para calcular uso (por defecto 30)</param>
        /// <returns>Lista de mesas más utilizadas</returns>
        Task<IEnumerable<object>> GetMesasMasUtilizadasAsync(int limite = 10, int dias = 30);

        /// <summary>
        /// Obtiene el tiempo promedio de ocupación por mesa
        /// </summary>
        /// <param name="dias">Período en días para calcular (por defecto 30)</param>
        /// <returns>Tiempo promedio de ocupación en minutos</returns>
        Task<double> GetTiempoPromedioOcupacionAsync(int dias = 30);

        // ============================================================================
        // GESTIÓN DE LIMPIEZA Y MANTENIMIENTO
        // ============================================================================

        /// <summary>
        /// Registra la limpieza de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>True si se registró la limpieza correctamente</returns>
        Task<bool> RegistrarLimpiezaAsync(int mesaId);

        /// <summary>
        /// Obtiene mesas que necesitan limpieza
        /// </summary>
        /// <param name="horasLimite">Horas desde la última limpieza (por defecto 4)</param>
        /// <returns>Lista de mesas que necesitan limpieza</returns>
        Task<IEnumerable<Mesa>> GetMesasQueNecesitanLimpiezaAsync(int horasLimite = 4);

        /// <summary>
        /// Obtiene el resumen de estado actual de todas las mesas
        /// </summary>
        /// <returns>Resumen con conteo por estado</returns>
        Task<object> GetResumenEstadoMesasAsync();

        /// <summary>
        /// Obtiene todas las mesas con información adicional
        /// </summary>
        Task<IEnumerable<Mesa>> GetAllWithIncludesAsync();

        /// <summary>
        /// Obtiene una mesa por ID con información adicional
        /// </summary>
        Task<Mesa?> GetByIdWithIncludesAsync(int mesaId);

        /// <summary>
        /// Obtiene mesas activas
        /// </summary>
        Task<IEnumerable<Mesa>> GetMesasActivasAsync();
    }
}