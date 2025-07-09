import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { ordenesService } from '@/services/ordenesService';
import { useOrdenesContext } from '@/contexts/OrdenesContext';
import type { Orden } from '@/types';

interface UseOrdenesMesaOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const useOrdenesMesa = (mesaId: number, options: UseOrdenesMesaOptions = {}) => {
  const {
    autoRefresh = true,
    refreshInterval = 10000, // 10 segundos para órdenes de mesa
  } = options;

  const { ordenesActualizadas } = useOrdenesContext();
  const [ordenes, setOrdenes] = useState<Orden[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchOrdenes = useCallback(async () => {
    try {
      setError(null);
      setLoading(true);

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
  }, [mesaId]);

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

  // Efecto para auto-refresh cuando hay cambios en el contexto
  useEffect(() => {
    if (ordenesActualizadas.size > 0) {
      // Refrescar cuando se detectan cambios en órdenes
      fetchOrdenes();
    }
  }, [ordenesActualizadas, fetchOrdenes]);

  // Auto-refresh periódico
  useEffect(() => {
    let interval: NodeJS.Timeout;

    if (autoRefresh && !loading) {
      interval = setInterval(() => {
        fetchOrdenes();
      }, refreshInterval);
    }

    return () => {
      if (interval) {
        clearInterval(interval);
      }
    };
  }, [autoRefresh, refreshInterval, loading, fetchOrdenes]);

  return {
    ordenes,
    loading,
    error,
    refrescar,
    fetchOrdenes,
  };
};
