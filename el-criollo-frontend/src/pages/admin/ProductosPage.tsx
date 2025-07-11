import React, { useState } from 'react';
import { toast } from 'react-toastify';
import { Search, Plus, Edit, Trash2, Package, Clock, AlertTriangle, Eye } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { useProductos } from '@/hooks/useProductos';
import { Producto } from '@/types';
import { formatearPrecio } from '@/utils/dominicanValidations';

interface CreateProductoForm {
  nombre: string;
  descripcion: string;
  categoriaID: number;
  precio: string;
  tiempoPreparacion: string;
}

interface EditProductoForm {
  nombre: string;
  descripcion: string;
  categoriaID: number;
  precio: string;
  tiempoPreparacion: string;
}

const ProductosPage: React.FC = () => {
  const {
    categorias,
    isLoading,
    error,
    loadData,
    createProducto,
    updateProducto,
    deleteProducto,
    searchProductos,
    totalProductos,
    productosActivos,
    productosInactivos,
    productosStockBajo,
  } = useProductos({ autoRefresh: true, refreshInterval: 30000 });

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategoria, setSelectedCategoria] = useState<number | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [selectedProducto, setSelectedProducto] = useState<Producto | null>(null);
  const [createForm, setCreateForm] = useState<CreateProductoForm>({
    nombre: '',
    descripcion: '',
    categoriaID: 0,
    precio: '',
    tiempoPreparacion: '',
  });
  const [editForm, setEditForm] = useState<EditProductoForm>({
    nombre: '',
    descripcion: '',
    categoriaID: 0,
    precio: '',
    tiempoPreparacion: '',
  });

  const handleCreateProducto = async () => {
    if (!createForm.nombre.trim() || !createForm.categoriaID || !createForm.precio.trim()) {
      toast.error('Nombre, categoría y precio son requeridos');
      return;
    }

    const precioNumerico = parseFloat(createForm.precio.replace(/[^\d.]/g, ''));
    if (isNaN(precioNumerico) || precioNumerico <= 0) {
      toast.error('El precio debe ser un número válido mayor a 0');
      return;
    }

    const productoData = {
      nombre: createForm.nombre.trim(),
      descripcion: createForm.descripcion.trim(),
      categoriaId: createForm.categoriaID,
      precio: precioNumerico,
      tiempoPreparacion: createForm.tiempoPreparacion.trim()
        ? parseInt(createForm.tiempoPreparacion)
        : undefined,
    };

    const success = await createProducto(productoData);
    if (success) {
      setShowCreateModal(false);
      setCreateForm({
        nombre: '',
        descripcion: '',
        categoriaID: 0,
        precio: '',
        tiempoPreparacion: '',
      });
    }
  };

  const handleEditProducto = async () => {
    if (
      !selectedProducto ||
      !editForm.nombre.trim() ||
      !editForm.categoriaID ||
      !editForm.precio.trim()
    ) {
      toast.error('Nombre, categoría y precio son requeridos');
      return;
    }

    const precioNumerico = parseFloat(editForm.precio.replace(/[^\d.]/g, ''));
    if (isNaN(precioNumerico) || precioNumerico <= 0) {
      toast.error('El precio debe ser un número válido mayor a 0');
      return;
    }

    const productoData = {
      nombre: editForm.nombre.trim(),
      descripcion: editForm.descripcion.trim(),
      categoriaId: editForm.categoriaID,
      precio: precioNumerico,
      tiempoPreparacion: editForm.tiempoPreparacion.trim()
        ? parseInt(editForm.tiempoPreparacion)
        : undefined,
    };

    const success = await updateProducto(selectedProducto.productoID, productoData);
    if (success) {
      setShowEditModal(false);
      setSelectedProducto(null);
      setEditForm({
        nombre: '',
        descripcion: '',
        categoriaID: 0,
        precio: '',
        tiempoPreparacion: '',
      });
    }
  };

  const handleDeleteProducto = async () => {
    if (!selectedProducto) return;

    const success = await deleteProducto(selectedProducto.productoID);
    if (success) {
      setShowDeleteModal(false);
      setSelectedProducto(null);
    }
  };

  const openEditModal = (producto: Producto) => {
    setSelectedProducto(producto);
    setEditForm({
      nombre: producto.nombre,
      descripcion: producto.descripcion || '',
      categoriaID: producto.categoria.categoriaID,
      precio: producto.precio,
      tiempoPreparacion: producto.tiempoPreparacion,
    });
    setShowEditModal(true);
  };

  const openDeleteModal = (producto: Producto) => {
    setSelectedProducto(producto);
    setShowDeleteModal(true);
  };

  const openDetailsModal = (producto: Producto) => {
    setSelectedProducto(producto);
    setShowDetailsModal(true);
  };

  const filteredProductos = searchProductos(searchQuery, selectedCategoria || undefined);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Gestión de Productos
          </h1>
          <p className="text-stone-gray mt-1">Administra el catálogo de productos</p>
          {error && <p className="text-red-600 text-sm mt-1">Error: {error}</p>}
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={loadData} disabled={isLoading}>
            {isLoading ? 'Actualizando...' : 'Actualizar'}
          </Button>
          <Button
            variant="primary"
            leftIcon={<Plus className="w-4 h-4" />}
            onClick={() => setShowCreateModal(true)}
          >
            Nuevo Producto
          </Button>
        </div>
      </div>

      {/* Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-dominican-blue">{totalProductos}</p>
          <p className="text-sm text-stone-gray">Total Productos</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-green-600">{productosActivos}</p>
          <p className="text-sm text-stone-gray">Disponibles</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-red-600">{productosInactivos}</p>
          <p className="text-sm text-stone-gray">No Disponibles</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-blue-600">{productosStockBajo}</p>
          <p className="text-sm text-stone-gray">Stock Bajo</p>
        </Card>
      </div>

      {/* Filtros */}
      <Card>
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <Input
              placeholder="Buscar productos por nombre o descripción..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              leftIcon={<Search className="w-5 h-5" />}
              fullWidth
            />
          </div>
          <div className="flex gap-2">
            <select
              value={selectedCategoria || ''}
              onChange={(e) => setSelectedCategoria(e.target.value ? Number(e.target.value) : null)}
              className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-gray-900 bg-white"
            >
              <option value="" className="text-gray-900">
                Todas las categorías
              </option>
              {categorias.map((categoria) => (
                <option
                  key={categoria.categoriaID}
                  value={categoria.categoriaID}
                  className="text-gray-900"
                >
                  {categoria.nombreCategoria || `Categoría ${categoria.categoriaID}`}
                </option>
              ))}
            </select>
            <Button
              variant="outline"
              onClick={() => {
                setSearchQuery('');
                setSelectedCategoria(null);
              }}
            >
              Limpiar
            </Button>
          </div>
        </div>
      </Card>

      {/* Tabla de productos */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="border-b">
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Producto
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Categoría
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Precio
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Tiempo
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Estado
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Stock
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="text-center p-8 text-stone-gray">
                    Cargando productos...
                  </td>
                </tr>
              ) : filteredProductos.length === 0 ? (
                <tr>
                  <td colSpan={7} className="text-center p-8 text-stone-gray">
                    No se encontraron productos
                  </td>
                </tr>
              ) : (
                filteredProductos.map((producto) => (
                  <tr key={producto.productoID} className="border-b hover:bg-gray-50">
                    <td className="p-4">
                      <div className="flex items-center">
                        <Package className="w-5 h-5 text-dominican-blue mr-2" />
                        <div>
                          <span className="font-medium text-dominican-blue">{producto.nombre}</span>
                          {producto.descripcion && (
                            <p className="text-sm text-stone-gray">{producto.descripcion}</p>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="p-4">
                      <span className="text-stone-gray">
                        {producto.categoria?.nombreCategoria || 'Categoría no disponible'}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <span className="font-medium text-dominican-red">
                        {formatearPrecio(producto.precioNumerico)}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <div className="flex items-center justify-center">
                        <Clock className="w-4 h-4 text-stone-gray mr-1" />
                        <span className="text-sm text-stone-gray">
                          {producto.tiempoPreparacion}
                        </span>
                      </div>
                    </td>
                    <td className="p-4 text-center">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${
                          producto.estaDisponible
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {producto.estaDisponible ? 'Disponible' : 'No Disponible'}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${
                          producto.inventario?.stockBajo
                            ? 'bg-red-100 text-red-800'
                            : 'bg-green-100 text-green-800'
                        }`}
                      >
                        {producto.inventario?.cantidadDisponible || 0}
                      </span>
                    </td>
                    <td className="p-4">
                      <div className="flex justify-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          leftIcon={<Eye className="w-4 h-4" />}
                          onClick={() => openDetailsModal(producto)}
                        >
                          Ver
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          leftIcon={<Edit className="w-4 h-4" />}
                          onClick={() => openEditModal(producto)}
                        >
                          Editar
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          leftIcon={<Trash2 className="w-4 h-4" />}
                          onClick={() => openDeleteModal(producto)}
                        >
                          Eliminar
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Modal Crear Producto */}
      <Modal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        title="Nuevo Producto"
        size="lg"
      >
        <div className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">
                Nombre del Producto *
              </label>
              <Input
                placeholder="Ej: La Bandera Dominicana"
                value={createForm.nombre}
                onChange={(e) => setCreateForm({ ...createForm, nombre: e.target.value })}
                fullWidth
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">
                Categoría *
              </label>
              <select
                value={createForm.categoriaID}
                onChange={(e) =>
                  setCreateForm({ ...createForm, categoriaID: Number(e.target.value) })
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-gray-900 bg-white"
              >
                <option value={0} className="text-gray-900">
                  Seleccionar categoría
                </option>
                {categorias.map((categoria) => (
                  <option
                    key={categoria.categoriaID}
                    value={categoria.categoriaID}
                    className="text-gray-900"
                  >
                    {categoria.nombreCategoria || `Categoría ${categoria.categoriaID}`}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-dominican-blue mb-2">
              Descripción
            </label>
            <textarea
              placeholder="Descripción del producto"
              value={createForm.descripcion}
              onChange={(e) => setCreateForm({ ...createForm, descripcion: e.target.value })}
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
              rows={3}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">Precio *</label>
              <Input
                placeholder="0.00"
                value={createForm.precio}
                onChange={(e) => setCreateForm({ ...createForm, precio: e.target.value })}
                fullWidth
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">
                Tiempo de Preparación
              </label>
              <Input
                placeholder="Ej: 15 min"
                value={createForm.tiempoPreparacion}
                onChange={(e) =>
                  setCreateForm({ ...createForm, tiempoPreparacion: e.target.value })
                }
                fullWidth
              />
            </div>
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="primary" onClick={handleCreateProducto} fullWidth>
              Crear Producto
            </Button>
            <Button variant="outline" onClick={() => setShowCreateModal(false)} fullWidth>
              Cancelar
            </Button>
          </div>
        </div>
      </Modal>

      {/* Modal Editar Producto */}
      <Modal
        isOpen={showEditModal}
        onClose={() => setShowEditModal(false)}
        title="Editar Producto"
        size="lg"
      >
        <div className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">
                Nombre del Producto *
              </label>
              <Input
                placeholder="Ej: La Bandera Dominicana"
                value={editForm.nombre}
                onChange={(e) => setEditForm({ ...editForm, nombre: e.target.value })}
                fullWidth
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">
                Categoría *
              </label>
              <select
                value={editForm.categoriaID}
                onChange={(e) => setEditForm({ ...editForm, categoriaID: Number(e.target.value) })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-gray-900 bg-white"
              >
                <option value={0} className="text-gray-900">
                  Seleccionar categoría
                </option>
                {categorias.map((categoria) => (
                  <option
                    key={categoria.categoriaID}
                    value={categoria.categoriaID}
                    className="text-gray-900"
                  >
                    {categoria.nombreCategoria || `Categoría ${categoria.categoriaID}`}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-dominican-blue mb-2">
              Descripción
            </label>
            <textarea
              placeholder="Descripción del producto"
              value={editForm.descripcion}
              onChange={(e) => setEditForm({ ...editForm, descripcion: e.target.value })}
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
              rows={3}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">Precio *</label>
              <Input
                placeholder="0.00"
                value={editForm.precio}
                onChange={(e) => setEditForm({ ...editForm, precio: e.target.value })}
                fullWidth
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-dominican-blue mb-2">
                Tiempo de Preparación
              </label>
              <Input
                placeholder="Ej: 15 min"
                value={editForm.tiempoPreparacion}
                onChange={(e) => setEditForm({ ...editForm, tiempoPreparacion: e.target.value })}
                fullWidth
              />
            </div>
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="primary" onClick={handleEditProducto} fullWidth>
              Actualizar Producto
            </Button>
            <Button variant="outline" onClick={() => setShowEditModal(false)} fullWidth>
              Cancelar
            </Button>
          </div>
        </div>
      </Modal>

      {/* Modal Detalles del Producto */}
      <Modal
        isOpen={showDetailsModal}
        onClose={() => setShowDetailsModal(false)}
        title="Detalles del Producto"
        size="md"
      >
        {selectedProducto && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">Nombre</label>
                <p className="font-medium text-dominican-blue">{selectedProducto.nombre}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">Categoría</label>
                <p className="font-medium text-dominican-blue">
                  {selectedProducto.categoria?.nombreCategoria || 'Categoría no disponible'}
                </p>
              </div>
            </div>

            {selectedProducto.descripcion && (
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">
                  Descripción
                </label>
                <p className="text-stone-gray">{selectedProducto.descripcion}</p>
              </div>
            )}

            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">Precio</label>
                <p className="font-medium text-dominican-red">
                  {formatearPrecio(selectedProducto.precioNumerico)}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">Tiempo</label>
                <p className="font-medium text-stone-gray">{selectedProducto.tiempoPreparacion}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">Estado</label>
                <span
                  className={`px-2 py-1 rounded-full text-xs font-medium ${
                    selectedProducto.estaDisponible
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}
                >
                  {selectedProducto.estaDisponible ? 'Disponible' : 'No Disponible'}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">
                  Stock Disponible
                </label>
                <p className="font-medium text-dominican-blue">
                  {selectedProducto.inventario?.cantidadDisponible || 0}
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-stone-gray mb-1">
                  Nivel de Stock
                </label>
                <span
                  className={`px-2 py-1 rounded-full text-xs font-medium ${
                    selectedProducto.inventario?.stockBajo
                      ? 'bg-red-100 text-red-800'
                      : 'bg-green-100 text-green-800'
                  }`}
                >
                  {selectedProducto.inventario?.nivelStock || 'Sin información'}
                </span>
              </div>
            </div>

            <div className="flex gap-3 pt-4">
              <Button
                variant="outline"
                onClick={() => {
                  setShowDetailsModal(false);
                  openEditModal(selectedProducto);
                }}
                fullWidth
              >
                Editar Producto
              </Button>
              <Button variant="outline" onClick={() => setShowDetailsModal(false)} fullWidth>
                Cerrar
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Modal Eliminar Producto */}
      <Modal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        title="Eliminar Producto"
        size="md"
      >
        <div className="space-y-4">
          <div className="flex items-center gap-3 p-4 bg-red-50 rounded-lg">
            <AlertTriangle className="w-6 h-6 text-red-600" />
            <div>
              <h3 className="font-medium text-red-800">¿Estás seguro?</h3>
              <p className="text-sm text-red-600">
                Esta acción no se puede deshacer. El producto "{selectedProducto?.nombre}" será
                eliminado permanentemente.
              </p>
            </div>
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="danger" onClick={handleDeleteProducto} fullWidth>
              Eliminar Producto
            </Button>
            <Button variant="outline" onClick={() => setShowDeleteModal(false)} fullWidth>
              Cancelar
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default ProductosPage;
