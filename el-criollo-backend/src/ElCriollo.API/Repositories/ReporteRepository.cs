using Microsoft.EntityFrameworkCore;
using ElCriollo.API.Data;
using ElCriollo.API.Interfaces;
using System.Globalization;

namespace ElCriollo.API.Repositories
{
    /// <summary>
    /// Implementación específica para reportes complejos y análisis del restaurante
    /// Maneja consultas avanzadas para dashboards, estadísticas y reportes gerenciales
    /// </summary>
    public class ReporteRepository : IReporteRepository
    {
        protected readonly ElCriolloDbContext _context;
        protected readonly ILogger<ReporteRepository> _logger;

        // Lista de productos típicamente dominicanos para análisis
        private readonly string[] _productosDominicanos = {
            "Pollo Guisado", "Res Guisada", "Cerdo Guisado", "Pescao Frito", "Chicharrón", "Pernil",
            "Mangú", "Tres Golpes", "Huevos Rancheros", "Casabe", "Yuca Hervida", "Yautía",
            "Arroz Blanco", "Habichuelas Rojas", "Moro de Guandules", "Locrio", "Ensalada Verde",
            "Tostones", "Yuca Frita", "Maduros", "Chicharrones", "Arepitas", "Bollitos de Yuca",
            "Morir Soñando", "Jugo de Chinola", "Mamajuana", "Cerveza Presidente", "Malta Morena",
            "Tres Leches", "Flan de Coco", "Majarete", "Dulce de Leche", "Cake de Ron",
            "Sancocho", "Sopa de Pollo", "Mondongo", "Asopao", "Caldo de Pollo"
        };

        public ReporteRepository(ElCriolloDbContext context, ILogger<ReporteRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================================================
        // DASHBOARD PRINCIPAL
        // ============================================================================

        /// <summary>
        /// Obtiene datos completos para el dashboard principal
        /// </summary>
        public async Task<object> GetDashboardPrincipalAsync()
        {
            try
            {
                _logger.LogDebug("Obteniendo datos para dashboard principal");

                var hoy = DateTime.Today;
                var mesActual = new DateTime(hoy.Year, hoy.Month, 1);
                var semanaActual = hoy.AddDays(-(int)hoy.DayOfWeek);

                // Métricas del día
                var ventasHoy = await _context.Facturas
                    .Where(f => f.FechaFactura >= hoy && f.FechaFactura < hoy.AddDays(1) && f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                var ordenesHoy = await _context.Ordenes
                    .CountAsync(o => o.FechaCreacion >= hoy && o.FechaCreacion < hoy.AddDays(1));

                var clientesHoy = await _context.Ordenes
                    .Where(o => o.FechaCreacion >= hoy && o.FechaCreacion < hoy.AddDays(1) && o.ClienteID != null)
                    .Select(o => o.ClienteID)
                    .Distinct()
                    .CountAsync();

                // Estado de mesas
                var estadoMesas = await _context.Mesas
                    .GroupBy(m => m.Estado)
                    .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                    .ToListAsync();

                // Órdenes pendientes
                var ordenesPendientes = await _context.Ordenes
                    .CountAsync(o => o.Estado == "Pendiente" || o.Estado == "EnPreparacion");

                // Productos más vendidos hoy
                var productosTopHoy = await _context.DetalleOrdenes
                    .Include(d => d.Producto)
                    .Include(d => d.Orden)
                    .Where(d => d.Orden.FechaCreacion >= hoy && 
                               d.Orden.FechaCreacion < hoy.AddDays(1) &&
                               d.ProductoID != null &&
                               d.Orden.Estado != "Cancelada")
                    .GroupBy(d => new { d.ProductoID, d.Producto!.Nombre })
                    .Select(g => new
                    {
                        Nombre = g.Key.Nombre,
                        Cantidad = g.Sum(d => d.Cantidad)
                    })
                    .OrderByDescending(p => p.Cantidad)
                    .Take(5)
                    .ToListAsync();

                // Reservaciones de hoy
                var reservacionesHoy = await _context.Reservaciones
                    .CountAsync(r => r.FechaYHora >= hoy && r.FechaYHora < hoy.AddDays(1));

                var dashboard = new
                {
                    FechaActualizacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    MetricasDelDia = new
                    {
                        VentasHoy = Math.Round(ventasHoy, 2),
                        OrdenesHoy = ordenesHoy,
                        ClientesHoy = clientesHoy,
                        ReservacionesHoy = reservacionesHoy
                    },
                    EstadoMesas = estadoMesas,
                    OrdenesPendientes = ordenesPendientes,
                    ProductosTopHoy = productosTopHoy,
                    AlertasImportantes = await GetAlertasSistemaAsync()
                };

                _logger.LogDebug("Dashboard principal generado exitosamente");
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar dashboard principal");
                throw;
            }
        }

        /// <summary>
        /// Obtiene métricas en tiempo real para el dashboard
        /// </summary>
        public async Task<object> GetMetricasTiempoRealAsync()
        {
            try
            {
                var ahora = DateTime.Now;
                var hoy = DateTime.Today;

                var metricas = new
                {
                    Timestamp = ahora.ToString("yyyy-MM-dd HH:mm:ss"),
                    MesasLibres = await _context.Mesas.CountAsync(m => m.Estado == "Libre"),
                    MesasOcupadas = await _context.Mesas.CountAsync(m => m.Estado == "Ocupada"),
                    OrdenesPendientes = await _context.Ordenes.CountAsync(o => o.Estado == "Pendiente"),
                    OrdenesEnPreparacion = await _context.Ordenes.CountAsync(o => o.Estado == "EnPreparacion"),
                    OrdenesListas = await _context.Ordenes.CountAsync(o => o.Estado == "Lista"),
                    VentasDelDia = await _context.Facturas
                        .Where(f => f.FechaFactura >= hoy && f.Estado == "Pagada")
                        .SumAsync(f => f.Total),
                    FacturasDelDia = await _context.Facturas
                        .CountAsync(f => f.FechaFactura >= hoy && f.Estado == "Pagada")
                };

                return metricas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas en tiempo real");
                throw;
            }
        }

        /// <summary>
        /// Obtiene resumen ejecutivo del restaurante
        /// </summary>
        public async Task<object> GetResumenEjecutivoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var totalVentas = await _context.Facturas
                    .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin && f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                var totalOrdenes = await _context.Ordenes
                    .CountAsync(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin);

                var ticketPromedio = totalOrdenes > 0 ? totalVentas / totalOrdenes : 0;

                var clientesUnicos = await _context.Ordenes
                    .Where(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion <= fechaFin && o.ClienteID != null)
                    .Select(o => o.ClienteID)
                    .Distinct()
                    .CountAsync();

                var diasEnPeriodo = (fechaFin - fechaInicio).TotalDays + 1;
                var ventasPromedioDiarias = totalVentas / (decimal)diasEnPeriodo;

                var resumen = new
                {
                    PeriodoAnalisis = new
                    {
                        FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                        FechaFin = fechaFin.ToString("yyyy-MM-dd"),
                        DiasAnalizados = (int)diasEnPeriodo
                    },
                    KPIsPrincipales = new
                    {
                        TotalVentas = Math.Round(totalVentas, 2),
                        TotalOrdenes = totalOrdenes,
                        TicketPromedio = Math.Round(ticketPromedio, 2),
                        ClientesUnicos = clientesUnicos,
                        VentasPromedioDiarias = Math.Round(ventasPromedioDiarias, 2)
                    },
                    TendenciaVentas = await GetTendenciasVentasAsync(fechaInicio, fechaFin, "mes_anterior")
                };

                return resumen;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar resumen ejecutivo");
                throw;
            }
        }

        // ============================================================================
        // REPORTES DE VENTAS
        // ============================================================================

        /// <summary>
        /// Obtiene reporte detallado de ventas por período
        /// </summary>
        public async Task<object> GetReporteVentasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "dia")
        {
            try
            {
                var facturas = await _context.Facturas
                    .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin && f.Estado == "Pagada")
                    .ToListAsync();

                var ventasAgrupadas = agruparPor.ToLower() switch
                {
                    "semana" => facturas
                        .GroupBy(f => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(f.FechaFactura, CalendarWeekRule.FirstDay, DayOfWeek.Sunday))
                        .Select(g => new
                        {
                            Periodo = $"Semana {g.Key}",
                            CantidadFacturas = g.Count(),
                            TotalVentas = g.Sum(f => f.Total),
                            VentasPromedio = g.Average(f => f.Total)
                        }).ToList(),
                    "mes" => facturas
                        .GroupBy(f => new { f.FechaFactura.Year, f.FechaFactura.Month })
                        .Select(g => new
                        {
                            Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                            CantidadFacturas = g.Count(),
                            TotalVentas = g.Sum(f => f.Total),
                            VentasPromedio = g.Average(f => f.Total)
                        }).ToList(),
                    _ => facturas
                        .GroupBy(f => f.FechaFactura.Date)
                        .Select(g => new
                        {
                            Periodo = g.Key.ToString("yyyy-MM-dd"),
                            CantidadFacturas = g.Count(),
                            TotalVentas = g.Sum(f => f.Total),
                            VentasPromedio = g.Average(f => f.Total)
                        }).ToList()
                };

                var reporte = new
                {
                    PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                    TipoAgrupacion = agruparPor,
                    ResumenGeneral = new
                    {
                        TotalVentas = Math.Round(facturas.Sum(f => f.Total), 2),
                        CantidadFacturas = facturas.Count,
                        TicketPromedio = facturas.Any() ? Math.Round(facturas.Average(f => f.Total), 2) : 0
                    },
                    VentasDetalladas = ventasAgrupadas
                };

                return reporte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ventas por período");
                throw;
            }
        }

        /// <summary>
        /// Obtiene tendencias de ventas comparando períodos
        /// </summary>
        public async Task<object> GetTendenciasVentasAsync(DateTime fechaInicio, DateTime fechaFin, string compararCon = "mes_anterior")
        {
            try
            {
                var diasPeriodo = (int)(fechaFin - fechaInicio).TotalDays + 1;
                
                // Calcular período de comparación
                var (fechaInicioComparacion, fechaFinComparacion) = compararCon.ToLower() switch
                {
                    "año_anterior" => (fechaInicio.AddYears(-1), fechaFin.AddYears(-1)),
                    _ => (fechaInicio.AddDays(-diasPeriodo), fechaInicio.AddDays(-1))
                };

                var ventasActuales = await _context.Facturas
                    .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin && f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                var ventasComparacion = await _context.Facturas
                    .Where(f => f.FechaFactura >= fechaInicioComparacion && f.FechaFactura <= fechaFinComparacion && f.Estado == "Pagada")
                    .SumAsync(f => f.Total);

                var diferencia = ventasActuales - ventasComparacion;
                var porcentajeCambio = ventasComparacion > 0 ? (diferencia / ventasComparacion) * 100 : 0;

                var tendencia = new
                {
                    PeriodoActual = new
                    {
                        Inicio = fechaInicio.ToString("yyyy-MM-dd"),
                        Fin = fechaFin.ToString("yyyy-MM-dd"),
                        Ventas = Math.Round(ventasActuales, 2)
                    },
                    PeriodoComparacion = new
                    {
                        Inicio = fechaInicioComparacion.ToString("yyyy-MM-dd"),
                        Fin = fechaFinComparacion.ToString("yyyy-MM-dd"),
                        Ventas = Math.Round(ventasComparacion, 2)
                    },
                    Comparacion = new
                    {
                        DiferenciaAbsoluta = Math.Round(diferencia, 2),
                        PorcentajeCambio = Math.Round(porcentajeCambio, 2),
                        Tendencia = porcentajeCambio > 0 ? "Positiva" : porcentajeCambio < 0 ? "Negativa" : "Estable"
                    }
                };

                return tendencia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular tendencias de ventas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene análisis de ventas por hora del día
        /// </summary>
        public async Task<object> GetVentasPorHoraAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasPorHora = await _context.Facturas
                    .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin && f.Estado == "Pagada")
                    .GroupBy(f => f.FechaFactura.Hour)
                    .Select(g => new
                    {
                        Hora = g.Key,
                        CantidadFacturas = g.Count(),
                        TotalVentas = g.Sum(f => f.Total),
                        VentasPromedio = g.Average(f => f.Total)
                    })
                    .OrderBy(v => v.Hora)
                    .ToListAsync();

                // Rellenar horas sin ventas
                var todasLasHoras = Enumerable.Range(0, 24)
                    .Select(hora => new
                    {
                        Hora = hora,
                        HoraFormateada = $"{hora:D2}:00",
                        CantidadFacturas = ventasPorHora.FirstOrDefault(v => v.Hora == hora)?.CantidadFacturas ?? 0,
                        TotalVentas = ventasPorHora.FirstOrDefault(v => v.Hora == hora)?.TotalVentas ?? 0,
                        VentasPromedio = ventasPorHora.FirstOrDefault(v => v.Hora == hora)?.VentasPromedio ?? 0
                    })
                    .ToList();

                var horasPopulares = todasLasHoras
                    .Where(h => h.CantidadFacturas > 0)
                    .OrderByDescending(h => h.TotalVentas)
                    .Take(5)
                    .ToList();

                return new
                {
                    PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                    VentasPorHora = todasLasHoras,
                    HorasPopulares = horasPopulares,
                    HoraPico = horasPopulares.FirstOrDefault()?.HoraFormateada ?? "N/A"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar ventas por hora");
                throw;
            }
        }

        /// <summary>
        /// Obtiene análisis de ventas por día de la semana
        /// </summary>
        public async Task<object> GetVentasPorDiaSemanlaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasPorDia = await _context.Facturas
                    .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin && f.Estado == "Pagada")
                    .GroupBy(f => f.FechaFactura.DayOfWeek)
                    .Select(g => new
                    {
                        DiaSemana = g.Key,
                        CantidadFacturas = g.Count(),
                        TotalVentas = g.Sum(f => f.Total),
                        VentasPromedio = g.Average(f => f.Total)
                    })
                    .ToListAsync();

                var diasFormateados = Enum.GetValues<DayOfWeek>()
                    .Select(dia => new
                    {
                        DiaSemana = dia,
                        NombreDia = dia.ToString(),
                        CantidadFacturas = ventasPorDia.FirstOrDefault(v => v.DiaSemana == dia)?.CantidadFacturas ?? 0,
                        TotalVentas = ventasPorDia.FirstOrDefault(v => v.DiaSemana == dia)?.TotalVentas ?? 0,
                        VentasPromedio = ventasPorDia.FirstOrDefault(v => v.DiaSemana == dia)?.VentasPromedio ?? 0
                    })
                    .OrderBy(d => (int)d.DiaSemana)
                    .ToList();

                var mejorDia = diasFormateados.OrderByDescending(d => d.TotalVentas).FirstOrDefault();

                return new
                {
                    PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                    VentasPorDia = diasFormateados,
                    MejorDia = mejorDia?.NombreDia ?? "N/A"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar ventas por día de semana");
                throw;
            }
        }

        // ============================================================================
        // REPORTES DE PRODUCTOS
        // ============================================================================

        /// <summary>
        /// Obtiene análisis completo de productos más vendidos
        /// </summary>
        public async Task<object> GetAnalisisProductosTopAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 20)
        {
            try
            {
                var productosTop = await _context.DetalleOrdenes
                    .Include(d => d.Producto)
                        .ThenInclude(p => p!.Categoria)
                    .Include(d => d.Orden)
                    .Where(d => d.Orden.FechaCreacion >= fechaInicio && 
                               d.Orden.FechaCreacion <= fechaFin &&
                               d.ProductoID != null &&
                               d.Orden.Estado != "Cancelada")
                    .GroupBy(d => new { 
                        d.ProductoID, 
                        d.Producto!.Nombre, 
                        CategoriaNombre = d.Producto.Categoria.Nombre,
                        d.Producto.Precio
                    })
                    .Select(g => new
                    {
                        ProductoId = g.Key.ProductoID,
                        NombreProducto = g.Key.Nombre,
                        Categoria = g.Key.CategoriaNombre,
                        PrecioUnitario = g.Key.Precio,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal),
                        VecesOrdenado = g.Count(),
                        EsDominicano = _productosDominicanos.Contains(g.Key.Nombre)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(limite)
                    .ToListAsync();

                var totalProductosDominicanos = productosTop.Count(p => p.EsDominicano);
                var ventasProductosDominicanos = productosTop.Where(p => p.EsDominicano).Sum(p => p.TotalVentas);

                return new
                {
                    PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                    ProductosTop = productosTop,
                    EstadisticasGenerales = new
                    {
                        TotalProductosAnalizados = productosTop.Count,
                        ProductosDominicanos = totalProductosDominicanos,
                        PorcentajeProductosDominicanos = productosTop.Count > 0 ? Math.Round((double)totalProductosDominicanos / productosTop.Count * 100, 2) : 0,
                        VentasProductosDominicanos = Math.Round(ventasProductosDominicanos, 2)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar productos top");
                throw;
            }
        }

        /// <summary>
        /// Obtiene análisis de ventas por categoría de producto
        /// </summary>
        public async Task<object> GetVentasPorCategoriaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasPorCategoria = await _context.DetalleOrdenes
                    .Include(d => d.Producto)
                        .ThenInclude(p => p!.Categoria)
                    .Include(d => d.Orden)
                    .Where(d => d.Orden.FechaCreacion >= fechaInicio && 
                               d.Orden.FechaCreacion <= fechaFin &&
                               d.ProductoID != null &&
                               d.Orden.Estado != "Cancelada")
                    .GroupBy(d => d.Producto!.Categoria.Nombre)
                    .Select(g => new
                    {
                        Categoria = g.Key,
                        CantidadProductos = g.Select(d => d.ProductoID).Distinct().Count(),
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal),
                        VentasPromedio = g.Average(d => d.Subtotal)
                    })
                    .OrderByDescending(c => c.TotalVentas)
                    .ToListAsync();

                var totalVentas = ventasPorCategoria.Sum(c => c.TotalVentas);

                var categoriaConPorcentajes = ventasPorCategoria.Select(c => new
                {
                    c.Categoria,
                    c.CantidadProductos,
                    c.CantidadVendida,
                    TotalVentas = Math.Round(c.TotalVentas, 2),
                    VentasPromedio = Math.Round(c.VentasPromedio, 2),
                    PorcentajeVentas = totalVentas > 0 ? Math.Round((double)(c.TotalVentas / totalVentas) * 100, 2) : 0
                }).ToList();

                return new
                {
                    PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                    VentasPorCategoria = categoriaConPorcentajes,
                    CategoriaLider = categoriaConPorcentajes.FirstOrDefault()?.Categoria ?? "N/A",
                    TotalVentasGeneral = Math.Round(totalVentas, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar ventas por categoría");
                throw;
            }
        }

        // ============================================================================
        // ANÁLISIS ESPECIALES DOMINICANOS
        // ============================================================================

        /// <summary>
        /// Obtiene análisis de ventas de comida típica dominicana
        /// </summary>
        public async Task<object> GetAnalisisComidaDominicanaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var ventasComidaDominicana = await _context.DetalleOrdenes
                    .Include(d => d.Producto)
                        .ThenInclude(p => p!.Categoria)
                    .Include(d => d.Orden)
                    .Where(d => d.Orden.FechaCreacion >= fechaInicio && 
                               d.Orden.FechaCreacion <= fechaFin &&
                               d.ProductoID != null &&
                               _productosDominicanos.Contains(d.Producto!.Nombre) &&
                               d.Orden.Estado != "Cancelada")
                    .GroupBy(d => new { 
                        d.Producto!.Nombre, 
                        CategoriaNombre = d.Producto.Categoria.Nombre 
                    })
                    .Select(g => new
                    {
                        Producto = g.Key.Nombre,
                        Categoria = g.Key.CategoriaNombre,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal),
                        VecesOrdenado = g.Count()
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .ToListAsync();

                // Clasificar por tipo de comida dominicana
                var desayunosDominicanos = ventasComidaDominicana
                    .Where(p => new[] { "Mangú", "Tres Golpes", "Huevos Rancheros" }.Contains(p.Producto))
                    .ToList();

                var almuerzosDominicanos = ventasComidaDominicana
                    .Where(p => new[] { "Pollo Guisado", "Res Guisada", "Arroz Blanco", "Habichuelas Rojas", "Moro de Guandules" }.Contains(p.Producto))
                    .ToList();

                var bebidasDominicanas = ventasComidaDominicana
                    .Where(p => new[] { "Morir Soñando", "Jugo de Chinola", "Mamajuana" }.Contains(p.Producto))
                    .ToList();

                var postresDominicanos = ventasComidaDominicana
                    .Where(p => new[] { "Tres Leches", "Flan de Coco", "Majarete" }.Contains(p.Producto))
                    .ToList();

                var totalVentasDominicanas = ventasComidaDominicana.Sum(p => p.TotalVentas);
                var totalVentasGenerales = await _context.DetalleOrdenes
                    .Include(d => d.Orden)
                    .Where(d => d.Orden.FechaCreacion >= fechaInicio && 
                               d.Orden.FechaCreacion <= fechaFin &&
                               d.Orden.Estado != "Cancelada")
                    .SumAsync(d => d.Subtotal);

                return new
                {
                    PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                    ResumenGeneral = new
                    {
                        TotalVentasDominicanas = Math.Round(totalVentasDominicanas, 2),
                        PorcentajeVentasDominicanas = totalVentasGenerales > 0 ? Math.Round((double)(totalVentasDominicanas / totalVentasGenerales) * 100, 2) : 0,
                        ProductosDominicanosVendidos = ventasComidaDominicana.Count
                    },
                    ProductosTopDominicanos = ventasComidaDominicana.Take(10),
                    PorTipoComida = new
                    {
                        Desayunos = desayunosDominicanos,
                        Almuerzos = almuerzosDominicanos,
                        Bebidas = bebidasDominicanas,
                        Postres = postresDominicanos
                    },
                    AutenticidadCultural = new
                    {
                        ProductosAutenticos = ventasComidaDominicana.Count,
                        MasPopular = ventasComidaDominicana.FirstOrDefault()?.Producto ?? "N/A",
                        TradicionMasVendida = almuerzosDominicanos.Any() ? "Almuerzo típico dominicano" : "Desayuno típico dominicano"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar comida dominicana");
                throw;
            }
        }

        /// <summary>
        /// Obtiene alertas y notificaciones importantes
        /// </summary>
        public async Task<object> GetAlertasSistemaAsync()
        {
            try
            {
                var alertas = new List<object>();

                // Productos con stock bajo
                var productosStockBajo = await _context.Productos
                    .Include(p => p.Inventario)
                    .Where(p => p.Estado && 
                               p.Inventario != null && 
                               p.Inventario.CantidadDisponible <= p.Inventario.CantidadMinima)
                    .CountAsync();

                if (productosStockBajo > 0)
                {
                    alertas.Add(new
                    {
                        Tipo = "Inventario",
                        Nivel = "Warning",
                        Mensaje = $"{productosStockBajo} productos con stock bajo",
                        Accion = "Revisar inventario"
                    });
                }

                // Órdenes con tiempo excedido
                var ordenesRetrasadas = await _context.Ordenes
                    .Where(o => (o.Estado == "Pendiente" || o.Estado == "EnPreparacion") &&
                               o.FechaCreacion <= DateTime.UtcNow.AddMinutes(-30))
                    .CountAsync();

                if (ordenesRetrasadas > 0)
                {
                    alertas.Add(new
                    {
                        Tipo = "Operaciones",
                        Nivel = "Error",
                        Mensaje = $"{ordenesRetrasadas} órdenes con tiempo excedido",
                        Accion = "Revisar cocina"
                    });
                }

                // Mesas que necesitan limpieza
                var mesasLimpieza = await _context.Mesas
                    .Where(m => m.FechaUltimaLimpieza == null || 
                               m.FechaUltimaLimpieza < DateTime.UtcNow.AddHours(-4))
                    .CountAsync();

                if (mesasLimpieza > 0)
                {
                    alertas.Add(new
                    {
                        Tipo = "Limpieza",
                        Nivel = "Info",
                        Mensaje = $"{mesasLimpieza} mesas necesitan limpieza",
                        Accion = "Programar limpieza"
                    });
                }

                // Reservaciones próximas
                var reservacionesProximas = await _context.Reservaciones
                    .Where(r => r.Estado == "Confirmada" &&
                               r.FechaYHora >= DateTime.UtcNow &&
                               r.FechaYHora <= DateTime.UtcNow.AddHours(1))
                    .CountAsync();

                if (reservacionesProximas > 0)
                {
                    alertas.Add(new
                    {
                        Tipo = "Reservaciones",
                        Nivel = "Info",
                        Mensaje = $"{reservacionesProximas} reservaciones en la próxima hora",
                        Accion = "Preparar mesas"
                    });
                }

                return new
                {
                    FechaActualizacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalAlertas = alertas.Count,
                    Alertas = alertas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas del sistema");
                throw;
            }
        }

        // ============================================================================
        // MÉTODOS AUXILIARES Y CONFIGURACIÓN
        // ============================================================================

        /// <summary>
        /// Obtiene configuración disponible de reportes
        /// </summary>
        public async Task<object> GetConfiguracionReportesAsync()
        {
            try
            {
                await Task.Delay(1); // Para mantener la signatura async

                var configuracion = new
                {
                    TiposReporte = new[]
                    {
                        "Dashboard Principal",
                        "Ventas por Período",
                        "Productos Más Vendidos",
                        "Análisis de Comida Dominicana",
                        "Reportes Financieros",
                        "Ocupación de Mesas",
                        "Performance de Empleados"
                    },
                    PeriodosDisponibles = new[]
                    {
                        "Hoy",
                        "Ayer",
                        "Esta Semana",
                        "Semana Pasada",
                        "Este Mes",
                        "Mes Pasado",
                        "Personalizado"
                    },
                    FormatosExportacion = new[]
                    {
                        "JSON",
                        "CSV",
                        "PDF"
                    },
                    AgrupacionesDisponibles = new[]
                    {
                        "Día",
                        "Semana",
                        "Mes"
                    }
                };

                return configuracion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de reportes");
                throw;
            }
        }

        /// <summary>
        /// Genera reporte completo en formato exportable
        /// </summary>
        public async Task<object> GenerarReporteCompletoAsync(string tipoReporte, DateTime fechaInicio, DateTime fechaFin, string formato = "json")
        {
            try
            {
                object datosReporte = tipoReporte.ToLower() switch
                {
                    "dashboard" => await GetDashboardPrincipalAsync(),
                    "ventas" => await GetReporteVentasPorPeriodoAsync(fechaInicio, fechaFin),
                    "productos" => await GetAnalisisProductosTopAsync(fechaInicio, fechaFin),
                    "dominicano" => await GetAnalisisComidaDominicanaAsync(fechaInicio, fechaFin),
                    _ => await GetResumenEjecutivoAsync(fechaInicio, fechaFin)
                };

                var reporteCompleto = new
                {
                    MetadatosReporte = new
                    {
                        TipoReporte = tipoReporte,
                        FechaGeneracion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        PeriodoAnalisis = $"{fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}",
                        Formato = formato.ToUpper(),
                        GeneradoPor = "Sistema El Criollo",
                        Version = "1.0"
                    },
                    DatosReporte = datosReporte,
                    ConfiguracionUtilizada = await GetConfiguracionReportesAsync()
                };

                return reporteCompleto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte completo: {TipoReporte}", tipoReporte);
                throw;
            }
        }

        // Implementaciones restantes de la interfaz con versiones básicas
        public Task<object> GetProductosBajoRendimientoAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 10) => throw new NotImplementedException();
        public Task<object> GetRentabilidadProductosAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetReporteEficienciaAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetAnalisisOcupacionMesasAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetTiemposServicioAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetAnalisisReservacionesAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetPerformanceEmpleadosAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetVentasPorEmpleadoAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetPropinasPorEmpleadoAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetComportamientoClientesAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetTopClientesAsync(int limite = 20, int dias = 90) => throw new NotImplementedException();
        public Task<object> GetTicketPromedioClientesAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetEstadoResultadosAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetAnalisisMargenesAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetFlujoCajaAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetProyeccionVentasAsync(int diasProyeccion = 30) => throw new NotImplementedException();
        public Task<object> GetRotacionInventarioAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetAlertasInventarioAsync() => throw new NotImplementedException();
        public Task<object> GetAnalisisDesperdicioAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetReporteEventosEspecialesAsync(int año) => throw new NotImplementedException();
        public Task<object> GetAnalisisRegionalAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
        public Task<object> GetPatronesDemandaAsync(int diasHistorico = 90) => throw new NotImplementedException();
        public Task<object> GetRecomendacionesNegocioAsync(DateTime fechaInicio, DateTime fechaFin) => throw new NotImplementedException();
    }
}