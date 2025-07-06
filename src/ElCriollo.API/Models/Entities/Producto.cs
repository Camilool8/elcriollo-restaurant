using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa los productos del menú del restaurante
/// </summary>
[Table("Productos")]
public class Producto
{
    /// <summary>
    /// Identificador único del producto
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductoID { get; set; }

    /// <summary>
    /// Nombre del producto (ej: Pollo Guisado, Mangu, Tres Leches)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del producto
    /// </summary>
    [StringLength(200)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Categoría a la que pertenece el producto (FK a Categorias)
    /// </summary>
    [Required]
    [ForeignKey("Categoria")]
    public int CategoriaID { get; set; }

    /// <summary>
    /// Precio de venta del producto en pesos dominicanos
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal Precio { get; set; }

    /// <summary>
    /// Costo de preparación del producto (para cálculos de rentabilidad)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? CostoPreparacion { get; set; }

    /// <summary>
    /// Tiempo estimado de preparación en minutos
    /// </summary>
    [Range(1, 999, ErrorMessage = "El tiempo de preparación debe estar entre 1 y 999 minutos")]
    public int? TiempoPreparacion { get; set; }

    /// <summary>
    /// Ruta de la imagen del producto
    /// </summary>
    [StringLength(255)]
    public string? Imagen { get; set; }

    /// <summary>
    /// Indica si el producto está activo en el menú
    /// </summary>
    [Required]
    public bool Estado { get; set; } = true;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Categoría a la que pertenece el producto
    /// </summary>
    public virtual Categoria Categoria { get; set; } = null!;

    /// <summary>
    /// Información de inventario del producto
    /// </summary>
    public virtual Inventario? Inventario { get; set; }

    /// <summary>
    /// Combos que incluyen este producto
    /// </summary>
    public virtual ICollection<ComboProducto> ComboProductos { get; set; } = new List<ComboProducto>();

    /// <summary>
    /// Detalles de órdenes que incluyen este producto
    /// </summary>
    public virtual ICollection<DetalleOrden> DetalleOrdenes { get; set; } = new List<DetalleOrden>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Margen de ganancia del producto (si se conoce el costo)
    /// </summary>
    [NotMapped]
    public decimal? MargenGanancia => CostoPreparacion.HasValue && CostoPreparacion > 0
        ? ((Precio - CostoPreparacion.Value) / CostoPreparacion.Value) * 100
        : null;

    /// <summary>
    /// Ganancia por unidad vendida
    /// </summary>
    [NotMapped]
    public decimal? GananciaPorUnidad => CostoPreparacion.HasValue
        ? Precio - CostoPreparacion.Value
        : null;

    /// <summary>
    /// Indica si el producto tiene stock disponible
    /// </summary>
    [NotMapped]
    public bool TieneStock => Inventario?.CantidadDisponible > 0;

    /// <summary>
    /// Cantidad disponible en inventario
    /// </summary>
    [NotMapped]
    public int StockDisponible => Inventario?.CantidadDisponible ?? 0;

    /// <summary>
    /// Indica si el producto tiene stock bajo
    /// </summary>
    [NotMapped]
    public bool TieneStockBajo => Inventario?.CantidadDisponible <= Inventario?.CantidadMinima;

    /// <summary>
    /// Indica si el producto está disponible para venta
    /// </summary>
    [NotMapped]
    public bool EstaDisponible => Estado && TieneStock;

    /// <summary>
    /// Tiempo de preparación formateado
    /// </summary>
    [NotMapped]
    public string TiempoPreparacionFormateado => TiempoPreparacion.HasValue
        ? $"{TiempoPreparacion} min"
        : "No especificado";

    /// <summary>
    /// Precio formateado en pesos dominicanos
    /// </summary>
    [NotMapped]
    public string PrecioFormateado => $"RD$ {Precio:N2}";

    /// <summary>
    /// Total de veces que se ha ordenado el producto
    /// </summary>
    [NotMapped]
    public int TotalOrdenado => DetalleOrdenes?.Sum(d => d.Cantidad) ?? 0;

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Activa el producto en el menú
    /// </summary>
    public void Activar()
    {
        Estado = true;
    }

    /// <summary>
    /// Desactiva el producto del menú
    /// </summary>
    public void Desactivar()
    {
        Estado = false;
    }

    /// <summary>
    /// Actualiza el precio del producto
    /// </summary>
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0)
            throw new ArgumentException("El precio debe ser mayor a 0");

        Precio = nuevoPrecio;
    }

    /// <summary>
    /// Verifica si es un plato típico dominicano
    /// </summary>
    public bool EsPlatoDominicano()
    {
        var platosTypicos = new[]
        {
            "Pollo Guisado", "Pernil", "Rabo Encendido", "Chivo Guisado",
            "Mangú", "Tres Golpes", "Sancocho", "Mondongo", "Pescao Frito",
            "Moro", "Habichuelas", "Tostones", "Maduros", "Morir Soñando"
        };

        return platosTypicos.Any(plato => 
            Nombre.Contains(plato, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica si es una bebida tradicional dominicana
    /// </summary>
    public bool EsBebidaDominicana()
    {
        var bebidasDominicanas = new[]
        {
            "Morir Soñando", "Chinola", "Tamarindo", "Mamajuana", "Presidente"
        };

        return bebidasDominicanas.Any(bebida => 
            Nombre.Contains(bebida, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtiene el nivel de dificultad de preparación basado en el tiempo
    /// </summary>
    public string ObtenerNivelDificultad()
    {
        return TiempoPreparacion switch
        {
            <= 10 => "Fácil",
            <= 30 => "Moderado",
            <= 60 => "Complejo",
            _ => "Muy Complejo"
        };
    }

    /// <summary>
    /// Verifica si el producto puede ser parte de un combo
    /// </summary>
    public bool PuedeSerParteDeCombo()
    {
        return Estado && EstaDisponible && !string.IsNullOrEmpty(Nombre);
    }

    /// <summary>
    /// Calcula el precio con descuento
    /// </summary>
    public decimal CalcularPrecioConDescuento(decimal porcentajeDescuento)
    {
        if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
            throw new ArgumentException("El descuento debe estar entre 0 y 100");

        return Precio * (1 - porcentajeDescuento / 100);
    }

    /// <summary>
    /// Obtiene información nutricional básica (simulada para productos dominicanos)
    /// </summary>
    public object ObtenerInformacionNutricional()
    {
        // Información nutricional simulada basada en comida dominicana típica
        return Categoria?.Nombre switch
        {
            "Platos Principales" => new { Calorias = "450-650", Proteinas = "Alto", Carbohidratos = "Moderado" },
            "Frituras" => new { Calorias = "300-500", Proteinas = "Bajo", Carbohidratos = "Alto" },
            "Bebidas" => new { Calorias = "100-200", Proteinas = "Bajo", Carbohidratos = "Moderado" },
            "Postres" => new { Calorias = "250-400", Proteinas = "Bajo", Carbohidratos = "Alto" },
            _ => new { Calorias = "Variable", Proteinas = "Variable", Carbohidratos = "Variable" }
        };
    }

    /// <summary>
    /// Verifica si el producto necesita reabastecimiento
    /// </summary>
    public bool NecesitaReabastecimiento()
    {
        return Inventario?.CantidadDisponible <= (Inventario?.CantidadMinima ?? 0);
    }

    /// <summary>
    /// Obtiene estadísticas del producto
    /// </summary>
    public object ObtenerEstadisticas()
    {
        return new
        {
            Nombre = Nombre,
            Categoria = Categoria?.Nombre,
            Precio = PrecioFormateado,
            TiempoPreparacion = TiempoPreparacionFormateado,
            Stock = StockDisponible,
            TotalOrdenado = TotalOrdenado,
            MargenGanancia = MargenGanancia?.ToString("F2") + "%" ?? "No calculado",
            Estado = Estado ? "Activo" : "Inactivo",
            Disponible = EstaDisponible ? "Sí" : "No"
        };
    }

    /// <summary>
    /// Representación en string del producto
    /// </summary>
    public override string ToString()
    {
        var categoria = Categoria?.Nombre ?? "Sin categoría";
        var disponibilidad = EstaDisponible ? "Disponible" : "No disponible";
        return $"{Nombre} ({categoria}) - {PrecioFormateado} - {disponibilidad}";
    }
}