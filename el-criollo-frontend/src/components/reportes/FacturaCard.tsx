import React from 'react';
import { DollarSign, Eye, Receipt } from 'lucide-react';
import type { Factura, Orden, Cliente } from '@/types';

interface FacturaCardProps {
  factura: Factura;
  orden: Orden | null;
  cliente: Cliente | null;
  onVerDetalle: (factura: Factura) => void;
  onExportar: () => void;
  className?: string;
}

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    minimumFractionDigits: 2,
  }).format(amount);
};

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('es-DO', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

export const FacturaCard: React.FC<FacturaCardProps> = ({
  factura,
  orden,
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
      case 'tarjeta de débito':
      case 'tarjeta de crédito':
        return <Receipt className="w-4 h-4" />;
      case 'transferencia bancaria':
        return <Receipt className="w-4 h-4" />;
      default:
        return <DollarSign className="w-4 h-4" />;
    }
  };

  // Asegurar que los valores numéricos sean correctos
  const subtotal = typeof factura.subtotal === 'number' ? factura.subtotal : 0;
  const total = typeof factura.total === 'number' ? factura.total : 0;
  const descuento = typeof factura.descuento === 'number' ? factura.descuento : 0;
  const propina = typeof factura.propina === 'number' ? factura.propina : 0;

  return (
    <div
      className={`bg-white border border-gray-200 rounded-xl shadow-sm hover:shadow-lg transition-all duration-200 cursor-pointer transform hover:scale-105 ${className}`}
      onClick={() => onVerDetalle(factura)}
    >
      {/* Header con información principal */}
      <div className="p-6 border-b border-gray-100">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="text-xl font-bold text-gray-900">Factura #{factura.numeroFactura}</h3>
            <p className="text-sm text-gray-500 mt-1">{formatDate(factura.fechaFactura)}</p>
          </div>
          <div className="text-right">
            <span
              className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-medium border ${getEstadoColor(factura.estado)}`}
            >
              {factura.estado}
            </span>
          </div>
        </div>

        {/* Información de pago */}
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            {getMetodoPagoIcon(factura.metodoPago)}
            <span className="text-sm font-medium text-gray-700">{factura.metodoPago}</span>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-dominican-blue">{formatCurrency(total)}</div>
            <div className="text-xs text-gray-500">Total</div>
          </div>
        </div>
      </div>

      {/* Detalles de la factura */}
      <div className="p-6">
        <div className="space-y-3">
          {/* Subtotal */}
          <div className="flex justify-between items-center">
            <span className="text-sm text-gray-600">Subtotal</span>
            <span className="text-sm font-medium text-gray-900">{formatCurrency(subtotal)}</span>
          </div>

          {/* Descuento */}
          {descuento > 0 && (
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">Descuento</span>
              <span className="text-sm font-medium text-red-600">-{formatCurrency(descuento)}</span>
            </div>
          )}

          {/* Propina */}
          {propina > 0 && (
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">Propina</span>
              <span className="text-sm font-medium text-gray-900">{formatCurrency(propina)}</span>
            </div>
          )}

          {/* Mesa */}
          {orden?.mesaID && (
            <div className="flex justify-between items-center pt-2 border-t border-gray-100">
              <span className="text-sm text-gray-600">Mesa</span>
              <span className="text-sm font-medium text-gray-900">#{orden.mesaID}</span>
            </div>
          )}

          {/* Productos */}
          <div className="flex justify-between items-center pt-2 border-t border-gray-100">
            <span className="text-sm text-gray-600">Productos</span>
            <span className="text-sm font-medium text-gray-900">
              {orden?.detalles?.length || 0} items
            </span>
          </div>
        </div>
      </div>

      {/* Footer con acción */}
      <div className="px-6 py-4 bg-gray-50 rounded-b-xl">
        <div className="flex items-center justify-center text-sm text-dominican-blue font-medium">
          <Eye className="w-4 h-4 mr-2" />
          Ver detalles completos
        </div>
      </div>
    </div>
  );
};
