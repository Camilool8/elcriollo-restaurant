import React, { useState, useMemo } from 'react';
import {
  ShoppingCart,
  Plus,
  Minus,
  Trash2,
  Users,
  Calculator,
  AlertCircle,
  Check,
  X,
} from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import type {
  Carrito,
  CrearOrdenRequest,
  ItemOrdenRequest,
  ClienteOcasionalRequest,
} from '@/types/orden';
import type { Producto, Mesa, Cliente } from '@/types';
import { parsePrice } from '@/utils/priceUtils';

interface CarritoComprasProps {
  carrito: Carrito;
  productos: Producto[];
  mesas: Mesa[];
  clientes: Cliente[];
  loading?: boolean;
  onAgregarProducto: (producto: Producto, cantidad?: number) => void;
  onActualizarCantidad: (productoId: number, nuevaCantidad: number) => void;
  onEliminarProducto: (productoId: number) => void;
  onSeleccionarCliente: (cliente: Cliente | null) => void;
  onActualizarObservaciones: (observaciones: string) => void;
  onConfirmarOrden: (request: CrearOrdenRequest) => Promise<void>;
  onLimpiarCarrito: () => void;
}

export const CarritoCompras: React.FC<CarritoComprasProps> = ({
  carrito,
  mesas,
  clientes,
  onActualizarCantidad,
  onEliminarProducto,
  onSeleccionarCliente,
  onActualizarObservaciones,
  onConfirmarOrden,
  onLimpiarCarrito,
}) => {
  const [clienteOcasional, setClienteOcasional] = useState<ClienteOcasionalRequest>({
    nombreCompleto: '',
  });
  const [usarClienteOcasional, setUsarClienteOcasional] = useState(false);
  const [procesandoOrden, setProcesandoOrden] = useState(false);

  const itemsAgrupados = useMemo(() => {
    return carrito.items.reduce(
      (acc, item) => {
        const categoria = item.producto.categoria?.nombreCategoria || 'Sin Categoría';
        if (!acc[categoria]) {
          acc[categoria] = [];
        }
        acc[categoria].push(item);
        return acc;
      },
      {} as Record<string, typeof carrito.items>
    );
  }, [carrito.items]);

  // Validaciones
  const puedeConfirmarOrden = () => {
    if (carrito.items.length === 0) return false;
    if (carrito.tipoOrden === 'Mesa' && !carrito.mesaSeleccionada) return false;
    return true;
  };

  const handleConfirmarOrden = async () => {
    if (!puedeConfirmarOrden()) return;

    try {
      setProcesandoOrden(true);

      const items: ItemOrdenRequest[] = carrito.items.map((item) => ({
        productoId: item.producto.productoID,
        cantidad: item.cantidad,
        notasEspeciales: item.notasEspeciales,
      }));

      const request: CrearOrdenRequest = {
        tipoOrden: 'Mesa',
        items,
        observaciones: carrito.observacionesGenerales,
      };

      // Agregar mesa si es orden de mesa
      if (carrito.tipoOrden === 'Mesa' && carrito.mesaSeleccionada) {
        request.mesaID = carrito.mesaSeleccionada.mesaID;
      }

      // Agregar cliente si está seleccionado
      if (carrito.clienteSeleccionado) {
        request.clienteID = carrito.clienteSeleccionado.clienteID;
      } else if (usarClienteOcasional && clienteOcasional.nombreCompleto.trim()) {
        request.clienteOcasional = clienteOcasional;
      }

      await onConfirmarOrden(request);

      // Limpiar formulario después del éxito
      setClienteOcasional({ nombreCompleto: '' });
      setUsarClienteOcasional(false);
    } catch (error) {
      console.error('Error confirmando orden:', error);
    } finally {
      setProcesandoOrden(false);
    }
  };

  if (carrito.items.length === 0) {
    return (
      <Card className="p-6 text-center">
        <ShoppingCart className="w-16 h-16 mx-auto text-gray-400 mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">Carrito vacío</h3>
        <p className="text-gray-600">Agrega productos para crear una orden</p>
      </Card>
    );
  }

  return (
    <Card className="p-6 space-y-6">
      {/* Header del carrito */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <ShoppingCart className="w-6 h-6 text-dominican-blue" />
          <h3 className="text-lg font-bold text-gray-900">Nueva Orden</h3>
          <Badge variant="secondary">{carrito.items.length} items</Badge>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={onLimpiarCarrito}
          className="text-red-600 hover:text-red-700 hover:bg-red-50"
        >
          <Trash2 className="w-4 h-4 mr-1" />
          Limpiar
        </Button>
      </div>

      {/* Items del carrito agrupados por categoría */}
      <div className="space-y-4">
        {Object.entries(itemsAgrupados).map(([categoria, items]) => (
          <div key={categoria}>
            <h4 className="text-sm font-semibold text-gray-500 mb-2 border-b pb-1">{categoria}</h4>
            <div className="space-y-3">
              {items.map((item) => {
                const precioNumerico = parsePrice(item.producto.precio);
                return (
                  <div key={item.producto.productoID} className="flex items-center space-x-3">
                    {/* Detalles del producto */}
                    <div className="flex-1">
                      <div className="font-medium text-gray-900">{item.producto.nombre}</div>
                      <div className="text-sm text-gray-500">
                        {`RD$ ${precioNumerico.toFixed(2)}`}
                      </div>
                    </div>

                    {/* Controles de cantidad */}
                    <div className="flex items-center space-x-2">
                      <Button
                        variant="outline"
                        size="icon"
                        onClick={() =>
                          onActualizarCantidad(item.producto.productoID, item.cantidad - 1)
                        }
                        className="w-8 h-8"
                      >
                        <Minus className="w-4 h-4" />
                      </Button>
                      <Input
                        type="number"
                        value={item.cantidad}
                        onChange={(e) =>
                          onActualizarCantidad(
                            item.producto.productoID,
                            parseInt(e.target.value, 10) || 1
                          )
                        }
                        className="w-14 text-center"
                      />
                      <Button
                        variant="outline"
                        size="icon"
                        onClick={() =>
                          onActualizarCantidad(item.producto.productoID, item.cantidad + 1)
                        }
                        className="w-8 h-8"
                      >
                        <Plus className="w-4 h-4" />
                      </Button>
                    </div>

                    {/* Subtotal y botón de eliminar */}
                    <div className="w-24 text-right">
                      <div className="font-medium text-gray-800">
                        {`RD$ ${(precioNumerico * item.cantidad).toFixed(2)}`}
                      </div>
                    </div>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => onEliminarProducto(item.producto.productoID)}
                      className="text-gray-400 hover:text-red-600 w-8 h-8"
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                );
              })}
            </div>
          </div>
        ))}
      </div>

      {/* Selección de cliente */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Cliente</label>
        <select
          value={carrito.clienteSeleccionado?.clienteID ?? 'anonimo'}
          onChange={(e) => {
            const value = e.target.value;
            if (value === 'anonimo') {
              onSeleccionarCliente(null);
            } else {
              const cliente = clientes.find((c) => c.clienteID === parseInt(value));
              onSeleccionarCliente(cliente || null);
            }
          }}
          className="w-full p-3 border border-gray-300 rounded-lg"
        >
          <option value="anonimo">Cliente Anónimo</option>
          {clientes.map((cliente) => (
            <option key={cliente.clienteID} value={cliente.clienteID}>
              {cliente.nombreCompleto}
            </option>
          ))}
        </select>
      </div>

      {/* Observaciones */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Observaciones generales
        </label>
        <textarea
          value={carrito.observacionesGenerales || ''}
          onChange={(e) => onActualizarObservaciones(e.target.value)}
          placeholder="Notas especiales para la orden..."
          className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent resize-none"
          rows={3}
        />
      </div>

      {/* Resumen de totales */}
      <div className="border-t pt-4">
        <h4 className="font-medium text-gray-900 mb-2 flex items-center">
          <Calculator className="w-5 h-5 mr-2" />
          Resumen
        </h4>
        <div className="space-y-2">
          <div className="flex justify-between">
            <span className="text-gray-600">Subtotal:</span>
            <span className="font-medium">{`RD$ ${carrito.subtotal.toFixed(2)}`}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">ITBIS (18%):</span>
            <span className="font-medium">{`RD$ ${carrito.impuesto.toFixed(2)}`}</span>
          </div>
          <div className="flex justify-between text-lg font-bold">
            <span>Total:</span>
            <span className="text-dominican-blue">{`RD$ ${carrito.total.toFixed(2)}`}</span>
          </div>
        </div>
      </div>

      {/* Validaciones */}
      {!puedeConfirmarOrden() && (
        <div className="flex items-center space-x-2 p-3 bg-amber-50 border border-amber-200 rounded-lg">
          <AlertCircle className="w-5 h-5 text-amber-600" />
          <div className="text-sm text-amber-800">
            {carrito.items.length === 0 && 'Agrega al menos un producto'}
            {carrito.tipoOrden === 'Mesa' && !carrito.mesaSeleccionada && 'Selecciona una mesa'}
          </div>
        </div>
      )}

      {/* Botones de acción */}
      <div className="flex space-x-3">
        <Button
          onClick={handleConfirmarOrden}
          disabled={!puedeConfirmarOrden() || procesandoOrden}
          className="flex-1 bg-dominican-blue hover:bg-dominican-blue/90"
        >
          {procesandoOrden ? (
            <>
              <Calculator className="w-4 h-4 mr-2 animate-spin" />
              Procesando...
            </>
          ) : (
            <>
              <Check className="w-4 h-4 mr-2" />
              Confirmar Orden
            </>
          )}
        </Button>

        <Button
          variant="outline"
          onClick={onLimpiarCarrito}
          className="text-gray-600 hover:text-gray-700"
        >
          <X className="w-4 h-4 mr-2" />
          Cancelar
        </Button>
      </div>
    </Card>
  );
};
