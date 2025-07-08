import { api } from './api';

// ====================================
// SERVICIO DE DASHBOARD
// ====================================

export interface DashboardStats {
  ventasHoy: number;
  ordenesHoy: number;
  clientesHoy: number;
  mesasOcupadas: number;
  productosBajoStock: number;
  facturasPendientes: number;
  reservacionesHoy: number;
  empleadosActivos: number;
  ticketPromedio: number;
  crecimientoVentas: number;
}

export interface VentasPorHora {
  hora: string;
  ventas: number;
  ordenes: number;
}

export interface ProductoMasVendido {
  nombre: string;
  cantidad: number;
  ingresos: number;
  categoria: string;
}

export interface AlertaOperacional {
  id: string;
  tipo: 'stock_bajo' | 'factura_pendiente' | 'reservacion_confirmacion' | 'mesa_atencion';
  titulo: string;
  mensaje: string;
  prioridad: 'alta' | 'media' | 'baja';
  fechaCreacion: string;
}

class DashboardService {
  // Obtener estadísticas principales
  async getDashboardStats(): Promise<DashboardStats> {
    try {
      const response = await api.get<{ success: boolean; data: DashboardStats }>(
        '/reporte/dashboard'
      );
      return response.data;
    } catch (error: any) {
      console.warn('Error obteniendo estadísticas, usando datos mock');

      // Datos mock para desarrollo
      return {
        ventasHoy: 15240.5,
        ordenesHoy: 23,
        clientesHoy: 18,
        mesasOcupadas: 8,
        productosBajoStock: 3,
        facturasPendientes: 2,
        reservacionesHoy: 5,
        empleadosActivos: 12,
        ticketPromedio: 662.2,
        crecimientoVentas: 12.5,
      };
    }
  }

  // Obtener ventas por hora
  async getVentasPorHora(): Promise<VentasPorHora[]> {
    try {
      const response = await api.get<{ success: boolean; data: VentasPorHora[] }>(
        '/reporte/ventas/por-hora'
      );
      return response.data;
    } catch (error: any) {
      console.warn('Error obteniendo ventas por hora, usando datos mock');

      // Datos mock para desarrollo
      return [
        { hora: '08:00', ventas: 850, ordenes: 3 },
        { hora: '09:00', ventas: 1240, ordenes: 4 },
        { hora: '10:00', ventas: 920, ordenes: 3 },
        { hora: '11:00', ventas: 1580, ordenes: 5 },
        { hora: '12:00', ventas: 2350, ordenes: 7 },
        { hora: '13:00', ventas: 3100, ordenes: 9 },
        { hora: '14:00', ventas: 2890, ordenes: 8 },
        { hora: '15:00', ventas: 1650, ordenes: 5 },
        { hora: '16:00', ventas: 980, ordenes: 3 },
        { hora: '17:00', ventas: 1420, ordenes: 4 },
        { hora: '18:00', ventas: 2180, ordenes: 6 },
        { hora: '19:00', ventas: 2890, ordenes: 8 },
      ];
    }
  }

  // Obtener productos más vendidos
  async getProductosMasVendidos(): Promise<ProductoMasVendido[]> {
    try {
      const response = await api.get<{ success: boolean; data: ProductoMasVendido[] }>(
        '/reporte/productos/mas-vendidos'
      );
      return response.data;
    } catch (error: any) {
      console.warn('Error obteniendo productos más vendidos, usando datos mock');

      return [
        {
          nombre: 'La Bandera Dominicana',
          cantidad: 12,
          ingresos: 5760,
          categoria: 'Platos Principales',
        },
        { nombre: 'Pollo Guisado', cantidad: 8, ingresos: 3200, categoria: 'Platos Principales' },
        { nombre: 'Moro de Guandules', cantidad: 15, ingresos: 2250, categoria: 'Acompañantes' },
        { nombre: 'Tostones', cantidad: 18, ingresos: 1800, categoria: 'Frituras' },
        { nombre: 'Tres Golpes', cantidad: 6, ingresos: 1920, categoria: 'Desayunos' },
      ];
    }
  }

  // Obtener alertas operacionales
  async getAlertasOperacionales(): Promise<AlertaOperacional[]> {
    try {
      const response = await api.get<{ success: boolean; data: AlertaOperacional[] }>(
        '/reporte/alertas'
      );
      return response.data;
    } catch (error: any) {
      console.warn('Error obteniendo alertas, usando datos mock');

      return [
        {
          id: '1',
          tipo: 'stock_bajo',
          titulo: 'Stock Bajo',
          mensaje: 'Yuca frita - Solo quedan 3 porciones',
          prioridad: 'alta',
          fechaCreacion: new Date().toISOString(),
        },
        {
          id: '2',
          tipo: 'factura_pendiente',
          titulo: 'Factura Pendiente',
          mensaje: 'Mesa 5 - Orden completada hace 15 minutos',
          prioridad: 'media',
          fechaCreacion: new Date().toISOString(),
        },
        {
          id: '3',
          tipo: 'reservacion_confirmacion',
          titulo: 'Reservación Pendiente',
          mensaje: '2 reservaciones requieren confirmación para hoy',
          prioridad: 'media',
          fechaCreacion: new Date().toISOString(),
        },
      ];
    }
  }
}

export const dashboardService = new DashboardService();
