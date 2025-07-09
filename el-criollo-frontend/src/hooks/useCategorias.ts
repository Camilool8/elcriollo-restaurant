import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { categoriaService } from '@/services/categoriaService';
import { Categoria } from '@/types';

interface UseCategoriasOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const useCategorias = (options: UseCategoriasOptions = {}) => {
  const { autoRefresh = false, refreshInterval = 30000 } = options;

  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  // Cargar categorías
  const loadCategorias = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await categoriaService.getCategorias();
      setCategorias(response || []);
      setLastUpdated(new Date());
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar categorías';
      setError(errorMessage);
      toast.error(errorMessage);
      setCategorias([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Crear categoría
  const createCategoria = useCallback(
    async (categoriaData: { nombre: string; descripcion?: string }): Promise<Categoria | null> => {
      try {
        const newCategoria = await categoriaService.crearCategoria(categoriaData);
        setCategorias((prev) => [newCategoria, ...prev]);
        setLastUpdated(new Date());
        toast.success('¡Categoría creada exitosamente! 🎉');
        return newCategoria;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al crear categoría';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Actualizar categoría
  const updateCategoria = useCallback(
    async (
      categoriaId: number,
      categoriaData: { nombre: string; descripcion?: string; estado?: boolean }
    ): Promise<Categoria | null> => {
      try {
        const updatedCategoria = await categoriaService.actualizarCategoria(
          categoriaId,
          categoriaData
        );
        setCategorias((prev) =>
          prev.map((cat) => (cat.categoriaID === categoriaId ? updatedCategoria : cat))
        );
        setLastUpdated(new Date());
        toast.success('¡Categoría actualizada exitosamente! 🎉');
        return updatedCategoria;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al actualizar categoría';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Eliminar categoría
  const deleteCategoria = useCallback(async (categoriaId: number): Promise<boolean> => {
    try {
      await categoriaService.eliminarCategoria(categoriaId);
      setCategorias((prev) => prev.filter((cat) => cat.categoriaID !== categoriaId));
      setLastUpdated(new Date());
      toast.success('¡Categoría eliminada exitosamente! 🎉');
      return true;
    } catch (error: any) {
      const errorMessage = error.message || 'Error al eliminar categoría';
      toast.error(errorMessage);
      return false;
    }
  }, []);

  // Buscar categorías
  const searchCategorias = useCallback(
    (query: string) => {
      const lowerQuery = query.trim().toLowerCase();
      if (!lowerQuery) {
        return categorias;
      }

      return categorias.filter(
        (categoria) =>
          categoria.nombre.toLowerCase().includes(lowerQuery) ||
          (categoria.descripcion && categoria.descripcion.toLowerCase().includes(lowerQuery))
      );
    },
    [categorias]
  );

  // Obtener categoría por ID
  const getCategoriaById = useCallback(
    (categoriaId: number): Categoria | undefined => {
      return categorias.find((cat) => cat.categoriaID === categoriaId);
    },
    [categorias]
  );

  // Obtener categorías activas
  const getCategoriasActivas = useCallback(() => {
    return categorias.filter((cat) => cat.estado);
  }, [categorias]);

  // Obtener categorías con productos
  const getCategoriasConProductos = useCallback(() => {
    return categorias.filter((cat) => cat.totalProductos > 0);
  }, [categorias]);

  // Obtener categorías sin productos
  const getCategoriasSinProductos = useCallback(() => {
    return categorias.filter((cat) => cat.totalProductos === 0);
  }, [categorias]);

  // Auto-refresh
  useEffect(() => {
    let interval: NodeJS.Timeout;

    if (autoRefresh && !isLoading) {
      interval = setInterval(() => {
        loadCategorias();
      }, refreshInterval);
    }

    return () => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [autoRefresh, refreshInterval, isLoading, loadCategorias]);

  // Cargar datos iniciales
  useEffect(() => {
    loadCategorias();
  }, [loadCategorias]);

  return {
    // Datos
    categorias,
    isLoading,
    error,
    lastUpdated,

    // Acciones principales
    loadCategorias,
    createCategoria,
    updateCategoria,
    deleteCategoria,
    searchCategorias,

    // Utilidades
    getCategoriaById,
    getCategoriasActivas,
    getCategoriasConProductos,
    getCategoriasSinProductos,

    // Estadísticas computadas
    totalCategorias: categorias.length,
    totalProductos: categorias.reduce((sum, cat) => sum + cat.totalProductos, 0),
    categoriasActivas: getCategoriasActivas().length,
    categoriasConProductos: getCategoriasConProductos().length,
    categoriasSinProductos: getCategoriasSinProductos().length,
  };
};
