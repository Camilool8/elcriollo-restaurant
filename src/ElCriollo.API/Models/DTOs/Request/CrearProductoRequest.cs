using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear un nuevo producto
/// </summary>
public class CrearProductoRequest
{
    /// <summary>
    /// Nombre del producto
    /// </summary>
    [Required(ErrorMessage = "El nombre del producto es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto
    /// </summary>
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// ID de la categoría a la que pertenece
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar una categoría")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int CategoriaId { get; set; }

    /// <summary>
    /// Precio del producto en pesos dominicanos
    /// </summary>
    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal Precio { get; set; }

    /// <summary>
    /// Tiempo de preparación en minutos (opcional)
    /// </summary>
    [Range(1, 999, ErrorMessage = "El tiempo de preparación debe estar entre 1 y 999 minutos")]
    public int? TiempoPreparacion { get; set; }

    /// <summary>
    /// URL de la imagen del producto (opcional)
    /// </summary>
    [StringLength(255, ErrorMessage = "La URL de la imagen no puede exceder 255 caracteres")]
    public string? Imagen { get; set; }

    /// <summary>
    /// Costo de preparación (opcional, para cálculos de rentabilidad)
    /// </summary>
    [Range(0.01, 999999.99, ErrorMessage = "El costo debe estar entre 0.01 y 999,999.99")]
    public decimal? CostoPreparacion { get; set; }
} 