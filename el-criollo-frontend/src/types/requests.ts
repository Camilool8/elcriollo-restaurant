import { Cliente, Empleado, MesaEstado } from './index';
// ====================================
// REQUESTS DE USUARIO Y EMPLEADO
// ====================================

export interface CreateUsuarioRequest {
  // CAMPOS REQUERIDOS
  username: string;
  password: string;
  confirmarPassword: string;
  email: string;
  rolId: number;
  cedula: string;
  nombre: string;
  apellido: string;

  // CAMPOS OPCIONALES
  sexo?: string;
  direccion?: string;
  telefono?: string;
  salario?: number;
  departamento?: string;
  fechaIngreso?: string;
  requiereCambioContrasena?: boolean;
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
  nombreCompleto: string;
  cedula?: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  fechaNacimiento?: string;
}

export interface UpdateClienteRequest {
  nombreCompleto: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  fechaNacimiento?: string;
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
  nombreCompleto?: string;
  telefono?: string;
  email?: string;
  direccion?: string;
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
  telefono: /^\d{3}-\d{3}-\d{4}$/,
  email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  password: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._#-])[A-Za-z\d@$!%*?&._#-]{8,}$/,
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

// ====================================
// EXPORTAR TODO
// ====================================

export * from './index'; // Re-exportar tipos principales
