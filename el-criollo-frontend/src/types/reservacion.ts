import type { Cliente } from './cliente';
import type { Empleado } from './empleado';
import type { Mesa } from './mesa';

export interface Reservacion {
  reservacionID: number;
  clienteID: number;
  cliente?: Cliente;
  mesaID: number;
  mesa?: Mesa;
  fechaYHora: string;
  cantidadPersonas: number;
  estado: ReservacionEstado;
  observaciones?: string;
  fechaCreacion: string;
  empleadoID: number;
  empleado?: Empleado;
}

export type ReservacionEstado =
  | 'Pendiente'
  | 'Confirmada'
  | 'En Curso'
  | 'Completada'
  | 'Cancelada';

export interface CrearReservacionRequest {
  clienteID: number;
  mesaID: number;
  fechaYHora: string;
  cantidadPersonas: number;
  observaciones?: string;
}
