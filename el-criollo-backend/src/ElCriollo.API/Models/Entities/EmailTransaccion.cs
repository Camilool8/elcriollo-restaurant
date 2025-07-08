using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa el historial de emails enviados por el sistema
/// </summary>
[Table("EmailTransacciones")]
public class EmailTransaccion
{
    /// <summary>
    /// Identificador único de la transacción de email
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EmailID { get; set; }

    /// <summary>
    /// Email del destinatario
    /// </summary>
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string DestinatarioEmail { get; set; } = string.Empty;

    /// <summary>
    /// Asunto del email
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Contenido del mensaje (puede ser HTML o texto plano)
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? Mensaje { get; set; }

    /// <summary>
    /// Tipo de email (Confirmacion, Reserva, Factura, Promocion, BienvenidaUsuario)
    /// </summary>
    [StringLength(50)]
    public string? TipoEmail { get; set; }

    /// <summary>
    /// ID de referencia del objeto relacionado (reserva, orden, factura, usuario)
    /// </summary>
    public int? ReferenciaID { get; set; }

    /// <summary>
    /// Fecha y hora de envío del email
    /// </summary>
    [Required]
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estado del envío (Pendiente, Enviado, Error)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Mensaje de error si falló el envío
    /// </summary>
    [StringLength(500)]
    public string? MensajeError { get; set; }

    /// <summary>
    /// Intentos de envío
    /// </summary>
    public int IntentosEnvio { get; set; } = 0;

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si el email está pendiente de envío
    /// </summary>
    [NotMapped]
    public bool EstaPendiente => Estado == "Pendiente";

    /// <summary>
    /// Indica si el email fue enviado exitosamente
    /// </summary>
    [NotMapped]
    public bool FueEnviado => Estado == "Enviado";

    /// <summary>
    /// Indica si hubo error en el envío
    /// </summary>
    [NotMapped]
    public bool TuvoError => Estado == "Error";

    /// <summary>
    /// Tiempo transcurrido desde el envío
    /// </summary>
    [NotMapped]
    public TimeSpan TiempoTranscurrido => DateTime.UtcNow - FechaEnvio;

    /// <summary>
    /// Indica si es un email del día actual
    /// </summary>
    [NotMapped]
    public bool EsDelDiaActual => FechaEnvio.Date == DateTime.Now.Date;

    /// <summary>
    /// Indica si es un email de confirmación
    /// </summary>
    [NotMapped]
    public bool EsConfirmacion => TipoEmail == "Confirmacion";

    /// <summary>
    /// Indica si es un email de reserva
    /// </summary>
    [NotMapped]
    public bool EsReserva => TipoEmail == "Reserva";

    /// <summary>
    /// Indica si es un email de factura
    /// </summary>
    [NotMapped]
    public bool EsFactura => TipoEmail == "Factura";

    /// <summary>
    /// Indica si es un email promocional
    /// </summary>
    [NotMapped]
    public bool EsPromocion => TipoEmail == "Promocion";

    /// <summary>
    /// Indica si es un email de bienvenida
    /// </summary>
    [NotMapped]
    public bool EsBienvenida => TipoEmail == "BienvenidaUsuario";

    /// <summary>
    /// Longitud del mensaje en caracteres
    /// </summary>
    [NotMapped]
    public int LongitudMensaje => Mensaje?.Length ?? 0;

    /// <summary>
    /// Fecha de envío formateada
    /// </summary>
    [NotMapped]
    public string FechaEnvioFormateada => FechaEnvio.ToString("dd/MM/yyyy HH:mm");

    /// <summary>
    /// Resumen del email para mostrar en listas
    /// </summary>
    [NotMapped]
    public string ResumenEmail => LongitudMensaje > 100 
        ? $"{Mensaje?.Substring(0, 100)}..." 
        : Mensaje ?? "";

    // ============================================================================
    // PROPIEDADES ALIAS PARA COMPATIBILIDAD
    // ============================================================================

    /// <summary>
    /// Alias para EstadoEnvio (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string EstadoEnvio 
    { 
        get => Estado;
        set => Estado = value;
    }

    /// <summary>
    /// Alias para EmailDestinatario (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string EmailDestinatario 
    { 
        get => DestinatarioEmail;
        set => DestinatarioEmail = value;
    }

    /// <summary>
    /// Alias para Cuerpo (compatibilidad con servicios)
    /// </summary>
    [NotMapped]
    public string? Cuerpo 
    { 
        get => Mensaje;
        set => Mensaje = value;
    }

    /// <summary>
    /// Indica si fue exitoso (alias para FueEnviado)
    /// </summary>
    [NotMapped]
    public bool FueExitoso => FueEnviado;

    /// <summary>
    /// Indica si requiere reintento (más de 0 intentos y no enviado)
    /// </summary>
    [NotMapped]
    public bool RequiereReintento => !FueEnviado && IntentosEnvio < 3 && TuvoError;

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Marca el email como enviado exitosamente
    /// </summary>
    public void MarcarComoEnviado()
    {
        Estado = "Enviado";
        FechaEnvio = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca el email como error en el envío
    /// </summary>
    public void MarcarComoError()
    {
        Estado = "Error";
    }

    /// <summary>
    /// Marca el email como pendiente de envío
    /// </summary>
    public void MarcarComoPendiente()
    {
        Estado = "Pendiente";
    }

    /// <summary>
    /// Valida que el email sea válido
    /// </summary>
    public List<string> ValidarEmail()
    {
        var errores = new List<string>();

        if (string.IsNullOrEmpty(DestinatarioEmail))
            errores.Add("El email del destinatario es requerido");

        if (!string.IsNullOrEmpty(DestinatarioEmail) && !IsValidEmail(DestinatarioEmail))
            errores.Add("El formato del email no es válido");

        if (string.IsNullOrEmpty(Asunto))
            errores.Add("El asunto del email es requerido");

        if (Asunto?.Length > 200)
            errores.Add("El asunto no puede tener más de 200 caracteres");

        if (!string.IsNullOrEmpty(TipoEmail))
        {
            var tiposValidos = new[] { "Confirmacion", "Reserva", "Factura", "Promocion", "BienvenidaUsuario" };
            if (!tiposValidos.Contains(TipoEmail))
                errores.Add($"Tipo de email inválido: {TipoEmail}");
        }

        var estadosValidos = new[] { "Pendiente", "Enviado", "Error" };
        if (!estadosValidos.Contains(Estado))
            errores.Add($"Estado inválido: {Estado}");

        return errores;
    }

    /// <summary>
    /// Valida formato de email
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica si es un email relacionado a una reserva específica
    /// </summary>
    public bool EsDeReserva(int reservaId)
    {
        return EsReserva && ReferenciaID == reservaId;
    }

    /// <summary>
    /// Verifica si es un email relacionado a una factura específica
    /// </summary>
    public bool EsDeFactura(int facturaId)
    {
        return EsFactura && ReferenciaID == facturaId;
    }

    /// <summary>
    /// Verifica si es un email relacionado a un usuario específico
    /// </summary>
    public bool EsDeUsuario(int usuarioId)
    {
        return EsBienvenida && ReferenciaID == usuarioId;
    }

    /// <summary>
    /// Obtiene el icono apropiado para el tipo de email
    /// </summary>
    public string ObtenerIcono()
    {
        return TipoEmail switch
        {
            "Confirmacion" => "✅",
            "Reserva" => "📅",
            "Factura" => "🧾",
            "Promocion" => "🎉",
            "BienvenidaUsuario" => "👋",
            _ => "📧"
        };
    }

    /// <summary>
    /// Obtiene el color apropiado para el estado del email
    /// </summary>
    public string ObtenerColorEstado()
    {
        return Estado switch
        {
            "Pendiente" => "orange",
            "Enviado" => "green",
            "Error" => "red",
            _ => "gray"
        };
    }

    /// <summary>
    /// Genera plantillas de email predefinidas para diferentes tipos
    /// </summary>
    public static EmailTransaccion CrearEmailReserva(string destinatario, int reservaId, string nombreCliente, DateTime fechaReserva, int numeroMesa)
    {
        var asunto = "Confirmación de Reserva - Restaurante El Criollo";
        var mensaje = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #8B4513; text-align: center;'>🇩🇴 Restaurante El Criollo</h2>
                    <h3 style='color: #228B22;'>¡Reserva Confirmada!</h3>
                    
                    <p>Estimado/a {nombreCliente},</p>
                    
                    <p>Su reserva ha sido confirmada exitosamente:</p>
                    
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <strong>Detalles de la Reserva:</strong><br>
                        📅 Fecha y Hora: {fechaReserva:dd/MM/yyyy HH:mm}<br>
                        🪑 Mesa: {numeroMesa}<br>
                        📧 Número de Reserva: {reservaId}
                    </div>
                    
                    <p>Le esperamos para disfrutar de nuestra auténtica comida dominicana.</p>
                    
                    <p>Saludos cordiales,<br>
                    <strong>Equipo de Restaurante El Criollo</strong><br>
                    📞 +1 (809) 555-0123</p>
                </div>
            </body>
            </html>";

        return new EmailTransaccion
        {
            DestinatarioEmail = destinatario,
            Asunto = asunto,
            Mensaje = mensaje,
            TipoEmail = "Reserva",
            ReferenciaID = reservaId
        };
    }

    /// <summary>
    /// Crea email de factura
    /// </summary>
    public static EmailTransaccion CrearEmailFactura(string destinatario, int facturaId, string numeroFactura, decimal total)
    {
        var asunto = $"Factura #{numeroFactura} - Restaurante El Criollo";
        var mensaje = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #8B4513; text-align: center;'>🇩🇴 Restaurante El Criollo</h2>
                    <h3 style='color: #228B22;'>Factura Electrónica</h3>
                    
                    <p>Estimado cliente,</p>
                    
                    <p>Adjunto encontrará su factura electrónica:</p>
                    
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <strong>Resumen de la Factura:</strong><br>
                        🧾 Número: {numeroFactura}<br>
                        💰 Total: RD$ {total:N2}<br>
                        📅 Fecha: {DateTime.Now:dd/MM/yyyy}
                    </div>
                    
                    <p>¡Gracias por visitarnos! Esperamos verle pronto.</p>
                    
                    <p>Saludos cordiales,<br>
                    <strong>Equipo de Restaurante El Criollo</strong></p>
                </div>
            </body>
            </html>";

        return new EmailTransaccion
        {
            DestinatarioEmail = destinatario,
            Asunto = asunto,
            Mensaje = mensaje,
            TipoEmail = "Factura",
            ReferenciaID = facturaId
        };
    }

    /// <summary>
    /// Crea email de bienvenida para nuevo usuario
    /// </summary>
    public static EmailTransaccion CrearEmailBienvenidaUsuario(string destinatario, int usuarioId, string nombreUsuario, string rolUsuario)
    {
        var asunto = "Bienvenido al Sistema El Criollo";
        var mensaje = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #8B4513; text-align: center;'>🇩🇴 Restaurante El Criollo</h2>
                    <h3 style='color: #228B22;'>¡Bienvenido al Equipo!</h3>
                    
                    <p>Estimado/a {nombreUsuario},</p>
                    
                    <p>Su cuenta de usuario ha sido creada exitosamente en el sistema de Restaurante El Criollo.</p>
                    
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <strong>Detalles de su Cuenta:</strong><br>
                        👤 Usuario: {nombreUsuario}<br>
                        🎭 Rol: {rolUsuario}<br>
                        📧 Email: {destinatario}
                    </div>
                    
                    <p>Ya puede acceder al sistema con sus credenciales asignadas.</p>
                    
                    <p>Si tiene alguna pregunta, no dude en contactarnos.</p>
                    
                    <p>Saludos cordiales,<br>
                    <strong>Administración de El Criollo</strong></p>
                </div>
            </body>
            </html>";

        return new EmailTransaccion
        {
            DestinatarioEmail = destinatario,
            Asunto = asunto,
            Mensaje = mensaje,
            TipoEmail = "BienvenidaUsuario",
            ReferenciaID = usuarioId
        };
    }

    /// <summary>
    /// Genera estadísticas del email
    /// </summary>
    public object GenerarEstadisticas()
    {
        return new
        {
            EmailID = EmailID,
            Destinatario = DestinatarioEmail,
            Asunto = Asunto,
            TipoEmail = TipoEmail ?? "No especificado",
            Estado = Estado,
            FechaEnvio = FechaEnvioFormateada,
            TiempoTranscurrido = $"{TiempoTranscurrido.Days} días, {TiempoTranscurrido.Hours} horas",
            LongitudMensaje = LongitudMensaje,
            ReferenciaID = ReferenciaID,
            Icono = ObtenerIcono(),
            ColorEstado = ObtenerColorEstado()
        };
    }

    /// <summary>
    /// Representación en string del email
    /// </summary>
    public override string ToString()
    {
        var tipo = !string.IsNullOrEmpty(TipoEmail) ? $"[{TipoEmail}]" : "";
        return $"{tipo} {Asunto} → {DestinatarioEmail} ({Estado})";
    }
}