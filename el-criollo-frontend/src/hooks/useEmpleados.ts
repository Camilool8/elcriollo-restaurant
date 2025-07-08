import { useState, useCallback, useEffect } from 'react';
import { toast } from 'react-toastify';
import { Empleado } from '@/types';
import { empleadoService } from '@/services/empleadoService';

export const useEmpleados = () => {
  const [empleados, setEmpleados] = useState<Empleado[]>([]);
  const [originalEmpleados, setOriginalEmpleados] = useState<Empleado[]>([]);
  const [departamentos, setDepartamentos] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Cargar empleados
  const loadEmpleados = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const data = await empleadoService.getEmpleados();
      setOriginalEmpleados(data || []);
      setEmpleados(data || []);
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar empleados';
      setError(errorMessage);
      setEmpleados([]);
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

  // Buscar empleados
  const searchEmpleados = useCallback(
    (query: string) => {
      const lowerQuery = query.trim().toLowerCase();
      if (!lowerQuery) {
        setEmpleados(originalEmpleados);
        return;
      }

      const filtered = originalEmpleados.filter(
        (empleado) =>
          empleado.nombreCompleto.toLowerCase().includes(lowerQuery) ||
          (empleado.cedula && empleado.cedula.includes(lowerQuery)) ||
          (empleado.email && empleado.email.toLowerCase().includes(lowerQuery))
      );
      setEmpleados(filtered);
    },
    [originalEmpleados]
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
    loadEmpleados,
    searchEmpleados,
    refreshEmpleados: loadEmpleados,
  };
};

export default useEmpleados;
