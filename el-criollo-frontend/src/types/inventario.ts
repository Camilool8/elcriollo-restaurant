import type { Producto } from './producto';

export interface Inventario {
  inventarioID: number;
  productoID: number;
  producto?: Producto;
  cantidadDisponible: number;
  cantidadMinima: number;
  ultimaActualizacion: string;
}

export interface MovimientoInventario {
  movimientoID: number;
  productoID: number;
  producto?: Producto;
  tipoMovimiento: TipoMovimiento;
  cantidad: number;
  stockAnterior: number;
  stockResultante: number;
  costoUnitario?: number;
  referencia?: string;
  usuario: string;
  observaciones?: string;
  fechaMovimiento: string;
  motivo?: string;
  proveedor?: string;
}

export type TipoMovimiento = 'Entrada' | 'Salida' | 'Ajuste';
