namespace ElCriollo.API.Models.DTOs.Response;

/// <summary>
/// DTO de respuesta completa para empleados
/// </summary>
public class EmpleadoResponse
{
    /// <summary>
    /// ID del empleado
    /// </summary>
    public int EmpleadoID { get; set; }

    /// <summary>
    /// Nombre completo del empleado
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Cédula del empleado
    /// </summary>
    public string Cedula { get; set; } = string.Empty;

    /// <summary>
    /// Email del empleado
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Teléfono del empleado
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Teléfono formateado (ej: (809) 555-1234)
    /// </summary>
    public string TelefonoFormateado { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del empleado
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime FechaNacimiento { get; set; }

    /// <summary>
    /// Fecha de contratación
    /// </summary>
    public DateTime FechaContratacion { get; set; }

    /// <summary>
    /// Cargo o posición
    /// </summary>
    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Salario formateado (ej: RD$ 25,000.00)
    /// </summary>
    public string SalarioFormateado { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo trabajando en la empresa
    /// </summary>
    public string TiempoEnEmpresa { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el empleado está activo
    /// </summary>
    public bool EsEmpleadoActivo { get; set; }
} 