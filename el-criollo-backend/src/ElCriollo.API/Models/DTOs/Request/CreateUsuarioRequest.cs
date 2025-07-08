using System.ComponentModel.DataAnnotations;

namespace ElCriollo.API.Models.DTOs.Request;

/// <summary>
/// DTO para crear un nuevo usuario con su empleado asociado
/// </summary>
public class CreateUsuarioRequest
{
    // ============================================================================
    // DATOS DEL USUARIO
    // ============================================================================

    /// <summary>
    /// Nombre de usuario único
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números y guiones bajos")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._#-])[A-Za-z\d@$!%*?&._#-]+$", 
        ErrorMessage = "La contraseña debe contener al menos: 1 minúscula, 1 mayúscula, 1 número y 1 carácter especial")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmación de contraseña
    /// </summary>
    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    /// <summary>
    /// Email del usuario
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(70, ErrorMessage = "El email no puede exceder 70 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Rol que se asignará al usuario
    /// </summary>
    [Required(ErrorMessage = "El rol es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido")]
    public int RolId { get; set; }

    /// <summary>
    /// Indica si el usuario debe cambiar la contraseña en el primer login
    /// </summary>
    public bool RequiereCambioContrasena { get; set; } = true;

    // ============================================================================
    // DATOS DEL EMPLEADO
    // ============================================================================

    /// <summary>
    /// Cédula de identidad del empleado (documento dominicano)
    /// </summary>
    [Required(ErrorMessage = "La cédula es requerida")]
    [StringLength(16, ErrorMessage = "La cédula no puede exceder 16 caracteres")]
    [RegularExpression(@"^\d{3}-\d{7}-\d{1}$", ErrorMessage = "La cédula debe tener el formato XXX-XXXXXXX-X")]
    public string Cedula { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del empleado
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del empleado
    /// </summary>
    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Sexo del empleado
    /// </summary>
    [StringLength(15, ErrorMessage = "El sexo no puede exceder 15 caracteres")]
    public string? Sexo { get; set; }

    /// <summary>
    /// Dirección de residencia del empleado
    /// </summary>
    [StringLength(100, ErrorMessage = "La dirección no puede exceder 100 caracteres")]
    public string? Direccion { get; set; }

    /// <summary>
    /// Teléfono de contacto del empleado
    /// </summary>
    [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
    [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "El teléfono debe tener el formato XXX-XXX-XXXX")]
    public string? Telefono { get; set; }

    /// <summary>
    /// Salario del empleado en pesos dominicanos
    /// </summary>
    [Range(0, 999999.99, ErrorMessage = "El salario debe ser un valor positivo")]
    public decimal? Salario { get; set; }

    /// <summary>
    /// Departamento o área del empleado
    /// </summary>
    [StringLength(50, ErrorMessage = "El departamento no puede exceder 50 caracteres")]
    public string? Departamento { get; set; }

    /// <summary>
    /// Fecha de ingreso del empleado (opcional, por defecto hoy)
    /// </summary>
    public DateTime? FechaIngreso { get; set; }

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Nombre completo del empleado
    /// </summary>
    public string NombreCompleto => $"{Nombre} {Apellido}";

    /// <summary>
    /// Fecha de ingreso efectiva (usa la fecha proporcionada o la fecha actual)
    /// </summary>
    public DateTime FechaIngresoEfectiva => FechaIngreso ?? DateTime.Now.Date;
}