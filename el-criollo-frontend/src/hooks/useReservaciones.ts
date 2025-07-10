import { useState, useCallback, useEffect } from 'react';
import { showErrorToast, showSuccessToast } from '@/utils/toastUtils';
import { reservacionService } from '@/services/reservacionService';
import type {
  ReservacionConDetalles,
  CrearReservacionRequest,
  ActualizarReservacionRequest,
  FiltrosReservacion,
  EstadisticasReservacion,
} from '@/types/reservacion';

interface UseReservacionesOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
  filtros?: FiltrosReservacion;
}

export const useReservaciones = (options: UseReservacionesOptions = {}) => {
  const {
    autoRefresh = true,
    refreshInterval = 30000, // 30 segundos
    filtros,
  } = options;

  // Estados principales
  const [reservaciones, setReservaciones] = useState<ReservacionConDetalles[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [estadisticas, setEstadisticas] = useState<EstadisticasReservacion>({
    totalReservaciones: 0,
    reservacionesPendientes: 0,
    reservacionesConfirmadas: 0,
    reservacionesEnCurso: 0,
    reservacionesCompletadas: 0,
    reservacionesCanceladas: 0,
    reservacionesNoShow: 0,
    promedioPersonas: 0,
  });

  // ============================================================================
  // FUNCIONES PRINCIPALES
  // ============================================================================

  const cargarReservaciones = useCallback(async () => {
    try {
      setError(null);
      const data = await reservacionService.getReservaciones(filtros);
      setReservaciones(data);
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : 'Error al cargar reservaciones';
      setError(mensaje);
      showErrorToast(`Error: ${mensaje}`);
    } finally {
      setLoading(false);
    }
  }, [filtros]);

  const refrescar = useCallback(() => {
    setLoading(true);
    cargarReservaciones();
  }, [cargarReservaciones]);

  // ============================================================================
  // OPERACIONES CRUD
  // ============================================================================

  const crearReservacion = useCallback(async (data: CrearReservacionRequest) => {
    try {
      const nuevaReservacion = await reservacionService.crearReservacion(data);
      setReservaciones((prev) => [nuevaReservacion, ...prev]);
      showSuccessToast('Reservación creada exitosamente');
      return nuevaReservacion;
    } catch (error: any) {
      const mensaje = error.message || 'Error al crear la reservación';
      showErrorToast(mensaje);
      throw error;
    }
  }, []);

  const actualizarReservacion = useCallback(
    async (id: number, data: ActualizarReservacionRequest) => {
      try {
        const reservacionActualizada = await reservacionService.actualizarReservacion(id, data);
        setReservaciones((prev) =>
          prev.map((r) => (r.reservacionID === id ? reservacionActualizada : r))
        );
        showSuccessToast('Reservación actualizada exitosamente');
        return reservacionActualizada;
      } catch (error: any) {
        const mensaje = error.message || 'Error al actualizar la reservación';
        showErrorToast(mensaje);
        throw error;
      }
    },
    []
  );

  const cancelarReservacion = useCallback(async (id: number, motivo?: string) => {
    try {
      await reservacionService.cancelarReservacion(id, motivo);
      setReservaciones((prev) =>
        prev.map((r) => (r.reservacionID === id ? { ...r, estado: 'Cancelada' } : r))
      );
      showSuccessToast('Reservación cancelada exitosamente');
      return true;
    } catch (error: any) {
      const mensaje = error.message || 'Error al cancelar la reservación';
      showErrorToast(mensaje);
      throw error;
    }
  }, []);

  // ============================================================================
  // GESTIÓN DE ESTADOS
  // ============================================================================

  const confirmarReservacion = useCallback(async (id: number) => {
    try {
      const reservacionConfirmada = await reservacionService.confirmarReservacion(id);
      setReservaciones((prev) =>
        prev.map((r) => (r.reservacionID === id ? reservacionConfirmada : r))
      );
      showSuccessToast('Reservación confirmada exitosamente');
      return reservacionConfirmada;
    } catch (error: any) {
      const mensaje = error.message || 'Error al confirmar la reservación';
      showErrorToast(mensaje);
      throw error;
    }
  }, []);

  const iniciarReservacion = useCallback(async (id: number) => {
    try {
      const reservacionIniciada = await reservacionService.iniciarReservacion(id);
      setReservaciones((prev) =>
        prev.map((r) => (r.reservacionID === id ? reservacionIniciada : r))
      );
      showSuccessToast('Reservación iniciada exitosamente');
      return reservacionIniciada;
    } catch (error: any) {
      const mensaje = error.message || 'Error al iniciar la reservación';
      showErrorToast(mensaje);
      throw error;
    }
  }, []);

  const completarReservacion = useCallback(async (id: number) => {
    try {
      const reservacionCompletada = await reservacionService.completarReservacion(id);
      setReservaciones((prev) =>
        prev.map((r) => (r.reservacionID === id ? reservacionCompletada : r))
      );
      showSuccessToast('Reservación completada exitosamente');
      return reservacionCompletada;
    } catch (error: any) {
      const mensaje = error.message || 'Error al completar la reservación';
      showErrorToast(mensaje);
      throw error;
    }
  }, []);

  const marcarNoShow = useCallback(async (id: number) => {
    try {
      const reservacionNoShow = await reservacionService.marcarNoShow(id);
      setReservaciones((prev) => prev.map((r) => (r.reservacionID === id ? reservacionNoShow : r)));
      showSuccessToast('Reservación marcada como No Show');
      return reservacionNoShow;
    } catch (error: any) {
      const mensaje = error.message || 'Error al marcar como No Show';
      showErrorToast(mensaje);
      throw error;
    }
  }, []);

  // ============================================================================
  // CONSULTAS ESPECÍFICAS
  // ============================================================================

  const cargarEstadisticas = useCallback(async () => {
    try {
      const stats = await reservacionService.getEstadisticas();
      setEstadisticas(stats);
    } catch (error) {
      console.error('Error cargando estadísticas:', error);
    }
  }, []);

  const getReservacionesDelDia = useCallback(async () => {
    try {
      const data = await reservacionService.getReservacionesDelDia();
      return data;
    } catch (error) {
      console.error('Error obteniendo reservaciones del día:', error);
      throw error;
    }
  }, []);

  const getReservacionesPendientes = useCallback(async () => {
    try {
      const data = await reservacionService.getReservacionesPendientes();
      return data;
    } catch (error) {
      console.error('Error obteniendo reservaciones pendientes:', error);
      throw error;
    }
  }, []);

  const getReservacionesRetrasadas = useCallback(async () => {
    try {
      const data = await reservacionService.getReservacionesRetrasadas();
      return data;
    } catch (error) {
      console.error('Error obteniendo reservaciones retrasadas:', error);
      throw error;
    }
  }, []);

  const getReservacionesProximas = useCallback(async (minutos: number = 30) => {
    try {
      const data = await reservacionService.getReservacionesProximas(minutos);
      return data;
    } catch (error) {
      console.error('Error obteniendo reservaciones próximas:', error);
      throw error;
    }
  }, []);

  // ============================================================================
  // UTILIDADES
  // ============================================================================

  const getReservacionById = useCallback(
    (id: number) => {
      return reservaciones.find((r) => r.reservacionID === id);
    },
    [reservaciones]
  );

  const getReservacionesPorMesa = useCallback(
    (mesaId: number) => {
      return reservaciones.filter((r) => r.mesaID === mesaId);
    },
    [reservaciones]
  );

  const getReservacionesPorCliente = useCallback(
    (clienteId: number) => {
      return reservaciones.filter((r) => r.clienteID === clienteId);
    },
    [reservaciones]
  );

  const getReservacionesPorEstado = useCallback(
    (estado: string) => {
      return reservaciones.filter((r) => r.estado === estado);
    },
    [reservaciones]
  );

  // ============================================================================
  // EFECTOS
  // ============================================================================

  // Auto-refresh de reservaciones
  useEffect(() => {
    let interval: NodeJS.Timeout;

    if (autoRefresh && !loading) {
      interval = setInterval(() => {
        cargarReservaciones();
      }, refreshInterval);
    }

    return () => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [autoRefresh, refreshInterval, loading, cargarReservaciones]);

  // Carga inicial
  useEffect(() => {
    cargarReservaciones();
    cargarEstadisticas();
  }, [cargarReservaciones, cargarEstadisticas]);

  // ============================================================================
  // RETORNO
  // ============================================================================

  return {
    // Estados
    reservaciones,
    loading,
    error,
    estadisticas,

    // Funciones principales
    refrescar,
    cargarReservaciones,

    // Operaciones CRUD
    crearReservacion,
    actualizarReservacion,
    cancelarReservacion,

    // Gestión de estados
    confirmarReservacion,
    iniciarReservacion,
    completarReservacion,
    marcarNoShow,

    // Consultas específicas
    getReservacionesDelDia,
    getReservacionesPendientes,
    getReservacionesRetrasadas,
    getReservacionesProximas,

    // Utilidades
    getReservacionById,
    getReservacionesPorMesa,
    getReservacionesPorCliente,
    getReservacionesPorEstado,
  };
};
