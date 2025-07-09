using ElCriollo.API.Models.DTOs.Common;

namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para órdenes
/// </summary>
public class OrdenResponse
{
    /// <summary>
    /// ID de la orden
    /// </summary>
    public int OrdenID { get; set; }

    /// <summary>
    /// Número de orden único
    /// </summary>
    public string NumeroOrden { get; set; } = string.Empty;

    /// <summary>
    /// Mesa asignada
    /// </summary>
    public MesaBasicaResponse? Mesa { get; set; }

    /// <summary>
    /// Cliente de la orden
    /// </summary>
    public ClienteBasicoResponse? Cliente { get; set; }

    /// <summary>
    /// Empleado que tomó la orden
    /// </summary>
    public EmpleadoBasicoResponse Empleado { get; set; } = null!;

    /// <summary>
    /// Fecha y hora de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Estado actual de la orden
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de orden
    /// </summary>
    public string TipoOrden { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones de la orden
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Detalles de la orden
    /// </summary>
    public List<DetalleOrdenResponse> Detalles { get; set; } = new List<DetalleOrdenResponse>();

    /// <summary>
    /// Total de items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Subtotal formateado
    /// </summary>
    public string Subtotal { get; set; } = string.Empty;

    /// <summary>
    /// Total formateado
    /// </summary>
    public string Total { get; set; } = string.Empty;

    /// <summary>
    /// Subtotal calculado (valor numérico)
    /// </summary>
    public decimal SubtotalCalculado { get; set; }

    /// <summary>
    /// Impuesto calculado (valor numérico)
    /// </summary>
    public decimal Impuesto { get; set; }

    /// <summary>
    /// Total calculado (valor numérico)
    /// </summary>
    public decimal TotalCalculado { get; set; }

    /// <summary>
    /// Tiempo transcurrido desde creación
    /// </summary>
    public string TiempoTranscurrido { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo estimado de preparación
    /// </summary>
    public string TiempoPreparacionEstimado { get; set; } = string.Empty;

    /// <summary>
    /// Hora estimada de finalización
    /// </summary>
    public DateTime HoraEstimadaFinalizacion { get; set; }

    /// <summary>
    /// Indica si está retrasada
    /// </summary>
    public bool EstaRetrasada { get; set; }

    /// <summary>
    /// Indica si está facturada
    /// </summary>
    public bool EstaFacturada { get; set; }

    /// <summary>
    /// Categorías de productos incluidas
    /// </summary>
    public List<string> CategoriasProductos { get; set; } = new List<string>();
}

/// <summary>
/// DTO de respuesta para detalles de orden
/// </summary>
public class DetalleOrdenResponse
{
    /// <summary>
    /// ID del detalle
    /// </summary>
    public int DetalleOrdenID { get; set; }

    /// <summary>
    /// Producto completo (para edición de órdenes)
    /// </summary>
    public ProductoResponse? Producto { get; set; }

    /// <summary>
    /// Tipo de item (Producto o Combo)
    /// </summary>
    public string TipoItem { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del item
    /// </summary>
    public string NombreItem { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del item
    /// </summary>
    public string? DescripcionItem { get; set; }

    /// <summary>
    /// Categoría (solo para productos)
    /// </summary>
    public string? CategoriaItem { get; set; }

    /// <summary>
    /// Cantidad ordenada
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario formateado
    /// </summary>
    public string PrecioUnitario { get; set; } = string.Empty;

    /// <summary>
    /// Descuento aplicado
    /// </summary>
    public string Descuento { get; set; } = string.Empty;

    /// <summary>
    /// Subtotal formateado
    /// </summary>
    public string Subtotal { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones del item
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Disponibilidad del item
    /// </summary>
    public bool EstaDisponible { get; set; }

    /// <summary>
    /// Tiempo de preparación
    /// </summary>
    public string TiempoPreparacion { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo con cantidad
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;
}

/// <summary>
/// DTO de respuesta básica para mesa
/// </summary>
public class MesaBasicaResponse
{
    /// <summary>
    /// ID de la mesa
    /// </summary>
    public int MesaID { get; set; }

    /// <summary>
    /// Número de mesa
    /// </summary>
    public int NumeroMesa { get; set; }

    /// <summary>
    /// Capacidad
    /// </summary>
    public int Capacidad { get; set; }

    /// <summary>
    /// Ubicación
    /// </summary>
    public string? Ubicacion { get; set; }

    /// <summary>
    /// Descripción completa
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;
}