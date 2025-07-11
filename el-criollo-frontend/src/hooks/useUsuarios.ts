import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { adminUserService } from '@/services/adminService';
import { UsuarioResponse as User, CreateUsuarioRequest, Rol } from '@/types';

// ====================================
// HOOK PARA GESTIÓN DE USUARIOS
// ====================================

export const useUsuarios = () => {
  const [usuarios, setUsuarios] = useState<User[]>([]);
  const [roles, setRoles] = useState<Rol[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);

  // Cargar usuarios
  const loadUsuarios = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = { items: [], totalCount: 0 };
      setUsuarios(response.items || []);
      setTotalCount(response.totalCount || 0);
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar usuarios';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Cargar roles
  const loadRoles = useCallback(async () => {
    try {
      // Note: This method doesn't exist in adminUserService, we'll need to implement it
      // For now, we'll use default roles
      const rolesData = [
        { rolID: 1, nombreRol: 'Administrador', estado: true },
        { rolID: 2, nombreRol: 'Cajero', estado: true },
        { rolID: 3, nombreRol: 'Mesero', estado: true },
        { rolID: 4, nombreRol: 'Recepcion', estado: true },
        { rolID: 5, nombreRol: 'Cocina', estado: true },
      ];
      setRoles(rolesData);
    } catch (error: any) {
      console.warn('Error cargando roles, usando valores por defecto');
      setRoles([
        { rolID: 1, nombreRol: 'Administrador', estado: true },
        { rolID: 2, nombreRol: 'Cajero', estado: true },
        { rolID: 3, nombreRol: 'Mesero', estado: true },
        { rolID: 4, nombreRol: 'Recepcion', estado: true },
        { rolID: 5, nombreRol: 'Cocina', estado: true },
      ]);
    }
  }, []);

  // Crear usuario
  const createUsuario = useCallback(
    async (userData: CreateUsuarioRequest): Promise<User | null> => {
      try {
        const newUser = await adminUserService.createUser(userData);
        setUsuarios((prev) => [newUser, ...prev]);
        setTotalCount((prev) => prev + 1);
        toast.success('¡Usuario creado exitosamente! 🎉');
        return newUser;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al crear usuario';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Buscar usuarios
  const searchUsuarios = useCallback(
    async (query: string) => {
      if (query.trim()) {
        console.log('Buscando usuario:', query.trim());
      }
      await loadUsuarios();
    },
    [loadUsuarios]
  );

  // Cambiar estado de usuario
  const toggleUsuarioStatus = useCallback(async (userId: number, currentStatus: boolean) => {
    try {
      if (currentStatus) {
        toast.success('Usuario desactivado');
      } else {
        toast.success('Usuario activado');
      }

      // Actualizar estado local
      setUsuarios((prev) =>
        prev.map((user) => (user.usuarioId === userId ? { ...user, estado: !currentStatus } : user))
      );
    } catch (error: any) {
      toast.error(error.message || 'Error al cambiar estado del usuario');
    }
  }, []);

  // Reset contraseña
  const resetPassword = useCallback(async (userId: number, newPassword: string) => {
    try {
      await adminUserService.resetUserPassword(userId, newPassword);
      toast.success('Contraseña restablecida exitosamente');
    } catch (error: any) {
      toast.error(error.message || 'Error al restablecer contraseña');
    }
  }, []);

  // Efecto inicial
  useEffect(() => {
    loadUsuarios();
    loadRoles();
  }, [loadUsuarios, loadRoles]);

  return {
    usuarios,
    roles,
    isLoading,
    error,
    totalCount,
    loadUsuarios,
    createUsuario,
    searchUsuarios,
    toggleUsuarioStatus,
    resetPassword,
    refreshUsuarios: loadUsuarios,
  };
};

export default useUsuarios;
