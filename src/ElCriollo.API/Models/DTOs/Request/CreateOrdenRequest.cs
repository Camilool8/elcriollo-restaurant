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
    public int? MesaID { get; set; }

    /// <summary>
    /// Cliente que hace la orden (opcional para clientes eventuales)
    /// </summary>
    public int? ClienteID { get; set; }

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
    /// Detalles de la orden (productos y combos)
    /// </summary>
    [Required(ErrorMessage = "La orden debe tener al menos un item")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un item")]
    public List<CreateDetalleOrdenRequest> Detalles { get; set; } = new List<CreateDetalleOrdenRequest>();

    /// <summary>
    /// Datos del cliente si es ocasional y no está registrado
    /// </summary>
    public CreateClienteOcasionalRequest? ClienteOcasional { get; set; }
}

/// <summary>
/// DTO para crear un detalle de orden
/// </summary>
public class CreateDetalleOrdenRequest
{
    /// <summary>
    /// ID del producto (opcional si es combo)
    /// </summary>
    public int? ProductoID { get; set; }

    /// <summary>
    /// ID del combo (opcional si es producto)
    /// </summary>
    public int? ComboID { get; set; }

    /// <summary>
    /// Cantidad solicitada
    /// </summary>
    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 99, ErrorMessage = "La cantidad debe estar entre 1 y 99")]
    public int Cantidad { get; set; }

    /// <summary>
    /// Observaciones específicas del item
    /// </summary>
    [StringLength(250, ErrorMessage = "Las observaciones no pueden exceder 250 caracteres")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Valida que sea producto o combo, pero no ambos
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();

        if (!ProductoID.HasValue && !ComboID.HasValue)
        {
            errors.Add(new ValidationResult(
                "Debe especificar un producto o un combo",
                new[] { nameof(ProductoID), nameof(ComboID) }));
        }

        if (ProductoID.HasValue && ComboID.HasValue)
        {
            errors.Add(new ValidationResult(
                "No puede especificar producto y combo al mismo tiempo",
                new[] { nameof(ProductoID), nameof(ComboID) }));
        }

        return errors;
    }
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