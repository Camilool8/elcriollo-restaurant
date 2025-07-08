export interface ClienteBasico {
  clienteID: number;
  nombreCompleto: string;
  telefono?: string;
}

export interface OrdenBasica {
  ordenID: number;
  numeroOrden: string;
  totalCalculado: number;
  estado: string;
  fechaCreacion: string;
}

export interface ReservacionBasica {
  reservacionID: number;
  numeroReservacion: string;
  cantidadPersonas: number;
  fechaHora: string;
  estado: string;
}

export interface Mesa {
  mesaID: number;
  numeroMesa: number;
  capacidad: number;
  ubicacion?: string;
  estado: EstadoMesa;
  descripcion: string;
  clienteActual?: ClienteBasico;
  ordenActual?: OrdenBasica;
  reservacionActual?: ReservacionBasica;
  tiempoOcupada?: string;
  necesitaLimpieza: boolean;
  fechaUltimaLimpieza?: string;
  requiereAtencion: boolean;
  tiempoHastaReserva?: string;
}

export type EstadoMesa = 'Libre' | 'Ocupada' | 'Reservada' | 'Mantenimiento';

export interface CambioEstadoMesaRequest {
  nuevoEstado: EstadoMesa;
  motivo?: string;
}

export interface MantenimientoMesaRequest {
  motivo: string;
  descripcion?: string;
}

export interface FiltrosMesa {
  estado?: EstadoMesa;
  capacidadMinima?: number;
  capacidadMaxima?: number;
  ubicacion?: string;
  soloDisponibles?: boolean;
}

export interface EstadisticasMesas {
  totalMesas: number;
  mesasLibres: number;
  mesasOcupadas: number;
  mesasReservadas: number;
  mesasMantenimiento: number;
  porcentajeOcupacion: number;
}

// Configuración de colores por estado usando las clases de Tailwind existentes
export const COLORES_ESTADO_MESA = {
  Libre: {
    bg: 'bg-palm-green',
    border: 'border-palm-green',
    text: 'text-white',
    icon: '🟢',
  },
  Ocupada: {
    bg: 'bg-dominican-red',
    border: 'border-dominican-red',
    text: 'text-white',
    icon: '🔴',
  },
  Reservada: {
    bg: 'bg-dominican-blue',
    border: 'border-dominican-blue',
    text: 'text-white',
    icon: '🔵',
  },
  Mantenimiento: {
    bg: 'bg-amber-500',
    border: 'border-amber-500',
    text: 'text-white',
    icon: '🟡',
  },
} as const;

export const UBICACIONES_RESTAURANTE = [
  'Terraza',
  'Salón Principal',
  'Área VIP',
  'Bar',
  'Exterior',
  'Privado',
] as const;

export type UbicacionRestaurante = (typeof UBICACIONES_RESTAURANTE)[number];
