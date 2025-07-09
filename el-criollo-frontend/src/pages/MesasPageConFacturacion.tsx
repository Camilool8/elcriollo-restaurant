import React, { useState, useMemo, useEffect } from 'react';
import { AlertTriangle, Clock, MapPin, Receipt } from 'lucide-react';

// Components
import { MesaCard } from '@/components/mesas/MesaCard';
import { MesaFilters } from '@/components/mesas/MesaFilters';
import { Card, Button, Badge } from '@/components';
import { Modal } from '@/components/ui/Modal';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import { GestionMesaModal } from '@/components/mesas/GestionMesaModal';
import { CrearReservacionModal } from '@/components/mesas/CrearReservacionModal';
import { UnifiedAutoRefreshControl } from '@/components/ui/UnifiedAutoRefreshControl';

// Components de facturación y órdenes
import { FacturaForm, ResumenFactura } from '@/components/facturacion';

// Hooks
import { useMesas } from '@/hooks/useMesas';
import { useFacturacion } from '@/hooks/useFacturacion';

// Types
import type { FiltrosMesa, Mesa, EstadoMesa } from '@/types/mesa';
import type { Orden, Cliente } from '@/types';
import { clienteService } from '@/services/clienteService';

// Toast notifications
import { toast } from 'react-toastify';

export const MesasPageConFacturacion: React.FC = () => {
  // Estados básicos
  const [filtros, setFiltros] = useState<FiltrosMesa>({});
  const [mesaSeleccionada, setMesaSeleccionada] = useState<Mesa | null>(null);
  const [ordenActiva, setOrdenActiva] = useState<Orden | null>(null);
  const [loadingOrden, setLoadingOrden] = useState(false);
  const [clientes, setClientes] = useState<Cliente[]>([]);

  // Estados de modales
  const [isVerFacturasModalOpen, setIsVerFacturasModalOpen] = useState(false);
  const [isCrearFacturaModalOpen, setIsCrearFacturaModalOpen] = useState(false);
  const [isCrearReservacionModalOpen, setIsCrearReservacionModalOpen] = useState(false);
  const [mesaParaReservar, setMesaParaReservar] = useState<Mesa | null>(null);

  // Hooks
  const {
    mesas,
    loading,
    error,
    refrescar,
    liberarMesa,
    ocuparMesa,
    cambiarEstadoMesa,
    marcarMantenimiento,
    mesasQueNecesitanAtencion,
    autoRefresh,
  } = useMesas({ autoRefresh: true, refreshInterval: 30000 });

  const {
    state: { facturasDelDia, facturaActual, error: errorFacturas },
    obtenerFacturasPorOrden,
    seleccionarFactura,
    limpiarError,
    autoRefresh: facturacionAutoRefresh,
  } = useFacturacion({ autoRefresh: true, refreshInterval: 30000 });

  useEffect(() => {
    const fetchClientes = async () => {
      try {
        const data = await clienteService.getClientes();
        setClientes(data);
      } catch (error) {
        console.error('Error fetching clients', error);
        toast.error('No se pudieron cargar los clientes.');
      }
    };

    fetchClientes();
  }, []);

  // Obtener mesas que necesitan atención (función llamada)
  const mesasConAtencion = useMemo(() => {
    return mesasQueNecesitanAtencion();
  }, [mesasQueNecesitanAtencion]);

  // ============================================================================
  // FUNCIONES DE FILTRADO
  // ============================================================================

  // Filtrar mesas según los filtros aplicados
  const mesasFiltradas = useMemo(() => {
    let resultado = [...mesas];

    if (filtros.estado) {
      resultado = resultado.filter((mesa) => mesa.estado === filtros.estado);
    }

    if (filtros.capacidadMinima) {
      resultado = resultado.filter((mesa) => mesa.capacidad >= filtros.capacidadMinima!);
    }

    if (filtros.capacidadMaxima) {
      resultado = resultado.filter((mesa) => mesa.capacidad <= filtros.capacidadMaxima!);
    }

    if (filtros.ubicacion) {
      resultado = resultado.filter((mesa) => mesa.ubicacion === filtros.ubicacion);
    }

    if (filtros.soloDisponibles) {
      resultado = resultado.filter((mesa) => mesa.estado === 'Libre');
    }

    return resultado;
  }, [mesas, filtros]);

  // Agrupar mesas por ubicación para mejor organización
  const mesasPorUbicacion = useMemo(() => {
    const grupos: Record<string, Mesa[]> = {};

    mesasFiltradas.forEach((mesa) => {
      const ubicacion = mesa.ubicacion || 'Sin ubicación';
      if (!grupos[ubicacion]) {
        grupos[ubicacion] = [];
      }
      grupos[ubicacion].push(mesa);
    });

    // Ordenar mesas por número dentro de cada ubicación
    Object.keys(grupos).forEach((ubicacion) => {
      grupos[ubicacion].sort((a, b) => a.numeroMesa - b.numeroMesa);
    });

    return grupos;
  }, [mesasFiltradas]);

  // ============================================================================
  // HANDLERS DE MESAS Y ÓRDENES
  // ============================================================================

  const handleMantenimiento = async (mesaId: number, motivo: string) => {
    await marcarMantenimiento(mesaId, { motivo });
  };

  const handleCambiarEstado = async (mesaId: number, nuevoEstado: EstadoMesa, motivo?: string) => {
    await cambiarEstadoMesa(mesaId, { nuevoEstado, motivo });
  };

  const handleLiberarMesa = async (mesaId: number) => {
    await liberarMesa(mesaId);
  };

  const handleOcuparMesa = async (mesaId: number) => {
    await ocuparMesa(mesaId);
  };

  const handleMesaClick = (mesa: Mesa) => {
    if (mesa.estado === 'Ocupada' || mesa.estado === 'Libre' || mesa.estado === 'Reservada') {
      setMesaSeleccionada(mesa);
    } else {
      toast.info(`La mesa está en estado de ${mesa.estado} y no puede ser gestionada.`);
    }
  };

  const handleReservarMesa = (mesa: Mesa) => {
    if (mesa.estado !== 'Libre') {
      toast.warning('Solo se pueden reservar mesas libres');
      return;
    }
    setMesaParaReservar(mesa);
    setIsCrearReservacionModalOpen(true);
  };

  const handleReservacionCreada = () => {
    refrescar();
    toast.success('Reservación creada exitosamente');
  };

  // ============================================================================
  // HANDLERS DE FACTURACIÓN
  // ============================================================================

  const handleVerFacturas = async (mesa: Mesa) => {
    if (!mesa.ordenActual) {
      toast.warning('Esta mesa no tiene una orden activa');
      return;
    }

    try {
      const facturas = await obtenerFacturasPorOrden(mesa.ordenActual.ordenID);

      if (facturas.length === 0) {
        toast.info('No hay facturas para esta mesa');
        return;
      }

      seleccionarFactura(facturas[0]);
      setMesaSeleccionada(mesa);
      setIsVerFacturasModalOpen(true);
    } catch (error) {
      console.error('Error obteniendo facturas:', error);
      toast.error('Error al obtener las facturas de la mesa');
    }
  };

  // ============================================================================
  // UTILIDADES
  // ============================================================================

  const cerrarModales = () => {
    setMesaSeleccionada(null);
    setIsVerFacturasModalOpen(false);
    setIsCrearFacturaModalOpen(false);
    setIsCrearReservacionModalOpen(false);
    setMesaParaReservar(null);
  };

  const renderFacturacionModals = () => {
    if (!mesaSeleccionada) return null;

    return (
      <>
        {isCrearFacturaModalOpen && mesaSeleccionada && (
          <Modal
            isOpen={isCrearFacturaModalOpen}
            onClose={cerrarModales}
            title={`Crear Factura - Mesa ${mesaSeleccionada.numeroMesa}`}
            size="xl"
          >
            {loadingOrden ? (
              <div className="flex justify-center items-center p-8">
                <LoadingSpinner />
              </div>
            ) : ordenActiva ? (
              <FacturaForm
                orden={ordenActiva}
                onFacturaCreada={() => {
                  cerrarModales();
                  refrescar();
                }}
                onClose={cerrarModales}
              />
            ) : (
              <div className="p-8 text-center">
                <AlertTriangle className="mx-auto h-12 w-12 text-gray-400" />
                <h3 className="mt-2 text-sm font-medium text-gray-900">
                  No se pudo cargar la orden
                </h3>
                <p className="mt-1 text-sm text-gray-500">
                  Intente cerrar esta ventana y volver a abrirla.
                </p>
              </div>
            )}
          </Modal>
        )}

        {isVerFacturasModalOpen && mesaSeleccionada && facturaActual && (
          <Modal
            isOpen={isVerFacturasModalOpen}
            onClose={cerrarModales}
            title={`Facturas - Mesa ${mesaSeleccionada.numeroMesa}`}
            size="xl"
          >
            <ResumenFactura factura={facturaActual} onClose={cerrarModales} />
          </Modal>
        )}
      </>
    );
  };

  // ============================================================================
  // LOADING & ERROR STATES
  // ============================================================================

  if (loading && mesas.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <LoadingSpinner size="lg" />
        <span className="ml-3 text-gray-600">Cargando estado de mesas...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Card className="text-center p-8 max-w-md">
          <AlertTriangle className="w-12 h-12 text-red-500 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Error al cargar mesas</h3>
          <p className="text-gray-600 mb-4">{error}</p>
          <Button onClick={refrescar} variant="primary">
            Reintentar
          </Button>
        </Card>
      </div>
    );
  }

  // ============================================================================
  // RENDERIZADO PRINCIPAL
  // ============================================================================

  return (
    <div className="p-4 md:p-6 lg:p-8 space-y-6">
      {/* Header y estadísticas */}
      <header className="flex flex-wrap justify-between items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-800">Salón de Mesas</h1>
          <p className="text-gray-600">Gestión de mesas, órdenes y facturación en tiempo real.</p>
        </div>
        <div className="flex items-center space-x-4">
          <UnifiedAutoRefreshControl
            controls={[
              {
                isEnabled: autoRefresh.isEnabled,
                isRefreshing: autoRefresh.isRefreshing,
                lastRefresh: autoRefresh.lastRefresh,
                onToggle: autoRefresh.toggleAutoRefresh,
                onRefresh: autoRefresh.refreshNow,
                label: 'Mesas',
              },
              {
                isEnabled: facturacionAutoRefresh.isEnabled,
                isRefreshing: facturacionAutoRefresh.isRefreshing,
                lastRefresh: facturacionAutoRefresh.lastRefresh,
                onToggle: facturacionAutoRefresh.toggleAutoRefresh,
                onRefresh: facturacionAutoRefresh.refreshNow,
                label: 'Facturación',
              },
            ]}
            interval={30000}
            className="bg-white border border-gray-200"
          />
          <Badge variant={facturasDelDia.length > 0 ? 'success' : 'secondary'}>
            <Receipt className="w-4 h-4 mr-2" />
            {facturasDelDia.length} Facturas Hoy
          </Badge>
        </div>
      </header>

      {/* Alertas de atención */}
      {mesasConAtencion.length > 0 && (
        <Card className="bg-amber-50 border-amber-300 p-4">
          <h3 className="font-bold text-amber-800 flex items-center mb-2">
            <AlertTriangle className="w-5 h-5 mr-2" />
            Mesas que requieren atención
          </h3>
          <div className="flex flex-wrap gap-2">
            {mesasConAtencion.map((m) => (
              <Badge key={m.mesaID} variant="warning" className="cursor-pointer">
                Mesa {m.numeroMesa}
              </Badge>
            ))}
          </div>
        </Card>
      )}

      {/* Filtros */}
      <MesaFilters
        filtros={filtros}
        onFiltrosChange={setFiltros}
        onRefresh={refrescar}
        loading={loading}
        totalMesas={mesas.length}
        mesasFiltradas={mesasFiltradas.length}
      />

      {/* Grid de mesas por ubicación */}
      <div className="space-y-8">
        {Object.keys(mesasPorUbicacion).map((ubicacion) => (
          <div key={ubicacion}>
            <h2 className="text-xl font-bold text-gray-700 mb-4 flex items-center">
              <MapPin className="w-5 h-5 mr-2 text-dominican-blue" />
              {ubicacion}
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {mesasPorUbicacion[ubicacion].map((mesa) => (
                <MesaCard
                  key={mesa.mesaID}
                  mesa={mesa}
                  onGestionarOrden={() => handleMesaClick(mesa)}
                  onVerFacturas={() => handleVerFacturas(mesa)}
                  onReservar={() => handleReservarMesa(mesa)}
                  onLiberar={() => handleLiberarMesa(mesa.mesaID)}
                  onOcupar={() => handleOcuparMesa(mesa.mesaID)}
                  onCambiarEstado={(mesaId, nuevoEstado, motivo) =>
                    handleCambiarEstado(mesaId, nuevoEstado, motivo)
                  }
                  onMantenimiento={(mesaId, motivo) => handleMantenimiento(mesaId, motivo)}
                />
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* Modales */}
      {renderFacturacionModals()}

      {/* Modal de crear reservación */}
      {mesaParaReservar && (
        <CrearReservacionModal
          mesa={mesaParaReservar}
          isOpen={isCrearReservacionModalOpen}
          onClose={() => {
            setIsCrearReservacionModalOpen(false);
            setMesaParaReservar(null);
          }}
          onReservacionCreada={handleReservacionCreada}
        />
      )}

      {/* Nuevo Modal de Gestión de Mesa */}
      {mesaSeleccionada && (
        <GestionMesaModal
          mesa={mesaSeleccionada}
          onClose={cerrarModales}
          onOrdenChange={() => refrescar()}
        />
      )}

      {/* Información adicional */}
      <Card className="bg-gradient-to-r from-dominican-blue to-dominican-blue text-white">
        <div className="flex items-center justify-between">
          <div>
            <h4 className="font-heading font-semibold text-lg mb-2">
              🇩🇴 El Criollo - Sistema Integrado de Mesas y Facturación
            </h4>
            <p className="text-blue-100">
              Gestión completa desde la mesa hasta la facturación. Los datos se actualizan
              automáticamente cada 30 segundos.
            </p>
          </div>
          <div className="text-right text-blue-100">
            <div className="flex items-center justify-end space-x-1 mb-1">
              <Clock className="w-4 h-4" />
              <span className="text-sm">
                Última actualización: {new Date().toLocaleTimeString('es-DO')}
              </span>
            </div>
            <div className="flex items-center justify-end space-x-1">
              <Receipt className="w-4 h-4" />
              <span className="text-sm">Facturas del día: {facturasDelDia.length}</span>
            </div>
          </div>
        </div>
      </Card>

      {/* Error Display */}
      {(errorFacturas || error) && (
        <div className="fixed bottom-4 right-4 max-w-md">
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded shadow-lg">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <AlertTriangle className="w-5 h-5" />
                <span className="font-medium">Error</span>
              </div>
              <button onClick={limpiarError} className="text-red-700 hover:text-red-900">
                ×
              </button>
            </div>
            <p className="mt-1 text-sm">{errorFacturas || error}</p>
          </div>
        </div>
      )}
    </div>
  );
};
