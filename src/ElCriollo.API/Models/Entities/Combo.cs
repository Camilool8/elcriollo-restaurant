using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los combos especiales del restaurante
/// </summary>
[Table("Combos")]
public class Combo
{
    /// <summary>
    /// Identificador único del combo
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ComboID { get; set; }

    /// <summary>
    /// Nombre del combo (ej: La Bandera Dominicana, Combo Criollo)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del combo y qué incluye
    /// </summary>
    [StringLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio final del combo en pesos dominicanos
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal Precio { get; set; }

    /// <summary>
    /// Descuento aplicado al combo (diferencia entre suma de productos individuales y precio final)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 999999.99, ErrorMessage = "El descuento no puede ser negativo")]
    public decimal Descuento { get; set; } = 0;

    /// <summary>
    /// Indica si el combo está activo en el menú
    /// </summary>
    [Required]
    public bool Estado { get; set; } = true;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Productos que componen este combo
    /// </summary>
    public virtual ICollection<ComboProducto> ComboProductos { get; set; } = new List<ComboProducto>();

    /// <summary>
    /// Detalles de órdenes que incluyen este combo
    /// </summary>
    public virtual ICollection<DetalleOrden> DetalleOrdenes { get; set; } = new List<DetalleOrden>();

    /// <summary>
    /// Facturas que incluyen este combo
    /// </summary>
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Precio total de los productos individuales (sin descuento)
    /// </summary>
    [NotMapped]
    public decimal PrecioSinDescuento => ComboProductos?
        .Sum(cp => (cp.Producto?.Precio ?? 0) * cp.Cantidad) ?? 0;

    /// <summary>
    /// Porcentaje de descuento del combo
    /// </summary>
    [NotMapped]
    public decimal PorcentajeDescuento => PrecioSinDescuento > 0 
        ? (Descuento / PrecioSinDescuento) * 100 
        : 0;

    /// <summary>
    /// Ahorro que obtiene el cliente comprando el combo
    /// </summary>
    [NotMapped]
    public decimal Ahorro => PrecioSinDescuento - Precio;

    /// <summary>
    /// Cantidad de productos diferentes en el combo
    /// </summary>
    [NotMapped]
    public int CantidadProductos => ComboProductos?.Count ?? 0;

    /// <summary>
    /// Cantidad total de items en el combo (considerando cantidades)
    /// </summary>
    [NotMapped]
    public int TotalItems => ComboProductos?.Sum(cp => cp.Cantidad) ?? 0;

    /// <summary>
    /// Indica si todos los productos del combo están disponibles
    /// </summary>
    [NotMapped]
    public bool EstaDisponible => Estado && 
        ComboProductos?.All(cp => cp.Producto?.EstaDisponible == true) == true;

    /// <summary>
    /// Productos que no están disponibles en el combo
    /// </summary>
    [NotMapped]
    public IEnumerable<Producto> ProductosNoDisponibles => ComboProductos?
        .Where(cp => cp.Producto?.EstaDisponible != true)
        .Select(cp => cp.Producto!)
        .Where(p => p != null) ?? Enumerable.Empty<Producto>();

    /// <summary>
    /// Tiempo estimado total de preparación del combo
    /// </summary>
    [NotMapped]
    public int TiempoPreparacionTotal => ComboProductos?
        .Max(cp => cp.Producto?.TiempoPreparacion ?? 0) ?? 0;

    /// <summary>
    /// Categorías de productos incluidas en el combo
    /// </summary>
    [NotMapped]
    public IEnumerable<string> CategoriasIncluidas => ComboProductos?
        .Select(cp => cp.Producto?.Categoria?.Nombre)
        .Where(c => !string.IsNullOrEmpty(c))
        .Cast<string>()
        .Distinct() ?? Enumerable.Empty<string>();

    /// <summary>
    /// Precio formateado en pesos dominicanos
    /// </summary>
    [NotMapped]
    public string PrecioFormateado => $"RD$ {Precio:N2}";

    /// <summary>
    /// Descuento formateado
    /// </summary>
    [NotMapped]
    public string DescuentoFormateado => $"RD$ {Descuento:N2}";

    /// <summary>
    /// Total de veces que se ha ordenado el combo
    /// </summary>
    [NotMapped]
    public int TotalOrdenado => DetalleOrdenes?.Sum(d => d.Cantidad) ?? 0;

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Activa el combo en el menú
    /// </summary>
    public void Activar()
    {
        Estado = true;
    }

    /// <summary>
    /// Desactiva el combo del menú
    /// </summary>
    public void Desactivar()
    {
        Estado = false;
    }

    /// <summary>
    /// Actualiza el precio del combo
    /// </summary>
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0)
            throw new ArgumentException("El precio debe ser mayor a 0");

        Precio = nuevoPrecio;
        RecalcularDescuento();
    }

    /// <summary>
    /// Recalcula el descuento basado en el precio actual y los productos
    /// </summary>
    public void RecalcularDescuento()
    {
        var precioSinDescuento = PrecioSinDescuento;
        Descuento = precioSinDescuento > Precio ? precioSinDescuento - Precio : 0;
    }

    /// <summary>
    /// Verifica si el combo incluye un producto específico
    /// </summary>
    public bool IncluyeProducto(int productoId)
    {
        return ComboProductos?.Any(cp => cp.ProductoID == productoId) ?? false;
    }

    /// <summary>
    /// Verifica si el combo incluye productos de una categoría específica
    /// </summary>
    public bool IncluyeCategoria(string nombreCategoria)
    {
        return ComboProductos?.Any(cp => 
            cp.Producto?.Categoria?.Nombre?.Equals(nombreCategoria, StringComparison.OrdinalIgnoreCase) == true) ?? false;
    }

    /// <summary>
    /// Obtiene la lista de productos con sus cantidades
    /// </summary>
    public List<string> ObtenerListaProductos()
    {
        return ComboProductos?
            .Select(cp => $"{cp.Cantidad}x {cp.Producto?.Nombre}")
            .ToList() ?? new List<string>();
    }

    /// <summary>
    /// Verifica si es un combo de comida dominicana
    /// </summary>
    public bool EsComboDominicano()
    {
        var categoriasDominicanas = new[]
        {
            "Platos Principales", "Acompañamientos", "Frituras", 
            "Desayunos", "Sopas", "Mariscos"
        };

        return CategoriasIncluidas.Any(cat => 
            categoriasDominicanas.Contains(cat, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Calcula el valor nutricional aproximado del combo
    /// </summary>
    public object CalcularValorNutricional()
    {
        // Simulación de valores nutricionales para combos dominicanos
        var tieneComidaPrincipal = IncluyeCategoria("Platos Principales");
        var tieneAcompanamiento = IncluyeCategoria("Acompañamientos");
        var tieneFritura = IncluyeCategoria("Frituras");
        var tieneBebida = IncluyeCategoria("Bebidas");

        var calorias = 0;
        if (tieneComidaPrincipal) calorias += 500;
        if (tieneAcompanamiento) calorias += 200;
        if (tieneFritura) calorias += 300;
        if (tieneBebida) calorias += 150;

        return new
        {
            CaloriasAproximadas = calorias,
            Proteinas = tieneComidaPrincipal ? "Alto" : "Moderado",
            Carbohidratos = (tieneAcompanamiento || tieneFritura) ? "Alto" : "Moderado",
            EsCompletoNutricionalmente = tieneComidaPrincipal && tieneAcompanamiento
        };
    }

    /// <summary>
    /// Verifica si hay suficiente stock para el combo
    /// </summary>
    public bool HaySuficienteStock(int cantidad = 1)
    {
        return ComboProductos?.All(cp => 
            cp.Producto?.Inventario?.PuedeSatisfacerOrden(cp.Cantidad * cantidad) == true) ?? false;
    }

    /// <summary>
    /// Obtiene los productos que no tienen suficiente stock
    /// </summary>
    public List<string> ObtenerProductosSinStock(int cantidad = 1)
    {
        return ComboProductos?
            .Where(cp => cp.Producto?.Inventario?.PuedeSatisfacerOrden(cp.Cantidad * cantidad) != true)
            .Select(cp => cp.Producto?.Nombre ?? "Producto desconocido")
            .ToList() ?? new List<string>();
    }

    /// <summary>
    /// Genera un resumen del combo
    /// </summary>
    public object GenerarResumen()
    {
        return new
        {
            Nombre = Nombre,
            Descripcion = Descripcion,
            Precio = PrecioFormateado,
            PrecioSinDescuento = $"RD$ {PrecioSinDescuento:N2}",
            Ahorro = $"RD$ {Ahorro:N2}",
            PorcentajeDescuento = $"{PorcentajeDescuento:F1}%",
            CantidadProductos = CantidadProductos,
            TotalItems = TotalItems,
            TiempoPreparacion = $"{TiempoPreparacionTotal} min",
            EstaDisponible = EstaDisponible,
            EsComboDominicano = EsComboDominicano(),
            Estado = Estado ? "Activo" : "Inactivo"
        };
    }

    /// <summary>
    /// Representación en string del combo
    /// </summary>
    public override string ToString()
    {
        var productos = CantidadProductos;
        var disponibilidad = EstaDisponible ? "Disponible" : "No disponible";
        return $"{Nombre} ({productos} productos) - {PrecioFormateado} - {disponibilidad}";
    }
}