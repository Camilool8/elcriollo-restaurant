using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de órdenes de El Criollo
    /// Maneja el flujo completo de comandas desde creación hasta entrega
    /// </summary>
    public interface IOrdenService
    {
        // ============================================================================
        // CREACIÓN Y GESTIÓN DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Crea una nueva orden con validación completa de productos y stock
        /// </summary>
        /// <param name="crearOrdenRequest">Datos de la nueva orden</param>
        /// <param name="usuarioId">ID del mesero que crea la orden</param>
        /// <returns>Orden creada con detalles completos</returns>
        Task<OrdenResponse> CrearOrdenAsync(CrearOrdenRequest crearOrdenRequest, int usuarioId);

        /// <summary>
        /// Obtiene una orden específica con todos sus detalles
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Orden completa con detalles y estado actual</returns>
        Task<OrdenResponse?> GetOrdenByIdAsync(int ordenId);

        /// <summary>
        /// Actualiza los items de una orden existente (solo si está en estado Pendiente)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="nuevosItems">Nuevos items de la orden</param>
        /// <param name="usuarioId">ID del usuario que actualiza</param>
        /// <returns>Orden actualizada</returns>
        Task<OrdenResponse?> ActualizarItemsOrdenAsync(int ordenId, List<ItemOrdenRequest> nuevosItems, int usuarioId);

        /// <summary>
        /// Cancela una orden completa (solo si no está en preparación)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="motivo">Motivo de la cancelación</param>
        /// <param name="usuarioId">ID del usuario que cancela</param>
        /// <returns>Resultado de la cancelación</returns>
        Task<CancelacionOrdenResult> CancelarOrdenAsync(int ordenId, string motivo, int usuarioId);

        /// <summary>
        /// Duplica una orden existente para repetir pedido
        /// </summary>
        /// <param name="ordenIdOriginal">ID de la orden a duplicar</param>
        /// <param name="mesaId">Nueva mesa (opcional)</param>
        /// <param name="usuarioId">ID del usuario que duplica</param>
        /// <returns>Nueva orden duplicada</returns>
        Task<OrdenResponse> DuplicarOrdenAsync(int ordenIdOriginal, int? mesaId, int usuarioId);

        // ============================================================================
        // FLUJO DE ESTADOS DE ORDEN
        // ============================================================================

        /// <summary>
        /// Cambia el estado de una orden siguiendo el flujo definido
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="nuevoEstado">Nuevo estado de la orden</param>
        /// <param name="usuarioId">ID del usuario que cambia el estado</param>
        /// <param name="notas">Notas adicionales (opcional)</param>
        /// <returns>Resultado del cambio de estado</returns>
        Task<CambioEstadoOrdenResult> CambiarEstadoOrdenAsync(int ordenId, EstadoOrden nuevoEstado, int usuarioId, string? notas = null);

        /// <summary>
        /// Marca una orden como "En Preparación" (para cocina)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del cocinero/usuario</param>
        /// <param name="tiempoEstimadoMinutos">Tiempo estimado de preparación</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> IniciarPreparacionAsync(int ordenId, int usuarioId, int? tiempoEstimadoMinutos = null);

        /// <summary>
        /// Marca una orden como "Lista" (lista para servir)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del cocinero/usuario</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> MarcarOrdenListaAsync(int ordenId, int usuarioId);

        /// <summary>
        /// Marca una orden como "Entregada" (servida al cliente)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del mesero que entrega</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> MarcarOrdenEntregadaAsync(int ordenId, int usuarioId);

        /// <summary>
        /// Obtiene todas las órdenes filtradas por estado
        /// </summary>
        /// <param name="estado">Estado de las órdenes</param>
        /// <param name="fechaInicio">Fecha inicio (opcional)</param>
        /// <param name="fechaFin">Fecha fin (opcional)</param>
        /// <returns>Lista de órdenes en el estado especificado</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesPorEstadoAsync(EstadoOrden estado, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        // ============================================================================
        // GESTIÓN DE ÓRDENES POR MESA
        // ============================================================================

        /// <summary>
        /// Obtiene todas las órdenes activas de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Órdenes activas de la mesa</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesPorMesaAsync(int mesaId);

        /// <summary>
        /// Crea una orden grupal para múltiples personas en una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="ordenesIndividuales">Lista de órdenes individuales</param>
        /// <param name="usuarioId">ID del mesero</param>
        /// <returns>Orden grupal creada</returns>
        Task<OrdenGrupalResult> CrearOrdenGrupalAsync(int mesaId, List<CrearOrdenRequest> ordenesIndividuales, int usuarioId);

        /// <summary>
        /// Divide una orden grupal en órdenes individuales para facturación separada
        /// </summary>
        /// <param name="ordenGrupalId">ID de la orden grupal</param>
        /// <param name="division">Especificación de cómo dividir</param>
        /// <param name="usuarioId">ID del usuario que divide</param>
        /// <returns>Resultado de la división</returns>
        Task<DivisionOrdenResult> DividirOrdenGrupalAsync(int ordenGrupalId, DivisionOrdenRequest division, int usuarioId);

        /// <summary>
        /// Consolida múltiples órdenes de una mesa en una sola factura
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="ordenesIds">IDs de las órdenes a consolidar</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Orden consolidada</returns>
        Task<OrdenConsolidadaResult> ConsolidarOrdenesAsync(int mesaId, List<int> ordenesIds, int usuarioId);

        // ============================================================================
        // DASHBOARD Y MONITOREO DE COCINA
        // ============================================================================

        /// <summary>
        /// Obtiene el dashboard de cocina con órdenes en preparación
        /// </summary>
        /// <returns>Vista completa del estado de la cocina</returns>
        Task<DashboardCocinaViewModel> GetDashboardCocinaAsync();

        /// <summary>
        /// Obtiene órdenes pendientes de preparación (cola de cocina)
        /// </summary>
        /// <param name="filtrarPorTipo">Filtrar por tipo de orden (opcional)</param>
        /// <returns>Cola de órdenes para cocina</returns>
        Task<IEnumerable<OrdenCocinaResponse>> GetColaOrdensCocinaAsync(TipoOrden? filtrarPorTipo = null);

        /// <summary>
        /// Obtiene órdenes listas para servir (para meseros)
        /// </summary>
        /// <param name="meseroId">ID del mesero específico (opcional)</param>
        /// <returns>Órdenes listas para entregar</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesListasParaServirAsync(int? meseroId = null);

        /// <summary>
        /// Obtiene estadísticas de tiempo de preparación en tiempo real
        /// </summary>
        /// <returns>Métricas de rendimiento de cocina</returns>
        Task<EstadisticasCocinaResult> GetEstadisticasCocinaAsync();

        /// <summary>
        /// Notifica a cocina sobre una nueva orden urgente
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="prioridad">Nivel de prioridad</param>
        /// <param name="usuarioId">ID del usuario que notifica</param>
        /// <returns>Éxito de la notificación</returns>
        Task<bool> NotificarOrdenUrgenteAsync(int ordenId, PrioridadOrden prioridad, int usuarioId);

        // ============================================================================
        // CÁLCULOS Y VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Calcula el total de una orden con descuentos aplicables
        /// </summary>
        /// <param name="items">Items de la orden</param>
        /// <param name="aplicarDescuentos">Si aplicar descuentos automáticos</param>
        /// <returns>Desglose completo de precios</returns>
        Task<CalculoOrdenResult> CalcularTotalOrdenAsync(List<ItemOrdenRequest> items, bool aplicarDescuentos = true);

        /// <summary>
        /// Valida una orden completa antes de crearla
        /// </summary>
        /// <param name="crearOrdenRequest">Datos de la orden a validar</param>
        /// <returns>Resultado de validación con errores/advertencias</returns>
        Task<ValidacionOrdenResult> ValidarOrdenAsync(CrearOrdenRequest crearOrdenRequest);

        /// <summary>
        /// Estima el tiempo total de preparación de una orden
        /// </summary>
        /// <param name="items">Items de la orden</param>
        /// <returns>Tiempo estimado en minutos</returns>
        Task<int> EstimarTiempoPreparacionAsync(List<ItemOrdenRequest> items);

        /// <summary>
        /// Verifica disponibilidad de todos los productos de una orden
        /// </summary>
        /// <param name="items">Items a verificar</param>
        /// <returns>Resultado de disponibilidad con detalles</returns>
        Task<DisponibilidadOrdenResult> VerificarDisponibilidadOrdenAsync(List<ItemOrdenRequest> items);

        // ============================================================================
        // HISTORIAL Y REPORTES
        // ============================================================================

        /// <summary>
        /// Obtiene el historial de órdenes de un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="limit">Número máximo de órdenes</param>
        /// <returns>Historial de órdenes del cliente</returns>
        Task<IEnumerable<OrdenResponse>> GetHistorialClienteAsync(int clienteId, int limit = 10);

        /// <summary>
        /// Obtiene órdenes recientes del día actual
        /// </summary>
        /// <param name="soloActivas">Solo órdenes activas (no entregadas/canceladas)</param>
        /// <returns>Órdenes del día</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesDelDiaAsync(bool soloActivas = false);

        /// <summary>
        /// Obtiene productos más ordenados en un período
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="limit">Número máximo de productos</param>
        /// <returns>Ranking de productos más ordenados</returns>
        Task<IEnumerable<ProductoMasOrdenadoResult>> GetProductosMasOrdenadosAsync(DateTime fechaInicio, DateTime fechaFin, int limit = 10);

        /// <summary>
        /// Obtiene estadísticas de órdenes por período
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Estadísticas detalladas del período</returns>
        Task<EstadisticasOrdenesResult> GetEstadisticasOrdenesAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // NOTIFICACIONES Y COMUNICACIÓN
        // ============================================================================

        /// <summary>
        /// Envía notificación de cambio de estado a personal relevante
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="usuarioId">ID del usuario que notifica</param>
        /// <returns>Éxito del envío</returns>
        Task<bool> NotificarCambioEstadoAsync(int ordenId, EstadoOrden nuevoEstado, int usuarioId);

        /// <summary>
        /// Obtiene notificaciones activas para un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="soloNoLeidas">Solo notificaciones no leídas</param>
        /// <returns>Lista de notificaciones</returns>
        Task<IEnumerable<NotificacionOrden>> GetNotificacionesUsuarioAsync(int usuarioId, bool soloNoLeidas = true);

        /// <summary>
        /// Marca notificaciones como leídas
        /// </summary>
        /// <param name="notificacionesIds">IDs de las notificaciones</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Éxito de la operación</returns>
        Task<bool> MarcarNotificacionesLeidasAsync(List<int> notificacionesIds, int usuarioId);
    }

    // ============================================================================
    // ENUMS Y MODELOS ESPECÍFICOS DEL SERVICIO
    // ============================================================================

    /// <summary>
    /// Estados posibles de una orden
    /// </summary>
    public enum EstadoOrden
    {
        Pendiente = 1,        // Recién creada, esperando preparación
        EnPreparacion = 2,    // En cocina, siendo preparada
        Lista = 3,            // Lista para servir
        Entregada = 4,        // Servida al cliente
        Facturada = 5,        // Incluida en factura
        Cancelada = 6         // Cancelada por algún motivo
    }

    /// <summary>
    /// Tipos de orden según servicio
    /// </summary>
    public enum TipoOrden
    {
        Mesa = 1,      // Para consumir en mesa
        Llevar = 2,    // Para llevar
        Delivery = 3   // Entrega a domicilio
    }

    /// <summary>
    /// Prioridades de orden
    /// </summary>
    public enum PrioridadOrden
    {
        Normal = 1,
        Alta = 2,
        Urgente = 3
    }

    /// <summary>
    /// Item para solicitud de orden
    /// </summary>
    public class ItemOrdenRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public int? ComboId { get; set; }
        public string? NotasEspeciales { get; set; }
        public decimal? PrecioUnitario { get; set; }
    }

    /// <summary>
    /// Resultado de cancelación de orden
    /// </summary>
    public class CancelacionOrdenResult
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public decimal? MontoReembolso { get; set; }
        public bool StockRestaurado { get; set; }
        public DateTime FechaCancelacion { get; set; }
    }

    /// <summary>
    /// Resultado de cambio de estado de orden
    /// </summary>
    public class CambioEstadoOrdenResult
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public EstadoOrden EstadoAnterior { get; set; }
        public EstadoOrden EstadoNuevo { get; set; }
        public DateTime FechaCambio { get; set; }
        public string? Usuario { get; set; }
        public int? TiempoEstimadoMinutos { get; set; }
    }

    /// <summary>
    /// Resultado de orden grupal
    /// </summary>
    public class OrdenGrupalResult
    {
        public bool Exitoso { get; set; }
        public OrdenResponse? OrdenPrincipal { get; set; }
        public List<OrdenResponse> OrdenesIndividuales { get; set; } = new();
        public decimal TotalGrupal { get; set; }
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Solicitud de división de orden
    /// </summary>
    public class DivisionOrdenRequest
    {
        public List<DivisionPersona> Divisiones { get; set; } = new();
        public bool DividirEquitativamente { get; set; }
    }

    /// <summary>
    /// División por persona
    /// </summary>
    public class DivisionPersona
    {
        public string? NombrePersona { get; set; }
        public List<int> ItemsIds { get; set; } = new();
        public decimal? MontoFijo { get; set; }
    }

    /// <summary>
    /// Resultado de división de orden
    /// </summary>
    public class DivisionOrdenResult
    {
        public bool Exitoso { get; set; }
        public List<OrdenResponse> OrdenesIndividuales { get; set; } = new();
        public decimal TotalOriginal { get; set; }
        public decimal TotalDividido { get; set; }
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Resultado de orden consolidada
    /// </summary>
    public class OrdenConsolidadaResult
    {
        public bool Exitoso { get; set; }
        public OrdenResponse? OrdenConsolidada { get; set; }
        public List<int> OrdenesOriginalesIds { get; set; } = new();
        public decimal TotalConsolidado { get; set; }
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Respuesta de orden para cocina
    /// </summary>
    public class OrdenCocinaResponse
    {
        public int Id { get; set; }
        public string? NumeroOrden { get; set; }
        public int? MesaId { get; set; }
        public string? NumeroMesa { get; set; }
        public TipoOrden Tipo { get; set; }
        public PrioridadOrden Prioridad { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int TiempoEsperaMinutos { get; set; }
        public int TiempoEstimadoPreparacion { get; set; }
        public List<ItemCocinaResponse> Items { get; set; } = new();
        public string? NotasEspeciales { get; set; }
    }

    /// <summary>
    /// Item para cocina
    /// </summary>
    public class ItemCocinaResponse
    {
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public string? NotasEspeciales { get; set; }
        public bool RequierePrecaucion { get; set; }
        public int TiempoPreparacionMinutos { get; set; }
    }

    /// <summary>
    /// Estadísticas de cocina
    /// </summary>
    public class EstadisticasCocinaResult
    {
        public int OrdenesEnCola { get; set; }
        public int OrdenesEnPreparacion { get; set; }
        public int OrdenesListasParaServir { get; set; }
        public TimeSpan TiempoPromedioPreparacion { get; set; }
        public TimeSpan TiempoEsperaMaximo { get; set; }
        public decimal PorcentajeEficiencia { get; set; }
        public List<ProductoMasOrdenadoResult> ProductosMasSolicitados { get; set; } = new();
    }

    /// <summary>
    /// Resultado de cálculo de orden
    /// </summary>
    public class CalculoOrdenResult
    {
        public decimal Subtotal { get; set; }
        public decimal Descuentos { get; set; }
        public decimal ITBIS { get; set; }
        public decimal Total { get; set; }
        public List<DescuentoAplicado> DescuentosDetalle { get; set; } = new();
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Resultado de validación de orden
    /// </summary>
    public class ValidacionOrdenResult
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
        public List<string> Sugerencias { get; set; } = new();
        public decimal TotalEstimado { get; set; }
        public int TiempoEstimadoMinutos { get; set; }
    }

    /// <summary>
    /// Resultado de disponibilidad de orden
    /// </summary>
    public class DisponibilidadOrdenResult
    {
        public bool TodoDisponible { get; set; }
        public List<string> ProductosNoDisponibles { get; set; } = new();
        public List<string> ProductosStockBajo { get; set; } = new();
        public List<ProductoResponse> Alternativas { get; set; } = new();
        public string? Mensaje { get; set; }
    }

    /// <summary>
    /// Producto más ordenado
    /// </summary>
    public class ProductoMasOrdenadoResult
    {
        public int ProductoId { get; set; }
        public string? NombreProducto { get; set; }
        public int CantidadOrdenada { get; set; }
        public decimal MontoTotal { get; set; }
        public int VecesOrdenado { get; set; }
        public decimal Porcentaje { get; set; }
    }

    /// <summary>
    /// Estadísticas de órdenes
    /// </summary>
    public class EstadisticasOrdenesResult
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalOrdenes { get; set; }
        public int OrdenesCompletadas { get; set; }
        public int OrdenesCanceladas { get; set; }
        public decimal MontoTotalVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public TimeSpan TiempoPromedioPreparacion { get; set; }
        public List<ProductoMasOrdenadoResult> TopProductos { get; set; } = new();
        public Dictionary<string, int> OrdenesPorHora { get; set; } = new();
    }

    /// <summary>
    /// Notificación de orden
    /// </summary>
    public class NotificacionOrden
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public string? NumeroOrden { get; set; }
        public string? Tipo { get; set; }
        public string? Mensaje { get; set; }
        public string? Urgencia { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public int? MesaId { get; set; }
        public string? NumeroMesa { get; set; }
    }
}