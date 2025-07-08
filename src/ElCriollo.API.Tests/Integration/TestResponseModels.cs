using ElCriollo.API.Models.DTOs.Request;

namespace ElCriollo.API.Tests.Integration
{
    /// <summary>
    /// Modelos de respuesta para las pruebas de integración
    /// </summary>

    public class UsuarioResponse
    {
        public int UsuarioId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public EmpleadoBasicoResponse? Empleado { get; set; }
    }

    public class EmpleadoBasicoResponse
    {
        public int EmpleadoID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string? Departamento { get; set; }
    }

    public class EmpleadoResponse
    {
        public int EmpleadoID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? PreferenciasComida { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal? Salario { get; set; }
        public string? Departamento { get; set; }
    }

    public class MesaResponse
    {
        public int MesaID { get; set; }
        public int NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public string? Ubicacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public ClienteBasicoResponse? ClienteActual { get; set; }
        public OrdenBasicaResponse? OrdenActual { get; set; }
        public ReservacionBasicaResponse? ReservacionActual { get; set; }
        public string? TiempoOcupada { get; set; }
        public bool NecesitaLimpieza { get; set; }
        public DateTime? FechaUltimaLimpieza { get; set; }
        public bool RequiereAtencion { get; set; }
        public string? TiempoHastaReserva { get; set; }
        
        // Alias para compatibilidad con tests existentes
        public int MesaId => MesaID;
    }

    public class OrdenBasicaResponse
    {
        public int OrdenID { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string TiempoTranscurrido { get; set; } = string.Empty;
        
        // Alias para compatibilidad con tests existentes
        public int OrdenId => OrdenID;
    }

    public class ReservacionBasicaResponse
    {
        public int ReservacionID { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int CantidadPersonas { get; set; }
        public string Horario { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? TiempoHastaReservacion { get; set; }
        
        // Alias para compatibilidad con tests existentes
        public int ReservacionId => ReservacionID;
        public DateTime FechaHora => DateTime.TryParse(Horario, out var fecha) ? fecha : DateTime.MinValue;
        public string NombreCliente => ClienteNombre;
    }

    public class InventarioResponse
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }
        public int CantidadMinima { get; set; }
        public string? UnidadMedida { get; set; }
        public decimal? CostoUnitario { get; set; }
        public DateTime? FechaUltimoMovimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class MovimientoInventarioResponse
    {
        public bool Success { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadMovimiento { get; set; }
        public int StockAnterior { get; set; }
        public int StockActual { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool RequiereReabastecimiento { get; set; }
    }

    public class ClienteResponse
    {
        public int ClienteID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string CategoriaCliente { get; set; } = string.Empty;
        public int TotalOrdenes { get; set; }
        public int TotalReservaciones { get; set; }
        public int TotalFacturas { get; set; }
        public string PromedioConsumo { get; set; } = string.Empty;
        public DateTime? UltimaVisita { get; set; }
        public bool Estado { get; set; }
        
        // Alias para compatibilidad con tests existentes
        public int ClienteId => ClienteID;
    }

    public class ClienteBasicoResponse
    {
        public int ClienteID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        
        // Alias para compatibilidad con tests existentes
        public int ClienteId => ClienteID;
    }

    public class MesaBasicaResponse
    {
        public int MesaID { get; set; }
        public int NumeroMesa { get; set; }
        public string? Ubicacion { get; set; }
        
        // Alias para compatibilidad con tests existentes
        public int MesaId => MesaID;
    }

    public class OrdenResponse
    {
        public int OrdenID { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public MesaBasicaResponse? Mesa { get; set; }
        public ClienteBasicoResponse? Cliente { get; set; }
        public EmpleadoBasicoResponse Empleado { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TipoOrden { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<DetalleOrdenResponse> Detalles { get; set; } = new();
        public int TotalItems { get; set; }
        public string Subtotal { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string TiempoTranscurrido { get; set; } = string.Empty;
        public string TiempoPreparacionEstimado { get; set; } = string.Empty;
        public DateTime HoraEstimadaFinalizacion { get; set; }
        public bool EstaRetrasada { get; set; }
        public bool EstaFacturada { get; set; }
        public List<string> CategoriasProductos { get; set; } = new();
        
        // Alias para compatibilidad con tests existentes
        public List<DetalleOrdenResponse> Items => Detalles;
        public int OrdenId => OrdenID;
        public int? MesaId => Mesa?.MesaID;
        public int? ClienteId => Cliente?.ClienteID;
    }

    public class DetalleOrdenResponse
    {
        public int DetalleOrdenID { get; set; }
        public string TipoItem { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public string? DescripcionItem { get; set; }
        public string? CategoriaItem { get; set; }
        public int Cantidad { get; set; }
        public string PrecioUnitario { get; set; } = string.Empty;
        public string Descuento { get; set; } = string.Empty;
        public string Subtotal { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public bool EstaDisponible { get; set; }
        public string TiempoPreparacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        
        // Alias para compatibilidad con tests existentes
        public int ItemId => DetalleOrdenID;
        public int ProductoId => 0; // Se puede obtener del contexto si es necesario
        public string NombreProducto => NombreItem;
        public string? NotasEspeciales => Observaciones;
        public string Estado => EstaDisponible ? "Disponible" : "No disponible";
    }

    public class ReservacionResponse
    {
        public int ReservacionID { get; set; }
        public ClienteBasicoResponse Cliente { get; set; } = null!;
        public MesaBasicaResponse Mesa { get; set; } = null!;
        public DateTime FechaYHora { get; set; }
        public int CantidadPersonas { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string Horario { get; set; } = string.Empty;
        public string? TiempoHastaReservacion { get; set; }
        public bool PuedeModificar { get; set; }
        public bool PuedeCancelar { get; set; }
        public int? TiempoParaLlegar { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int DuracionMinutos { get; set; } = 120;
        
        // Alias para compatibilidad con tests existentes
        public int ReservacionId => ReservacionID;
        public int MesaId => Mesa?.MesaID ?? 0;
        public int ClienteId => Cliente?.ClienteID ?? 0;
        public DateTime FechaHora => FechaYHora;
        public string? NotasEspeciales => Observaciones;
    }

    public class FacturaResponse
    {
        // Propiedades que coinciden exactamente con la clase real FacturaResponse
        public int FacturaID { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public int OrdenID { get; set; }
        public ClienteBasicoResponse Cliente { get; set; } = null!;
        public EmpleadoBasicoResponse Empleado { get; set; } = null!;
        public MesaBasicaResponse? Mesa { get; set; }
        public DateTime FechaFactura { get; set; }
        public string Subtotal { get; set; } = string.Empty;
        public string Impuesto { get; set; } = string.Empty; 
        public string Descuento { get; set; } = string.Empty; 
        public string Propina { get; set; } = string.Empty; 
        public string Total { get; set; } = string.Empty; 
        public string MetodoPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal PorcentajeImpuesto { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public decimal PorcentajePropina { get; set; }
        public string? ObservacionesPago { get; set; }
        public DateTime? FechaPago { get; set; }
        
        // Alias para compatibilidad con tests existentes
        public int FacturaId => FacturaID;
        public int OrdenId => OrdenID;
        public int? ClienteId => Cliente?.ClienteID;
        public DateTime FechaCreacion => FechaFactura;
        public string? Observaciones => ObservacionesPago;
        
        // Propiedades numéricas para cálculos (derivadas de los strings)
        public decimal SubtotalNumerico => ParseCurrency(Subtotal);
        public decimal ImpuestoNumerico => ParseCurrency(Impuesto);
        public decimal DescuentoNumerico => ParseCurrency(Descuento);
        public decimal PropinaNumerico => ParseCurrency(Propina);
        public decimal TotalNumerico => ParseCurrency(Total);
        
        private static decimal ParseCurrency(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            // Remover "RD$ " y cualquier formato de moneda
            var cleanValue = value.Replace("RD$ ", "").Replace("RD$", "").Replace(",", "").Trim();
            return decimal.TryParse(cleanValue, out var result) ? result : 0;
        }
    }

    public class ReporteVentasDiariasResponse
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalOrdenes { get; set; }
        public int TotalFacturas { get; set; }
        public decimal PromedioVenta { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal TotalPropinas { get; set; }
        public List<VentaDiariaDetalle> VentasPorDia { get; set; } = new();
        public List<ProductoMasVendido> ProductosMasVendidos { get; set; } = new();
        public List<MetodoPagoResumen> VentasPorMetodoPago { get; set; } = new();
    }

    public class VentaDiariaDetalle
    {
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    public class ProductoMasVendido
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class MetodoPagoResumen
    {
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class DashboardResponse
    {
        public decimal VentasHoy { get; set; }
        public decimal VentasAyer { get; set; }
        public decimal VentasMes { get; set; }
        public int OrdenesActivas { get; set; }
        public int OrdenesHoy { get; set; }
        public int MesasOcupadas { get; set; }
        public int MesasLibres { get; set; }
        public int ClientesUnicos { get; set; }
        public int ReservacionesHoy { get; set; }
        public int ProductosStockBajo { get; set; }
        public decimal PromedioVentaDiaria { get; set; }
        public List<VentaHoraria> VentasPorHora { get; set; } = new();
        public List<ProductoMasVendido> ProductosMasVendidos { get; set; } = new();
        public List<AlertaInventario> AlertasInventario { get; set; } = new();
    }

    public class VentaHoraria
    {
        public int Hora { get; set; }
        public decimal Total { get; set; }
        public int Ordenes { get; set; }
    }

    public class AlertaInventario
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadDisponible { get; set; }
        public int CantidadMinima { get; set; }
        public string TipoAlerta { get; set; } = string.Empty;
    }

    // Request models for testing
    public class CambiarContrasenaRequest
    {
        public int UsuarioId { get; set; }
        public string ContrasenaActual { get; set; } = string.Empty;
        public string NuevaContrasena { get; set; } = string.Empty;
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }

    public class CrearClienteRequest
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? PreferenciasComida { get; set; }
    }

    public class EntradaInventarioRequest
    {
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public decimal CostoUnitario { get; set; }
    }

    public class ConfirmarReservacionRequest
    {
        public string? Observaciones { get; set; }
    }

    // Classes that need to be added for orders
    public class ItemOrdenRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? NotasEspeciales { get; set; }
    }

    public class CreateOrdenRequest
    {
        public int? MesaId { get; set; }
        public int? ClienteId { get; set; }
        public string TipoOrden { get; set; } = "Mesa";
        public string? Observaciones { get; set; }
        public List<ItemOrdenRequest> Items { get; set; } = new();
    }

    public class AgregarItemsOrdenRequest
    {
        public List<ItemOrdenRequest> Items { get; set; } = new();
    }

    public class ActualizarEstadoOrdenRequest
    {
        public string NuevoEstado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    public class PagarFacturaRequest
    {
        public string MetodoPago { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    // Classes that need to be added for reservations
    public class CreateReservacionRequest
    {
        public int? MesaId { get; set; }
        public int? ClienteId { get; set; }
        public DateTime FechaHora { get; set; }
        public int CantidadPersonas { get; set; }
        public int? DuracionMinutos { get; set; }
        public string? NotasEspeciales { get; set; }
    }

    public class CrearFacturaRequest
    {
        public int OrdenId { get; set; }
        public string? MetodoPago { get; set; } = "Efectivo";
        public decimal Descuento { get; set; }
        public decimal Propina { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CrearFacturaMesaRequest
    {
        public int MesaId { get; set; }
        public decimal Descuento { get; set; }
        public decimal Propina { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
} 