import React, { useState } from 'react';
import { toast } from 'react-toastify';
import { Search, Plus, Edit, Trash2, Package, AlertTriangle } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { useCategorias } from '@/hooks/useCategorias';
import { Categoria } from '@/types';

interface CreateCategoriaForm {
  nombre: string;
  descripcion: string;
}

interface EditCategoriaForm {
  nombre: string;
  descripcion: string;
}

const CategoriasPage: React.FC = () => {
  const {
    isLoading,
    error,
    loadCategorias,
    createCategoria,
    updateCategoria,
    deleteCategoria,
    searchCategorias,
    totalCategorias,
    totalProductos,
  } = useCategorias({ autoRefresh: true, refreshInterval: 30000 });

  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedCategoria, setSelectedCategoria] = useState<Categoria | null>(null);
  const [createForm, setCreateForm] = useState<CreateCategoriaForm>({
    nombre: '',
    descripcion: '',
  });
  const [editForm, setEditForm] = useState<EditCategoriaForm>({
    nombre: '',
    descripcion: '',
  });

  const handleCreateCategoria = async () => {
    if (!createForm.nombre.trim()) {
      toast.error('El nombre de la categoría es requerido');
      return;
    }

    const success = await createCategoria({
      nombre: createForm.nombre.trim(),
      descripcion: createForm.descripcion.trim(),
    });

    if (success) {
      setShowCreateModal(false);
      setCreateForm({ nombre: '', descripcion: '' });
    }
  };

  const handleEditCategoria = async () => {
    if (!selectedCategoria || !editForm.nombre.trim()) {
      toast.error('El nombre de la categoría es requerido');
      return;
    }

    const success = await updateCategoria(selectedCategoria.categoriaID, {
      nombre: editForm.nombre.trim(),
      descripcion: editForm.descripcion.trim(),
    });

    if (success) {
      setShowEditModal(false);
      setSelectedCategoria(null);
      setEditForm({ nombre: '', descripcion: '' });
    }
  };

  const handleDeleteCategoria = async () => {
    if (!selectedCategoria) return;

    const success = await deleteCategoria(selectedCategoria.categoriaID);
    if (success) {
      setShowDeleteModal(false);
      setSelectedCategoria(null);
    }
  };

  const openEditModal = (categoria: Categoria) => {
    setSelectedCategoria(categoria);
    setEditForm({
      nombre: categoria.nombre,
      descripcion: categoria.descripcion || '',
    });
    setShowEditModal(true);
  };

  const openDeleteModal = (categoria: Categoria) => {
    setSelectedCategoria(categoria);
    setShowDeleteModal(true);
  };

  const filteredCategorias = searchCategorias(searchQuery);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Gestión de Categorías
          </h1>
          <p className="text-stone-gray mt-1">Administra las categorías de productos</p>
          {error && <p className="text-red-600 text-sm mt-1">Error: {error}</p>}
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={loadCategorias} disabled={isLoading}>
            {isLoading ? 'Actualizando...' : 'Actualizar'}
          </Button>
          <Button
            variant="primary"
            leftIcon={<Plus className="w-4 h-4" />}
            onClick={() => setShowCreateModal(true)}
          >
            Nueva Categoría
          </Button>
        </div>
      </div>

      {/* Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-dominican-blue">{totalCategorias}</p>
          <p className="text-sm text-stone-gray">Total Categorías</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-green-600">{totalProductos}</p>
          <p className="text-sm text-stone-gray">Total Productos</p>
        </Card>
      </div>

      {/* Filtros */}
      <Card>
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <Input
              placeholder="Buscar categorías..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              leftIcon={<Search className="w-5 h-5" />}
              fullWidth
            />
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => setSearchQuery('')}>
              Limpiar
            </Button>
          </div>
        </div>
      </Card>

      {/* Tabla de categorías */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="border-b">
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Categoría
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Descripción
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Productos
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={4} className="text-center p-8 text-stone-gray">
                    Cargando categorías...
                  </td>
                </tr>
              ) : filteredCategorias.length === 0 ? (
                <tr>
                  <td colSpan={4} className="text-center p-8 text-stone-gray">
                    No se encontraron categorías
                  </td>
                </tr>
              ) : (
                filteredCategorias.map((categoria) => (
                  <tr key={categoria.categoriaID} className="border-b hover:bg-gray-50">
                    <td className="p-4">
                      <div className="flex items-center">
                        <Package className="w-5 h-5 text-dominican-blue mr-2" />
                        <span className="font-medium text-dominican-blue">{categoria.nombre}</span>
                      </div>
                    </td>
                    <td className="p-4">
                      <span className="text-stone-gray">
                        {categoria.descripcion || 'Sin descripción'}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <div className="flex items-center justify-center gap-2">
                        <span className="font-medium text-dominican-blue">
                          {categoria.totalProductos}
                        </span>
                        {categoria.totalProductos > 0 && (
                          <span className="text-xs text-yellow-600 bg-yellow-100 px-2 py-1 rounded-full">
                            No se puede eliminar
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="p-4">
                      <div className="flex justify-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          leftIcon={<Edit className="w-4 h-4" />}
                          onClick={() => openEditModal(categoria)}
                        >
                          Editar
                        </Button>
                        <Button
                          variant={categoria.totalProductos > 0 ? 'outline' : 'outline'}
                          size="sm"
                          leftIcon={<Trash2 className="w-4 h-4" />}
                          onClick={() => openDeleteModal(categoria)}
                          className={
                            categoria.totalProductos > 0
                              ? 'border-yellow-300 text-yellow-600 hover:bg-yellow-50'
                              : ''
                          }
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

      {/* Modal Crear Categoría */}
      <Modal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        title="Nueva Categoría"
        size="md"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-dominican-blue mb-2">
              Nombre de la Categoría *
            </label>
            <Input
              placeholder="Ej: Platos Principales"
              value={createForm.nombre}
              onChange={(e) => setCreateForm({ ...createForm, nombre: e.target.value })}
              fullWidth
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-dominican-blue mb-2">
              Descripción
            </label>
            <textarea
              placeholder="Descripción opcional de la categoría"
              value={createForm.descripcion}
              onChange={(e) => setCreateForm({ ...createForm, descripcion: e.target.value })}
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
              rows={3}
            />
          </div>
          <div className="flex gap-3 pt-4">
            <Button variant="primary" onClick={handleCreateCategoria} fullWidth>
              Crear Categoría
            </Button>
            <Button variant="outline" onClick={() => setShowCreateModal(false)} fullWidth>
              Cancelar
            </Button>
          </div>
        </div>
      </Modal>

      {/* Modal Editar Categoría */}
      <Modal
        isOpen={showEditModal}
        onClose={() => setShowEditModal(false)}
        title="Editar Categoría"
        size="md"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-dominican-blue mb-2">
              Nombre de la Categoría *
            </label>
            <Input
              placeholder="Ej: Platos Principales"
              value={editForm.nombre}
              onChange={(e) => setEditForm({ ...editForm, nombre: e.target.value })}
              fullWidth
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-dominican-blue mb-2">
              Descripción
            </label>
            <textarea
              placeholder="Descripción opcional de la categoría"
              value={editForm.descripcion}
              onChange={(e) => setEditForm({ ...editForm, descripcion: e.target.value })}
              className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent"
              rows={3}
            />
          </div>
          <div className="flex gap-3 pt-4">
            <Button variant="primary" onClick={handleEditCategoria} fullWidth>
              Actualizar Categoría
            </Button>
            <Button variant="outline" onClick={() => setShowEditModal(false)} fullWidth>
              Cancelar
            </Button>
          </div>
        </div>
      </Modal>

      {/* Modal Eliminar Categoría */}
      <Modal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        title="Eliminar Categoría"
        size="md"
      >
        <div className="space-y-4">
          <div className="flex items-center gap-3 p-4 bg-red-50 rounded-lg">
            <AlertTriangle className="w-6 h-6 text-red-600" />
            <div>
              <h3 className="font-medium text-red-800">¿Estás seguro?</h3>
              <p className="text-sm text-red-600">
                Esta acción no se puede deshacer. La categoría "{selectedCategoria?.nombre}" será
                eliminada permanentemente.
              </p>
            </div>
          </div>

          {selectedCategoria && (selectedCategoria.totalProductos || 0) > 0 && (
            <div className="p-4 bg-yellow-50 rounded-lg border border-yellow-200">
              <div className="flex items-start gap-3">
                <AlertTriangle className="w-5 h-5 text-yellow-600 mt-0.5 flex-shrink-0" />
                <div>
                  <h4 className="font-medium text-yellow-800 mb-2">⚠️ Categoría con productos</h4>
                  <p className="text-sm text-yellow-700 mb-3">
                    Esta categoría tiene{' '}
                    <strong>{selectedCategoria.totalProductos || 0} productos</strong>. Para poder
                    eliminarla, primero debes:
                  </p>
                  <ul className="text-sm text-yellow-700 space-y-1 ml-4">
                    <li>• Eliminar todos los productos de esta categoría, o</li>
                    <li>• Mover los productos a otra categoría</li>
                  </ul>
                  <p className="text-sm text-yellow-700 mt-3 font-medium">
                    Una vez que no queden productos en esta categoría, podrás eliminarla.
                  </p>
                </div>
              </div>
            </div>
          )}

          <div className="flex gap-3 pt-4">
            <Button
              variant="danger"
              onClick={handleDeleteCategoria}
              fullWidth
              disabled={(selectedCategoria?.totalProductos || 0) > 0}
            >
              {selectedCategoria && (selectedCategoria.totalProductos || 0) > 0
                ? 'No se puede eliminar'
                : 'Eliminar Categoría'}
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

export default CategoriasPage;
