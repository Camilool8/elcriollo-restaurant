import React, { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { UserRole } from '@/types';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

// ====================================
// TIPOS
// ====================================

interface ProtectedRouteProps {
  children: ReactNode;
  requiredRole?: UserRole;
  requiredRoles?: UserRole[];
  fallbackPath?: string;
  showUnauthorized?: boolean;
}

// ====================================
// COMPONENTE PRINCIPAL
// ====================================

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredRole,
  requiredRoles,
  fallbackPath = '/login',
  showUnauthorized = false,
}) => {
  const { state, hasRole, hasAnyRole } = useAuth();
  const location = useLocation();

  // ====================================
  // LOADING STATE
  // ====================================
  if (state.isLoading) {
    return (
      <div className="min-h-screen bg-warm-beige flex items-center justify-center">
        <div className="text-center">
          <LoadingSpinner size="lg" />
          <p className="mt-4 text-dominican-blue font-medium">Verificando permisos...</p>
        </div>
      </div>
    );
  }

  // ====================================
  // NO AUTENTICADO
  // ====================================
  if (!state.isAuthenticated) {
    return <Navigate to={fallbackPath} state={{ from: location.pathname }} replace />;
  }

  // ====================================
  // VERIFICAR ROLES ESPECÍFICOS
  // ====================================

  // Verificar rol único
  if (requiredRole && !hasRole(requiredRole)) {
    return showUnauthorized ? (
      <UnauthorizedAccess requiredRole={requiredRole} />
    ) : (
      <Navigate to="/dashboard" replace />
    );
  }

  // Verificar múltiples roles
  if (requiredRoles && !hasAnyRole(requiredRoles)) {
    return showUnauthorized ? (
      <UnauthorizedAccess requiredRoles={requiredRoles} />
    ) : (
      <Navigate to="/dashboard" replace />
    );
  }

  // ====================================
  // ACCESO AUTORIZADO
  // ====================================
  return <>{children}</>;
};

// ====================================
// COMPONENTE DE NO AUTORIZADO
// ====================================

interface UnauthorizedAccessProps {
  requiredRole?: UserRole;
  requiredRoles?: UserRole[];
}

const UnauthorizedAccess: React.FC<UnauthorizedAccessProps> = ({ requiredRole, requiredRoles }) => {
  const { state, logout } = useAuth();

  const handleGoBack = () => {
    window.history.back();
  };

  const handleLogout = async () => {
    await logout();
  };

  const getRoleText = (): string => {
    if (requiredRole) {
      return requiredRole;
    }
    if (requiredRoles) {
      return requiredRoles.join(', ');
    }
    return 'rol específico';
  };

  return (
    <div className="min-h-screen bg-warm-beige flex items-center justify-center p-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-8 text-center">
        {/* Icono de acceso denegado */}
        <div className="w-16 h-16 mx-auto mb-6 bg-dominican-red bg-opacity-10 rounded-full flex items-center justify-center">
          <svg
            className="w-8 h-8 text-dominican-red"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L5.636 5.636"
            />
          </svg>
        </div>

        {/* Título y mensaje */}
        <h1 className="text-2xl font-heading font-semibold text-dominican-blue mb-4">
          Acceso Denegado
        </h1>

        <p className="text-stone-gray mb-2">No tienes permisos para acceder a esta sección.</p>

        <p className="text-sm text-stone-gray mb-6">
          Se requiere el rol:{' '}
          <span className="font-medium text-dominican-red">{getRoleText()}</span>
        </p>

        {/* Información del usuario actual */}
        <div className="bg-gray-50 rounded-lg p-4 mb-6">
          <p className="text-sm text-stone-gray">
            Usuario actual: <span className="font-medium">{state.user?.usuario}</span>
          </p>
          <p className="text-sm text-stone-gray">
            Rol: <span className="font-medium text-dominican-blue">{state.user?.nombreRol}</span>
          </p>
        </div>

        {/* Botones de acción */}
        <div className="space-y-3">
          <button
            onClick={handleGoBack}
            className="w-full px-4 py-2 bg-dominican-blue text-white rounded-lg hover:bg-opacity-90 smooth-transition font-medium"
          >
            Regresar
          </button>

          <button
            onClick={handleLogout}
            className="w-full px-4 py-2 border border-dominican-red text-dominican-red rounded-lg hover:bg-dominican-red hover:text-white smooth-transition font-medium"
          >
            Cerrar Sesión
          </button>
        </div>
      </div>
    </div>
  );
};

// ====================================
// HOOK PARA VERIFICAR PERMISOS
// ====================================

export const usePermissions = () => {
  const { hasRole, hasAnyRole, isAdmin, isCajero, isMesero, isRecepcion, isCocina, state } =
    useAuth();

  const canAccess = (requiredRole?: UserRole, requiredRoles?: UserRole[]): boolean => {
    if (!state.isAuthenticated) return false;

    if (requiredRole && !hasRole(requiredRole)) return false;
    if (requiredRoles && !hasAnyRole(requiredRoles)) return false;

    return true;
  };

  const canAccessAdmin = (): boolean => isAdmin();
  const canAccessCaja = (): boolean => hasAnyRole(['Cajero', 'Administrador']);
  const canAccessMesero = (): boolean => hasAnyRole(['Mesero', 'Administrador']);
  const canAccessRecepcion = (): boolean => hasAnyRole(['Recepcion', 'Administrador']);
  const canAccessCocina = (): boolean => hasAnyRole(['Cocina', 'Administrador']);

  return {
    canAccess,
    canAccessAdmin,
    canAccessCaja,
    canAccessMesero,
    canAccessRecepcion,
    canAccessCocina,
    hasRole,
    hasAnyRole,
    isAdmin,
    isCajero,
    isMesero,
    isRecepcion,
    isCocina,
    userRole: state.user?.nombreRol as UserRole,
  };
};

// ====================================
// COMPONENTE PARA MOSTRAR CONTENIDO CONDICIONAL
// ====================================

interface RoleGuardProps {
  children: ReactNode;
  requiredRole?: UserRole;
  requiredRoles?: UserRole[];
  fallback?: ReactNode;
}

export const RoleGuard: React.FC<RoleGuardProps> = ({
  children,
  requiredRole,
  requiredRoles,
  fallback = null,
}) => {
  const { canAccess } = usePermissions();

  if (!canAccess(requiredRole, requiredRoles)) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};

// ====================================
// COMPONENTE HOC PARA RUTAS ADMIN
// ====================================

interface AdminRouteProps {
  children: ReactNode;
}

export const AdminRoute: React.FC<AdminRouteProps> = ({ children }) => (
  <ProtectedRoute requiredRole="Administrador" showUnauthorized>
    {children}
  </ProtectedRoute>
);

// ====================================
// COMPONENTE HOC PARA RUTAS CAJERO
// ====================================

interface CajeroRouteProps {
  children: ReactNode;
}

export const CajeroRoute: React.FC<CajeroRouteProps> = ({ children }) => (
  <ProtectedRoute requiredRoles={['Cajero', 'Administrador']} showUnauthorized>
    {children}
  </ProtectedRoute>
);

// ====================================
// COMPONENTE HOC PARA RUTAS MESERO
// ====================================

interface MeseroRouteProps {
  children: ReactNode;
}

export const MeseroRoute: React.FC<MeseroRouteProps> = ({ children }) => (
  <ProtectedRoute requiredRoles={['Mesero', 'Administrador']} showUnauthorized>
    {children}
  </ProtectedRoute>
);

// ====================================
// COMPONENTE HOC PARA RUTAS RECEPCIÓN
// ====================================

interface RecepcionRouteProps {
  children: ReactNode;
}

export const RecepcionRoute: React.FC<RecepcionRouteProps> = ({ children }) => (
  <ProtectedRoute requiredRoles={['Recepcion', 'Administrador']} showUnauthorized>
    {children}
  </ProtectedRoute>
);

// ====================================
// COMPONENTE HOC PARA RUTAS COCINA
// ====================================

interface CocinaRouteProps {
  children: ReactNode;
}

export const CocinaRoute: React.FC<CocinaRouteProps> = ({ children }) => (
  <ProtectedRoute requiredRoles={['Cocina', 'Administrador']} showUnauthorized>
    {children}
  </ProtectedRoute>
);

export default ProtectedRoute;
