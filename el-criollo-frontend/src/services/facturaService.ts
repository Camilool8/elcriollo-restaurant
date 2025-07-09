import { api } from './api';
import { getErrorMessage } from './api';
import {
  calcularITBIS,
  desglosarFactura,
  generarNumeroFactura,
} from '@/utils/dominicanValidations';
import type {
  Factura,
  CrearFacturaRequest,
  FacturaDividida,
  DivisionFacturaRequest,
  DivisionCliente,
  ResumenDivisionFactura,
  PagoRequest,
  PagoResponse,
  EstadisticasFacturacion,
  ReporteFacturacion,
  MetodoPago,
  FacturaEstado,
  Orden,
  DetalleOrden,
  Cliente,
  ClienteOcasional,
  DetalleOrdenAsignado,
} from '@/types';

class FacturaService {
  // ============================================================================
  // MÉTODOS BÁSICOS DE FACTURACIÓN
  // ============================================================================

  /**
   * Crear una factura individual para una orden específica
   */
  async crearFactura(request: CrearFacturaRequest): Promise<Factura> {
    try {
      const response = await api.post<Factura>('/Factura', request);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando factura: ${message}`);
    }
  }

  /**
   * Crear factura grupal para todas las órdenes de una mesa
   */
  async crearFacturaGrupal(
    mesaId: number,
    metodoPago: MetodoPago,
    descuento?: number,
    propina?: number
  ): Promise<Factura> {
    try {
      const request = {
        metodoPago,
        descuento,
        propina,
      };
      const response = await api.post<Factura>(`/Factura/mesa/${mesaId}/factura-grupal`, request);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando factura grupal: ${message}`);
    }
  }

  /**
   * Obtener una factura por ID
   */
  async obtenerFactura(facturaId: number): Promise<Factura> {
    try {
      const response = await api.get<Factura>(`/Factura/${facturaId}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo factura: ${message}`);
    }
  }

  /**
   * Obtener todas las facturas del día
   */
  async obtenerFacturasDelDia(fecha?: Date): Promise<Factura[]> {
    try {
      const fechaConsulta = fecha || new Date();
      const response = await api.get<Factura[]>('/Factura/dia', {
        params: { fecha: fechaConsulta.toISOString() },
      });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo facturas del día: ${message}`);
    }
  }

  /**
   * Obtener facturas por rango de fechas
   */
  async obtenerFacturasPorRango(fechaInicio: Date, fechaFin: Date): Promise<Factura[]> {
    try {
      const response = await api.get<Factura[]>('/Factura/rango', {
        params: {
          fechaInicio: fechaInicio.toISOString(),
          fechaFin: fechaFin.toISOString(),
        },
      });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo facturas por rango: ${message}`);
    }
  }

  /**
   * Anular una factura
   */
  async anularFactura(facturaId: number, motivo: string): Promise<void> {
    try {
      await api.post(`/Factura/${facturaId}/anular`, { motivo });
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error anulando factura: ${message}`);
    }
  }

  /**
   * Marcar una factura como pagada
   */
  async marcarComoPagada(facturaId: number, metodoPago: MetodoPago): Promise<void> {
    try {
      await api.post(`/Factura/${facturaId}/marcar-pagada`, { metodoPago });
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error marcando factura como pagada: ${message}`);
    }
  }

  /**
   * Enviar factura por email
   */
  async enviarPorEmail(facturaId: number, email: string): Promise<void> {
    try {
      await api.post(`/Factura/${facturaId}/enviar-email`, { email });
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error enviando factura por email: ${message}`);
    }
  }

  // ============================================================================
  // DIVISIÓN DE FACTURAS (SIMULADA EN FRONTEND)
  // ============================================================================

  /**
   * Dividir una factura entre múltiples clientes
   * Esta funcionalidad simula la división en el frontend y crea múltiples facturas individuales
   */
  async dividirFactura(divisionRequest: DivisionFacturaRequest): Promise<ResumenDivisionFactura> {
    try {
      // 1. Obtener la orden original
      const ordenResponse = await api.get<Orden>(`/Orden/${divisionRequest.ordenID}`);
      const orden = ordenResponse;

      if (!orden.detalles || orden.detalles.length === 0) {
        throw new Error('La orden no tiene items para dividir');
      }

      // 2. Validar que todos los items están asignados
      const itemsAsignados = divisionRequest.divisiones.flatMap((d) => d.itemsAsignados);
      const todosLosItems = orden.detalles.map((d: DetalleOrden) => d.detalleOrdenID);

      if (itemsAsignados.length !== todosLosItems.length) {
        throw new Error('Todos los items deben estar asignados a un cliente');
      }

      // 3. Crear facturas individuales para cada división
      const facturas: FacturaDividida[] = [];
      const facturaGrupoId = `div-${Date.now()}`;

      for (const division of divisionRequest.divisiones) {
        const factura = await this.crearFacturaParaDivision(
          orden,
          division,
          facturaGrupoId,
          divisionRequest.aplicarDescuentoProporcionalmente,
          divisionRequest.aplicarPropinaProporcionalmente
        );
        facturas.push(factura);
      }

      // 4. Crear resumen de la división
      const resumen: ResumenDivisionFactura = {
        ordenID: orden.ordenID,
        numeroOrden: orden.numeroOrden,
        mesa: orden.mesa,
        totalOriginal: orden.totalCalculado,
        totalDividido: facturas.reduce((sum, f) => sum + f.total, 0),
        cantidadFacturas: facturas.length,
        facturas,
        fechaCreacion: new Date().toISOString(),
        estado: 'Completada',
      };

      return resumen;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error dividiendo factura: ${message}`);
    }
  }

  /**
   * Crear una factura individual para una división específica
   */
  private async crearFacturaParaDivision(
    orden: Orden,
    division: DivisionCliente,
    facturaGrupoId: string,
    aplicarDescuentoProporcionalmente?: boolean,
    aplicarPropinaProporcionalmente?: boolean
  ): Promise<FacturaDividida> {
    // Calcular items asignados a este cliente
    const itemsAsignados: DetalleOrdenAsignado[] = orden
      .detalles!.filter((detalle: DetalleOrden) =>
        division.itemsAsignados.includes(detalle.detalleOrdenID)
      )
      .map((detalle: DetalleOrden) => ({
        detalleOrdenID: detalle.detalleOrdenID,
        productoID: detalle.producto?.productoID || 0,
        nombreProducto: detalle.producto?.nombre || 'Producto desconocido',
        cantidad: detalle.cantidad,
        precioUnitario: parseFloat(detalle.precioUnitario),
        subtotal: parseFloat(detalle.subtotal),
        notasEspeciales: detalle.observaciones,
        asignadoA: division.cliente,
      }));

    // Calcular subtotal de los items asignados
    const subtotalItems = itemsAsignados.reduce((sum, item) => sum + item.subtotal, 0);

    // Calcular proporciones para descuentos y propinas
    const totalOrden = orden.subtotalCalculado;
    const proporcion = subtotalItems / totalOrden;

    // Aplicar descuentos
    let descuento = 0;
    if (division.descuentoPersonalizado !== undefined) {
      descuento = division.descuentoPersonalizado;
    } else if (
      aplicarDescuentoProporcionalmente &&
      orden.totalCalculado > orden.subtotalCalculado
    ) {
      // Si hay descuento en la orden original, aplicarlo proporcionalmente
      const descuentoOriginal = totalOrden - orden.subtotalCalculado;
      descuento = descuentoOriginal * proporcion;
    }

    // Aplicar propinas
    let propina = 0;
    if (division.propinaPersonalizada !== undefined) {
      propina = division.propinaPersonalizada;
    } else if (aplicarPropinaProporcionalmente) {
      // Aplicar propina proporcionalmente (asumiendo que hay propina en la orden)
      propina = 0; // Por defecto 0, se puede personalizar
    }

    // Calcular totales
    const subtotal = subtotalItems - descuento;
    const impuesto = calcularITBIS(subtotal);
    const total = subtotal + impuesto + propina;

    // Crear factura individual via API
    const facturaRequest: CrearFacturaRequest = {
      ordenID: orden.ordenID,
      metodoPago: division.metodoPago,
      descuento,
      propina,
      observacionesPago:
        division.observacionesPago ||
        `Factura dividida - Cliente: ${this.obtenerNombreCliente(division.cliente)}`,
    };

    const facturaCreada = await this.crearFactura(facturaRequest);

    // Convertir a FacturaDividida
    const facturaDividida: FacturaDividida = {
      facturaID: facturaCreada.facturaID,
      numeroFactura: facturaCreada.numeroFactura,
      ordenID: facturaCreada.ordenID,
      clienteAsignado: division.cliente,
      itemsAsignados,
      subtotal,
      impuesto,
      descuento,
      propina,
      total,
      metodoPago: division.metodoPago,
      estado: facturaCreada.estado,
      fechaFactura: facturaCreada.fechaFactura,
      fechaPago: facturaCreada.fechaPago,
      observacionesPago: facturaCreada.observacionesPago,
      esParte: true,
      facturaGrupoId,
    };

    return facturaDividida;
  }

  /**
   * Obtener el nombre completo de un cliente (registrado u ocasional)
   */
  private obtenerNombreCliente(cliente: Cliente | ClienteOcasional): string {
    if ('esOcasional' in cliente) {
      return `${cliente.nombre} ${cliente.apellido || ''}`.trim();
    } else {
      return cliente.nombreCompleto;
    }
  }

  // ============================================================================
  // PROCESAMIENTO DE PAGOS
  // ============================================================================

  /**
   * Procesar un pago para una factura
   */
  async procesarPago(pagoRequest: PagoRequest): Promise<PagoResponse> {
    try {
      // En este caso, simplemente marcamos la factura como pagada
      await this.marcarComoPagada(pagoRequest.facturaID, pagoRequest.metodoPago);

      // Simular respuesta de pago
      const pagoResponse: PagoResponse = {
        pagoID: Date.now(), // ID simulado
        facturaID: pagoRequest.facturaID,
        numeroFactura: `FACT-${Date.now()}`, // Se obtendría de la factura real
        metodoPago: pagoRequest.metodoPago,
        montoPagado: pagoRequest.montoPagado,
        fechaPago: new Date().toISOString(),
        referenciaPago: pagoRequest.referenciaPago,
        estadoPago: 'Completado',
        observaciones: pagoRequest.observaciones,
      };

      return pagoResponse;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error procesando pago: ${message}`);
    }
  }

  // ============================================================================
  // REPORTES Y ESTADÍSTICAS
  // ============================================================================

  /**
   * Obtener resumen de ventas del día
   */
  async obtenerResumenVentas(fecha?: Date): Promise<any> {
    try {
      const fechaConsulta = fecha || new Date();
      const response = await api.get('/Factura/resumen-ventas', {
        params: { fecha: fechaConsulta.toISOString() },
      });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo resumen de ventas: ${message}`);
    }
  }

  /**
   * Obtener estadísticas de facturación
   */
  async obtenerEstadisticasFacturacion(
    fechaInicio: Date,
    fechaFin: Date
  ): Promise<EstadisticasFacturacion> {
    try {
      const response = await api.get('/Factura/estadisticas', {
        params: {
          fechaInicio: fechaInicio.toISOString(),
          fechaFin: fechaFin.toISOString(),
        },
      });
      return response as EstadisticasFacturacion;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo estadísticas: ${message}`);
    }
  }

  /**
   * Generar reporte de facturación
   */
  async generarReporte(fechaInicio: Date, fechaFin: Date): Promise<ReporteFacturacion> {
    try {
      const facturas = await this.obtenerFacturasPorRango(fechaInicio, fechaFin);

      // Calcular estadísticas del reporte
      const ventasBrutas = facturas.reduce((sum, f) => sum + f.subtotal, 0);
      const totalDescuentos = facturas.reduce((sum, f) => sum + f.descuento, 0);
      const totalPropinas = facturas.reduce((sum, f) => sum + f.propina, 0);
      const totalImpuestos = facturas.reduce((sum, f) => sum + f.impuesto, 0);
      const ventasNetas = facturas.reduce((sum, f) => sum + f.total, 0);

      // Distribución por método de pago
      const facturasPorMetodo = facturas.reduce(
        (acc, f) => {
          acc[f.metodoPago] = (acc[f.metodoPago] || 0) + 1;
          return acc;
        },
        {} as Record<MetodoPago, number>
      );

      const ventasPorMetodo = facturas.reduce(
        (acc, f) => {
          acc[f.metodoPago] = (acc[f.metodoPago] || 0) + f.total;
          return acc;
        },
        {} as Record<MetodoPago, number>
      );

      const reporte: ReporteFacturacion = {
        fechaInicio: fechaInicio.toISOString(),
        fechaFin: fechaFin.toISOString(),
        totalFacturas: facturas.length,
        ventasBrutas,
        totalDescuentos,
        totalPropinas,
        totalImpuestos,
        ventasNetas,
        facturasPorMetodo,
        ventasPorMetodo,
        promedioFactura: facturas.length > 0 ? ventasNetas / facturas.length : 0,
        facturaMaxima: Math.max(...facturas.map((f) => f.total), 0),
        facturaMinima: Math.min(...facturas.map((f) => f.total), 0),
      };

      return reporte;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error generando reporte: ${message}`);
    }
  }

  // ============================================================================
  // UTILIDADES
  // ============================================================================

  /**
   * Validar si una factura puede ser dividida
   */
  puedeSerDividida(factura: Factura): boolean {
    return factura.estado === 'Pendiente';
  }

  /**
   * Validar si una factura puede ser anulada
   */
  puedeSerAnulada(factura: Factura): boolean {
    return factura.estado === 'Pendiente' || factura.estado === 'Pagada';
  }

  /**
   * Calcular totales de una factura
   */
  calcularTotales(
    subtotal: number,
    descuento: number = 0,
    propina: number = 0
  ): { subtotal: number; impuesto: number; total: number } {
    const resultado = desglosarFactura(subtotal, descuento, propina);
    return {
      subtotal: resultado.subtotal,
      impuesto: resultado.itbis,
      total: resultado.total,
    };
  }

  /**
   * Formatear número de factura
   */
  generarNumeroFactura(): string {
    return generarNumeroFactura();
  }

  /**
   * Obtener métodos de pago disponibles
   */
  getMetodosPagoDisponibles(): MetodoPago[] {
    return [
      'Efectivo',
      'Tarjeta de Débito',
      'Tarjeta de Crédito',
      'Transferencia Bancaria',
      'Pago Móvil',
      'Cheque',
    ];
  }

  /**
   * Obtener estados de factura disponibles
   */
  getEstadosFactura(): FacturaEstado[] {
    return ['Pendiente', 'Pagada', 'Anulada'];
  }
}

// Exportar instancia singleton
export const facturaService = new FacturaService();
export default facturaService;
