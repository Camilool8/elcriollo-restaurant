using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElCriollo.API.Models.Entities;

/// <summary>
/// Entidad que representa las facturas generadas por las órdenes
/// </summary>
[Table("Facturas")]
public class Factura
{
    /// <summary>
    /// Identificador único de la factura
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int FacturaID { get; set; }

    /// <summary>
    /// Número de factura único generado automáticamente (ej: FACT-20241205-0001)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string NumeroFactura { get; set; } = string.Empty;

    /// <summary>
    /// Orden que se está facturando (FK a Ordenes)
    /// </summary>
    [Required]
    [ForeignKey("Orden")]
    public int OrdenID { get; set; }

    /// <summary>
    /// Cliente al que se le factura (FK a Clientes)
    /// </summary>
    [Required]
    [ForeignKey("Cliente")]
    public int ClienteID { get; set; }

    /// <summary>
    /// Empleado que procesa la factura (FK a Empleados)
    /// </summary>
    [Required]
    [ForeignKey("Empleado")]
    public int EmpleadoID { get; set; }

    /// <summary>
    /// Fecha y hora de generación de la factura
    /// </summary>
    [Required]
    public DateTime FechaFactura { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Subtotal de la factura (suma de productos/combos sin impuestos)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El subtotal debe ser mayor a 0")]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Impuestos aplicados (ITBIS en República Dominicana = 18%)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 999999.99, ErrorMessage = "El impuesto no puede ser negativo")]
    public decimal Impuesto { get; set; } = 0;

    /// <summary>
    /// Descuento total aplicado a la factura
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 999999.99, ErrorMessage = "El descuento no puede ser negativo")]
    public decimal Descuento { get; set; } = 0;

    /// <summary>
    /// Propina dejada por el cliente
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 999999.99, ErrorMessage = "La propina no puede ser negativa")]
    public decimal Propina { get; set; } = 0;

    /// <summary>
    /// Total final de la factura (Subtotal + Impuesto - Descuento + Propina)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El total debe ser mayor a 0")]
    public decimal Total { get; set; }

    /// <summary>
    /// Método de pago utilizado (Efectivo, Tarjeta, Transferencia)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string MetodoPago { get; set; } = "Efectivo";

    /// <summary>
    /// Estado de la factura (Pendiente, Pagada, Anulada)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Pagada";

    // ============================================================================
    // NAVEGACIÓN - RELACIONES
    // ============================================================================

    /// <summary>
    /// Orden que se está facturando
    /// </summary>
    public virtual Orden Orden { get; set; } = null!;

    /// <summary>
    /// Cliente al que se le factura
    /// </summary>
    public virtual Cliente Cliente { get; set; } = null!;

    /// <summary>
    /// Empleado que procesa la factura
    /// </summary>
    public virtual Empleado Empleado { get; set; } = null!;

    // ============================================================================
    // PROPIEDADES CALCULADAS
    // ============================================================================

    /// <summary>
    /// Indica si la factura está pendiente de pago
    /// </summary>
    [NotMapped]
    public bool EstaPendiente => Estado == "Pendiente";

    /// <summary>
    /// Indica si la factura está pagada
    /// </summary>
    [NotMapped]
    public bool EstaPagada => Estado == "Pagada";

    /// <summary>
    /// Indica si la factura está anulada
    /// </summary>
    [NotMapped]
    public bool EstaAnulada => Estado == "Anulada";

    /// <summary>
    /// Indica si el pago fue en efectivo
    /// </summary>
    [NotMapped]
    public bool EsEfectivo => MetodoPago == "Efectivo";

    /// <summary>
    /// Indica si el pago fue con tarjeta
    /// </summary>
    [NotMapped]
    public bool EsTarjeta => MetodoPago == "Tarjeta";

    /// <summary>
    /// Indica si el pago fue por transferencia
    /// </summary>
    [NotMapped]
    public bool EsTransferencia => MetodoPago == "Transferencia";

    /// <summary>
    /// Porcentaje de impuesto aplicado
    /// </summary>
    [NotMapped]
    public decimal PorcentajeImpuesto => Subtotal > 0 ? (Impuesto / Subtotal) * 100 : 0;

    /// <summary>
    /// Porcentaje de descuento aplicado
    /// </summary>
    [NotMapped]
    public decimal PorcentajeDescuento => Subtotal > 0 ? (Descuento / Subtotal) * 100 : 0;

    /// <summary>
    /// Porcentaje de propina respecto al subtotal
    /// </summary>
    [NotMapped]
    public decimal PorcentajePropina => Subtotal > 0 ? (Propina / Subtotal) * 100 : 0;

    /// <summary>
    /// Subtotal antes de impuestos y descuentos
    /// </summary>
    [NotMapped]
    public decimal SubtotalBruto => Subtotal + Descuento;

    /// <summary>
    /// Base gravable para el impuesto (subtotal después de descuento)
    /// </summary>
    [NotMapped]
    public decimal BaseGravable => Subtotal - Descuento;

    /// <summary>
    /// Total sin propina
    /// </summary>
    [NotMapped]
    public decimal TotalSinPropina => Total - Propina;

    /// <summary>
    /// Mesa donde se originó la factura
    /// </summary>
    [NotMapped]
    public int? NumeroMesa => Orden?.Mesa?.NumeroMesa;

    /// <summary>
    /// Tipo de orden facturada
    /// </summary>
    [NotMapped]
    public string? TipoOrden => Orden?.TipoOrden;

    /// <summary>
    /// Cantidad de items facturados
    /// </summary>
    [NotMapped]
    public int TotalItems => Orden?.TotalItems ?? 0;

    /// <summary>
    /// Tiempo transcurrido desde la facturación
    /// </summary>
    [NotMapped]
    public TimeSpan TiempoTranscurrido => DateTime.UtcNow - FechaFactura;

    /// <summary>
    /// Indica si es una factura del día actual
    /// </summary>
    [NotMapped]
    public bool EsDelDiaActual => FechaFactura.Date == DateTime.Now.Date;

    /// <summary>
    /// Formatos para mostrar
    /// </summary>
    [NotMapped]
    public string SubtotalFormateado => $"RD$ {Subtotal:N2}";

    [NotMapped]
    public string ImpuestoFormateado => $"RD$ {Impuesto:N2}";

    [NotMapped]
    public string DescuentoFormateado => $"RD$ {Descuento:N2}";

    [NotMapped]
    public string PropinaFormateado => $"RD$ {Propina:N2}";

    [NotMapped]
    public string TotalFormateado => $"RD$ {Total:N2}";

    // ============================================================================
    // MÉTODOS DE UTILIDAD
    // ============================================================================

    /// <summary>
    /// Calcula automáticamente el total de la factura
    /// </summary>
    public void CalcularTotal()
    {
        Total = Subtotal + Impuesto - Descuento + Propina;
    }

    /// <summary>
    /// Calcula el impuesto basado en la tasa dominicana (18% ITBIS)
    /// </summary>
    public void CalcularImpuesto(decimal tasaImpuesto = 0.18m)
    {
        if (tasaImpuesto < 0 || tasaImpuesto > 1)
            throw new ArgumentException("La tasa de impuesto debe estar entre 0 y 1");

        var baseGravable = Subtotal - Descuento;
        Impuesto = Math.Round(baseGravable * tasaImpuesto, 2);
        CalcularTotal();
    }

    /// <summary>
    /// Aplica un descuento en monto fijo
    /// </summary>
    public void AplicarDescuento(decimal montoDescuento)
    {
        if (montoDescuento < 0)
            throw new ArgumentException("El descuento no puede ser negativo");

        if (montoDescuento > Subtotal)
            throw new ArgumentException("El descuento no puede ser mayor al subtotal");

        if (!EstaPendiente)
            throw new InvalidOperationException("Solo se puede aplicar descuentos a facturas pendientes");

        Descuento = montoDescuento;
        CalcularImpuesto(); // Recalcular impuesto después del descuento
    }

    /// <summary>
    /// Aplica un descuento por porcentaje
    /// </summary>
    public void AplicarDescuentoPorcentaje(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100");

        var montoDescuento = Subtotal * (porcentaje / 100);
        AplicarDescuento(montoDescuento);
    }

    /// <summary>
    /// Establece la propina
    /// </summary>
    public void EstablecerPropina(decimal montoPropina)
    {
        if (montoPropina < 0)
            throw new ArgumentException("La propina no puede ser negativa");

        Propina = montoPropina;
        CalcularTotal();
    }

    /// <summary>
    /// Establece la propina por porcentaje del subtotal
    /// </summary>
    public void EstablecerPropinaPorcentaje(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100");

        var montoPropina = Subtotal * (porcentaje / 100);
        EstablecerPropina(montoPropina);
    }

    /// <summary>
    /// Cambia el método de pago
    /// </summary>
    public void CambiarMetodoPago(string nuevoMetodo)
    {
        var metodosValidos = new[] { "Efectivo", "Tarjeta", "Transferencia" };
        
        if (!metodosValidos.Contains(nuevoMetodo))
            throw new ArgumentException($"Método de pago inválido: {nuevoMetodo}");

        if (EstaAnulada)
            throw new InvalidOperationException("No se puede cambiar el método de pago de una factura anulada");

        MetodoPago = nuevoMetodo;
    }

    /// <summary>
    /// Marca la factura como pagada
    /// </summary>
    public void MarcarComoPagada()
    {
        if (EstaAnulada)
            throw new InvalidOperationException("No se puede marcar como pagada una factura anulada");

        Estado = "Pagada";
    }

    /// <summary>
    /// Anula la factura
    /// </summary>
    public void Anular()
    {
        if (EstaPagada && TiempoTranscurrido.TotalHours > 24)
            throw new InvalidOperationException("No se puede anular una factura pagada después de 24 horas");

        Estado = "Anulada";
    }

    /// <summary>
    /// Valida que la factura sea válida
    /// </summary>
    public List<string> ValidarFactura()
    {
        var errores = new List<string>();

        if (Subtotal <= 0)
            errores.Add("El subtotal debe ser mayor a 0");

        if (Impuesto < 0)
            errores.Add("El impuesto no puede ser negativo");

        if (Descuento < 0)
            errores.Add("El descuento no puede ser negativo");

        if (Descuento > Subtotal)
            errores.Add("El descuento no puede ser mayor al subtotal");

        if (Propina < 0)
            errores.Add("La propina no puede ser negativa");

        if (Total <= 0)
            errores.Add("El total debe ser mayor a 0");

        if (string.IsNullOrEmpty(MetodoPago))
            errores.Add("Debe especificar un método de pago");

        if (OrdenID <= 0)
            errores.Add("Debe estar asociada a una orden válida");

        if (ClienteID <= 0)
            errores.Add("Debe estar asociada a un cliente válido");

        if (EmpleadoID <= 0)
            errores.Add("Debe estar asociada a un empleado válido");

        // Validar que el total calculado coincida
        var totalCalculado = Subtotal + Impuesto - Descuento + Propina;
        if (Math.Abs(Total - totalCalculado) > 0.01m)
            errores.Add("El total no coincide con el cálculo (Subtotal + Impuesto - Descuento + Propina)");

        return errores;
    }

    /// <summary>
    /// Calcula estadísticas de la factura
    /// </summary>
    public object CalcularEstadisticas()
    {
        return new
        {
            NumeroFactura = NumeroFactura,
            FechaFactura = FechaFactura.ToString("dd/MM/yyyy HH:mm"),
            Cliente = Cliente?.NombreCompleto,
            Mesa = NumeroMesa?.ToString() ?? "N/A",
            TipoOrden = TipoOrden,
            TotalItems = TotalItems,
            Subtotal = SubtotalFormateado,
            Impuesto = $"{ImpuestoFormateado} ({PorcentajeImpuesto:F1}%)",
            Descuento = Descuento > 0 ? $"{DescuentoFormateado} ({PorcentajeDescuento:F1}%)" : "N/A",
            Propina = Propina > 0 ? $"{PropinaFormateado} ({PorcentajePropina:F1}%)" : "N/A",
            Total = TotalFormateado,
            MetodoPago = MetodoPago,
            Estado = Estado,
            TiempoTranscurrido = $"{TiempoTranscurrido.Hours:D2}:{TiempoTranscurrido.Minutes:D2}"
        };
    }

    /// <summary>
    /// Genera el texto para imprimir en la factura física
    /// </summary>
    public string GenerarTextoFactura()
    {
        var texto = new System.Text.StringBuilder();
        
        texto.AppendLine("==========================================");
        texto.AppendLine("        RESTAURANTE EL CRIOLLO           ");
        texto.AppendLine("      Comida Dominicana Auténtica        ");
        texto.AppendLine("    Santo Domingo, República Dominicana   ");
        texto.AppendLine("         Tel: +1 (809) 555-0123          ");
        texto.AppendLine("==========================================");
        texto.AppendLine();
        texto.AppendLine($"Factura: {NumeroFactura}");
        texto.AppendLine($"Fecha: {FechaFactura:dd/MM/yyyy HH:mm}");
        texto.AppendLine($"Cliente: {Cliente?.NombreCompleto ?? "Cliente ocasional"}");
        texto.AppendLine($"Mesa: {NumeroMesa?.ToString() ?? "N/A"}");
        texto.AppendLine($"Orden: {Orden?.NumeroOrden}");
        texto.AppendLine($"Cajero: {Empleado?.NombreCompleto}");
        texto.AppendLine("------------------------------------------");
        
        // Detalles de productos
        if (Orden?.DetalleOrdenes != null)
        {
            foreach (var detalle in Orden.DetalleOrdenes)
            {
                texto.AppendLine($"{detalle.Cantidad}x {detalle.NombreItem}");
                texto.AppendLine($"    {detalle.PrecioUnitarioFormateado} c/u = {detalle.SubtotalFormateado}");
                if (!string.IsNullOrEmpty(detalle.Observaciones))
                    texto.AppendLine($"    Nota: {detalle.Observaciones}");
            }
        }
        
        texto.AppendLine("------------------------------------------");
        texto.AppendLine($"Subtotal:         {SubtotalFormateado}");
        
        if (Descuento > 0)
            texto.AppendLine($"Descuento:       -{DescuentoFormateado}");
        
        texto.AppendLine($"ITBIS (18%):      {ImpuestoFormateado}");
        
        if (Propina > 0)
            texto.AppendLine($"Propina:          {PropinaFormateado}");
        
        texto.AppendLine("------------------------------------------");
        texto.AppendLine($"TOTAL:            {TotalFormateado}");
        texto.AppendLine($"Método de pago:   {MetodoPago}");
        texto.AppendLine();
        texto.AppendLine("==========================================");
        texto.AppendLine("       ¡GRACIAS POR SU VISITA!          ");
        texto.AppendLine("      ¡Que disfrute su comida!          ");
        texto.AppendLine("==========================================");
        
        return texto.ToString();
    }

    /// <summary>
    /// Representación en string de la factura
    /// </summary>
    public override string ToString()
    {
        var cliente = Cliente?.NombreCompleto ?? "Cliente ocasional";
        var mesa = NumeroMesa?.ToString() ?? "Sin mesa";
        return $"{NumeroFactura} - {cliente} (Mesa {mesa}) - {TotalFormateado} - {Estado}";
    }
}