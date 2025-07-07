using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que registra todos los movimientos del inventario
/// </summary>
[Table("MovimientosInventario")]
public class MovimientoInventario
{
    /// <summary>
    /// Identificador único del movimiento
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MovimientoID { get; set; }

    /// <summary>
    /// Producto al que corresponde este movimiento (FK a Productos)
    /// </summary>
    [Required]
    [ForeignKey("Producto")]
    public int ProductoID { get; set; }

    /// <summary>
    /// Tipo de movimiento (Entrada, Salida, Ajuste)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string TipoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del movimiento (positivo para entradas, negativo para salidas)
    /// </summary>
    [Required]
    public int Cantidad { get; set; }

    /// <summary>
    /// Stock antes del movimiento
    /// </summary>
    [Required]
    public int StockAnterior { get; set; }

    /// <summary>
    /// Stock después del movimiento
    /// </summary>
    [Required]
    public int StockResultante { get; set; }

    /// <summary>
    /// Costo unitario al momento del movimiento
    /// </summary>
    public decimal? CostoUnitario { get; set; }

    /// <summary>
    /// Referencia del documento (factura, orden, etc.)
    /// </summary>
    [StringLength(100)]
    public string? Referencia { get; set; }

    /// <summary>
    /// Usuario que realizó el movimiento
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones del movimiento
    /// </summary>
    [StringLength(500)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Fecha y hora del movimiento
    /// </summary>
    [Required]
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Motivo del movimiento (para ajustes)
    /// </summary>
    [StringLength(200)]
    public string? Motivo { get; set; }

    /// <summary>
    /// Proveedor (para entradas)
    /// </summary>
    [StringLength(200)]
    public string? Proveedor { get; set; }

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Producto al que corresponde este movimiento
    /// </summary>
    public virtual Producto Producto { get; set; } = null!;

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si es un movimiento de entrada
    /// </summary>
    [NotMapped]
    public bool EsEntrada => TipoMovimiento == "Entrada";

    /// <summary>
    /// Indica si es un movimiento de salida
    /// </summary>
    [NotMapped]
    public bool EsSalida => TipoMovimiento == "Salida";

    /// <summary>
    /// Indica si es un ajuste
    /// </summary>
    [NotMapped]
    public bool EsAjuste => TipoMovimiento == "Ajuste";

    /// <summary>
    /// Valor total del movimiento
    /// </summary>
    [NotMapped]
    public decimal ValorTotal => Math.Abs(Cantidad) * (CostoUnitario ?? 0);

    /// <summary>
    /// Descripción completa del movimiento
    /// </summary>
    [NotMapped]
    public string DescripcionCompleta
    {
        get
        {
            var descripcion = $"{TipoMovimiento} de {Math.Abs(Cantidad)} unidades de {Producto?.Nombre ?? "Producto"}";
            if (!string.IsNullOrEmpty(Motivo))
                descripcion += $" - {Motivo}";
            return descripcion;
        }
    }

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Crea un movimiento de entrada
    /// </summary>
    public static MovimientoInventario CrearEntrada(int productoId, int cantidad, int stockAnterior, 
        decimal? costoUnitario, string usuario, string? proveedor = null, string? referencia = null, string? observaciones = null)
    {
        return new MovimientoInventario
        {
            ProductoID = productoId,
            TipoMovimiento = "Entrada",
            Cantidad = cantidad,
            StockAnterior = stockAnterior,
            StockResultante = stockAnterior + cantidad,
            CostoUnitario = costoUnitario,
            Usuario = usuario,
            Proveedor = proveedor,
            Referencia = referencia,
            Observaciones = observaciones,
            FechaMovimiento = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea un movimiento de salida
    /// </summary>
    public static MovimientoInventario CrearSalida(int productoId, int cantidad, int stockAnterior, 
        string usuario, string? referencia = null, string? observaciones = null)
    {
        return new MovimientoInventario
        {
            ProductoID = productoId,
            TipoMovimiento = "Salida",
            Cantidad = -cantidad, // Negativo para salidas
            StockAnterior = stockAnterior,
            StockResultante = stockAnterior - cantidad,
            Usuario = usuario,
            Referencia = referencia,
            Observaciones = observaciones,
            FechaMovimiento = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea un movimiento de ajuste
    /// </summary>
    public static MovimientoInventario CrearAjuste(int productoId, int cantidadAnterior, int cantidadNueva, 
        string usuario, string motivo, string? observaciones = null)
    {
        var diferencia = cantidadNueva - cantidadAnterior;
        return new MovimientoInventario
        {
            ProductoID = productoId,
            TipoMovimiento = "Ajuste",
            Cantidad = diferencia,
            StockAnterior = cantidadAnterior,
            StockResultante = cantidadNueva,
            Usuario = usuario,
            Motivo = motivo,
            Observaciones = observaciones,
            FechaMovimiento = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validación del movimiento
    /// </summary>
    public bool EsValido()
    {
        return ProductoID > 0 &&
               !string.IsNullOrEmpty(TipoMovimiento) &&
               !string.IsNullOrEmpty(Usuario) &&
               StockAnterior >= 0 &&
               StockResultante >= 0;
    }

    public override string ToString()
    {
        return $"{TipoMovimiento}: {Producto?.Nombre ?? $"Producto {ProductoID}"} - {Math.Abs(Cantidad)} unidades ({FechaMovimiento:dd/MM/yyyy HH:mm})";
    }
} 