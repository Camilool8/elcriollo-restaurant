import React, { createContext, useContext, useReducer, useEffect, ReactNode } from 'react';
import { toast } from 'react-toastify';
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
      try {
        dispatch({ type: 'AUTH_START' });

        const user = authService.getStoredUser();
        const token = authService.getStoredToken();
        const refreshToken = authService.getStoredRefreshToken();

        if (user && token && refreshToken) {
          const isValid = await authService.validateToken(token);
          if (isValid) {
            dispatch({
              type: 'AUTH_SUCCESS',
              payload: { user, token, refreshToken },
            });
            console.log('✅ Autenticación restaurada:', user.usuario);
          } else {
            // Si el token no es válido, intentar refrescarlo
            const newToken = await authService.refreshToken();
            if (newToken) {
              const refreshedUser = authService.getStoredUser();
              const refreshedRefreshToken = authService.getStoredRefreshToken();
              if (refreshedUser && refreshedRefreshToken) {
                dispatch({
                  type: 'AUTH_SUCCESS',
                  payload: {
                    user: refreshedUser,
                    token: newToken,
                    refreshToken: refreshedRefreshToken,
                  },
                });
                console.log('✅ Autenticación restaurada con nuevo token:', refreshedUser.usuario);
              }
            } else {
              dispatch({ type: 'AUTH_LOGOUT' });
            }
          }
        } else {
          // Si no hay datos válidos, limpiar estado
          dispatch({ type: 'AUTH_LOGOUT' });
        }
      } catch (error: any) {
        console.error('❌ Error inicializando autenticación:', error);
        dispatch({
          type: 'AUTH_FAILURE',
          payload: 'Error al inicializar la sesión',
        });
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
      toast.success(`¡Bienvenido, ${userName}! 🇩🇴`, {
        position: 'top-right',
        autoClose: 3000,
      });

      console.log('✅ Login exitoso en contexto:', response.usuario);
      return true;
    } catch (error: any) {
      const errorMessage = error.message || 'Error desconocido durante el login';

      dispatch({
        type: 'AUTH_FAILURE',
        payload: errorMessage,
      });

      // Mostrar toast de error
      toast.error(errorMessage, {
        position: 'top-right',
        autoClose: 5000,
      });

      console.error('❌ Error en login:', errorMessage);
      return false;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      dispatch({ type: 'AUTH_SET_LOADING', payload: true });

      await authService.logout();

      dispatch({ type: 'AUTH_LOGOUT' });

      toast.info('Sesión cerrada correctamente. ¡Hasta luego! 👋', {
        position: 'top-right',
        autoClose: 2000,
      });

      console.log('✅ Logout exitoso');
    } catch (error: any) {
      console.error('❌ Error en logout:', error);

      // Incluso si hay error, forzar logout local
      dispatch({ type: 'AUTH_LOGOUT' });

      toast.warning('Sesión cerrada localmente debido a un error.', {
        position: 'top-right',
        autoClose: 3000,
      });
    }
  };

  const checkAuth = async (): Promise<void> => {
    dispatch({ type: 'AUTH_SET_LOADING', payload: true });
    const token = authService.getStoredToken();
    if (token) {
      const isValid = await authService.validateToken(token);
      if (!isValid) {
        await authService.refreshToken();
      }
    } else {
      dispatch({ type: 'AUTH_LOGOUT' });
    }
    dispatch({ type: 'AUTH_SET_LOADING', payload: false });
  };

  const changePassword = async (currentPassword: string, newPassword: string): Promise<boolean> => {
    try {
      dispatch({ type: 'AUTH_SET_LOADING', payload: true });

      await authService.changePassword(currentPassword, newPassword);

      dispatch({ type: 'AUTH_SET_LOADING', payload: false });

      toast.success('Contraseña cambiada exitosamente. 🔐', {
        position: 'top-right',
        autoClose: 3000,
      });

      return true;
    } catch (error: any) {
      const errorMessage = error.message || 'Error desconocido';
      toast.error(errorMessage);
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
