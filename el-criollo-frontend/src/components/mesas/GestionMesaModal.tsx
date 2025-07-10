import React, { useState, useEffect } from 'react';
import { PlusCircle, FileText, Check } from 'lucide-react';
import { showErrorToast } from '@/utils/toastUtils';

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

  const { ordenes, loading, refrescar } = useOrdenesMesa(mesa.mesaID, {
    autoRefresh: false, // Desactivar auto-refresh para evitar múltiples llamadas
    refreshInterval: 30000,
  });

  const [vista, setVista] = useState<VistaModal>('LISTA_ORDENES');
  const [ordenSeleccionada, setOrdenSeleccionada] = useState<Orden | null>(null);
  const [ordenActualizada, setOrdenActualizada] = useState<Orden | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const handleCrearNuevaOrden = () => {
    setVista('CREAR_ORDEN');
  };

  const handleEditarOrden = async (orden: Orden) => {
    try {
      const ordenCompleta = await ordenesService.getOrdenById(orden.ordenID);
      setOrdenSeleccionada(ordenCompleta);
      setVista('EDITAR_ORDEN');
    } catch (error) {
      showErrorToast('No se pudo cargar el detalle de la orden para editar.');
      console.error('Error fetching order details:', error);
    }
  };

  const handleVolverALista = async () => {
    console.log('🔄 Volviendo a la lista de órdenes');
    setVista('LISTA_ORDENES');
    setOrdenSeleccionada(null); // Limpiar selección
    setOrdenActualizada(null); // Limpiar orden actualizada

    // Solo refrescar si no está ya refrescando
    if (!isRefreshing && !loading) {
      setIsRefreshing(true);
      try {
        await refrescar();
      } finally {
        setIsRefreshing(false);
      }
    }

    onOrdenChange(); // Notificar cambio para actualizar otros componentes
  };

  // Efecto para limpiar la orden actualizada cuando cambia la vista
  useEffect(() => {
    if (vista !== 'EDITAR_ORDEN') {
      setOrdenActualizada(null);
    }
  }, [vista]);

  const handleOrdenActualizada = async (orden: Orden) => {
    console.log('🔄 Orden actualizada recibida:', orden.ordenID);

    // Guardar la orden actualizada para mostrarla inmediatamente
    setOrdenActualizada(orden);

    // Refrescar los datos solo si no está ya refrescando
    if (!isRefreshing && !loading) {
      setIsRefreshing(true);
      try {
        await refrescar();
      } finally {
        setIsRefreshing(false);
      }
    }

    // Volver a la lista después de un pequeño delay
    setTimeout(() => {
      console.log('🔄 Volviendo a la lista después de actualizar orden');
      handleVolverALista();
    }, 200); // Reducido de 300ms a 200ms
  };

  const handleFacturarOrden = async (orden: Orden) => {
    try {
      const ordenCompleta = await ordenesService.getOrdenById(orden.ordenID);
      setOrdenSeleccionada(ordenCompleta);
      setVista('FACTURAR');
    } catch (error) {
      showErrorToast('No se pudo cargar el detalle de la orden para facturar.');
      console.error('Error fetching order details for billing:', error);
    }
  };

  // Función para obtener la orden más actualizada
  const getOrdenActualizada = (orden: Orden): Orden => {
    // Si hay una orden actualizada para este ID, usarla
    if (ordenActualizada && ordenActualizada.ordenID === orden.ordenID) {
      console.log('🔄 Usando orden actualizada local:', ordenActualizada.ordenID);
      return ordenActualizada;
    }

    // Buscar en las órdenes actuales (que pueden estar más actualizadas)
    const ordenActual = ordenes.find((o) => o.ordenID === orden.ordenID);
    if (ordenActual) {
      console.log('🔄 Usando orden actualizada del servidor:', ordenActual.ordenID);
      return ordenActual;
    }

    // Si no se encuentra, usar la orden original pero con totales recalculados
    console.log('🔄 Recalculando totales para orden:', orden.ordenID);

    // Calcular la suma real de los items (subtotal sin ITBIS)
    let subtotalSinITBIS = 0;
    if (orden.detalles && orden.detalles.length > 0) {
      subtotalSinITBIS = orden.detalles.reduce((acc, detalle) => {
        const subtotal =
          detalle.subtotalNumerico ||
          (typeof detalle.subtotal === 'string'
            ? parseFloat(detalle.subtotal.replace(/[^\d.-]/g, ''))
            : 0);
        return acc + subtotal;
      }, 0);
    }

    // Calcular el total con ITBIS (18%)
    const totalConITBIS = subtotalSinITBIS * 1.18;

    // Usar el total del servidor solo si es mayor o igual al total calculado
    const totalCalculado =
      orden.totalCalculado && orden.totalCalculado >= totalConITBIS
        ? orden.totalCalculado
        : totalConITBIS;

    const subtotalCalculado = totalCalculado / 1.18; // Sin ITBIS
    const totalItems = orden.detalles?.reduce((acc, detalle) => acc + detalle.cantidad, 0) || 0;

    if (orden.totalCalculado && orden.totalCalculado < totalConITBIS) {
      console.log(
        `⚠️ Modal: Total del servidor (${orden.totalCalculado}) es menor que total calculado con ITBIS (${totalConITBIS}). Usando total calculado.`
      );
    }

    return {
      ...orden,
      totalCalculado,
      subtotalCalculado,
      totalItems,
    };
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
            onOrdenActualizada={handleOrdenActualizada}
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
        <div className="flex items-center space-x-2"></div>
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
        {loading || isRefreshing ? (
          <div className="flex justify-center p-8">
            <LoadingSpinner />
          </div>
        ) : ordenes.length > 0 ? (
          ordenes.map((orden) => {
            // Usar la orden más actualizada disponible
            const ordenActualizada = getOrdenActualizada(orden);
            return (
              <Card key={orden.ordenID} className="p-4">
                <OrdenCard
                  orden={ordenActualizada}
                  showActions={false}
                  onEditarOrden={() => handleEditarOrden(ordenActualizada)}
                  onFacturarOrden={() => handleFacturarOrden(ordenActualizada)}
                />
                <div className="flex justify-end space-x-2 mt-4 pt-4 border-t">
                  {ordenActualizada.estado !== 'Facturada' && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleFacturarOrden(ordenActualizada)}
                    >
                      <FileText className="w-4 h-4 mr-2" />
                      Facturar
                    </Button>
                  )}
                  {ordenActualizada.estado !== 'Facturada' && (
                    <Button size="sm" onClick={() => handleEditarOrden(ordenActualizada)}>
                      Editar Orden
                    </Button>
                  )}
                  {ordenActualizada.estado === 'Facturada' && (
                    <Button
                      size="sm"
                      className="bg-green-600 hover:bg-green-700 text-white"
                      disabled
                    >
                      <Check className="w-4 h-4 mr-2" />
                      Facturada
                    </Button>
                  )}
                </div>
              </Card>
            );
          })
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
