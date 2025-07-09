import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { toast } from 'react-toastify';
import { Button } from '@/components/ui/Button';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import { Input } from '@/components/ui/Input';
import {
  Search,
  PlusCircle,
  ChevronDown,
  Minus,
  Plus,
  Trash2,
  Calculator,
  Edit,
} from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { ordenesService } from '@/services/ordenesService';
import { productosService } from '@/services/productosService';
import { clienteService } from '@/services/clienteService';

import type {
  ActualizarOrdenRequest,
  Carrito,
  Orden,
  Producto,
  ItemCarrito,
  Cliente,
} from '@/types';
import { AlertTriangle, ChefHat } from 'lucide-react';

// Componente para búsqueda de productos
interface PanelBusquedaProductosProps {
  productos: Producto[];
  onAgregarProducto: (producto: Producto) => void;
}

const PanelBusquedaProductos: React.FC<PanelBusquedaProductosProps> = ({
  productos,
  onAgregarProducto,
}) => {
  const [terminoBusqueda, setTerminoBusqueda] = useState('');
  const [openCategories, setOpenCategories] = useState<Record<string, boolean>>({});

  const productosFiltrados = useMemo(() => {
    return productos.filter((p) => p.nombre.toLowerCase().includes(terminoBusqueda.toLowerCase()));
  }, [productos, terminoBusqueda]);

  const productosAgrupados = useMemo(() => {
    return productosFiltrados.reduce(
      (acc, producto) => {
        const categoria = producto.categoria?.nombreCategoria || 'Sin Categoría';
        if (!acc[categoria]) {
          acc[categoria] = [];
        }
        acc[categoria].push(producto);
        return acc;
      },
      {} as Record<string, Producto[]>
    );
  }, [productosFiltrados]);

  useEffect(() => {
    const initialOpenState = Object.keys(productosAgrupados).reduce(
      (acc, categoria) => {
        acc[categoria] = true;
        return acc;
      },
      {} as Record<string, boolean>
    );
    setOpenCategories(initialOpenState);
  }, [productosAgrupados]);

  const toggleCategory = (categoria: string) => {
    setOpenCategories((prev) => ({ ...prev, [categoria]: !prev[categoria] }));
  };

  return (
    <div className="space-y-3">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <Input
          placeholder="Buscar producto por nombre..."
          value={terminoBusqueda}
          onChange={(e) => setTerminoBusqueda(e.target.value)}
          className="pl-10"
        />
      </div>
      <div className="max-h-96 overflow-y-auto space-y-2 pr-2">
        {Object.entries(productosAgrupados).map(([categoria, productos]) => (
          <div key={categoria} className="rounded-lg overflow-hidden">
            <button
              onClick={() => toggleCategory(categoria)}
              className="w-full flex justify-between items-center p-3 bg-gray-100 hover:bg-gray-200 transition-colors"
            >
              <span className="font-bold text-gray-700">{categoria}</span>
              <ChevronDown
                className={`w-5 h-5 transition-transform ${
                  openCategories[categoria] ? 'rotate-180' : ''
                }`}
              />
            </button>
            {openCategories[categoria] && (
              <div className="p-2 space-y-2">
                {productos.map((producto) => (
                  <div
                    key={producto.productoID}
                    className="flex items-center justify-between p-3 bg-white rounded-lg shadow-sm"
                  >
                    <div>
                      <div className="font-medium">{producto.nombre}</div>
                      <div className="text-sm text-gray-600">
                        RD$ {producto.precioNumerico.toFixed(2)}
                      </div>
                    </div>
                    <Button size="sm" variant="ghost" onClick={() => onAgregarProducto(producto)}>
                      <PlusCircle className="w-5 h-5" />
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

// Componente para resumen de orden específico para editar
interface ResumenOrdenEditarProps {
  carrito: Carrito;
  clientes: Cliente[];
  onActualizarCantidad: (productoId: number, nuevaCantidad: number) => void;
  onEliminarProducto: (productoId: number) => void;
  onSeleccionarCliente: (cliente: Cliente | null) => void;
  onActualizarObservaciones: (observaciones: string) => void;
  onLimpiarCarrito: () => void;
}

const ResumenOrdenEditar: React.FC<ResumenOrdenEditarProps> = ({
  carrito,
  clientes,
  onActualizarCantidad,
  onEliminarProducto,
  onSeleccionarCliente,
  onActualizarObservaciones,
  onLimpiarCarrito,
}) => {
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

  if (carrito.items.length === 0) {
    return (
      <Card className="p-6 text-center">
        <Edit className="w-16 h-16 mx-auto text-gray-400 mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">Orden vacía</h3>
        <p className="text-gray-600">Agrega productos para editar la orden</p>
      </Card>
    );
  }

  return (
    <Card className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <Edit className="w-6 h-6 text-dominican-blue" />
          <h3 className="text-lg font-bold text-gray-900">Editar Orden</h3>
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
              {items.map((item) => (
                <div key={item.producto.productoID} className="flex items-center space-x-3">
                  {/* Detalles del producto */}
                  <div className="flex-1">
                    <div className="font-medium text-gray-900">{item.producto.nombre}</div>
                    <div className="text-sm text-gray-500">
                      RD$ {item.producto.precioNumerico.toFixed(2)}
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
                      RD$ {(item.producto.precioNumerico * item.cantidad).toFixed(2)}
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
              ))}
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
            <span className="font-medium">RD$ {carrito.subtotal.toFixed(2)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">ITBIS (18%):</span>
            <span className="font-medium">RD$ {carrito.impuesto.toFixed(2)}</span>
          </div>
          <div className="flex justify-between text-lg font-bold">
            <span>Total:</span>
            <span className="text-dominican-blue">RD$ {carrito.total.toFixed(2)}</span>
          </div>
        </div>
      </div>
    </Card>
  );
};

interface EditarOrdenFormProps {
  orden: Orden;
  onOrdenActualizada: (orden: Orden) => void;
  onClose: () => void;
}

export const EditarOrdenForm: React.FC<EditarOrdenFormProps> = ({
  orden,
  onOrdenActualizada,
  onClose,
}) => {
  const [productos, setProductos] = useState<Producto[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [carrito, setCarrito] = useState<Carrito>({
    items: [],
    subtotal: 0,
    impuesto: 0,
    total: 0,
    tipoOrden: 'Mesa',
  });
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const inicializarCarrito = useCallback(() => {
    if (orden && orden.detalles) {
      console.log('Inicializando carrito con orden:', orden);
      console.log('Detalles de la orden:', orden.detalles);

      const items: ItemCarrito[] = orden.detalles
        .map((detalle): ItemCarrito | null => {
          if (!detalle.producto) {
            console.log('Detalle sin producto:', detalle);
            return null;
          }
          return {
            producto: detalle.producto,
            cantidad: detalle.cantidad,
            subtotal: detalle.producto.precioNumerico * detalle.cantidad,
            notasEspeciales: detalle.observaciones,
          };
        })
        .filter((item): item is ItemCarrito => item !== null);

      console.log('Items procesados:', items);

      const subtotal = items.reduce((acc: number, item: ItemCarrito) => acc + item.subtotal, 0);
      const impuesto = subtotal * 0.18;
      const total = subtotal + impuesto;

      const carritoInicial = {
        items,
        subtotal,
        impuesto,
        total,
        tipoOrden: orden.tipoOrden,
        mesaSeleccionada: orden.mesa,
        clienteSeleccionado: orden.cliente,
        observacionesGenerales: orden.observaciones,
      };

      console.log('Carrito inicial:', carritoInicial);
      setCarrito(carritoInicial);
    }
  }, [orden]);

  useEffect(() => {
    const cargarDatos = async () => {
      try {
        setLoading(true);
        const [productosDisponibles, clientesDisponibles] = await Promise.all([
          productosService.getAllProductos(),
          clienteService.getClientes(),
        ]);
        setProductos(productosDisponibles.filter((p) => p.estaDisponible));
        setClientes(clientesDisponibles);
        inicializarCarrito();
        setError(null);
      } catch (err) {
        setError('Error al cargar los datos necesarios para la orden.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    cargarDatos();
  }, [orden, inicializarCarrito]);

  const actualizarCalculosCarrito = (items: Carrito['items']) => {
    const subtotal = items.reduce(
      (acc: number, item: ItemCarrito) => acc + item.producto.precioNumerico * item.cantidad,
      0
    );
    const impuesto = subtotal * 0.18;
    const total = subtotal + impuesto;
    setCarrito((prev) => ({ ...prev, items, subtotal, impuesto, total }));
  };

  const handleAgregarProducto = (producto: Producto, cantidad = 1) => {
    setCarrito((prev) => {
      const existente = prev.items.find((i) => i.producto.productoID === producto.productoID);
      let nuevosItems;
      if (existente) {
        nuevosItems = prev.items.map((i) =>
          i.producto.productoID === producto.productoID
            ? { ...i, cantidad: i.cantidad + cantidad }
            : i
        );
      } else {
        nuevosItems = [
          ...prev.items,
          { producto, cantidad, subtotal: producto.precioNumerico * cantidad },
        ];
      }
      actualizarCalculosCarrito(nuevosItems);
      return { ...prev, items: nuevosItems };
    });
  };

  const handleActualizarCantidad = (productoId: number, nuevaCantidad: number) => {
    if (nuevaCantidad <= 0) {
      handleEliminarProducto(productoId);
      return;
    }
    const nuevosItems = carrito.items.map((i) =>
      i.producto.productoID === productoId ? { ...i, cantidad: nuevaCantidad } : i
    );
    actualizarCalculosCarrito(nuevosItems);
  };

  const handleEliminarProducto = (productoId: number) => {
    const nuevosItems = carrito.items.filter((i) => i.producto.productoID !== productoId);
    actualizarCalculosCarrito(nuevosItems);
  };

  const handleLimpiarCarrito = () => {
    actualizarCalculosCarrito([]);
  };

  const handleSubmit = async () => {
    if (carrito.items.length === 0) {
      toast.error('Debe agregar al menos un producto a la orden');
      return;
    }

    setIsSubmitting(true);
    try {
      const request: ActualizarOrdenRequest = {
        ordenID: orden.ordenID,
        observaciones: carrito.observacionesGenerales,
        items: carrito.items.map((i) => ({
          productoId: i.producto.productoID,
          cantidad: i.cantidad,
          notasEspeciales: i.notasEspeciales,
        })),
      };
      const ordenActualizada = await ordenesService.actualizarOrden(orden.ordenID, request);
      toast.success(`Orden #${orden.numeroOrden} actualizada exitosamente`);
      onOrdenActualizada(ordenActualizada);
    } catch (err) {
      toast.error('Ocurrió un error al actualizar la orden.');
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading)
    return (
      <div className="flex justify-center p-8">
        <LoadingSpinner />
      </div>
    );
  if (error)
    return (
      <div className="p-8 text-center text-red-600">
        <AlertTriangle className="w-8 h-8 mx-auto mb-2" />
        <p>{error}</p>
      </div>
    );

  return (
    <div className="p-2 md:p-4 bg-gray-50 max-h-[85vh] flex flex-col">
      <div className="flex-shrink-0 mb-4">
        <h3 className="text-xl font-bold text-gray-800">
          Editando Orden #{orden.numeroOrden} - Mesa {orden.mesa?.numeroMesa}
        </h3>
      </div>
      <div className="flex-grow grid grid-cols-1 lg:grid-cols-2 gap-6 overflow-y-auto">
        <div className="space-y-4">
          <h3 className="text-lg font-bold text-gray-900 sticky top-0 bg-gray-50 py-2">
            Añadir Productos
          </h3>
          <PanelBusquedaProductos productos={productos} onAgregarProducto={handleAgregarProducto} />
        </div>
        <div className="space-y-4">
          <h3 className="text-lg font-bold text-gray-900 sticky top-0 bg-gray-50 py-2">
            Resumen de la Orden
          </h3>
          <ResumenOrdenEditar
            carrito={carrito}
            clientes={clientes}
            onActualizarCantidad={handleActualizarCantidad}
            onEliminarProducto={handleEliminarProducto}
            onLimpiarCarrito={handleLimpiarCarrito}
            onActualizarObservaciones={(obs) =>
              setCarrito((prev) => ({ ...prev, observacionesGenerales: obs }))
            }
            onSeleccionarCliente={(cliente) =>
              setCarrito((prev) => ({ ...prev, clienteSeleccionado: cliente || undefined }))
            }
          />
        </div>
      </div>
      <div className="mt-6 pt-4 border-t flex justify-between items-center flex-shrink-0">
        <Button variant="ghost" onClick={onClose}>
          Cerrar
        </Button>
        <Button onClick={handleSubmit} disabled={isSubmitting || carrito.items.length === 0}>
          {isSubmitting ? <LoadingSpinner size="sm" /> : <ChefHat className="w-4 h-4 mr-2" />}
          Actualizar Orden
        </Button>
      </div>
    </div>
  );
};
