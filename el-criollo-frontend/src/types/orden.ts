import type { Mesa } from './mesa';
import type { Cliente } from './cliente';
import type { Empleado } from './empleado';
import type { Producto } from './producto';

// Estados de orden según el backend
export type EstadoOrden = 'Pendiente' | 'En Preparacion' | 'Lista' | 'Entregada' | 'Cancelada';

// Tipos de orden según el backend
export type TipoOrden = 'Mesa' | 'Llevar' | 'Delivery';

// ====================================
// ORDEN PRINCIPAL
// ====================================

export interface Orden {
  ordenID: number;
  numeroOrden: string;
  mesaID?: number;
  mesa?: Mesa;
  clienteID?: number;
  cliente?: Cliente;
  empleadoID: number;
  empleado?: Empleado;
  fechaCreacion: string;
  fechaActualizacion?: string;
  estado: EstadoOrden;
  tipoOrden: TipoOrden;
  observaciones?: string;
  detalles?: DetalleOrden[];
  totalItems: number;
  subtotal: string; // Viene formateado del backend
  total: string; // Viene formateado del backend
  // Campos adicionales del backend
  subtotalCalculado: number; // Campo numérico para cálculos
  impuesto: number; // Campo numérico para cálculos
  totalCalculado: number; // Campo numérico para cálculos
  tiempoTranscurrido?: string;
  tiempoPreparacionEstimado?: string;
  estaRetrasada?: boolean;
  estaFacturada?: boolean;
  categoriasProductos?: string[];
}

// ====================================
// DETALLE DE ORDEN
// ====================================

export interface DetalleOrden {
  detalleOrdenID: number;
  ordenID: number;
  producto?: Producto; // Objeto producto completo del backend
  tipoItem: string; // "Producto" o "Combo"
  nombreItem: string;
  descripcionItem?: string;
  categoriaItem?: string;
  cantidad: number;
  precioUnitario: string; // Formateado del backend
  descuento: string; // Formateado del backend
  subtotal: string; // Formateado del backend
  observaciones?: string;
  estaDisponible: boolean;
  tiempoPreparacion?: string;
  nombreCompleto: string; // Nombre con cantidad incluida
}

// ====================================
// REQUESTS PARA CREAR ÓRDENES
// ====================================

export interface CrearOrdenRequest {
  mesaID?: number;
  clienteID?: number;
  tipoOrden: TipoOrden;
  observaciones?: string;
  items: ItemOrdenRequest[];
  clienteOcasional?: ClienteOcasionalRequest;
}

export interface ItemOrdenRequest {
  productoID: number;
  cantidad: number;
  notasEspeciales?: string;
}

export interface ClienteOcasionalRequest {
  nombreCompleto: string;
  cedula?: string;
  telefono?: string;
  email?: string;
}

// ====================================
// REQUESTS PARA ACTUALIZAR ÓRDENES
// ====================================

export interface ActualizarEstadoOrdenRequest {
  nuevoEstado: EstadoOrden;
  observaciones?: string;
}

export interface AgregarItemsOrdenRequest {
  items: ItemOrdenRequest[];
}

export interface ActualizarOrdenRequest {
  ordenID: number;
  observaciones?: string;
  items: ItemOrdenRequest[];
}

// ====================================
// FILTROS Y BÚSQUEDAS
// ====================================

export interface FiltrosOrden {
  estado?: EstadoOrden;
  tipoOrden?: TipoOrden;
  mesaID?: number;
  clienteID?: number;
  fechaDesde?: string;
  fechaHasta?: string;
  soloActivas?: boolean;
}

// ====================================
// ESTADÍSTICAS DE ÓRDENES
// ====================================

export interface EstadisticasOrdenes {
  totalOrdenes: number;
  ordenesPendientes: number;
  ordenesEnPreparacion: number;
  ordenesListas: number;
  ordenesEntregadas: number;
  ordenesCanceladas: number;
  promedioTiempoPreparacion: string;
  ventasDelDia: number;
  ordenMasReciente?: Orden;
}

// ====================================
// CARRITO DE COMPRAS (PARA UI)
// ====================================

export interface ItemCarrito {
  producto: Producto;
  cantidad: number;
  notasEspeciales?: string;
  subtotal: number;
}

export interface Carrito {
  items: ItemCarrito[];
  observacionesGenerales?: string;
  tipoOrden: TipoOrden;
  mesaSeleccionada?: Mesa;
  clienteSeleccionado?: Cliente;
  subtotal: number;
  impuesto: number;
  total: number;
}

// ====================================
// CONSTANTES Y CONFIGURACIÓN
// ====================================

// Configuración de colores por estado usando las clases existentes
export const COLORES_ESTADO_ORDEN = {
  Pendiente: {
    bg: 'bg-amber-100',
    border: 'border-amber-500',
    text: 'text-amber-800',
    icon: '⏳',
  },
  'En Preparacion': {
    bg: 'bg-blue-100',
    border: 'border-blue-500',
    text: 'text-blue-800',
    icon: '👨‍🍳',
  },
  Lista: {
    bg: 'bg-green-100',
    border: 'border-green-500',
    text: 'text-green-800',
    icon: '✅',
  },
  Entregada: {
    bg: 'bg-emerald-100',
    border: 'border-emerald-500',
    text: 'text-emerald-800',
    icon: '🎉',
  },
  Cancelada: {
    bg: 'bg-red-100',
    border: 'border-red-500',
    text: 'text-red-800',
    icon: '❌',
  },
} as const;

export const ICONOS_TIPO_ORDEN = {
  Mesa: '🪑',
  Llevar: '🥡',
  Delivery: '🛵',
} as const;

// Tiempo de auto-refresh para órdenes en tiempo real
export const ORDEN_REFRESH_INTERVAL = 15000; // 15 segundos

// Estados que permiten modificaciones
export const ESTADOS_MODIFICABLES: EstadoOrden[] = ['Pendiente', 'En Preparacion'];

// Estados considerados como "activos"
export const ESTADOS_ACTIVOS: EstadoOrden[] = ['Pendiente', 'En Preparacion', 'Lista'];

// Transiciones de estado permitidas
export const TRANSICIONES_ESTADO: Record<EstadoOrden, EstadoOrden[]> = {
  Pendiente: ['En Preparacion', 'Cancelada'],
  'En Preparacion': ['Lista', 'Cancelada'],
  Lista: ['Entregada'],
  Entregada: [], // Estado final
  Cancelada: [], // Estado final
};
