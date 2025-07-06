namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta para mesas del restaurante
/// </summary>
public class MesaResponse
{
    /// <summary>
    /// ID de la mesa
    /// </summary>
    public int MesaID { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public int NumeroMesa { get; set; }

    /// <summary>
    /// Capacidad de personas
    /// </summary>
    public int Capacidad { get; set; }

    /// <summary>
    /// Ubicación en el restaurante
    /// </summary>
    public string? Ubicacion { get; set; }

    /// <summary>
    /// Estado actual de la mesa
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Descripción completa de la mesa
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Cliente actual (si está ocupada)
    /// </summary>
    public ClienteBasicoResponse? ClienteActual { get; set; }

    /// <summary>
    /// Orden actual (si está ocupada)
    /// </summary>
    public OrdenBasicaResponse? OrdenActual { get; set; }

    /// <summary>
    /// Reservación actual (si está reservada)
    /// </summary>
    public ReservacionBasicaResponse? ReservacionActual { get; set; }

    /// <summary>
    /// Tiempo ocupada (si aplica)
    /// </summary>
    public string? TiempoOcupada { get; set; }

    /// <summary>
    /// Indica si necesita limpieza
    /// </summary>
    public bool NecesitaLimpieza { get; set; }

    /// <summary>
    /// Última limpieza
    /// </summary>
    public DateTime? FechaUltimaLimpieza { get; set; }
}

/// <summary>
/// DTO de respuesta básica para clientes
/// </summary>
public class ClienteBasicoResponse
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public int ClienteID { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Categoría del cliente
    /// </summary>
    public string CategoriaCliente { get; set; } = string.Empty;
}

/// <summary>
/// DTO de respuesta básica para órdenes
/// </summary>
public class OrdenBasicaResponse
{
    /// <summary>
    /// ID de la orden
    /// </summary>
    public int OrdenID { get; set; }

    /// <summary>
    /// Número de orden
    /// </summary>
    public string NumeroOrden { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la orden
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Total de la orden
    /// </summary>
    public string Total { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Tiempo transcurrido
    /// </summary>
    public string TiempoTranscurrido { get; set; } = string.Empty;
}

/// <summary>
/// DTO de respuesta básica para reservaciones
/// </summary>
public class ReservacionBasicaResponse
{
    /// <summary>
    /// ID de la reservación
    /// </summary>
    public int ReservacionID { get; set; }

    /// <summary>
    /// Cliente que reservó
    /// </summary>
    public string ClienteNombre { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de personas
    /// </summary>
    public int CantidadPersonas { get; set; }

    /// <summary>
    /// Horario formateado
    /// </summary>
    public string Horario { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la reservación
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo restante hasta la reservación
    /// </summary>
    public string? TiempoHastaReservacion { get; set; }
}