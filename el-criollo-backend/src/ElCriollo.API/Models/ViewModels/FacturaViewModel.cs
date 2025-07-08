namespace ElCriollo.API.Models.ViewModels;

/// <summary>
/// ViewModel para el resumen de facturación del día
/// </summary>
public class ResumenFacturacionDiaViewModel
{
    /// <summary>
    /// Fecha del resumen
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Today;

    /// <summary>
    /// Total de facturas emitidas en el día
    /// </summary>
    public int TotalFacturas { get; set; }

    /// <summary>
    /// Cantidad de facturas pagadas
    /// </summary>
    public int FacturasPagadas { get; set; }

    /// <summary>
    /// Cantidad de facturas pendientes
    /// </summary>
    public int FacturasPendientes { get; set; }

    /// <summary>
    /// Cantidad de facturas anuladas
    /// </summary>
    public int FacturasAnuladas { get; set; }

    /// <summary>
    /// Total de ventas del día (solo facturas pagadas)
    /// </summary>
    public decimal TotalVentas { get; set; }

    /// <summary>
    /// Total de ventas formateado
    /// </summary>
    public string TotalVentasFormateado => $"RD$ {TotalVentas:N2}";

    /// <summary>
    /// Total de ITBIS recaudado
    /// </summary>
    public decimal TotalITBIS { get; set; }

    /// <summary>
    /// Total de ITBIS formateado
    /// </summary>
    public string TotalITBISFormateado => $"RD$ {TotalITBIS:N2}";

    /// <summary>
    /// Total de propinas recibidas
    /// </summary>
    public decimal TotalPropinas { get; set; }

    /// <summary>
    /// Total de propinas formateado
    /// </summary>
    public string TotalPropinaFormateado => $"RD$ {TotalPropinas:N2}";

    /// <summary>
    /// Promedio de venta por factura
    /// </summary>
    public decimal PromedioVentaPorFactura { get; set; }

    /// <summary>
    /// Promedio de venta formateado
    /// </summary>
    public string PromedioVentaFormateado => $"RD$ {PromedioVentaPorFactura:N2}";

    /// <summary>
    /// Porcentaje de facturas pagadas
    /// </summary>
    public decimal PorcentajeFacturasPagadas => TotalFacturas > 0 ? Math.Round((decimal)FacturasPagadas / TotalFacturas * 100, 2) : 0;

    /// <summary>
    /// Meta diaria de ventas (configurable)
    /// </summary>
    public decimal MetaDiariaVentas { get; set; } = 15000; // RD$ 15,000 por defecto

    /// <summary>
    /// Porcentaje de cumplimiento de meta
    /// </summary>
    public decimal PorcentajeCumplimientoMeta => MetaDiariaVentas > 0 ? Math.Round(TotalVentas / MetaDiariaVentas * 100, 2) : 0;

    /// <summary>
    /// Indica si se alcanzó la meta del día
    /// </summary>
    public bool MetaAlcanzada => TotalVentas >= MetaDiariaVentas;

    /// <summary>
    /// Desglose por métodos de pago
    /// </summary>
    public Dictionary<string, decimal> DesglosePorMetodoPago { get; set; } = new();

    /// <summary>
    /// Horario de mayor actividad
    /// </summary>
    public string? HorarioPicoVentas { get; set; }

    /// <summary>
    /// Comparación con el día anterior
    /// </summary>
    public decimal CambioRespectoAyer { get; set; }

    /// <summary>
    /// Fecha formateada para mostrar
    /// </summary>
    public string FechaFormateada => Fecha.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-DO"));
}

/// <summary>
/// ViewModel para estadísticas de facturación por período
/// </summary>
public class EstadisticasFacturacionViewModel
{
    /// <summary>
    /// Fecha de inicio del período
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin del período
    /// </summary>
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Total de facturas en el período
    /// </summary>
    public int TotalFacturas { get; set; }

    /// <summary>
    /// Total de ventas del período
    /// </summary>
    public decimal TotalVentas { get; set; }

    /// <summary>
    /// Total de ventas formateado
    /// </summary>
    public string TotalVentasFormateado => $"RD$ {TotalVentas:N2}";

    /// <summary>
    /// Total de ITBIS del período
    /// </summary>
    public decimal TotalITBIS { get; set; }

    /// <summary>
    /// Total de ITBIS formateado
    /// </summary>
    public string TotalITBISFormateado => $"RD$ {TotalITBIS:N2}";

    /// <summary>
    /// Venta promedio diaria
    /// </summary>
    public decimal VentaPromedioDiaria { get; set; }

    /// <summary>
    /// Venta promedio diaria formateada
    /// </summary>
    public string VentaPromedioDiariaFormateada => $"RD$ {VentaPromedioDiaria:N2}";

    /// <summary>
    /// Factura con mayor monto del período
    /// </summary>
    public decimal FacturaConMayorMonto { get; set; }

    /// <summary>
    /// Factura con mayor monto formateada
    /// </summary>
    public string FacturaMayorMontoFormateada => $"RD$ {FacturaConMayorMonto:N2}";

    /// <summary>
    /// Factura con menor monto del período
    /// </summary>
    public decimal FacturaConMenorMonto { get; set; }

    /// <summary>
    /// Factura con menor monto formateada
    /// </summary>
    public string FacturaMenorMontoFormateada => $"RD$ {FacturaConMenorMonto:N2}";

    /// <summary>
    /// Promedio de ticket por factura
    /// </summary>
    public decimal PromedioTicket => TotalFacturas > 0 ? TotalVentas / TotalFacturas : 0;

    /// <summary>
    /// Promedio de ticket formateado
    /// </summary>
    public string PromedioTicketFormateado => $"RD$ {PromedioTicket:N2}";

    /// <summary>
    /// Días incluidos en el período
    /// </summary>
    public int DiasEnPeriodo => (FechaFin - FechaInicio).Days + 1;

    /// <summary>
    /// Ventas por método de pago
    /// </summary>
    public Dictionary<string, decimal> VentasPorMetodoPago { get; set; } = new();

    /// <summary>
    /// Ventas por día de la semana
    /// </summary>
    public Dictionary<string, decimal> VentasPorDiaSemana { get; set; } = new();

    /// <summary>
    /// Tendencia de ventas (positiva/negativa)
    /// </summary>
    public string TendenciaVentas { get; set; } = "Estable";

    /// <summary>
    /// Período formateado para mostrar
    /// </summary>
    public string PeriodoFormateado => $"Del {FechaInicio:dd/MM/yyyy} al {FechaFin:dd/MM/yyyy}";

    /// <summary>
    /// Facturas por estado en el período
    /// </summary>
    public Dictionary<string, int> FacturasPorEstado { get; set; } = new();
}

/// <summary>
/// ViewModel para el análisis de rendimiento de facturación
/// </summary>
public class RendimientoFacturacionViewModel
{
    /// <summary>
    /// Período analizado
    /// </summary>
    public string Periodo { get; set; } = string.Empty;

    /// <summary>
    /// Hora pico de facturación
    /// </summary>
    public string HoraPicoFacturacion { get; set; } = string.Empty;

    /// <summary>
    /// Día de la semana con más ventas
    /// </summary>
    public string MejorDiaSemana { get; set; } = string.Empty;

    /// <summary>
    /// Método de pago más utilizado
    /// </summary>
    public string MetodoPagoMasUsado { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo promedio de facturación (desde orden hasta factura)
    /// </summary>
    public TimeSpan TiempoPromedioFacturacion { get; set; }

    /// <summary>
    /// Tiempo promedio formateado
    /// </summary>
    public string TiempoPromedioFormateado => $"{TiempoPromedioFacturacion.Hours}h {TiempoPromedioFacturacion.Minutes}m";

    /// <summary>
    /// Porcentaje de facturas pagadas inmediatamente
    /// </summary>
    public decimal PorcentajeFacturasInmediatas { get; set; }

    /// <summary>
    /// Valor promedio de propinas
    /// </summary>
    public decimal PromedioPropinasPorFactura { get; set; }

    /// <summary>
    /// Promedio de propinas formateado
    /// </summary>
    public string PromedioPropinaFormateado => $"RD$ {PromedioPropinasPorFactura:N2}";

    /// <summary>
    /// Porcentaje de descuentos aplicados
    /// </summary>
    public decimal PorcentajeFacturasConDescuento { get; set; }

    /// <summary>
    /// Recomendaciones para mejorar el rendimiento
    /// </summary>
    public List<string> Recomendaciones { get; set; } = new();
}