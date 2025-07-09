import type { Cliente } from './cliente';
import type { Mesa } from './mesa';

export interface Reservacion {
  reservacionID: number;
  numeroReservacion: string;
  clienteID: number;
  mesaID: number;
  cantidadPersonas: number;
  fechaHora: string;
  duracionEstimada: number; // en minutos
  observaciones?: string;
  estado: EstadoReservacion;
  fechaCreacion: string;
  fechaActualizacion: string;
  cliente?: ClienteBasico;
  mesa?: MesaBasica;
}

export type EstadoReservacion =
  | 'Pendiente'
  | 'Confirmada'
  | 'En Curso'
  | 'Completada'
  | 'Cancelada'
  | 'No Show';

export interface CrearReservacionRequest {
  clienteID: number;
  mesaID: number;
  cantidadPersonas: number;
  fechaHora: string;
  duracionEstimada?: number;
  observaciones?: string;
}

export interface ActualizarReservacionRequest {
  cantidadPersonas?: number;
  fechaHora?: string;
  duracionEstimada?: number;
  observaciones?: string;
  estado?: EstadoReservacion;
}

export interface FiltrosReservacion {
  estado?: EstadoReservacion;
  fechaDesde?: string;
  fechaHasta?: string;
  clienteID?: number;
  mesaID?: number;
}

export interface EstadisticasReservacion {
  totalReservaciones: number;
  reservacionesPendientes: number;
  reservacionesConfirmadas: number;
  reservacionesEnCurso: number;
  reservacionesCompletadas: number;
  reservacionesCanceladas: number;
  reservacionesNoShow: number;
  promedioPersonas: number;
}

export interface ClienteBasico {
  clienteID: number;
  nombreCompleto: string;
  telefono?: string;
  email?: string;
}

export interface MesaBasica {
  mesaID: number;
  numeroMesa: number;
  capacidad: number;
  ubicacion?: string;
  estado: string;
}

export interface ReservacionConDetalles extends Reservacion {
  cliente: ClienteBasico;
  mesa: MesaBasica;
  tiempoRestante?: string;
  tiempoTranscurrido?: string;
  estaRetrasada: boolean;
  proximaALlegar: boolean;
}

export interface HorarioDisponible {
  hora: string;
  disponible: boolean;
  reservacionesExistentes: number;
}

export interface DisponibilidadMesa {
  mesaID: number;
  numeroMesa: number;
  capacidad: number;
  disponible: boolean;
  reservacionesExistentes: ReservacionBasica[];
  horariosDisponibles: HorarioDisponible[];
}

export interface ReservacionBasica {
  reservacionID: number;
  numeroReservacion: string;
  fechaHora: string;
  cantidadPersonas: number;
  estado: EstadoReservacion;
}
