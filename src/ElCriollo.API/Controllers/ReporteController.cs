using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElCriollo.API.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ElCriollo.API.Controllers
{
    /// <summary>
    /// Controlador para la generación de reportes y estadísticas del restaurante El Criollo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [SwaggerTag("Generación de reportes gerenciales, estadísticas y análisis de negocio")]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteService _reporteService;
        private readonly ILogger<ReporteController> _logger;

        public ReporteController(IReporteService reporteService, ILogger<ReporteController> logger)
        {
            _reporteService = reporteService;
            _logger = logger;
        }

        // ============================================================================
        // REPORTES DE VENTAS
        // ============================================================================

        /// <summary>
        /// Generar reporte de ventas diarias
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <returns>Reporte detallado de ventas diarias</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        /// <response code="400">Rango de fechas inválido</response>
        [HttpGet("ventas/diarias")]
        [SwaggerOperation(
            Summary = "Reporte de ventas diarias",
            Description = "Genera un reporte detallado de ventas por día con totales, promedios y tendencias",
            OperationId = "Reporte.VentasDiarias",
            Tags = new[] { "Reportes de Ventas" }
        )]
        [ProducesResponseType(typeof(ReporteVentasDiariasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public Task<ActionResult<ReporteVentasDiariasResponse>> GetReporteVentasDiarias(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    var badRequest = BadRequest(new ValidationProblemDetails
                    {
                        Title = "Rango de fechas inválido",
                        Detail = "La fecha de inicio debe ser anterior a la fecha fin",
                        Status = StatusCodes.Status400BadRequest
                    });

                    return Task.FromResult<ActionResult<ReporteVentasDiariasResponse>>(badRequest);
                }

                _logger.LogInformation("📊 Generando reporte de ventas diarias desde {FechaInicio} hasta {FechaFin}", 
                    fechaInicio, fechaFin);

                // TODO: Implementar GetVentasDiariasAsync en IReporteService
                // Por ahora retornamos datos vacíos
                var reporte = new ReporteVentasDiariasResponse
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    VentasTotales = 0,
                    PromedioVentaDiaria = 0,
                    DiasOperativos = 0,
                    VentasPorDia = new List<VentaDiariaDetalladaItem>(),
                    Tendencia = new GraficoTendencia()
                };
                
                _logger.LogWarning("⚠️ GetVentasDiariasAsync no está implementado. Retornando datos vacíos.");
                var result = Ok(reporte);
                return Task.FromResult<ActionResult<ReporteVentasDiariasResponse>>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de ventas diarias");
                var result = StatusCode(StatusCodes.Status500InternalServerError);
                return Task.FromResult<ActionResult<ReporteVentasDiariasResponse>>(result);
            }
        }

        /// <summary>
        /// Generar reporte de ventas por producto
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <param name="top">Cantidad de productos a mostrar (default: 10)</param>
        /// <returns>Reporte de productos más vendidos</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("ventas/productos")]
        [SwaggerOperation(
            Summary = "Reporte de ventas por producto",
            Description = "Muestra los productos más vendidos con cantidades, ingresos y porcentajes",
            OperationId = "Reporte.VentasPorProducto",
            Tags = new[] { "Reportes de Ventas" }
        )]
        [ProducesResponseType(typeof(ReporteVentasProductosResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteVentasProductosResponse>> GetReporteVentasPorProducto(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin,
            [FromQuery] int top = 10)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de ventas por producto - Top {Top}", top);

                // TODO: Implementar GetVentasPorProductoAsync en IReporteService
                // Por ahora usamos GetProductosPopularesAsync como alternativa
                var productosPopulares = await _reporteService.GetProductosPopularesAsync(fechaInicio, top);
                
                var reporte = new ReporteVentasProductosResponse
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalProductosVendidos = productosPopulares.Count,
                    TopProductos = productosPopulares.Select((p, index) => new ProductoVendidoItem
                    {
                        ProductoId = p.Posicion, // Usar Posicion como ID temporal
                        Nombre = p.Nombre,
                        Categoria = p.Categoria,
                        CantidadVendida = p.CantidadVendida,
                        IngresoTotal = decimal.TryParse(p.Ingresos.Replace("RD$", "").Replace(",", "").Trim(), out var ingreso) ? ingreso : 0,
                        PorcentajeVentas = 0, // Calcular si es necesario
                        Ranking = p.Posicion
                    }).ToList()
                };
                
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de ventas por producto");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Generar reporte de ventas por categoría
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <returns>Reporte de ventas agrupadas por categoría</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("ventas/categorias")]
        [SwaggerOperation(
            Summary = "Reporte de ventas por categoría",
            Description = "Muestra las ventas agrupadas por categoría con totales y porcentajes",
            OperationId = "Reporte.VentasPorCategoria",
            Tags = new[] { "Reportes de Ventas" }
        )]
        [ProducesResponseType(typeof(ReporteVentasCategoriasResponse), StatusCodes.Status200OK)]
        public Task<ActionResult<ReporteVentasCategoriasResponse>> GetReporteVentasPorCategoria(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de ventas por categoría");

                // TODO: Implementar GetVentasPorCategoriaAsync en IReporteService
                // Por ahora retornamos datos vacíos
                var reporte = new ReporteVentasCategoriasResponse
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    VentasTotales = 0,
                    Categorias = new List<CategoriaVentaItem>()
                };
                
                _logger.LogWarning("⚠️ GetVentasPorCategoriaAsync no está implementado. Retornando datos vacíos.");
                var result = Ok(reporte);
                return Task.FromResult<ActionResult<ReporteVentasCategoriasResponse>>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de ventas por categoría");
                var result = StatusCode(StatusCodes.Status500InternalServerError);
                return Task.FromResult<ActionResult<ReporteVentasCategoriasResponse>>(result);
            }
        }

        /// <summary>
        /// Generar reporte de ventas por mesero
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <returns>Reporte de rendimiento por mesero</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("ventas/meseros")]
        [SwaggerOperation(
            Summary = "Reporte de ventas por mesero",
            Description = "Muestra el rendimiento de cada mesero con ventas, propinas y promedios",
            OperationId = "Reporte.VentasPorMesero",
            Tags = new[] { "Reportes de Ventas" }
        )]
        [ProducesResponseType(typeof(ReporteVentasMeserosResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteVentasMeserosResponse>> GetReporteVentasPorMesero(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de ventas por mesero");

                var reporte = await _reporteService.GetVentasPorMeseroAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de ventas por mesero");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REPORTES OPERACIONALES
        // ============================================================================

        /// <summary>
        /// Generar reporte de ocupación de mesas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <returns>Reporte de ocupación y rotación de mesas</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("operacional/ocupacion-mesas")]
        [SwaggerOperation(
            Summary = "Reporte de ocupación de mesas",
            Description = "Analiza la ocupación, rotación y eficiencia en el uso de mesas",
            OperationId = "Reporte.OcupacionMesas",
            Tags = new[] { "Reportes Operacionales" }
        )]
        [ProducesResponseType(typeof(ReporteOcupacionMesasResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteOcupacionMesasResponse>> GetReporteOcupacionMesas(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de ocupación de mesas");

                var reporte = await _reporteService.GetOcupacionMesasAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de ocupación de mesas");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Generar reporte de tiempos de servicio
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <returns>Análisis de tiempos de preparación y servicio</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("operacional/tiempos-servicio")]
        [SwaggerOperation(
            Summary = "Reporte de tiempos de servicio",
            Description = "Analiza los tiempos de preparación, servicio y atención al cliente",
            OperationId = "Reporte.TiemposServicio",
            Tags = new[] { "Reportes Operacionales" }
        )]
        [ProducesResponseType(typeof(ReporteTiemposServicioResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteTiemposServicioResponse>> GetReporteTiemposServicio(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de tiempos de servicio");

                var reporte = await _reporteService.GetTiemposServicioAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de tiempos de servicio");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REPORTES DE INVENTARIO
        // ============================================================================

        /// <summary>
        /// Generar reporte de inventario actual
        /// </summary>
        /// <returns>Estado actual del inventario con valorización</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("inventario/actual")]
        [SwaggerOperation(
            Summary = "Reporte de inventario actual",
            Description = "Muestra el estado actual del inventario con cantidades, costos y alertas",
            OperationId = "Reporte.InventarioActual",
            Tags = new[] { "Reportes de Inventario" }
        )]
        [ProducesResponseType(typeof(ReporteInventarioActualResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteInventarioActualResponse>> GetReporteInventarioActual()
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de inventario actual");

                var reporte = await _reporteService.GetInventarioActualAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de inventario actual");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Generar reporte de movimientos de inventario
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del reporte</param>
        /// <param name="fechaFin">Fecha fin del reporte</param>
        /// <returns>Movimientos de entrada y salida del inventario</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("inventario/movimientos")]
        [SwaggerOperation(
            Summary = "Reporte de movimientos de inventario",
            Description = "Detalla todos los movimientos de entrada y salida del inventario",
            OperationId = "Reporte.MovimientosInventario",
            Tags = new[] { "Reportes de Inventario" }
        )]
        [ProducesResponseType(typeof(ReporteMovimientosInventarioResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReporteMovimientosInventarioResponse>> GetReporteMovimientosInventario(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de movimientos de inventario");

                var reporte = await _reporteService.GetMovimientosInventarioAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de movimientos de inventario");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REPORTES FINANCIEROS
        // ============================================================================

        /// <summary>
        /// Generar estado de resultados
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del período</param>
        /// <param name="fechaFin">Fecha fin del período</param>
        /// <returns>Estado de resultados con ingresos, costos y utilidades</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("financiero/estado-resultados")]
        [SwaggerOperation(
            Summary = "Estado de resultados",
            Description = "Genera el estado de resultados con análisis de ingresos, costos y márgenes",
            OperationId = "Reporte.EstadoResultados",
            Tags = new[] { "Reportes Financieros" }
        )]
        [ProducesResponseType(typeof(EstadoResultadosResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<EstadoResultadosResponse>> GetEstadoResultados(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando estado de resultados");

                var reporte = await _reporteService.GetEstadoResultadosAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar estado de resultados");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Generar reporte de flujo de caja
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio del período</param>
        /// <param name="fechaFin">Fecha fin del período</param>
        /// <returns>Flujo de caja con entradas y salidas</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        [HttpGet("financiero/flujo-caja")]
        [SwaggerOperation(
            Summary = "Reporte de flujo de caja",
            Description = "Analiza el flujo de efectivo con entradas, salidas y saldo final",
            OperationId = "Reporte.FlujoCaja",
            Tags = new[] { "Reportes Financieros" }
        )]
        [ProducesResponseType(typeof(FlujoCajaResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<FlujoCajaResponse>> GetFlujoCaja(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                _logger.LogInformation("📊 Generando reporte de flujo de caja");

                var reporte = await _reporteService.GetFlujoCajaAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar flujo de caja");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // REPORTES ANALÍTICOS Y PREDICTIVOS
        // ============================================================================

        /// <summary>
        /// Generar análisis de tendencias
        /// </summary>
        /// <param name="meses">Cantidad de meses a analizar (default: 6)</param>
        /// <returns>Análisis de tendencias y proyecciones</returns>
        /// <response code="200">Análisis generado exitosamente</response>
        [HttpGet("analitico/tendencias")]
        [SwaggerOperation(
            Summary = "Análisis de tendencias",
            Description = "Analiza tendencias históricas y genera proyecciones básicas",
            OperationId = "Reporte.Tendencias",
            Tags = new[] { "Reportes Analíticos" }
        )]
        [ProducesResponseType(typeof(AnalisisTendenciasResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AnalisisTendenciasResponse>> GetAnalisisTendencias([FromQuery] int meses = 6)
        {
            try
            {
                _logger.LogInformation("📊 Generando análisis de tendencias para {Meses} meses", meses);

                var fechaFin = DateTime.Today;
                var fechaInicio = fechaFin.AddMonths(-meses);

                var reporte = await _reporteService.GetAnalisisTendenciasAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar análisis de tendencias");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Generar dashboard ejecutivo
        /// </summary>
        /// <returns>Resumen ejecutivo con KPIs principales</returns>
        /// <response code="200">Dashboard generado exitosamente</response>
        [HttpGet("dashboard/ejecutivo")]
        [SwaggerOperation(
            Summary = "Dashboard ejecutivo",
            Description = "Genera un resumen ejecutivo con los KPIs más importantes del negocio",
            OperationId = "Reporte.DashboardEjecutivo",
            Tags = new[] { "Dashboard" }
        )]
        [ProducesResponseType(typeof(DashboardEjecutivoResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardEjecutivoResponse>> GetDashboardEjecutivo()
        {
            try
            {
                _logger.LogInformation("📊 Generando dashboard ejecutivo");

                var reporte = await _reporteService.GetDashboardEjecutivoAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar dashboard ejecutivo");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // ============================================================================
        // DTOs DE RESPUESTA
        // ============================================================================

        public class ReporteVentasDiariasResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal VentasTotales { get; set; }
            public decimal PromedioVentaDiaria { get; set; }
            public int DiasOperativos { get; set; }
            public List<VentaDiariaDetalladaItem> VentasPorDia { get; set; } = new();
            public GraficoTendencia Tendencia { get; set; } = new();
        }

        public class VentaDiariaDetalladaItem
        {
            public DateTime Fecha { get; set; }
            public decimal VentasBrutas { get; set; }
            public decimal Descuentos { get; set; }
            public decimal ITBIS { get; set; }
            public decimal VentasNetas { get; set; }
            public int CantidadOrdenes { get; set; }
            public decimal TicketPromedio { get; set; }
        }

        public class GraficoTendencia
        {
            public string Tipo { get; set; } = "Creciente";
            public decimal Porcentaje { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }

        public class ReporteVentasProductosResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public int TotalProductosVendidos { get; set; }
            public List<ProductoVendidoItem> TopProductos { get; set; } = new();
        }

        public class ProductoVendidoItem
        {
            public int ProductoId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public int CantidadVendida { get; set; }
            public decimal IngresoTotal { get; set; }
            public decimal PorcentajeVentas { get; set; }
            public int Ranking { get; set; }
        }

        public class ReporteVentasCategoriasResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal VentasTotales { get; set; }
            public List<CategoriaVentaItem> Categorias { get; set; } = new();
        }

        public class CategoriaVentaItem
        {
            public string Categoria { get; set; } = string.Empty;
            public decimal VentasTotal { get; set; }
            public int CantidadProductos { get; set; }
            public decimal PorcentajeTotal { get; set; }
            public decimal TicketPromedio { get; set; }
        }

        public class ReporteVentasMeserosResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public List<MeseroVentaItem> Meseros { get; set; } = new();
        }

        public class MeseroVentaItem
        {
            public int EmpleadoId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public decimal VentasTotal { get; set; }
            public int CantidadOrdenes { get; set; }
            public decimal PropinaTotal { get; set; }
            public decimal TicketPromedio { get; set; }
            public int Ranking { get; set; }
        }

        public class ReporteOcupacionMesasResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal TasaOcupacionPromedio { get; set; }
            public TimeSpan TiempoPromedioOcupacion { get; set; }
            public List<MesaOcupacionItem> MesasDetalle { get; set; } = new();
            public Dictionary<string, decimal> OcupacionPorHora { get; set; } = new();
        }

        public class MesaOcupacionItem
        {
            public int MesaId { get; set; }
            public int NumeroMesa { get; set; }
            public decimal TasaOcupacion { get; set; }
            public int VecesOcupada { get; set; }
            public TimeSpan TiempoPromedioOcupacion { get; set; }
            public decimal IngresoGenerado { get; set; }
        }

        public class ReporteTiemposServicioResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public TimeSpan TiempoPromedioPreparacion { get; set; }
            public TimeSpan TiempoPromedioServicio { get; set; }
            public TimeSpan TiempoPromedioTotal { get; set; }
            public Dictionary<string, TimeSpan> TiemposPorCategoria { get; set; } = new();
            public List<TiempoServicioHoraItem> TiemposPorHora { get; set; } = new();
        }

        public class TiempoServicioHoraItem
        {
            public string Hora { get; set; } = string.Empty;
            public TimeSpan TiempoPromedio { get; set; }
            public int CantidadOrdenes { get; set; }
        }

        public class ReporteInventarioActualResponse
        {
            public DateTime FechaReporte { get; set; }
            public decimal ValorTotalInventario { get; set; }
            public int TotalProductos { get; set; }
            public int ProductosStockBajo { get; set; }
            public int ProductosAgotados { get; set; }
            public List<InventarioProductoItem> Productos { get; set; } = new();
        }

        public class InventarioProductoItem
        {
            public int ProductoId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
            public decimal CantidadDisponible { get; set; }
            public decimal StockMinimo { get; set; }
            public decimal CostoUnitario { get; set; }
            public decimal ValorTotal { get; set; }
            public string Estado { get; set; } = string.Empty;
        }

        public class ReporteMovimientosInventarioResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public int TotalMovimientos { get; set; }
            public decimal TotalEntradas { get; set; }
            public decimal TotalSalidas { get; set; }
            public List<MovimientoInventarioItem> Movimientos { get; set; } = new();
        }

        public class MovimientoInventarioItem
        {
            public DateTime Fecha { get; set; }
            public string TipoMovimiento { get; set; } = string.Empty;
            public string Producto { get; set; } = string.Empty;
            public decimal Cantidad { get; set; }
            public decimal CostoUnitario { get; set; }
            public string Motivo { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }

        public class EstadoResultadosResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal Ingresos { get; set; }
            public decimal CostoVentas { get; set; }
            public decimal UtilidadBruta { get; set; }
            public decimal MargenBruto { get; set; }
            public decimal GastosOperativos { get; set; }
            public decimal UtilidadOperativa { get; set; }
            public decimal MargenOperativo { get; set; }
            public decimal ITBIS { get; set; }
            public decimal UtilidadNeta { get; set; }
            public decimal MargenNeto { get; set; }
        }

        public class FlujoCajaResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public decimal SaldoInicial { get; set; }
            public decimal TotalEntradas { get; set; }
            public decimal TotalSalidas { get; set; }
            public decimal SaldoFinal { get; set; }
            public Dictionary<string, decimal> EntradasPorTipo { get; set; } = new();
            public Dictionary<string, decimal> SalidasPorTipo { get; set; } = new();
            public List<MovimientoCajaItem> Movimientos { get; set; } = new();
        }

        public class MovimientoCajaItem
        {
            public DateTime Fecha { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string Concepto { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public decimal SaldoAcumulado { get; set; }
        }

        public class AnalisisTendenciasResponse
        {
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
            public TendenciaItem TendenciaVentas { get; set; } = new();
            public TendenciaItem TendenciaClientes { get; set; } = new();
            public TendenciaItem TendenciaTicketPromedio { get; set; } = new();
            public List<ProyeccionItem> Proyecciones { get; set; } = new();
            public List<string> Recomendaciones { get; set; } = new();
        }

        public class TendenciaItem
        {
            public string Metrica { get; set; } = string.Empty;
            public string Tendencia { get; set; } = string.Empty;
            public decimal CambioPocentual { get; set; }
            public string Interpretacion { get; set; } = string.Empty;
        }

        public class ProyeccionItem
        {
            public string Periodo { get; set; } = string.Empty;
            public decimal ValorProyectado { get; set; }
            public decimal MargenError { get; set; }
        }

        public class DashboardEjecutivoResponse
        {
            public DateTime FechaActualizacion { get; set; }
            public ResumenVentasKPI Ventas { get; set; } = new();
            public ResumenOperacionalKPI Operacional { get; set; } = new();
            public ResumenFinancieroKPI Financiero { get; set; } = new();
            public List<AlertaImportante> Alertas { get; set; } = new();
            public List<MetricaClave> MetricasClave { get; set; } = new();
        }

        public class ResumenVentasKPI
        {
            public decimal VentasHoy { get; set; }
            public decimal VentasSemana { get; set; }
            public decimal VentasMes { get; set; }
            public decimal CrecimientoMensual { get; set; }
            public int OrdenesHoy { get; set; }
            public decimal TicketPromedioHoy { get; set; }
        }

        public class ResumenOperacionalKPI
        {
            public decimal TasaOcupacionActual { get; set; }
            public int MesasOcupadas { get; set; }
            public int ReservacionesHoy { get; set; }
            public TimeSpan TiempoPromedioServicio { get; set; }
            public int ClientesAtendidosHoy { get; set; }
        }

        public class ResumenFinancieroKPI
        {
            public decimal MargenBrutoMes { get; set; }
            public decimal CostoVentasPorcentaje { get; set; }
            public decimal PropinaPromedioMes { get; set; }
            public Dictionary<string, decimal> VentasPorMetodoPago { get; set; } = new();
        }

        public class AlertaImportante
        {
            public string Tipo { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
            public string Severidad { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
        }

        public class MetricaClave
        {
            public string Nombre { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public string Unidad { get; set; } = string.Empty;
            public decimal CambioPorcentual { get; set; }
            public string Tendencia { get; set; } = string.Empty;
        }
    }
}