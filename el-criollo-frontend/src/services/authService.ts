import { api, isTokenExpired } from './api';
import { LoginRequest, AuthResponse, RefreshTokenRequest, User, ApiResponse } from '@/types';

// ====================================
// SERVICIO DE AUTENTICACIÓN
// ====================================

class AuthService {
  // ====================================
  // LOGIN
  // ====================================
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await api.post<any>('/auth/login', credentials);

      // El API devuelve directamente los datos sin wrapper success/message
      if (response.token && response.usuario) {
        // Crear la respuesta normalizada
        const authResponse: AuthResponse = {
          success: true,
          message: 'Login exitoso',
          user: response.usuario, // El API devuelve 'usuario' no 'user'
          token: response.token,
          refreshToken: response.refreshToken,
          expiresIn: response.expiresAt ? this.calculateExpiresIn(response.expiresAt) : 3600, // Convertir expiresAt a expiresIn
        };

        // Guardar datos en localStorage
        this.saveAuthData(authResponse);

        console.log('✅ Login exitoso:', authResponse.user.usuario);
        return authResponse;
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
      // Llamar al endpoint de logout en el backend
      await api.post('/auth/logout');
    } catch (error) {
      // Incluso si el logout del backend falla, limpiamos datos locales
      console.warn('⚠️ Error en logout del backend:', error);
    } finally {
      // Siempre limpiar datos locales
      this.clearLocalAuthData();
      console.log('✅ Logout completado');
    }
  }

  // ====================================
  // REFRESH TOKEN
  // ====================================
  async refreshToken(): Promise<string | null> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');

      if (!refreshToken) {
        throw new Error('No hay refresh token disponible');
      }

      const request: RefreshTokenRequest = { refreshToken };
      const response = await api.post<AuthResponse>('/auth/refresh', request);

      if (response.success) {
        // Actualizar tokens
        localStorage.setItem('authToken', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);

        console.log('✅ Token renovado exitosamente');
        return response.token;
      } else {
        throw new Error(response.message || 'Error al renovar token');
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
      const response = await api.post<ApiResponse>('/auth/validate-token', {
        token,
      });
      return response.success;
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
      const response = await api.post<ApiResponse>('/auth/change-password', {
        currentPassword,
        newPassword,
      });

      if (!response.success) {
        throw new Error(response.message || 'Error al cambiar contraseña');
      }

      console.log('✅ Contraseña cambiada exitosamente');
    } catch (error: any) {
      console.error('❌ Error cambiando contraseña:', error);
      throw new Error(error.response?.data?.message || 'Error al cambiar contraseña');
    }
  }

  // ====================================
  // OBTENER USUARIO ACTUAL
  // ====================================
  async getCurrentUser(): Promise<User | null> {
    try {
      const response = await api.get<User>('/auth/me');

      if (response) {
        // Actualizar usuario en localStorage
        localStorage.setItem('user', JSON.stringify(response));
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
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('user');

    if (!token || !user) {
      return false;
    }

    // Verificar si el token no está expirado
    if (isTokenExpired(token)) {
      console.log('🔄 Token expirado, intentando renovar...');
      // El interceptor se encargará de renovar automáticamente
      return true; // Temporalmente true, el interceptor manejará la renovación
    }

    return true;
  }

  // ====================================
  // OBTENER DATOS LOCALES
  // ====================================
  getStoredUser(): User | null {
    try {
      const userString = localStorage.getItem('user');
      return userString ? JSON.parse(userString) : null;
    } catch (error) {
      console.error('❌ Error parsing stored user:', error);
      return null;
    }
  }

  getStoredToken(): string | null {
    return localStorage.getItem('authToken');
  }

  getStoredRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  // ====================================
  // VERIFICAR PERMISOS POR ROL
  // ====================================
  hasRole(requiredRole: string): boolean {
    const user = this.getStoredUser();
    if (!user) return false;
    // Handle both nombreRol and rol properties for compatibility
    const userRole = user.nombreRol || user.rol;
    return userRole === requiredRole;
  }

  hasAnyRole(requiredRoles: string[]): boolean {
    const user = this.getStoredUser();
    if (!user) return false;
    // Handle both nombreRol and rol properties for compatibility
    const userRole = user.nombreRol || user.rol;
    return userRole ? requiredRoles.includes(userRole) : false;
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
  private saveAuthData(authResponse: AuthResponse): void {
    localStorage.setItem('authToken', authResponse.token);
    localStorage.setItem('refreshToken', authResponse.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResponse.user));

    // Opcional: Guardar tiempo de expiración
    const expirationTime = Date.now() + authResponse.expiresIn * 1000;
    localStorage.setItem('tokenExpiration', expirationTime.toString());
  }

  private clearLocalAuthData(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('tokenExpiration');
  }

  // ====================================
  // UTILIDAD PARA CALCULAR EXPIRES IN
  // ====================================
  private calculateExpiresIn(expiresAt: string): number {
    const expirationTime = new Date(expiresAt).getTime();
    const currentTime = Date.now();
    const expiresInMs = expirationTime - currentTime;
    return Math.max(0, Math.floor(expiresInMs / 1000)); // Convertir a segundos
  }

  // ====================================
  // INICIALIZACIÓN
  // ====================================
  async initialize(): Promise<User | null> {
    try {
      if (!this.isAuthenticated()) {
        return null;
      }

      // Verificar que el usuario actual sea válido
      const user = await this.getCurrentUser();
      return user;
    } catch (error) {
      console.error('❌ Error inicializando auth service:', error);
      this.clearLocalAuthData();
      return null;
    }
  }

  // ====================================
  // MANEJO DE ERRORES ESPECÍFICOS
  // ====================================
  handleAuthError(error: any): string {
    if (error.response?.status === 401) {
      return 'Credenciales inválidas. Verifica tu usuario y contraseña.';
    }
    if (error.response?.status === 403) {
      return 'No tienes permisos para acceder a esta sección.';
    }
    if (error.response?.status === 429) {
      return 'Demasiados intentos. Espera unos minutos antes de intentar nuevamente.';
    }
    if (error.code === 'ERR_NETWORK') {
      return 'Error de conexión. Verifica tu internet y que el servidor esté disponible.';
    }

    return error.message || 'Error de autenticación inesperado.';
  }
}

// Crear instancia singleton
const authService = new AuthService();

export default authService;

// También exportar la clase para testing
export { AuthService };
