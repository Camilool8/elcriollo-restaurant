import type { Empleado } from './empleado';

export interface UsuarioResponse {
  usuarioId: number;
  usuario: string;
  username: string;
  email: string;
  rol: string;
  nombreRol: string;
  empleado: Empleado;
}

export interface LoginRequest {
  username: string;
  password: string;
  recordarSesion?: boolean;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  usuario: UsuarioResponse;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export type UserRole = 'Administrador' | 'Cajero' | 'Mesero' | 'Recepcion' | 'Cocina';

export interface AuthState {
  user: UsuarioResponse | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}
