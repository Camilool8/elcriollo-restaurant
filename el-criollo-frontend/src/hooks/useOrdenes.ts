import { useState, useEffect, useCallback } from 'react';
import { showErrorToast, showSuccessToast } from '@/utils/toastUtils';
import { ordenesService } from '@/services/ordenesService';
import type {
  Orden,
  EstadoOrden,
  TipoOrden,
  CrearOrdenRequest,
  ActualizarOrdenRequest,
  ActualizarEstadoOrdenRequest,
  AgregarItemsOrdenRequest,
  FiltrosOrden,
  EstadisticasOrdenes,
} from '@/types/orden';

interface UseOrdenesOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
  filtroEstado?: EstadoOrden;
  filtroTipo?: TipoOrden;
  mesaId?: number;
  soloActivas?: boolean;
}

export const useOrdenes = (options: UseOrdenesOptions = {}) => {
  const {
    autoRefresh = false,
    refreshInterval = 30000, // 30 segundos para órdenes en tiempo real
    filtroEstado,
    filtroTipo,
    mesaId,
    soloActivas = true,
  } = options;

  // Estados principales
  const [ordenes, setOrdenes] = useState<Orden[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [estadisticas, setEstadisticas] = useState<EstadisticasOrdenes>({
    totalOrdenes: 0,
    ordenesPendientes: 0,
    ordenesEnPreparacion: 0,
    ordenesListas: 0,
    ordenesEntregadas: 0,
    ordenesCanceladas: 0,
    promedioTiempoPreparacion: '0 min',
    ventasDelDia: 0,
  });

  // Estado para el carrito de nueva orden
  const [carritoActivo, setCarritoActivo] = useState(false);

  // ============================================================================
  // OPERACIONES DE CARGA
  // ============================================================================

  /**
   * Carga órdenes según los filtros especificados
   */
  const cargarOrdenes = useCallback(async () => {
    try {
      setError(null);

      let ordenesData: Orden[];

      // Aplicar filtros específicos primero
      if (filtroEstado) {
        ordenesData = await ordenesService.getOrdenesByEstado(filtroEstado);
      } else if (mesaId) {
        ordenesData = await ordenesService.getOrdenesByMesa(mesaId);
      } else if (soloActivas) {
        ordenesData = await ordenesService.getOrdenesActivas();
      } else {
        // Por defecto, cargar órdenes activas para mejor performance
        ordenesData = await ordenesService.getOrdenesActivas();
      }

      // Aplicar filtros adicionales localmente
      const filtros: FiltrosOrden = {
        tipoOrden: filtroTipo,
        soloActivas,
      };

      const ordenesFiltradas = ordenesService.filtrarOrdenes(ordenesData, filtros);
      setOrdenes(ordenesFiltradas);

      // Calcular estadísticas solo si no hay filtros específicos
      if (!filtroEstado && !mesaId && !filtroTipo) {
        const stats = await ordenesService.getEstadisticasOrdenes();
        setEstadisticas(stats);
      }
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : 'Error al cargar órdenes';
      setError(mensaje);
      showErrorToast(`Error: ${mensaje}`);
    } finally {
      setLoading(false);
      setLastUpdated(new Date());
    }
  }, [filtroEstado, filtroTipo, mesaId, soloActivas]);

  /**
   * Refresca los datos inmediatamente
   */
  const refrescar = useCallback(() => {
    setLoading(true);
    cargarOrdenes();
  }, [cargarOrdenes]);

  /**
   * Actualiza una orden específica en el estado local
   */
  const actualizarOrdenLocal = useCallback((ordenActualizada: Orden) => {
    setOrdenes((prev) =>
      prev.map((o) => (o.ordenID === ordenActualizada.ordenID ? ordenActualizada : o))
    );
  }, []);

  // ============================================================================
  // OPERACIONES CRUD
  // ============================================================================

  /**
   * Crea una nueva orden
   */
  const crearOrden = useCallback(
    async (request: CrearOrdenRequest) => {
      try {
        const nuevaOrden = await ordenesService.crearOrden(request);

        showSuccessToast(`Orden ${nuevaOrden.numeroOrden} creada exitosamente`);
        await cargarOrdenes(); // Refrescar lista

        return nuevaOrden;
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al crear orden';
        showErrorToast(`Error: ${mensaje}`);
        throw err;
      }
    },
    [cargarOrdenes]
  );

  /**
   * Actualiza una orden completa (incluyendo items)
   */
  const actualizarOrden = useCallback(
    async (orden: Orden) => {
      try {
        const request: ActualizarOrdenRequest = {
          ordenID: orden.ordenID,
          observaciones: orden.observaciones,
          items:
            orden.detalles?.map((d) => ({
              productoId: d.producto!.productoID,
              cantidad: d.cantidad,
              notasEspeciales: d.observaciones,
            })) || [],
        };

        const ordenActualizada = await ordenesService.actualizarOrden(orden.ordenID, request);

        // Actualizar el estado localmente para reflejar el cambio de inmediato
        actualizarOrdenLocal(ordenActualizada);

        return ordenActualizada;
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al actualizar la orden';
        showErrorToast(`Error: ${mensaje}`);
        throw err;
      }
    },
    [actualizarOrdenLocal]
  );

  /**
   * Actualiza el estado de una orden
   */
  const actualizarEstadoOrden = useCallback(
    async (ordenId: number, nuevoEstado: EstadoOrden, observaciones?: string) => {
      try {
        const request: ActualizarEstadoOrdenRequest = {
          nuevoEstado,
          observaciones,
        };

        const resultado = await ordenesService.actualizarEstadoOrden(ordenId, request);

        if (resultado.success) {
          showSuccessToast(`Estado actualizado: ${resultado.message}`);
          await cargarOrdenes();
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al actualizar estado';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarOrdenes]
  );

  /**
   * Cancela una orden
   */
  const cancelarOrden = useCallback(
    async (ordenId: number, motivo?: string) => {
      try {
        const resultado = await ordenesService.cancelarOrden(ordenId, motivo);

        if (resultado.success) {
          showSuccessToast(`Orden cancelada: ${resultado.message}`);
          await cargarOrdenes();
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al cancelar orden';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarOrdenes]
  );

  /**
   * Marca una orden como lista
   */
  const marcarOrdenLista = useCallback(
    async (ordenId: number) => {
      try {
        const resultado = await ordenesService.marcarOrdenLista(ordenId);

        if (resultado.success) {
          showSuccessToast(`Orden marcada como lista: ${resultado.message}`);
          await cargarOrdenes();
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al marcar como lista';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarOrdenes]
  );

  /**
   * Agrega items a una orden existente
   */
  const agregarItemsOrden = useCallback(
    async (ordenId: number, request: AgregarItemsOrdenRequest) => {
      try {
        const ordenActualizada = await ordenesService.agregarItemsOrden(ordenId, request);

        showSuccessToast('Items agregados a la orden exitosamente');
        await cargarOrdenes();

        return ordenActualizada;
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al agregar items';
        showErrorToast(`Error: ${mensaje}`);
        throw err;
      }
    },
    [cargarOrdenes]
  );

  /**
   * Obtiene estadísticas de órdenes
   */
  const obtenerEstadisticas = useCallback(async () => {
    try {
      const resultado = await ordenesService.getEstadisticasOrdenes();
      return resultado;
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : 'Error al obtener estadísticas';
      showErrorToast(`Error: ${mensaje}`);
      throw err;
    }
  }, []);

  // ============================================================================
  // UTILIDADES Y BÚSQUEDAS
  // ============================================================================

  /**
   * Busca una orden por ID local
   */
  const buscarOrdenPorId = useCallback(
    (ordenId: number): Orden | undefined => {
      return ordenes.find((orden) => orden.ordenID === ordenId);
    },
    [ordenes]
  );

  /**
   * Busca orden por número de orden
   */
  const buscarOrdenPorNumero = useCallback(
    (numeroOrden: string): Orden | undefined => {
      return ordenes.find((orden) => orden.numeroOrden === numeroOrden);
    },
    [ordenes]
  );

  /**
   * Filtra órdenes por estado local
   */
  const filtrarOrdenesPorEstado = useCallback(
    (estado: EstadoOrden): Orden[] => {
      return ordenes.filter((orden) => orden.estado === estado);
    },
    [ordenes]
  );

  /**
   * Obtiene órdenes que necesitan atención urgente
   */
  const ordenesUrgentes = useCallback((): Orden[] => {
    return ordenes.filter((orden) => {
      // Órdenes pendientes por más de 30 minutos
      if (orden.estado === 'Pendiente') {
        const tiempoCreacion = new Date(orden.fechaCreacion).getTime();
        const ahora = new Date().getTime();
        const diferenciaMinutos = (ahora - tiempoCreacion) / (1000 * 60);
        return diferenciaMinutos > 30;
      }

      // Órdenes listas por más de 15 minutos
      if (orden.estado === 'Lista') {
        // TODO: Implementar cálculo basado en fecha de último cambio de estado
        return false;
      }

      return false;
    });
  }, [ordenes]);

  /**
   * Obtiene órdenes por mesa específica
   */
  const getOrdenesPorMesa = useCallback(
    (mesaId: number): Orden[] => {
      return ordenes.filter((orden) => orden.mesaID === mesaId);
    },
    [ordenes]
  );

  // ============================================================================
  // EFECTOS
  // ============================================================================

  // Auto-refresh de órdenes
  useEffect(() => {
    let interval: NodeJS.Timeout;

    if (autoRefresh && !loading) {
      interval = setInterval(() => {
        cargarOrdenes();
      }, refreshInterval);
    }

    return () => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [autoRefresh, refreshInterval, loading, cargarOrdenes]);

  // Carga inicial
  useEffect(() => {
    cargarOrdenes();
  }, [cargarOrdenes]);

  // ============================================================================
  // RETURN
  // ============================================================================

  return {
    ordenes,
    loading,
    error,
    lastUpdated,
    cargarOrdenes,
    crearOrden,
    actualizarOrden,
    actualizarEstadoOrden,
    cancelarOrden,
    marcarOrdenLista,
    agregarItemsOrden,
    refrescar,
    obtenerEstadisticas,

    // Utilidades de actualización
    actualizarOrdenLocal,
  };
};
