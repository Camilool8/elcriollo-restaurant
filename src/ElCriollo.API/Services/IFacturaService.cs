using ElCriollo.API.Models.Entities;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de facturación del restaurante El Criollo
    /// Maneja la facturación con ITBIS dominicano y liberación automática de mesas
    /// </summary>
    public interface IFacturaService
    {
        // ============================================================================
        // CREACIÓN Y GESTIÓN BÁSICA DE FACTURAS
        // ============================================================================

        /// <summary>
        /// Crea una factura individual para una orden específica
        /// </summary>
        /// <param name="crearFacturaRequest">Datos de la factura a crear</param>
        /// <returns>Factura creada con cálculos ITBIS incluidos</returns>
        Task<FacturaDto> CrearFacturaAsync(CrearFacturaRequest crearFacturaRequest);

        /// <summary>
        /// Crea una factura grupal para todas las órdenes de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa a facturar</param>
        /// <param name="metodoPago">Método de pago utilizado</param>
        /// <param name="descuento">Descuento aplicado (opcional)</param>
        /// <param name="propina">Propina incluida (opcional)</param>
        /// <returns>Factura grupal creada</returns>
        Task<FacturaDto> CrearFacturaGrupalAsync(int mesaId, string metodoPago = "Efectivo", decimal descuento = 0, decimal propina = 0);

        /// <summary>
        /// Obtiene una factura por su ID
        /// </summary>
        /// <param name="facturaId">ID de la factura</param>
        /// <returns>Datos completos de la factura</returns>
        Task<FacturaDto?> GetFacturaByIdAsync(int facturaId);

        /// <summary>
        /// Obtiene una factura por su número de factura
        /// </summary>
        /// <param name="numeroFactura">Número único de la factura</param>
        /// <returns>Datos completos de la factura</returns>
        Task<FacturaDto?> GetFacturaByNumeroAsync(string numeroFactura);

        /// <summary>
        /// Marca una factura como pagada
        /// </summary>
        /// <param name="facturaId">ID de la factura</param>
        /// <param name="metodoPago">Método de pago utilizado</param>
        /// <returns>True si se marcó exitosamente</returns>
        Task<bool> MarcarFacturaPagadaAsync(int facturaId, string metodoPago);

        /// <summary>
        /// Cancela una factura (solo si está pendiente)
        /// </summary>
        /// <param name="facturaId">ID de la factura</param>
        /// <param name="motivoCancelacion">Motivo de la cancelación</param>
        /// <returns>True si se canceló exitosamente</returns>
        Task<bool> CancelarFacturaAsync(int facturaId, string motivoCancelacion);

        // ============================================================================
        // CÁLCULOS Y OPERACIONES MATEMÁTICAS
        // ============================================================================

        /// <summary>
        /// Calcula los totales de una orden con ITBIS 18% dominicano
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="descuento">Descuento aplicado</param>
        /// <param name="propina">Propina incluida</param>
        /// <returns>Desglose completo de totales</returns>
        Task<CalculoTotalesResult> CalcularTotalesOrdenAsync(int ordenId, decimal descuento = 0, decimal propina = 0);

        /// <summary>
        /// Calcula el ITBIS (18%) para un subtotal dado
        /// </summary>
        /// <param name="subtotal">Subtotal antes de impuestos</param>
        /// <returns>Monto del ITBIS calculado</returns>
        decimal CalcularITBIS(decimal subtotal);

        /// <summary>
        /// Calcula el total final con descuentos, ITBIS y propina
        /// </summary>
        /// <param name="subtotal">Subtotal de productos</param>
        /// <param name="descuento">Descuento aplicado</param>
        /// <param name="propina">Propina incluida</param>
        /// <returns>Total final a pagar</returns>
        decimal CalcularTotalFinal(decimal subtotal, decimal descuento = 0, decimal propina = 0);

        // ============================================================================
        // CONSULTAS POR ESTADO Y FECHA
        // ============================================================================

        /// <summary>
        /// Obtiene facturas por estado específico
        /// </summary>
        /// <param name="estado">Estado de la factura (Pagada, Pendiente, Anulada)</param>
        /// <returns>Lista de facturas en el estado especificado</returns>
        Task<IEnumerable<FacturaDto>> GetFacturasPorEstadoAsync(string estado);

        /// <summary>
        /// Obtiene todas las facturas pendientes de pago
        /// </summary>
        /// <returns>Lista de facturas pendientes</returns>
        Task<IEnumerable<FacturaDto>> GetFacturasPendientesAsync();

        /// <summary>
        /// Obtiene facturas del día actual
        /// </summary>
        /// <returns>Lista de facturas de hoy</returns>
        Task<IEnumerable<FacturaDto>> GetFacturasHoyAsync();

        /// <summary>
        /// Obtiene facturas de una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de facturas de la fecha</returns>
        Task<IEnumerable<FacturaDto>> GetFacturasPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Obtiene facturas en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del rango</param>
        /// <param name="fechaFin">Fecha fin del rango</param>
        /// <returns>Lista de facturas en el rango</returns>
        Task<IEnumerable<FacturaDto>> GetFacturasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // VALIDACIONES Y VERIFICACIONES
        // ============================================================================

        /// <summary>
        /// Verifica si una orden puede ser facturada
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Resultado de validación con detalles</returns>
        Task<ValidacionFacturaResult> ValidarOrdenParaFacturacionAsync(int ordenId);

        /// <summary>
        /// Verifica si un número de factura ya existe
        /// </summary>
        /// <param name="numeroFactura">Número de factura a verificar</param>
        /// <returns>True si ya existe</returns>
        Task<bool> ExisteNumeroFacturaAsync(string numeroFactura);

        /// <summary>
        /// Valida que una mesa puede ser facturada grupalmente
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Resultado de validación con detalles</returns>
        Task<ValidacionFacturaResult> ValidarMesaParaFacturacionGrupalAsync(int mesaId);

        // ============================================================================
        // REPORTES BÁSICOS DE FACTURACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene resumen básico de facturación del día
        /// </summary>
        /// <returns>Resumen con totales del día</returns>
        Task<ResumenFacturacionDiaViewModel> GetResumenFacturacionHoyAsync();

        /// <summary>
        /// Obtiene estadísticas básicas de facturación por rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estadísticas de facturación</returns>
        Task<EstadisticasFacturacionViewModel> GetEstadisticasFacturacionAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // INTEGRACIÓN CON OTROS SERVICIOS
        // ============================================================================

        /// <summary>
        /// Libera automáticamente la mesa después de crear la factura
        /// </summary>
        /// <param name="mesaId">ID de la mesa a liberar</param>
        /// <returns>True si se liberó exitosamente</returns>
        Task<bool> LiberarMesaPostFacturacionAsync(int mesaId);

        /// <summary>
        /// Actualiza el estado de la orden a "Facturada"
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>True si se actualizó exitosamente</returns>
        Task<bool> ActualizarEstadoOrdenPostFacturacionAsync(int ordenId);
    }

    // ============================================================================
    // MODELOS DE RESULTADO PARA VALIDACIONES Y CÁLCULOS
    // ============================================================================

    /// <summary>
    /// Resultado del cálculo de totales con ITBIS dominicano
    /// </summary>
    public class CalculoTotalesResult
    {
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal SubtotalConDescuento { get; set; }
        public decimal ITBIS { get; set; }
        public decimal Propina { get; set; }
        public decimal TotalFinal { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public Dictionary<string, decimal> DesglosePorCategoria { get; set; } = new();
    }

    /// <summary>
    /// Resultado de validación para facturación
    /// </summary>
    public class ValidacionFacturaResult
    {
        public bool EsValida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
        public decimal TotalEstimado { get; set; }
        public int CantidadOrdenes { get; set; }
        public string EstadoMesa { get; set; } = string.Empty;
    }
}