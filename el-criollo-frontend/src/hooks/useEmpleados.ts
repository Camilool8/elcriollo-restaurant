import { useState, useCallback, useEffect } from 'react';
import { toast } from 'react-toastify';
import { Empleado, CreateEmpleadoRequest, SearchEmpleadoParams } from '@/types';
import { empleadoService } from '@/services/empleadoService';

export const useEmpleados = () => {
  const [empleados, setEmpleados] = useState<Empleado[]>([]);
  const [departamentos, setDepartamentos] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);

  // Cargar empleados
  const loadEmpleados = useCallback(async (params?: SearchEmpleadoParams) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await empleadoService.getEmpleados(params);
      setEmpleados(response.items || []);
      setTotalCount(response.totalCount || 0);
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar empleados';
      setError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Cargar departamentos
  const loadDepartamentos = useCallback(async () => {
    try {
      const deps = await empleadoService.getDepartamentos();
      setDepartamentos(deps);
    } catch (error: any) {
      console.warn('Error cargando departamentos, usando valores por defecto');
      setDepartamentos([
        'Administración',
        'Cocina',
        'Servicio',
        'Caja',
        'Recepción',
        'Limpieza',
        'Seguridad',
      ]);
    }
  }, []);

  // Crear empleado
  const createEmpleado = useCallback(
    async (empleadoData: CreateEmpleadoRequest): Promise<Empleado | null> => {
      try {
        const newEmpleado = await empleadoService.createEmpleado(empleadoData);
        setEmpleados((prev) => [newEmpleado, ...prev]);
        setTotalCount((prev) => prev + 1);
        toast.success('¡Empleado agregado exitosamente! 💼');
        return newEmpleado;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al agregar empleado';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Buscar empleados
  const searchEmpleados = useCallback(
    async (query: string) => {
      if (query.trim()) {
        await loadEmpleados({ query: query.trim() });
      } else {
        await loadEmpleados();
      }
    },
    [loadEmpleados]
  );

  // Efecto inicial
  useEffect(() => {
    loadEmpleados();
    loadDepartamentos();
  }, [loadEmpleados, loadDepartamentos]);

  return {
    empleados,
    departamentos,
    isLoading,
    error,
    totalCount,
    loadEmpleados,
    createEmpleado,
    searchEmpleados,
    refreshEmpleados: loadEmpleados,
  };
};

export default useEmpleados;
