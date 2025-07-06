namespace ElCriollo.API.Interfaces
{
    /// <summary>
    /// Interfaz específica para reportes complejos y análisis del restaurante
    /// Maneja consultas avanzadas para dashboards, estadísticas y reportes gerenciales
    /// </summary>
    public interface IReporteRepository
    {
        // ============================================================================
        // DASHBOARD PRINCIPAL
        // ============================================================================

        /// <summary>
        /// Obtiene datos completos para el dashboard principal
        /// </summary>
        /// <returns>Objeto con todas las métricas del dashboard</returns>
        Task<object> GetDashboardPrincipalAsync();

        /// <summary>
        /// Obtiene métricas en tiempo real para el dashboard
        /// </summary>
        /// <returns>Métricas actualizadas del día</returns>
        Task<object> GetMetricasTiempoRealAsync();

        /// <summary>
        /// Obtiene resumen ejecutivo del restaurante
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del período</param>
        /// <param name="fechaFin">Fecha fin del período</param>
        /// <returns>Resumen ejecutivo con KPIs principales</returns>
        Task<object> GetResumenEjecutivoAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES DE VENTAS
        // ============================================================================

        /// <summary>
        /// Obtiene reporte detallado de ventas por período
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <param name="agruparPor">Agrupación (dia, semana, mes)</param>
        /// <returns>Reporte de ventas detallado</returns>
        Task<object> GetReporteVentasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "dia");

        /// <summary>
        /// Obtiene tendencias de ventas comparando períodos
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio período actual</param>
        /// <param name="fechaFin">Fecha fin período actual</param>
        /// <param name="compararCon">Período de comparación (mes_anterior, año_anterior)</param>
        /// <returns>Análisis de tendencias</returns>
        Task<object> GetTendenciasVentasAsync(DateTime fechaInicio, DateTime fechaFin, string compararCon = "mes_anterior");

        /// <summary>
        /// Obtiene análisis de ventas por hora del día
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Distribución de ventas por hora</returns>
        Task<object> GetVentasPorHoraAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis de ventas por día de la semana
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Distribución de ventas por día de semana</returns>
        Task<object> GetVentasPorDiaSemanlaAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES DE PRODUCTOS
        // ============================================================================

        /// <summary>
        /// Obtiene análisis completo de productos más vendidos
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <param name="limite">Número de productos (por defecto 20)</param>
        /// <returns>Análisis de productos top</returns>
        Task<object> GetAnalisisProductosTopAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 20);

        /// <summary>
        /// Obtiene productos con bajo rendimiento
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <param name="limite">Número de productos (por defecto 10)</param>
        /// <returns>Productos con ventas bajas</returns>
        Task<object> GetProductosBajoRendimientoAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 10);

        /// <summary>
        /// Obtiene análisis de rentabilidad por producto
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Rentabilidad por producto</returns>
        Task<object> GetRentabilidadProductosAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis de ventas por categoría de producto
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Ventas agrupadas por categoría</returns>
        Task<object> GetVentasPorCategoriaAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES DE OPERACIONES
        // ============================================================================

        /// <summary>
        /// Obtiene reporte de eficiencia operacional
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Métricas de eficiencia</returns>
        Task<object> GetReporteEficienciaAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis de ocupación de mesas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estadísticas de ocupación de mesas</returns>
        Task<object> GetAnalisisOcupacionMesasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene tiempos promedio de servicio
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Tiempos de preparación y servicio</returns>
        Task<object> GetTiemposServicioAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis de reservaciones vs walk-ins
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Comparación reservaciones vs clientes sin reserva</returns>
        Task<object> GetAnalisisReservacionesAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES DE EMPLEADOS
        // ============================================================================

        /// <summary>
        /// Obtiene reporte de performance de empleados
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Métricas de performance por empleado</returns>
        Task<object> GetPerformanceEmpleadosAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene ventas por empleado/mesero
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Ventas generadas por cada empleado</returns>
        Task<object> GetVentasPorEmpleadoAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis de propinas por empleado
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Propinas recibidas por empleado</returns>
        Task<object> GetPropinasPorEmpleadoAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES DE CLIENTES
        // ============================================================================

        /// <summary>
        /// Obtiene análisis de comportamiento de clientes
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Patrones de comportamiento de clientes</returns>
        Task<object> GetComportamientoClientesAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene clientes más frecuentes y valiosos
        /// </summary>
        /// <param name="limite">Número de clientes (por defecto 20)</param>
        /// <param name="dias">Período en días (por defecto 90)</param>
        /// <returns>Top clientes por frecuencia y valor</returns>
        Task<object> GetTopClientesAsync(int limite = 20, int dias = 90);

        /// <summary>
        /// Obtiene análisis de ticket promedio por cliente
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Ticket promedio y distribución</returns>
        Task<object> GetTicketPromedioClientesAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES FINANCIEROS
        // ============================================================================

        /// <summary>
        /// Obtiene estado de resultados simplificado
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Estado de resultados del período</returns>
        Task<object> GetEstadoResultadosAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis de márgenes de ganancia
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Análisis de márgenes por producto y categoría</returns>
        Task<object> GetAnalisisMargenesAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene flujo de caja diario
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Flujo de caja del período</returns>
        Task<object> GetFlujoCajaAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene proyección de ventas basada en históricos
        /// </summary>
        /// <param name="diasProyeccion">Días a proyectar (por defecto 30)</param>
        /// <returns>Proyección de ventas</returns>
        Task<object> GetProyeccionVentasAsync(int diasProyeccion = 30);

        // ============================================================================
        // REPORTES DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Obtiene reporte de rotación de inventario
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Análisis de rotación de productos</returns>
        Task<object> GetRotacionInventarioAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene productos que necesitan restock urgente
        /// </summary>
        /// <returns>Lista de productos con stock crítico</returns>
        Task<object> GetAlertasInventarioAsync();

        /// <summary>
        /// Obtiene análisis de desperdicio y merma
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Reporte de desperdicio</returns>
        Task<object> GetAnalisisDesperdicioAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES ESPECIALES DOMINICANOS
        // ============================================================================

        /// <summary>
        /// Obtiene análisis de ventas de comida típica dominicana
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Popularidad de platos dominicanos</returns>
        Task<object> GetAnalisisComidaDominicanaAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene reporte de eventos especiales dominicanos
        /// Analiza ventas en fechas especiales como Merengue Festival, etc.
        /// </summary>
        /// <param name="año">Año a analizar</param>
        /// <returns>Impacto de eventos especiales en ventas</returns>
        Task<object> GetReporteEventosEspecialesAsync(int año);

        /// <summary>
        /// Obtiene análisis de preferencias por región dominicana
        /// Si se registra la procedencia de clientes
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Preferencias culinarias por región</returns>
        Task<object> GetAnalisisRegionalAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // EXPORTACIÓN DE REPORTES
        // ============================================================================

        /// <summary>
        /// Genera reporte completo en formato exportable
        /// </summary>
        /// <param name="tipoReporte">Tipo de reporte a generar</param>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <param name="formato">Formato de exportación (json, csv, pdf)</param>
        /// <returns>Datos del reporte formateados</returns>
        Task<object> GenerarReporteCompletoAsync(string tipoReporte, DateTime fechaInicio, DateTime fechaFin, string formato = "json");

        /// <summary>
        /// Obtiene configuración disponible de reportes
        /// </summary>
        /// <returns>Lista de tipos de reportes disponibles</returns>
        Task<object> GetConfiguracionReportesAsync();

        // ============================================================================
        // ANÁLISIS PREDICTIVO BÁSICO
        // ============================================================================

        /// <summary>
        /// Obtiene patrones de demanda por día/hora
        /// </summary>
        /// <param name="diasHistorico">Días de historial a analizar (por defecto 90)</param>
        /// <returns>Patrones de demanda identificados</returns>
        Task<object> GetPatronesDemandaAsync(int diasHistorico = 90);

        /// <summary>
        /// Obtiene recomendaciones basadas en análisis de datos
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio</param>
        /// <param name="fechaFin">Fecha fin</param>
        /// <returns>Recomendaciones para mejorar el negocio</returns>
        Task<object> GetRecomendacionesNegocioAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene alertas y notificaciones importantes
        /// </summary>
        /// <returns>Alertas del sistema que requieren atención</returns>
        Task<object> GetAlertasSistemaAsync();
    }
}