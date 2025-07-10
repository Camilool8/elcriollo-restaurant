import { useState, useEffect, useCallback } from 'react';
import { showErrorToast, showSuccessToast } from '@/utils/toastUtils';
import { mesasService } from '@/services/mesasService';
import { useAutoRefresh } from './useAutoRefresh';
import type {
  Mesa,
  EstadoMesa,
  CambioEstadoMesaRequest,
  MantenimientoMesaRequest,
} from '@/types/mesa';

interface UseMesasOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
  filtroEstado?: EstadoMesa;
}

export const useMesas = (options: UseMesasOptions = {}) => {
  const {
    autoRefresh = true,
    refreshInterval = 30000, // 30 segundos
    filtroEstado,
  } = options;

  // Estados
  const [mesas, setMesas] = useState<Mesa[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [estadisticas, setEstadisticas] = useState({
    totalMesas: 0,
    mesasLibres: 0,
    mesasOcupadas: 0,
    mesasReservadas: 0,
    mesasMantenimiento: 0,
    porcentajeOcupacion: 0,
  });

  // Cargar mesas
  const cargarMesas = useCallback(async () => {
    try {
      setError(null);

      let mesasData: Mesa[];
      if (filtroEstado) {
        mesasData = await mesasService.getMesasByEstado(filtroEstado);
      } else {
        mesasData = await mesasService.getAllMesas();
      }

      setMesas(mesasData);

      // Calcular estadísticas solo si no hay filtro
      if (!filtroEstado) {
        const stats = await mesasService.getEstadisticasMesas();
        setEstadisticas(stats);
      }
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : 'Error al cargar mesas';
      setError(mensaje);
      showErrorToast(`Error: ${mensaje}`);
    } finally {
      setLoading(false);
    }
  }, [filtroEstado]);

  // Refrescar datos
  const refrescar = useCallback(() => {
    setLoading(true);
    cargarMesas();
  }, [cargarMesas]);

  // Liberar mesa
  const liberarMesa = useCallback(
    async (mesaId: number) => {
      try {
        const resultado = await mesasService.liberarMesa(mesaId);

        if (resultado.success) {
          showSuccessToast(`Mesa liberada: ${resultado.message}`);
          await cargarMesas(); // Refrescar después del cambio
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al liberar mesa';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarMesas]
  );

  // Ocupar mesa
  const ocuparMesa = useCallback(
    async (mesaId: number) => {
      try {
        const resultado = await mesasService.ocuparMesa(mesaId);

        if (resultado.success) {
          showSuccessToast(`Mesa ocupada: ${resultado.message}`);
          await cargarMesas();
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al ocupar mesa';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarMesas]
  );

  // Cambiar estado de mesa
  const cambiarEstadoMesa = useCallback(
    async (mesaId: number, request: CambioEstadoMesaRequest) => {
      try {
        const resultado = await mesasService.cambiarEstadoMesa(mesaId, request);

        if (resultado.success) {
          showSuccessToast(`Estado cambiado: ${resultado.message}`);
          await cargarMesas();
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al cambiar estado';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarMesas]
  );

  // Marcar mantenimiento
  const marcarMantenimiento = useCallback(
    async (mesaId: number, request: MantenimientoMesaRequest) => {
      try {
        const resultado = await mesasService.marcarMantenimiento(mesaId, request);

        if (resultado.success) {
          showSuccessToast(`Mesa en mantenimiento: ${resultado.message}`);
          await cargarMesas();
          return true;
        } else {
          showErrorToast(`Error: ${resultado.message}`);
          return false;
        }
      } catch (err) {
        const mensaje = err instanceof Error ? err.message : 'Error al marcar mantenimiento';
        showErrorToast(`Error: ${mensaje}`);
        return false;
      }
    },
    [cargarMesas]
  );

  // Buscar mesa por número
  const buscarMesaPorNumero = useCallback(
    (numeroMesa: number): Mesa | undefined => {
      return mesas.find((mesa) => mesa.numeroMesa === numeroMesa);
    },
    [mesas]
  );

  // Filtrar mesas por estado local
  const filtrarMesasPorEstado = useCallback(
    (estado: EstadoMesa): Mesa[] => {
      return mesas.filter((mesa) => mesa.estado === estado);
    },
    [mesas]
  );

  // Obtener mesas que necesitan atención
  const mesasQueNecesitanAtencion = useCallback((): Mesa[] => {
    return mesas.filter(
      (mesa) =>
        mesa.requiereAtencion ||
        mesa.necesitaLimpieza ||
        (mesa.estado === 'Ocupada' && mesa.tiempoOcupada && mesa.tiempoOcupada.includes('h'))
    );
  }, [mesas]);

  // Obtener información detallada de órdenes de una mesa (para debugging)
  const getOrdenesDetalladas = useCallback(async (mesaId: number) => {
    try {
      const resultado = await mesasService.getOrdenesDetalladas(mesaId);
      console.log('🔍 Información detallada de mesa', mesaId, ':', resultado);
      return resultado;
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : 'Error obteniendo información detallada';
      showErrorToast(`Error: ${mensaje}`);
      return null;
    }
  }, []);

  // Auto-refresh control
  const autoRefreshControl = useAutoRefresh({
    enabled: autoRefresh,
    interval: refreshInterval,
    onRefresh: cargarMesas,
  });

  // Cargar datos iniciales
  useEffect(() => {
    cargarMesas();
  }, [cargarMesas]);

  return {
    // Datos
    mesas,
    estadisticas,
    loading,
    error,

    // Acciones principales
    refrescar,
    liberarMesa,
    ocuparMesa,
    cambiarEstadoMesa,
    marcarMantenimiento,

    // Utilidades
    buscarMesaPorNumero,
    filtrarMesasPorEstado,
    mesasQueNecesitanAtencion,
    getOrdenesDetalladas,

    // Auto-refresh control
    autoRefresh: autoRefreshControl,

    // Estadísticas computadas
    mesasLibres: filtrarMesasPorEstado('Libre'),
    mesasOcupadas: filtrarMesasPorEstado('Ocupada'),
    mesasReservadas: filtrarMesasPorEstado('Reservada'),
    mesasMantenimiento: filtrarMesasPorEstado('Mantenimiento'),
  };
};
