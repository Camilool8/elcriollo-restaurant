import { api, getErrorMessage } from './api';
import {
  CreateUsuarioRequest,
  UsuarioResponse,
  ResetPasswordRequest,
  SearchUsuarioParams,
  Rol,
} from '@/types/requests';

class AdminUserService {
  async createUser(userData: CreateUsuarioRequest): Promise<UsuarioResponse> {
    try {
      const response = await api.post<UsuarioResponse>('/auth/register', userData);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando usuario: ${message}`);
    }
  }

  async getUsers(
    params?: SearchUsuarioParams
  ): Promise<{ usuarios: UsuarioResponse[]; total: number }> {
    try {
      const response = await api.get<{ usuarios: UsuarioResponse[]; total: number }>(
        '/auth/users',
        { params }
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo usuarios: ${message}`);
    }
  }

  async getRoles(): Promise<Rol[]> {
    try {
      const response = await api.get<Rol[]>('/auth/roles');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo roles: ${message}`);
    }
  }

  async deactivateUser(userId: number): Promise<void> {
    try {
      await api.put(`/auth/${userId}/deactivate`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error desactivando usuario: ${message}`);
    }
  }

  async activateUser(userId: number): Promise<void> {
    try {
      await api.put(`/auth/${userId}/activate`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error activando usuario: ${message}`);
    }
  }

  async resetUserPassword(userId: number, newPassword: string): Promise<void> {
    try {
      const request: ResetPasswordRequest = { newPassword };
      await api.post(`/auth/${userId}/reset-password`, request);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error reseteando contraseña: ${message}`);
    }
  }
}

export const adminUserService = new AdminUserService();
