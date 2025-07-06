using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa el control de inventario de productos
/// </summary>
[Table("Inventario")]
public class Inventario
{
    /// <summary>
    /// Identificador único del registro de inventario
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InventarioID { get; set; }

    /// <summary>
    /// Producto al que corresponde este inventario (FK a Productos)
    /// </summary>
    [Required]
    [ForeignKey("Producto")]
    public int ProductoID { get; set; }

    /// <summary>
    /// Cantidad disponible actualmente en stock
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible no puede ser negativa")]
    public int CantidadDisponible { get; set; } = 0;

    /// <summary>
    /// Cantidad mínima que debe mantenerse en stock (punto de reorden)
    /// </summary>
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad mínima no puede ser negativa")]
    public int CantidadMinima { get; set; } = 5;

    /// <summary>
    /// Fecha y hora de la última actualización del inventario
    /// </summary>
    [Required]
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Producto al que corresponde este inventario
    /// </summary>
    public virtual Producto Producto { get; set; } = null!;

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si el stock está bajo (igual o por debajo del mínimo)
    /// </summary>
    [NotMapped]
    public bool StockBajo => CantidadDisponible <= CantidadMinima;

    /// <summary>
    /// Indica si el producto está agotado
    /// </summary>
    [NotMapped]
    public bool Agotado => CantidadDisponible == 0;

    /// <summary>
    /// Indica si hay stock disponible
    /// </summary>
    [NotMapped]
    public bool HayStock => CantidadDisponible > 0;

    /// <summary>
    /// Porcentaje de stock respecto al mínimo
    /// </summary>
    [NotMapped]
    public decimal PorcentajeStock => CantidadMinima > 0 
        ? (decimal)CantidadDisponible / CantidadMinima * 100 
        : 0;

    /// <summary>
    /// Nivel de stock en formato descriptivo
    /// </summary>
    [NotMapped]
    public string NivelStock => CantidadDisponible switch
    {
        0 => "Agotado",
        var n when n <= CantidadMinima => "Stock Bajo",
        var n when n <= CantidadMinima * 2 => "Stock Moderado",
        _ => "Stock Suficiente"
    };

    /// <summary>
    /// Color del indicador de stock para UI
    /// </summary>
    [NotMapped]
    public string ColorIndicador => CantidadDisponible switch
    {
        0 => "red",
        var n when n <= CantidadMinima => "orange",
        var n when n <= CantidadMinima * 2 => "yellow",
        _ => "green"
    };

    /// <summary>
    /// Tiempo transcurrido desde la última actualización
    /// </summary>
    [NotMapped]
    public TimeSpan TiempoDesdePocaUltimaActualizacion => DateTime.UtcNow - UltimaActualizacion;

    /// <summary>
    /// Indica si la información de inventario está desactualizada (más de 24 horas)
    /// </summary>
    [NotMapped]
    public bool EstaDesactualizado => TiempoDesdePocaUltimaActualizacion.TotalHours > 24;

    /// <summary>
    /// Cantidad recomendada para reordenar
    /// </summary>
    [NotMapped]
    public int CantidadRecomendadaReorden => Math.Max(CantidadMinima * 3 - CantidadDisponible, 0);

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Actualiza la cantidad disponible en stock
    /// </summary>
    public void ActualizarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad < 0)
            throw new ArgumentException("La cantidad no puede ser negativa");

        CantidadDisponible = nuevaCantidad;
        UltimaActualizacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Reduce el stock por una venta
    /// </summary>
    public bool ReducirStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad a reducir debe ser positiva");

        if (cantidad > CantidadDisponible)
            return false; // No hay suficiente stock

        CantidadDisponible -= cantidad;
        UltimaActualizacion = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Aumenta el stock por una compra o reabastecimiento
    /// </summary>
    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad a aumentar debe ser positiva");

        CantidadDisponible += cantidad;
        UltimaActualizacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Establece la cantidad mínima de stock
    /// </summary>
    public void EstablecerCantidadMinima(int nuevaCantidadMinima)
    {
        if (nuevaCantidadMinima < 0)
            throw new ArgumentException("La cantidad mínima no puede ser negativa");

        CantidadMinima = nuevaCantidadMinima;
        UltimaActualizacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si se puede satisfacer una orden de cierta cantidad
    /// </summary>
    public bool PuedeSatisfacerOrden(int cantidadSolicitada)
    {
        return cantidadSolicitada > 0 && cantidadSolicitada <= CantidadDisponible;
    }

    /// <summary>
    /// Calcula cuántas unidades faltan para alcanzar el stock mínimo
    /// </summary>
    public int CalcularDeficitMinimo()
    {
        return Math.Max(CantidadMinima - CantidadDisponible, 0);
    }

    /// <summary>
    /// Obtiene una alerta de inventario si es necesaria
    /// </summary>
    public string? ObtenerAlerta()
    {
        if (Agotado)
            return $"⚠️ URGENTE: {Producto?.Nombre} está AGOTADO";
        
        if (StockBajo)
            return $"⚠️ ATENCIÓN: {Producto?.Nombre} tiene stock bajo ({CantidadDisponible} unidades)";
        
        if (EstaDesactualizado)
            return $"⚠️ INFO: Inventario de {Producto?.Nombre} no actualizado desde {UltimaActualizacion:dd/MM/yyyy HH:mm}";

        return null;
    }

    /// <summary>
    /// Genera un reporte de stock del producto
    /// </summary>
    public object GenerarReporteStock()
    {
        return new
        {
            Producto = Producto?.Nombre,
            CantidadDisponible = CantidadDisponible,
            CantidadMinima = CantidadMinima,
            NivelStock = NivelStock,
            PorcentajeStock = Math.Round(PorcentajeStock, 2),
            UltimaActualizacion = UltimaActualizacion.ToString("dd/MM/yyyy HH:mm"),
            EstaDesactualizado = EstaDesactualizado,
            CantidadRecomendadaReorden = CantidadRecomendadaReorden,
            Alerta = ObtenerAlerta()
        };
    }

    /// <summary>
    /// Simula el uso de ingredientes para platos dominicanos
    /// </summary>
    public void SimularConsumoPlato()
    {
        // Simulación de consumo de ingredientes según el tipo de producto
        var consumo = Producto?.Categoria?.Nombre switch
        {
            "Platos Principales" => 1, // Un plato consume 1 unidad de ingrediente principal
            "Acompañamientos" => 1,
            "Frituras" => 1,
            "Bebidas" => 1,
            "Postres" => 1,
            "Desayunos" => 1,
            "Sopas" => 1,
            "Mariscos" => 1,
            _ => 1
        };

        ReducirStock(consumo);
    }

    /// <summary>
    /// Obtiene recomendaciones para el inventario
    /// </summary>
    public List<string> ObtenerRecomendaciones()
    {
        var recomendaciones = new List<string>();

        if (Agotado)
        {
            recomendaciones.Add("🔴 Rebastecer INMEDIATAMENTE - Producto agotado");
        }
        else if (StockBajo)
        {
            recomendaciones.Add($"🟠 Reabastecer pronto - Solo quedan {CantidadDisponible} unidades");
            recomendaciones.Add($"💡 Cantidad recomendada: {CantidadRecomendadaReorden} unidades");
        }
        else if (PorcentajeStock < 150)
        {
            recomendaciones.Add("🟡 Considerar reabastecimiento en los próximos días");
        }

        if (EstaDesactualizado)
        {
            recomendaciones.Add("📅 Actualizar conteo de inventario - Información antigua");
        }

        if (CantidadMinima == 0)
        {
            recomendaciones.Add("⚙️ Establecer cantidad mínima de stock");
        }

        return recomendaciones;
    }

    /// <summary>
    /// Representación en string del inventario
    /// </summary>
    public override string ToString()
    {
        var producto = Producto?.Nombre ?? "Producto desconocido";
        return $"{producto}: {CantidadDisponible} unidades ({NivelStock})";
    }
}