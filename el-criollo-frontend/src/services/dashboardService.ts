import { api } from './api';

export interface DashboardResponse {
  ventasHoy: number;
  ventasAyer: number;
  ventasMes: number;
  ordenesActivas: number;
  ordenesHoy: number;
  clientesHoy: number;
  mesasOcupadas: number;
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

class DashboardService {
  async getDashboardStats(): Promise<DashboardResponse> {
    try {
      const response = await api.get<DashboardResponse>('/reporte/dashboard');
      return response;
    } catch (error: any) {
      console.warn('Error obteniendo estadísticas, usando datos mock');
      return {
        ventasHoy: 15240.5,
        ventasAyer: 12300.0,
        ventasMes: 250000.0,
        ordenesActivas: 5,
        ordenesHoy: 23,
        clientesHoy: 18,
        mesasOcupadas: 8,
      };
    }
  }

  async getVentasPorHora(): Promise<VentasPorHora[]> {
    try {
      const response = await api.get<VentasPorHora[]>('/reporte/ventas/por-hora');
      return response;
    } catch (error: any) {
      console.warn('Error obteniendo ventas por hora, usando datos mock');
      return [
        { hora: '12:00', ventas: 2350, ordenes: 7 },
        { hora: '13:00', ventas: 3100, ordenes: 9 },
        { hora: '14:00', ventas: 2890, ordenes: 8 },
        { hora: '19:00', ventas: 2890, ordenes: 8 },
      ];
    }
  }

  async getProductosMasVendidos(): Promise<ProductoMasVendido[]> {
    try {
      const response = await api.get<ProductoMasVendido[]>('/reporte/productos/mas-vendidos');
      return response;
    } catch (error: any) {
      console.warn('Error obteniendo productos más vendidos, usando datos mock');
      return [
        {
          nombre: 'La Bandera Dominicana',
          cantidad: 12,
          ingresos: 5760,
          categoria: 'Platos Principales',
        },
        { nombre: 'Moro de Guandules', cantidad: 15, ingresos: 2250, categoria: 'Acompañantes' },
      ];
    }
  }
}

export const dashboardService = new DashboardService();
