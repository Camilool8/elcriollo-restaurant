import React, { useState, useEffect, useMemo } from 'react';
import { toast } from 'react-toastify';
import { Button } from '@/components/ui/Button';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import { CarritoCompras } from '@/components/ordenes/CarritoCompras';
import { Input } from '@/components/ui/Input';
import { Search, PlusCircle, ChevronDown } from 'lucide-react';
import { ordenesService } from '@/services/ordenesService';
import { productosService } from '@/services/productosService';
import { clienteService } from '@/services/clienteService';
import { useOrdenesContext } from '@/contexts/OrdenesContext';

import type {
  CrearOrdenRequest,
  Carrito,
  Orden,
  Producto,
  Mesa,
  ItemCarrito,
  Cliente,
} from '@/types';
import { AlertTriangle, ChefHat } from 'lucide-react';

// ============================================================================
// COMPONENTE INTERNO PARA BÚSQUEDA DE PRODUCTOS
// ============================================================================

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
                      <div className="text-sm text-gray-600">{producto.precio}</div>
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

// ============================================================================
// COMPONENTE PRINCIPAL DEL FORMULARIO
// ============================================================================

interface CrearOrdenFormProps {
  mesa: Mesa;
  onOrdenCreada: (orden: Orden) => void;
  onClose: () => void;
}

export const CrearOrdenForm: React.FC<CrearOrdenFormProps> = ({ mesa, onOrdenCreada, onClose }) => {
  const { notificarCambioOrden } = useOrdenesContext();

  // Estados de datos
  const [productos, setProductos] = useState<Producto[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [carrito, setCarrito] = useState<Carrito>({
    items: [],
    subtotal: 0,
    impuesto: 0,
    total: 0,
    tipoOrden: 'Mesa',
    mesaSeleccionada: mesa,
  });

  // Estados de UI
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

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
        setError(null);
      } catch (err) {
        setError('Error al cargar los datos necesarios para la orden.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    cargarDatos();
  }, []);

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
    setIsSubmitting(true);
    try {
      const request: CrearOrdenRequest = {
        mesaID: mesa.mesaID,
        clienteID: carrito.clienteSeleccionado?.clienteID,
        tipoOrden: 'Mesa',
        observaciones: carrito.observacionesGenerales,
        items: carrito.items.map((i) => ({
          productoId: i.producto.productoID,
          cantidad: i.cantidad,
          notasEspeciales: i.notasEspeciales,
        })),
      };
      const nuevaOrden = await ordenesService.crearOrden(request);
      toast.success(`Nueva orden creada para la mesa ${mesa.numeroMesa}`);
      notificarCambioOrden(nuevaOrden.ordenID);
      onOrdenCreada(nuevaOrden);
    } catch (err) {
      toast.error('Ocurrió un error al procesar la orden.');
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center p-8">
        <LoadingSpinner />
        <span className="ml-2">Cargando productos...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8 text-center text-red-600">
        <AlertTriangle className="mx-auto w-8 h-8 mb-2" />
        <p>{error}</p>
      </div>
    );
  }

  return (
    <div className="p-2 md:p-4 bg-gray-50 max-h-[85vh] flex flex-col">
      <div className="flex-shrink-0 mb-4">
        <h3 className="text-xl font-bold text-gray-800">Nueva Orden - Mesa {mesa.numeroMesa}</h3>
      </div>
      <div className="flex-grow grid grid-cols-1 lg:grid-cols-2 gap-6 overflow-y-auto">
        {/* Columna de la izquierda: Búsqueda de productos */}
        <div className="space-y-4">
          <h3 className="text-lg font-bold text-gray-900 sticky top-0 bg-gray-50 py-2">
            Añadir Productos
          </h3>
          <PanelBusquedaProductos productos={productos} onAgregarProducto={handleAgregarProducto} />
        </div>

        {/* Columna de la derecha: Carrito de compras */}
        <div className="space-y-4">
          <h3 className="text-lg font-bold text-gray-900 sticky top-0 bg-gray-50 py-2">
            Resumen de la Orden
          </h3>
          <CarritoCompras
            carrito={carrito}
            productos={productos}
            mesas={[]}
            clientes={clientes}
            onAgregarProducto={handleAgregarProducto}
            onActualizarCantidad={handleActualizarCantidad}
            onEliminarProducto={handleEliminarProducto}
            onLimpiarCarrito={handleLimpiarCarrito}
            onActualizarObservaciones={(obs) =>
              setCarrito((prev) => ({ ...prev, observacionesGenerales: obs }))
            }
            onSeleccionarCliente={(cliente) =>
              setCarrito((prev) => ({ ...prev, clienteSeleccionado: cliente || undefined }))
            }
            onConfirmarOrden={handleSubmit}
          />
        </div>
      </div>

      <div className="mt-6 pt-4 border-t flex justify-between items-center flex-shrink-0">
        <Button variant="ghost" onClick={onClose}>
          Cerrar
        </Button>
        <div className="flex space-x-2">
          <Button onClick={handleSubmit} disabled={isSubmitting || carrito.items.length === 0}>
            {isSubmitting ? <LoadingSpinner size="sm" /> : <ChefHat className="w-4 h-4 mr-2" />}
            Crear Orden
          </Button>
        </div>
      </div>
    </div>
  );
};
