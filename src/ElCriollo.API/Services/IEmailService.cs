using ElCriollo.API.Models.Entities;

namespace ElCriollo.API.Services
{
    /// <summary>
    /// Interfaz para el servicio de email del restaurante El Criollo
    /// Maneja envío de confirmaciones, notificaciones y facturas por email
    /// </summary>
    public interface IEmailService
    {
        // ============================================================================
        // CONFIRMACIONES DE RESERVAS
        // ============================================================================

        /// <summary>
        /// Envía confirmación de reserva por email
        /// </summary>
        /// <param name="reservacion">Reservación confirmada</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarConfirmacionReservaAsync(Reservacion reservacion);

        /// <summary>
        /// Envía recordatorio de reserva próxima
        /// </summary>
        /// <param name="reservacion">Reservación próxima</param>
        /// <param name="minutosAntes">Minutos de anticipación</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarRecordatorioReservaAsync(Reservacion reservacion, int minutosAntes = 30);

        /// <summary>
        /// Envía notificación de cancelación de reserva
        /// </summary>
        /// <param name="reservacion">Reservación cancelada</param>
        /// <param name="motivoCancelacion">Motivo de la cancelación</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarCancelacionReservaAsync(Reservacion reservacion, string motivoCancelacion);

        // ============================================================================
        // FACTURAS Y PAGOS
        // ============================================================================

        /// <summary>
        /// Envía factura por email al cliente
        /// </summary>
        /// <param name="factura">Factura a enviar</param>
        /// <param name="emailDestino">Email alternativo (opcional)</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarFacturaPorEmailAsync(Factura factura, string? emailDestino = null);

        /// <summary>
        /// Envía comprobante de pago
        /// </summary>
        /// <param name="factura">Factura pagada</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarComprobantePagoAsync(Factura factura);

        // ============================================================================
        // CONFIRMACIONES DE USUARIO
        // ============================================================================

        /// <summary>
        /// Envía email de bienvenida a nuevo usuario
        /// </summary>
        /// <param name="usuario">Usuario recién registrado</param>
        /// <param name="contraseñaTemporal">Contraseña temporal (opcional)</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarBienvenidaUsuarioAsync(Usuario usuario, string? contraseñaTemporal = null);

        /// <summary>
        /// Envía email de confirmación de registro
        /// </summary>
        /// <param name="cliente">Cliente recién registrado</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarConfirmacionRegistroAsync(Cliente cliente);

        /// <summary>
        /// Envía email de restablecimiento de contraseña
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="tokenReset">Token para restablecer contraseña</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarRestablecimientoPasswordAsync(string email, string tokenReset);

        // ============================================================================
        // NOTIFICACIONES GENERALES
        // ============================================================================

        /// <summary>
        /// Envía notificación personalizada
        /// </summary>
        /// <param name="destinatario">Email del destinatario</param>
        /// <param name="asunto">Asunto del email</param>
        /// <param name="mensaje">Mensaje del email</param>
        /// <param name="esHtml">Indica si el mensaje es HTML</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> EnviarNotificacionPersonalizadaAsync(string destinatario, string asunto, string mensaje, bool esHtml = false);

        /// <summary>
        /// Envía promoción o newsletter
        /// </summary>
        /// <param name="destinatarios">Lista de emails destinatarios</param>
        /// <param name="asunto">Asunto de la promoción</param>
        /// <param name="contenidoHtml">Contenido HTML de la promoción</param>
        /// <returns>Número de emails enviados exitosamente</returns>
        Task<int> EnviarPromocionAsync(List<string> destinatarios, string asunto, string contenidoHtml);

        // ============================================================================
        // NOTIFICACIONES INTERNAS
        // ============================================================================

        /// <summary>
        /// Envía notificación de orden crítica a administradores
        /// </summary>
        /// <param name="orden">Orden con retraso crítico</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> NotificarOrdenCriticaAsync(Orden orden);

        /// <summary>
        /// Envía notificación de stock bajo
        /// </summary>
        /// <param name="producto">Producto con stock bajo</param>
        /// <returns>True si se envió exitosamente</returns>
        Task<bool> NotificarStockBajoAsync(Producto producto);

        // ============================================================================
        // UTILIDADES Y CONFIGURACIÓN
        // ============================================================================

        /// <summary>
        /// Valida si una dirección de email es válida
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <returns>True si es válido</returns>
        bool ValidarEmail(string email);

        /// <summary>
        /// Obtiene el historial de emails enviados
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio (opcional)</param>
        /// <param name="fechaFin">Fecha de fin (opcional)</param>
        /// <returns>Lista de transacciones de email</returns>
        Task<List<EmailTransaccion>> GetHistorialEmailsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Verifica si el servicio de email está disponible
        /// </summary>
        /// <returns>True si está disponible</returns>
        Task<bool> VerificarConexionEmailAsync();

        /// <summary>
        /// Obtiene estadísticas básicas de emails
        /// </summary>
        /// <returns>Estadísticas de envío de emails</returns>
        Task<EstadisticasEmailViewModel> GetEstadisticasEmailAsync();
    }

    // ============================================================================
    // VIEWMODELS Y MODELOS AUXILIARES
    // ============================================================================

    /// <summary>
    /// ViewModel para estadísticas de email
    /// </summary>
    public class EstadisticasEmailViewModel
    {
        /// <summary>
        /// Total de emails enviados hoy
        /// </summary>
        public int EmailsEnviadosHoy { get; set; }

        /// <summary>
        /// Total de emails enviados esta semana
        /// </summary>
        public int EmailsEnviadosSemana { get; set; }

        /// <summary>
        /// Total de emails enviados este mes
        /// </summary>
        public int EmailsEnviadosMes { get; set; }

        /// <summary>
        /// Emails fallidos hoy
        /// </summary>
        public int EmailsFallidosHoy { get; set; }

        /// <summary>
        /// Porcentaje de éxito en envíos
        /// </summary>
        public decimal PorcentajeExito { get; set; }

        /// <summary>
        /// Tipos de email más enviados
        /// </summary>
        public Dictionary<string, int> EmailsPorTipo { get; set; } = new();

        /// <summary>
        /// Último email enviado
        /// </summary>
        public DateTime? UltimoEmailEnviado { get; set; }

        /// <summary>
        /// Estado del servicio de email
        /// </summary>
        public string EstadoServicio { get; set; } = "Activo";
    }

    /// <summary>
    /// Resultado de envío de email
    /// </summary>
    public class ResultadoEmail
    {
        /// <summary>
        /// Indica si el envío fue exitoso
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Mensaje de error si falló
        /// </summary>
        public string? MensajeError { get; set; }

        /// <summary>
        /// ID de transacción del email
        /// </summary>
        public int? TransaccionId { get; set; }

        /// <summary>
        /// Fecha y hora del envío
        /// </summary>
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
    }
}