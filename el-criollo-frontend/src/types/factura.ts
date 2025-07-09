import type { Orden } from './orden';
import type { Cliente } from './cliente';
import type { Empleado } from './empleado';
import type { DetalleOrden } from './orden';
import type { Mesa } from './mesa';

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

export interface FacturaDividida {
  facturaID: number;
  numeroFactura: string;
  ordenID: number;
  clienteAsignado: Cliente | ClienteOcasional;
  itemsAsignados: DetalleOrdenAsignado[];
  subtotal: number;
  impuesto: number;
  descuento: number;
  propina: number;
  total: number;
  metodoPago: MetodoPago;
  estado: FacturaEstado;
  fechaFactura: string;
  fechaPago?: string;
  observacionesPago?: string;
  esParte: boolean; // Indica si es parte de una división
  facturaGrupoId?: string; // ID del grupo de facturas divididas
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

export interface DivisionFacturaRequest {
  ordenID: number;
  divisiones: DivisionCliente[];
  aplicarDescuentoProporcionalmente?: boolean;
  aplicarPropinaProporcionalmente?: boolean;
}

export interface DivisionCliente {
  cliente: Cliente | ClienteOcasional;
  itemsAsignados: number[]; // Array de detalleOrdenIDs
  descuentoPersonalizado?: number;
  propinaPersonalizada?: number;
  metodoPago: MetodoPago;
  observacionesPago?: string;
}

export interface ResumenDivisionFactura {
  ordenID: number;
  numeroOrden: string;
  mesa?: Mesa;
  totalOriginal: number;
  totalDividido: number;
  cantidadFacturas: number;
  facturas: FacturaDividida[];
  fechaCreacion: string;
  estado: 'Pendiente' | 'Completada' | 'Parcial';
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
