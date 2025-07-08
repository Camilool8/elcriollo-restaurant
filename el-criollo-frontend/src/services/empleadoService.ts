import { Empleado } from '@/types';
import {
  CreateEmpleadoRequest,
  UpdateEmpleadoRequest,
  SearchEmpleadoParams,
} from '@/types/requests';
import { api, getErrorMessage } from './api';

const DEPARTAMENTOS_ESTATICOS = [
  'Administración',
  'Cocina',
  'Servicio',
  'Caja',
  'Recepción',
  'Limpieza',
  'Seguridad',
];

class EmpleadoService {
  async createEmpleado(empleadoData: CreateEmpleadoRequest): Promise<Empleado> {
    try {
      const response = await api.post<Empleado>('/empleado', empleadoData);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando empleado: ${message}`);
    }
  }

  async getEmpleados(params?: SearchEmpleadoParams): Promise<Empleado[]> {
    try {
      const response = await api.get<Empleado[]>('/empleado', { params });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo empleados: ${message}`);
    }
  }

  async getEmpleadoById(empleadoId: number): Promise<Empleado> {
    try {
      const response = await api.get<Empleado>(`/empleado/${empleadoId}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo empleado: ${message}`);
    }
  }

  async searchEmpleados(query: string): Promise<Empleado[]> {
    try {
      const response = await api.get<Empleado[]>('/empleado/buscar', {
        params: { termino: query },
      });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error buscando empleados: ${message}`);
    }
  }

  async updateEmpleado(empleadoId: number, empleadoData: UpdateEmpleadoRequest): Promise<Empleado> {
    try {
      const response = await api.put<Empleado>(`/empleado/${empleadoId}`, empleadoData);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando empleado: ${message}`);
    }
  }

  async deactivateEmpleado(empleadoId: number): Promise<void> {
    try {
      await api.delete(`/empleado/${empleadoId}`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error desactivando empleado: ${message}`);
    }
  }

  async getDepartamentos(): Promise<string[]> {
    return Promise.resolve(DEPARTAMENTOS_ESTATICOS);
  }

  async getEmpleadoEstadisticas(empleadoId: number): Promise<any> {
    try {
      const response = await api.get<any>(`/empleado/${empleadoId}/estadisticas`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo estadísticas: ${message}`);
    }
  }
}

export const empleadoService = new EmpleadoService();
