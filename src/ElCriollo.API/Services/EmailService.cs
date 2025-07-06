using ElCriollo.API.Configuration;
using ElCriollo.API.Interfaces;
using ElCriollo.API.Models.Entities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text.RegularExpressions;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Servicio de email para el restaurante El Criollo
    /// Enfoque básico y funcional para proyecto universitario
    /// 
    /// CONFIGURACIÓN DE ENVÍO:
    /// - EnableEmailSending = true + SaveEmailsToFile = false → Solo envía por SMTP
    /// - EnableEmailSending = false + SaveEmailsToFile = true → Solo guarda en archivo  
    /// - EnableEmailSending = true + SaveEmailsToFile = true → Envía Y guarda en archivo
    /// - EnableEmailSending = false + SaveEmailsToFile = false → No hace nada
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IBaseRepository<EmailTransaccion> _emailRepository;

        // Constantes para el restaurante dominicano
        private const string RESTAURANTE_NOMBRE = "Restaurante El Criollo";
        private const string RESTAURANTE_DIRECCION = "Santo Domingo, República Dominicana";
        private const string RESTAURANTE_TELEFONO = "+1 (809) 555-0123";

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IBaseRepository<EmailTransaccion> emailRepository)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _emailRepository = emailRepository;
        }

        // ============================================================================
        // CONFIRMACIONES DE RESERVAS
        // ============================================================================

        public async Task<bool> EnviarConfirmacionReservaAsync(Reservacion reservacion)
        {
            try
            {
                if (reservacion.Cliente?.Email == null)
                {
                    _logger.LogWarning("⚠️ Cliente sin email para confirmar reserva {ReservacionId}", reservacion.ReservacionID);
                    return false;
                }

                var asunto = "✅ Confirmación de Reserva - El Criollo";
                var contenidoHtml = GenerarPlantillaConfirmacionReserva(reservacion);

                return await EnviarEmailInternoAsync(
                    reservacion.Cliente.Email,
                    asunto,
                    contenidoHtml,
                    "ConfirmacionReserva",
                    $"Reserva #{reservacion.ReservacionID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar confirmación de reserva {ReservacionId}", reservacion.ReservacionID);
                return false;
            }
        }

        public async Task<bool> EnviarRecordatorioReservaAsync(Reservacion reservacion, int minutosAntes = 30)
        {
            try
            {
                if (reservacion.Cliente?.Email == null) return false;

                var asunto = "🔔 Recordatorio de Reserva - El Criollo";
                var contenidoHtml = GenerarPlantillaRecordatorioReserva(reservacion, minutosAntes);

                return await EnviarEmailInternoAsync(
                    reservacion.Cliente.Email,
                    asunto,
                    contenidoHtml,
                    "RecordatorioReserva",
                    $"Recordatorio #{reservacion.ReservacionID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar recordatorio de reserva {ReservacionId}", reservacion.ReservacionID);
                return false;
            }
        }

        public async Task<bool> EnviarCancelacionReservaAsync(Reservacion reservacion, string motivoCancelacion)
        {
            try
            {
                if (reservacion.Cliente?.Email == null) return false;

                var asunto = "❌ Cancelación de Reserva - El Criollo";
                var contenidoHtml = GenerarPlantillaCancelacionReserva(reservacion, motivoCancelacion);

                return await EnviarEmailInternoAsync(
                    reservacion.Cliente.Email,
                    asunto,
                    contenidoHtml,
                    "CancelacionReserva",
                    $"Cancelación #{reservacion.ReservacionID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar cancelación de reserva {ReservacionId}", reservacion.ReservacionID);
                return false;
            }
        }

        // ============================================================================
        // FACTURAS Y PAGOS
        // ============================================================================

        public async Task<bool> EnviarFacturaPorEmailAsync(Factura factura, string? emailDestino = null)
        {
            try
            {
                var email = emailDestino ?? factura.Cliente?.Email;
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("⚠️ No hay email para enviar factura {FacturaId}", factura.FacturaID);
                    return false;
                }

                var asunto = $"🧾 Factura #{factura.NumeroFactura} - El Criollo";
                var contenidoHtml = GenerarPlantillaFactura(factura);

                return await EnviarEmailInternoAsync(
                    email,
                    asunto,
                    contenidoHtml,
                    "Factura",
                    $"Factura #{factura.NumeroFactura}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar factura {FacturaId}", factura.FacturaID);
                return false;
            }
        }

        public async Task<bool> EnviarComprobantePagoAsync(Factura factura)
        {
            try
            {
                if (factura.Cliente?.Email == null) return false;

                var asunto = $"✅ Comprobante de Pago #{factura.NumeroFactura} - El Criollo";
                var contenidoHtml = GenerarPlantillaComprobantePago(factura);

                return await EnviarEmailInternoAsync(
                    factura.Cliente.Email,
                    asunto,
                    contenidoHtml,
                    "ComprobantePago",
                    $"Comprobante #{factura.NumeroFactura}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar comprobante de pago {FacturaId}", factura.FacturaID);
                return false;
            }
        }

        // ============================================================================
        // CONFIRMACIONES DE USUARIO
        // ============================================================================

        public async Task<bool> EnviarBienvenidaUsuarioAsync(Usuario usuario, string? contraseñaTemporal = null)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario.Email)) return false;

                var asunto = "👋 Bienvenido al Sistema El Criollo";
                var contenidoHtml = GenerarPlantillaBienvenidaUsuario(usuario, contraseñaTemporal);

                return await EnviarEmailInternoAsync(
                    usuario.Email,
                    asunto,
                    contenidoHtml,
                    "BienvenidaUsuario",
                    $"Usuario {usuario.UsuarioNombre}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar bienvenida a usuario {UsuarioId}", usuario.UsuarioID);
                return false;
            }
        }

        public async Task<bool> EnviarConfirmacionRegistroAsync(Cliente cliente)
        {
            try
            {
                if (string.IsNullOrEmpty(cliente.Email)) return false;

                var asunto = "🎉 ¡Registro Exitoso en El Criollo!";
                var contenidoHtml = GenerarPlantillaRegistroCliente(cliente);

                return await EnviarEmailInternoAsync(
                    cliente.Email,
                    asunto,
                    contenidoHtml,
                    "RegistroCliente",
                    $"Cliente {cliente.NombreCompleto}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar confirmación de registro {ClienteId}", cliente.ClienteID);
                return false;
            }
        }

        public async Task<bool> EnviarRestablecimientoPasswordAsync(string email, string tokenReset)
        {
            try
            {
                var asunto = "🔐 Restablecimiento de Contraseña - El Criollo";
                var contenidoHtml = GenerarPlantillaRestablecimientoPassword(email, tokenReset);

                return await EnviarEmailInternoAsync(
                    email,
                    asunto,
                    contenidoHtml,
                    "RestablecimientoPassword",
                    $"Reset para {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar restablecimiento de password para {Email}", email);
                return false;
            }
        }

        // ============================================================================
        // NOTIFICACIONES GENERALES
        // ============================================================================

        public async Task<bool> EnviarNotificacionPersonalizadaAsync(string destinatario, string asunto, string mensaje, bool esHtml = false)
        {
            try
            {
                var contenido = esHtml ? mensaje : GenerarPlantillaPersonalizada(mensaje);

                return await EnviarEmailInternoAsync(
                    destinatario,
                    asunto,
                    contenido,
                    "NotificacionPersonalizada",
                    asunto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar notificación personalizada a {Email}", destinatario);
                return false;
            }
        }

        public async Task<int> EnviarPromocionAsync(List<string> destinatarios, string asunto, string contenidoHtml)
        {
            var exitosos = 0;

            foreach (var email in destinatarios)
            {
                try
                {
                    var enviado = await EnviarEmailInternoAsync(
                        email,
                        asunto,
                        contenidoHtml,
                        "Promocion",
                        "Envío masivo promocional");

                    if (enviado) exitosos++;

                    // Pequeña pausa para evitar spam
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al enviar promoción a {Email}", email);
                }
            }

            _logger.LogInformation("📧 Promoción enviada a {Exitosos}/{Total} destinatarios", exitosos, destinatarios.Count);
            return exitosos;
        }

        // ============================================================================
        // NOTIFICACIONES INTERNAS
        // ============================================================================

        public async Task<bool> NotificarOrdenCriticaAsync(Orden orden)
        {
            try
            {
                var emailsAdmin = new[] { "admin@elcriollo.com", "gerencia@elcriollo.com" };
                var asunto = $"🚨 ORDEN CRÍTICA #{orden.NumeroOrden}";
                var contenidoHtml = GenerarPlantillaOrdenCritica(orden);

                var enviados = 0;
                foreach (var email in emailsAdmin)
                {
                    var resultado = await EnviarEmailInternoAsync(email, asunto, contenidoHtml, "AlertaInterna", $"Orden crítica #{orden.NumeroOrden}");
                    if (resultado) enviados++;
                }

                return enviados > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al notificar orden crítica {OrdenId}", orden.OrdenID);
                return false;
            }
        }

        public async Task<bool> NotificarStockBajoAsync(Producto producto)
        {
            try
            {
                var emailsAdmin = new[] { "admin@elcriollo.com", "inventario@elcriollo.com" };
                var asunto = $"⚠️ STOCK BAJO: {producto.Nombre}";
                var contenidoHtml = GenerarPlantillaStockBajo(producto);

                var enviados = 0;
                foreach (var email in emailsAdmin)
                {
                    var resultado = await EnviarEmailInternoAsync(email, asunto, contenidoHtml, "AlertaInventario", $"Stock bajo {producto.Nombre}");
                    if (resultado) enviados++;
                }

                return enviados > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al notificar stock bajo {ProductoId}", producto.ProductoID);
                return false;
            }
        }

        // ============================================================================
        // UTILIDADES Y CONFIGURACIÓN
        // ============================================================================

        public bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<EmailTransaccion>> GetHistorialEmailsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var emails = await _emailRepository.GetAllAsync();

                if (fechaInicio.HasValue)
                    emails = emails.Where(e => e.FechaEnvio >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    emails = emails.Where(e => e.FechaEnvio <= fechaFin.Value);

                return emails.OrderByDescending(e => e.FechaEnvio).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener historial de emails");
                return new List<EmailTransaccion>();
            }
        }

        public async Task<bool> VerificarConexionEmailAsync()
        {
            try
            {
                if (!_emailSettings.EnableEmailSending)
                {
                    _logger.LogInformation("📧 Servicio de email deshabilitado en configuración");
                    return true; // Consideramos válido si está intencionalmente deshabilitado
                }

                using var client = new SmtpClient();
                var secureSocketOptions = DeterminarSecureSocketOptions();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, secureSocketOptions);
                
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                await client.DisconnectAsync(true);
                
                _logger.LogInformation("✅ Conexión de email verificada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar conexión de email");
                return false;
            }
        }

        public async Task<EstadisticasEmailViewModel> GetEstadisticasEmailAsync()
        {
            try
            {
                var emails = await _emailRepository.GetAllAsync();
                var hoy = DateTime.Today;
                var semanaAtras = hoy.AddDays(-7);
                var mesAtras = hoy.AddDays(-30);

                var estadisticas = new EstadisticasEmailViewModel
                {
                    EmailsEnviadosHoy = emails.Count(e => e.FechaEnvio.Date == hoy && e.EstadoEnvio == "Enviado"),
                    EmailsEnviadosSemana = emails.Count(e => e.FechaEnvio >= semanaAtras && e.EstadoEnvio == "Enviado"),
                    EmailsEnviadosMes = emails.Count(e => e.FechaEnvio >= mesAtras && e.EstadoEnvio == "Enviado"),
                    EmailsFallidosHoy = emails.Count(e => e.FechaEnvio.Date == hoy && e.EstadoEnvio == "Error"),
                    UltimoEmailEnviado = emails.Where(e => e.EstadoEnvio == "Enviado").Max(e => (DateTime?)e.FechaEnvio),
                    EstadoServicio = _emailSettings.EnableEmailSending ? "Activo" : "Deshabilitado"
                };

                var totalEnviados = estadisticas.EmailsEnviadosHoy + estadisticas.EmailsFallidosHoy;
                estadisticas.PorcentajeExito = totalEnviados > 0 ? 
                    Math.Round((decimal)estadisticas.EmailsEnviadosHoy / totalEnviados * 100, 2) : 100;

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener estadísticas de email");
                return new EstadisticasEmailViewModel();
            }
        }

        // ============================================================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ============================================================================

        private async Task<bool> EnviarEmailInternoAsync(string destinatario, string asunto, string contenidoHtml, string tipoEmail, string descripcion)
        {
            var transaccion = new EmailTransaccion
            {
                EmailDestinatario = destinatario,
                Asunto = asunto,
                Cuerpo = contenidoHtml,
                TipoEmail = tipoEmail,
                FechaEnvio = DateTime.Now,
                EstadoEnvio = "Pendiente"
            };

            try
            {
                // Lógica mejorada para manejo de emails
                bool emailEnviado = false;
                bool emailGuardado = false;
                string? errorEnvio = null;

                // Guardar en archivo si está habilitado
                if (_emailSettings.SaveEmailsToFile)
                {
                    try
                    {
                        await GuardarEmailEnArchivoAsync(destinatario, asunto, contenidoHtml);
                        emailGuardado = true;
                        _logger.LogInformation("📁 Email guardado en archivo: {Destinatario} - {Asunto}", destinatario, asunto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error al guardar email en archivo: {Destinatario}", destinatario);
                        errorEnvio = ex.Message;
                    }
                }

                // Enviar por SMTP si está habilitado
                if (_emailSettings.EnableEmailSending)
                {
                    try
                    {
                        await EnviarEmailRealAsync(destinatario, asunto, contenidoHtml);
                        emailEnviado = true;
                        _logger.LogInformation("✅ Email enviado por SMTP: {Destinatario} - {Asunto}", destinatario, asunto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error al enviar email por SMTP: {Destinatario}", destinatario);
                        errorEnvio = ex.Message;
                    }
                }

                // Determinar estado final
                if (emailEnviado && emailGuardado)
                    transaccion.EstadoEnvio = "Enviado y guardado";
                else if (emailEnviado)
                    transaccion.EstadoEnvio = "Enviado";
                else if (emailGuardado)
                    transaccion.EstadoEnvio = "Guardado en archivo";
                else if (!string.IsNullOrEmpty(errorEnvio))
                {
                    transaccion.EstadoEnvio = "Error";
                    transaccion.MensajeError = errorEnvio;
                    _logger.LogError("❌ Error en procesamiento de email: {Destinatario} - {Error}", destinatario, errorEnvio);
                    throw new Exception(errorEnvio); // Re-lanzar para que el catch externo lo maneje
                }
                else
                {
                    transaccion.EstadoEnvio = "Sin procesar";
                    _logger.LogWarning("⚠️ Email no fue ni enviado ni guardado: {Destinatario} - {Asunto}", destinatario, asunto);
                }

                // Guardar transacción en base de datos
                await _emailRepository.AddAsync(transaccion);
                return true;
            }
            catch (Exception ex)
            {
                transaccion.EstadoEnvio = "Error";
                transaccion.MensajeError = ex.Message;
                await _emailRepository.AddAsync(transaccion);
                
                _logger.LogError(ex, "❌ Error al enviar email: {Destinatario} - {Asunto}", destinatario, asunto);
                return false;
            }
        }

        private async Task EnviarEmailRealAsync(string destinatario, string asunto, string contenidoHtml)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            mensaje.To.Add(new MailboxAddress("", destinatario));
            mensaje.Subject = asunto;

            var builder = new BodyBuilder
            {
                HtmlBody = contenidoHtml
            };
            mensaje.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Configuración correcta para Gmail y otros proveedores
            var secureSocketOptions = DeterminarSecureSocketOptions();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, secureSocketOptions);
            
            if (!string.IsNullOrEmpty(_emailSettings.Username))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
        }

        private MailKit.Security.SecureSocketOptions DeterminarSecureSocketOptions()
        {
            // Gmail y configuraciones comunes
            if (_emailSettings.SmtpServer.Contains("gmail.com"))
            {
                return _emailSettings.SmtpPort == 465 ? 
                    MailKit.Security.SecureSocketOptions.SslOnConnect : 
                    MailKit.Security.SecureSocketOptions.StartTls;
            }
            
            // Outlook/Hotmail
            if (_emailSettings.SmtpServer.Contains("outlook.com") || _emailSettings.SmtpServer.Contains("hotmail.com"))
            {
                return MailKit.Security.SecureSocketOptions.StartTls;
            }
            
            // Configuración por puerto estándar
            return _emailSettings.SmtpPort switch
            {
                587 => MailKit.Security.SecureSocketOptions.StartTls,  // Puerto STARTTLS estándar
                465 => MailKit.Security.SecureSocketOptions.SslOnConnect, // Puerto SSL estándar
                25 => MailKit.Security.SecureSocketOptions.None,      // Puerto sin encriptación
                _ => _emailSettings.EnableSsl ? 
                    MailKit.Security.SecureSocketOptions.StartTls : 
                    MailKit.Security.SecureSocketOptions.None
            };
        }

        private async Task GuardarEmailEnArchivoAsync(string destinatario, string asunto, string contenidoHtml)
        {
            try
            {
                var directorioEmails = _emailSettings.EmailOutputPath;
                if (!Directory.Exists(directorioEmails))
                {
                    Directory.CreateDirectory(directorioEmails);
                }

                var nombreArchivo = $"email_{DateTime.Now:yyyyMMdd_HHmmss}_{destinatario.Replace("@", "_at_")}.html";
                var rutaArchivo = Path.Combine(directorioEmails, nombreArchivo);

                var contenidoCompleto = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{asunto}</title>
</head>
<body>
    <h3>Para: {destinatario}</h3>
    <h3>Asunto: {asunto}</h3>
    <h3>Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</h3>
    <hr>
    {contenidoHtml}
</body>
</html>";

                await File.WriteAllTextAsync(rutaArchivo, contenidoCompleto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar email en archivo");
            }
        }

        // ============================================================================
        // PLANTILLAS DE EMAIL BÁSICAS
        // ============================================================================

        private string GenerarPlantillaConfirmacionReserva(Reservacion reservacion)
        {
            return $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #d97706;'>🇩🇴 {RESTAURANTE_NOMBRE}</h2>
    <h3>✅ Confirmación de Reserva</h3>
    
    <p>Estimado/a <strong>{reservacion.Cliente?.NombreCompleto}</strong>,</p>
    
    <p>Su reserva ha sido confirmada exitosamente:</p>
    
    <div style='background: #f3f4f6; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <p><strong>📅 Fecha:</strong> {reservacion.FechaYHora:dddd, dd 'de' MMMM 'de' yyyy}</p>
        <p><strong>🕐 Hora:</strong> {reservacion.FechaYHora:HH:mm}</p>
        <p><strong>👥 Personas:</strong> {reservacion.CantidadPersonas}</p>
        <p><strong>🪑 Mesa:</strong> Mesa #{reservacion.Mesa?.NumeroMesa}</p>
        <p><strong>📋 Observaciones:</strong> {reservacion.ObservacionesEspeciales ?? "Ninguna"}</p>
    </div>
    
    <p>¡Lo esperamos para disfrutar de la mejor comida dominicana!</p>
    
    <div style='margin-top: 30px; padding: 15px; background: #065f46; color: white; text-align: center;'>
        <p><strong>{RESTAURANTE_NOMBRE}</strong></p>
        <p>{RESTAURANTE_DIRECCION}</p>
        <p>📞 {RESTAURANTE_TELEFONO}</p>
    </div>
</div>";
        }

        private string GenerarPlantillaFactura(Factura factura)
        {
            return $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #d97706;'>🇩🇴 {RESTAURANTE_NOMBRE}</h2>
    <h3>🧾 Factura #{factura.NumeroFactura}</h3>
    
    <div style='background: #f3f4f6; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <p><strong>📅 Fecha:</strong> {factura.FechaFactura:dd/MM/yyyy HH:mm}</p>
        <p><strong>👤 Cliente:</strong> {factura.Cliente?.NombreCompleto}</p>
        <p><strong>🪑 Mesa:</strong> {(factura.Orden?.Mesa?.NumeroMesa.ToString() ?? "Para llevar")}</p>
    </div>
    
    <h4>💰 Resumen de Pago</h4>
    <table style='width: 100%; border-collapse: collapse;'>
        <tr><td>Subtotal:</td><td style='text-align: right;'>RD$ {factura.Subtotal:N2}</td></tr>
        <tr><td>Descuento:</td><td style='text-align: right;'>RD$ -{factura.Descuento:N2}</td></tr>
        <tr><td>ITBIS (18%):</td><td style='text-align: right;'>RD$ {factura.Impuesto:N2}</td></tr>
        <tr><td>Propina:</td><td style='text-align: right;'>RD$ {factura.Propina:N2}</td></tr>
        <tr style='border-top: 2px solid #000; font-weight: bold;'>
            <td>TOTAL:</td><td style='text-align: right;'>RD$ {factura.Total:N2}</td>
        </tr>
    </table>
    
    <p style='margin-top: 20px;'><strong>💳 Método de Pago:</strong> {factura.MetodoPago}</p>
    
    <div style='margin-top: 30px; padding: 15px; background: #065f46; color: white; text-align: center;'>
        <p><strong>¡Gracias por visitarnos!</strong></p>
        <p>{RESTAURANTE_NOMBRE} - {RESTAURANTE_DIRECCION}</p>
    </div>
</div>";
        }

        private string GenerarPlantillaBienvenidaUsuario(Usuario usuario, string? contraseñaTemporal)
        {
            var infoPassword = !string.IsNullOrEmpty(contraseñaTemporal) ? 
                $"<p><strong>🔐 Contraseña temporal:</strong> {contraseñaTemporal}</p><p><em>Por favor, cambie su contraseña al iniciar sesión.</em></p>" : 
                "";

            return $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #d97706;'>🇩🇴 {RESTAURANTE_NOMBRE}</h2>
    <h3>👋 ¡Bienvenido al Sistema!</h3>
    
    <p>Hola <strong>{usuario.Empleado?.NombreCompleto ?? usuario.UsuarioNombre}</strong>,</p>
    
    <p>Su cuenta ha sido creada exitosamente en el sistema de {RESTAURANTE_NOMBRE}.</p>
    
    <div style='background: #f3f4f6; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <p><strong>👤 Usuario:</strong> {usuario.UsuarioNombre}</p>
        <p><strong>🎭 Rol:</strong> {usuario.Rol?.NombreRol}</p>
        <p><strong>📧 Email:</strong> {usuario.Email}</p>
        {infoPassword}
    </div>
    
    <p>Ya puede acceder al sistema para comenzar a trabajar.</p>
    
    <div style='margin-top: 30px; padding: 15px; background: #065f46; color: white; text-align: center;'>
        <p><strong>{RESTAURANTE_NOMBRE}</strong></p>
        <p>Sistema de Gestión Restaurante</p>
    </div>
</div>";
        }

        private string GenerarPlantillaPersonalizada(string mensaje)
        {
            return $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #d97706;'>🇩🇴 {RESTAURANTE_NOMBRE}</h2>
    
    <div style='background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
        {mensaje.Replace("\n", "<br>")}
    </div>
    
    <div style='margin-top: 30px; padding: 15px; background: #065f46; color: white; text-align: center;'>
        <p><strong>{RESTAURANTE_NOMBRE}</strong></p>
        <p>{RESTAURANTE_DIRECCION}</p>
    </div>
</div>";
        }

        private string GenerarPlantillaRecordatorioReserva(Reservacion reservacion, int minutos) => 
            GenerarPlantillaConfirmacionReserva(reservacion).Replace("Confirmación", $"Recordatorio ({minutos} min)");

        private string GenerarPlantillaCancelacionReserva(Reservacion reservacion, string motivo) => 
            GenerarPlantillaConfirmacionReserva(reservacion).Replace("✅ Confirmación", "❌ Cancelación").Replace("confirmada exitosamente", $"cancelada: {motivo}");

        private string GenerarPlantillaComprobantePago(Factura factura) => 
            GenerarPlantillaFactura(factura).Replace("🧾 Factura", "✅ Comprobante de Pago");

        private string GenerarPlantillaRegistroCliente(Cliente cliente) => 
            $"<h3>¡Bienvenido {cliente.NombreCompleto}!</h3><p>Su registro en El Criollo ha sido exitoso.</p>";

        private string GenerarPlantillaRestablecimientoPassword(string email, string token) => 
            $"<h3>🔐 Restablecimiento de Contraseña</h3><p>Use este código: <strong>{token}</strong></p>";

        private string GenerarPlantillaOrdenCritica(Orden orden) => 
            $"<h3>🚨 ALERTA: Orden #{orden.NumeroOrden}</h3><p>Tiempo excedido: {DateTime.Now - orden.FechaCreacion}</p>";

        private string GenerarPlantillaStockBajo(Producto producto) => 
            $"<h3>⚠️ Stock Bajo</h3><p>Producto: {producto.Nombre}<br>Stock actual: {producto.Inventario?.CantidadDisponible ?? 0}</p>";
    }
}