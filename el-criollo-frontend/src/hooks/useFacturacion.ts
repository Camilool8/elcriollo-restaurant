import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';

// Services
import { facturaService } from '@/services/facturaService';
import { useAutoRefresh } from './useAutoRefresh';

// Types
import type {
  Factura,
  CrearFacturaRequest,
  PagoRequest,
  PagoResponse,
  MetodoPago,
  EstadisticasFacturacion,
  FacturaEstado,
} from '@/types';

// ============================================================================
// INTERFACES
// ============================================================================

interface UseFacturacionOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

interface FacturacionState {
  facturas: Factura[];
  facturasDelDia: Factura[];
  estadisticas: EstadisticasFacturacion | null;
  facturaActual: Factura | null;
  isLoading: boolean;
  isCreating: boolean;
  isPaying: boolean;
  error: string | null;
  lastUpdated: string | null;
}

export interface UseFacturacionReturn {
  // Estado
  state: FacturacionState;

  // Acciones de facturas
  crearFactura: (request: CrearFacturaRequest) => Promise<Factura>;
  anularFactura: (facturaId: number, razon: string) => Promise<void>;

  // Acciones de pagos
  procesarPago: (request: PagoRequest) => Promise<PagoResponse>;

  // Consultas
  obtenerFactura: (facturaId: number) => Promise<Factura>;
  obtenerFacturasPorOrden: (ordenId: number) => Promise<Factura[]>;
  obtenerFacturasDelDia: () => Promise<Factura[]>;
  obtenerEstadisticas: () => Promise<EstadisticasFacturacion>;

  // Acciones de UI
  seleccionarFactura: (factura: Factura | null) => void;
  limpiarError: () => void;
  refrescarDatos: () => Promise<void>;

  // Utilidades
  calcularTotalFacturado: (facturas: Factura[]) => number;
  filtrarFacturasPorEstado: (estado: FacturaEstado) => Factura[];
  filtrarFacturasPorMetodo: (metodo: MetodoPago) => Factura[];

  // Validaciones
  validarCrearFactura: (request: CrearFacturaRequest) => string | null;

  // Auto-refresh control
  autoRefresh: {
    isEnabled: boolean;
    isRefreshing: boolean;
    lastRefresh: Date | null;
    toggleAutoRefresh: () => void;
    enableAutoRefresh: () => void;
    disableAutoRefresh: () => void;
    refreshNow: () => Promise<void>;
    setInterval: (interval: number) => void;
  };
}

// ============================================================================
// ESTADO INICIAL
// ============================================================================

const estadoInicial: FacturacionState = {
  facturas: [],
  facturasDelDia: [],
  estadisticas: null,
  facturaActual: null,
  isLoading: false,
  isCreating: false,
  isPaying: false,
  error: null,
  lastUpdated: null,
};

// ============================================================================
// HOOK PRINCIPAL
// ============================================================================

export const useFacturacion = (options: UseFacturacionOptions = {}): UseFacturacionReturn => {
  const { autoRefresh = false, refreshInterval = 30000 } = options;

  const [state, setState] = useState<FacturacionState>(estadoInicial);

  // ============================================================================
  // FUNCIONES AUXILIARES
  // ============================================================================

  const actualizarEstado = useCallback((updates: Partial<FacturacionState>) => {
    setState((prev) => ({
      ...prev,
      ...updates,
      lastUpdated: new Date().toISOString(),
    }));
  }, []);

  const manejarError = useCallback(
    (error: any, accion: string) => {
      console.error(`Error en ${accion}:`, error);
      const mensaje = error.message || `Error al ${accion}`;
      actualizarEstado({ error: mensaje, isLoading: false });
      toast.error(mensaje);
      return mensaje;
    },
    [actualizarEstado]
  );

  // ============================================================================
  // FUNCIONES DE FACTURACIÓN
  // ============================================================================

  const crearFactura = useCallback(
    async (request: CrearFacturaRequest): Promise<Factura> => {
      try {
        // Validar request
        const errorValidacion = validarCrearFactura(request);
        if (errorValidacion) {
          throw new Error(errorValidacion);
        }

        actualizarEstado({ isCreating: true, error: null });

        const factura = await facturaService.crearFactura(request);

        // Actualizar estado local
        actualizarEstado({
          facturas: [...state.facturas, factura],
          facturasDelDia: [...state.facturasDelDia, factura],
          isCreating: false,
        });

        toast.success(`Factura ${factura.numeroFactura} creada exitosamente`);
        return factura;
      } catch (error: any) {
        manejarError(error, 'crear factura');
        actualizarEstado({ isCreating: false });
        throw error;
      }
    },
    [state.facturas, state.facturasDelDia, manejarError, actualizarEstado]
  );

  const anularFactura = useCallback(
    async (facturaId: number, razon: string): Promise<void> => {
      try {
        actualizarEstado({ isLoading: true, error: null });

        await facturaService.anularFactura(facturaId, razon);

        // Actualizar estado local
        actualizarEstado({
          facturas: state.facturas.map((f) =>
            f.facturaID === facturaId ? { ...f, estado: 'Anulada' as FacturaEstado } : f
          ),
          facturasDelDia: state.facturasDelDia.map((f) =>
            f.facturaID === facturaId ? { ...f, estado: 'Anulada' as FacturaEstado } : f
          ),
          isLoading: false,
        });

        toast.success('Factura anulada exitosamente');
      } catch (error: any) {
        manejarError(error, 'anular factura');
      }
    },
    [state.facturas, state.facturasDelDia, manejarError, actualizarEstado]
  );

  const procesarPago = useCallback(
    async (request: PagoRequest): Promise<PagoResponse> => {
      try {
        actualizarEstado({ isPaying: true, error: null });

        const pagoResponse = await facturaService.procesarPago(request);

        // Actualizar estado local
        actualizarEstado({
          facturas: state.facturas.map((f) =>
            f.facturaID === request.facturaID ? { ...f, estado: 'Pagada' as FacturaEstado } : f
          ),
          isPaying: false,
        });

        toast.success(`Pago procesado exitosamente: ${pagoResponse.metodoPago}`);
        return pagoResponse;
      } catch (error: any) {
        manejarError(error, 'procesar pago');
        actualizarEstado({ isPaying: false });
        throw error;
      }
    },
    [state.facturas, manejarError, actualizarEstado]
  );

  // ============================================================================
  // FUNCIONES DE CONSULTA
  // ============================================================================

  const obtenerFactura = useCallback(
    async (facturaId: number): Promise<Factura> => {
      try {
        actualizarEstado({ isLoading: true, error: null });

        const factura = await facturaService.obtenerFactura(facturaId);

        actualizarEstado({ facturaActual: factura, isLoading: false });
        return factura;
      } catch (error: any) {
        manejarError(error, 'obtener factura');
        throw error;
      }
    },
    [manejarError, actualizarEstado]
  );

  const obtenerFacturasPorOrden = useCallback(
    async (ordenId: number): Promise<Factura[]> => {
      try {
        actualizarEstado({ isLoading: true, error: null });

        // Simulamos obtener facturas por orden usando rango del día
        const hoy = new Date();
        const inicioDelDia = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
        const finDelDia = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate() + 1);

        const todasLasFacturas = await facturaService.obtenerFacturasPorRango(
          inicioDelDia,
          finDelDia
        );
        const facturas = todasLasFacturas.filter((f) => f.ordenID === ordenId);

        actualizarEstado({ isLoading: false });
        return facturas;
      } catch (error: any) {
        manejarError(error, 'obtener facturas por orden');
        return [];
      }
    },
    [manejarError, actualizarEstado]
  );

  const obtenerFacturasDelDia = useCallback(async (): Promise<Factura[]> => {
    try {
      actualizarEstado({ isLoading: true, error: null });

      const facturas = await facturaService.obtenerFacturasDelDia();

      actualizarEstado({ facturasDelDia: facturas, isLoading: false });
      return facturas;
    } catch (error: any) {
      manejarError(error, 'obtener facturas del día');
      return [];
    }
  }, [manejarError, actualizarEstado]);

  const obtenerEstadisticas = useCallback(async (): Promise<EstadisticasFacturacion> => {
    try {
      actualizarEstado({ isLoading: true, error: null });

      // Usar rango del día actual para obtener estadísticas
      const hoy = new Date();
      const inicioDelDia = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
      const finDelDia = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate() + 1);

      const estadisticas = await facturaService.obtenerEstadisticasFacturacion(
        inicioDelDia,
        finDelDia
      );

      actualizarEstado({ estadisticas, isLoading: false });
      return estadisticas;
    } catch (error: any) {
      manejarError(error, 'obtener estadísticas');
      throw error;
    }
  }, [manejarError, actualizarEstado]);

  // ============================================================================
  // FUNCIONES DE UI
  // ============================================================================

  const seleccionarFactura = useCallback(
    (factura: Factura | null) => {
      actualizarEstado({ facturaActual: factura });
    },
    [actualizarEstado]
  );

  const limpiarError = useCallback(() => {
    actualizarEstado({ error: null });
  }, [actualizarEstado]);

  const refrescarDatos = useCallback(async () => {
    try {
      actualizarEstado({ isLoading: true, error: null });

      const [facturas, estadisticas] = await Promise.all([
        obtenerFacturasDelDia(),
        obtenerEstadisticas(),
      ]);

      actualizarEstado({
        facturas,
        estadisticas,
        isLoading: false,
      });
    } catch (error: any) {
      manejarError(error, 'refrescar datos');
    }
  }, [obtenerFacturasDelDia, obtenerEstadisticas, manejarError, actualizarEstado]);

  // ============================================================================
  // UTILIDADES
  // ============================================================================

  const calcularTotalFacturado = useCallback((facturas: Factura[]): number => {
    return facturas.filter((f) => f.estado === 'Pagada').reduce((total, f) => total + f.total, 0);
  }, []);

  const filtrarFacturasPorEstado = useCallback(
    (estado: FacturaEstado): Factura[] => {
      return state.facturas.filter((f) => f.estado === estado);
    },
    [state.facturas]
  );

  const filtrarFacturasPorMetodo = useCallback(
    (metodo: MetodoPago): Factura[] => {
      return state.facturas.filter((f) => f.metodoPago === metodo);
    },
    [state.facturas]
  );

  // ============================================================================
  // VALIDACIONES
  // ============================================================================

  const validarCrearFactura = useCallback((request: CrearFacturaRequest): string | null => {
    if (!request.ordenID) {
      return 'ID de orden es requerido';
    }

    if (!request.metodoPago) {
      return 'Método de pago es requerido';
    }

    if (request.descuento && request.descuento < 0) {
      return 'El descuento no puede ser negativo';
    }

    if (request.propina && request.propina < 0) {
      return 'La propina no puede ser negativa';
    }

    return null;
  }, []);

  // ============================================================================
  // EFECTOS
  // ============================================================================

  // Cargar datos iniciales
  useEffect(() => {
    refrescarDatos();
  }, []);

  // Auto-refresh control
  const autoRefreshControl = useAutoRefresh({
    enabled: autoRefresh,
    interval: refreshInterval,
    onRefresh: refrescarDatos,
  });

  // ============================================================================
  // RETURN
  // ============================================================================

  return {
    state,
    crearFactura,
    anularFactura,
    procesarPago,
    obtenerFactura,
    obtenerFacturasPorOrden,
    obtenerFacturasDelDia,
    obtenerEstadisticas,
    seleccionarFactura,
    limpiarError,
    refrescarDatos,
    calcularTotalFacturado,
    filtrarFacturasPorEstado,
    filtrarFacturasPorMetodo,
    validarCrearFactura,
    autoRefresh: autoRefreshControl,
  };
};
