using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para operaciones con facturas
    /// Maneja facturación, pagos y reportes financieros del restaurante
    /// </summary>
    public interface IFacturaRepository : IBaseRepository<Factura>
    {
        // ============================================================================
        // GESTIÓN DE FACTURAS
        // ============================================================================

        /// <summary>
        /// Genera el próximo número de factura automáticamente
        /// </summary>
        /// <returns>Número de factura generado</returns>
        Task<string> GenerarNumeroFacturaAsync();

        /// <summary>
        /// Obtiene una factura por su número de factura
        /// </summary>
        /// <param name="numeroFactura">Número de factura único</param>
        /// <returns>Factura encontrada o null</returns>
        Task<Factura?> GetByNumeroFacturaAsync(string numeroFactura);

        /// <summary>
        /// Verifica si un número de factura ya existe
        /// </summary>
        /// <param name="numeroFactura">Número de factura a verificar</param>
        /// <returns>True si ya existe</returns>
        Task<bool> NumeroFacturaExisteAsync(string numeroFactura);

        /// <summary>
        /// Crea una factura para una orden específica
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <param name="metodoPago">Método de pago utilizado</param>
        /// <param name="impuesto">Impuesto aplicado (ITBIS 18%)</param>
        /// <param name="descuento">Descuento aplicado</param>
        /// <param name="propina">Propina incluida</param>
        /// <returns>Factura creada</returns>
        Task<Factura> CrearFacturaParaOrdenAsync(int ordenId, string metodoPago = "Efectivo", decimal impuesto = 0.18m, decimal descuento = 0, decimal propina = 0);

        // ============================================================================
        // CONSULTAS POR ESTADO Y MÉTODO DE PAGO
        // ============================================================================

        /// <summary>
        /// Obtiene facturas por estado específico
        /// </summary>
        /// <param name="estado">Estado de la factura (Pagada, Pendiente, Anulada)</param>
        /// <returns>Lista de facturas en el estado especificado</returns>
        Task<IEnumerable<Factura>> GetByEstadoAsync(string estado);

        /// <summary>
        /// Obtiene facturas pagadas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de facturas pagadas</returns>
        Task<IEnumerable<Factura>> GetFacturasPagadasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene facturas pendientes de pago
        /// </summary>
        /// <returns>Lista de facturas pendientes</returns>
        Task<IEnumerable<Factura>> GetFacturasPendientesAsync();

        /// <summary>
        /// Obtiene facturas anuladas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de facturas anuladas</returns>
        Task<IEnumerable<Factura>> GetFacturasAnuladasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene facturas por método de pago
        /// </summary>
        /// <param name="metodoPago">Método de pago (Efectivo, Tarjeta, Transferencia)</param>
        /// <param name="fechaInicio">Fecha inicio del filtro (opcional)</param>
        /// <param name="fechaFin">Fecha fin del filtro (opcional)</param>
        /// <returns>Lista de facturas del método de pago</returns>
        Task<IEnumerable<Factura>> GetByMetodoPagoAsync(string metodoPago, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        // ============================================================================
        // CONSULTAS POR FECHA Y TIEMPO
        // ============================================================================

        /// <summary>
        /// Obtiene facturas del día actual
        /// </summary>
        /// <returns>Lista de facturas de hoy</returns>
        Task<IEnumerable<Factura>> GetFacturasHoyAsync();

        /// <summary>
        /// Obtiene facturas de una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de facturas de la fecha</returns>
        Task<IEnumerable<Factura>> GetFacturasPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Obtiene facturas en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Lista de facturas en el rango</returns>
        Task<IEnumerable<Factura>> GetFacturasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene facturas del mes actual
        /// </summary>
        /// <returns>Lista de facturas del mes</returns>
        Task<IEnumerable<Factura>> GetFacturasMesActualAsync();

        /// <summary>
        /// Obtiene facturas de un mes específico
        /// </summary>
        /// <param name="año">Año</param>
        /// <param name="mes">Mes (1-12)</param>
        /// <returns>Lista de facturas del mes</returns>
        Task<IEnumerable<Factura>> GetFacturasPorMesAsync(int año, int mes);

        // ============================================================================
        // CONSULTAS POR ORDEN Y CLIENTE
        // ============================================================================

        /// <summary>
        /// Obtiene facturas de una orden específica
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Lista de facturas de la orden</returns>
        Task<IEnumerable<Factura>> GetByOrdenAsync(int ordenId);

        /// <summary>
        /// Obtiene la factura principal de una orden
        /// </summary>
        /// <param name="ordenId">ID de la orden</param>
        /// <returns>Factura principal de la orden o null</returns>
        Task<Factura?> GetFacturaPrincipalOrdenAsync(int ordenId);

        /// <summary>
        /// Obtiene facturas de un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Lista de facturas del cliente</returns>
        Task<IEnumerable<Factura>> GetByClienteAsync(int clienteId);

        /// <summary>
        /// Obtiene el historial de facturas de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="limite">Número máximo de facturas (por defecto 20)</param>
        /// <returns>Historial de facturas del cliente</returns>
        Task<IEnumerable<Factura>> GetHistorialClienteAsync(int clienteId, int limite = 20);

        // ============================================================================
        // OPERACIONES DE FACTURACIÓN
        // ============================================================================

        /// <summary>
        /// Procesa el pago de una factura
        /// </summary>
        /// <param name="facturaId">ID de la factura</param>
        /// <param name="metodoPago">Método de pago utilizado</param>
        /// <param name="montoPagado">Monto pagado</param>
        /// <returns>True si se procesó correctamente</returns>
        Task<bool> ProcesarPagoAsync(int facturaId, string metodoPago, decimal montoPagado);

        /// <summary>
        /// Anula una factura
        /// </summary>
        /// <param name="facturaId">ID de la factura</param>
        /// <param name="razon">Razón de la anulación</param>
        /// <returns>True si se anuló correctamente</returns>
        Task<bool> AnularFacturaAsync(int facturaId, string razon);

        /// <summary>
        /// Divide una factura en varias (para grupos)
        /// </summary>
        /// <param name="facturaOriginalId">ID de la factura original</param>
        /// <param name="divisiones">Número de divisiones</param>
        /// <returns>Lista de facturas divididas</returns>
        Task<IEnumerable<Factura>> DividirFacturaAsync(int facturaOriginalId, int divisiones);

        /// <summary>
        /// Aplica descuento a una factura
        /// </summary>
        /// <param name="facturaId">ID de la factura</param>
        /// <param name="descuento">Monto o porcentaje de descuento</param>
        /// <param name="esPorcentaje">True si es porcentaje, False si es monto fijo</param>
        /// <returns>True si se aplicó correctamente</returns>
        Task<bool> AplicarDescuentoAsync(int facturaId, decimal descuento, bool esPorcentaje = false);

        // ============================================================================
        // REPORTES FINANCIEROS
        // ============================================================================

        /// <summary>
        /// Calcula el total de ventas del día
        /// </summary>
        /// <param name="fecha">Fecha específica (opcional, por defecto hoy)</param>
        /// <returns>Total de ventas del día</returns>
        Task<decimal> GetVentasDelDiaAsync(DateTime? fecha = null);

        /// <summary>
        /// Calcula el total de ventas del mes
        /// </summary>
        /// <param name="año">Año</param>
        /// <param name="mes">Mes (1-12)</param>
        /// <returns>Total de ventas del mes</returns>
        Task<decimal> GetVentasDelMesAsync(int año, int mes);

        /// <summary>
        /// Calcula ventas por período
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Total de ventas del período</returns>
        Task<decimal> GetVentasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene reporte de ventas diarias para un período
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Reporte de ventas diarias</returns>
        Task<IEnumerable<object>> GetReporteVentasDiariasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene reporte de ventas por método de pago
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Reporte por método de pago</returns>
        Task<IEnumerable<object>> GetReporteVentasPorMetodoPagoAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene las facturas más altas del período
        /// </summary>
        /// <param name="limite">Número máximo de facturas (por defecto 10)</param>
        /// <param name="dias">Período en días (por defecto 30)</param>
        /// <returns>Lista de facturas más altas</returns>
        Task<IEnumerable<Factura>> GetFacturasMasAltasAsync(int limite = 10, int dias = 30);

        // ============================================================================
        // ESTADÍSTICAS Y ANÁLISIS
        // ============================================================================

        /// <summary>
        /// Obtiene estadísticas financieras del día
        /// </summary>
        /// <returns>Estadísticas del día actual</returns>
        Task<object> GetEstadisticasFinancierasDelDiaAsync();

        /// <summary>
        /// Obtiene estadísticas financieras del mes
        /// </summary>
        /// <returns>Estadísticas del mes actual</returns>
        Task<object> GetEstadisticasFinancierasDelMesAsync();

        /// <summary>
        /// Obtiene el ticket promedio
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio (opcional)</param>
        /// <param name="fechaFin">Fecha fin (opcional)</param>
        /// <returns>Valor promedio por factura</returns>
        Task<decimal> GetTicketPromedioAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene el total de impuestos recaudados
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Total de impuestos del período</returns>
        Task<decimal> GetTotalImpuestosAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene el total de propinas recibidas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Total de propinas del período</returns>
        Task<decimal> GetTotalPropinasAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // OPERACIONES ESPECIALES DOMINICANAS
        // ============================================================================

        /// <summary>
        /// Calcula el ITBIS (Impuesto dominicano 18%) para un período
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Total de ITBIS del período</returns>
        Task<decimal> GetTotalITBISAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Genera reporte fiscal para declaraciones en República Dominicana
        /// </summary>
        /// <param name="mes">Mes del reporte</param>
        /// <param name="año">Año del reporte</param>
        /// <returns>Reporte fiscal estructurado</returns>
        Task<object> GenerarReporteFiscalAsync(int mes, int año);
    }
}