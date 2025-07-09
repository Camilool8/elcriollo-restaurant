namespace ElCriollo.API.Configuration;

/// <summary>
/// Configuración para el servicio de correo electrónico
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Servidor SMTP para envío de emails
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Puerto del servidor SMTP
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Indica si SSL está habilitado
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Email del remitente
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del remitente
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Usuario para autenticación SMTP
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña para autenticación SMTP
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ruta donde se encuentran las plantillas de email
    /// </summary>
    public string TemplatesPath { get; set; } = "Templates/Email/";

    /// <summary>
    /// Indica si el envío de emails está habilitado (útil para desarrollo)
    /// </summary>
    public bool EnableEmailSending { get; set; } = true;

    /// <summary>
    /// En desarrollo, guardar emails en archivos en lugar de enviarlos
    /// </summary>
    public bool SaveEmailsToFile { get; set; } = false;

    /// <summary>
    /// Ruta donde guardar los emails en desarrollo
    /// </summary>
    public string EmailOutputPath { get; set; } = "logs/emails/";

    /// <summary>
    /// Email por defecto para clientes anónimos cuando no proporcionan email
    /// </summary>
    public string DefaultEmailForAnonymousClients { get; set; } = string.Empty;

    /// <summary>
    /// Validar si la configuración de email es válida
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SmtpServer) && 
               SmtpPort > 0 && 
               !string.IsNullOrEmpty(FromEmail) && 
               !string.IsNullOrEmpty(FromName);
    }

    /// <summary>
    /// Obtener la dirección completa del remitente
    /// </summary>
    public string GetFromAddress()
    {
        return $"{FromName} <{FromEmail}>";
    }
}