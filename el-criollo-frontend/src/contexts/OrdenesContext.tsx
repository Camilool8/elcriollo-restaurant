import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import type { Orden } from '@/types';

interface OrdenesContextType {
  // Estado
  ordenesActualizadas: Set<number>;

  // Acciones
  marcarOrdenActualizada: (ordenId: number) => void;
  limpiarOrdenesActualizadas: () => void;
  notificarCambioOrden: (ordenId: number) => void;

  // Utilidades
  esOrdenReciente: (ordenId: number) => boolean;
}

const OrdenesContext = createContext<OrdenesContextType | undefined>(undefined);

interface OrdenesProviderProps {
  children: ReactNode;
}

export const OrdenesProvider: React.FC<OrdenesProviderProps> = ({ children }) => {
  const [ordenesActualizadas, setOrdenesActualizadas] = useState<Set<number>>(new Set());

  const marcarOrdenActualizada = useCallback((ordenId: number) => {
    setOrdenesActualizadas((prev) => new Set([...prev, ordenId]));

    // Limpiar después de 5 segundos para evitar acumulación
    setTimeout(() => {
      setOrdenesActualizadas((prev) => {
        const nuevo = new Set(prev);
        nuevo.delete(ordenId);
        return nuevo;
      });
    }, 5000);
  }, []);

  const limpiarOrdenesActualizadas = useCallback(() => {
    setOrdenesActualizadas(new Set());
  }, []);

  const notificarCambioOrden = useCallback(
    (ordenId: number) => {
      marcarOrdenActualizada(ordenId);
    },
    [marcarOrdenActualizada]
  );

  const esOrdenReciente = useCallback(
    (ordenId: number) => {
      return ordenesActualizadas.has(ordenId);
    },
    [ordenesActualizadas]
  );

  const value: OrdenesContextType = {
    ordenesActualizadas,
    marcarOrdenActualizada,
    limpiarOrdenesActualizadas,
    notificarCambioOrden,
    esOrdenReciente,
  };

  return <OrdenesContext.Provider value={value}>{children}</OrdenesContext.Provider>;
};

export const useOrdenesContext = (): OrdenesContextType => {
  const context = useContext(OrdenesContext);
  if (context === undefined) {
    throw new Error('useOrdenesContext debe ser usado dentro de un OrdenesProvider');
  }
  return context;
};
