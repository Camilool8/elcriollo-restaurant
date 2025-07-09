import React, { useState, useEffect, useMemo } from 'react';
import { Receipt, Calculator, AlertCircle, Check, X } from 'lucide-react';
import { toast } from 'react-toastify';

// Components
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Input } from '@/components/ui/Input';
import LoadingSpinner from '../ui/LoadingSpinner';

// Services
import { facturaService } from '@/services/facturaService';

// Types
import type { Orden, CrearFacturaRequest, MetodoPago } from '@/types';

interface FacturaFormSimpleProps {
  orden: Orden;
  onFacturaCreada: () => void;
  onClose: () => void;
}

export const FacturaFormSimple: React.FC<FacturaFormSimpleProps> = ({
  orden,
  onFacturaCreada,
  onClose,
}) => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [metodoPago, setMetodoPago] = useState<MetodoPago>('Efectivo');
  const [montoRecibido, setMontoRecibido] = useState<number>(0);

  // Calcular totales usando useMemo para evitar re-renders
  const { subtotal, impuesto, total } = useMemo(() => {
    if (!orden || !orden.detalles) {
      return { subtotal: 0, impuesto: 0, total: 0 };
    }

    const subtotalCalculado = orden.detalles.reduce((acc, detalle) => {
      // Usar el campo numérico si está disponible, sino intentar parsear el string
      const subtotalDetalle =
        detalle.subtotalNumerico ||
        (typeof detalle.subtotal === 'string'
          ? parseFloat(detalle.subtotal.replace(/[^\d.-]/g, ''))
          : 0);
      return acc + subtotalDetalle;
    }, 0);

    const impuestoCalculado = subtotalCalculado * 0.18;
    const totalCalculado = subtotalCalculado + impuestoCalculado;

    return {
      subtotal: subtotalCalculado,
      impuesto: impuestoCalculado,
      total: totalCalculado,
    };
  }, [orden]);

  // Inicializar monto recibido cuando cambie el total
  useEffect(() => {
    setMontoRecibido(total);
  }, [total]);

  // Calcular cambio
  const cambio = useMemo(() => {
    const cambioCalculado = montoRecibido - total;
    return cambioCalculado > 0 ? cambioCalculado : 0;
  }, [montoRecibido, total]);

  const handleConfirmarCreacion = async () => {
    // Verificar si la orden ya está facturada
    if (orden.estado === 'Facturada') {
      await handleMarcarComoPagada();
      return;
    }

    if (montoRecibido < total) {
      toast.error('El monto recibido debe ser mayor o igual al total.');
      return;
    }

    setIsSubmitting(true);
    try {
      const facturaRequest: CrearFacturaRequest = {
        ordenID: orden.ordenID,
        metodoPago: metodoPago,
      };

      await facturaService.crearFactura(facturaRequest);
      toast.success('Factura creada exitosamente.');
      onFacturaCreada();
    } catch (error) {
      toast.error('Hubo un error al crear la factura.');
      console.error(error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleMarcarComoPagada = async () => {
    setIsSubmitting(true);
    try {
      // Obtener la factura existente de la orden
      const facturas = await facturaService.obtenerFacturasPorOrden(orden.ordenID);
      const facturaPendiente = facturas.find((f) => f.estado === 'Pendiente');

      if (!facturaPendiente) {
        toast.error('No se encontró una factura pendiente para esta orden.');
        return;
      }

      // Marcar la factura como pagada
      await facturaService.marcarComoPagada(facturaPendiente.facturaID, metodoPago);
      toast.success('Factura marcada como pagada exitosamente.');
      onFacturaCreada();
    } catch (error) {
      toast.error('Hubo un error al marcar la factura como pagada.');
      console.error(error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!orden || !orden.detalles) {
    return (
      <div className="p-8 text-center">
        <AlertCircle className="mx-auto h-12 w-12 text-gray-400" />
        <h3 className="mt-2 text-sm font-medium text-gray-900">No se pudo cargar la orden</h3>
        <p className="mt-1 text-sm text-gray-500">
          Intente cerrar esta ventana y volver a abrirla.
        </p>
      </div>
    );
  }

  // Verificar si la orden ya está facturada
  if (orden.estado === 'Facturada') {
    return (
      <div className="p-8 text-center">
        <Check className="mx-auto h-12 w-12 text-green-500" />
        <h3 className="mt-2 text-sm font-medium text-gray-900">Orden ya facturada</h3>
        <p className="mt-1 text-sm text-gray-500 mb-4">
          Esta orden ya ha sido facturada. Puede marcar la factura como pagada si el cliente ya
          realizó el pago.
        </p>
        <div className="space-y-3">
          <Button
            onClick={handleConfirmarCreacion}
            className="bg-green-600 hover:bg-green-700"
            disabled={isSubmitting}
          >
            {isSubmitting ? <LoadingSpinner size="sm" /> : <Check className="w-4 h-4 mr-2" />}
            Marcar como Pagada
          </Button>
          <Button onClick={onClose} variant="outline">
            Cerrar
          </Button>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="space-y-6 p-1">
        {/* Encabezado */}
        <div className="flex justify-between items-start">
          <div>
            <h2 className="text-2xl font-bold text-gray-800 flex items-center">
              <Receipt className="w-6 h-6 mr-2 text-dominican-blue" />
              Facturar Orden
            </h2>
            <p className="text-gray-500 text-sm">
              Orden #{orden.numeroOrden} - Mesa {orden.mesa?.numeroMesa}
            </p>
          </div>
          <Button type="button" variant="ghost" size="icon" onClick={onClose}>
            <X className="w-5 h-5" />
          </Button>
        </div>

        {/* Detalles de la orden */}
        <div className="border-t border-b py-4 space-y-3">
          <h3 className="font-semibold text-lg text-gray-800">Resumen de la Orden</h3>
          <div className="max-h-40 overflow-y-auto pr-2 space-y-2">
            {orden.detalles.map((item) => (
              <div key={item.detalleOrdenID} className="flex justify-between items-center">
                <span className="text-gray-600">
                  {item.cantidad} x {item.producto?.nombre || 'Producto no disponible'}
                </span>
                <span className="font-mono text-gray-800">
                  ${(item.subtotalNumerico || 0).toFixed(2)}
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Resumen de costos */}
        <div className="p-4 bg-gray-50 rounded-lg space-y-2">
          <h4 className="font-semibold text-gray-800 flex items-center">
            <Calculator className="w-5 h-5 mr-2" />
            Totales
          </h4>
          <div className="flex justify-between">
            <span className="text-gray-600">Subtotal:</span>
            <span className="font-mono font-medium text-gray-900">${subtotal.toFixed(2)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">ITBIS (18%):</span>
            <span className="font-mono font-medium text-gray-900">${impuesto.toFixed(2)}</span>
          </div>
          <hr />
          <div className="flex justify-between text-lg font-bold">
            <span className="text-gray-900">Total a Pagar:</span>
            <span className="font-mono text-dominican-blue">${total.toFixed(2)}</span>
          </div>
        </div>

        {/* Formulario de pago */}
        <div className="space-y-4">
          <div>
            <label htmlFor="metodoDePago" className="block text-sm font-medium text-gray-700">
              Método de Pago
            </label>
            <select
              id="metodoDePago"
              value={metodoPago}
              onChange={(e) => setMetodoPago(e.target.value as MetodoPago)}
              className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-dominican-blue focus:border-dominican-blue sm:text-sm rounded-md"
            >
              <option value="Efectivo">Efectivo</option>
              <option value="Tarjeta de Débito">Tarjeta de Débito</option>
              <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
              <option value="Transferencia Bancaria">Transferencia Bancaria</option>
              <option value="Pago Móvil">Pago Móvil</option>
              <option value="Cheque">Cheque</option>
            </select>
          </div>

          <div>
            <label htmlFor="montoRecibido" className="block text-sm font-medium text-gray-700">
              Monto Recibido
            </label>
            <Input
              id="montoRecibido"
              type="number"
              step="0.01"
              min={total}
              value={montoRecibido}
              onChange={(e) => setMontoRecibido(parseFloat(e.target.value) || 0)}
              className="mt-1"
            />
            {montoRecibido < total && montoRecibido > 0 && (
              <p className="mt-2 text-sm text-red-600 flex items-center">
                <AlertCircle className="w-4 h-4 mr-1" />
                El monto recibido debe ser mayor o igual al total.
              </p>
            )}
          </div>

          <div className="flex justify-between items-center text-lg font-medium p-3 bg-green-50 rounded-md">
            <span>Cambio:</span>
            <span className="font-mono text-green-700">${cambio.toFixed(2)}</span>
          </div>
        </div>

        {/* Acciones */}
        <div className="flex flex-col sm:flex-row justify-end space-y-2 sm:space-y-0 sm:space-x-3 pt-4 border-t">
          <Button
            onClick={handleConfirmarCreacion}
            disabled={isSubmitting || montoRecibido < total}
            className="w-full sm:w-auto"
          >
            {isSubmitting ? (
              <LoadingSpinner size="sm" />
            ) : (
              <>
                <Check className="w-4 h-4 mr-2" />
                Confirmar y Facturar
              </>
            )}
          </Button>
        </div>
      </div>
    </>
  );
};
