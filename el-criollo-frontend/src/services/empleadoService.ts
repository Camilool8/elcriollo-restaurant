import {
  CreateEmpleadoRequest,
  UpdateEmpleadoRequest,
  SearchEmpleadoParams,
  EmpleadoResponse,
  Empleado,
  PaginatedResponse,
} from '@/types';
import api, { getErrorMessage } from './api';

class EmpleadoService {
  // Crear nuevo empleado
  async createEmpleado(empleadoData: CreateEmpleadoRequest): Promise<Empleado> {
    try {
      const response = await api.post<EmpleadoResponse>('/empleado', empleadoData);
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando empleado: ${message}`);
    }
  }

  // Obtener todos los empleados
  async getEmpleados(params?: SearchEmpleadoParams): Promise<PaginatedResponse<Empleado>> {
    try {
      const response = await api.get<PaginatedResponse<Empleado>>('/empleado', { params });
      return response.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo empleados: ${message}`);
    }
  }

  // Obtener empleado por ID
  async getEmpleadoById(empleadoId: number): Promise<Empleado> {
    try {
      const response = await api.get<{ success: boolean; data: Empleado }>(
        `/empleado/${empleadoId}`
      );
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo empleado: ${message}`);
    }
  }

  // Buscar empleados
  async searchEmpleados(query: string): Promise<Empleado[]> {
    try {
      const response = await api.get<{ success: boolean; data: Empleado[] }>('/empleado/buscar', {
        params: { query },
      });
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error buscando empleados: ${message}`);
    }
  }

  // Actualizar empleado
  async updateEmpleado(empleadoId: number, empleadoData: UpdateEmpleadoRequest): Promise<Empleado> {
    try {
      const response = await api.put<EmpleadoResponse>(`/empleado/${empleadoId}`, empleadoData);
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando empleado: ${message}`);
    }
  }

  // Desactivar empleado (solo admin)
  async deactivateEmpleado(empleadoId: number): Promise<void> {
    try {
      await api.delete(`/empleado/${empleadoId}`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error desactivando empleado: ${message}`);
    }
  }

  // Obtener departamentos disponibles
  async getDepartamentos(): Promise<string[]> {
    try {
      const response = await api.get<{ success: boolean; data: string[] }>(
        '/empleado/departamentos'
      );
      return response.data.data;
    } catch (error: any) {
      // Si no hay endpoint específico, retornar lista predefinida
      return ['Administración', 'Cocina', 'Servicio', 'Caja', 'Recepción', 'Limpieza', 'Seguridad'];
    }
  }

  // Obtener estadísticas de empleado
  async getEmpleadoEstadisticas(empleadoId: number): Promise<any> {
    try {
      const response = await api.get<{ success: boolean; data: any }>(
        `/empleado/${empleadoId}/estadisticas`
      );
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo estadísticas: ${message}`);
    }
  }
}

export const empleadoService = new EmpleadoService();

export { EmpleadoService };
