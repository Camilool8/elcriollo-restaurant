import React from 'react';
import { Receipt, User, Calendar, CreditCard, DollarSign, Package, MapPin } from 'lucide-react';
import type { FacturaCardProps } from '@/types/reportes';
const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    minimumFractionDigits: 2,
  }).format(amount);
};

export const FacturaCard: React.FC<FacturaCardProps> = ({
  factura,
  orden,
  cliente,
  onVerDetalle,
  className = '',
}) => {
  const getEstadoColor = (estado: string) => {
    switch (estado.toLowerCase()) {
      case 'pagada':
        return 'text-green-600 bg-green-50 border-green-200';
      case 'pendiente':
        return 'text-yellow-600 bg-yellow-50 border-yellow-200';
      case 'anulada':
        return 'text-red-600 bg-red-50 border-red-200';
      default:
        return 'text-gray-600 bg-gray-50 border-gray-200';
    }
  };

  const getMetodoPagoIcon = (metodo: string) => {
    switch (metodo.toLowerCase()) {
      case 'efectivo':
        return <DollarSign className="w-4 h-4" />;
      case 'tarjeta':
        return <CreditCard className="w-4 h-4" />;
      case 'transferencia':
        return <Receipt className="w-4 h-4" />;
      default:
        return <DollarSign className="w-4 h-4" />;
    }
  };

  return (
    <div
      className={`bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow cursor-pointer ${className}`}
      onClick={() => onVerDetalle?.(factura)}
    >
      {/* Header de la factura */}
      <div className="bg-gradient-to-r from-dominican-blue to-dominican-blue-dark text-white p-4 rounded-t-lg">
        <div className="flex justify-between items-start">
          <div>
            <h3 className="text-lg font-bold">🇩🇴 El Criollo</h3>
            <p className="text-sm opacity-90">Restaurante Dominicano</p>
            <p className="text-xs opacity-75 mt-1">
              <MapPin className="w-3 h-3 inline mr-1" />
              Santo Domingo, República Dominicana
            </p>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold">#{factura.numeroFactura}</div>
            <div className="text-xs opacity-75">FACTURA</div>
          </div>
        </div>
      </div>

      {/* Información del cliente y fecha */}
      <div className="p-4 border-b border-gray-100">
        <div className="grid grid-cols-2 gap-4">
          <div className="flex items-center space-x-2">
            <User className="w-4 h-4 text-gray-500" />
            <div>
              <p className="text-sm font-medium text-gray-900">
                {cliente?.nombreCompleto || 'Cliente General'}
              </p>
              <p className="text-xs text-gray-500">{cliente?.email || 'Sin email'}</p>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <Calendar className="w-4 h-4 text-gray-500" />
            <div>
              <p className="text-sm font-medium text-gray-900">
                {new Date(factura.fechaFactura).toLocaleDateString('es-DO')}
              </p>
              <p className="text-xs text-gray-500">
                {new Date(factura.fechaFactura).toLocaleTimeString('es-DO')}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Detalles de la factura */}
      <div className="p-4">
        <div className="space-y-3">
          {/* Método de pago */}
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              {getMetodoPagoIcon(factura.metodoPago)}
              <span className="text-sm font-medium text-gray-700">Método de Pago:</span>
            </div>
            <span className="text-sm text-gray-900 capitalize">{factura.metodoPago}</span>
          </div>

          {/* Estado */}
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-700">Estado:</span>
            <span
              className={`text-xs px-2 py-1 rounded-full border ${getEstadoColor(factura.estado)}`}
            >
              {factura.estado}
            </span>
          </div>

          {/* Mesa */}
          {orden?.mesaID && (
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-700">Mesa:</span>
              <span className="text-sm text-gray-900">#{orden.mesaID}</span>
            </div>
          )}

          {/* Subtotal */}
          <div className="flex items-center justify-between pt-2 border-t border-gray-100">
            <span className="text-sm font-medium text-gray-700">Subtotal:</span>
            <span className="text-sm text-gray-900">{formatCurrency(factura.subtotal)}</span>
          </div>

          {/* Descuento */}
          {factura.descuento > 0 && (
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-700">Descuento:</span>
              <span className="text-sm text-red-600">-{formatCurrency(factura.descuento)}</span>
            </div>
          )}

          {/* Propina */}
          {factura.propina > 0 && (
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-gray-700">Propina:</span>
              <span className="text-sm text-gray-900">{formatCurrency(factura.propina)}</span>
            </div>
          )}

          {/* Total */}
          <div className="flex items-center justify-between pt-2 border-t border-gray-200">
            <span className="text-lg font-bold text-gray-900">TOTAL:</span>
            <span className="text-lg font-bold text-dominican-blue">
              {formatCurrency(factura.total)}
            </span>
          </div>
        </div>
      </div>

      {/* Footer con información adicional */}
      <div className="bg-gray-50 p-3 rounded-b-lg">
        <div className="flex items-center justify-between text-xs text-gray-500">
          <div className="flex items-center space-x-1">
            <Package className="w-3 h-3" />
            <span>{orden?.detalles?.length || 0} productos</span>
          </div>
          <div className="flex items-center space-x-1">
            <Receipt className="w-3 h-3" />
            <span>Orden #{orden?.ordenID}</span>
          </div>
        </div>
      </div>
    </div>
  );
};
