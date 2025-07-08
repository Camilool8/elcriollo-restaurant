// ====================================
// TIPOS DE AUTENTICACIÓN
// ====================================

export interface User {
  usuarioID: number;
  usuario: string;
  email: string;
  rolID: number;
  nombreRol: string;
  empleadoID?: number;
  empleado?: Empleado;
  estado: boolean;
  fechaCreacion: string;
  ultimoAcceso?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
  recordarSesion?: boolean;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  user: User;
  token: string;
  refreshToken: string;
  expiresIn: number;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export type UserRole = 'Administrador' | 'Cajero' | 'Mesero' | 'Recepcion' | 'Cocina';

// ====================================
// TIPOS DE EMPLEADOS
// ====================================

export interface Empleado {
  empleadoID: number;
  cedula: string;
  nombre: string;
  apellido: string;
  sexo?: string;
  direccion?: string;
  telefono?: string;
  email?: string;
  fechaNacimiento?: string;
  salario?: number;
  departamento?: string;
  fechaIngreso: string;
  estado: string;
  usuarioID?: number;
}

// ====================================
// TIPOS DE CLIENTES
// ====================================

export interface Cliente {
  clienteID: number;
  cedula?: string;
  nombre: string;
  apellido: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  fechaNacimiento?: string;
  preferenciasComida?: string;
  estado: string;
  fechaRegistro: string;
}

// ====================================
// TIPOS DE MESAS
// ====================================

export interface Mesa {
  mesaID: number;
  numeroMesa: number;
  capacidad: number;
  estado: MesaEstado;
  ubicacion?: string;
  observaciones?: string;
}

export type MesaEstado = 'Libre' | 'Ocupada' | 'Reservada' | 'Mantenimiento';

// ====================================
// TIPOS DE PRODUCTOS Y MENÚ
// ====================================

export interface Categoria {
  categoriaID: number;
  nombre: string;
  descripcion?: string;
  estado: boolean;
}

export interface Producto {
  productoID: number;
  nombre: string;
  descripcion?: string;
  categoriaID: number;
  categoria?: Categoria;
  precio: number;
  costoPreparacion?: number;
  tiempoPreparacion?: number;
  imagen?: string;
  estado: boolean;
}

export interface Combo {
  comboID: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  descuento: number;
  estado: boolean;
  productos?: ComboProducto[];
}

export interface ComboProducto {
  comboProductoID: number;
  comboID: number;
  productoID: number;
  cantidad: number;
  producto?: Producto;
}

// ====================================
// TIPOS DE ÓRDENES
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
  estado: OrdenEstado;
  tipoOrden: TipoOrden;
  observaciones?: string;
  subtotalCalculado: number;
  impuesto: number;
  totalCalculado: number;
  items?: DetalleOrden[];
}

export type OrdenEstado = 'Pendiente' | 'En Preparacion' | 'Lista' | 'Entregada' | 'Cancelada';
export type TipoOrden = 'Mesa' | 'Llevar' | 'Delivery';

export interface DetalleOrden {
  detalleOrdenID: number;
  ordenID: number;
  productoID?: number;
  producto?: Producto;
  comboID?: number;
  combo?: Combo;
  cantidad: number;
  precioUnitario: number;
  descuento: number;
  subtotal: number;
  observaciones?: string;
}

export interface CrearOrdenRequest {
  mesaID?: number;
  clienteID?: number;
  tipoOrden: TipoOrden;
  observaciones?: string;
  items: ItemOrdenRequest[];
}

export interface ItemOrdenRequest {
  productoID?: number;
  comboID?: number;
  cantidad: number;
  notasEspeciales?: string;
}

// ====================================
// TIPOS DE FACTURAS
// ====================================

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

export type MetodoPago = 'Efectivo' | 'Tarjeta' | 'Transferencia';
export type FacturaEstado = 'Pendiente' | 'Pagada' | 'Anulada';

export interface CrearFacturaRequest {
  ordenID: number;
  metodoPago: MetodoPago;
  descuento?: number;
  propina?: number;
  observacionesPago?: string;
}

// ====================================
// TIPOS DE RESERVACIONES
// ====================================

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

// ====================================
// TIPOS DE INVENTARIO
// ====================================

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

// ====================================
// TIPOS DE REPORTES
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

export interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

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
// TIPOS DE NOTIFICACIONES
// ====================================

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

// ====================================
// UTILIDADES DE TIPOS
// ====================================

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
// REQUESTS DE USUARIO Y EMPLEADO
// ====================================

export interface CreateUsuarioRequest {
  // Datos del usuario
  usuario: string;
  email: string;
  contrasena: string;
  rolID: number;

  // Datos del empleado (se crea automáticamente)
  cedula: string;
  nombre: string;
  apellido: string;
  sexo?: string;
  direccion?: string;
  telefono?: string;
  fechaNacimiento?: string;
  salario?: number;
  departamento?: string;
}

export interface UpdateUsuarioRequest {
  usuario?: string;
  email?: string;
  rolID?: number;
  estado?: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ResetPasswordRequest {
  newPassword: string;
}

// ====================================
// REQUESTS DE CLIENTE
// ====================================

export interface CreateClienteRequest {
  cedula?: string;
  nombre: string;
  apellido: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  fechaNacimiento?: string;
  preferenciasComida?: string;
}

export interface UpdateClienteRequest {
  cedula?: string;
  nombre?: string;
  apellido?: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  fechaNacimiento?: string;
  preferenciasComida?: string;
  estado?: string;
}

// ====================================
// REQUESTS DE EMPLEADO
// ====================================

export interface CreateEmpleadoRequest {
  cedula: string;
  nombre: string;
  apellido: string;
  sexo?: string;
  direccion?: string;
  telefono?: string;
  email?: string;
  fechaNacimiento?: string;
  salario?: number;
  departamento?: string;
}

export interface UpdateEmpleadoRequest {
  cedula?: string;
  nombre?: string;
  apellido?: string;
  sexo?: string;
  direccion?: string;
  telefono?: string;
  email?: string;
  fechaNacimiento?: string;
  salario?: number;
  departamento?: string;
  estado?: string;
}

// ====================================
// REQUESTS DE PRODUCTO
// ====================================

export interface CreateProductoRequest {
  nombre: string;
  descripcion?: string;
  categoriaId: number;
  precio: number;
  tiempoPreparacion?: number;
  imagen?: string;
  costoPreparacion?: number;
}

export interface UpdateProductoRequest {
  nombre?: string;
  descripcion?: string;
  categoriaId?: number;
  precio?: number;
  tiempoPreparacion?: number;
  imagen?: string;
  costoPreparacion?: number;
  estado?: boolean;
}

// ====================================
// REQUESTS DE MESA
// ====================================

export interface CreateMesaRequest {
  numeroMesa: number;
  capacidad: number;
  ubicacion?: string;
  observaciones?: string;
}

export interface UpdateMesaRequest {
  numeroMesa?: number;
  capacidad?: number;
  ubicacion?: string;
  observaciones?: string;
}

export interface CambiarEstadoMesaRequest {
  estado: MesaEstado;
  observaciones?: string;
}

// ====================================
// RESPONSES ESPECÍFICOS
// ====================================

export interface UsuarioResponse {
  success: boolean;
  message: string;
  data: User;
}

export interface ClienteResponse {
  success: boolean;
  message: string;
  data: Cliente;
}

export interface EmpleadoResponse {
  success: boolean;
  message: string;
  data: Empleado;
}

export interface ProductoResponse {
  success: boolean;
  message: string;
  data: Producto;
}

// ====================================
// TIPOS DE ROLES
// ====================================

export interface Rol {
  rolID: number;
  nombreRol: string;
  descripcion?: string;
  estado: boolean;
}

// ====================================
// TIPOS DE BÚSQUEDA Y FILTROS
// ====================================

export interface SearchParams {
  query?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  filters?: Record<string, any>;
}

export interface SearchClienteParams extends SearchParams {
  estado?: string;
  ciudad?: string;
  fechaRegistroDesde?: string;
  fechaRegistroHasta?: string;
}

export interface SearchEmpleadoParams extends SearchParams {
  estado?: string;
  departamento?: string;
  fechaIngresoDesde?: string;
  fechaIngresoHasta?: string;
}

export interface SearchUsuarioParams extends SearchParams {
  rolID?: number;
  estado?: boolean;
  fechaCreacionDesde?: string;
  fechaCreacionHasta?: string;
}

// ====================================
// TIPOS DE VALIDACIÓN DOMINICANA
// ====================================

export interface DominicanValidations {
  cedula: string;
  telefono: string;
  email: string;
}

// Patrones de validación dominicanos
export const DOMINICAN_PATTERNS = {
  cedula: /^\d{3}-\d{7}-\d{1}$/,
  telefono: /^(\+1\s?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$/,
  email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  numeroMesa: /^\d{1,3}$/,
  precio: /^\d+(\.\d{1,2})?$/,
};

// ====================================
// TIPOS DE ESTADÍSTICAS
// ====================================

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
// TIPOS DE NOTIFICACIONES
// ====================================

export interface NotificationData {
  tipo: 'usuario_creado' | 'cliente_registrado' | 'empleado_agregado' | 'password_changed';
  titulo: string;
  mensaje: string;
  datos?: Record<string, any>;
  fechaCreacion: string;
  leido: boolean;
}

// ====================================
// TIPOS DE CONFIGURACIÓN
// ====================================

export interface SystemConfig {
  // Configuración de usuarios
  passwordMinLength: number;
  passwordRequireUppercase: boolean;
  passwordRequireNumbers: boolean;
  passwordRequireSpecialChars: boolean;
  sessionTimeoutMinutes: number;

  // Configuración de clientes
  clienteRequiereCedula: boolean;
  clienteRequiereTelefono: boolean;
  clienteRequiereEmail: boolean;

  // Configuración de empleados
  empleadoRequiereCedula: boolean;
  empleadoRequiereSalario: boolean;
  empleadoRequiereDepartamento: boolean;
}
