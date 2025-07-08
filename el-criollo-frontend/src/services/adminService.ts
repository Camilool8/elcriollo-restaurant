import { api, getErrorMessage } from './api';
import { CreateUsuarioRequest, UsuarioResponse, ResetPasswordRequest } from '@/types/requests';

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
