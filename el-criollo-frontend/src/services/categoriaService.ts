import { api } from './api';
import { getErrorMessage } from './api';
import type { Categoria } from '@/types';

class CategoriaService {
  /**
   * Obtener todas las categorías
   */
  async getCategorias(): Promise<Categoria[]> {
    try {
      const response = await api.get<Categoria[]>('/Categorias');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo categorías: ${message}`);
    }
  }

  /**
   * Obtener categoría por ID
   */
  async getCategoriaById(id: number): Promise<Categoria> {
    try {
      const response = await api.get<Categoria>(`/Categorias/${id}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo categoría: ${message}`);
    }
  }

  /**
   * Crear nueva categoría
   */
  async crearCategoria(categoria: { nombre: string; descripcion?: string }): Promise<Categoria> {
    try {
      const response = await api.post<Categoria>('/Categorias', categoria);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando categoría: ${message}`);
    }
  }

  /**
   * Actualizar categoría existente
   */
  async actualizarCategoria(
    id: number,
    categoria: {
      nombre: string;
      descripcion?: string;
      estado?: boolean;
    }
  ): Promise<Categoria> {
    try {
      const response = await api.put<Categoria>(`/Categorias/${id}`, categoria);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando categoría: ${message}`);
    }
  }

  /**
   * Eliminar categoría
   */
  async eliminarCategoria(id: number): Promise<void> {
    try {
      await api.delete(`/Categorias/${id}`);
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error eliminando categoría: ${message}`);
    }
  }
}

// Exportar instancia singleton
export const categoriaService = new CategoriaService();
export default categoriaService;
