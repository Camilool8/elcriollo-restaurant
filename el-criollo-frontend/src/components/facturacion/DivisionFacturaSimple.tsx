import React, { useState } from 'react';
import { Users, UserPlus, Check, X, AlertCircle, Split } from 'lucide-react';
import { toast } from 'react-toastify';

// Components
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

// Types
import type {
  Orden,
  DetalleOrden,
  MetodoPago,
  DivisionFacturaRequest,
  DivisionCliente,
  ClienteOcasional,
} from '@/types';

// Utils
import { formatearPrecio, METODOS_PAGO_RD } from '@/utils/dominicanValidations';

interface DivisionFacturaProps {
  orden: Orden;
  onFacturaDividida: (request: DivisionFacturaRequest) => void;
  onClose?: () => void;
}

interface ClienteSimple {
  id: string;
  nombre: string;
  documento?: string;
  metodoPago: MetodoPago;
  itemsAsignados: number[];
  subtotal: number;
}

export const DivisionFacturaSimple: React.FC<DivisionFacturaProps> = ({
  orden,
  onFacturaDividida,
  onClose,
}) => {
  // Estados principales
  const [loading, setLoading] = useState(false);
  const [clientes, setClientes] = useState<ClienteSimple[]>([]);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [nuevoClienteNombre, setNuevoClienteNombre] = useState('');
  const [nuevoClienteDocumento, setNuevoClienteDocumento] = useState('');
  const [nuevoClienteMetodoPago, setNuevoClienteMetodoPago] = useState<MetodoPago>('Efectivo');

  // Estados de validación
  const [errores, setErrores] = useState<Record<string, string>>({});

  // ============================================================================
  // FUNCIONES AUXILIARES
  // ============================================================================

  const calcularSubtotalCliente = (itemsAsignados: number[]): number => {
    if (!orden.detalles) return 0;
    return itemsAsignados.reduce((total, itemId) => {
      const item = orden.detalles!.find((i: DetalleOrden) => i.detalleOrdenID === itemId);
      return total + (item ? parseFloat(item.subtotal) : 0);
    }, 0);
  };

  const obtenerItemsDisponibles = (): DetalleOrden[] => {
    if (!orden.detalles) return [];
    const itemsAsignados = clientes.flatMap((c) => c.itemsAsignados);
    return orden.detalles.filter(
      (item: DetalleOrden) => !itemsAsignados.includes(item.detalleOrdenID)
    );
  };

  // ============================================================================
  // VALIDACIONES
  // ============================================================================

  const validarDivision = (): boolean => {
    const nuevosErrores: Record<string, string> = {};

    if (clientes.length === 0) {
      nuevosErrores.clientes = 'Debe agregar al menos un cliente';
    }

    const itemsDisponibles = obtenerItemsDisponibles();
    if (itemsDisponibles.length > 0) {
      nuevosErrores.items = 'Todos los items deben estar asignados a un cliente';
    }

    // Validar que cada cliente tenga al menos un item
    clientes.forEach((cliente, index) => {
      if (cliente.itemsAsignados.length === 0) {
        nuevosErrores[`cliente_${index}`] = 'El cliente debe tener al menos un item asignado';
      }
    });

    setErrores(nuevosErrores);
    return Object.keys(nuevosErrores).length === 0;
  };

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const agregarCliente = () => {
    if (!nuevoClienteNombre.trim()) {
      toast.error('El nombre es requerido');
      return;
    }

    const nuevoCliente: ClienteSimple = {
      id: `cliente_${Date.now()}`,
      nombre: nuevoClienteNombre.trim(),
      documento: nuevoClienteDocumento.trim() || undefined,
      metodoPago: nuevoClienteMetodoPago,
      itemsAsignados: [],
      subtotal: 0,
    };

    setClientes([...clientes, nuevoCliente]);
    setNuevoClienteNombre('');
    setNuevoClienteDocumento('');
    setNuevoClienteMetodoPago('Efectivo');
    setMostrarFormulario(false);
  };

  const eliminarCliente = (clienteId: string) => {
    setClientes(clientes.filter((c) => c.id !== clienteId));
  };

  const asignarItemACliente = (itemId: number, clienteId: string) => {
    setClientes(
      clientes.map((cliente) => {
        if (cliente.id === clienteId) {
          const nuevosItems = [...cliente.itemsAsignados, itemId];
          return {
            ...cliente,
            itemsAsignados: nuevosItems,
            subtotal: calcularSubtotalCliente(nuevosItems),
          };
        }
        return cliente;
      })
    );
  };

  const desasignarItemDeCliente = (itemId: number, clienteId: string) => {
    setClientes(
      clientes.map((cliente) => {
        if (cliente.id === clienteId) {
          const nuevosItems = cliente.itemsAsignados.filter((id) => id !== itemId);
          return {
            ...cliente,
            itemsAsignados: nuevosItems,
            subtotal: calcularSubtotalCliente(nuevosItems),
          };
        }
        return cliente;
      })
    );
  };

  const procesarDivision = async () => {
    if (!validarDivision()) return;

    try {
      setLoading(true);

      const request: DivisionFacturaRequest = {
        ordenID: orden.ordenID,
        divisiones: clientes.map(
          (cliente): DivisionCliente => ({
            cliente: {
              nombre: cliente.nombre,
              documento: cliente.documento,
              esOcasional: true,
            } as ClienteOcasional,
            itemsAsignados: cliente.itemsAsignados,
            metodoPago: cliente.metodoPago,
          })
        ),
      };

      if (onFacturaDividida) {
        onFacturaDividida(request);
      }
    } catch (error: any) {
      console.error('Error dividiendo factura:', error);
      toast.error('Error al procesar la división de la factura');
    } finally {
      setLoading(false);
    }
  };

  // ============================================================================
  // COMPONENTE DE ITEM
  // ============================================================================

  const ItemCard: React.FC<{
    item: DetalleOrden;
    asignadoA?: string;
    onAsignar?: (clienteId: string) => void;
    onDesasignar?: () => void;
  }> = ({ item, asignadoA, onAsignar, onDesasignar }) => {
    return (
      <div className="p-3 border rounded-lg bg-gray-50">
        <div className="flex justify-between items-start mb-2">
          <div className="flex-1">
            <h4 className="font-medium text-gray-900">{item.producto?.nombre || 'Producto'}</h4>
            <p className="text-sm text-gray-600">
              Cantidad: {item.cantidad} • {formatearPrecio(parseFloat(item.subtotal))}
            </p>
          </div>

          <div className="flex flex-col items-end space-y-1">
            {asignadoA && (
              <Badge variant="success" className="text-xs">
                {asignadoA}
              </Badge>
            )}

            {onDesasignar && (
              <button onClick={onDesasignar} className="text-red-500 hover:text-red-700 text-xs">
                <X className="w-3 h-3" />
              </button>
            )}
          </div>
        </div>

        {onAsignar && (
          <div className="flex flex-wrap gap-1">
            {clientes.map((cliente) => (
              <button
                key={cliente.id}
                onClick={() => onAsignar(cliente.id)}
                className="px-2 py-1 text-xs bg-dominican-blue text-white rounded hover:bg-dominican-blue/90"
              >
                {cliente.nombre}
              </button>
            ))}
          </div>
        )}
      </div>
    );
  };

  // ============================================================================
  // RENDER
  // ============================================================================

  const itemsDisponibles = obtenerItemsDisponibles();
  const totalDividido = clientes.reduce((sum, c) => sum + c.subtotal, 0);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-dominican-blue bg-opacity-10 rounded-lg">
            <Split className="w-6 h-6 text-dominican-blue" />
          </div>
          <div>
            <h3 className="text-lg font-bold text-gray-900">División de Factura</h3>
            <p className="text-sm text-gray-600">
              Orden #{orden.numeroOrden} • {orden.detalles?.length || 0} items
            </p>
          </div>
        </div>

        <div className="text-right">
          <div className="text-sm text-gray-600">Total Original</div>
          <div className="text-xl font-bold text-dominican-blue">
            {formatearPrecio(orden.subtotalCalculado)}
          </div>
        </div>
      </div>

      {/* Resumen estadístico */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="text-center">
            <div className="text-2xl font-bold text-dominican-blue">{clientes.length}</div>
            <div className="text-sm text-gray-600">Clientes</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-green-600">
              {formatearPrecio(totalDividido)}
            </div>
            <div className="text-sm text-gray-600">Total Dividido</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-amber-600">{itemsDisponibles.length}</div>
            <div className="text-sm text-gray-600">Items Sin Asignar</div>
          </div>
        </div>
      </Card>

      {/* Gestión de clientes */}
      <Card className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h4 className="font-medium text-gray-900">Clientes</h4>
          <Button
            onClick={() => setMostrarFormulario(true)}
            size="sm"
            className="bg-dominican-blue hover:bg-dominican-blue/90"
          >
            <UserPlus className="w-4 h-4 mr-2" />
            Agregar Cliente
          </Button>
        </div>

        {/* Formulario de nuevo cliente */}
        {mostrarFormulario && (
          <div className="mb-4 p-4 border rounded-lg bg-gray-50">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Input
                placeholder="Nombre del cliente"
                value={nuevoClienteNombre}
                onChange={(e) => setNuevoClienteNombre(e.target.value)}
              />
              <Input
                placeholder="Documento (opcional)"
                value={nuevoClienteDocumento}
                onChange={(e) => setNuevoClienteDocumento(e.target.value)}
              />
              <select
                value={nuevoClienteMetodoPago}
                onChange={(e) => setNuevoClienteMetodoPago(e.target.value as MetodoPago)}
                className="p-2 border border-gray-300 rounded-lg"
              >
                {METODOS_PAGO_RD.map((metodo) => (
                  <option key={metodo} value={metodo}>
                    {metodo}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex justify-end space-x-2 mt-4">
              <Button variant="outline" onClick={() => setMostrarFormulario(false)} size="sm">
                Cancelar
              </Button>
              <Button
                onClick={agregarCliente}
                size="sm"
                className="bg-dominican-blue hover:bg-dominican-blue/90"
              >
                Agregar
              </Button>
            </div>
          </div>
        )}

        {/* Lista de clientes */}
        {clientes.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <Users className="w-12 h-12 mx-auto mb-4 text-gray-400" />
            <p>No hay clientes agregados</p>
            <p className="text-sm">Agrega clientes para dividir la factura</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {clientes.map((cliente) => (
              <div key={cliente.id} className="p-4 border rounded-lg">
                <div className="flex items-center justify-between mb-3">
                  <div>
                    <span className="font-medium">{cliente.nombre}</span>
                    {cliente.documento && (
                      <span className="text-sm text-gray-600 ml-2">({cliente.documento})</span>
                    )}
                  </div>
                  <button
                    onClick={() => eliminarCliente(cliente.id)}
                    className="text-red-500 hover:text-red-700"
                  >
                    <X className="w-4 h-4" />
                  </button>
                </div>

                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span>Items:</span>
                    <span>{cliente.itemsAsignados.length}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Subtotal:</span>
                    <span className="font-medium">{formatearPrecio(cliente.subtotal)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Método:</span>
                    <Badge variant="secondary" className="text-xs">
                      {cliente.metodoPago}
                    </Badge>
                  </div>
                </div>

                {/* Items asignados */}
                {cliente.itemsAsignados.length > 0 && (
                  <div className="mt-3 pt-3 border-t">
                    <div className="text-xs text-gray-600 mb-2">Items asignados:</div>
                    <div className="space-y-1">
                      {cliente.itemsAsignados.map((itemId) => {
                        const item = orden.detalles?.find(
                          (i: DetalleOrden) => i.detalleOrdenID === itemId
                        );
                        return item ? (
                          <ItemCard
                            key={itemId}
                            item={item}
                            asignadoA={cliente.nombre}
                            onDesasignar={() => desasignarItemDeCliente(itemId, cliente.id)}
                          />
                        ) : null;
                      })}
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}

        {errores.clientes && <p className="text-red-600 text-sm mt-2">{errores.clientes}</p>}
      </Card>

      {/* Items no asignados */}
      {itemsDisponibles.length > 0 && (
        <Card className="p-6">
          <h4 className="font-medium text-gray-900 mb-4 flex items-center">
            <AlertCircle className="w-5 h-5 mr-2 text-amber-500" />
            Items Sin Asignar ({itemsDisponibles.length})
          </h4>

          <div className="space-y-3">
            {itemsDisponibles.map((item) => (
              <ItemCard
                key={item.detalleOrdenID}
                item={item}
                onAsignar={(clienteId) => asignarItemACliente(item.detalleOrdenID, clienteId)}
              />
            ))}
          </div>

          {errores.items && <p className="text-red-600 text-sm mt-2">{errores.items}</p>}
        </Card>
      )}

      {/* Botones de acción */}
      <div className="flex justify-end space-x-3">
        <Button variant="outline" onClick={onClose} disabled={loading}>
          <X className="w-4 h-4 mr-2" />
          Cancelar
        </Button>

        <Button
          onClick={procesarDivision}
          disabled={loading || !validarDivision()}
          className="bg-dominican-blue hover:bg-dominican-blue/90"
        >
          {loading ? (
            <>
              <LoadingSpinner size="sm" className="mr-2" />
              Procesando...
            </>
          ) : (
            <>
              <Check className="w-4 h-4 mr-2" />
              Dividir Factura
            </>
          )}
        </Button>
      </div>
    </div>
  );
};
