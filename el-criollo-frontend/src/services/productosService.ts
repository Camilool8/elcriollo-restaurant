import { api } from './api';
import { getErrorMessage } from './api';
import type { Producto, Categoria, Combo } from '@/types';

class ProductosService {
  /**
   * Obtener todos los productos
   */
  async getAllProductos(): Promise<Producto[]> {
    try {
      const response = await api.get<Producto[]>('/Productos');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo productos: ${message}`);
    }
  }

  /**
   * Obtener producto por ID
   */
  async getProductoById(id: number): Promise<Producto> {
    try {
      const response = await api.get<Producto>(`/Productos/${id}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo producto: ${message}`);
    }
  }

  /**
   * Obtener productos por categoría
   */
  async getProductosPorCategoria(categoriaId: number): Promise<Producto[]> {
    try {
      const response = await api.get<Producto[]>(`/Productos/categoria/${categoriaId}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo productos por categoría: ${message}`);
    }
  }

  /**
   * Obtener menú digital completo
   */
  async getMenuDigital(): Promise<any> {
    try {
      const response = await api.get('/Productos/menu-digital');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo menú digital: ${message}`);
    }
  }

  /**
   * Obtener todas las categorías
   */
  async getCategorias(): Promise<Categoria[]> {
    try {
      const response = await api.get<Categoria[]>('/Productos/categorias');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo categorías: ${message}`);
    }
  }

  /**
   * Obtener combos disponibles
   */
  async getCombos(): Promise<Combo[]> {
    try {
      const response = await api.get<Combo[]>('/Productos/combos');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo combos: ${message}`);
    }
  }

  /**
   * Buscar productos por nombre
   */
  async buscarProductos(termino: string): Promise<Producto[]> {
    try {
      const response = await api.get<Producto[]>('/Productos/buscar', {
        params: { termino },
      });
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error buscando productos: ${message}`);
    }
  }

  /**
   * Crear nuevo producto
   */
  async crearProducto(producto: Omit<Producto, 'productoID'>): Promise<Producto> {
    try {
      const response = await api.post<Producto>('/Productos', producto);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando producto: ${message}`);
    }
  }

  /**
   * Actualizar producto existente
   */
  async actualizarProducto(id: number, producto: Partial<Producto>): Promise<Producto> {
    try {
      const response = await api.put<Producto>(`/Productos/${id}`, producto);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando producto: ${message}`);
    }
  }

  /**
   * Eliminar producto
   */
  async eliminarProducto(id: number): Promise<void> {
    try {
      await api.delete(`/Productos/${id}`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error eliminando producto: ${message}`);
    }
  }
}

// Exportar instancia singleton
export const productosService = new ProductosService();
export default productosService;
