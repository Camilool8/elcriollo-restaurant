namespace ElCriollo.API.Models.ViewModels;

/// <summary>
/// ViewModel para el dashboard principal del restaurante
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// Información general del día
    /// </summary>
    public ResumenDiarioViewModel ResumenDiario { get; set; } = new ResumenDiarioViewModel();

    /// <summary>
    /// Estado actual de las mesas
    /// </summary>
    public EstadoMesasViewModel EstadoMesas { get; set; } = new EstadoMesasViewModel();

    /// <summary>
    /// Órdenes activas
    /// </summary>
    public OrdenesActivasViewModel OrdenesActivas { get; set; } = new OrdenesActivasViewModel();

    /// <summary>
    /// Alertas e indicadores importantes
    /// </summary>
    public AlertasViewModel Alertas { get; set; } = new AlertasViewModel();

    /// <summary>
    /// Productos más vendidos hoy
    /// </summary>
    public List<ProductoPopularViewModel> ProductosPopulares { get; set; } = new List<ProductoPopularViewModel>();

    /// <summary>
    /// Reservaciones próximas
    /// </summary>
    public List<ReservacionProximaViewModel> ReservacionesProximas { get; set; } = new List<ReservacionProximaViewModel>();

    /// <summary>
    /// Empleados conectados
    /// </summary>
    public List<EmpleadoActivoViewModel> EmpleadosActivos { get; set; } = new List<EmpleadoActivoViewModel>();
}

/// <summary>
/// Resumen de ventas y actividad del día
/// </summary>
public class ResumenDiarioViewModel
{
    /// <summary>
    /// Fecha del resumen
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Now.Date;

    /// <summary>
    /// Total de ventas del día
    /// </summary>
    public string VentasDelDia { get; set; } = "RD$ 0.00";

    /// <summary>
    /// Número de órdenes completadas
    /// </summary>
    public int OrdenesCompletadas { get; set; }

    /// <summary>
    /// Número de clientes atendidos
    /// </summary>
    public int ClientesAtendidos { get; set; }

    /// <summary>
    /// Promedio por orden
    /// </summary>
    public string PromedioOrden { get; set; } = "RD$ 0.00";

    /// <summary>
    /// Comparación con ayer (% de cambio)
    /// </summary>
    public decimal CambioRespectoAyer { get; set; }

    /// <summary>
    /// Meta diaria de ventas
    /// </summary>
    public string MetaDiaria { get; set; } = "RD$ 15,000.00";

    /// <summary>
    /// Porcentaje de cumplimiento de meta
    /// </summary>
    public decimal PorcentajeMeta { get; set; }
}

/// <summary>
/// Estado actual de todas las mesas
/// </summary>
public class EstadoMesasViewModel
{
    /// <summary>
    /// Total de mesas
    /// </summary>
    public int TotalMesas { get; set; }

    /// <summary>
    /// Mesas libres
    /// </summary>
    public int MesasLibres { get; set; }

    /// <summary>
    /// Mesas ocupadas
    /// </summary>
    public int MesasOcupadas { get; set; }

    /// <summary>
    /// Mesas reservadas
    /// </summary>
    public int MesasReservadas { get; set; }

    /// <summary>
    /// Mesas en mantenimiento
    /// </summary>
    public int MesasMantenimiento { get; set; }

    /// <summary>
    /// Porcentaje de ocupación
    /// </summary>
    public decimal PorcentajeOcupacion { get; set; }

    /// <summary>
    /// Tiempo promedio de ocupación
    /// </summary>
    public string TiempoPromedioOcupacion { get; set; } = "0h 0m";
}

/// <summary>
/// Información de órdenes activas
/// </summary>
public class OrdenesActivasViewModel
{
    /// <summary>
    /// Total de órdenes activas
    /// </summary>
    public int TotalOrdenes { get; set; }

    /// <summary>
    /// Órdenes pendientes
    /// </summary>
    public int OrdenesPendientes { get; set; }

    /// <summary>
    /// Órdenes en preparación
    /// </summary>
    public int OrdenesEnPreparacion { get; set; }

    /// <summary>
    /// Órdenes listas para entregar
    /// </summary>
    public int OrdenesListas { get; set; }

    /// <summary>
    /// Órdenes retrasadas
    /// </summary>
    public int OrdenesRetrasadas { get; set; }

    /// <summary>
    /// Tiempo promedio de preparación
    /// </summary>
    public string TiempoPromedioPreparacion { get; set; } = "0 min";
}

/// <summary>
/// Alertas e indicadores del sistema
/// </summary>
public class AlertasViewModel
{
    /// <summary>
    /// Productos con stock bajo
    /// </summary>
    public int ProductosStockBajo { get; set; }

    /// <summary>
    /// Productos agotados
    /// </summary>
    public int ProductosAgotados { get; set; }

    /// <summary>
    /// Mesas que necesitan limpieza
    /// </summary>
    public int MesasParaLimpieza { get; set; }

    /// <summary>
    /// Reservaciones pendientes de confirmación
    /// </summary>
    public int ReservacionesPendientes { get; set; }

    /// <summary>
    /// Órdenes críticas (muy retrasadas)
    /// </summary>
    public int OrdenesCriticas { get; set; }

    /// <summary>
    /// Emails pendientes de envío
    /// </summary>
    public int EmailsPendientes { get; set; }

    /// <summary>
    /// Lista de alertas específicas
    /// </summary>
    public List<string> AlertasEspecificas { get; set; } = new List<string>();
}

/// <summary>
/// Producto popular del día
/// </summary>
public class ProductoPopularViewModel
{
    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Categoría
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad vendida
    /// </summary>
    public int CantidadVendida { get; set; }

    /// <summary>
    /// Ingresos generados
    /// </summary>
    public string Ingresos { get; set; } = "RD$ 0.00";

    /// <summary>
    /// Posición en ranking
    /// </summary>
    public int Posicion { get; set; }
}

/// <summary>
/// Reservación próxima
/// </summary>
public class ReservacionProximaViewModel
{
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string Cliente { get; set; } = string.Empty;

    /// <summary>
    /// Número de mesa
    /// </summary>
    public int NumeroMesa { get; set; }

    /// <summary>
    /// Cantidad de personas
    /// </summary>
    public int CantidadPersonas { get; set; }

    /// <summary>
    /// Hora de la reservación
    /// </summary>
    public string Hora { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo hasta la reservación
    /// </summary>
    public string TiempoHasta { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la reservación
    /// </summary>
    public string Estado { get; set; } = string.Empty;
}

/// <summary>
/// Empleado activo en el sistema
/// </summary>
public class EmpleadoActivoViewModel
{
    /// <summary>
    /// Nombre del empleado
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Rol del empleado
    /// </summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Último acceso
    /// </summary>
    public string UltimoAcceso { get; set; } = string.Empty;

    /// <summary>
    /// Órdenes atendidas hoy
    /// </summary>
    public int OrdenesAtendidas { get; set; }

    /// <summary>
    /// Estado de conexión
    /// </summary>
    public string EstadoConexion { get; set; } = "En línea";
}