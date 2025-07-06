using ElCriollo.API.Models.ViewModels;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de reportes y analytics del restaurante El Criollo
    /// </summary>
    public interface IReporteService
    {
        // ============================================================================
        // DASHBOARD PRINCIPAL
        // ============================================================================

        /// <summary>
        /// Obtiene todos los datos para el dashboard principal
        /// </summary>
        /// <returns>Dashboard completo con métricas del día</returns>
        Task<DashboardViewModel> GetDashboardPrincipalAsync();

        /// <summary>
        /// Obtiene el resumen diario de actividad
        /// </summary>
        /// <param name="fecha">Fecha del resumen (por defecto hoy)</param>
        /// <returns>Resumen de ventas y actividad del día</returns>
        Task<ResumenDiarioViewModel> GetResumenDiarioAsync(DateTime? fecha = null);

        /// <summary>
        /// Obtiene el estado actual de todas las mesas
        /// </summary>
        /// <returns>Estado y estadísticas de mesas</returns>
        Task<EstadoMesasViewModel> GetEstadoMesasAsync();

        /// <summary>
        /// Obtiene las órdenes activas en el sistema
        /// </summary>
        /// <returns>Información de órdenes en proceso</returns>
        Task<OrdenesActivasViewModel> GetOrdenesActivasAsync();

        /// <summary>
        /// Obtiene alertas e indicadores del sistema
        /// </summary>
        /// <returns>Alertas de stock, mesas, órdenes, etc.</returns>
        Task<AlertasViewModel> GetAlertasSistemaAsync();

        // ============================================================================
        // REPORTES DE PRODUCTOS Y VENTAS
        // ============================================================================

        /// <summary>
        /// Obtiene los productos más vendidos del día
        /// </summary>
        /// <param name="fecha">Fecha a consultar (por defecto hoy)</param>
        /// <param name="limite">Número de productos a retornar</param>
        /// <returns>Lista de productos populares</returns>
        Task<List<ProductoPopularViewModel>> GetProductosPopularesAsync(DateTime? fecha = null, int limite = 5);

        /// <summary>
        /// Obtiene estadísticas básicas de ventas por período
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Estadísticas de ventas del período</returns>
        Task<EstadisticasVentasViewModel> GetEstadisticasVentasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene análisis básico de la comida dominicana
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Análisis de preferencias culinarias dominicanas</returns>
        Task<AnalisisComidaDominicanaViewModel> GetAnalisisComidaDominicanaAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES DE OPERACIONES
        // ============================================================================

        /// <summary>
        /// Obtiene las reservaciones próximas
        /// </summary>
        /// <param name="horasAdelante">Horas hacia adelante a consultar</param>
        /// <returns>Lista de reservaciones próximas</returns>
        Task<List<ReservacionProximaViewModel>> GetReservacionesProximasAsync(int horasAdelante = 4);

        /// <summary>
        /// Obtiene información de empleados activos
        /// </summary>
        /// <returns>Lista de empleados conectados y su actividad</returns>
        Task<List<EmpleadoActivoViewModel>> GetEmpleadosActivosAsync();

        /// <summary>
        /// Obtiene estadísticas básicas de ocupación de mesas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Estadísticas de ocupación</returns>
        Task<EstadisticasOcupacionViewModel> GetEstadisticasOcupacionAsync(DateTime fechaInicio, DateTime fechaFin);

        // ============================================================================
        // REPORTES FINANCIEROS BÁSICOS
        // ============================================================================

        /// <summary>
        /// Obtiene resumen financiero básico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Resumen de ingresos, ITBIS y propinas</returns>
        Task<ResumenFinancieroViewModel> GetResumenFinancieroAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene comparación de ventas con períodos anteriores
        /// </summary>
        /// <param name="fechaActual">Fecha del período actual</param>
        /// <param name="diasComparacion">Días hacia atrás para comparar</param>
        /// <returns>Comparación de rendimiento</returns>
        Task<ComparacionVentasViewModel> GetComparacionVentasAsync(DateTime fechaActual, int diasComparacion = 7);

        // ============================================================================
        // UTILIDADES Y CONFIGURACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene métricas clave para el período actual
        /// </summary>
        /// <returns>KPIs principales del restaurante</returns>
        Task<MetricasClaveViewModel> GetMetricasClaveAsync();

        /// <summary>
        /// Valida si hay datos suficientes para generar reportes
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Resultado de validación</returns>
        Task<ValidacionReporteResult> ValidarDatosParaReporteAsync(DateTime fechaInicio, DateTime fechaFin);
    }

    // ============================================================================
    // VIEWMODELS ADICIONALES PARA REPORTES
    // ============================================================================

    /// <summary>
    /// ViewModel para estadísticas de ventas
    /// </summary>
    public class EstadisticasVentasViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TotalVentas { get; set; } = "RD$ 0.00";
        public int TotalOrdenes { get; set; }
        public string PromedioOrden { get; set; } = "RD$ 0.00";
        public string VentaDiaria { get; set; } = "RD$ 0.00";
        public decimal CrecimientoRespectoPeriodoAnterior { get; set; }
        public Dictionary<string, decimal> VentasPorCategoria { get; set; } = new();
        public List<string> DiasConMayorVenta { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para análisis de comida dominicana
    /// </summary>
    public class AnalisisComidaDominicanaViewModel
    {
        public string PlatoMasVendido { get; set; } = string.Empty;
        public string CategoriaPreferida { get; set; } = string.Empty;
        public string HorarioPreferido { get; set; } = string.Empty;
        public decimal PorcentajeComidaTipica { get; set; }
        public List<string> ProductosTradicionales { get; set; } = new();
        public Dictionary<string, int> PreferenciasRegionales { get; set; } = new();
        public string Recomendacion { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel para estadísticas de ocupación
    /// </summary>
    public class EstadisticasOcupacionViewModel
    {
        public decimal PorcentajeOcupacionPromedio { get; set; }
        public TimeSpan TiempoPromedioOcupacion { get; set; }
        public int MesaMasUsada { get; set; }
        public string HorarioPico { get; set; } = string.Empty;
        public string HorarioValle { get; set; } = string.Empty;
        public Dictionary<string, decimal> OcupacionPorDia { get; set; } = new();
        public List<string> Recomendaciones { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para resumen financiero
    /// </summary>
    public class ResumenFinancieroViewModel
    {
        public string TotalIngresos { get; set; } = "RD$ 0.00";
        public string TotalITBIS { get; set; } = "RD$ 0.00";
        public string TotalPropinas { get; set; } = "RD$ 0.00";
        public string IngresosSinImpuestos { get; set; } = "RD$ 0.00";
        public decimal PorcentajeITBIS { get; set; } = 18.0m;
        public Dictionary<string, decimal> IngresosPorMetodoPago { get; set; } = new();
        public string CrecimientoMensual { get; set; } = "0%";
    }

    /// <summary>
    /// ViewModel para comparación de ventas
    /// </summary>
    public class ComparacionVentasViewModel
    {
        public string PeriodoActual { get; set; } = string.Empty;
        public string PeriodoAnterior { get; set; } = string.Empty;
        public string VentasActuales { get; set; } = "RD$ 0.00";
        public string VentasAnteriores { get; set; } = "RD$ 0.00";
        public decimal PorcentajeCambio { get; set; }
        public string Tendencia { get; set; } = "Estable";
        public List<string> FactoresCambio { get; set; } = new();
        public string Recomendacion { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel para métricas clave
    /// </summary>
    public class MetricasClaveViewModel
    {
        public string VentasHoy { get; set; } = "RD$ 0.00";
        public string MetaDiaria { get; set; } = "RD$ 15,000.00";
        public decimal PorcentajeMeta { get; set; }
        public int ClientesAtendidosHoy { get; set; }
        public string TicketPromedio { get; set; } = "RD$ 0.00";
        public decimal SatisfaccionCliente { get; set; } = 85.0m;
        public int MesasDisponibles { get; set; }
        public string TiempoPromedioServicio { get; set; } = "0 min";
    }

    /// <summary>
    /// Resultado de validación para reportes
    /// </summary>
    public class ValidacionReporteResult
    {
        public bool TieneDatosSuficientes { get; set; }
        public List<string> Advertencias { get; set; } = new();
        public List<string> Recomendaciones { get; set; } = new();
        public int TotalRegistrosEncontrados { get; set; }
        public DateTime PrimerRegistro { get; set; }
        public DateTime UltimoRegistro { get; set; }
    }
}