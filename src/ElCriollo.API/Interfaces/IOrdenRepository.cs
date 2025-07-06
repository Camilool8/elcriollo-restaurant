using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para operaciones con órdenes del restaurante
    /// Maneja comandas, estados de preparación y flujo de cocina
    /// </summary>
    public interface IOrdenRepository : IBaseRepository<Orden>
    {
        // ============================================================================
        // GESTIÓN DE ESTADOS DE ORDEN
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes por estado específico
        /// </summary>
        /// <param name="estado">Estado de la orden (Pendiente, EnPreparacion, Lista, Entregada, Cancelada)</param>
        /// <returns>Lista de órdenes en el estado especificado</returns>
        Task<IEnumerable<Orden>> GetByEstadoAsync(string estado);

        /// <summary>
        /// Obtiene órdenes pendientes de preparación
        /// </summary>
        /// <returns>Lista de órdenes pendientes</returns>
        Task<IEnumerable<Orden>> GetOrdenesPendientesAsync();

        /// <summary>
        /// Obtiene órdenes en preparación
        /// </summary>
        /// <returns>Lista de órdenes en preparación</returns>
        Task<IEnumerable<Orden>> GetOrdenesEnPreparacionAsync();

        /// <summary>
        /// Obtiene órdenes listas para entregar
        /// </summary>
        /// <returns>Lista de órdenes listas</returns>
        Task<IEnumerable<Orden>> GetOrdenesListasAsync();

        /// <summary>
        /// Obtiene órdenes entregadas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de órdenes entregadas</returns>
        Task<IEnumerable<Orden>> GetOrdenesEntregadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene órdenes canceladas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de órdenes canceladas</returns>
        Task<IEnumerable<Orden>> GetOrdenesCanceladasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Cambia el estado de una orden
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="observaciones">Observaciones del cambio (opcional)</param>
        /// <returns>True si se cambió correctamente</returns>
        Task<bool> CambiarEstadoOrdenAsync(int ordenId, string nuevoEstado, string? observaciones = null);

        // ============================================================================
        // CONSULTAS POR TIPO DE ORDEN
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes por tipo específico
        /// </summary>
        /// <param name="tipoOrden">Tipo de orden (Mesa, Llevar, Delivery)</param>
        /// <returns>Lista de órdenes del tipo especificado</returns>
        Task<IEnumerable<Orden>> GetByTipoOrdenAsync(string tipoOrden);

        /// <summary>
        /// Obtiene órdenes para mesa
        /// </summary>
        /// <returns>Lista de órdenes para mesa</returns>
        Task<IEnumerable<Orden>> GetOrdenesMesaAsync();

        /// <summary>
        /// Obtiene órdenes para llevar
        /// </summary>
        /// <returns>Lista de órdenes para llevar</returns>
        Task<IEnumerable<Orden>> GetOrdenesLlevarAsync();

        /// <summary>
        /// Obtiene órdenes para delivery
        /// </summary>
        /// <returns>Lista de órdenes para delivery</returns>
        Task<IEnumerable<Orden>> GetOrdenesDeliveryAsync();

        // ============================================================================
        // CONSULTAS POR MESA Y EMPLEADO
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Lista de órdenes de la mesa</returns>
        Task<IEnumerable<Orden>> GetByMesaAsync(int mesaId);

        /// <summary>
        /// Obtiene la orden activa de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Orden activa de la mesa o null</returns>
        Task<Orden?> GetOrdenActivaMesaAsync(int mesaId);

        /// <summary>
        /// Obtiene órdenes atendidas por un empleado
        /// </summary>
        /// <param name="empleadoId">ID del empleado</param>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de órdenes del empleado</returns>
        Task<IEnumerable<Orden>> GetByEmpleadoAsync(int empleadoId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene órdenes de un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Lista de órdenes del cliente</returns>
        Task<IEnumerable<Orden>> GetByClienteAsync(int clienteId);

        // ============================================================================
        // GESTIÓN DE NÚMEROS DE ORDEN
        // ============================================================================

        /// <summary>
        /// Genera el próximo número de orden automáticamente
        /// </summary>
        /// <returns>Número de orden generado</returns>
        Task<string> GenerarNumeroOrdenAsync();

        /// <summary>
        /// Obtiene una orden por su número de orden
        /// </summary>
        /// <param name="numeroOrden">Número de orden único</param>
        /// <returns>Orden encontrada o null</returns>
        Task<Orden?> GetByNumeroOrdenAsync(string numeroOrden);

        /// <summary>
        /// Verifica si un número de orden ya existe
        /// </summary>
        /// <param name="numeroOrden">Número de orden a verificar</param>
        /// <returns>True si ya existe</returns>
        Task<bool> NumeroOrdenExisteAsync(string numeroOrden);

        /// <summary>
        /// Obtiene una orden con todos sus includes
        /// </summary>
        Task<Orden?> GetByIdWithIncludesAsync(int ordenId);

        /// <summary>
        /// Obtiene órdenes activas
        /// </summary>
        Task<IEnumerable<Orden>> GetOrdenesActivasAsync();

        /// <summary>
        /// Obtiene órdenes por mesa actualmente activas
        /// </summary>
        Task<IEnumerable<Orden>> GetOrdenesPorMesaAsync(int mesaId);

        /// <summary>
        /// Obtiene órdenes por fecha
        /// </summary>
        Task<IEnumerable<Orden>> GetOrdenesPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Agrega un detalle de orden
        /// </summary>
        Task<DetalleOrden> AddDetalleOrdenAsync(DetalleOrden detalle);

        /// <summary>
        /// Agrega una nueva orden (alias de CreateAsync)
        /// </summary>
        new Task<Orden> AddAsync(Orden orden);

        // ============================================================================
        // CONSULTAS POR FECHA Y TIEMPO
        // ============================================================================

        /// <summary>
        /// Obtiene órdenes del día actual
        /// </summary>
        /// <returns>Lista de órdenes de hoy</returns>
        Task<IEnumerable<Orden>> GetOrdenesHoyAsync();

        /// <summary>
        /// Obtiene órdenes en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Lista de órdenes en el rango</returns>
        Task<IEnumerable<Orden>> GetOrdenesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene órdenes con tiempo de preparación excedido
        /// </summary>
        /// <param name="tiempoLimite">Tiempo límite en minutos (por defecto 30)</param>
        /// <returns>Lista de órdenes con tiempo excedido</returns>
        Task<IEnumerable<Orden>> GetOrdenesConTiempoExcedidoAsync(int tiempoLimite = 30);

        // ============================================================================
        // OPERACIONES CON DETALLES DE ORDEN
        // ============================================================================

        /// <summary>
        /// Obtiene una orden con todos sus detalles incluidos
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Orden con detalles cargados</returns>
        Task<Orden?> GetConDetallesAsync(int ordenId);

        /// <summary>
        /// Agrega un producto a una orden existente
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="productoId">ID del producto (null si es combo)</param>
        /// <param name="comboId">ID del combo (null si es producto)</param>
        /// <param name="cantidad">Cantidad del producto/combo</param>
        /// <param name="observaciones">Observaciones especiales</param>
        /// <returns>Detalle de orden creado</returns>
        Task<DetalleOrden> AgregarProductoAOrdenAsync(int ordenId, int? productoId, int? comboId, int cantidad, string? observaciones = null);

        /// <summary>
        /// Remueve un producto de una orden
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="detalleOrdenId">ID del detalle a remover</param>
        /// <returns>True si se removió correctamente</returns>
        Task<bool> RemoverProductoDeOrdenAsync(int ordenId, int detalleOrdenId);

        /// <summary>
        /// Calcula el total de una orden
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Total calculado de la orden</returns>
        Task<decimal> CalcularTotalOrdenAsync(int ordenId);

        // ============================================================================
        // ESTADÍSTICAS Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas de órdenes del día
        /// </summary>
        /// <returns>Estadísticas del día actual</returns>
        Task<object> GetEstadisticasDelDiaAsync();

        /// <summary>
        /// Obtiene estadísticas de órdenes por período
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estadísticas del período</returns>
        Task<object> GetEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene el tiempo promedio de preparación
        /// </summary>
        /// <param name="dias">Número de días hacia atrás (por defecto 7)</param>
        /// <returns>Tiempo promedio en minutos</returns>
        Task<double> GetTiempoPromedioPreparacionAsync(int dias = 7);

        /// <summary>
        /// Obtiene las órdenes más grandes del día
        /// </summary>
        /// <param name="limite">Número máximo de órdenes (por defecto 10)</param>
        /// <returns>Lista de órdenes con mayor valor</returns>
        Task<IEnumerable<object>> GetOrdenesMasGrandesAsync(int limite = 10);

        // ============================================================================
        // OPERACIONES ESPECIALES
        // ============================================================================

        /// <summary>
        /// Cancela una orden y libera recursos asociados
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="razon">Razón de la cancelación</param>
        /// <returns>True si se canceló correctamente</returns>
        Task<bool> CancelarOrdenAsync(int ordenId, string razon);

        /// <summary>
        /// Duplica una orden existente (para repetir pedido)
        /// </summary>
        /// <param name="ordenId">ID de la orden a duplicar</param>
        /// <param name="nuevaMesaId">ID de la nueva mesa (opcional)</param>
        /// <returns>Nueva orden duplicada</returns>
        Task<Orden> DuplicarOrdenAsync(int ordenId, int? nuevaMesaId = null);

        /// <summary>
        /// Obtiene órdenes que necesitan seguimiento
        /// Órdenes en preparación por más tiempo del esperado
        /// </summary>
        /// <returns>Lista de órdenes que necesitan atención</returns>
        Task<IEnumerable<Orden>> GetOrdenesQueNecesitanSeguimientoAsync();
    }
}