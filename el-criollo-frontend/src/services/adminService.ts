import { api, getErrorMessage } from './api';
import {
  CreateUsuarioRequest,
  UpdateUsuarioRequest,
  ChangePasswordRequest,
  SearchUsuarioParams,
  UsuarioResponse,
  User,
  PaginatedResponse,
  Rol,
} from '@/types';

// ====================================
// SERVICIO DE GESTIÓN DE USUARIOS
// ====================================

class AdminUserService {
  // Crear nuevo usuario (con empleado automático)
  async createUser(userData: CreateUsuarioRequest): Promise<User> {
    try {
      const response = await api.post<UsuarioResponse>('/auth/register', userData);
      return response.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando usuario: ${message}`);
    }
  }

  // Obtener todos los usuarios
  async getUsers(params?: SearchUsuarioParams): Promise<PaginatedResponse<User>> {
    try {
      const response = await api.get<PaginatedResponse<User>>('/auth/users', { params });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo usuarios: ${message}`);
    }
  }

  // Obtener usuario por ID
  async getUserById(userId: number): Promise<User> {
    try {
      const response = await api.get<{ success: boolean; data: User }>(`/auth/users/${userId}`);
      return response.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo usuario: ${message}`);
    }
  }

  // Actualizar usuario
  async updateUser(userId: number, userData: UpdateUsuarioRequest): Promise<User> {
    try {
      const response = await api.put<UsuarioResponse>(`/auth/users/${userId}`, userData);
      return response.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando usuario: ${message}`);
    }
  }

  // Cambiar contraseña de usuario
  async changeUserPassword(userId: number, passwordData: ChangePasswordRequest): Promise<void> {
    try {
      await api.post(`/auth/${userId}/change-password`, passwordData);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error cambiando contraseña: ${message}`);
    }
  }

  // Reset contraseña de usuario (solo admin)
  async resetUserPassword(userId: number, newPassword: string): Promise<void> {
    try {
      await api.post(`/auth/${userId}/reset-password`, { newPassword });
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error reseteando contraseña: ${message}`);
    }
  }

  // Obtener roles disponibles
  async getRoles(): Promise<Rol[]> {
    try {
      const response = await api.get<{ success: boolean; data: Rol[] }>('/auth/roles');
      return response.data;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo roles: ${message}`);
    }
  }

  // Desactivar usuario
  async deactivateUser(userId: number): Promise<void> {
    try {
      await api.patch(`/auth/users/${userId}/deactivate`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error desactivando usuario: ${message}`);
    }
  }

  // Activar usuario
  async activateUser(userId: number): Promise<void> {
    try {
      await api.patch(`/auth/users/${userId}/activate`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error activando usuario: ${message}`);
    }
  }
}

export const adminUserService = new AdminUserService();

export { AdminUserService };
