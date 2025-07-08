import { api } from './api';
import { LoginRequest, AuthResponse, RefreshTokenRequest, UsuarioResponse } from '@/types';

// ====================================
// SERVICIO DE AUTENTICACIÓN
// ====================================

class AuthService {
  // ====================================
  // LOGIN
  // ====================================
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await api.post<AuthResponse>('/auth/login', credentials);

      if (response.token && response.usuario) {
        this.saveAuthData(response, credentials.recordarSesion);
        console.log('✅ Login exitoso:', response.usuario.usuario);
        return response;
      } else {
        throw new Error('Respuesta del servidor inválida');
      }
    } catch (error: any) {
      console.error('❌ Error en login:', error);
      throw new Error(error.response?.data?.message || 'Credenciales inválidas');
    }
  }

  // ====================================
  // LOGOUT
  // ====================================
  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
    } catch (error) {
      console.warn('⚠️ Error en logout del backend:', error);
    } finally {
      this.clearLocalAuthData();
      console.log('✅ Logout completado');
    }
  }

  // ====================================
  // REFRESH TOKEN
  // ====================================
  async refreshToken(): Promise<string | null> {
    try {
      const refreshToken = this.getStoredRefreshToken();

      if (!refreshToken) {
        throw new Error('No hay refresh token disponible');
      }

      const request: RefreshTokenRequest = { refreshToken };
      const response = await api.post<AuthResponse>('/auth/refresh', request);

      if (response.token) {
        this.saveAuthData(response, true); // Assume refresh token should be remembered
        console.log('✅ Token renovado exitosamente');
        return response.token;
      } else {
        throw new Error('Error al renovar token');
      }
    } catch (error: any) {
      console.error('❌ Error renovando token:', error);
      this.clearLocalAuthData();
      return null;
    }
  }

  // ====================================
  // VALIDAR TOKEN
  // ====================================
  async validateToken(token: string): Promise<boolean> {
    try {
      await api.post<void>('/auth/validate-token', { token });
      return true;
    } catch (error) {
      console.error('❌ Error validando token:', error);
      return false;
    }
  }

  // ====================================
  // CAMBIAR CONTRASEÑA
  // ====================================
  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    try {
      await api.post<void>('/auth/change-password', {
        currentPassword,
        newPassword,
      });
      console.log('✅ Contraseña cambiada exitosamente');
    } catch (error: any) {
      console.error('❌ Error cambiando contraseña:', error);
      throw new Error(error.response?.data?.message || 'Error al cambiar contraseña');
    }
  }

  // ====================================
  // OBTENER USUARIO ACTUAL
  // ====================================
  async getCurrentUser(): Promise<UsuarioResponse | null> {
    try {
      const response = await api.get<UsuarioResponse>('/auth/me');
      if (response) {
        // We only need to return the user, the interceptor will handle token storage
        return response;
      }
      return null;
    } catch (error) {
      console.error('❌ Error obteniendo usuario actual:', error);
      return null;
    }
  }

  // ====================================
  // VERIFICAR AUTENTICACIÓN
  // ====================================
  isAuthenticated(): boolean {
    const token = this.getStoredToken();
    if (!token) {
      return false;
    }
    // The interceptor will handle token expiration and refreshing.
    // Here, we just check for presence.
    return true;
  }

  // ====================================
  // OBTENER DATOS LOCALES
  // ====================================
  getStoredUser(): UsuarioResponse | null {
    try {
      const userString = localStorage.getItem('user') || sessionStorage.getItem('user');
      return userString ? JSON.parse(userString) : null;
    } catch (error) {
      console.error('❌ Error parsing stored user:', error);
      return null;
    }
  }

  getStoredToken(): string | null {
    return localStorage.getItem('token') || sessionStorage.getItem('token');
  }

  getStoredRefreshToken(): string | null {
    return localStorage.getItem('refreshToken') || sessionStorage.getItem('refreshToken');
  }

  // ====================================
  // VERIFICAR PERMISOS POR ROL
  // ====================================
  hasRole(requiredRole: string): boolean {
    const user = this.getStoredUser();
    return !!user && user.rol === requiredRole;
  }

  hasAnyRole(requiredRoles: string[]): boolean {
    const user = this.getStoredUser();
    return !!user && requiredRoles.includes(user.rol);
  }

  isAdmin(): boolean {
    return this.hasRole('Administrador');
  }

  isCajero(): boolean {
    return this.hasRole('Cajero');
  }

  isMesero(): boolean {
    return this.hasRole('Mesero');
  }

  isRecepcion(): boolean {
    return this.hasRole('Recepcion');
  }

  isCocina(): boolean {
    return this.hasRole('Cocina');
  }

  // ====================================
  // UTILIDADES PRIVADAS
  // ====================================
  private saveAuthData(authResponse: AuthResponse, rememberSession?: boolean): void {
    const storage = rememberSession ? localStorage : sessionStorage;
    storage.setItem('token', authResponse.token);
    storage.setItem('refreshToken', authResponse.refreshToken);
    storage.setItem('user', JSON.stringify(authResponse.usuario));
    storage.setItem('tokenExpiresAt', authResponse.expiresAt);
  }

  private clearLocalAuthData(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('tokenExpiresAt');
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('refreshToken');
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('tokenExpiresAt');
  }
}

export const authService = new AuthService();
