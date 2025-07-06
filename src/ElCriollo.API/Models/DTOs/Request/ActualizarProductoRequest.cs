using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para actualizar un producto existente
/// </summary>
public class ActualizarProductoRequest
{
    /// <summary>
    /// Nuevo nombre del producto (opcional)
    /// </summary>
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Nueva descripción del producto (opcional)
    /// </summary>
    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Nuevo precio del producto (opcional)
    /// </summary>
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal? Precio { get; set; }

    /// <summary>
    /// ID de la nueva categoría (opcional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int? CategoriaId { get; set; }

    /// <summary>
    /// Nueva disponibilidad del producto (opcional)
    /// </summary>
    public bool? Disponible { get; set; }

    /// <summary>
    /// Nuevo tiempo de preparación en minutos (opcional)
    /// </summary>
    [Range(1, 999, ErrorMessage = "El tiempo de preparación debe estar entre 1 y 999 minutos")]
    public int? TiempoPreparacion { get; set; }

    /// <summary>
    /// Nueva URL de imagen (opcional)
    /// </summary>
    [StringLength(255, ErrorMessage = "La URL de la imagen no puede exceder 255 caracteres")]
    public string? Imagen { get; set; }
} 