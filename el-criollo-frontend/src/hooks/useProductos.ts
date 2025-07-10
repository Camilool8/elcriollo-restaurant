import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { productosService } from '@/services/productosService';
import { Producto, CategoriaBasica } from '@/types';

interface UseProductosOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const useProductos = (options: UseProductosOptions = {}) => {
  const { autoRefresh = false, refreshInterval = 30000 } = options;

  const [productos, setProductos] = useState<Producto[]>([]);
  const [categorias, setCategorias] = useState<CategoriaBasica[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  // Cargar productos y categorías
  const loadData = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const [productosData, categoriasData] = await Promise.all([
        productosService.getAllProductos(),
        productosService.getCategorias(),
      ]);

      setProductos(productosData || []);
      // Convertir CategoriaBasicaResponse a CategoriaBasica
      const categoriasConvertidas = (categoriasData || []).map((cat: any) => ({
        categoriaID: cat.categoriaID,
        nombreCategoria: cat.nombreCategoria,
        descripcion: cat.descripcion,
        cantidadProductos: cat.cantidadProductos,
        productosDisponibles: cat.productosDisponibles,
      }));
      setCategorias(categoriasConvertidas);
      setLastUpdated(new Date());
    } catch (error: any) {
      const errorMessage = error.message || 'Error al cargar datos';
      setError(errorMessage);
      toast.error(errorMessage);
      setProductos([]);
      setCategorias([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Crear producto
  const createProducto = useCallback(
    async (productoData: {
      nombre: string;
      descripcion?: string;
      categoriaId: number;
      precio: number;
      tiempoPreparacion?: number;
      imagen?: string;
      costoPreparacion?: number;
      disponible?: boolean;
    }): Promise<Producto | null> => {
      try {
        const newProducto = await productosService.crearProducto(productoData);
        setProductos((prev) => [newProducto, ...prev]);
        setLastUpdated(new Date());
        toast.success('¡Producto creado exitosamente! 🎉');
        return newProducto;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al crear producto';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Actualizar producto
  const updateProducto = useCallback(
    async (
      productoId: number,
      productoData: {
        nombre?: string;
        descripcion?: string;
        categoriaId?: number;
        precio?: number;
        tiempoPreparacion?: number;
        imagen?: string;
        costoPreparacion?: number;
        disponible?: boolean;
      }
    ): Promise<Producto | null> => {
      try {
        const updatedProducto = await productosService.actualizarProducto(productoId, productoData);
        setProductos((prev) =>
          prev.map((prod) => (prod.productoID === productoId ? updatedProducto : prod))
        );
        setLastUpdated(new Date());
        toast.success('¡Producto actualizado exitosamente! 🎉');
        return updatedProducto;
      } catch (error: any) {
        const errorMessage = error.message || 'Error al actualizar producto';
        toast.error(errorMessage);
        return null;
      }
    },
    []
  );

  // Eliminar producto
  const deleteProducto = useCallback(async (productoId: number): Promise<boolean> => {
    try {
      await productosService.eliminarProducto(productoId);
      setProductos((prev) => prev.filter((prod) => prod.productoID !== productoId));
      setLastUpdated(new Date());
      toast.success('¡Producto eliminado exitosamente! 🎉');
      return true;
    } catch (error: any) {
      const errorMessage = error.message || 'Error al eliminar producto';
      toast.error(errorMessage);
      return false;
    }
  }, []);

  // Buscar productos
  const searchProductos = useCallback(
    (query: string, categoriaId?: number) => {
      const lowerQuery = query.trim().toLowerCase();
      let filtered = productos;

      if (lowerQuery) {
        filtered = filtered.filter(
          (producto) =>
            producto.nombre.toLowerCase().includes(lowerQuery) ||
            (producto.descripcion && producto.descripcion.toLowerCase().includes(lowerQuery))
        );
      }

      if (categoriaId) {
        filtered = filtered.filter((producto) => producto.categoria.categoriaID === categoriaId);
      }

      return filtered;
    },
    [productos]
  );

  // Obtener producto por ID
  const getProductoById = useCallback(
    (productoId: number): Producto | undefined => {
      return productos.find((prod) => prod.productoID === productoId);
    },
    [productos]
  );

  // Obtener productos activos
  const getProductosActivos = useCallback(() => {
    return productos.filter((prod) => prod.estaDisponible);
  }, [productos]);

  // Obtener productos por categoría
  const getProductosPorCategoria = useCallback(
    (categoriaId: number) => {
      return productos.filter((prod) => prod.categoria.categoriaID === categoriaId);
    },
    [productos]
  );

  // Obtener productos con stock bajo
  const getProductosStockBajo = useCallback(() => {
    return productos.filter((prod) => prod.inventario?.stockBajo);
  }, [productos]);

  // Obtener productos agotados
  const getProductosAgotados = useCallback(() => {
    return productos.filter((prod) => prod.inventario?.cantidadDisponible === 0);
  }, [productos]);

  // Auto-refresh
  useEffect(() => {
    let interval: NodeJS.Timeout;

    if (autoRefresh && !isLoading) {
      interval = setInterval(() => {
        loadData();
      }, refreshInterval);
    }

    return () => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [autoRefresh, refreshInterval, isLoading, loadData]);

  // Cargar datos iniciales
  useEffect(() => {
    loadData();
  }, [loadData]);

  return {
    // Datos
    productos,
    categorias,
    isLoading,
    error,
    lastUpdated,

    // Acciones principales
    loadData,
    createProducto,
    updateProducto,
    deleteProducto,
    searchProductos,

    // Utilidades
    getProductoById,
    getProductosActivos,
    getProductosPorCategoria,
    getProductosStockBajo,
    getProductosAgotados,

    // Estadísticas computadas
    totalProductos: productos.length,
    productosActivos: getProductosActivos().length,
    productosInactivos: productos.length - getProductosActivos().length,
    productosStockBajo: getProductosStockBajo().length,
    productosAgotados: getProductosAgotados().length,
    totalCategorias: categorias.length,
  };
};
