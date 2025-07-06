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

        // Cliente
        CreateMap<Cliente, ClienteBasicoResponse>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto))
            .ForMember(dest => dest.CategoriaCliente, opt => opt.MapFrom(src => src.ObtenerCategoriaCliente()));

        // Producto
        CreateMap<Producto, ProductoResponse>()
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.PrecioFormateado))
            .ForMember(dest => dest.PrecioNumerico, opt => opt.MapFrom(src => src.Precio))
            .ForMember(dest => dest.TiempoPreparacion, opt => opt.MapFrom(src => src.TiempoPreparacionFormateado))
            .ForMember(dest => dest.EstaDisponible, opt => opt.MapFrom(src => src.EstaDisponible))
            .ForMember(dest => dest.EsPlatoDominicano, opt => opt.MapFrom(src => src.EsPlatoDominicano()))
            .ForMember(dest => dest.InformacionNutricional, opt => opt.MapFrom(src => src.ObtenerInformacionNutricional()));

        // Categoría
        CreateMap<Categoria, CategoriaBasicaResponse>();

        // Inventario
        CreateMap<Inventario, InventarioBasicoResponse>()
            .ForMember(dest => dest.NivelStock, opt => opt.MapFrom(src => src.NivelStock))
            .ForMember(dest => dest.ColorIndicador, opt => opt.MapFrom(src => src.ColorIndicador))
            .ForMember(dest => dest.StockBajo, opt => opt.MapFrom(src => src.StockBajo));

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

        // ============================================================================
        // MAPEO DE REQUEST DTOS A ENTIDADES
        // ============================================================================

        // Usuario
        CreateMap<CreateUsuarioRequest, Usuario>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Usuario))
            .ForMember(dest => dest.ContrasenaHash, opt => opt.Ignore()) // Se manejará en el servicio
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.RequiereCambioContrasena, opt => opt.MapFrom(src => src.RequiereCambioContrasena));

        // Reservación
        CreateMap<CreateReservacionRequest, Reservacion>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Pendiente"));

        // Orden
        CreateMap<CreateOrdenRequest, Orden>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Pendiente"))
            .ForMember(dest => dest.NumeroOrden, opt => opt.Ignore()) // Se generará automáticamente
            .ForMember(dest => dest.DetalleOrdenes, opt => opt.MapFrom(src => src.Detalles));

        // DetalleOrden
        CreateMap<CreateDetalleOrdenRequest, DetalleOrden>()
            .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore()) // Se calculará en el servicio
            .ForMember(dest => dest.Descuento, opt => opt.MapFrom(src => 0));

        // Cliente ocasional
        CreateMap<CreateClienteOcasionalRequest, Cliente>()
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.Now.Date))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => true));

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

        // Ignorar propiedades calculadas en mapeos inversos
        CreateMap<UsuarioResponse, Usuario>()
            .ForMember(dest => dest.Rol, opt => opt.Ignore())
            .ForMember(dest => dest.Empleado, opt => opt.Ignore())
            .ForMember(dest => dest.Ordenes, opt => opt.Ignore())
            .ForMember(dest => dest.Facturas, opt => opt.Ignore());

        // Configuración para manejar valores nulos
        AllowNullCollections = true;
        AllowNullDestinationValues = true;

        // Configuración para fechas
        CreateMap<DateTime, string>().ConvertUsing(src => src.ToString("dd/MM/yyyy HH:mm"));
        CreateMap<DateTime?, string>().ConvertUsing(src => src.HasValue ? src.Value.ToString("dd/MM/yyyy HH:mm") : "");
    }

    // ============================================================================
    // MÉTODOS DE UTILIDAD PARA MAPEO COMPLEJO
    // ============================================================================

    /// <summary>
    /// Mapea una orden completa con sus relaciones
    /// </summary>
    public static OrdenResponse MapearOrdenCompleta(Orden orden, IMapper mapper)
    {
        var ordenResponse = mapper.Map<OrdenResponse>(orden);
        
        // Mapear detalles con información adicional
        ordenResponse.Detalles = orden.DetalleOrdenes.Select(detalle => 
        {
            var detalleResponse = mapper.Map<DetalleOrdenResponse>(detalle);
            // Agregar información adicional si es necesario
            return detalleResponse;
        }).ToList();

        return ordenResponse;
    }

    /// <summary>
    /// Mapea el dashboard con información agregada
    /// </summary>
    public static DashboardViewModel MapearDashboard(
        List<Orden> ordenesHoy,
        List<Mesa> mesas,
        List<Reservacion> reservacionesProximas,
        List<Usuario> usuariosActivos,
        IMapper mapper)
    {
        var dashboard = new DashboardViewModel();

        // Resumen diario
        var ventasHoy = ordenesHoy.Where(o => o.EstaFacturada).Sum(o => o.Total);
        dashboard.ResumenDiario = new ResumenDiarioViewModel
        {
            Fecha = DateTime.Today,
            VentasDelDia = $"RD$ {ventasHoy:N2}",
            OrdenesCompletadas = ordenesHoy.Count(o => o.Estado == "Entregada"),
            ClientesAtendidos = ordenesHoy.Select(o => o.ClienteID).Distinct().Count(),
            PromedioOrden = ordenesHoy.Any() ? $"RD$ {ordenesHoy.Average(o => o.Total):N2}" : "RD$ 0.00"
        };

        // Estado de mesas
        dashboard.EstadoMesas = new EstadoMesasViewModel
        {
            TotalMesas = mesas.Count,
            MesasLibres = mesas.Count(m => m.Estado == "Libre"),
            MesasOcupadas = mesas.Count(m => m.Estado == "Ocupada"),
            MesasReservadas = mesas.Count(m => m.Estado == "Reservada"),
            MesasMantenimiento = mesas.Count(m => m.Estado == "Mantenimiento")
        };

        // Órdenes activas
        var ordenesActivas = ordenesHoy.Where(o => o.Estado != "Entregada" && o.Estado != "Cancelada");
        dashboard.OrdenesActivas = new OrdenesActivasViewModel
        {
            OrdenesPendientes = ordenesActivas.Count(o => o.Estado == "Pendiente"),
            OrdenesEnPreparacion = ordenesActivas.Count(o => o.Estado == "EnPreparacion"),
            OrdenesListas = ordenesActivas.Count(o => o.Estado == "Lista"),
            OrdenesRetrasadas = ordenesActivas.Count(o => o.EstaRetrasada)
        };

        // Mapear reservaciones próximas
        dashboard.ReservacionesProximas = reservacionesProximas
            .Where(r => r.FechaYHora.Date == DateTime.Today && r.FechaYHora > DateTime.Now)
            .OrderBy(r => r.FechaYHora)
            .Take(5)
            .Select(r => mapper.Map<ReservacionProximaViewModel>(r))
            .ToList();

        // Mapear empleados activos
        dashboard.EmpleadosActivos = usuariosActivos
            .Where(u => u.Estado && u.Empleado != null)
            .OrderByDescending(u => u.UltimoAcceso)
            .Take(10)
            .Select(u => mapper.Map<EmpleadoActivoViewModel>(u))
            .ToList();

        return dashboard;
    }

    /// <summary>
    /// Formatea el tiempo ocupada de una mesa
    /// </summary>
    private static string? FormatearTiempoOcupada(TimeSpan? tiempo)
    {
        return tiempo.HasValue ? $"{tiempo.Value.Hours}h {tiempo.Value.Minutes}m" : null;
    }
}