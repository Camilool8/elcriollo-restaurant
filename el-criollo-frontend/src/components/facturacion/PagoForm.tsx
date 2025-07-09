import React, { useState, useEffect } from 'react';
import {
  CreditCard,
  DollarSign,
  Check,
  X,
  AlertCircle,
  Receipt,
  Clock,
  User,
  FileText,
  Calculator,
} from 'lucide-react';
import { toast } from 'react-toastify';

// Components
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

// Services
import { facturaService } from '@/services/facturaService';

// Types
import type { Factura, PagoRequest, PagoResponse, MetodoPago, FacturaEstado } from '@/types';

// Utils
import { formatearPrecio, METODOS_PAGO_RD } from '@/utils/dominicanValidations';

interface PagoFormProps {
  factura: Factura;
  onPagoProcesado?: (respuesta: PagoResponse) => void;
  onClose?: () => void;
  readonly?: boolean;
}

export const PagoForm: React.FC<PagoFormProps> = ({
  factura,
  onPagoProcesado,
  onClose,
  readonly = false,
}) => {
  // Estados principales
  const [loading, setLoading] = useState(false);
  const [metodoPago, setMetodoPago] = useState<MetodoPago>(factura.metodoPago);
  const [montoPagado, setMontoPagado] = useState<number>(factura.total);
  const [observacionesPago, setObservacionesPago] = useState<string>(
    factura.observacionesPago || ''
  );

  // Estados calculados
  const [vuelto, setVuelto] = useState<number>(0);
  const [confirmacionPago, setConfirmacionPago] = useState<boolean>(false);

  // Estados de validación
  const [errores, setErrores] = useState<Record<string, string>>({});

  // ============================================================================
  // EFECTOS
  // ============================================================================

  useEffect(() => {
    calcularVuelto();
  }, [montoPagado, factura.total]);

  // ============================================================================
  // FUNCIONES DE CÁLCULO
  // ============================================================================

  const calcularVuelto = () => {
    const vueltoCalculado = montoPagado - factura.total;
    setVuelto(vueltoCalculado > 0 ? vueltoCalculado : 0);
  };

  // ============================================================================
  // VALIDACIONES
  // ============================================================================

  const validarPago = (): boolean => {
    const nuevosErrores: Record<string, string> = {};

    if (!metodoPago) {
      nuevosErrores.metodoPago = 'Debe seleccionar un método de pago';
    }

    if (montoPagado <= 0) {
      nuevosErrores.montoPagado = 'El monto pagado debe ser mayor a 0';
    }

    if (montoPagado < factura.total) {
      nuevosErrores.montoPagado = 'El monto pagado no puede ser menor al total de la factura';
    }

    if (factura.estado === 'Pagada') {
      nuevosErrores.estado = 'Esta factura ya ha sido pagada';
    }

    if (factura.estado === 'Anulada') {
      nuevosErrores.estado = 'No se puede procesar pago de una factura anulada';
    }

    setErrores(nuevosErrores);
    return Object.keys(nuevosErrores).length === 0;
  };

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const handleProcesarPago = async () => {
    if (!validarPago()) return;

    if (!confirmacionPago) {
      setConfirmacionPago(true);
      return;
    }

    try {
      setLoading(true);

      const pagoRequest: PagoRequest = {
        facturaID: factura.facturaID,
        metodoPago,
        montoPagado,
        observaciones: observacionesPago || undefined,
      };

      const respuestaPago = await facturaService.procesarPago(pagoRequest);

      toast.success('Pago procesado exitosamente');

      if (onPagoProcesado) {
        onPagoProcesado(respuestaPago);
      }

      if (onClose) {
        onClose();
      }
    } catch (error: any) {
      console.error('Error procesando pago:', error);
      toast.error(error.message || 'Error al procesar el pago');
      setConfirmacionPago(false);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelar = () => {
    if (confirmacionPago) {
      setConfirmacionPago(false);
    } else if (onClose) {
      onClose();
    }
  };

  const handleMontoChange = (value: string) => {
    const numero = parseFloat(value) || 0;
    setMontoPagado(numero);
  };

  const aplicarMontoExacto = () => {
    setMontoPagado(factura.total);
  };

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

  // ============================================================================
  // RENDER
  // ============================================================================

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-dominican-blue bg-opacity-10 rounded-lg">
            <CreditCard className="w-6 h-6 text-dominican-blue" />
          </div>
          <div>
            <h3 className="text-lg font-bold text-gray-900">
              {readonly ? 'Detalles de Pago' : 'Procesar Pago'}
            </h3>
            <p className="text-sm text-gray-600">Factura #{factura.numeroFactura}</p>
          </div>
        </div>

        {obtenerEstadoBadge(factura.estado)}
      </div>

      {/* Información de la factura */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <FileText className="w-4 h-4 text-gray-500" />
              <span className="font-medium">Factura:</span>
              <span>{factura.numeroFactura}</span>
            </div>

            <div className="flex items-center space-x-2">
              <Receipt className="w-4 h-4 text-gray-500" />
              <span className="font-medium">Orden:</span>
              <span>#{factura.orden?.numeroOrden || factura.ordenID}</span>
            </div>

            {factura.cliente && (
              <div className="flex items-center space-x-2">
                <User className="w-4 h-4 text-gray-500" />
                <span className="font-medium">Cliente:</span>
                <span>{factura.cliente.nombreCompleto}</span>
              </div>
            )}
          </div>

          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <Clock className="w-4 h-4 text-gray-500" />
              <span className="font-medium">Fecha:</span>
              <span>{new Date(factura.fechaFactura).toLocaleDateString('es-DO')}</span>
            </div>

            {factura.fechaPago && (
              <div className="flex items-center space-x-2">
                <Check className="w-4 h-4 text-green-500" />
                <span className="font-medium">Pagado:</span>
                <span>{new Date(factura.fechaPago).toLocaleDateString('es-DO')}</span>
              </div>
            )}

            <div className="flex items-center space-x-2">
              <span className="font-medium">Método Original:</span>
              <Badge variant="secondary">{factura.metodoPago}</Badge>
            </div>
          </div>
        </div>
      </Card>

      {/* Resumen de la factura */}
      <Card className="p-6">
        <h4 className="font-medium text-gray-900 mb-4 flex items-center">
          <Calculator className="w-5 h-5 mr-2" />
          Resumen de Factura
        </h4>

        <div className="space-y-3">
          <div className="flex justify-between text-gray-600">
            <span>Subtotal:</span>
            <span>{formatearPrecio(factura.subtotal)}</span>
          </div>

          {factura.descuento > 0 && (
            <div className="flex justify-between text-red-600">
              <span>Descuento:</span>
              <span>-{formatearPrecio(factura.descuento)}</span>
            </div>
          )}

          <div className="flex justify-between text-gray-600">
            <span>ITBIS (18%):</span>
            <span>{formatearPrecio(factura.impuesto)}</span>
          </div>

          {factura.propina > 0 && (
            <div className="flex justify-between text-green-600">
              <span>Propina:</span>
              <span>+{formatearPrecio(factura.propina)}</span>
            </div>
          )}

          <div className="border-t pt-3">
            <div className="flex justify-between text-lg font-bold text-gray-900">
              <span>Total:</span>
              <span className="text-dominican-blue">{formatearPrecio(factura.total)}</span>
            </div>
          </div>
        </div>
      </Card>

      {/* Formulario de pago */}
      {!readonly && factura.estado === 'Pendiente' && (
        <Card className="p-6">
          <h4 className="font-medium text-gray-900 mb-4 flex items-center">
            <DollarSign className="w-5 h-5 mr-2" />
            Procesamiento de Pago
          </h4>

          <div className="space-y-4">
            {/* Método de pago */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Método de Pago *
              </label>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
                {METODOS_PAGO_RD.map((metodo) => (
                  <button
                    key={metodo}
                    onClick={() => setMetodoPago(metodo as MetodoPago)}
                    disabled={confirmacionPago}
                    className={`p-3 rounded-lg border text-sm transition-colors ${
                      metodoPago === metodo
                        ? 'bg-dominican-blue text-white border-dominican-blue'
                        : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                    } ${confirmacionPago ? 'opacity-50 cursor-not-allowed' : ''}`}
                  >
                    {metodo}
                  </button>
                ))}
              </div>
              {errores.metodoPago && (
                <p className="text-red-600 text-sm mt-1">{errores.metodoPago}</p>
              )}
            </div>

            {/* Monto pagado */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Monto Pagado (RD$) *
              </label>
              <div className="flex space-x-2">
                <Input
                  type="number"
                  value={montoPagado}
                  onChange={(e) => handleMontoChange(e.target.value)}
                  placeholder="0.00"
                  min="0"
                  step="0.01"
                  disabled={confirmacionPago}
                  className="flex-1"
                />
                <Button
                  type="button"
                  variant="outline"
                  onClick={aplicarMontoExacto}
                  disabled={confirmacionPago}
                  className="whitespace-nowrap"
                >
                  Monto Exacto
                </Button>
              </div>
              {errores.montoPagado && (
                <p className="text-red-600 text-sm mt-1">{errores.montoPagado}</p>
              )}
            </div>

            {/* Vuelto */}
            {vuelto > 0 && (
              <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
                <div className="flex items-center justify-between">
                  <span className="font-medium text-green-800">Vuelto:</span>
                  <span className="text-lg font-bold text-green-800">
                    {formatearPrecio(vuelto)}
                  </span>
                </div>
              </div>
            )}

            {/* Observaciones */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Observaciones de Pago
              </label>
              <textarea
                value={observacionesPago}
                onChange={(e) => setObservacionesPago(e.target.value)}
                placeholder="Notas adicionales sobre el pago..."
                disabled={confirmacionPago}
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent resize-none"
                rows={3}
              />
            </div>

            {/* Confirmación */}
            {confirmacionPago && (
              <div className="p-4 bg-amber-50 border border-amber-200 rounded-lg">
                <div className="flex items-start space-x-3">
                  <AlertCircle className="w-5 h-5 text-amber-600 mt-0.5" />
                  <div>
                    <h4 className="font-medium text-amber-800">Confirmar Procesamiento de Pago</h4>
                    <p className="text-sm text-amber-700 mt-1">
                      ¿Está seguro que desea procesar el pago de{' '}
                      <span className="font-medium">{formatearPrecio(montoPagado)}</span> por{' '}
                      {metodoPago}?
                    </p>
                    {vuelto > 0 && (
                      <p className="text-sm text-amber-700 mt-1">
                        Vuelto a entregar:{' '}
                        <span className="font-medium">{formatearPrecio(vuelto)}</span>
                      </p>
                    )}
                  </div>
                </div>
              </div>
            )}

            {/* Errores generales */}
            {errores.estado && (
              <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
                <div className="flex items-center space-x-2">
                  <AlertCircle className="w-5 h-5 text-red-600" />
                  <p className="text-red-800">{errores.estado}</p>
                </div>
              </div>
            )}
          </div>
        </Card>
      )}

      {/* Información de pago existente */}
      {readonly ||
        (factura.estado === 'Pagada' && (
          <Card className="p-6">
            <h4 className="font-medium text-gray-900 mb-4 flex items-center">
              <Check className="w-5 h-5 mr-2 text-green-600" />
              Información de Pago
            </h4>

            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Método de Pago:</span>
                <Badge variant="secondary">{factura.metodoPago}</Badge>
              </div>

              <div className="flex justify-between">
                <span className="text-gray-600">Monto Pagado:</span>
                <span className="font-medium">{formatearPrecio(factura.total)}</span>
              </div>

              {factura.fechaPago && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Fecha de Pago:</span>
                  <span className="font-medium">
                    {new Date(factura.fechaPago).toLocaleString('es-DO')}
                  </span>
                </div>
              )}

              {factura.observacionesPago && (
                <div>
                  <span className="text-gray-600">Observaciones:</span>
                  <p className="mt-1 text-sm text-gray-800 bg-gray-50 p-2 rounded">
                    {factura.observacionesPago}
                  </p>
                </div>
              )}
            </div>
          </Card>
        ))}

      {/* Botones de acción */}
      <div className="flex justify-end space-x-3">
        <Button variant="outline" onClick={handleCancelar} disabled={loading}>
          <X className="w-4 h-4 mr-2" />
          {confirmacionPago ? 'Volver' : 'Cancelar'}
        </Button>

        {!readonly && factura.estado === 'Pendiente' && (
          <Button
            onClick={handleProcesarPago}
            disabled={loading || !validarPago()}
            className="bg-dominican-blue hover:bg-dominican-blue/90"
          >
            {loading ? (
              <>
                <LoadingSpinner size="sm" className="mr-2" />
                Procesando...
              </>
            ) : confirmacionPago ? (
              <>
                <Check className="w-4 h-4 mr-2" />
                Confirmar Pago
              </>
            ) : (
              <>
                <DollarSign className="w-4 h-4 mr-2" />
                Procesar Pago
              </>
            )}
          </Button>
        )}
      </div>
    </div>
  );
};
