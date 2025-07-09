import React, { useState, useEffect } from 'react';
import { PlusCircle, FileText, X, Loader } from 'lucide-react';
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

// Hooks y servicios
import { ordenesService } from '@/services/ordenesService';

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

  const [vista, setVista] = useState<VistaModal>('LISTA_ORDENES');
  const [ordenes, setOrdenes] = useState<Orden[]>([]);
  const [ordenSeleccionada, setOrdenSeleccionada] = useState<Orden | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchOrdenes = async () => {
    try {
      setLoading(true);
      const data = await ordenesService.getOrdenesByMesa(mesa.mesaID);
      setOrdenes(data.filter((o) => o.estado !== 'Entregada' && o.estado !== 'Cancelada'));
    } catch (error) {
      console.error(`Error fetching orders for table ${mesa.mesaID}:`, error);
      toast.error('No se pudo cargar las órdenes de la mesa.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (mesa) {
      fetchOrdenes();
    }
  }, [mesa]);

  const handleCrearNuevaOrden = () => {
    setVista('CREAR_ORDEN');
  };

  const handleEditarOrden = async (orden: Orden) => {
    try {
      setLoading(true);
      const ordenCompleta = await ordenesService.getOrdenById(orden.ordenID);
      setOrdenSeleccionada(ordenCompleta);
      setVista('EDITAR_ORDEN');
    } catch (error) {
      toast.error('No se pudo cargar el detalle de la orden para editar.');
      console.error('Error fetching order details:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleVolverALista = () => {
    setVista('LISTA_ORDENES');
    setOrdenSeleccionada(null); // Limpiar selección
    fetchOrdenes();
    onOrdenChange();
  };

  const handleFacturarOrden = async (orden: Orden) => {
    try {
      setLoading(true);
      const ordenCompleta = await ordenesService.getOrdenById(orden.ordenID);
      setOrdenSeleccionada(ordenCompleta);
      setVista('FACTURAR');
    } catch (error) {
      toast.error('No se pudo cargar el detalle de la orden para facturar.');
      console.error('Error fetching order details for billing:', error);
    } finally {
      setLoading(false);
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
        <Button onClick={onClose} variant="ghost" size="sm">
          <X className="w-5 h-5" />
        </Button>
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
                <Button variant="outline" size="sm" onClick={() => handleFacturarOrden(orden)}>
                  <FileText className="w-4 h-4 mr-2" />
                  Facturar
                </Button>
                <Button size="sm" onClick={() => handleEditarOrden(orden)}>
                  Editar Orden
                </Button>
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
