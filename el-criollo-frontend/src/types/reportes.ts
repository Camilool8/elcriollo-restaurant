import type { Factura, Orden, Cliente, Producto, Categoria } from './index';

// ============================================================================
// TIPOS DE FILTROS
// ============================================================================

export interface FiltrosReporteVentas {
  fechaInicio?: Date;
  fechaFin?: Date;
  clienteId?: number;
  productoId?: number;
  categoriaId?: number;
  metodoPago?: string;
  estadoFactura?: string;
  montoMinimo?: number;
  montoMaximo?: number;
}

// ============================================================================
// TIPOS DE CÁLCULOS
// ============================================================================

export interface ResumenVentas {
  totalFacturado: number;
  totalPagado: number;
  totalPendiente: number;
  cantidadFacturas: number;
  cantidadOrdenes: number;
  promedioPorFactura: number;
  promedioPorOrden: number;
  metodoPagoMasUsado: string;
  clienteMasFrecuente: string;
  productoMasVendido: string;
  categoriaMasVendida: string;
}

export interface EstadisticasPorPeriodo {
  fecha: string;
  totalFacturado: number;
  cantidadFacturas: number;
  promedioPorFactura: number;
}

export interface EstadisticasPorCliente {
  clienteId: number;
  clienteNombre: string;
  totalComprado: number;
  cantidadFacturas: number;
  promedioPorCompra: number;
  ultimaCompra: Date;
}

export interface EstadisticasPorProducto {
  productoId: number;
  productoNombre: string;
  categoriaNombre: string;
  cantidadVendida: number;
  totalVendido: number;
  precioPromedio: number;
}

export interface EstadisticasPorCategoria {
  categoriaId: number;
  categoriaNombre: string;
  cantidadProductos: number;
  totalVendido: number;
  cantidadVentas: number;
}

// ============================================================================
// TIPOS DE VISTAS
// ============================================================================

export type VistaReporte =
  | 'FACTURAS'
  | 'RESUMEN'
  | 'CLIENTES'
  | 'PRODUCTOS'
  | 'CATEGORIAS'
  | 'PERIODO';

export interface ReporteVentasState {
  facturas: Factura[];
  ordenes: Orden[];
  clientes: Cliente[];
  productos: Producto[];
  categorias: Categoria[];
  filtros: FiltrosReporteVentas;
  resumen: ResumenVentas | null;
  estadisticasPorPeriodo: EstadisticasPorPeriodo[];
  estadisticasPorCliente: EstadisticasPorCliente[];
  estadisticasPorProducto: EstadisticasPorProducto[];
  estadisticasPorCategoria: EstadisticasPorCategoria[];
  vistaActual: VistaReporte;
  loading: boolean;
  error: string | null;
}

// ============================================================================
// TIPOS DE COMPONENTES
// ============================================================================

export interface FacturaCardProps {
  factura: Factura;
  orden?: Orden;
  cliente?: Cliente;
  onVerDetalle?: (factura: Factura) => void;
  className?: string;
}

export interface FiltrosReporteProps {
  filtros: FiltrosReporteVentas;
  onFiltrosChange: (filtros: FiltrosReporteVentas) => void;
  onLimpiarFiltros: () => void;
  clientes: Cliente[];
  productos: Producto[];
  categorias: Categoria[];
  loading?: boolean;
}

export interface ResumenCardProps {
  titulo: string;
  valor: string | number;
  subtitulo?: string;
  icono?: React.ReactNode;
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info';
  className?: string;
}
