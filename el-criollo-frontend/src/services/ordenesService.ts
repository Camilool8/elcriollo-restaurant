// ====================================
// SERVICIO DE ÓRDENES - EL CRIOLLO
// ====================================
// Ubicación: src/services/ordenesService.ts

import { api, getErrorMessage } from './api';
import type {
  Orden,
  EstadoOrden,
  CrearOrdenRequest,
  ActualizarOrdenRequest,
  ActualizarEstadoOrdenRequest,
  AgregarItemsOrdenRequest,
  FiltrosOrden,
  EstadisticasOrdenes,
} from '@/types/orden';

class OrdenesService {
  // ============================================================================
  // OPERACIONES CRUD BÁSICAS
  // ============================================================================

  /**
   * Crea una nueva orden
   */
  async crearOrden(request: CrearOrdenRequest): Promise<Orden> {
    try {
      const response = await api.post<Orden>('/Orden', request);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error creando orden: ${message}`);
    }
  }

  /**
   * Obtiene una orden específica por ID
   */
  async getOrdenById(ordenId: number): Promise<Orden> {
    try {
      const response = await api.get<Orden>(`/Orden/${ordenId}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo orden: ${message}`);
    }
  }

  /**
   * Actualiza una orden existente
   */
  async actualizarOrden(ordenId: number, request: ActualizarOrdenRequest): Promise<Orden> {
    try {
      const response = await api.put<Orden>(`/Orden/${ordenId}`, request);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando orden: ${message}`);
    }
  }

  /**
   * Obtiene todas las órdenes activas (no finalizadas)
   */
  async getOrdenesActivas(): Promise<Orden[]> {
    try {
      const response = await api.get<Orden[]>('/Orden/activas');
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo órdenes activas: ${message}`);
    }
  }

  // ============================================================================
  // FILTROS Y BÚSQUEDAS
  // ============================================================================

  /**
   * Obtiene órdenes filtradas por estado
   */
  async getOrdenesByEstado(estado: EstadoOrden): Promise<Orden[]> {
    try {
      const response = await api.get<Orden[]>(`/Orden/estado/${estado}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo órdenes por estado: ${message}`);
    }
  }

  /**
   * Obtiene órdenes de una mesa específica
   */
  async getOrdenesByMesa(mesaId: number): Promise<Orden[]> {
    try {
      const response = await api.get<Orden[]>(`/Orden/mesa/${mesaId}`);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo órdenes de la mesa: ${message}`);
    }
  }

  // ============================================================================
  // GESTIÓN DE ESTADOS
  // ============================================================================

  /**
   * Actualiza el estado de una orden
   */
  async actualizarEstadoOrden(
    ordenId: number,
    request: ActualizarEstadoOrdenRequest
  ): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.put<{ success: boolean; message: string }>(
        `/Orden/${ordenId}/estado`,
        request
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error actualizando estado: ${message}`);
    }
  }

  /**
   * Marca una orden como lista (solo para cocina/admin)
   */
  async marcarOrdenLista(ordenId: number): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.post<{ success: boolean; message: string }>(
        `/Orden/${ordenId}/lista`
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error marcando orden como lista: ${message}`);
    }
  }

  /**
   * Cancela una orden
   */
  async cancelarOrden(
    ordenId: number,
    motivo?: string
  ): Promise<{ success: boolean; message: string }> {
    try {
      const response = await api.post<{ success: boolean; message: string }>(
        `/Orden/${ordenId}/cancelar`,
        { motivo }
      );
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error cancelando orden: ${message}`);
    }
  }

  // ============================================================================
  // GESTIÓN DE ITEMS
  // ============================================================================

  /**
   * Agrega items a una orden existente
   */
  async agregarItemsOrden(ordenId: number, request: AgregarItemsOrdenRequest): Promise<Orden> {
    try {
      const response = await api.post<Orden>(`/Orden/${ordenId}/items`, request);
      return response;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error agregando items: ${message}`);
    }
  }

  // ============================================================================
  // ESTADÍSTICAS Y REPORTES
  // ============================================================================

  /**
   * Calcula estadísticas de órdenes
   */
  async getEstadisticasOrdenes(): Promise<EstadisticasOrdenes> {
    try {
      const ordenes = await this.getOrdenesActivas();

      const stats: EstadisticasOrdenes = {
        totalOrdenes: ordenes.length,
        ordenesPendientes: ordenes.filter((o) => o.estado === 'Pendiente').length,
        ordenesEnPreparacion: ordenes.filter((o) => o.estado === 'En Preparacion').length,
        ordenesListas: ordenes.filter((o) => o.estado === 'Lista').length,
        ordenesEntregadas: ordenes.filter((o) => o.estado === 'Entregada').length,
        ordenesCanceladas: ordenes.filter((o) => o.estado === 'Cancelada').length,
        promedioTiempoPreparacion: 'Calculando...', // TODO: Implementar cálculo real
        ventasDelDia: ordenes.reduce((sum, orden) => sum + orden.totalCalculado, 0),
        ordenMasReciente:
          ordenes.sort(
            (a, b) => new Date(b.fechaCreacion).getTime() - new Date(a.fechaCreacion).getTime()
          )[0] || undefined,
      };

      return stats;
    } catch (error: any) {
      const message = getErrorMessage(error);
      throw new Error(`Error obteniendo estadísticas: ${message}`);
    }
  }

  // ============================================================================
  // UTILIDADES
  // ============================================================================

  /**
   * Filtra órdenes localmente con múltiples criterios
   */
  filtrarOrdenes(ordenes: Orden[], filtros: FiltrosOrden): Orden[] {
    let resultado = [...ordenes];

    if (filtros.estado) {
      resultado = resultado.filter((orden) => orden.estado === filtros.estado);
    }

    if (filtros.tipoOrden) {
      resultado = resultado.filter((orden) => orden.tipoOrden === filtros.tipoOrden);
    }

    if (filtros.mesaID) {
      resultado = resultado.filter((orden) => orden.mesaID === filtros.mesaID);
    }

    if (filtros.clienteID) {
      resultado = resultado.filter((orden) => orden.clienteID === filtros.clienteID);
    }

    if (filtros.soloActivas) {
      const estadosActivos: EstadoOrden[] = ['Pendiente', 'En Preparacion', 'Lista'];
      resultado = resultado.filter((orden) => estadosActivos.includes(orden.estado));
    }

    if (filtros.fechaDesde) {
      const fechaDesde = new Date(filtros.fechaDesde);
      resultado = resultado.filter((orden) => new Date(orden.fechaCreacion) >= fechaDesde);
    }

    if (filtros.fechaHasta) {
      const fechaHasta = new Date(filtros.fechaHasta);
      resultado = resultado.filter((orden) => new Date(orden.fechaCreacion) <= fechaHasta);
    }

    return resultado;
  }

  /**
   * Genera resumen de una orden para tooltips
   */
  getResumenOrden(orden: Orden): string {
    let resumen = `Orden ${orden.numeroOrden}`;

    if (orden.mesa) {
      resumen += ` - Mesa ${orden.mesa.numeroMesa}`;
    }

    if (orden.cliente) {
      resumen += `\n👤 ${orden.cliente.nombreCompleto}`;
    }

    resumen += `\n🍽️ ${orden.totalItems} items`;
    resumen += `\n💰 Total: RD$${orden.totalCalculado.toFixed(2)}`;
    resumen += `\n📅 ${new Date(orden.fechaCreacion).toLocaleString('es-DO')}`;

    if (orden.tiempoTranscurrido) {
      resumen += `\n⏱️ ${orden.tiempoTranscurrido}`;
    }

    if (orden.observaciones) {
      resumen += `\n📝 ${orden.observaciones}`;
    }

    return resumen;
  }

  /**
   * Verifica si una orden puede ser modificada
   */
  puedeModificarseOrden(orden: Orden): boolean {
    const estadosModificables: EstadoOrden[] = ['Pendiente', 'En Preparacion'];
    return estadosModificables.includes(orden.estado);
  }

  /**
   * Obtiene las transiciones de estado permitidas para una orden
   */
  getTransicionesPosibles(estadoActual: EstadoOrden): EstadoOrden[] {
    const transiciones: Record<EstadoOrden, EstadoOrden[]> = {
      Pendiente: ['En Preparacion', 'Cancelada'],
      'En Preparacion': ['Lista', 'Cancelada'],
      Lista: ['Entregada'],
      Entregada: [], // Estado final
      Cancelada: [], // Estado final
    };

    return transiciones[estadoActual] || [];
  }

  /**
   * Calcula el total de un carrito antes de crear la orden
   */
  calcularTotalCarrito(items: Array<{ precio: number; cantidad: number }>): {
    subtotal: number;
    impuesto: number;
    total: number;
  } {
    const subtotal = items.reduce((sum, item) => sum + item.precio * item.cantidad, 0);
    const impuesto = subtotal * 0.18; // ITBIS 18%
    const total = subtotal + impuesto;

    return {
      subtotal: Number(subtotal.toFixed(2)),
      impuesto: Number(impuesto.toFixed(2)),
      total: Number(total.toFixed(2)),
    };
  }
}

// Instancia singleton exportada
export const ordenesService = new OrdenesService();
