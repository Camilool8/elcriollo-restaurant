import React, { useState } from 'react';
import {
  Users,
  Clock,
  MapPin,
  AlertTriangle,
  CheckCircle,
  XCircle,
  Settings,
  RefreshCw,
  Receipt,
  DollarSign,
  Split,
  FileText,
  Eye,
  Wrench,
  ConciergeBell,
} from 'lucide-react';
import { toast } from 'react-toastify';

// Components
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { ActionMenu } from '@/components/ui/ActionMenu';

// Services
import { ordenesService } from '@/services/ordenesService';

// Types
import type { Mesa, EstadoMesa } from '@/types/mesa';
import type { Orden } from '@/types/orden';
import type { ActionMenuItem } from '@/types';

interface MesaCardProps {
  mesa: Mesa;
  onCambiarEstado?: (mesaId: number, nuevoEstado: EstadoMesa, motivo?: string) => Promise<void>;
  onMantenimiento?: (mesaId: number, motivo: string) => Promise<void>;
  onOcupar?: (mesaId: number) => Promise<void>;
  onLiberar?: (mesaId: number) => Promise<void>;
  onGestionarOrden?: (mesa: Mesa) => void;
  onVerFacturas?: (mesa: Mesa) => void;
  className?: string;
}

export const MesaCard: React.FC<MesaCardProps> = ({
  mesa,
  onCambiarEstado,
  onMantenimiento,
  onOcupar,
  onLiberar,
  onGestionarOrden,
  onVerFacturas,
  className = '',
}) => {
  // Estados
  const [loading, setLoading] = useState(false);
  const [mostrarDetalles, setMostrarDetalles] = useState(false);
  const [ordenesActivas, setOrdenesActivas] = useState<Orden[]>([]);
  const [loadingOrdenes, setLoadingOrdenes] = useState(false);

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const handleCargarOrdenes = async () => {
    if (!mesa.mesaID) return;

    try {
      setLoadingOrdenes(true);
      const ordenes = await ordenesService.getOrdenesByMesa(mesa.mesaID);
      setOrdenesActivas(
        ordenes.filter((o: Orden) => o.estado !== 'Entregada' && o.estado !== 'Cancelada')
      );
    } catch (error: any) {
      console.error('Error cargando órdenes:', error);
      toast.error('Error al cargar las órdenes de la mesa');
    } finally {
      setLoadingOrdenes(false);
    }
  };

  const handleVerDetalles = async () => {
    setMostrarDetalles(true);
    await handleCargarOrdenes();
  };

  const handleCambiarEstado = async (nuevoEstado: EstadoMesa, motivo?: string) => {
    if (!onCambiarEstado) return;

    try {
      setLoading(true);
      await onCambiarEstado(mesa.mesaID, nuevoEstado, motivo);
      toast.success(`Mesa ${mesa.numeroMesa} cambiada a ${nuevoEstado}`);
    } catch (error: any) {
      console.error('Error cambiando estado:', error);
      toast.error(error.message || 'Error al cambiar el estado de la mesa');
    } finally {
      setLoading(false);
    }
  };

  const handleMantenimiento = async () => {
    if (!onMantenimiento) return;

    const motivo = prompt('Motivo del mantenimiento:');
    if (!motivo) return;

    try {
      setLoading(true);
      await onMantenimiento(mesa.mesaID, motivo);
      toast.success(`Mesa ${mesa.numeroMesa} marcada para mantenimiento`);
    } catch (error: any) {
      console.error('Error en mantenimiento:', error);
      toast.error(error.message || 'Error al marcar la mesa para mantenimiento');
    } finally {
      setLoading(false);
    }
  };

  const handleOcupar = async () => {
    if (!onOcupar) return;

    try {
      setLoading(true);
      await onOcupar(mesa.mesaID);
      toast.success(`Mesa ${mesa.numeroMesa} ocupada`);
    } catch (error: any) {
      console.error('Error ocupando mesa:', error);
      toast.error(error.message || 'Error al ocupar la mesa');
    } finally {
      setLoading(false);
    }
  };

  const handleLiberar = async () => {
    if (!onLiberar) return;

    const confirmado = confirm(`¿Está seguro que desea liberar la mesa ${mesa.numeroMesa}?`);
    if (!confirmado) return;

    try {
      setLoading(true);
      await onLiberar(mesa.mesaID);
      toast.success(`Mesa ${mesa.numeroMesa} liberada`);
    } catch (error: any) {
      console.error('Error liberando mesa:', error);
      toast.error(error.message || 'Error al liberar la mesa');
    } finally {
      setLoading(false);
    }
  };

  // ============================================================================
  // LÓGICA DE ACCIONES DINÁMICAS
  // ============================================================================

  const getAccionPrincipal = () => {
    switch (mesa.estado) {
      case 'Libre':
      case 'Reservada':
        return {
          label: 'Ocupar / Gestionar',
          onClick: () => onGestionarOrden?.(mesa),
          icon: <ConciergeBell className="w-4 h-4 mr-2" />,
        };
      case 'Ocupada':
        return {
          label: 'Gestionar Órdenes',
          onClick: () => onGestionarOrden?.(mesa),
          icon: <Receipt className="w-4 h-4 mr-2" />,
        };
      case 'Mantenimiento':
        return {
          label: 'Quitar Mantenimiento',
          onClick: () => handleCambiarEstado('Libre'),
          icon: <Wrench className="w-4 h-4 mr-2" />,
          variant: 'default',
        };
      default:
        return null;
    }
  };

  const getAccionesSecundarias = (): ActionMenuItem[] => {
    const items: ActionMenuItem[] = [];

    if (mesa.estado === 'Ocupada') {
      items.push({
        label: 'Ver Facturas',
        icon: <Eye className="w-4 h-4" />,
        onClick: () => onVerFacturas?.(mesa),
      });
      items.push({
        label: 'Liberar Mesa',
        icon: <XCircle className="w-4 h-4" />,
        onClick: handleLiberar,
        variant: 'danger',
      });
    }

    if (mesa.estado !== 'Mantenimiento') {
      items.push({
        label: 'Poner en Mantenimiento',
        icon: <Settings className="w-4 h-4" />,
        onClick: handleMantenimiento,
        variant: 'warning',
      });
    }

    return items;
  };

  const accionPrincipal = getAccionPrincipal();
  const accionesSecundarias = getAccionesSecundarias();

  // ============================================================================
  // FUNCIONES AUXILIARES
  // ============================================================================

  const obtenerColorEstado = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return 'bg-green-100 border-green-200 text-green-800';
      case 'Ocupada':
        return 'bg-blue-100 border-blue-200 text-blue-800';
      case 'Reservada':
        return 'bg-amber-100 border-amber-200 text-amber-800';
      case 'Mantenimiento':
        return 'bg-red-100 border-red-200 text-red-800';
      default:
        return 'bg-gray-100 border-gray-200 text-gray-800';
    }
  };

  const obtenerIconoEstado = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return <CheckCircle className="w-4 h-4" />;
      case 'Ocupada':
        return <Users className="w-4 h-4" />;
      case 'Reservada':
        return <Clock className="w-4 h-4" />;
      case 'Mantenimiento':
        return <AlertTriangle className="w-4 h-4" />;
      default:
        return <XCircle className="w-4 h-4" />;
    }
  };

  const puedeFacturar = () => {
    return mesa.estado === 'Ocupada' && mesa.ordenActual;
  };

  const puedeAbrirOrden = () => {
    return mesa.estado === 'Libre' || mesa.estado === 'Reservada';
  };

  // ============================================================================
  // RENDER
  // ============================================================================

  return (
    <>
      <Card
        className={`flex flex-col justify-between h-full group ${className} ${
          loading ? 'opacity-70 pointer-events-none' : ''
        }`}
      >
        <div className="p-4">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center space-x-2">
              <div className="text-xl font-bold text-dominican-blue">Mesa {mesa.numeroMesa}</div>
              <Badge className={`${obtenerColorEstado(mesa.estado)} text-xs`}>
                {obtenerIconoEstado(mesa.estado)}
                <span className="ml-1">{mesa.estado}</span>
              </Badge>
            </div>
          </div>

          <div className="text-sm text-gray-600 space-y-3">
            {/* Descripción o estado actual */}
            <p className="line-clamp-2">{mesa.descripcion || 'Sin descripción adicional.'}</p>

            {/* Detalles rápidos */}
            <div className="flex items-center justify-between text-xs text-gray-500">
              <div className="flex items-center">
                <Users className="w-3 h-3 mr-1" />
                <span>Capacidad: {mesa.capacidad}</span>
              </div>
              <div className="flex items-center">
                <MapPin className="w-3 h-3 mr-1" />
                <span>{mesa.ubicacion}</span>
              </div>
            </div>
          </div>
        </div>

        <div className="p-4 border-t border-gray-100 flex items-center justify-between">
          {accionPrincipal && (
            <Button
              size="sm"
              onClick={accionPrincipal.onClick}
              variant={accionPrincipal.variant as any}
              className="flex-grow"
              disabled={loading}
            >
              {accionPrincipal.icon}
              {accionPrincipal.label}
            </Button>
          )}
          {accionesSecundarias.length > 0 && (
            <div className="ml-2">
              <ActionMenu items={accionesSecundarias} />
            </div>
          )}
        </div>
      </Card>
    </>
  );
};
