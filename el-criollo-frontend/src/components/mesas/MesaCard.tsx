import React, { useState } from 'react';
import {
  Users,
  Clock,
  MapPin,
  AlertTriangle,
  CheckCircle,
  XCircle,
  Settings,
  Receipt,
  Eye,
  Wrench,
  ConciergeBell,
  Bug,
  Calendar,
  Star,
} from 'lucide-react';
import { toast } from 'react-toastify';

// Components
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { ActionMenu } from '@/components/ui/ActionMenu';

// Services
import { ordenesService } from '@/services/ordenesService';
import { mesasService } from '@/services/mesasService';

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
  onReservar?: (mesa: Mesa) => void;
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
  onReservar,
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
        return {
          label: 'Ocupar Mesa',
          onClick: () => onGestionarOrden?.(mesa),
          icon: <ConciergeBell className="w-4 h-4 mr-2" />,
          variant: 'primary' as const,
        };
      case 'Reservada':
        return {
          label: 'Confirmar Llegada',
          onClick: () => onGestionarOrden?.(mesa),
          icon: <Calendar className="w-4 h-4 mr-2" />,
          variant: 'primary' as const,
        };
      case 'Ocupada':
        return {
          label: 'Gestionar Órdenes',
          onClick: () => onGestionarOrden?.(mesa),
          icon: <Receipt className="w-4 h-4 mr-2" />,
          variant: 'secondary' as const,
        };
      case 'Mantenimiento':
        return {
          label: 'Quitar Mantenimiento',
          onClick: () => handleCambiarEstado('Libre'),
          icon: <Wrench className="w-4 h-4 mr-2" />,
          variant: 'outline' as const,
        };
      default:
        return null;
    }
  };

  const getAccionesSecundarias = (): ActionMenuItem[] => {
    const items: ActionMenuItem[] = [];

    if (mesa.estado === 'Libre') {
      items.push({
        label: 'Reservar Mesa',
        icon: <Calendar className="w-4 h-4" />,
        onClick: () => onReservar?.(mesa),
        variant: 'default',
      });
    }

    if (mesa.estado === 'Ocupada') {
      items.push({
        label: 'Ver Facturas',
        icon: <Eye className="w-4 h-4" />,
        onClick: () => onVerFacturas?.(mesa),
        variant: 'default',
      });
      items.push({
        label: 'Liberar Mesa',
        icon: <XCircle className="w-4 h-4" />,
        onClick: handleLiberar,
        variant: 'danger',
      });
    }

    if (mesa.estado === 'Reservada') {
      items.push({
        label: 'Cancelar Reserva',
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

    // Botón de debug para desarrollo
    if (import.meta.env.DEV) {
      items.push({
        label: 'Debug Estado',
        icon: <Bug className="w-4 h-4" />,
        onClick: async () => {
          try {
            const estadoDetallado = await mesasService.getEstadoDetallado(mesa.mesaID);
            console.log('Estado detallado de la mesa:', estadoDetallado);
            alert(
              `Estado detallado:\nPuede liberarse: ${estadoDetallado.puedeLiberarse}\nVer consola para más detalles`
            );
          } catch (error) {
            console.error('Error obteniendo estado detallado:', error);
            alert('Error obteniendo estado detallado');
          }
        },
        variant: 'default',
      });
    }

    return items;
  };

  // ============================================================================
  // UTILIDADES DE ESTILO
  // ============================================================================

  const obtenerColorEstado = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return 'bg-gradient-to-r from-palm-green-500 to-palm-green-600 text-white border-palm-green-600 shadow-sm';
      case 'Ocupada':
        return 'bg-gradient-to-r from-dominican-red-500 to-red-600 text-white border-dominican-red-600 shadow-sm';
      case 'Reservada':
        return 'bg-gradient-to-r from-dominican-blue-500 to-blue-600 text-white border-dominican-blue-600 shadow-sm';
      case 'Mantenimiento':
        return 'bg-gradient-to-r from-caribbean-gold to-sunset-orange text-white border-orange-500 shadow-sm';
      default:
        return 'bg-gradient-to-r from-stone-gray to-gray-600 text-white border-gray-600 shadow-sm';
    }
  };

  const obtenerIconoEstado = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return <CheckCircle className="w-4 h-4" />;
      case 'Ocupada':
        return <XCircle className="w-4 h-4" />;
      case 'Reservada':
        return <Calendar className="w-4 h-4" />;
      case 'Mantenimiento':
        return <Settings className="w-4 h-4" />;
      default:
        return <AlertTriangle className="w-4 h-4" />;
    }
  };

  const obtenerColorFondo = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return 'bg-gradient-to-br from-palm-green-50 via-green-50 to-white border-palm-green-200 hover:border-palm-green-300';
      case 'Ocupada':
        return 'bg-gradient-to-br from-dominican-red-50 via-red-50 to-white border-dominican-red-200 hover:border-dominican-red-300';
      case 'Reservada':
        return 'bg-gradient-to-br from-dominican-blue-50 via-blue-50 to-white border-dominican-blue-200 hover:border-dominican-blue-300';
      case 'Mantenimiento':
        return 'bg-gradient-to-br from-caribbean-gold-50 via-yellow-50 to-white border-caribbean-gold-200 hover:border-caribbean-gold-300';
      default:
        return 'bg-gradient-to-br from-gray-50 to-white border-gray-200 hover:border-gray-300';
    }
  };

  const obtenerColorAccent = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return 'text-palm-green-700 bg-palm-green-100';
      case 'Ocupada':
        return 'text-dominican-red-700 bg-dominican-red-100';
      case 'Reservada':
        return 'text-dominican-blue-700 bg-dominican-blue-100';
      case 'Mantenimiento':
        return 'text-orange-700 bg-orange-100';
      default:
        return 'text-gray-700 bg-gray-100';
    }
  };

  const obtenerColorBoton = (estado: EstadoMesa) => {
    switch (estado) {
      case 'Libre':
        return 'bg-gradient-to-r from-palm-green-500 to-palm-green-600 hover:from-palm-green-600 hover:to-palm-green-700 text-white';
      case 'Ocupada':
        return 'bg-gradient-to-r from-dominican-red-500 to-red-600 hover:from-red-600 hover:to-red-700 text-white';
      case 'Reservada':
        return 'bg-gradient-to-r from-dominican-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white';
      case 'Mantenimiento':
        return 'bg-gradient-to-r from-caribbean-gold to-sunset-orange hover:from-orange-500 hover:to-orange-600 text-white';
      default:
        return 'bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white';
    }
  };

  const accionPrincipal = getAccionPrincipal();
  const accionesSecundarias = getAccionesSecundarias();

  return (
    <Card
      className={`relative overflow-hidden transition-all duration-300 hover:shadow-lg hover:scale-[1.02] ${obtenerColorFondo(mesa.estado)} ${className}`}
    >
      {/* Header con número de mesa y estado */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center space-x-3">
          <div className="flex items-center justify-center w-12 h-12 bg-gradient-to-br from-dominican-blue to-blue-600 rounded-full shadow-lg border-2 border-white">
            <span className="text-lg font-bold text-white">{mesa.numeroMesa}</span>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Mesa {mesa.numeroMesa}</h3>
            <Badge
              className={`${obtenerColorEstado(mesa.estado)} flex items-center space-x-1 font-medium px-3 py-1`}
            >
              {obtenerIconoEstado(mesa.estado)}
              <span className="font-semibold">{mesa.estado}</span>
            </Badge>
          </div>
        </div>

        {/* Indicador de prioridad para mesas ocupadas */}
        {mesa.estado === 'Ocupada' && (
          <div className="flex items-center space-x-1 text-dominican-red-600 bg-dominican-red-50 px-2 py-1 rounded-full">
            <Star className="w-4 h-4 fill-current" />
            <span className="text-xs font-semibold">Ocupada</span>
          </div>
        )}
      </div>

      {/* Información de la mesa */}
      <div className="space-y-3 mb-4">
        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center space-x-2 text-gray-600">
            <Users className="w-4 h-4" />
            <span>Capacidad: {mesa.capacidad} personas</span>
          </div>
          <div className="flex items-center space-x-2 text-gray-600">
            <MapPin className="w-4 h-4" />
            <span className="capitalize">{mesa.ubicacion}</span>
          </div>
        </div>

        {/* Información adicional según estado */}
        {mesa.estado === 'Ocupada' && (
          <div
            className={`flex items-center space-x-2 text-sm ${obtenerColorAccent(mesa.estado)} p-3 rounded-lg border`}
          >
            <Clock className="w-4 h-4" />
            <span className="font-medium">En uso - Gestión activa</span>
          </div>
        )}

        {mesa.estado === 'Reservada' && (
          <div
            className={`flex items-center space-x-2 text-sm ${obtenerColorAccent(mesa.estado)} p-3 rounded-lg border`}
          >
            <Calendar className="w-4 h-4" />
            <span className="font-medium">Reservada - Esperando cliente</span>
          </div>
        )}

        {mesa.estado === 'Mantenimiento' && (
          <div
            className={`flex items-center space-x-2 text-sm ${obtenerColorAccent(mesa.estado)} p-3 rounded-lg border`}
          >
            <Settings className="w-4 h-4" />
            <span className="font-medium">En mantenimiento</span>
          </div>
        )}

        {mesa.estado === 'Libre' && (
          <div
            className={`flex items-center space-x-2 text-sm ${obtenerColorAccent(mesa.estado)} p-3 rounded-lg border`}
          >
            <CheckCircle className="w-4 h-4" />
            <span className="font-medium">Disponible para uso</span>
          </div>
        )}
      </div>

      {/* Acciones */}
      <div className="flex items-center justify-between pt-3 border-t border-gray-200">
        {accionPrincipal && (
          <Button
            variant={accionPrincipal.variant}
            size="sm"
            onClick={accionPrincipal.onClick}
            disabled={loading}
            className={`flex-1 mr-2 ${obtenerColorBoton(mesa.estado)} shadow-sm hover:shadow-md transition-all duration-200`}
          >
            {accionPrincipal.icon}
            {accionPrincipal.label}
          </Button>
        )}

        <ActionMenu
          items={accionesSecundarias}
          trigger={
            <Button
              variant="ghost"
              size="sm"
              className="text-gray-600 hover:text-gray-800 hover:bg-gray-100 transition-colors"
            >
              <Settings className="w-4 h-4" />
            </Button>
          }
        />
      </div>

      {/* Indicador de carga */}
      {loading && (
        <div className="absolute inset-0 bg-white bg-opacity-90 flex items-center justify-center rounded-lg backdrop-blur-sm">
          <div className="flex flex-col items-center space-y-2">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-dominican-blue"></div>
            <span className="text-sm text-gray-600 font-medium">Procesando...</span>
          </div>
        </div>
      )}
    </Card>
  );
};
