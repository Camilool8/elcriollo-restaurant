import React from 'react';
import {
  Receipt,
  User,
  DollarSign,
  CreditCard,
  FileText,
  Check,
  Download,
  Mail,
  Printer,
} from 'lucide-react';

// Components
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';

// Types
import type { Factura, FacturaDividida, FacturaEstado, MetodoPago } from '@/types';

// Utils
import { formatearPrecio } from '@/utils/dominicanValidations';

interface ResumenFacturaProps {
  factura?: Factura;
  facturasDivididas?: FacturaDividida[];
  onDescargarFactura?: (facturaId: number) => void;
  onEnviarEmail?: (facturaId: number) => void;
  onImprimir?: (facturaId: number) => void;
  onClose?: () => void;
  showActions?: boolean;
}

export const ResumenFacturaSimple: React.FC<ResumenFacturaProps> = ({
  factura,
  facturasDivididas,
  onDescargarFactura,
  onEnviarEmail,
  onImprimir,
  onClose,
  showActions = true,
}) => {
  // ============================================================================
  // FUNCIONES AUXILIARES
  // ============================================================================

  const obtenerEstadoBadge = (estado: FacturaEstado) => {
    switch (estado) {
      case 'Pendiente':
        return <Badge variant="warning">Pendiente</Badge>;
      case 'Pagada':
        return <Badge variant="success">Pagada</Badge>;
      case 'Anulada':
        return <Badge variant="danger">Anulada</Badge>;
      default:
        return <Badge variant="secondary">{estado}</Badge>;
    }
  };

  const obtenerIconoMetodoPago = (metodo: MetodoPago) => {
    switch (metodo) {
      case 'Efectivo':
        return <DollarSign className="w-4 h-4" />;
      case 'Tarjeta de Crédito':
      case 'Tarjeta de Débito':
        return <CreditCard className="w-4 h-4" />;
      default:
        return <FileText className="w-4 h-4" />;
    }
  };

  const calcularTotalGeneral = () => {
    if (factura) {
      return factura.total;
    }

    if (facturasDivididas) {
      return facturasDivididas.reduce((total, f) => total + f.total, 0);
    }

    return 0;
  };

  // ============================================================================
  // COMPONENTES AUXILIARES
  // ============================================================================

  const FacturaCard: React.FC<{ factura: Factura | FacturaDividida }> = ({ factura }) => (
    <Card className="p-4">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-dominican-blue bg-opacity-10 rounded-lg">
            <Receipt className="w-5 h-5 text-dominican-blue" />
          </div>
          <div>
            <h4 className="font-medium text-gray-900">Factura #{factura.numeroFactura}</h4>
            <p className="text-sm text-gray-600">
              {new Date(factura.fechaFactura).toLocaleDateString('es-DO')}
            </p>
          </div>
        </div>

        <div className="flex items-center space-x-2">
          {obtenerEstadoBadge(factura.estado)}
          <div className="flex items-center space-x-1 text-sm text-gray-600">
            {obtenerIconoMetodoPago(factura.metodoPago)}
            <span>{factura.metodoPago}</span>
          </div>
        </div>
      </div>

      <div className="space-y-2">
        {/* Cliente regular */}
        {'cliente' in factura && factura.cliente && (
          <div className="flex items-center space-x-2 text-sm">
            <User className="w-4 h-4 text-gray-500" />
            <span className="text-gray-600">Cliente:</span>
            <span className="font-medium">{factura.cliente.nombreCompleto}</span>
          </div>
        )}

        {/* Cliente asignado para facturas divididas */}
        {'clienteAsignado' in factura && factura.clienteAsignado && (
          <div className="flex items-center space-x-2 text-sm">
            <User className="w-4 h-4 text-gray-500" />
            <span className="text-gray-600">Cliente:</span>
            <span className="font-medium">
              {'nombreCompleto' in factura.clienteAsignado
                ? factura.clienteAsignado.nombreCompleto
                : factura.clienteAsignado.nombre}
            </span>
            {'cedula' in factura.clienteAsignado && factura.clienteAsignado.cedula && (
              <span className="text-gray-500">({factura.clienteAsignado.cedula})</span>
            )}
          </div>
        )}

        <div className="flex items-center space-x-2 text-sm">
          <FileText className="w-4 h-4 text-gray-500" />
          <span className="text-gray-600">Orden:</span>
          <span className="font-medium">
            #{'orden' in factura ? factura.orden?.numeroOrden : factura.ordenID}
          </span>
        </div>

        {/* Items asignados para facturas divididas */}
        {'itemsAsignados' in factura && factura.itemsAsignados && (
          <div className="mt-3 pt-3 border-t">
            <h5 className="text-sm font-medium text-gray-700 mb-2">
              Items Asignados ({factura.itemsAsignados.length})
            </h5>
            <div className="space-y-1">
              {factura.itemsAsignados.map((item) => (
                <div key={item.detalleOrdenID} className="flex justify-between text-sm">
                  <div className="flex-1">
                    <span className="text-gray-900">{item.nombreProducto}</span>
                    <span className="text-gray-600 ml-2">x{item.cantidad}</span>
                  </div>
                  <span className="font-medium">{formatearPrecio(item.subtotal)}</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Desglose de totales */}
      <div className="mt-4 pt-4 border-t">
        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Subtotal:</span>
            <span>{formatearPrecio(factura.subtotal)}</span>
          </div>

          {factura.descuento > 0 && (
            <div className="flex justify-between text-sm text-red-600">
              <span>Descuento:</span>
              <span>-{formatearPrecio(factura.descuento)}</span>
            </div>
          )}

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">ITBIS (18%):</span>
            <span>{formatearPrecio(factura.impuesto)}</span>
          </div>

          {factura.propina > 0 && (
            <div className="flex justify-between text-sm text-green-600">
              <span>Propina:</span>
              <span>+{formatearPrecio(factura.propina)}</span>
            </div>
          )}

          <div className="flex justify-between font-medium text-lg border-t pt-2">
            <span>Total:</span>
            <span className="text-dominican-blue">{formatearPrecio(factura.total)}</span>
          </div>
        </div>
      </div>

      {/* Acciones por factura */}
      {showActions && (
        <div className="mt-4 pt-4 border-t">
          <div className="flex flex-wrap gap-2">
            <Button
              size="sm"
              variant="outline"
              onClick={() => onDescargarFactura?.(factura.facturaID)}
            >
              <Download className="w-4 h-4 mr-1" />
              Descargar
            </Button>
            <Button size="sm" variant="outline" onClick={() => onImprimir?.(factura.facturaID)}>
              <Printer className="w-4 h-4 mr-1" />
              Imprimir
            </Button>
            <Button size="sm" variant="outline" onClick={() => onEnviarEmail?.(factura.facturaID)}>
              <Mail className="w-4 h-4 mr-1" />
              Enviar
            </Button>
          </div>
        </div>
      )}
    </Card>
  );

  // ============================================================================
  // RENDER PRINCIPAL
  // ============================================================================

  const totalGeneral = calcularTotalGeneral();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-dominican-blue bg-opacity-10 rounded-lg">
            <Receipt className="w-6 h-6 text-dominican-blue" />
          </div>
          <div>
            <h3 className="text-lg font-bold text-gray-900">
              {facturasDivididas ? 'Resumen de Facturas Divididas' : 'Resumen de Factura'}
            </h3>
            <p className="text-sm text-gray-600">
              {facturasDivididas
                ? `${facturasDivididas.length} facturas generadas`
                : 'Factura individual'}
            </p>
          </div>
        </div>

        <div className="text-right">
          <div className="text-sm text-gray-600">Total General</div>
          <div className="text-2xl font-bold text-dominican-blue">
            {formatearPrecio(totalGeneral)}
          </div>
        </div>
      </div>

      {/* Estadísticas generales */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="text-center">
            <div className="text-2xl font-bold text-dominican-blue">
              {facturasDivididas ? facturasDivididas.length : 1}
            </div>
            <div className="text-sm text-gray-600">Facturas</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-green-600">
              {formatearPrecio(totalGeneral * 0.82)} {/* Sin impuesto */}
            </div>
            <div className="text-sm text-gray-600">Subtotal</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-amber-600">
              {formatearPrecio(totalGeneral * 0.18)} {/* ITBIS 18% */}
            </div>
            <div className="text-sm text-gray-600">ITBIS</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-dominican-blue">
              {formatearPrecio(totalGeneral)}
            </div>
            <div className="text-sm text-gray-600">Total</div>
          </div>
        </div>
      </Card>

      {/* Facturas individuales */}
      <div className="space-y-4">
        {factura && (
          <div>
            <h4 className="font-medium text-gray-900 mb-3">Factura</h4>
            <FacturaCard factura={factura} />
          </div>
        )}

        {facturasDivididas && (
          <div>
            <h4 className="font-medium text-gray-900 mb-3">Facturas Divididas</h4>
            <div className="space-y-3">
              {facturasDivididas.map((factura) => (
                <FacturaCard key={factura.facturaID} factura={factura} />
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Acciones generales */}
      {showActions && (
        <Card className="p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <Check className="w-5 h-5 text-green-600" />
              <span className="text-sm font-medium text-gray-900">
                {facturasDivididas
                  ? 'Facturas procesadas exitosamente'
                  : 'Factura procesada exitosamente'}
              </span>
            </div>

            <div className="flex space-x-2">
              {facturasDivididas && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => {
                    facturasDivididas.forEach((f) => onDescargarFactura?.(f.facturaID));
                  }}
                >
                  <Download className="w-4 h-4 mr-1" />
                  Descargar Todo
                </Button>
              )}

              <Button
                size="sm"
                onClick={onClose}
                className="bg-dominican-blue hover:bg-dominican-blue/90"
              >
                Cerrar
              </Button>
            </div>
          </div>
        </Card>
      )}
    </div>
  );
};
