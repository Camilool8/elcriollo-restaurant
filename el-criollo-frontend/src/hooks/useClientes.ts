import { useState, useCallback, useEffect } from 'react';
import { showErrorToast, showSuccessToast } from '@/utils/toastUtils';
import { Cliente } from '@/types';
import { clienteService } from '@/services/clienteService';

export const useClientes = () => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);

  // Cargar clientes
  const loadClientes = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await clienteService.getClientes();
      setClientes(response || []);
      setTotalCount(response.length || 0);
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar clientes';
      setError(errorMessage);

      setTotalCount(2);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Crear cliente
  const createCliente = useCallback(async (clienteData: Cliente): Promise<Cliente | null> => {
    try {
      const newCliente = await clienteService.createCliente(clienteData);
      setClientes((prev) => [newCliente, ...prev]);
      setTotalCount((prev) => prev + 1);
      showSuccessToast('¡Cliente registrado exitosamente! 🎉');
      return newCliente;
    } catch (error: any) {
      const errorMessage = error.message || 'Error al registrar cliente';
      showErrorToast(errorMessage);
      return null;
    }
  }, []);

  // Buscar clientes
  const searchClientes = useCallback(
    async (query: string) => {
      if (query.trim()) {
        await loadClientes();
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
      showErrorToast('Error al obtener clientes frecuentes');
      return [];
    }
  }, []);

  // Obtener cumpleañeros
  const getClientesCumpleanos = useCallback(async () => {
    try {
      const cumpleanos = await clienteService.getClientesCumpleanos();
      return cumpleanos;
    } catch (error: any) {
      showErrorToast('Error al obtener cumpleañeros');
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
