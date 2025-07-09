import React from 'react';
import {
  Clock,
  User,
  MapPin,
  DollarSign,
  Utensils,
  AlertTriangle,
  Check,
  X,
  ChefHat,
  Package,
  Receipt,
  Edit,
} from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import type { Orden, EstadoOrden } from '@/types/orden';
import { COLORES_ESTADO_ORDEN, ICONOS_TIPO_ORDEN } from '@/types/orden';
import { parsePrice } from '@/utils/priceUtils';
import { formatearPrecio } from '@/utils/dominicanValidations';
import { useOrdenesContext } from '@/contexts/OrdenesContext';

interface OrdenCardProps {
  orden: Orden;
  onEstadoChange?: (ordenId: number, nuevoEstado: EstadoOrden) => void;
  onVerDetalles?: (orden: Orden) => void;
  onEditarOrden?: (orden: Orden) => void;
  onCancelarOrden?: (orden: Orden) => void;
  onFacturarOrden?: (orden: Orden) => void;
  compact?: boolean;
  showActions?: boolean;
}

export const OrdenCard: React.FC<OrdenCardProps> = ({
  orden,
  onEstadoChange,
  onVerDetalles,
  onEditarOrden,
  onCancelarOrden,
  onFacturarOrden,
  compact = false,
  showActions = true,
}) => {
  const { esOrdenReciente } = useOrdenesContext();
  const colorConfig = COLORES_ESTADO_ORDEN[orden.estado] || {
    bg: 'bg-gray-100',
    border: 'border-gray-500',
    text: 'text-gray-800',
    icon: '❓',
  };
  const iconoTipo = ICONOS_TIPO_ORDEN[orden.tipoOrden] || '📋';

  // Aplicar estilo especial si la orden fue actualizada recientemente
  const esReciente = esOrdenReciente(orden.ordenID);
  const estiloReciente = esReciente ? 'ring-2 ring-blue-500 ring-opacity-50' : '';

  const handleEstadoChange = (nuevoEstado: EstadoOrden) => {
    if (onEstadoChange) {
      onEstadoChange(orden.ordenID, nuevoEstado);
    }
  };

  const getAccionesRapidas = () => {
    const acciones: React.ReactNode[] = [];

    // No mostrar acciones rápidas si la orden está facturada
    if (orden.estado === 'Facturada') {
      return acciones;
    }

    switch (orden.estado) {
      case 'Pendiente':
        acciones.push(
          <button
            key="preparar"
            onClick={() => handleEstadoChange('En Preparacion')}
            className="flex items-center space-x-1 px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-xs hover:bg-blue-200 transition-colors"
            title="Iniciar preparación"
          >
            <ChefHat className="w-3 h-3" />
            <span>Preparar</span>
          </button>
        );
        break;

      case 'En Preparacion':
        acciones.push(
          <button
            key="completar"
            onClick={() => handleEstadoChange('Lista')}
            className="flex items-center space-x-1 px-3 py-1 bg-green-100 text-green-800 rounded-full text-xs hover:bg-green-200 transition-colors"
            title="Marcar como lista"
          >
            <Check className="w-3 h-3" />
            <span>Lista</span>
          </button>
        );
        break;

      case 'Lista':
        acciones.push(
          <button
            key="entregar"
            onClick={() => handleEstadoChange('Entregada')}
            className="flex items-center space-x-1 px-3 py-1 bg-emerald-100 text-emerald-800 rounded-full text-xs hover:bg-emerald-200 transition-colors"
            title="Marcar como entregada"
          >
            <Package className="w-3 h-3" />
            <span>Entregar</span>
          </button>
        );
        break;
    }

    // Botón de cancelar (solo si no está finalizada)
    if (!['Entregada', 'Cancelada', 'Facturada'].includes(orden.estado)) {
      acciones.push(
        <button
          key="cancelar"
          onClick={() => onCancelarOrden?.(orden)}
          className="flex items-center space-x-1 px-3 py-1 bg-red-100 text-red-800 rounded-full text-xs hover:bg-red-200 transition-colors"
          title="Cancelar orden"
        >
          <X className="w-3 h-3" />
          <span>Cancelar</span>
        </button>
      );
    }

    return acciones;
  };

  if (compact) {
    return (
      <div
        className={`p-3 border rounded-lg cursor-pointer hover:shadow-md transition-shadow ${colorConfig.border} ${colorConfig.bg} ${colorConfig.text}`}
        onClick={() => onVerDetalles?.(orden)}
      >
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <span className="text-lg">{iconoTipo}</span>
            <div>
              <div className="font-medium text-sm">{orden.numeroOrden}</div>
              {orden.mesa && <div className="text-xs opacity-75">Mesa {orden.mesa.numeroMesa}</div>}
            </div>
          </div>
          <div className="text-right">
            <div className="font-bold text-sm">{formatearPrecio(orden.totalCalculado || 0)}</div>
            <div className="text-xs opacity-75">{orden.totalItems} items</div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <Card
      className={`p-4 hover:shadow-lg transition-shadow border-l-4 ${colorConfig.border} ${estiloReciente}`}
    >
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center space-x-3">
          <div className="flex items-center space-x-2">
            <span className="text-2xl">{iconoTipo}</span>
            <div>
              <h3 className="font-bold text-lg text-gray-900">{orden.numeroOrden}</h3>
              <p className="text-sm text-gray-600">
                {new Date(orden.fechaCreacion).toLocaleString('es-DO', {
                  hour: '2-digit',
                  minute: '2-digit',
                  day: '2-digit',
                  month: '2-digit',
                })}
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center space-x-2">
          <Badge className={`${colorConfig.bg} ${colorConfig.text} border-0`}>
            <span className="mr-1">{colorConfig.icon}</span>
            {orden.estado}
          </Badge>
          {orden.tiempoTranscurrido && (
            <Badge variant="secondary" className="text-xs">
              <Clock className="w-3 h-3 mr-1" />
              {orden.tiempoTranscurrido}
            </Badge>
          )}
        </div>
      </div>

      {/* Información principal */}
      <div className="grid grid-cols-2 gap-4 mb-4">
        <div className="space-y-2">
          {orden.mesa && (
            <div className="flex items-center space-x-2 text-sm">
              <MapPin className="w-4 h-4 text-gray-500" />
              <span className="font-medium">Mesa {orden.mesa.numeroMesa}</span>
              {orden.mesa.ubicacion && (
                <span className="text-gray-600">({orden.mesa.ubicacion})</span>
              )}
            </div>
          )}

          {orden.cliente && (
            <div className="flex items-center space-x-2 text-sm">
              <User className="w-4 h-4 text-gray-500" />
              <span>{orden.cliente.nombreCompleto}</span>
            </div>
          )}

          <div className="flex items-center space-x-2 text-sm">
            <Utensils className="w-4 h-4 text-gray-500" />
            <span>{orden.totalItems} items</span>
          </div>
        </div>

        <div className="space-y-2">
          <div className="flex items-center space-x-2 text-sm">
            <DollarSign className="w-4 h-4 text-gray-500" />
            <div>
              <div className="font-bold text-lg text-gray-900">
                {formatearPrecio(orden.totalCalculado || 0)}
              </div>
              <div className="text-xs text-gray-600">
                Subtotal: {formatearPrecio(orden.subtotalCalculado || 0)}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Observaciones */}
      {orden.observaciones && (
        <div className="mb-4 p-2 bg-yellow-50 border-l-4 border-yellow-400 rounded">
          <div className="flex items-start space-x-2">
            <AlertTriangle className="w-4 h-4 text-yellow-600 mt-0.5" />
            <p className="text-sm text-yellow-800">{orden.observaciones}</p>
          </div>
        </div>
      )}

      {/* Items preview (primeros 3) */}
      {orden.detalles && orden.detalles.length > 0 && (
        <div className="mb-4">
          <h4 className="font-medium text-sm text-gray-700 mb-2">Items:</h4>
          <div className="space-y-1">
            {orden.detalles.slice(0, 3).map((detalle, index) => (
              <div key={index} className="flex justify-between text-sm">
                <span className="text-gray-700">
                  {detalle.cantidad}x {detalle.nombreItem}
                </span>
                <span className="font-medium">{detalle.subtotal}</span>
              </div>
            ))}
            {orden.detalles.length > 3 && (
              <div className="text-xs text-gray-500 italic">
                +{orden.detalles.length - 3} items más...
              </div>
            )}
          </div>
        </div>
      )}

      {/* Acciones */}
      {showActions && (
        <div className="mt-4 pt-4 border-t flex flex-col sm:flex-row justify-between items-center space-y-2 sm:space-y-0 sm:space-x-2">
          <div className="flex flex-wrap gap-2">{getAccionesRapidas()}</div>

          <div className="flex space-x-2">
            <Button variant="outline" size="sm" onClick={() => onVerDetalles?.(orden)}>
              Detalles
            </Button>

            {/* Solo mostrar botón de editar si no está facturada */}
            {orden.estado !== 'Facturada' && (
              <Button variant="outline" size="sm" onClick={() => onEditarOrden?.(orden)}>
                <Edit className="w-4 h-4 mr-2" />
                Editar
              </Button>
            )}

            {/* Solo mostrar botón de facturar si no está facturada y está en estado válido */}
            {orden.estado !== 'Facturada' && (
              <Button
                size="sm"
                className="bg-dominican-blue hover:bg-dominican-blue/90"
                onClick={() => onFacturarOrden?.(orden)}
                disabled={
                  orden.estado !== 'Entregada' &&
                  orden.estado !== 'Lista' &&
                  orden.estado !== 'Pendiente'
                }
              >
                <Receipt className="w-4 h-4 mr-2" />
                Facturar
              </Button>
            )}

            {/* Mostrar indicador de facturada si está facturada */}
            {orden.estado === 'Facturada' && (
              <Button size="sm" className="bg-green-600 hover:bg-green-700 text-white" disabled>
                <Check className="w-4 h-4 mr-2" />
                Facturada
              </Button>
            )}
          </div>
        </div>
      )}
    </Card>
  );
};
