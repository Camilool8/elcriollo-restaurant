import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { Search, Plus, Edit, Trash2, Package, AlertTriangle } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { categoriaService } from '@/services/categoriaService';
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
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [isLoading, setIsLoading] = useState(false);
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

  useEffect(() => {
    loadCategorias();
  }, []);

  const loadCategorias = async () => {
    setIsLoading(true);
    try {
      const response = await categoriaService.getCategorias();
      setCategorias(response || []);
    } catch (error: any) {
      console.error('Error cargando categorías:', error);
      toast.error('Error al cargar categorías');
      setCategorias([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = () => {
    const query = searchQuery.trim().toLowerCase();
    // La búsqueda se hace en el frontend ya que las categorías son pocas
  };

  const handleCreateCategoria = async () => {
    if (!createForm.nombre.trim()) {
      toast.error('El nombre de la categoría es requerido');
      return;
    }

    try {
      await categoriaService.crearCategoria({
        nombre: createForm.nombre.trim(),
        descripcion: createForm.descripcion.trim(),
      });

      toast.success('Categoría creada exitosamente');
      setShowCreateModal(false);
      setCreateForm({ nombre: '', descripcion: '' });
      loadCategorias();
    } catch (error: any) {
      toast.error(error.message || 'Error al crear categoría');
    }
  };

  const handleEditCategoria = async () => {
    if (!selectedCategoria || !editForm.nombre.trim()) {
      toast.error('El nombre de la categoría es requerido');
      return;
    }

    try {
      await categoriaService.actualizarCategoria(selectedCategoria.categoriaID, {
        nombre: editForm.nombre.trim(),
        descripcion: editForm.descripcion.trim(),
      });

      toast.success('Categoría actualizada exitosamente');
      setShowEditModal(false);
      setSelectedCategoria(null);
      setEditForm({ nombre: '', descripcion: '' });
      loadCategorias();
    } catch (error: any) {
      toast.error(error.message || 'Error al actualizar categoría');
    }
  };

  const handleDeleteCategoria = async () => {
    if (!selectedCategoria) return;

    try {
      await categoriaService.eliminarCategoria(selectedCategoria.categoriaID);
      toast.success('Categoría eliminada exitosamente');
      setShowDeleteModal(false);
      setSelectedCategoria(null);
      loadCategorias();
    } catch (error: any) {
      toast.error(error.message || 'Error al eliminar categoría');
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

  const filteredCategorias = categorias.filter(
    (categoria) =>
      categoria.nombre.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (categoria.descripcion &&
        categoria.descripcion.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Gestión de Categorías
          </h1>
          <p className="text-stone-gray mt-1">Administra las categorías de productos</p>
        </div>
        <Button
          variant="primary"
          leftIcon={<Plus className="w-4 h-4" />}
          onClick={() => setShowCreateModal(true)}
        >
          Nueva Categoría
        </Button>
      </div>

      {/* Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-dominican-blue">{categorias.length}</p>
          <p className="text-sm text-stone-gray">Total Categorías</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-green-600">
            {categorias.reduce((sum, cat) => sum + cat.totalProductos, 0)}
          </p>
          <p className="text-sm text-stone-gray">Total Productos</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-blue-600">
            {categorias.reduce((sum, cat) => sum + cat.productosActivos, 0)}
          </p>
          <p className="text-sm text-stone-gray">Productos Disponibles</p>
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
                  Disponibles
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={5} className="text-center p-8 text-stone-gray">
                    Cargando categorías...
                  </td>
                </tr>
              ) : filteredCategorias.length === 0 ? (
                <tr>
                  <td colSpan={5} className="text-center p-8 text-stone-gray">
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
                      <span className="font-medium text-dominican-blue">
                        {categoria.totalProductos}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <span
                        className={`font-medium ${
                          categoria.productosActivos > 0 ? 'text-green-600' : 'text-red-600'
                        }`}
                      >
                        {categoria.productosActivos}
                      </span>
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
                          variant="outline"
                          size="sm"
                          leftIcon={<Trash2 className="w-4 h-4" />}
                          onClick={() => openDeleteModal(categoria)}
                          disabled={categoria.totalProductos > 0}
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
            <div className="p-4 bg-yellow-50 rounded-lg">
              <p className="text-sm text-yellow-800">
                ⚠️ Esta categoría tiene {selectedCategoria.totalProductos || 0} productos. No se
                puede eliminar hasta que se muevan o eliminen todos los productos.
              </p>
            </div>
          )}

          <div className="flex gap-3 pt-4">
            <Button
              variant="danger"
              onClick={handleDeleteCategoria}
              fullWidth
              disabled={(selectedCategoria?.totalProductos || 0) > 0}
            >
              Eliminar Categoría
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
