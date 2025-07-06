using AutoMapper;
using ElCriollo.API.Models.Entities;
using ElCriollo.API.Models.DTOs.Request;
using ElCriollo.API.Models.DTOs.Response;
using ElCriollo.API.Models.ViewModels;

namespace ElCriollo.API.Helpers;

/// <summary>
/// Configuración de mapeo automático entre entidades y DTOs
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ============================================================================
        // MAPEO DE ENTIDADES A RESPONSE DTOS
        // ============================================================================

        // Usuario
        CreateMap<Usuario, UsuarioResponse>()
            .ForMember(dest => dest.Usuario, opt => opt.MapFrom(src => src.UsuarioNombre))
            .ForMember(dest => dest.Empleado, opt => opt.MapFrom(src => src.Empleado));

        // Rol
        CreateMap<Rol, RolResponse>();

        // Empleado
        CreateMap<Empleado, EmpleadoBasicoResponse>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto));

        CreateMap<Empleado, EmpleadoResponse>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto))
            .ForMember(dest => dest.TelefonoFormateado, opt => opt.MapFrom(src => src.TelefonoFormateado))
            .ForMember(dest => dest.SalarioFormateado, opt => opt.MapFrom(src => src.SalarioFormateado))
            .ForMember(dest => dest.TiempoEnEmpresa, opt => opt.MapFrom(src => src.TiempoEnEmpresa)) // TiempoEnEmpresa ya es string
            .ForMember(dest => dest.EsEmpleadoActivo, opt => opt.MapFrom(src => src.EsActivo));

        // Cliente
        CreateMap<Cliente, ClienteBasicoResponse>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto))
            .ForMember(dest => dest.CategoriaCliente, opt => opt.MapFrom(src => src.ObtenerCategoriaCliente()));

        CreateMap<Cliente, ClienteResponse>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto))
            .ForMember(dest => dest.CategoriaCliente, opt => opt.MapFrom(src => src.ObtenerCategoriaCliente()))
            .ForMember(dest => dest.TotalOrdenes, opt => opt.MapFrom(src => src.Ordenes.Count))
            .ForMember(dest => dest.TotalReservaciones, opt => opt.MapFrom(src => src.Reservaciones.Count))
            .ForMember(dest => dest.TotalFacturas, opt => opt.MapFrom(src => src.Facturas.Count))
            .ForMember(dest => dest.PromedioConsumo, opt => opt.MapFrom(src => 
                src.Ordenes.Any() ? $"RD$ {src.Ordenes.Average(o => o.Total):N2}" : "RD$ 0.00"))
            .ForMember(dest => dest.UltimaVisita, opt => opt.MapFrom(src => 
                src.Ordenes.OrderByDescending(o => o.FechaCreacion).FirstOrDefault() != null ?
                src.Ordenes.OrderByDescending(o => o.FechaCreacion).FirstOrDefault().FechaCreacion :
                (DateTime?)null));

        // Producto
        CreateMap<Producto, ProductoResponse>()
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.PrecioFormateado))
            .ForMember(dest => dest.PrecioNumerico, opt => opt.MapFrom(src => src.Precio))
            .ForMember(dest => dest.TiempoPreparacion, opt => opt.MapFrom(src => src.TiempoPreparacionFormateado))
            .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.EstaDisponible))
            .ForMember(dest => dest.EsPlatoDominicano, opt => opt.MapFrom(src => src.EsPlatoDominicano()))
            .ForMember(dest => dest.InformacionNutricional, opt => opt.MapFrom(src => src.ObtenerInformacionNutricional()));

        // Categoría
        CreateMap<Categoria, CategoriaBasicaResponse>()
            .ForMember(dest => dest.NombreCategoria, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.CantidadProductos, opt => opt.MapFrom(src => src.Productos.Count))
            .ForMember(dest => dest.ProductosDisponibles, opt => opt.MapFrom(src => src.Productos.Count(p => p.Estado)));

        CreateMap<Categoria, CategoriaResponse>()
            .ForMember(dest => dest.TotalProductos, opt => opt.MapFrom(src => src.Productos.Count))
            .ForMember(dest => dest.ProductosActivos, opt => opt.MapFrom(src => src.Productos.Count(p => p.Estado)))
            .ForMember(dest => dest.RangoPrecios, opt => opt.MapFrom(src => src.ObtenerRangoPrecios()));

        // Inventario
        CreateMap<Inventario, InventarioBasicoResponse>()
            .ForMember(dest => dest.NivelStock, opt => opt.MapFrom(src => src.NivelStock))
            .ForMember(dest => dest.ColorIndicador, opt => opt.MapFrom(src => src.ColorIndicador))
            .ForMember(dest => dest.StockBajo, opt => opt.MapFrom(src => src.StockBajo));

        CreateMap<Inventario, InventarioResponse>()
            .ForMember(dest => dest.NivelStock, opt => opt.MapFrom(src => src.NivelStock))
            .ForMember(dest => dest.ColorIndicador, opt => opt.MapFrom(src => src.ColorIndicador))
            .ForMember(dest => dest.StockBajo, opt => opt.MapFrom(src => src.StockBajo))
            .ForMember(dest => dest.DiasParaReabastecer, opt => opt.MapFrom(src => src.DiasParaReabastecer))
            .ForMember(dest => dest.ValorInventario, opt => opt.MapFrom(src => src.ValorInventarioFormateado))
            .ForMember(dest => dest.PorcentajeStock, opt => opt.MapFrom(src => src.PorcentajeStock))
            .ForMember(dest => dest.NecesitaReabastecimiento, opt => opt.MapFrom(src => src.NecesitaReabastecimiento));

        // Mesa
        CreateMap<Mesa, MesaResponse>()
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.ObtenerDescripcion()))
            .ForMember(dest => dest.ClienteActual, opt => opt.MapFrom(src => src.ObtenerClienteActual()))
            .ForMember(dest => dest.OrdenActual, opt => opt.MapFrom(src => src.OrdenActual))
            .ForMember(dest => dest.ReservacionActual, opt => opt.MapFrom(src => src.ReservacionActual))
            .ForMember(dest => dest.TiempoOcupada, opt => opt.MapFrom(src => 
                FormatearTiempoOcupada(src.TiempoOcupada())))
            .ForMember(dest => dest.NecesitaLimpieza, opt => opt.MapFrom(src => src.NecesitaLimpieza));

        CreateMap<Mesa, MesaBasicaResponse>()
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.ObtenerDescripcion()));

        // Reservación
        CreateMap<Reservacion, ReservacionBasicaResponse>()
            .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente.NombreCompleto))
            .ForMember(dest => dest.Horario, opt => opt.MapFrom(src => src.ObtenerHorarioFormateado()))
            .ForMember(dest => dest.TiempoHastaReservacion, opt => opt.MapFrom(src => 
                src.TiempoHastaReservacion.TotalMinutes > 0 ? 
                $"{src.TiempoHastaReservacion.Hours}h {src.TiempoHastaReservacion.Minutes}m" : null));

        CreateMap<Reservacion, ReservacionResponse>()
            .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente))
            .ForMember(dest => dest.Mesa, opt => opt.MapFrom(src => src.Mesa))
            .ForMember(dest => dest.Horario, opt => opt.MapFrom(src => src.ObtenerHorarioFormateado()))
            .ForMember(dest => dest.TiempoHastaReservacion, opt => opt.MapFrom(src => 
                src.TiempoHastaReservacion.TotalMinutes > 0 ? 
                $"{src.TiempoHastaReservacion.Hours}h {src.TiempoHastaReservacion.Minutes}m" : null))
            .ForMember(dest => dest.PuedeModificar, opt => opt.MapFrom(src => src.PuedeModificar))
            .ForMember(dest => dest.PuedeCancelar, opt => opt.MapFrom(src => src.PuedeCancelar))
            .ForMember(dest => dest.TiempoParaLlegar, opt => opt.MapFrom(src => src.TiempoParaLlegar)); // Ya es int?

        // Orden
        CreateMap<Orden, OrdenResponse>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.TotalFormateado))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalFormateado))
            .ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => 
                $"{src.TiempoTranscurrido.Hours:D2}:{src.TiempoTranscurrido.Minutes:D2}"))
            .ForMember(dest => dest.TiempoPreparacionEstimado, opt => opt.MapFrom(src => $"{src.TiempoPreparacionEstimado} min"))
            .ForMember(dest => dest.EstaRetrasada, opt => opt.MapFrom(src => src.EstaRetrasada))
            .ForMember(dest => dest.EstaFacturada, opt => opt.MapFrom(src => src.EstaFacturada))
            .ForMember(dest => dest.CategoriasProductos, opt => opt.MapFrom(src => src.CategoriasProductos.ToList()));

        CreateMap<Orden, OrdenBasicaResponse>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalFormateado))
            .ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => 
                $"{src.TiempoTranscurrido.Hours:D2}:{src.TiempoTranscurrido.Minutes:D2}"));

        // DetalleOrden
        CreateMap<DetalleOrden, DetalleOrdenResponse>()
            .ForMember(dest => dest.TipoItem, opt => opt.MapFrom(src => src.TipoItem))
            .ForMember(dest => dest.NombreItem, opt => opt.MapFrom(src => src.NombreItem))
            .ForMember(dest => dest.DescripcionItem, opt => opt.MapFrom(src => src.DescripcionItem))
            .ForMember(dest => dest.CategoriaItem, opt => opt.MapFrom(src => src.CategoriaItem))
            .ForMember(dest => dest.PrecioUnitario, opt => opt.MapFrom(src => src.PrecioUnitarioFormateado))
            .ForMember(dest => dest.Descuento, opt => opt.MapFrom(src => $"RD$ {src.Descuento:N2}"))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.SubtotalFormateado))
            .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.EstaDisponible))
            .ForMember(dest => dest.TiempoPreparacion, opt => opt.MapFrom(src => $"{src.TiempoPreparacion} min"))
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto));

        // Combo
        CreateMap<Combo, ComboResponse>()
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.PrecioFormateado))
            .ForMember(dest => dest.PrecioNumerico, opt => opt.MapFrom(src => src.Precio))
            .ForMember(dest => dest.Descuento, opt => opt.MapFrom(src => src.DescuentoFormateado))
            .ForMember(dest => dest.Ahorro, opt => opt.MapFrom(src => $"RD$ {src.Ahorro:N2}"))
            .ForMember(dest => dest.PorcentajeDescuento, opt => opt.MapFrom(src => src.PorcentajeDescuento))
            .ForMember(dest => dest.CantidadProductos, opt => opt.MapFrom(src => src.CantidadProductos))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.EstaDisponible))
            .ForMember(dest => dest.TiempoPreparacion, opt => opt.MapFrom(src => $"{src.TiempoPreparacionTotal} min"))
            .ForMember(dest => dest.EsComboDominicano, opt => opt.MapFrom(src => src.EsComboDominicano()))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.Productos, opt => opt.MapFrom(src => src.ComboProductos));

        CreateMap<Combo, ComboBasicoResponse>()
            .ForMember(dest => dest.NombreCombo, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio))
            .ForMember(dest => dest.Disponible, opt => opt.MapFrom(src => src.EstaDisponible))
            .ForMember(dest => dest.Productos, opt => opt.MapFrom(src => 
                src.ComboProductos.Select(cp => new ProductoComboBasico
                {
                    ProductoID = cp.ProductoID,
                    Nombre = cp.Producto.Nombre,
                    Cantidad = cp.Cantidad
                })));

        // ComboProducto
        CreateMap<ComboProducto, ComboProductoResponse>()
            .ForMember(dest => dest.Producto, opt => opt.MapFrom(src => src.Producto))
            .ForMember(dest => dest.PrecioTotal, opt => opt.MapFrom(src => src.PrecioTotalFormateado))
            .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.EstaDisponible));

        // Factura
        CreateMap<Factura, FacturaResponse>()
            .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente))
            .ForMember(dest => dest.Empleado, opt => opt.MapFrom(src => src.Empleado))
            .ForMember(dest => dest.Mesa, opt => opt.MapFrom(src => src.Orden.Mesa))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.SubtotalFormateado))
            .ForMember(dest => dest.Impuesto, opt => opt.MapFrom(src => src.ImpuestoFormateado))
            .ForMember(dest => dest.Descuento, opt => opt.MapFrom(src => src.DescuentoFormateado))
            .ForMember(dest => dest.Propina, opt => opt.MapFrom(src => src.PropinaFormateado))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalFormateado))
            .ForMember(dest => dest.PorcentajeImpuesto, opt => opt.MapFrom(src => src.PorcentajeImpuesto))
            .ForMember(dest => dest.PorcentajeDescuento, opt => opt.MapFrom(src => src.PorcentajeDescuento))
            .ForMember(dest => dest.PorcentajePropina, opt => opt.MapFrom(src => src.PorcentajePropina))
            .ForMember(dest => dest.ObservacionesPago, opt => opt.MapFrom(src => src.ObservacionesPago));

        CreateMap<Factura, FacturaBasicaResponse>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalFormateado))
            .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente.NombreCompleto))
            .ForMember(dest => dest.FechaFormateada, opt => opt.MapFrom(src => src.FechaFactura.ToString("dd/MM/yyyy HH:mm")));

        // EmailTransaccion
        CreateMap<EmailTransaccion, EmailTransaccionResponse>()
            .ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => src.TiempoTranscurrido))
            .ForMember(dest => dest.FueExitoso, opt => opt.MapFrom(src => src.FueExitoso))
            .ForMember(dest => dest.RequiereReintento, opt => opt.MapFrom(src => src.RequiereReintento));

        // ============================================================================
        // MAPEO DE REQUEST DTOS A ENTIDADES
        // ============================================================================

        // Usuario
        CreateMap<LoginRequest, Usuario>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Username));
                
        CreateMap<CreateUsuarioRequest, Usuario>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.RolID, opt => opt.MapFrom(src => src.RolId))
            .ForMember(dest => dest.EmpleadoID, opt => opt.MapFrom(src => src.EmpleadoId));
                
        CreateMap<CreateOrdenRequest, Orden>()
            .ForMember(dest => dest.MesaID, opt => opt.MapFrom(src => src.MesaId))
            .ForMember(dest => dest.ClienteID, opt => opt.MapFrom(src => src.ClienteId));
                
        CreateMap<CreateReservacionRequest, Reservacion>()
            .ForMember(dest => dest.MesaID, opt => opt.MapFrom(src => src.MesaId))
            .ForMember(dest => dest.ClienteID, opt => opt.MapFrom(src => src.ClienteId))
            .ForMember(dest => dest.FechaYHora, opt => opt.MapFrom(src => src.FechaHora))
            .ForMember(dest => dest.DuracionEstimada, opt => opt.MapFrom(src => src.DuracionMinutos ?? 120))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.NotasEspeciales));

        // Actualizar Reservación - mapeo parcial para actualizaciones
        CreateMap<ActualizarReservacionRequest, Reservacion>()
            .ForMember(dest => dest.ReservacionID, opt => opt.Ignore())
            .ForMember(dest => dest.FechaYHora, opt => opt.Condition(src => src.FechaHora.HasValue))
            .ForMember(dest => dest.CantidadPersonas, opt => opt.Condition(src => src.CantidadPersonas.HasValue))
            .ForMember(dest => dest.MesaID, opt => opt.MapFrom(src => src.MesaId))
            .ForMember(dest => dest.MesaID, opt => opt.Condition(src => src.MesaId.HasValue))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.NotasEspeciales))
            .ForMember(dest => dest.Observaciones, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.NotasEspeciales)));

        // Producto
        CreateMap<CrearProductoRequest, Producto>()
            .ForMember(dest => dest.CategoriaID, opt => opt.MapFrom(src => src.CategoriaId))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.TiempoPreparacion, opt => opt.MapFrom(src => src.TiempoPreparacion ?? 15)); // Default 15 min

        // Actualizar Producto - mapeo parcial para actualizaciones
        CreateMap<ActualizarProductoRequest, Producto>()
            .ForMember(dest => dest.ProductoID, opt => opt.Ignore())
            .ForMember(dest => dest.Nombre, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Nombre)))
            .ForMember(dest => dest.Descripcion, opt => opt.Condition(src => src.Descripcion != null))
            .ForMember(dest => dest.Precio, opt => opt.Condition(src => src.Precio.HasValue))
            .ForMember(dest => dest.CategoriaID, opt => opt.Condition(src => src.CategoriaId.HasValue))
            .ForMember(dest => dest.Estado, opt => opt.Condition(src => src.Disponible.HasValue));

        // Orden
        CreateMap<CreateOrdenRequest, Orden>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Pendiente"))
            .ForMember(dest => dest.NumeroOrden, opt => opt.Ignore()) // Se generará automáticamente
            .ForMember(dest => dest.DetalleOrdenes, opt => opt.MapFrom(src => src.Items)); // Cambiar Detalles por Items

        // DetalleOrden
        CreateMap<ItemOrdenRequest, DetalleOrden>() // Cambiar CreateDetalleOrdenRequest por ItemOrdenRequest
            .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore()) // Se calculará en el servicio
            .ForMember(dest => dest.Descuento, opt => opt.MapFrom(src => 0));

        // Cliente ocasional
        CreateMap<CreateClienteOcasionalRequest, Cliente>()
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.Now.Date))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => true));

        // Factura Request - No se mapea directamente a entidad porque requiere lógica de negocio
        // CreateFacturaRequest se usa para parámetros, no para crear directamente la entidad

        // ============================================================================
        // MAPEO DE ENTIDADES A DTOs (para servicios que usan FacturaDto)
        // ============================================================================

        // Factura a FacturaDto (igual que FacturaResponse, se usa como sinónimo)
        CreateMap<Factura, FacturaDto>()
            .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente))
            .ForMember(dest => dest.Empleado, opt => opt.MapFrom(src => src.Empleado))
            .ForMember(dest => dest.Mesa, opt => opt.MapFrom(src => src.Orden.Mesa))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.SubtotalFormateado))
            .ForMember(dest => dest.Impuesto, opt => opt.MapFrom(src => src.ImpuestoFormateado))
            .ForMember(dest => dest.Descuento, opt => opt.MapFrom(src => src.DescuentoFormateado))
            .ForMember(dest => dest.Propina, opt => opt.MapFrom(src => src.PropinaFormateado))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalFormateado))
            .ForMember(dest => dest.PorcentajeImpuesto, opt => opt.MapFrom(src => src.PorcentajeImpuesto))
            .ForMember(dest => dest.PorcentajeDescuento, opt => opt.MapFrom(src => src.PorcentajeDescuento))
            .ForMember(dest => dest.PorcentajePropina, opt => opt.MapFrom(src => src.PorcentajePropina))
            .ForMember(dest => dest.ObservacionesPago, opt => opt.MapFrom(src => src.ObservacionesPago))
            // Mapear propiedades numéricas
            .ForMember(dest => dest.TotalNumerico, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.ImpuestoNumerico, opt => opt.MapFrom(src => src.Impuesto))
            .ForMember(dest => dest.PropinaNumerico, opt => opt.MapFrom(src => src.Propina))
            .ForMember(dest => dest.DescuentoNumerico, opt => opt.MapFrom(src => src.Descuento))
            .ForMember(dest => dest.SubtotalNumerico, opt => opt.MapFrom(src => src.Subtotal));

        // ============================================================================
        // MAPEO PARA VIEWMODELS ESPECÍFICOS
        // ============================================================================

        // Dashboard - Producto popular
        CreateMap<Producto, ProductoPopularViewModel>()
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria.Nombre))
            .ForMember(dest => dest.CantidadVendida, opt => opt.MapFrom(src => src.TotalOrdenado))
            .ForMember(dest => dest.Ingresos, opt => opt.MapFrom(src => 
                $"RD$ {src.DetalleOrdenes.Sum(d => d.Subtotal):N2}"));

        // Dashboard - Empleado activo
        CreateMap<Usuario, EmpleadoActivoViewModel>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.ObtenerNombreCompleto()))
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol.NombreRol))
            .ForMember(dest => dest.UltimoAcceso, opt => opt.MapFrom(src => 
                src.UltimoAcceso.HasValue ? src.UltimoAcceso.Value.ToString("dd/MM HH:mm") : "Nunca"))
            .ForMember(dest => dest.OrdenesAtendidas, opt => opt.MapFrom(src => 
                src.Ordenes.Count(o => o.FechaCreacion.Date == DateTime.Today)))
            .ForMember(dest => dest.EstadoConexion, opt => opt.MapFrom(src => 
                src.UltimoAcceso.HasValue && src.UltimoAcceso.Value > DateTime.UtcNow.AddMinutes(-30) ? "En línea" : "Desconectado"));

        // Dashboard - Reservación próxima
        CreateMap<Reservacion, ReservacionProximaViewModel>()
            .ForMember(dest => dest.Cliente, opt => opt.MapFrom(src => src.Cliente.NombreCompleto))
            .ForMember(dest => dest.NumeroMesa, opt => opt.MapFrom(src => src.Mesa.NumeroMesa))
            .ForMember(dest => dest.Hora, opt => opt.MapFrom(src => src.FechaYHora.ToString("HH:mm")))
            .ForMember(dest => dest.TiempoHasta, opt => opt.MapFrom(src => 
                src.TiempoHastaReservacion.TotalMinutes > 0 ? 
                $"{src.TiempoHastaReservacion.Hours}h {src.TiempoHastaReservacion.Minutes}m" : "Ahora"));

        // ============================================================================
        // CONFIGURACIONES ADICIONALES
        // ============================================================================

        // Configuración para manejar valores nulos
        AllowNullCollections = true;
        AllowNullDestinationValues = true;
    }

    // ============================================================================
    // MÉTODOS DE UTILIDAD PARA MAPEO COMPLEJO
    // ============================================================================

    /// <summary>
    /// Formatea el tiempo ocupada de una mesa
    /// </summary>
    private static string? FormatearTiempoOcupada(TimeSpan? tiempo)
    {
        return tiempo.HasValue ? $"{tiempo.Value.Hours}h {tiempo.Value.Minutes}m" : null;
    }
}