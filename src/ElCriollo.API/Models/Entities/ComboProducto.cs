using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa la relación entre combos y productos (qué productos incluye cada combo)
/// </summary>
[Table("ComboProductos")]
public class ComboProducto
{
    /// <summary>
    /// Identificador único de la relación combo-producto
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ComboProductoID { get; set; }

    /// <summary>
    /// Combo al que pertenece este producto (FK a Combos)
    /// </summary>
    [Required]
    [ForeignKey("Combo")]
    public int ComboID { get; set; }

    /// <summary>
    /// Producto incluido en el combo (FK a Productos)
    /// </summary>
    [Required]
    [ForeignKey("Producto")]
    public int ProductoID { get; set; }

    /// <summary>
    /// Cantidad de este producto incluida en el combo
    /// </summary>
    [Required]
    [Range(1, 99, ErrorMessage = "La cantidad debe estar entre 1 y 99")]
    public int Cantidad { get; set; } = 1;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Combo al que pertenece
    /// </summary>
    public virtual Combo Combo { get; set; } = null!;

    /// <summary>
    /// Producto incluido
    /// </summary>
    public virtual Producto Producto { get; set; } = null!;

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Precio total de este producto en el combo (precio unitario × cantidad)
    /// </summary>
    [NotMapped]
    public decimal PrecioTotal => (Producto?.Precio ?? 0) * Cantidad;

    /// <summary>
    /// Indica si el producto está disponible en la cantidad requerida
    /// </summary>
    [NotMapped]
    public bool EstaDisponible => Producto?.EstaDisponible == true && 
        Producto.Inventario?.PuedeSatisfacerOrden(Cantidad) == true;

    /// <summary>
    /// Stock disponible del producto
    /// </summary>
    [NotMapped]
    public int StockDisponible => Producto?.Inventario?.CantidadDisponible ?? 0;

    /// <summary>
    /// Indica si hay suficiente stock para la cantidad requerida
    /// </summary>
    [NotMapped]
    public bool HaySuficienteStock => StockDisponible >= Cantidad;

    /// <summary>
    /// Cantidad faltante si no hay suficiente stock
    /// </summary>
    [NotMapped]
    public int CantidadFaltante => Math.Max(Cantidad - StockDisponible, 0);

    /// <summary>
    /// Tiempo de preparación total considerando la cantidad
    /// </summary>
    [NotMapped]
    public int TiempoPreparacionTotal => (Producto?.TiempoPreparacion ?? 0);

    /// <summary>
    /// Categoría del producto
    /// </summary>
    [NotMapped]
    public string? CategoriaProducto => Producto?.Categoria?.Nombre;

    /// <summary>
    /// Nombre completo para mostrar (incluye cantidad)
    /// </summary>
    [NotMapped]
    public string NombreCompleto => $"{Cantidad}x {Producto?.Nombre}";

    /// <summary>
    /// Precio total formateado
    /// </summary>
    [NotMapped]
    public string PrecioTotalFormateado => $"RD$ {PrecioTotal:N2}";

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Actualiza la cantidad del producto en el combo
    /// </summary>
    public void ActualizarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a 0");

        if (nuevaCantidad > 99)
            throw new ArgumentException("La cantidad no puede ser mayor a 99");

        Cantidad = nuevaCantidad;
    }

    /// <summary>
    /// Verifica si se puede aumentar la cantidad
    /// </summary>
    public bool PuedeAumentarCantidad(int incremento = 1)
    {
        var nuevaCantidad = Cantidad + incremento;
        return nuevaCantidad <= 99 && 
               Producto?.Inventario?.PuedeSatisfacerOrden(nuevaCantidad) == true;
    }

    /// <summary>
    /// Verifica si se puede reducir la cantidad
    /// </summary>
    public bool PuedeReducirCantidad(int decremento = 1)
    {
        return Cantidad - decremento >= 1;
    }

    /// <summary>
    /// Aumenta la cantidad del producto en el combo
    /// </summary>
    public bool AumentarCantidad(int incremento = 1)
    {
        if (!PuedeAumentarCantidad(incremento))
            return false;

        Cantidad += incremento;
        return true;
    }

    /// <summary>
    /// Reduce la cantidad del producto en el combo
    /// </summary>
    public bool ReducirCantidad(int decremento = 1)
    {
        if (!PuedeReducirCantidad(decremento))
            return false;

        Cantidad -= decremento;
        return true;
    }

    /// <summary>
    /// Valida que la configuración del combo-producto sea válida
    /// </summary>
    public List<string> ValidarConfiguracion()
    {
        var errores = new List<string>();

        if (Cantidad <= 0)
            errores.Add("La cantidad debe ser mayor a 0");

        if (Cantidad > 99)
            errores.Add("La cantidad no puede ser mayor a 99");

        if (Producto?.Estado != true)
            errores.Add($"El producto '{Producto?.Nombre}' no está activo");

        if (!HaySuficienteStock)
            errores.Add($"No hay suficiente stock de '{Producto?.Nombre}' (disponible: {StockDisponible}, requerido: {Cantidad})");

        if (ComboID <= 0)
            errores.Add("El combo debe estar especificado");

        if (ProductoID <= 0)
            errores.Add("El producto debe estar especificado");

        return errores;
    }

    /// <summary>
    /// Calcula el ahorro por este producto en el combo vs precio individual
    /// </summary>
    public decimal CalcularAhorro()
    {
        if (Combo?.Descuento == null || Combo.PrecioSinDescuento == 0)
            return 0;

        // Proporción de ahorro basada en el precio de este producto vs total del combo
        var proporcionProducto = PrecioTotal / Combo.PrecioSinDescuento;
        return Combo.Descuento * proporcionProducto;
    }

    /// <summary>
    /// Verifica si el producto es esencial para el combo (ej: plato principal)
    /// </summary>
    public bool EsProductoEsencial()
    {
        var categoriasEsenciales = new[]
        {
            "Platos Principales",
            "Desayunos"
        };

        return categoriasEsenciales.Contains(CategoriaProducto, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica si el producto es un acompañamiento
    /// </summary>
    public bool EsAcompanamiento()
    {
        var categoriasAcompanamiento = new[]
        {
            "Acompañamientos",
            "Frituras"
        };

        return categoriasAcompanamiento.Contains(CategoriaProducto, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica si el producto es una bebida
    /// </summary>
    public bool EsBebida()
    {
        return CategoriaProducto?.Equals("Bebidas", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Obtiene sugerencias de productos alternativos si no está disponible
    /// </summary>
    public IEnumerable<Producto> ObtenerAlternativas()
    {
        if (EstaDisponible || Producto?.Categoria?.Productos == null)
            return Enumerable.Empty<Producto>();

        return Producto.Categoria.Productos
            .Where(p => p.ProductoID != ProductoID && 
                       p.EstaDisponible && 
                       p.Inventario?.PuedeSatisfacerOrden(Cantidad) == true)
            .OrderBy(p => Math.Abs(p.Precio - Producto.Precio))
            .Take(3);
    }

    /// <summary>
    /// Genera información detallada del producto en el combo
    /// </summary>
    public object GenerarInformacion()
    {
        return new
        {
            ComboNombre = Combo?.Nombre,
            ProductoNombre = Producto?.Nombre,
            Categoria = CategoriaProducto,
            Cantidad = Cantidad,
            PrecioUnitario = Producto?.PrecioFormateado,
            PrecioTotal = PrecioTotalFormateado,
            EstaDisponible = EstaDisponible,
            StockDisponible = StockDisponible,
            HaySuficienteStock = HaySuficienteStock,
            CantidadFaltante = CantidadFaltante > 0 ? CantidadFaltante : (int?)null,
            TiempoPreparacion = $"{TiempoPreparacionTotal} min",
            EsEsencial = EsProductoEsencial(),
            EsAcompanamiento = EsAcompanamiento(),
            EsBebida = EsBebida(),
            Ahorro = $"RD$ {CalcularAhorro():N2}"
        };
    }

    /// <summary>
    /// Representación en string de la relación combo-producto
    /// </summary>
    public override string ToString()
    {
        var combo = Combo?.Nombre ?? "Combo desconocido";
        var producto = Producto?.Nombre ?? "Producto desconocido";
        var disponibilidad = EstaDisponible ? "Disponible" : "No disponible";
        
        return $"{combo}: {NombreCompleto} - {PrecioTotalFormateado} ({disponibilidad})";
    }
}