using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear una nueva orden
/// </summary>
public class CreateOrdenRequest
{
    /// <summary>
    /// Mesa donde se toma la orden (opcional para órdenes para llevar)
    /// </summary>
    public int? MesaId { get; set; }

    /// <summary>
    /// Cliente que hace la orden (opcional para clientes eventuales)
    /// </summary>
    public int? ClienteId { get; set; }

    /// <summary>
    /// Tipo de orden
    /// </summary>
    [Required(ErrorMessage = "El tipo de orden es requerido")]
    [RegularExpression("^(Mesa|Llevar|Delivery)$", ErrorMessage = "El tipo de orden debe ser: Mesa, Llevar o Delivery")]
    public string TipoOrden { get; set; } = "Mesa";

    /// <summary>
    /// Observaciones generales de la orden
    /// </summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Items de la orden (productos y combos)
    /// </summary>
    [Required(ErrorMessage = "La orden debe tener al menos un item")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un item")]
    public List<ItemOrdenRequest> Items { get; set; } = new List<ItemOrdenRequest>();

    /// <summary>
    /// Datos del cliente si es ocasional y no está registrado
    /// </summary>
    public CreateClienteOcasionalRequest? ClienteOcasional { get; set; }
}

/// <summary>
/// DTO para item de orden
/// </summary>
public class ItemOrdenRequest
{
    /// <summary>
    /// ID del producto
    /// </summary>
    [Required(ErrorMessage = "El producto es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser válido")]
    public int ProductoId { get; set; }

    /// <summary>
    /// Cantidad solicitada
    /// </summary>
    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 99, ErrorMessage = "La cantidad debe estar entre 1 y 99")]
    public int Cantidad { get; set; }

    /// <summary>
    /// Notas especiales del item
    /// </summary>
    [StringLength(250, ErrorMessage = "Las notas no pueden exceder 250 caracteres")]
    public string? NotasEspeciales { get; set; }
}

/// <summary>
/// DTO para cliente ocasional
/// </summary>
public class CreateClienteOcasionalRequest
{
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del cliente
    /// </summary>
    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente (opcional)
    /// </summary>
    [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
    [RegularExpression(@"^(\+1\s?)?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{4}$", 
        ErrorMessage = "El formato del teléfono no es válido")]
    public string? Telefono { get; set; }

    /// <summary>
    /// Email del cliente (opcional)
    /// </summary>
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(70, ErrorMessage = "El email no puede exceder 70 caracteres")]
    public string? Email { get; set; }
}