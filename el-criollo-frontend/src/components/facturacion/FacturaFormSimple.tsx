import React, { useState, useEffect } from 'react';
import { Receipt, Calculator, AlertCircle, Check, X, Split } from 'lucide-react';
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
import type { Orden, CrearFacturaRequest, Factura, MetodoPago } from '@/types';

// Utils
import { formatearPrecio, calcularITBIS, METODOS_PAGO_RD } from '@/utils/dominicanValidations';

interface FacturaFormProps {
  orden?: Orden;
  onFacturaCreada?: (factura: Factura) => void;
  onClose?: () => void;
  mostrarBotonDivision?: boolean;
  onAbrirDivision?: () => void;
}

export const FacturaFormSimple: React.FC<FacturaFormProps> = ({
  orden,
  onFacturaCreada,
  onClose,
  mostrarBotonDivision = true,
  onAbrirDivision,
}) => {
  // Estados principales
  const [loading, setLoading] = useState(false);
  const [metodoPago, setMetodoPago] = useState<MetodoPago>('Efectivo');
  const [descuento, setDescuento] = useState<number>(0);
  const [propina, setPropina] = useState<number>(0);
  const [observacionesPago, setObservacionesPago] = useState<string>('');

  // Estados calculados
  const [subtotal, setSubtotal] = useState<number>(0);
  const [impuesto, setImpuesto] = useState<number>(0);
  const [total, setTotal] = useState<number>(0);

  // Estados de validación
  const [errores, setErrores] = useState<Record<string, string>>({});

  // ============================================================================
  // EFECTOS
  // ============================================================================

  useEffect(() => {
    if (orden) {
      calcularTotales();
    }
  }, [orden, descuento, propina]);

  // ============================================================================
  // FUNCIONES DE CÁLCULO
  // ============================================================================

  const calcularTotales = () => {
    if (!orden) return;

    const subtotalBase = orden.subtotalCalculado - descuento;
    const impuestoCalculado = calcularITBIS(subtotalBase);
    const totalCalculado = subtotalBase + impuestoCalculado + propina;

    setSubtotal(subtotalBase);
    setImpuesto(impuestoCalculado);
    setTotal(totalCalculado);
  };

  // ============================================================================
  // VALIDACIONES
  // ============================================================================

  const validarFormulario = (): boolean => {
    const nuevosErrores: Record<string, string> = {};

    if (!orden) {
      nuevosErrores.orden = 'Debe seleccionar una orden válida';
    }

    if (!metodoPago) {
      nuevosErrores.metodoPago = 'Debe seleccionar un método de pago';
    }

    if (descuento < 0) {
      nuevosErrores.descuento = 'El descuento no puede ser negativo';
    }

    if (descuento > (orden?.subtotalCalculado || 0)) {
      nuevosErrores.descuento = 'El descuento no puede ser mayor al subtotal';
    }

    if (propina < 0) {
      nuevosErrores.propina = 'La propina no puede ser negativa';
    }

    setErrores(nuevosErrores);
    return Object.keys(nuevosErrores).length === 0;
  };

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const handleCrearFactura = async () => {
    if (!validarFormulario() || !orden) return;

    try {
      setLoading(true);

      const facturaRequest: CrearFacturaRequest = {
        ordenID: orden.ordenID,
        metodoPago,
        descuento: descuento || 0,
        propina: propina || 0,
        observacionesPago: observacionesPago || undefined,
      };

      const facturaCreada = await facturaService.crearFactura(facturaRequest);

      toast.success(`Factura ${facturaCreada.numeroFactura} creada exitosamente`);

      if (onFacturaCreada) {
        onFacturaCreada(facturaCreada);
      }

      if (onClose) {
        onClose();
      }
    } catch (error: any) {
      console.error('Error creando factura:', error);
      toast.error(error.message || 'Error al crear la factura');
    } finally {
      setLoading(false);
    }
  };

  const handleDescuentoChange = (value: string) => {
    const numero = parseFloat(value) || 0;
    setDescuento(numero);
  };

  const handlePropinaChange = (value: string) => {
    const numero = parseFloat(value) || 0;
    setPropina(numero);
  };

  const aplicarPorcentajePropina = (porcentaje: number) => {
    if (!orden) return;
    const propinaCalculada = (orden.subtotalCalculado * porcentaje) / 100;
    setPropina(Math.round(propinaCalculada * 100) / 100);
  };

  // ============================================================================
  // RENDER
  // ============================================================================

  if (!orden) {
    return (
      <Card className="p-6 text-center">
        <AlertCircle className="w-16 h-16 mx-auto text-amber-500 mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">Orden no seleccionada</h3>
        <p className="text-gray-600">Debe seleccionar una orden para poder facturar</p>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header de la factura */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-dominican-blue bg-opacity-10 rounded-lg">
            <Receipt className="w-6 h-6 text-dominican-blue" />
          </div>
          <div>
            <h3 className="text-lg font-bold text-gray-900">Crear Factura</h3>
            <p className="text-sm text-gray-600">
              Orden #{orden.numeroOrden} • {orden.detalles?.length || 0} items
            </p>
          </div>
        </div>

        <div className="text-right">
          <div className="text-sm text-gray-600">Mesa</div>
          <div className="text-xl font-bold text-dominican-blue">
            {orden.mesa?.numeroMesa || 'N/A'}
          </div>
        </div>
      </div>

      {/* Información de la orden */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <div className="text-sm text-gray-600">Cliente</div>
            <div className="font-medium">
              {orden.cliente?.nombreCompleto || 'Cliente ocasional'}
            </div>
          </div>
          <div>
            <div className="text-sm text-gray-600">Estado</div>
            <Badge variant="success">{orden.estado}</Badge>
          </div>
          <div>
            <div className="text-sm text-gray-600">Tipo</div>
            <div className="font-medium">{orden.tipoOrden}</div>
          </div>
        </div>
      </Card>

      {/* Configuración de pago */}
      <Card className="p-6">
        <h4 className="font-medium text-gray-900 mb-4">Configuración de Pago</h4>

        <div className="space-y-4">
          {/* Método de pago */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Método de Pago</label>
            <select
              value={metodoPago}
              onChange={(e) => setMetodoPago(e.target.value as MetodoPago)}
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
            >
              {METODOS_PAGO_RD.map((metodo) => (
                <option key={metodo} value={metodo}>
                  {metodo}
                </option>
              ))}
            </select>
            {errores.metodoPago && (
              <p className="text-red-600 text-sm mt-1">{errores.metodoPago}</p>
            )}
          </div>

          {/* Descuento */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Descuento (RD$)</label>
            <Input
              type="number"
              min="0"
              max={orden.subtotalCalculado}
              step="0.01"
              value={descuento}
              onChange={(e) => handleDescuentoChange(e.target.value)}
              placeholder="0.00"
            />
            {errores.descuento && <p className="text-red-600 text-sm mt-1">{errores.descuento}</p>}
          </div>

          {/* Propina */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Propina (RD$)</label>
            <div className="space-y-2">
              <Input
                type="number"
                min="0"
                step="0.01"
                value={propina}
                onChange={(e) => handlePropinaChange(e.target.value)}
                placeholder="0.00"
              />

              {/* Botones de porcentaje rápido */}
              <div className="flex space-x-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => aplicarPorcentajePropina(10)}
                >
                  10%
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => aplicarPorcentajePropina(15)}
                >
                  15%
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => aplicarPorcentajePropina(20)}
                >
                  20%
                </Button>
              </div>
            </div>
            {errores.propina && <p className="text-red-600 text-sm mt-1">{errores.propina}</p>}
          </div>

          {/* Observaciones */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Observaciones de Pago
            </label>
            <textarea
              value={observacionesPago}
              onChange={(e) => setObservacionesPago(e.target.value)}
              placeholder="Observaciones adicionales..."
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
              rows={3}
            />
          </div>
        </div>
      </Card>

      {/* Resumen de totales */}
      <Card className="p-6">
        <h4 className="font-medium text-gray-900 mb-4 flex items-center">
          <Calculator className="w-5 h-5 mr-2" />
          Resumen de Totales
        </h4>

        <div className="space-y-3">
          <div className="flex justify-between">
            <span className="text-gray-600">Subtotal original:</span>
            <span>{formatearPrecio(orden.subtotalCalculado)}</span>
          </div>

          {descuento > 0 && (
            <div className="flex justify-between text-red-600">
              <span>Descuento:</span>
              <span>-{formatearPrecio(descuento)}</span>
            </div>
          )}

          <div className="flex justify-between">
            <span className="text-gray-600">Subtotal con descuento:</span>
            <span>{formatearPrecio(subtotal)}</span>
          </div>

          <div className="flex justify-between">
            <span className="text-gray-600">ITBIS (18%):</span>
            <span>{formatearPrecio(impuesto)}</span>
          </div>

          {propina > 0 && (
            <div className="flex justify-between text-green-600">
              <span>Propina:</span>
              <span>+{formatearPrecio(propina)}</span>
            </div>
          )}

          <div className="flex justify-between text-lg font-bold border-t pt-3">
            <span>Total a pagar:</span>
            <span className="text-dominican-blue">{formatearPrecio(total)}</span>
          </div>
        </div>
      </Card>

      {/* Botones de acción */}
      <div className="flex justify-end space-x-3">
        <Button variant="outline" onClick={onClose} disabled={loading}>
          <X className="w-4 h-4 mr-2" />
          Cancelar
        </Button>

        {mostrarBotonDivision && (
          <Button
            variant="outline"
            onClick={onAbrirDivision}
            disabled={loading}
            className="border-dominican-blue text-dominican-blue hover:bg-dominican-blue hover:text-white"
          >
            <Split className="w-4 h-4 mr-2" />
            Dividir Factura
          </Button>
        )}

        <Button
          onClick={handleCrearFactura}
          disabled={loading || !validarFormulario()}
          className="bg-dominican-blue hover:bg-dominican-blue/90"
        >
          {loading ? (
            <>
              <LoadingSpinner size="sm" className="mr-2" />
              Creando...
            </>
          ) : (
            <>
              <Check className="w-4 h-4 mr-2" />
              Crear Factura
            </>
          )}
        </Button>
      </div>
    </div>
  );
};
