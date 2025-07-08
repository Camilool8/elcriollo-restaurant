namespace ElCriollo.API.Models.DTOs.Response
{
    /// <summary>
    /// Respuesta para el historial de compras de un cliente
    /// </summary>
    public class ClienteHistorialComprasResponse
    {
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalCompras { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal TicketPromedio { get; set; }
        public List<CompraHistorialItem> Compras { get; set; } = new();
    }

    /// <summary>
    /// Respuesta para el historial de compras de un empleado
    /// </summary>
    public class EmpleadoHistorialComprasResponse
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalCompras { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal TicketPromedio { get; set; }
        public List<CompraHistorialItem> Compras { get; set; } = new();
    }

    /// <summary>
    /// Item individual del historial de compras
    /// </summary>
    public class CompraHistorialItem
    {
        public DateTime Fecha { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public List<string> ProductosComprados { get; set; } = new();
    }
} 