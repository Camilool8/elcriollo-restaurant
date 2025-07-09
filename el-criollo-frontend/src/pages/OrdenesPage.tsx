import React, { useState, useEffect } from 'react';
import {
  Plus,
  Filter,
  Search,
  RefreshCw,
  Users,
  Clock,
  TrendingUp,
  AlertTriangle,
} from 'lucide-react';
import { toast } from 'react-toastify';

// Hooks y servicios
import { useOrdenes } from '@/hooks/useOrdenes';
import { useMesas } from '@/hooks/useMesas';
import { productosService } from '@/services/productosService';
import { clienteService } from '@/services/clienteService';

// Componentes
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import { Modal } from '@/components/ui/Modal';
import { OrdenCard } from '@/components/ordenes/OrdenCard';
import { CarritoCompras } from '@/components/ordenes/CarritoCompras';
import { EditarOrdenForm } from '@/components/ordenes/EditarOrdenForm';
import { FacturaFormSimple } from '@/components/facturacion/FacturaFormSimple';

// Types
import type {
  Orden,
  EstadoOrden,
  TipoOrden,
  CrearOrdenRequest,
  ItemCarrito,
  Carrito,
} from '@/types/orden';
import type { Producto, Cliente } from '@/types';
import { parsePrice } from '@/utils/priceUtils';

const OrdenesPage: React.FC = () => {
  // Hooks principales
  const {
    ordenes,
    estadisticas,
    loading: loadingOrdenes,
    refrescar,
    crearOrden,
    actualizarOrden,
    actualizarEstadoOrden,
    cancelarOrden,
    ordenesUrgentes,
  } = useOrdenes({
    autoRefresh: true,
    refreshInterval: 15000,
    soloActivas: true,
  });

  const { mesas } = useMesas({
    autoRefresh: true,
    refreshInterval: 30000,
  });

  // Estados locales
  const [productos, setProductos] = useState<Producto[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loadingDatos, setLoadingDatos] = useState(true);

  // Estados de UI
  const [filtroEstado, setFiltroEstado] = useState<EstadoOrden | 'Todas'>('Todas');
  const [filtroTipo, setFiltroTipo] = useState<TipoOrden | 'Todos'>('Todos');
  const [busqueda, setBusqueda] = useState('');

  // Estados de modales
  const [mostrarCarrito, setMostrarCarrito] = useState(false);
  const [ordenSeleccionada, setOrdenSeleccionada] = useState<Orden | null>(null);
  const [mostrarDetalles, setMostrarDetalles] = useState(false);
  const [mostrarEditorOrden, setMostrarEditorOrden] = useState(false);
  const [ordenParaEditar, setOrdenParaEditar] = useState<Orden | null>(null);
  const [mostrarFacturacion, setMostrarFacturacion] = useState(false);
  const [ordenParaFacturar, setOrdenParaFacturar] = useState<Orden | null>(null);

  // Estado del carrito
  const [carrito, setCarrito] = useState<Carrito>({
    items: [],
    tipoOrden: 'Mesa',
    subtotal: 0,
    impuesto: 0,
    total: 0,
  });

  // ============================================================================
  // EFECTOS
  // ============================================================================

  useEffect(() => {
    cargarDatosIniciales();
  }, []);

  useEffect(() => {
    calcularTotalesCarrito();
  }, [carrito.items]);

  // ============================================================================
  // FUNCIONES DE CARGA
  // ============================================================================

  const cargarDatosIniciales = async () => {
    try {
      setLoadingDatos(true);
      const [productosData, clientesData] = await Promise.all([
        productosService.getAllProductos(),
        clienteService.getClientes(),
      ]);

      setProductos(productosData.filter((p) => p.estaDisponible));
      setClientes(clientesData.filter((c) => c.estado));
    } catch (error) {
      console.error('Error cargando datos:', error);
      toast.error('Error cargando productos y clientes');
    } finally {
      setLoadingDatos(false);
    }
  };

  // ============================================================================
  // FUNCIONES DEL CARRITO
  // ============================================================================

  const agregarProductoCarrito = (producto: Producto, cantidad: number = 1) => {
    setCarrito((prev) => {
      const itemExistente = prev.items.find(
        (item) => item.producto.productoID === producto.productoID
      );

      let nuevosItems: ItemCarrito[];
      if (itemExistente) {
        nuevosItems = prev.items.map((item) =>
          item.producto.productoID === producto.productoID
            ? { ...item, cantidad: item.cantidad + cantidad }
            : item
        );
      } else {
        const nuevoItem: ItemCarrito = {
          producto,
          cantidad,
          subtotal: parsePrice(producto.precio) * cantidad,
        };
        nuevosItems = [...prev.items, nuevoItem];
      }

      return { ...prev, items: nuevosItems };
    });

    toast.success(`${producto.nombre} agregado al carrito`);
  };

  const actualizarCantidadCarrito = (productoId: number, nuevaCantidad: number) => {
    if (nuevaCantidad <= 0) {
      eliminarProductoCarrito(productoId);
      return;
    }

    setCarrito((prev) => ({
      ...prev,
      items: prev.items.map((item) =>
        item.producto.productoID === productoId
          ? {
              ...item,
              cantidad: nuevaCantidad,
              subtotal: parsePrice(item.producto.precio) * nuevaCantidad,
            }
          : item
      ),
    }));
  };

  const eliminarProductoCarrito = (productoId: number) => {
    setCarrito((prev) => ({
      ...prev,
      items: prev.items.filter((item) => item.producto.productoID !== productoId),
    }));
  };

  const calcularTotalesCarrito = () => {
    const subtotal = carrito.items.reduce((sum, item) => sum + item.subtotal, 0);
    const impuesto = subtotal * 0.18; // ITBIS 18%
    const total = subtotal + impuesto;

    setCarrito((prev) => ({
      ...prev,
      subtotal: Number(subtotal.toFixed(2)),
      impuesto: Number(impuesto.toFixed(2)),
      total: Number(total.toFixed(2)),
    }));
  };

  const limpiarCarrito = () => {
    setCarrito({
      items: [],
      tipoOrden: 'Mesa',
      subtotal: 0,
      impuesto: 0,
      total: 0,
    });
    setMostrarCarrito(false);
  };

  // ============================================================================
  // HANDLERS DE CARRITO
  // ============================================================================

  const handleConfirmarOrden = async (request: CrearOrdenRequest) => {
    try {
      await crearOrden(request);
      limpiarCarrito();
      toast.success('Orden creada exitosamente');
    } catch (error) {
      console.error('Error creando orden:', error);
      toast.error('Error al crear la orden');
    }
  };

  // ============================================================================
  // HANDLERS DE ÓRDENES
  // ============================================================================

  const handleCambiarEstado = async (ordenId: number, nuevoEstado: EstadoOrden) => {
    await actualizarEstadoOrden(ordenId, nuevoEstado);
  };

  const handleCancelarOrden = async (orden: Orden) => {
    const motivo = prompt('Motivo de cancelación (opcional):');
    await cancelarOrden(orden.ordenID, motivo || undefined);
  };

  const handleVerDetalles = (orden: Orden) => {
    setOrdenSeleccionada(orden);
    setMostrarDetalles(true);
  };

  const handleEditarOrden = (orden: Orden) => {
    setOrdenParaEditar(orden);
    setMostrarEditorOrden(true);
  };

  const handleFacturarOrden = (orden: Orden) => {
    // Verificar si la orden ya está facturada
    if (orden.estado === 'Facturada') {
      toast.warning('Esta orden ya ha sido facturada');
      return;
    }

    setOrdenParaFacturar(orden);
    setMostrarFacturacion(true);
  };

  const handleOrdenActualizada = (ordenActualizada: Orden) => {
    actualizarOrden(ordenActualizada);
    setMostrarEditorOrden(false);
    setOrdenParaEditar(null);
    toast.success(`Orden #${ordenActualizada.numeroOrden} actualizada.`);
  };

  const handleFacturaCreada = () => {
    setMostrarFacturacion(false);
    setOrdenParaFacturar(null);
    refrescar();
  };

  // ============================================================================
  // FILTROS
  // ============================================================================

  const ordenesFiltradas = ordenes.filter((orden) => {
    // Filtro por estado
    if (filtroEstado !== 'Todas' && orden.estado !== filtroEstado) {
      return false;
    }

    // Filtro por tipo
    if (filtroTipo !== 'Todos' && orden.tipoOrden !== filtroTipo) {
      return false;
    }

    // Filtro por búsqueda
    if (busqueda.trim()) {
      const termino = busqueda.toLowerCase();
      return (
        orden.numeroOrden.toLowerCase().includes(termino) ||
        orden.cliente?.nombreCompleto.toLowerCase().includes(termino) ||
        orden.mesa?.numeroMesa.toString().includes(termino)
      );
    }

    return true;
  });

  // ============================================================================
  // RENDER
  // ============================================================================

  if (loadingOrdenes || loadingDatos) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Gestión de Órdenes</h1>
          <p className="text-gray-600">Administra las órdenes del restaurante</p>
        </div>

        <div className="flex space-x-3">
          <Button onClick={refrescar} variant="outline" className="flex items-center space-x-2">
            <RefreshCw className="w-4 h-4" />
            <span>Actualizar</span>
          </Button>

          <Button
            onClick={() => setMostrarCarrito(true)}
            className="bg-dominican-blue hover:bg-dominican-blue/90"
          >
            <Plus className="w-4 h-4 mr-2" />
            Nueva Orden
          </Button>
        </div>
      </div>

      {/* Estadísticas rápidas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="p-4">
          <div className="flex items-center space-x-3">
            <div className="p-2 bg-amber-100 rounded-lg">
              <Clock className="w-6 h-6 text-amber-600" />
            </div>
            <div>
              <div className="text-2xl font-bold text-gray-900">
                {estadisticas.ordenesPendientes}
              </div>
              <div className="text-sm text-gray-600">Pendientes</div>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center space-x-3">
            <div className="p-2 bg-blue-100 rounded-lg">
              <Users className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <div className="text-2xl font-bold text-gray-900">
                {estadisticas.ordenesEnPreparacion}
              </div>
              <div className="text-sm text-gray-600">En preparación</div>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center space-x-3">
            <div className="p-2 bg-green-100 rounded-lg">
              <TrendingUp className="w-6 h-6 text-green-600" />
            </div>
            <div>
              <div className="text-2xl font-bold text-gray-900">{estadisticas.ordenesListas}</div>
              <div className="text-sm text-gray-600">Listas</div>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center space-x-3">
            <div className="p-2 bg-red-100 rounded-lg">
              <AlertTriangle className="w-6 h-6 text-red-600" />
            </div>
            <div>
              <div className="text-2xl font-bold text-gray-900">{ordenesUrgentes.length}</div>
              <div className="text-sm text-gray-600">Urgentes</div>
            </div>
          </div>
        </Card>
      </div>

      {/* Filtros */}
      <Card className="p-4">
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center space-x-2">
            <Filter className="w-4 h-4 text-gray-500" />
            <span className="text-sm font-medium text-gray-700">Filtros:</span>
          </div>

          <select
            value={filtroEstado}
            onChange={(e) => setFiltroEstado(e.target.value as EstadoOrden | 'Todas')}
            className="px-3 py-1 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
          >
            <option value="Todas">Todos los estados</option>
            <option value="Pendiente">Pendientes</option>
            <option value="En Preparacion">En preparación</option>
            <option value="Lista">Listas</option>
            <option value="Entregada">Entregadas</option>
            <option value="Cancelada">Canceladas</option>
          </select>

          <select
            value={filtroTipo}
            onChange={(e) => setFiltroTipo(e.target.value as TipoOrden | 'Todos')}
            className="px-3 py-1 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
          >
            <option value="Todos">Todos los tipos</option>
            <option value="Mesa">Mesa</option>
            <option value="Llevar">Para llevar</option>
            <option value="Delivery">Delivery</option>
          </select>

          <div className="flex items-center space-x-2 flex-1 max-w-md">
            <Search className="w-4 h-4 text-gray-500" />
            <Input
              placeholder="Buscar por número, cliente o mesa..."
              value={busqueda}
              onChange={(e) => setBusqueda(e.target.value)}
              className="flex-1"
            />
          </div>
        </div>
      </Card>

      {/* Lista de órdenes */}
      <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
        {ordenesFiltradas.map((orden) => (
          <OrdenCard
            key={orden.ordenID}
            orden={orden}
            onEstadoChange={handleCambiarEstado}
            onVerDetalles={handleVerDetalles}
            onEditarOrden={handleEditarOrden}
            onCancelarOrden={handleCancelarOrden}
            onFacturarOrden={handleFacturarOrden}
            showActions={true}
          />
        ))}
      </div>

      {ordenesFiltradas.length === 0 && (
        <Card className="p-8 text-center">
          <div className="text-gray-500 mb-4">
            <Search className="w-16 h-16 mx-auto" />
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">No se encontraron órdenes</h3>
          <p className="text-gray-600">Ajusta los filtros o crea una nueva orden para comenzar</p>
        </Card>
      )}

      {/* Modal de carrito */}
      <Modal
        isOpen={mostrarCarrito}
        onClose={() => setMostrarCarrito(false)}
        title="Nueva Orden"
        size="lg"
      >
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 max-h-[80vh] overflow-y-auto">
          {/* Lista de productos */}
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">Productos disponibles</h3>
            <div className="space-y-3 max-h-96 overflow-y-auto">
              {productos.map((producto) => (
                <div
                  key={producto.productoID}
                  className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50"
                >
                  <div className="flex-1">
                    <div className="font-medium text-gray-900">{producto.nombre}</div>
                    <div className="text-sm text-gray-600">{producto.descripcion}</div>
                    <div className="text-sm font-medium text-dominican-blue">
                      RD${parsePrice(producto.precio).toFixed(2)}
                    </div>
                  </div>
                  <Button
                    onClick={() => agregarProductoCarrito(producto)}
                    size="sm"
                    className="bg-dominican-blue hover:bg-dominican-blue/90"
                  >
                    <Plus className="w-4 h-4" />
                  </Button>
                </div>
              ))}
            </div>
          </div>

          {/* Carrito */}
          <div>
            <CarritoCompras
              carrito={carrito}
              productos={productos}
              mesas={mesas}
              clientes={clientes}
              onAgregarProducto={agregarProductoCarrito}
              onActualizarCantidad={actualizarCantidadCarrito}
              onEliminarProducto={eliminarProductoCarrito}
              onSeleccionarCliente={(cliente) =>
                setCarrito((prev) => ({ ...prev, clienteSeleccionado: cliente || undefined }))
              }
              onActualizarObservaciones={(obs) =>
                setCarrito((prev) => ({ ...prev, observacionesGenerales: obs }))
              }
              onConfirmarOrden={handleConfirmarOrden}
              onLimpiarCarrito={limpiarCarrito}
            />
          </div>
        </div>
      </Modal>

      {/* Modal de edición de orden */}
      <Modal
        isOpen={mostrarEditorOrden}
        onClose={() => setMostrarEditorOrden(false)}
        title={`Editando Orden - ${ordenParaEditar?.numeroOrden}`}
        size="lg"
      >
        {ordenParaEditar && (
          <EditarOrdenForm
            orden={ordenParaEditar}
            onOrdenActualizada={handleOrdenActualizada}
            onClose={() => setMostrarEditorOrden(false)}
          />
        )}
      </Modal>

      {/* Modal de facturación */}
      <Modal
        isOpen={mostrarFacturacion}
        onClose={() => setMostrarFacturacion(false)}
        title={`Facturar Orden - ${ordenParaFacturar?.numeroOrden}`}
        size="md"
      >
        {ordenParaFacturar && (
          <FacturaFormSimple
            orden={ordenParaFacturar}
            onFacturaCreada={handleFacturaCreada}
            onClose={() => setMostrarFacturacion(false)}
          />
        )}
      </Modal>

      {/* Modal de detalles */}
      <Modal
        isOpen={mostrarDetalles}
        onClose={() => setMostrarDetalles(false)}
        title={`Detalles - ${ordenSeleccionada?.numeroOrden}`}
        size="md"
      >
        {ordenSeleccionada && (
          <div className="space-y-4">
            <OrdenCard
              orden={ordenSeleccionada}
              onEstadoChange={handleCambiarEstado}
              showActions={false}
            />

            {/* Detalles completos */}
            {ordenSeleccionada.detalles && (
              <div>
                <h4 className="font-medium text-gray-900 mb-3">Items detallados:</h4>
                <div className="space-y-2">
                  {ordenSeleccionada.detalles.map((detalle, index) => (
                    <div
                      key={index}
                      className="flex justify-between items-center p-2 bg-gray-50 rounded"
                    >
                      <div>
                        <div className="font-medium">{detalle.nombreItem}</div>
                        <div className="text-sm text-gray-600">
                          {detalle.cantidad} x {detalle.precioUnitario}
                        </div>
                        {detalle.observaciones && (
                          <div className="text-xs text-amber-600">{detalle.observaciones}</div>
                        )}
                      </div>
                      <div className="font-bold">{detalle.subtotal}</div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
};

export default OrdenesPage;
