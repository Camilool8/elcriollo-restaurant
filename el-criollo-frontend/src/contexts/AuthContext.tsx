import React, { createContext, useContext, useReducer, useEffect, ReactNode } from 'react';
import {
  showErrorToast,
  showSuccessToast,
  showInfoToast,
  showWarningToast,
} from '@/utils/toastUtils';
import { authService } from '@/services/authService';
import { UsuarioResponse, LoginRequest, AuthState } from '@/types';

// ====================================
// TIPOS DEL CONTEXT
// ====================================

interface AuthContextType {
  // Estado
  state: AuthState;

  // Acciones
  login: (credentials: LoginRequest) => Promise<boolean>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
  changePassword: (currentPassword: string, newPassword: string) => Promise<boolean>;

  // Utilidades
  hasRole: (role: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;
  isAdmin: () => boolean;
  isCajero: () => boolean;
  isMesero: () => boolean;
  isRecepcion: () => boolean;
  isCocina: () => boolean;
}

// ====================================
// ACCIONES DEL REDUCER
// ====================================

type AuthAction =
  | { type: 'AUTH_START' }
  | {
      type: 'AUTH_SUCCESS';
      payload: { user: UsuarioResponse; token: string; refreshToken: string };
    }
  | { type: 'AUTH_FAILURE'; payload: string }
  | { type: 'AUTH_LOGOUT' }
  | { type: 'AUTH_CLEAR_ERROR' }
  | { type: 'AUTH_SET_LOADING'; payload: boolean };

// ====================================
// ESTADO INICIAL
// ====================================

const initialState: AuthState = {
  user: null,
  token: null,
  refreshToken: null,
  isAuthenticated: false,
  isLoading: true, // Empezamos con loading true para verificar auth inicial
  error: null,
};

// ====================================
// REDUCER
// ====================================

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'AUTH_START':
      return {
        ...state,
        isLoading: true,
        error: null,
      };

    case 'AUTH_SUCCESS':
      return {
        ...state,
        user: action.payload.user,
        token: action.payload.token,
        refreshToken: action.payload.refreshToken,
        isAuthenticated: true,
        isLoading: false,
        error: null,
      };

    case 'AUTH_FAILURE':
      return {
        ...state,
        user: null,
        token: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
        error: action.payload,
      };

    case 'AUTH_LOGOUT':
      return {
        ...state,
        user: null,
        token: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
      };

    case 'AUTH_CLEAR_ERROR':
      return {
        ...state,
        error: null,
      };

    case 'AUTH_SET_LOADING':
      return {
        ...state,
        isLoading: action.payload,
      };

    default:
      return state;
  }
};

// ====================================
// CONTEXT
// ====================================

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ====================================
// PROVIDER
// ====================================

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  // ====================================
  // VERIFICAR AUTENTICACIÓN INICIAL
  // ====================================
  useEffect(() => {
    const initializeAuth = async () => {
      dispatch({ type: 'AUTH_START' });
      try {
        const token = authService.getStoredToken();
        const refreshToken = authService.getStoredRefreshToken();
        const user = authService.getStoredUser();

        if (token && refreshToken && user) {
          if (!authService.isTokenExpired(token)) {
            // El token de acceso es válido
            dispatch({ type: 'AUTH_SUCCESS', payload: { user, token, refreshToken } });
            console.log('✅ Sesión restaurada con token válido:', user.usuario);
          } else {
            // El token de acceso expiró, intentar refrescar
            console.warn('⚠️ Token de acceso expirado, intentando refrescar...');
            const newAuthResponse = await authService.refreshToken();
            if (newAuthResponse) {
              dispatch({
                type: 'AUTH_SUCCESS',
                payload: {
                  user: newAuthResponse.usuario,
                  token: newAuthResponse.token,
                  refreshToken: newAuthResponse.refreshToken,
                },
              });
              console.log(
                '✅ Sesión restaurada con token refrescado:',
                newAuthResponse.usuario.usuario
              );
            } else {
              console.error('❌ Falló el refresco del token, cerrando sesión.');
              dispatch({ type: 'AUTH_LOGOUT' });
            }
          }
        } else {
          // No hay datos de sesión, no hacer nada y dejar que el usuario inicie sesión
          dispatch({ type: 'AUTH_LOGOUT' });
        }
      } catch (error) {
        console.error('❌ Error al inicializar la autenticación:', error);
        dispatch({ type: 'AUTH_LOGOUT' });
      }
    };

    initializeAuth();
  }, []);

  // ====================================
  // FUNCIONES DE AUTENTICACIÓN
  // ====================================

  const login = async (credentials: LoginRequest): Promise<boolean> => {
    try {
      dispatch({ type: 'AUTH_START' });

      const response = await authService.login(credentials);

      dispatch({
        type: 'AUTH_SUCCESS',
        payload: {
          user: response.usuario,
          token: response.token,
          refreshToken: response.refreshToken,
        },
      });

      // Mostrar notificación de bienvenida
      const userName = response.usuario.usuario || 'Usuario';
      showSuccessToast(`¡Bienvenido, ${userName}! 🇩🇴`);

      console.log('✅ Login exitoso en contexto:', response.usuario);
      return true;
    } catch (error: any) {
      const errorMessage = error.message || 'Error desconocido durante el login';

      dispatch({
        type: 'AUTH_FAILURE',
        payload: errorMessage,
      });

      // Mostrar toast de error
      showErrorToast(errorMessage);

      console.error('❌ Error en login:', errorMessage);
      return false;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      dispatch({ type: 'AUTH_SET_LOADING', payload: true });

      await authService.logout();

      dispatch({ type: 'AUTH_LOGOUT' });

      showInfoToast('Sesión cerrada correctamente. ¡Hasta luego! 👋');

      console.log('✅ Logout exitoso');
    } catch (error: any) {
      console.error('❌ Error en logout:', error);

      // Incluso si hay error, forzar logout local
      dispatch({ type: 'AUTH_LOGOUT' });

      showWarningToast('Sesión cerrada localmente debido a un error.');

      console.log('✅ Logout local exitoso');
    }
  };

  const checkAuth = async (): Promise<void> => {
    // Esta función ahora solo asegura que el estado de carga no interfiera.
    // La lógica principal está en `initializeAuth`.
    if (state.isLoading) {
      // Esperar a que termine la inicialización si aún está en proceso
      await new Promise((resolve) => {
        const check = () => {
          if (!state.isLoading) resolve(true);
          else setTimeout(check, 50);
        };
        check();
      });
    }
  };

  const changePassword = async (currentPassword: string, newPassword: string): Promise<boolean> => {
    try {
      dispatch({ type: 'AUTH_SET_LOADING', payload: true });

      await authService.changePassword(currentPassword, newPassword);

      dispatch({ type: 'AUTH_SET_LOADING', payload: false });

      showSuccessToast('Contraseña cambiada exitosamente. 🔐');

      return true;
    } catch (error: any) {
      const errorMessage = error.message || 'Error desconocido';
      showErrorToast(errorMessage);
      return false;
    }
  };

  // ====================================
  // UTILIDADES DE ROL
  // ====================================

  const hasRole = (role: string): boolean => {
    return state.user?.rol === role;
  };

  const hasAnyRole = (roles: string[]): boolean => {
    return !!state.user && roles.includes(state.user.rol);
  };

  const isAdmin = (): boolean => hasRole('Administrador');
  const isCajero = (): boolean => hasRole('Cajero');
  const isMesero = (): boolean => hasRole('Mesero');
  const isRecepcion = (): boolean => hasRole('Recepcion');
  const isCocina = (): boolean => hasRole('Cocina');

  // ====================================
  // VALUE DEL CONTEXT
  // ====================================

  const contextValue: AuthContextType = {
    state,
    login,
    logout,
    checkAuth,
    changePassword,
    hasRole,
    hasAnyRole,
    isAdmin,
    isCajero,
    isMesero,
    isRecepcion,
    isCocina,
  };

  return <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>;
};

// ====================================
// HOOK PERSONALIZADO
// ====================================

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);

  if (context === undefined) {
    throw new Error('useAuth debe ser usado dentro de un AuthProvider');
  }

  return context;
};

// ====================================
// HOC PARA COMPONENTES
// ====================================

interface WithAuthProps {
  requiredRole?: string;
  requiredRoles?: string[];
  fallback?: ReactNode;
}

export const withAuth = <P extends object>(
  Component: React.ComponentType<P>,
  options: WithAuthProps = {}
) => {
  const WrappedComponent: React.FC<P> = (props) => {
    const { state, hasRole, hasAnyRole } = useAuth();

    if (!state.isAuthenticated) {
      return options.fallback || <div>No autorizado</div>;
    }

    if (options.requiredRole && !hasRole(options.requiredRole)) {
      return options.fallback || <div>Sin permisos</div>;
    }

    if (options.requiredRoles && !hasAnyRole(options.requiredRoles)) {
      return options.fallback || <div>Sin permisos</div>;
    }

    return <Component {...props} />;
  };

  WrappedComponent.displayName = `withAuth(${Component.displayName || Component.name})`;

  return WrappedComponent;
};

export default AuthContext;
