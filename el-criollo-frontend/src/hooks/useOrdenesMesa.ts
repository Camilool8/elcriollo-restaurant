import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { ordenesService } from '@/services/ordenesService';
import { useOrdenesContext } from '@/contexts/OrdenesContext';
import { useAutoRefresh } from './useAutoRefresh';
import type { Orden } from '@/types';

interface UseOrdenesMesaOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const useOrdenesMesa = (mesaId: number, options: UseOrdenesMesaOptions = {}) => {
  const {
    autoRefresh = true,
    refreshInterval = 30000, // 30 segundos para órdenes de mesa (reducido para evitar parpadeo)
  } = options;

  const { ordenesActualizadas } = useOrdenesContext();
  const [ordenes, setOrdenes] = useState<Orden[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastRefreshTime, setLastRefreshTime] = useState<number>(0);

  const fetchOrdenes = useCallback(async () => {
    // Evitar refrescos muy frecuentes (mínimo 1 segundo entre refrescos)
    const now = Date.now();
    if (now - lastRefreshTime < 1000) {
      console.log('🔄 Evitando refresco muy frecuente');
      return;
    }

    try {
      setError(null);
      setLoading(true);
      setLastRefreshTime(now);

      const data = await ordenesService.getOrdenesByMesa(mesaId);
      const ordenesActivas = data.filter(
        (o) =>
          o.estado !== 'Entregada' &&
          o.estado !== 'Cancelada' &&
          o.estado !== 'Facturada' &&
          o.estado !== 'Completada'
      );

      setOrdenes(ordenesActivas);
    } catch (err) {
      const mensaje = err instanceof Error ? err.message : 'Error al cargar las órdenes de la mesa';
      setError(mensaje);
      toast.error(mensaje);
    } finally {
      setLoading(false);
    }
  }, [mesaId, lastRefreshTime]);

  // Refrescar datos
  const refrescar = useCallback(() => {
    fetchOrdenes();
  }, [fetchOrdenes]);

  // Efecto para cargar datos iniciales
  useEffect(() => {
    if (mesaId) {
      fetchOrdenes();
    }
  }, [mesaId, fetchOrdenes]);

  // Efecto para refrescar cuando hay cambios en órdenes de esta mesa
  useEffect(() => {
    if (ordenesActualizadas.size > 0) {
      const ordenesDeEstaMesa = ordenes.map((o) => o.ordenID);
      const hayCambiosEnEstaMesa = Array.from(ordenesActualizadas).some((ordenId) =>
        ordenesDeEstaMesa.includes(ordenId)
      );

      if (hayCambiosEnEstaMesa && !loading) {
        console.log(
          '🔄 Refrescando inmediatamente por cambio en orden de esta mesa:',
          Array.from(ordenesActualizadas)
        );
        // Refrescar inmediatamente solo si no está ya cargando
        fetchOrdenes();
      }
    }
  }, [ordenesActualizadas, ordenes, fetchOrdenes, loading]);

  // Auto-refresh control
  const autoRefreshControl = useAutoRefresh({
    enabled: autoRefresh,
    interval: refreshInterval,
    onRefresh: fetchOrdenes,
  });

  return {
    ordenes,
    loading,
    error,
    refrescar,
    fetchOrdenes,
    autoRefresh: autoRefreshControl,
  };
};
