import React, { useState, useEffect } from 'react';
import { PlusCircle, FileText, X, Loader, Check } from 'lucide-react';
import { toast } from 'react-toastify';

// Components
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui/Card';
import { OrdenCard } from '@/components/ordenes/OrdenCard';
import { CrearOrdenForm } from '@/components/ordenes/CrearOrdenForm';
import { EditarOrdenForm } from '@/components/ordenes/EditarOrdenForm';
import { FacturaFormSimple } from '@/components/facturacion/FacturaFormSimple';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import { AutoRefreshControl } from '@/components/ui/AutoRefreshControl';

// Hooks y servicios
import { ordenesService } from '@/services/ordenesService';
import { useOrdenesContext } from '@/contexts/OrdenesContext';
import { useOrdenesMesa } from '@/hooks/useOrdenesMesa';

// Types
import type { Mesa, Orden } from '@/types';

interface GestionMesaModalProps {
  mesa: Mesa | null;
  onClose: () => void;
  onOrdenChange: () => void;
}

type VistaModal = 'LISTA_ORDENES' | 'CREAR_ORDEN' | 'EDITAR_ORDEN' | 'FACTURAR';

export const GestionMesaModal: React.FC<GestionMesaModalProps> = ({
  mesa,
  onClose,
  onOrdenChange,
}) => {
  if (!mesa) return null;

  const { ordenesActualizadas } = useOrdenesContext();
  const { ordenes, loading, error, refrescar, autoRefresh } = useOrdenesMesa(mesa.mesaID, {
    autoRefresh: true,
    refreshInterval: 30000, // Refrescar cada 30 segundos (reducido para evitar parpadeo)
  });

  const [vista, setVista] = useState<VistaModal>('LISTA_ORDENES');
  const [ordenSeleccionada, setOrdenSeleccionada] = useState<Orden | null>(null);

  const handleCrearNuevaOrden = () => {
    setVista('CREAR_ORDEN');
  };

  const handleEditarOrden = async (orden: Orden) => {
    try {
      const ordenCompleta = await ordenesService.getOrdenById(orden.ordenID);
      setOrdenSeleccionada(ordenCompleta);
      setVista('EDITAR_ORDEN');
    } catch (error) {
      toast.error('No se pudo cargar el detalle de la orden para editar.');
      console.error('Error fetching order details:', error);
    }
  };

  const handleVolverALista = async () => {
    setVista('LISTA_ORDENES');
    setOrdenSeleccionada(null); // Limpiar selección
    refrescar(); // Refrescar órdenes para obtener precios actualizados
    onOrdenChange(); // Notificar cambio para actualizar otros componentes
  };

  const handleFacturarOrden = async (orden: Orden) => {
    try {
      const ordenCompleta = await ordenesService.getOrdenById(orden.ordenID);
      setOrdenSeleccionada(ordenCompleta);
      setVista('FACTURAR');
    } catch (error) {
      toast.error('No se pudo cargar el detalle de la orden para facturar.');
      console.error('Error fetching order details for billing:', error);
    }
  };

  const renderContent = () => {
    switch (vista) {
      case 'CREAR_ORDEN':
        return (
          <CrearOrdenForm
            mesa={mesa}
            onClose={handleVolverALista}
            onOrdenCreada={handleVolverALista}
          />
        );
      case 'EDITAR_ORDEN':
        if (!ordenSeleccionada) return renderVistaLista();
        if (loading)
          return (
            <div className="p-8 flex justify-center">
              <LoadingSpinner />
            </div>
          );
        return (
          <EditarOrdenForm
            orden={ordenSeleccionada}
            onClose={handleVolverALista}
            onOrdenActualizada={handleVolverALista}
          />
        );
      case 'FACTURAR':
        if (!ordenSeleccionada) return renderVistaLista();
        if (loading)
          return (
            <div className="p-8 flex justify-center">
              <LoadingSpinner />
            </div>
          );
        return (
          <FacturaFormSimple
            orden={ordenSeleccionada}
            onFacturaCreada={handleVolverALista}
            onClose={handleVolverALista}
          />
        );
      case 'LISTA_ORDENES':
      default:
        return renderVistaLista();
    }
  };

  const renderVistaLista = () => (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-800">Gestionar Mesa {mesa.numeroMesa}</h2>
        <div className="flex items-center space-x-2">
          <AutoRefreshControl
            isEnabled={autoRefresh.isEnabled}
            isRefreshing={autoRefresh.isRefreshing}
            lastRefresh={autoRefresh.lastRefresh}
            onToggle={autoRefresh.toggleAutoRefresh}
            onRefresh={autoRefresh.refreshNow}
            interval={30000}
            className="bg-gray-50"
          />
          <Button onClick={onClose} variant="ghost" size="sm">
            <X className="w-5 h-5" />
          </Button>
        </div>
      </div>

      <div className="flex justify-end">
        <Button
          onClick={handleCrearNuevaOrden}
          className="bg-dominican-blue hover:bg-dominican-blue-dark"
        >
          <PlusCircle className="w-4 h-4 mr-2" />
          Crear Nueva Orden
        </Button>
      </div>

      <div className="space-y-4">
        <h3 className="font-semibold text-lg">Órdenes Activas</h3>
        {loading ? (
          <div className="flex justify-center p-8">
            <LoadingSpinner />
          </div>
        ) : ordenes.length > 0 ? (
          ordenes.map((orden) => (
            <Card key={orden.ordenID} className="p-4">
              <OrdenCard
                orden={orden}
                showActions={false}
                onEditarOrden={() => handleEditarOrden(orden)}
                onFacturarOrden={() => handleFacturarOrden(orden)}
              />
              <div className="flex justify-end space-x-2 mt-4 pt-4 border-t">
                {orden.estado !== 'Facturada' && (
                  <Button variant="outline" size="sm" onClick={() => handleFacturarOrden(orden)}>
                    <FileText className="w-4 h-4 mr-2" />
                    Facturar
                  </Button>
                )}
                {orden.estado !== 'Facturada' && (
                  <Button size="sm" onClick={() => handleEditarOrden(orden)}>
                    Editar Orden
                  </Button>
                )}
                {orden.estado === 'Facturada' && (
                  <Button size="sm" className="bg-green-600 hover:bg-green-700 text-white" disabled>
                    <Check className="w-4 h-4 mr-2" />
                    Facturada
                  </Button>
                )}
              </div>
            </Card>
          ))
        ) : (
          <p className="text-gray-500 text-center py-4">No hay órdenes activas para esta mesa.</p>
        )}
      </div>
    </div>
  );

  const getModalSize = () => {
    switch (vista) {
      case 'CREAR_ORDEN':
      case 'EDITAR_ORDEN':
        return '4xl';
      case 'FACTURAR':
        return 'xl';
      case 'LISTA_ORDENES':
      default:
        return 'lg';
    }
  };

  return (
    <Modal isOpen={!!mesa} onClose={onClose} size={getModalSize()}>
      {renderContent()}
    </Modal>
  );
};
