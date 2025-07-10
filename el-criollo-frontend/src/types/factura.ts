import type { Orden } from './orden';
import type { Cliente } from './cliente';
import type { Empleado } from './empleado';

export interface Factura {
  facturaID: number;
  numeroFactura: string;
  ordenID: number;
  orden?: Orden;
  clienteID: number;
  cliente?: Cliente;
  empleadoID: number;
  empleado?: Empleado;
  fechaFactura: string;
  fechaPago?: string;
  subtotal: number;
  impuesto: number; // ITBIS 18%
  descuento: number;
  propina: number;
  total: number;
  metodoPago: MetodoPago;
  estado: FacturaEstado;
  observacionesPago?: string;
}

export type MetodoPago =
  | 'Efectivo'
  | 'Tarjeta de Débito'
  | 'Tarjeta de Crédito'
  | 'Transferencia Bancaria'
  | 'Pago Móvil'
  | 'Cheque';

export type FacturaEstado = 'Pendiente' | 'Pagada' | 'Anulada';

export interface CrearFacturaRequest {
  ordenID: number;
  metodoPago: MetodoPago;
  descuento?: number;
  propina?: number;
  observacionesPago?: string;
}

export interface DetalleOrdenAsignado {
  detalleOrdenID: number;
  productoID: number;
  nombreProducto: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
  notasEspeciales?: string;
  asignadoA: Cliente | ClienteOcasional;
}

export interface ClienteOcasional {
  nombre: string;
  apellido?: string;
  telefono?: string;
  email?: string;
  cedula?: string;
  esOcasional: true;
}

export interface PagoRequest {
  facturaID: number;
  metodoPago: MetodoPago;
  montoPagado: number;
  referenciaPago?: string;
  observaciones?: string;
}

export interface PagoResponse {
  pagoID: number;
  facturaID: number;
  numeroFactura: string;
  metodoPago: MetodoPago;
  montoPagado: number;
  fechaPago: string;
  referenciaPago?: string;
  estadoPago: 'Completado' | 'Pendiente' | 'Fallido';
  observaciones?: string;
}

export interface EstadisticasFacturacion {
  totalFacturasHoy: number;
  ventasDelDia: number;
  ventasDelMes: number;
  promedioFactura: number;
  facturasPendientes: number;
  facturasPagadas: number;
  facturasMayorVenta: Factura[];
  metodoPagoMasUsado: MetodoPago;
  distribucionMetodosPago: Record<MetodoPago, number>;
}

export interface ReporteFacturacion {
  fechaInicio: string;
  fechaFin: string;
  totalFacturas: number;
  ventasBrutas: number;
  totalDescuentos: number;
  totalPropinas: number;
  totalImpuestos: number;
  ventasNetas: number;
  facturasPorMetodo: Record<MetodoPago, number>;
  ventasPorMetodo: Record<MetodoPago, number>;
  promedioFactura: number;
  facturaMaxima: number;
  facturaMinima: number;
}
