namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para clientes
/// </summary>
public class ClienteResponse
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public int ClienteID { get; set; }

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Dirección del cliente
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Fecha de registro del cliente
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Categoría del cliente (Nuevo, Frecuente, VIP)
    /// </summary>
    public string CategoriaCliente { get; set; } = string.Empty;

    /// <summary>
    /// Total de órdenes realizadas
    /// </summary>
    public int TotalOrdenes { get; set; }

    /// <summary>
    /// Total de reservaciones realizadas
    /// </summary>
    public int TotalReservaciones { get; set; }

    /// <summary>
    /// Total de facturas generadas
    /// </summary>
    public int TotalFacturas { get; set; }

    /// <summary>
    /// Promedio de consumo formateado
    /// </summary>
    public string PromedioConsumo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la última visita
    /// </summary>
    public DateTime? UltimaVisita { get; set; }

    /// <summary>
    /// Estado del cliente (activo/inactivo)
    /// </summary>
    public bool Estado { get; set; }
} 