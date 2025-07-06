using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los detalles de cada orden (productos/combos específicos)
/// </summary>
[Table("DetalleOrdenes")]
public class DetalleOrden
{
    /// <summary>
    /// Identificador único del detalle de orden
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DetalleOrdenID { get; set; }

    /// <summary>
    /// Orden a la que pertenece este detalle (FK a Ordenes)
    /// </summary>
    [Required]
    [ForeignKey("Orden")]
    public int OrdenID { get; set; }

    /// <summary>
    /// Producto ordenado (FK a Productos, opcional si es un combo)
    /// </summary>
    [ForeignKey("Producto")]
    public int? ProductoID { get; set; }

    /// <summary>
    /// Combo ordenado (FK a Combos, opcional si es un producto individual)
    /// </summary>
    [ForeignKey("Combo")]
    public int? ComboID { get; set; }

    /// <summary>
    /// Cantidad ordenada
    /// </summary>
    [Required]
    [Range(1, 99, ErrorMessage = "La cantidad debe estar entre 1 y 99")]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario al momento de la orden
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Descuento aplicado a este detalle específico
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 999999.99, ErrorMessage = "El descuento no puede ser negativo")]
    public decimal Descuento { get; set; } = 0;

    /// <summary>
    /// Subtotal calculado automáticamente (Cantidad * PrecioUnitario - Descuento)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal Subtotal { get; private set; }

    /// <summary>
    /// Observaciones específicas para este item
    /// </summary>
    [StringLength(250)]
    public string? Observaciones { get; set; }

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Orden a la que pertenece
    /// </summary>
    public virtual Orden Orden { get; set; } = null!;

    /// <summary>
    /// Producto ordenado (si no es combo)
    /// </summary>
    public virtual Producto? Producto { get; set; }

    /// <summary>
    /// Combo ordenado (si no es producto individual)
    /// </summary>
    public virtual Combo? Combo { get; set; }

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si es un producto individual
    /// </summary>
    [NotMapped]
    public bool EsProducto => ProductoID.HasValue && !ComboID.HasValue;

    /// <summary>
    /// Indica si es un combo
    /// </summary>
    [NotMapped]
    public bool EsCombo => ComboID.HasValue && !ProductoID.HasValue;

    /// <summary>
    /// Nombre del item (producto o combo)
    /// </summary>
    [NotMapped]
    public string NombreItem => EsProducto ? (Producto?.Nombre ?? "Producto desconocido") 
                                          : (Combo?.Nombre ?? "Combo desconocido");

    /// <summary>
    /// Descripción del item
    /// </summary>
    [NotMapped]
    public string? DescripcionItem => EsProducto ? Producto?.Descripcion : Combo?.Descripcion;

    /// <summary>
    /// Categoría del item (solo aplica para productos)
    /// </summary>
    [NotMapped]
    public string? CategoriaItem => Producto?.Categoria?.Nombre;

    /// <summary>
    /// Indica si el item está disponible
    /// </summary>
    [NotMapped]
    public bool EstaDisponible => EsProducto 
        ? (Producto?.EstaDisponible == true && Producto.Inventario?.PuedeSatisfacerOrden(Cantidad) == true)
        : (Combo?.EstaDisponible == true && Combo.HaySuficienteStock(Cantidad));

    /// <summary>
    /// Stock disponible del item
    /// </summary>
    [NotMapped]
    public int StockDisponible => EsProducto 
        ? (Producto?.Inventario?.CantidadDisponible ?? 0)
        : (Combo?.ComboProductos?.Min(cp => cp.Producto?.Inventario?.CantidadDisponible / cp.Cantidad) ?? 0);

    /// <summary>
    /// Tiempo de preparación del item
    /// </summary>
    [NotMapped]
    public int TiempoPreparacion => EsProducto 
        ? (Producto?.TiempoPreparacion ?? 0)
        : (Combo?.TiempoPreparacionTotal ?? 0);

    /// <summary>
    /// Precio total del detalle
    /// </summary>
    [NotMapped]
    public decimal PrecioTotal => Cantidad * PrecioUnitario;

    /// <summary>
    /// Precio total con descuento
    /// </summary>
    [NotMapped]
    public decimal PrecioTotalConDescuento => PrecioTotal - Descuento;

    /// <summary>
    /// Porcentaje de descuento aplicado
    /// </summary>
    [NotMapped]
    public decimal PorcentajeDescuento => PrecioTotal > 0 ? (Descuento / PrecioTotal) * 100 : 0;

    /// <summary>
    /// Precio unitario formateado
    /// </summary>
    [NotMapped]
    public string PrecioUnitarioFormateado => $"RD$ {PrecioUnitario:N2}";

    /// <summary>
    /// Subtotal formateado
    /// </summary>
    [NotMapped]
    public string SubtotalFormateado => $"RD$ {PrecioTotalConDescuento:N2}";

    /// <summary>
    /// Nombre completo para mostrar (incluye cantidad)
    /// </summary>
    [NotMapped]
    public string NombreCompleto => $"{Cantidad}x {NombreItem}";

    /// <summary>
    /// Tipo de item para mostrar
    /// </summary>
    [NotMapped]
    public string TipoItem => EsProducto ? "Producto" : "Combo";

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Actualiza la cantidad del detalle
    /// </summary>
    public void ActualizarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a 0");

        if (nuevaCantidad > 99)
            throw new ArgumentException("La cantidad no puede ser mayor a 99");

        if (Orden?.Estado != "Pendiente")
            throw new InvalidOperationException("Solo se puede modificar la cantidad en órdenes pendientes");

        Cantidad = nuevaCantidad;
        RecalcularSubtotal();
    }

    /// <summary>
    /// Aplica un descuento al detalle
    /// </summary>
    public void AplicarDescuento(decimal montoDescuento)
    {
        if (montoDescuento < 0)
            throw new ArgumentException("El descuento no puede ser negativo");

        if (montoDescuento > PrecioTotal)
            throw new ArgumentException("El descuento no puede ser mayor al precio total");

        if (Orden?.Estado != "Pendiente")
            throw new InvalidOperationException("Solo se puede aplicar descuentos en órdenes pendientes");

        Descuento = montoDescuento;
        RecalcularSubtotal();
    }

    /// <summary>
    /// Aplica un descuento por porcentaje
    /// </summary>
    public void AplicarDescuentoPorcentaje(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100");

        var montoDescuento = PrecioTotal * (porcentaje / 100);
        AplicarDescuento(montoDescuento);
    }

    /// <summary>
    /// Recalcula el subtotal
    /// </summary>
    private void RecalcularSubtotal()
    {
        // En Entity Framework, esto se calcula automáticamente por la columna computed
        // Este método es para cuando se necesite recalcular manualmente
    }

    /// <summary>
    /// Valida que el detalle sea válido
    /// </summary>
    public List<string> ValidarDetalle()
    {
        var errores = new List<string>();

        if (Cantidad <= 0)
            errores.Add("La cantidad debe ser mayor a 0");

        if (Cantidad > 99)
            errores.Add("La cantidad no puede ser mayor a 99");

        if (PrecioUnitario <= 0)
            errores.Add("El precio unitario debe ser mayor a 0");

        if (Descuento < 0)
            errores.Add("El descuento no puede ser negativo");

        if (Descuento > PrecioTotal)
            errores.Add("El descuento no puede ser mayor al precio total");

        if (!ProductoID.HasValue && !ComboID.HasValue)
            errores.Add("Debe especificar un producto o un combo");

        if (ProductoID.HasValue && ComboID.HasValue)
            errores.Add("No puede especificar producto y combo al mismo tiempo");

        // Validar disponibilidad
        if (EsProducto && Producto?.Estado != true)
            errores.Add($"El producto '{NombreItem}' no está activo");

        if (EsCombo && Combo?.Estado != true)
            errores.Add($"El combo '{NombreItem}' no está activo");

        if (!EstaDisponible)
        {
            if (EsProducto)
                errores.Add($"No hay suficiente stock de '{NombreItem}' (disponible: {StockDisponible}, requerido: {Cantidad})");
            else
                errores.Add($"El combo '{NombreItem}' no está disponible completamente");
        }

        return errores;
    }

    /// <summary>
    /// Verifica si se puede aumentar la cantidad
    /// </summary>
    public bool PuedeAumentarCantidad(int incremento = 1)
    {
        var nuevaCantidad = Cantidad + incremento;
        return nuevaCantidad <= 99 && 
               Orden?.Estado == "Pendiente" &&
               (EsProducto 
                   ? Producto?.Inventario?.PuedeSatisfacerOrden(nuevaCantidad) == true
                   : Combo?.HaySuficienteStock(nuevaCantidad) == true);
    }

    /// <summary>
    /// Verifica si se puede reducir la cantidad
    /// </summary>
    public bool PuedeReducirCantidad(int decremento = 1)
    {
        return Cantidad - decremento >= 1 && Orden?.Estado == "Pendiente";
    }

    /// <summary>
    /// Aumenta la cantidad
    /// </summary>
    public bool AumentarCantidad(int incremento = 1)
    {
        if (!PuedeAumentarCantidad(incremento))
            return false;

        ActualizarCantidad(Cantidad + incremento);
        return true;
    }

    /// <summary>
    /// Reduce la cantidad
    /// </summary>
    public bool ReducirCantidad(int decremento = 1)
    {
        if (!PuedeReducirCantidad(decremento))
            return false;

        ActualizarCantidad(Cantidad - decremento);
        return true;
    }

    /// <summary>
    /// Obtiene productos alternativos si no está disponible
    /// </summary>
    public IEnumerable<Producto> ObtenerAlternativas()
    {
        if (EstaDisponible || !EsProducto || Producto?.Categoria?.Productos == null)
            return Enumerable.Empty<Producto>();

        return Producto.Categoria.Productos
            .Where(p => p.ProductoID != ProductoID && 
                       p.EstaDisponible && 
                       p.Inventario?.PuedeSatisfacerOrden(Cantidad) == true)
            .OrderBy(p => Math.Abs(p.Precio - Producto.Precio))
            .Take(3);
    }

    /// <summary>
    /// Obtiene combos alternativos si no está disponible
    /// </summary>
    public IEnumerable<Combo> ObtenerCombosAlternativos()
    {
        if (EstaDisponible || !EsCombo)
            return Enumerable.Empty<Combo>();

        // Buscar combos similares por rango de precio
        var rangoPrecio = PrecioUnitario * 0.2m; // ±20% del precio
        
        return Combo?.GetType().Assembly
            .GetTypes()
            .Where(t => t == typeof(Combo))
            .SelectMany(t => new List<Combo>()) // En un contexto real, esto sería una consulta a la base de datos
            .Where(c => c.ComboID != ComboID &&
                       c.EstaDisponible &&
                       Math.Abs(c.Precio - PrecioUnitario) <= rangoPrecio)
            .OrderBy(c => Math.Abs(c.Precio - PrecioUnitario))
            .Take(3) ?? Enumerable.Empty<Combo>();
    }

    /// <summary>
    /// Calcula información nutricional del detalle
    /// </summary>
    public object CalcularInformacionNutricional()
    {
        if (EsProducto)
        {
            var infoProducto = Producto?.ObtenerInformacionNutricional();
            return new
            {
                Tipo = "Producto",
                Cantidad = Cantidad,
                InformacionUnitaria = infoProducto
            };
        }
        else
        {
            var infoCombo = Combo?.CalcularValorNutricional();
            return new
            {
                Tipo = "Combo",
                Cantidad = Cantidad,
                InformacionUnitaria = infoCombo
            };
        }
    }

    /// <summary>
    /// Genera información detallada del detalle
    /// </summary>
    public object GenerarInformacion()
    {
        return new
        {
            DetalleOrdenID = DetalleOrdenID,
            OrdenNumero = Orden?.NumeroOrden,
            TipoItem = TipoItem,
            NombreItem = NombreItem,
            Descripcion = DescripcionItem,
            Categoria = CategoriaItem,
            Cantidad = Cantidad,
            PrecioUnitario = PrecioUnitarioFormateado,
            PrecioTotal = $"RD$ {PrecioTotal:N2}",
            Descuento = $"RD$ {Descuento:N2}",
            PorcentajeDescuento = $"{PorcentajeDescuento:F1}%",
            Subtotal = SubtotalFormateado,
            TiempoPreparacion = $"{TiempoPreparacion} min",
            EstaDisponible = EstaDisponible,
            StockDisponible = StockDisponible,
            Observaciones = Observaciones
        };
    }

    /// <summary>
    /// Representación en string del detalle de orden
    /// </summary>
    public override string ToString()
    {
        var orden = Orden?.NumeroOrden ?? "Orden desconocida";
        var disponibilidad = EstaDisponible ? "Disponible" : "No disponible";
        return $"{orden}: {NombreCompleto} - {SubtotalFormateado} ({disponibilidad})";
    }
}