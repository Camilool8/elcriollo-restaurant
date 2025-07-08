import {
  CreateClienteRequest,
  UpdateClienteRequest,
  SearchClienteParams,
  ClienteResponse,
  Cliente,
} from '@/types';
import api, { getErrorMessage } from './api';

class ClienteService {
  // Crear nuevo cliente
  async createCliente(clienteData: CreateClienteRequest): Promise<Cliente> {
    try {
      const response = await api.post<ClienteResponse>('/cliente', clienteData);
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando cliente: ${message}`);
    }
  }

  // Obtener todos los clientes
  async getClientes(params?: SearchClienteParams): Promise<any> {
    try {
      const response = await api.get<any>('/cliente', { params });
      return response.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo clientes: ${message}`);
    }
  }

  // Obtener cliente por ID
  async getClienteById(clienteId: number): Promise<Cliente> {
    try {
      const response = await api.get<{ success: boolean; data: Cliente }>(`/cliente/${clienteId}`);
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo cliente: ${message}`);
    }
  }

  // Buscar clientes
  async searchClientes(query: string): Promise<Cliente[]> {
    try {
      const response = await api.get<{ success: boolean; data: Cliente[] }>('/cliente/buscar', {
        params: { query },
      });
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error buscando clientes: ${message}`);
    }
  }

  // Actualizar cliente
  async updateCliente(clienteId: number, clienteData: UpdateClienteRequest): Promise<Cliente> {
    try {
      const response = await api.put<ClienteResponse>(`/cliente/${clienteId}`, clienteData);
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando cliente: ${message}`);
    }
  }

  // Desactivar cliente (solo admin)
  async deactivateCliente(clienteId: number): Promise<void> {
    try {
      await api.delete(`/cliente/${clienteId}`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error desactivando cliente: ${message}`);
    }
  }

  // Obtener historial de compras
  async getClienteHistorial(clienteId: number): Promise<any[]> {
    try {
      const response = await api.get<{ success: boolean; data: any[] }>(
        `/cliente/${clienteId}/historial-compras`
      );
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo historial: ${message}`);
    }
  }

  // Obtener estadísticas del cliente
  async getClienteEstadisticas(clienteId: number): Promise<any> {
    try {
      const response = await api.get<{ success: boolean; data: any }>(
        `/cliente/${clienteId}/estadisticas`
      );
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo estadísticas: ${message}`);
    }
  }

  // Obtener clientes frecuentes
  async getClientesFrecuentes(): Promise<Cliente[]> {
    try {
      const response = await api.get<{ success: boolean; data: Cliente[] }>('/cliente/frecuentes');
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo clientes frecuentes: ${message}`);
    }
  }

  // Obtener cumpleañeros del mes
  async getClientesCumpleanos(): Promise<Cliente[]> {
    try {
      const response = await api.get<{ success: boolean; data: Cliente[] }>('/cliente/cumpleanos');
      return response.data.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo cumpleañeros: ${message}`);
    }
  }
}

export const clienteService = new ClienteService();

export { ClienteService };
