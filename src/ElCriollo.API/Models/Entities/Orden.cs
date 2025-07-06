using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa las órdenes/comandas del restaurante
/// </summary>
[Table("Ordenes")]
public class Orden
{
    /// <summary>
    /// Identificador único de la orden
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrdenID { get; set; }

    /// <summary>
    /// Número de orden único generado automáticamente (ej: ORD-20241205-0001)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string NumeroOrden { get; set; } = string.Empty;

    /// <summary>
    /// Mesa donde se realizó la orden (opcional para órdenes para llevar)
    /// </summary>
    [ForeignKey("Mesa")]
    public int? MesaID { get; set; }

    /// <summary>
    /// Cliente que realizó la orden (opcional para clientes eventuales)
    /// </summary>
    [ForeignKey("Cliente")]
    public int? ClienteID { get; set; }

    /// <summary>
    /// Empleado que tomó la orden (FK a Empleados)
    /// </summary>
    [Required]
    [ForeignKey("Empleado")]
    public int EmpleadoID { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la orden
    /// </summary>
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estado de la orden (Pendiente, EnPreparacion, Lista, Entregada, Cancelada)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Tipo de orden (Mesa, Llevar, Delivery)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string TipoOrden { get; set; } = "Mesa";

    /// <summary>
    /// Observaciones especiales de la orden
    /// </summary>
    [StringLength(500)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>
    /// Subtotal calculado (guardado para performance)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubtotalCalculado { get; set; }

    /// <summary>
    /// Impuesto calculado
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Impuesto { get; set; }

    /// <summary>
    /// Total calculado (guardado para performance)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCalculado { get; set; }

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Mesa donde se realizó la orden
    /// </summary>
    public virtual Mesa? Mesa { get; set; }

    /// <summary>
    /// Cliente que realizó la orden
    /// </summary>
    public virtual Cliente? Cliente { get; set; }

    /// <summary>
    /// Empleado que tomó la orden
    /// </summary>
    public virtual Empleado Empleado { get; set; } = null!;

    /// <summary>
    /// Detalles de la orden (productos y cantidades)
    /// </summary>
    public virtual ICollection<DetalleOrden> DetalleOrdenes { get; set; } = new List<DetalleOrden>();

    /// <summary>
    /// Facturas generadas para esta orden
    /// </summary>
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si la orden está pendiente
    /// </summary>
    [NotMapped]
    public bool EstaPendiente => Estado == "Pendiente";

    /// <summary>
    /// Indica si la orden está en preparación
    /// </summary>
    [NotMapped]
    public bool EstaEnPreparacion => Estado == "EnPreparacion";

    /// <summary>
    /// Indica si la orden está lista
    /// </summary>
    [NotMapped]
    public bool EstaLista => Estado == "Lista";

    /// <summary>
    /// Indica si la orden fue entregada
    /// </summary>
    [NotMapped]
    public bool FueEntregada => Estado == "Entregada";

    /// <summary>
    /// Indica si la orden fue cancelada
    /// </summary>
    [NotMapped]
    public bool FueCancelada => Estado == "Cancelada";

    /// <summary>
    /// Indica si es una orden para mesa
    /// </summary>
    [NotMapped]
    public bool EsParaMesa => TipoOrden == "Mesa";

    /// <summary>
    /// Indica si es una orden para llevar
    /// </summary>
    [NotMapped]
    public bool EsParaLlevar => TipoOrden == "Llevar";

    /// <summary>
    /// Indica si es una orden de delivery
    /// </summary>
    [NotMapped]
    public bool EsDelivery => TipoOrden == "Delivery";

    /// <summary>
    /// Cantidad total de items en la orden
    /// </summary>
    [NotMapped]
    public int TotalItems => DetalleOrdenes?.Sum(d => d.Cantidad) ?? 0;

    /// <summary>
    /// Subtotal de la orden (sin descuentos ni impuestos)
    /// </summary>
    [NotMapped]
    public decimal Subtotal 
    { 
        get => SubtotalCalculado > 0 ? SubtotalCalculado : DetalleOrdenes?.Sum(d => d.Subtotal) ?? 0;
        set => SubtotalCalculado = value;
    }

    /// <summary>
    /// Total de descuentos aplicados
    /// </summary>
    [NotMapped]
    public decimal TotalDescuentos => DetalleOrdenes?.Sum(d => d.Descuento) ?? 0;

    /// <summary>
    /// Total de la orden
    /// </summary>
    [NotMapped]
    public decimal Total 
    { 
        get => TotalCalculado > 0 ? TotalCalculado : Subtotal + Impuesto;
        set => TotalCalculado = value;
    }

    /// <summary>
    /// Tiempo transcurrido desde la creación
    /// </summary>
    [NotMapped]
    public TimeSpan TiempoTranscurrido => DateTime.UtcNow - FechaCreacion;

    /// <summary>
    /// Tiempo estimado de preparación de toda la orden
    /// </summary>
    [NotMapped]
    public int TiempoPreparacionEstimado => DetalleOrdenes?
        .Where(d => d.Producto != null)
        .Max(d => d.Producto!.TiempoPreparacion ?? 0) ?? 0;

    /// <summary>
    /// Hora estimada de finalización
    /// </summary>
    [NotMapped]
    public DateTime HoraEstimadaFinalizacion => FechaCreacion.AddMinutes(TiempoPreparacionEstimado);

    /// <summary>
    /// Indica si la orden está retrasada
    /// </summary>
    [NotMapped]
    public bool EstaRetrasada => EstaEnPreparacion && DateTime.UtcNow > HoraEstimadaFinalizacion;

    /// <summary>
    /// Categorías de productos en la orden
    /// </summary>
    [NotMapped]
    public IEnumerable<string> CategoriasProductos => DetalleOrdenes?
        .Where(d => d.Producto?.Categoria?.Nombre != null)
        .Select(d => d.Producto!.Categoria!.Nombre)
        .Distinct() ?? Enumerable.Empty<string>();

    /// <summary>
    /// Indica si la orden está facturada
    /// </summary>
    [NotMapped]
    public bool EstaFacturada => Facturas?.Any(f => f.Estado == "Pagada") ?? false;

    /// <summary>
    /// Total formateado en pesos dominicanos
    /// </summary>
    [NotMapped]
    public string TotalFormateado => $"RD$ {Total:N2}";

    // ============================================================================
    // PROPIEDADES ALIAS PARA COMPATIBILIDAD
    // ============================================================================

    /// <summary>
    /// Alias para UsuarioID (mapea a EmpleadoID para compatibilidad)
    /// </summary>
    [NotMapped]
    public int UsuarioID 
    { 
        get => EmpleadoID;
        set => EmpleadoID = value;
    }

    /// <summary>
    /// Alias para ObservacionesEspeciales (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string? ObservacionesEspeciales 
    { 
        get => Observaciones;
        set => Observaciones = value;
    }

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Cambia el estado de la orden
    /// </summary>
    public void CambiarEstado(string nuevoEstado)
    {
        var estadosValidos = new[] { "Pendiente", "EnPreparacion", "Lista", "Entregada", "Cancelada" };
        
        if (!estadosValidos.Contains(nuevoEstado))
            throw new ArgumentException($"Estado inválido: {nuevoEstado}");

        // Validaciones de transiciones de estado
        switch (Estado)
        {
            case "Cancelada":
                throw new InvalidOperationException("No se puede cambiar el estado de una orden cancelada");
            case "Entregada":
                if (nuevoEstado != "Entregada")
                    throw new InvalidOperationException("No se puede cambiar el estado de una orden entregada");
                break;
        }

        Estado = nuevoEstado;
    }

    /// <summary>
    /// Marca la orden como en preparación
    /// </summary>
    public void IniciarPreparacion()
    {
        CambiarEstado("EnPreparacion");
    }

    /// <summary>
    /// Marca la orden como lista para entregar
    /// </summary>
    public void MarcarComoLista()
    {
        CambiarEstado("Lista");
    }

    /// <summary>
    /// Marca la orden como entregada
    /// </summary>
    public void MarcarComoEntregada()
    {
        CambiarEstado("Entregada");
    }

    /// <summary>
    /// Cancela la orden
    /// </summary>
    public void Cancelar()
    {
        if (Estado == "Entregada")
            throw new InvalidOperationException("No se puede cancelar una orden ya entregada");

        CambiarEstado("Cancelada");
    }

    /// <summary>
    /// Agrega un producto a la orden
    /// </summary>
    public void AgregarProducto(int productoId, int cantidad, decimal precioUnitario, string? observaciones = null)
    {
        if (Estado != "Pendiente")
            throw new InvalidOperationException("Solo se pueden agregar productos a órdenes pendientes");

        var detalleExistente = DetalleOrdenes.FirstOrDefault(d => d.ProductoID == productoId);
        
        if (detalleExistente != null)
        {
            detalleExistente.ActualizarCantidad(detalleExistente.Cantidad + cantidad);
        }
        else
        {
            var nuevoDetalle = new DetalleOrden
            {
                OrdenID = OrdenID,
                ProductoID = productoId,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario,
                Observaciones = observaciones
            };
            DetalleOrdenes.Add(nuevoDetalle);
        }
    }

    /// <summary>
    /// Agrega un combo a la orden
    /// </summary>
    public void AgregarCombo(int comboId, int cantidad, decimal precioUnitario, string? observaciones = null)
    {
        if (Estado != "Pendiente")
            throw new InvalidOperationException("Solo se pueden agregar combos a órdenes pendientes");

        var detalleExistente = DetalleOrdenes.FirstOrDefault(d => d.ComboID == comboId);
        
        if (detalleExistente != null)
        {
            detalleExistente.ActualizarCantidad(detalleExistente.Cantidad + cantidad);
        }
        else
        {
            var nuevoDetalle = new DetalleOrden
            {
                OrdenID = OrdenID,
                ComboID = comboId,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario,
                Observaciones = observaciones
            };
            DetalleOrdenes.Add(nuevoDetalle);
        }
    }

    /// <summary>
    /// Elimina un detalle de la orden
    /// </summary>
    public void EliminarDetalle(int detalleOrdenId)
    {
        if (Estado != "Pendiente")
            throw new InvalidOperationException("Solo se pueden eliminar items de órdenes pendientes");

        var detalle = DetalleOrdenes.FirstOrDefault(d => d.DetalleOrdenID == detalleOrdenId);
        if (detalle != null)
        {
            DetalleOrdenes.Remove(detalle);
        }
    }

    /// <summary>
    /// Valida que la orden sea válida antes de procesar
    /// </summary>
    public List<string> ValidarOrden()
    {
        var errores = new List<string>();

        if (!DetalleOrdenes.Any())
            errores.Add("La orden debe tener al menos un producto o combo");

        if (EsParaMesa && MesaID == null)
            errores.Add("Las órdenes para mesa deben tener una mesa asignada");

        if (EmpleadoID <= 0)
            errores.Add("La orden debe tener un empleado asignado");

        // Validar disponibilidad de productos
        foreach (var detalle in DetalleOrdenes)
        {
            var validacionDetalle = detalle.ValidarDetalle();
            errores.AddRange(validacionDetalle);
        }

        return errores;
    }

    /// <summary>
    /// Verifica si todos los productos están disponibles
    /// </summary>
    public bool TodosLosProductosDisponibles()
    {
        return DetalleOrdenes.All(d => d.EstaDisponible);
    }

    /// <summary>
    /// Obtiene los productos no disponibles
    /// </summary>
    public List<string> ObtenerProductosNoDisponibles()
    {
        return DetalleOrdenes
            .Where(d => !d.EstaDisponible)
            .Select(d => d.Producto?.Nombre ?? d.Combo?.Nombre ?? "Item desconocido")
            .ToList();
    }

    /// <summary>
    /// Calcula el tiempo total que ha estado la orden en cada estado
    /// </summary>
    public Dictionary<string, TimeSpan> CalcularTiemposEstado()
    {
        // Simulación básica - en un sistema real, esto requeriría un log de cambios de estado
        var tiempos = new Dictionary<string, TimeSpan>();
        var tiempoTotal = TiempoTranscurrido;

        switch (Estado)
        {
            case "Pendiente":
                tiempos["Pendiente"] = tiempoTotal;
                break;
            case "EnPreparacion":
                tiempos["Pendiente"] = TimeSpan.FromMinutes(5); // Estimación
                tiempos["EnPreparacion"] = tiempoTotal - TimeSpan.FromMinutes(5);
                break;
            case "Lista":
                tiempos["Pendiente"] = TimeSpan.FromMinutes(5);
                tiempos["EnPreparacion"] = TimeSpan.FromMinutes(TiempoPreparacionEstimado);
                tiempos["Lista"] = tiempoTotal - TimeSpan.FromMinutes(5 + TiempoPreparacionEstimado);
                break;
            case "Entregada":
                tiempos["Pendiente"] = TimeSpan.FromMinutes(5);
                tiempos["EnPreparacion"] = TimeSpan.FromMinutes(TiempoPreparacionEstimado);
                tiempos["Lista"] = TimeSpan.FromMinutes(5);
                tiempos["Entregada"] = tiempoTotal - TimeSpan.FromMinutes(10 + TiempoPreparacionEstimado);
                break;
        }

        return tiempos;
    }

    /// <summary>
    /// Obtiene un resumen de la orden
    /// </summary>
    public object GenerarResumen()
    {
        return new
        {
            NumeroOrden = NumeroOrden,
            Mesa = Mesa?.NumeroMesa.ToString() ?? "N/A",
            Cliente = Cliente?.NombreCompleto ?? "Cliente ocasional",
            Empleado = Empleado?.NombreCompleto,
            FechaCreacion = FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
            Estado = Estado,
            TipoOrden = TipoOrden,
            TotalItems = TotalItems,
            Total = TotalFormateado,
            TiempoTranscurrido = $"{TiempoTranscurrido.Hours:D2}:{TiempoTranscurrido.Minutes:D2}",
            TiempoPreparacionEstimado = $"{TiempoPreparacionEstimado} min",
            EstaRetrasada = EstaRetrasada,
            EstaFacturada = EstaFacturada,
            Observaciones = Observaciones
        };
    }

    /// <summary>
    /// Representación en string de la orden
    /// </summary>
    public override string ToString()
    {
        var cliente = Cliente?.NombreCompleto ?? "Cliente ocasional";
        var mesa = Mesa?.NumeroMesa.ToString() ?? "Sin mesa";
        return $"{NumeroOrden} - {cliente} (Mesa {mesa}) - {Estado} - {TotalFormateado}";
    }
}