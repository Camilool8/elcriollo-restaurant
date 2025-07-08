import { useState, useCallback, useEffect } from 'react';
import { toast } from 'react-toastify';
import { Cliente, CreateClienteRequest, SearchClienteParams } from '@/types';
import { clienteService } from '@/services/clienteService';

export const useClientes = () => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);

  // Cargar clientes
  const loadClientes = useCallback(async (params?: SearchClienteParams) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await clienteService.getClientes(params);
      setClientes(response.items || []);
      setTotalCount(response.totalCount || 0);
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar clientes';
      setError(errorMessage);

      // Datos de ejemplo para desarrollo
      setClientes([
        {
          clienteID: 1,
          nombre: 'María',
          apellido: 'González',
          cedula: '123-1234567-1',
          telefono: '809-123-4567',
          email: 'maria@ejemplo.com',
          estado: 'Activo',
          fechaRegistro: '2024-01-15T00:00:00Z',
        },
        {
          clienteID: 2,
          nombre: 'Juan',
          apellido: 'Pérez',
          telefono: '829-987-6543',
          email: 'juan@ejemplo.com',
          estado: 'Activo',
          fechaRegistro: '2024-02-01T00:00:00Z',
        },
      ]);
      setTotalCount(2);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Crear cliente
  const createCliente = useCallback(
    async (clienteData: CreateClienteRequest): Promise<Cliente | null> => {
      try {
        const newCliente = await clienteService.createCliente(clienteData);
        setClientes((prev) => [newCliente, ...prev]);
        setTotalCount((prev) => prev + 1);
        toast.success('¡Cliente registrado exitosamente! 🎉');
        return newCliente;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al registrar cliente';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Buscar clientes
  const searchClientes = useCallback(
    async (query: string) => {
      if (query.trim()) {
        await loadClientes({ query: query.trim() });
      } else {
        await loadClientes();
      }
    },
    [loadClientes]
  );

  // Obtener clientes frecuentes
  const getClientesFrecuentes = useCallback(async () => {
    try {
      const frecuentes = await clienteService.getClientesFrecuentes();
      return frecuentes;
    } catch (error: any) {
      toast.error('Error al obtener clientes frecuentes');
      return [];
    }
  }, []);

  // Obtener cumpleañeros
  const getClientesCumpleanos = useCallback(async () => {
    try {
      const cumpleanos = await clienteService.getClientesCumpleanos();
      return cumpleanos;
    } catch (error: any) {
      toast.error('Error al obtener cumpleañeros');
      return [];
    }
  }, []);

  // Efecto inicial
  useEffect(() => {
    loadClientes();
  }, [loadClientes]);

  return {
    clientes,
    isLoading,
    error,
    totalCount,
    loadClientes,
    createCliente,
    searchClientes,
    getClientesFrecuentes,
    getClientesCumpleanos,
    refreshClientes: loadClientes,
  };
};

export default useClientes;
