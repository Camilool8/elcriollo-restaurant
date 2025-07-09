import type { Cliente, Empleado, Mesa, Orden, Producto, UsuarioResponse } from './index'; // index will re-export everything

// ====================================
// TIPOS DE REPORTES Y ESTADÍSTICAS
// ====================================

export interface VentasResponse {
  fecha: string;
  ventasTotales: number;
  ordenes: number;
  clientesUnicos: number;
  ticketPromedio: number;
}

export interface ProductoVentasResponse {
  productoID: number;
  nombreProducto: string;
  categoria: string;
  cantidadVendida: number;
  ventasTotales: number;
  margenGanancia: number;
}

export interface DashboardResponse {
  ventasHoy: number;
  ordenesHoy: number;
  clientesHoy: number;
  mesasOcupadas: number;
  productosBajoStock: number;
  facturasPendientes: number;
  reservacionesHoy: number;
}

export interface EstadisticasClientes {
  totalClientes: number;
  clientesActivos: number;
  clientesNuevosEsteMes: number;
  clientesFrecuentes: number;
  promedioComprasPorCliente: number;
  clienteConMasCompras: Cliente | null;
}

export interface EstadisticasEmpleados {
  totalEmpleados: number;
  empleadosActivos: number;
  empleadosPorDepartamento: Record<string, number>;
  promedioSalario: number;
  empleadoDelMes?: Empleado;
}

export interface EstadisticasUsuarios {
  totalUsuarios: number;
  usuariosActivos: number;
  usuariosPorRol: Record<string, number>;
  ultimoLogin: string;
  sesionesActivas: number;
}

// ====================================
// TIPOS DE API Y RESPUESTAS
// ====================================

export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiError {
  message: string;
  status: number;
  details?: string;
}

// ====================================
// TIPOS DE FORMULARIOS
// ====================================

export interface FormState {
  isLoading: boolean;
  errors: Record<string, string>;
  touched: Record<string, boolean>;
}

// ====================================
// TIPOS DE ESTADO GLOBAL
// ====================================

export interface MesasState {
  mesas: Mesa[];
  mesaSeleccionada: Mesa | null;
  isLoading: boolean;
  lastUpdated: string | null;
}

export interface OrdenesState {
  ordenesActivas: Orden[];
  ordenSeleccionada: Orden | null;
  isLoading: boolean;
  error: string | null;
}

// ====================================
// TIPOS DE CONFIGURACIÓN
// ====================================

export interface AppConfig {
  apiBaseUrl: string;
  tokenExpiryMinutes: number;
  refreshTokenExpiryDays: number;
  maxRetries: number;
  requestTimeout: number;
}

// ====================================
// TIPOS DE UI
// ====================================

export interface ActionMenuItem {
  label: string;
  icon?: React.ReactNode;
  onClick: () => void;
  variant?: 'default' | 'danger' | 'warning';
  disabled?: boolean;
}

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  actions?: NotificationAction[];
}

export interface NotificationAction {
  label: string;
  action: () => void;
  variant?: 'primary' | 'secondary';
}

export interface NotificationData {
  tipo: 'usuario_creado' | 'cliente_registrado' | 'empleado_agregado' | 'password_changed';
  titulo: string;
  mensaje: string;
  datos?: Record<string, any>;
  fechaCreacion: string;
  leido: boolean;
}

export type SelectOption = {
  value: string | number;
  label: string;
  disabled?: boolean;
};

export type SortDirection = 'asc' | 'desc';

export type TableColumn<T> = {
  key: keyof T;
  label: string;
  sortable?: boolean;
  render?: (value: any, item: T) => React.ReactNode;
};

export type FilterOption = {
  key: string;
  value: any;
  operator: 'eq' | 'contains' | 'gte' | 'lte' | 'in';
};

// ====================================
// TIPOS DE RESPUESTAS GENÉRICAS
// ====================================

export interface UsuarioResponseWrapper {
  success: boolean;
  message: string;
  data: UsuarioResponse;
}

export interface ClienteResponseWrapper {
  success: boolean;
  message: string;
  data: Cliente;
}

export interface EmpleadoResponseWrapper {
  success: boolean;
  message: string;
  data: Empleado;
}

export interface ProductoResponseWrapper {
  success: boolean;
  message: string;
  data: Producto;
}

// ====================================
// TIPOS DE VALIDACIÓN DOMINICANA
// ====================================

export interface DominicanValidations {
  cedula: string;
  telefono: string;
  email: string;
}

export const DOMINICAN_PATTERNS = {
  cedula: /^\d{3}-\d{7}-\d{1}$/,
  telefono: /^(\+1\s?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$/,
  email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  numeroMesa: /^\d{1,3}$/,
  precio: /^\d+(\.\d{1,2})?$/,
};
