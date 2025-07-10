import React from 'react';
import {
  X,
  Receipt,
  User,
  Calendar,
  CreditCard,
  DollarSign,
  Package,
  MapPin,
  Phone,
  Mail,
} from 'lucide-react';
import { Modal } from '@/components/ui/Modal';
import type { Factura, Orden, Cliente } from '@/types';

interface FacturaDetalleModalProps {
  factura: Factura | null;
  orden: Orden | null;
  cliente: Cliente | null;
  isOpen: boolean;
  onClose: () => void;
}

// Función auxiliar para convertir strings de moneda a números
const parseCurrency = (value: string | number): number => {
  if (typeof value === 'number') return value;
  if (!value) return 0;

  // Remover "RD$ " y convertir a número
  const cleanValue = value
    .toString()
    .replace(/RD\$\s*/g, '')
    .replace(/,/g, '');
  return parseFloat(cleanValue) || 0;
};

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
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

export const FacturaDetalleModal: React.FC<FacturaDetalleModalProps> = ({
  factura,
  orden,
  cliente,
  isOpen,
  onClose,
}) => {
  if (!factura) return null;

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
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl mx-auto">
        {/* Header */}
        <div className="bg-white border-b border-gray-200 p-6 rounded-t-lg">
          <div className="flex justify-between items-center">
            <div className="flex items-center space-x-4">
              <div className="w-14 h-14 bg-dominican-blue rounded-xl flex items-center justify-center shadow-lg">
                <span className="text-white text-xl">🇩🇴</span>
              </div>
              <div>
                <h2 className="text-2xl font-bold text-gray-900">El Criollo</h2>
                <p className="text-gray-600">Restaurante Dominicano</p>
              </div>
            </div>
            <div className="text-right">
              <div className="bg-dominican-blue text-white px-6 py-3 rounded-xl shadow-lg">
                <div className="text-3xl font-bold">#{factura.numeroFactura}</div>
                <div className="text-sm opacity-90">FACTURA</div>
              </div>
            </div>
          </div>
        </div>

        {/* Información de la factura */}
        <div className="p-6 border-b border-gray-200">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="flex items-center space-x-3">
                <Receipt className="w-5 h-5 text-dominican-blue" />
                <div>
                  <h3 className="font-semibold text-gray-900">Información de Factura</h3>
                  <p className="text-sm text-gray-600">Número: #{factura.numeroFactura}</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <Calendar className="w-5 h-5 text-dominican-blue" />
                <div>
                  <h3 className="font-semibold text-gray-900">Fecha de Facturación</h3>
                  <p className="text-sm text-gray-600">{formatDate(factura.fechaFactura)}</p>
                </div>
              </div>
            </div>
            <div className="space-y-4">
              <div className="flex items-center space-x-3">
                <DollarSign className="w-5 h-5 text-dominican-blue" />
                <div>
                  <h3 className="font-semibold text-gray-900">Método de Pago</h3>
                  <p className="text-sm text-gray-600 capitalize">{factura.metodoPago}</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-5 h-5 flex items-center justify-center">
                  <div
                    className={`w-3 h-3 rounded-full ${getEstadoColor(factura.estado).includes('green') ? 'bg-green-500' : getEstadoColor(factura.estado).includes('yellow') ? 'bg-yellow-500' : 'bg-red-500'}`}
                  ></div>
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900">Estado</h3>
                  <p className="text-sm text-gray-600 capitalize">{factura.estado}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Detalles de la orden */}
        {orden && (
          <div className="p-6 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
              <Receipt className="w-5 h-5 mr-2 text-dominican-blue" />
              Detalles de la Orden #{orden.numeroOrden}
            </h3>
            <div className="space-y-4">
              {orden.detalles?.map((detalle, index) => (
                <div
                  key={index}
                  className="flex justify-between items-start py-3 border-b border-gray-100 last:border-b-0"
                >
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-2">
                      <span className="font-medium text-gray-900">
                        {detalle.cantidad}x {detalle.nombreItem}
                      </span>
                      <span className="text-xs text-gray-500 bg-gray-100 px-2 py-1 rounded">
                        {detalle.categoriaItem}
                      </span>
                    </div>
                    {detalle.descripcionItem && (
                      <p className="text-sm text-gray-600 mb-1">{detalle.descripcionItem}</p>
                    )}
                    {detalle.observaciones && (
                      <p className="text-xs text-orange-600">
                        <strong>Notas:</strong> {detalle.observaciones}
                      </p>
                    )}
                  </div>
                  <div className="text-right ml-4">
                    <div className="font-medium text-gray-900">
                      {formatCurrency(
                        typeof detalle.subtotalNumerico === 'number' ? detalle.subtotalNumerico : 0
                      )}
                    </div>
                    <div className="text-xs text-gray-500">
                      {formatCurrency(parseCurrency(detalle.precioUnitario))} c/u
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Resumen de pagos */}
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Resumen de Pagos</h3>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-gray-600">Subtotal</span>
              <span className="font-medium text-gray-900">{formatCurrency(factura.subtotal)}</span>
            </div>
            {factura.descuento > 0 && (
              <div className="flex justify-between items-center">
                <span className="text-gray-600">Descuento</span>
                <span className="font-medium text-red-600">
                  -{formatCurrency(factura.descuento)}
                </span>
              </div>
            )}
            {factura.propina > 0 && (
              <div className="flex justify-between items-center">
                <span className="text-gray-600">Propina</span>
                <span className="font-medium text-gray-900">{formatCurrency(factura.propina)}</span>
              </div>
            )}
            <div className="flex justify-between items-center pt-3 border-t border-gray-200">
              <span className="text-xl font-bold text-gray-900">TOTAL</span>
              <span className="text-xl font-bold text-dominican-blue">
                {formatCurrency(factura.total)}
              </span>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="p-6 bg-gray-50 rounded-b-lg">
          <div className="text-center text-sm text-gray-600">
            <p>¡Gracias por elegir El Criollo!</p>
            <p className="mt-1">🇩🇴 Auténtico sabor dominicano</p>
          </div>
        </div>
      </div>
    </Modal>
  );
};
