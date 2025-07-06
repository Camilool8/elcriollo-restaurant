using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa las categorías de productos del menú
/// </summary>
[Table("Categorias")]
public class Categoria
{
    /// <summary>
    /// Identificador único de la categoría
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoriaID { get; set; }

    /// <summary>
    /// Nombre de la categoría (ej: Platos Principales, Bebidas, Postres)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la categoría
    /// </summary>
    [StringLength(200)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Indica si la categoría está activa en el menú
    /// </summary>
    [Required]
    public bool Estado { get; set; } = true;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Productos que pertenecen a esta categoría
    /// </summary>
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Cantidad total de productos en la categoría
    /// </summary>
    [NotMapped]
    public int TotalProductos => Productos?.Count ?? 0;

    /// <summary>
    /// Cantidad de productos activos en la categoría
    /// </summary>
    [NotMapped]
    public int ProductosActivos => Productos?.Count(p => p.Estado) ?? 0;

    /// <summary>
    /// Indica si la categoría tiene productos disponibles
    /// </summary>
    [NotMapped]
    public bool TieneProductosDisponibles => ProductosActivos > 0;

    /// <summary>
    /// Precio promedio de los productos en la categoría
    /// </summary>
    [NotMapped]
    public decimal PrecioPromedio => Productos?.Where(p => p.Estado).Any() == true
        ? Productos.Where(p => p.Estado).Average(p => p.Precio)
        : 0;

    /// <summary>
    /// Producto más barato de la categoría
    /// </summary>
    [NotMapped]
    public Producto? ProductoMasBarato => Productos?
        .Where(p => p.Estado)
        .OrderBy(p => p.Precio)
        .FirstOrDefault();

    /// <summary>
    /// Producto más caro de la categoría
    /// </summary>
    [NotMapped]
    public Producto? ProductoMasCaro => Productos?
        .Where(p => p.Estado)
        .OrderByDescending(p => p.Precio)
        .FirstOrDefault();

    // ============================================================================
    // PROPIEDADES ALIAS PARA COMPATIBILIDAD
    // ============================================================================

    /// <summary>
    /// Alias para NombreCategoria (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string NombreCategoria => Nombre;

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Activa la categoría
    /// </summary>
    public void Activar()
    {
        Estado = true;
    }

    /// <summary>
    /// Desactiva la categoría
    /// </summary>
    public void Desactivar()
    {
        Estado = false;
    }

    /// <summary>
    /// Obtiene productos de la categoría con stock disponible
    /// </summary>
    public IEnumerable<Producto> ObtenerProductosConStock()
    {
        return Productos?
            .Where(p => p.Estado && p.Inventario?.CantidadDisponible > 0) 
            ?? Enumerable.Empty<Producto>();
    }

    /// <summary>
    /// Obtiene productos de la categoría ordenados por precio
    /// </summary>
    public IEnumerable<Producto> ObtenerProductosOrdenadosPorPrecio(bool ascendente = true)
    {
        var productos = Productos?.Where(p => p.Estado) ?? Enumerable.Empty<Producto>();
        
        return ascendente 
            ? productos.OrderBy(p => p.Precio)
            : productos.OrderByDescending(p => p.Precio);
    }

    /// <summary>
    /// Obtiene productos de la categoría por rango de precio
    /// </summary>
    public IEnumerable<Producto> ObtenerProductosPorRangoPrecio(decimal precioMinimo, decimal precioMaximo)
    {
        return Productos?
            .Where(p => p.Estado && p.Precio >= precioMinimo && p.Precio <= precioMaximo)
            ?? Enumerable.Empty<Producto>();
    }

    /// <summary>
    /// Verifica si es una categoría de comida dominicana típica
    /// </summary>
    public bool EsCategoriaComidaDominicana()
    {
        var categoriasComidaDominicana = new[]
        {
            "Platos Principales",
            "Acompañamientos", 
            "Frituras",
            "Desayunos",
            "Sopas",
            "Mariscos"
        };

        return categoriasComidaDominicana.Contains(Nombre);
    }

    /// <summary>
    /// Verifica si es una categoría de bebidas
    /// </summary>
    public bool EsCategoriaBebidas()
    {
        return Nombre.Equals("Bebidas", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica si es una categoría de postres
    /// </summary>
    public bool EsCategoriaPostres()
    {
        return Nombre.Equals("Postres", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene estadísticas de la categoría
    /// </summary>
    public object ObtenerEstadisticas()
    {
        return new
        {
            Nombre = Nombre,
            TotalProductos = TotalProductos,
            ProductosActivos = ProductosActivos,
            PrecioPromedio = PrecioPromedio,
            PrecioMinimo = ProductoMasBarato?.Precio ?? 0,
            PrecioMaximo = ProductoMasCaro?.Precio ?? 0,
            ProductosConStock = ObtenerProductosConStock().Count(),
            Estado = Estado ? "Activa" : "Inactiva"
        };
    }

    /// <summary>
    /// Obtiene el rango de precios de la categoría formateado
    /// </summary>
    public string ObtenerRangoPrecios()
    {
        if (!TieneProductosDisponibles)
            return "Sin productos disponibles";
            
        var min = ProductoMasBarato?.Precio ?? 0;
        var max = ProductoMasCaro?.Precio ?? 0;
        
        if (min == max)
            return $"RD$ {min:N2}";
            
        return $"RD$ {min:N2} - RD$ {max:N2}";
    }

    /// <summary>
    /// Representación en string de la categoría
    /// </summary>
    public override string ToString()
    {
        var estado = Estado ? "Activa" : "Inactiva";
        return $"{Nombre} ({ProductosActivos} productos activos) - {estado}";
    }
}