using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;
using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz simplificada para el servicio de gestión de órdenes de El Criollo
    /// </summary>
    public interface IOrdenService
    {
        // ============================================================================
        // GESTIÓN DE ÓRDENES
        // ============================================================================

        /// <summary>
        /// Crea una nueva orden básica
        /// </summary>
        /// <param name="crearOrdenRequest">Datos de la orden</param>
        /// <param name="usuarioId">ID del usuario que crea la orden</param>
        /// <returns>Orden creada</returns>
        Task<OrdenResponse> CrearOrdenAsync(CreateOrdenRequest crearOrdenRequest, int usuarioId);

        /// <summary>
        /// Obtiene una orden por su ID
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Datos de la orden</returns>
        Task<OrdenResponse?> GetOrdenByIdAsync(int ordenId);

        /// <summary>
        /// Obtiene una orden por su número
        /// </summary>
        /// <param name="numeroOrden">Número único de la orden</param>
        /// <returns>Datos de la orden</returns>
        Task<OrdenResponse?> GetOrdenByNumeroAsync(string numeroOrden);

        /// <summary>
        /// Cancela una orden (solo si no está en preparación)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="motivo">Motivo de cancelación</param>
        /// <param name="usuarioId">ID del usuario que cancela</param>
        /// <returns>True si se canceló exitosamente</returns>
        Task<bool> CancelarOrdenAsync(int ordenId, string motivo, int usuarioId);

        // ============================================================================
        // GESTIÓN DE ESTADOS
        // ============================================================================

        /// <summary>
        /// Cambia el estado de una orden
        /// Estados: Pendiente → EnPreparacion → Lista → Entregada → Facturada
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="usuarioId">ID del usuario que cambia el estado</param>
        /// <returns>True si se cambió exitosamente</returns>
        Task<bool> CambiarEstadoOrdenAsync(int ordenId, string nuevoEstado, int usuarioId);

        /// <summary>
        /// Marca una orden como "En Preparación"
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del cocinero</param>
        /// <returns>True si se marcó exitosamente</returns>
        Task<bool> IniciarPreparacionAsync(int ordenId, int usuarioId);

        /// <summary>
        /// Marca una orden como "Lista" para servir
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del cocinero</param>
        /// <returns>True si se marcó exitosamente</returns>
        Task<bool> MarcarOrdenListaAsync(int ordenId, int usuarioId);

        /// <summary>
        /// Marca una orden como "Entregada" al cliente
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="usuarioId">ID del mesero</param>
        /// <returns>True si se marcó exitosamente</returns>
        Task<bool> MarcarOrdenEntregadaAsync(int ordenId, int usuarioId);

        // ============================================================================
        // CONSULTAS
        // ============================================================================

        /// <summary>
        /// Obtiene todas las órdenes activas (no terminadas)
        /// </summary>
        /// <returns>Lista de órdenes activas</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesActivasAsync();

        /// <summary>
        /// Obtiene órdenes por estado específico
        /// </summary>
        /// <param name="estado">Estado a filtrar</param>
        /// <returns>Lista de órdenes en el estado especificado</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesPorEstadoAsync(string estado);

        /// <summary>
        /// Obtiene órdenes de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Lista de órdenes de la mesa</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesPorMesaAsync(int mesaId);

        /// <summary>
        /// Obtiene órdenes de una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de órdenes de la fecha</returns>
        Task<IEnumerable<OrdenResponse>> GetOrdenesPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Obtiene el historial de órdenes de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="limite">Número máximo de órdenes</param>
        /// <returns>Historial del cliente</returns>
        Task<IEnumerable<OrdenResponse>> GetHistorialClienteAsync(int clienteId, int limite = 10);

        // ============================================================================
        // CÁLCULOS Y TOTALES
        // ============================================================================

        /// <summary>
        /// Calcula el total de una orden con ITBIS dominicano
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Total calculado con desglose</returns>
        Task<CalculoOrdenResult> CalcularTotalOrdenAsync(int ordenId);

        /// <summary>
        /// Recalcula los totales de una orden (después de modificaciones)
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>True si se recalculó exitosamente</returns>
        Task<bool> RecalcularTotalesAsync(int ordenId);

        // ============================================================================
        // VALIDACIONES
        // ============================================================================

        /// <summary>
        /// Valida que una orden puede ser creada
        /// </summary>
        /// <param name="crearOrdenRequest">Datos de la orden</param>
        /// <returns>Resultado de validación</returns>
        Task<ValidacionOrdenResult> ValidarOrdenAsync(CreateOrdenRequest crearOrdenRequest);

        /// <summary>
        /// Verifica disponibilidad de productos para una orden
        /// </summary>
        /// <param name="items">Items de la orden</param>
        /// <returns>Resultado de disponibilidad</returns>
        Task<DisponibilidadResult> VerificarDisponibilidadAsync(List<ItemOrdenRequest> items);

        // ============================================================================
        // DASHBOARD PARA COCINA
        // ============================================================================

        /// <summary>
        /// Obtiene un dashboard simple para la cocina
        /// </summary>
        /// <returns>Vista básica de órdenes en cocina</returns>
        Task<DashboardCocinaBasicoViewModel> GetDashboardCocinaBasicoAsync();

        /// <summary>
        /// Obtiene órdenes pendientes para cocina (cola simple)
        /// </summary>
        /// <returns>Lista simple de órdenes pendientes</returns>
        Task<IEnumerable<OrdenCocinaBasicaResponse>> GetColaCocinaBasicaAsync();
    }

    // ============================================================================
    // MODELOS PARA RESULTADOS
    // ============================================================================

    /// <summary>
    /// Resultado del cálculo de una orden
    /// </summary>
    public class CalculoOrdenResult
    {
        public decimal Subtotal { get; set; }
        public decimal ITBIS { get; set; }
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
        public string TotalFormateado => $"RD$ {Total:N2}";
    }

    /// <summary>
    /// Resultado de validación de orden
    /// </summary>
    public class ValidacionOrdenResult
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
    }

    /// <summary>
    /// Resultado de disponibilidad de productos
    /// </summary>
    public class DisponibilidadResult
    {
        public bool TodoDisponible { get; set; }
        public List<string> ProductosNoDisponibles { get; set; } = new();
        public List<string> ProductosStockBajo { get; set; } = new();
    }

    /// <summary>
    /// Dashboard básico para cocina
    /// </summary>
    public class DashboardCocinaBasicoViewModel
    {
        public int OrdenesPendientes { get; set; }
        public int OrdenesEnPreparacion { get; set; }
        public int OrdenesListas { get; set; }
        public string TiempoPromedioPreparacion { get; set; } = "0 min";
        public List<OrdenCocinaBasicaResponse> OrdenesUrgentes { get; set; } = new();
    }

    /// <summary>
    /// Orden básica para vista de cocina
    /// </summary>
    public class OrdenCocinaBasicaResponse
    {
        public int OrdenID { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string? NumeroMesa { get; set; }
        public string TiempoEspera { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<string> ProductosResumen { get; set; } = new();
        public bool EsUrgente { get; set; }
        public string? NotasEspeciales { get; set; }
    }

    /// <summary>
    /// Item básico para crear orden
    /// </summary>
    public class ItemOrdenRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? NotasEspeciales { get; set; }
    }
}