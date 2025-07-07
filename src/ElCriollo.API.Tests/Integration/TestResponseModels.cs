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
        public int MesaId { get; set; }
        public int NumeroMesa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public string? Ubicacion { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaUltimoCambio { get; set; }
        public List<OrdenBasicaResponse> OrdenesActivas { get; set; } = new();
        public ReservacionBasicaResponse? ReservacionActiva { get; set; }
    }

    public class OrdenBasicaResponse
    {
        public int OrdenId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class ReservacionBasicaResponse
    {
        public int ReservacionId { get; set; }
        public DateTime FechaHora { get; set; }
        public int CantidadPersonas { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
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
        public int MovimientoId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public decimal? CostoUnitario { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string UsuarioResponsable { get; set; } = string.Empty;
    }

    public class ClienteResponse
    {
        public int ClienteId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? PreferenciasComida { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int TotalVisitas { get; set; }
        public decimal TotalGastado { get; set; }
    }

    public class OrdenResponse
    {
        public int OrdenId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int? MesaId { get; set; }
        public int? ClienteId { get; set; }
        public string TipoOrden { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Observaciones { get; set; }
        public List<ItemOrdenResponse> Items { get; set; } = new();
        public ClienteBasicoResponse? Cliente { get; set; }
        public MesaBasicaResponse? Mesa { get; set; }
    }

    public class ItemOrdenResponse
    {
        public int ItemId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? NotasEspeciales { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class ClienteBasicoResponse
    {
        public int ClienteId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class MesaBasicaResponse
    {
        public int MesaId { get; set; }
        public int NumeroMesa { get; set; }
        public string? Ubicacion { get; set; }
    }

    public class ReservacionResponse
    {
        public int ReservacionId { get; set; }
        public int MesaId { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaHora { get; set; }
        public int CantidadPersonas { get; set; }
        public int? DuracionMinutos { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? NotasEspeciales { get; set; }
        public DateTime FechaCreacion { get; set; }
        public ClienteBasicoResponse Cliente { get; set; } = null!;
        public MesaBasicaResponse Mesa { get; set; } = null!;
    }

    public class FacturaResponse
    {
        public int FacturaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public int OrdenId { get; set; }
        public int? ClienteId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Propina { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPago { get; set; }
        public string? Observaciones { get; set; }
        public OrdenBasicaResponse Orden { get; set; } = null!;
        public ClienteBasicoResponse? Cliente { get; set; }
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
        public int Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public decimal CostoUnitario { get; set; }
    }

    public class ConfirmarReservacionRequest
    {
        public string? Observaciones { get; set; }
    }

    // AgregarItemsOrdenRequest usa ItemOrdenRequest del proyecto principal
    // using ElCriollo.API.Models.DTOs.Request;

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

    // ItemOrdenRequest se usa del proyecto principal
    // using ElCriollo.API.Models.DTOs.Request;

    // CrearOrdenRequest se usa del proyecto principal
    // using ElCriollo.API.Models.DTOs.Request;

    public class CrearReservacionRequest
    {
        public int MesaId { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaHora { get; set; }
        public int CantidadPersonas { get; set; }
        public int DuracionMinutos { get; set; }
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