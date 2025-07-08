import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import {
  Search,
  Plus,
  Edit,
  Eye,
  Phone,
  Mail,
  MapPin,
  Calendar,
  UserCheck,
  UserX,
} from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import CreateClienteForm from '@/components/admin/CreateClienteForm';
import { clienteService } from '@/services/clienteService';
import { Cliente, SearchClienteParams } from '@/types';

const ClientesPage: React.FC = () => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedCliente, setSelectedCliente] = useState<Cliente | null>(null);

  // ====================================
  // EFECTOS
  // ====================================

  useEffect(() => {
    loadClientes();
  }, []);

  // ====================================
  // FUNCIONES
  // ====================================

  const loadClientes = async (params?: SearchClienteParams) => {
    setIsLoading(true);
    try {
      const response = await clienteService.getClientes(params);
      setClientes(response.items || []);
    } catch (error: any) {
      console.error('Error cargando clientes:', error);
      toast.error('Error al cargar clientes');
      // Datos de ejemplo para desarrollo
      setClientes([
        {
          clienteID: 1,
          nombre: 'María',
          apellido: 'González',
          cedula: '123-1234567-1',
          telefono: '809-123-4567',
          email: 'maria@ejemplo.com',
          estado: 'Activo',
          fechaRegistro: '2024-01-15T00:00:00Z',
        },
        {
          clienteID: 2,
          nombre: 'Juan',
          apellido: 'Pérez',
          telefono: '829-987-6543',
          email: 'juan@ejemplo.com',
          estado: 'Activo',
          fechaRegistro: '2024-02-01T00:00:00Z',
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = () => {
    if (searchQuery.trim()) {
      loadClientes({ query: searchQuery.trim() });
    } else {
      loadClientes();
    }
  };

  const handleClienteCreated = (newCliente: Cliente) => {
    setClientes((prev) => [newCliente, ...prev]);
  };

  const toggleClienteStatus = async (clienteId: number, currentStatus: string) => {
    try {
      if (currentStatus === 'Activo') {
        await clienteService.deactivateCliente(clienteId);
        toast.success('Cliente desactivado');
      } else {
        // Lógica para reactivar (no existe endpoint específico)
        toast.info('Función de reactivación no disponible');
      }
      loadClientes();
    } catch (error: any) {
      toast.error(error.message || 'Error al cambiar estado del cliente');
    }
  };

  // ====================================
  // RENDER
  // ====================================

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Gestión de Clientes
          </h1>
          <p className="text-stone-gray mt-1">Base de datos de clientes registrados</p>
        </div>

        <Button
          variant="primary"
          leftIcon={<Plus className="w-4 h-4" />}
          onClick={() => setShowCreateForm(true)}
        >
          Registrar Cliente
        </Button>
      </div>

      {/* Métricas rápidas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-dominican-blue">{clientes.length}</p>
          <p className="text-sm text-stone-gray">Total Clientes</p>
        </Card>

        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-green-600">
            {clientes.filter((c) => c.estado === 'Activo').length}
          </p>
          <p className="text-sm text-stone-gray">Activos</p>
        </Card>

        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-yellow-600">
            {clientes.filter((c) => c.email).length}
          </p>
          <p className="text-sm text-stone-gray">Con Email</p>
        </Card>

        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-blue-600">
            {clientes.filter((c) => c.telefono).length}
          </p>
          <p className="text-sm text-stone-gray">Con Teléfono</p>
        </Card>
      </div>

      {/* Filtros y búsqueda */}
      <Card>
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <Input
              placeholder="Buscar por nombre, apellido, cédula o teléfono..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              leftIcon={<Search className="w-5 h-5" />}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
              fullWidth
            />
          </div>

          <div className="flex gap-2">
            <Button variant="primary" onClick={handleSearch}>
              Buscar
            </Button>

            <Button
              variant="outline"
              onClick={() => {
                setSearchQuery('');
                loadClientes();
              }}
            >
              Limpiar
            </Button>
          </div>
        </div>
      </Card>

      {/* Lista de clientes */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="border-b">
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Cliente
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Contacto
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Identificación
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Estado
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Registro
                </th>
                <th className="text-center p-4 font-heading font-semibold text-dominican-blue">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="text-center p-8 text-stone-gray">
                    Cargando clientes...
                  </td>
                </tr>
              ) : clientes.length === 0 ? (
                <tr>
                  <td colSpan={6} className="text-center p-8 text-stone-gray">
                    No se encontraron clientes
                  </td>
                </tr>
              ) : (
                clientes.map((cliente) => (
                  <tr key={cliente.clienteID} className="border-b hover:bg-gray-50">
                    <td className="p-4">
                      <div className="font-medium text-gray-900">
                        {cliente.nombre} {cliente.apellido}
                      </div>
                      {cliente.preferenciasComida && (
                        <div className="text-sm text-stone-gray">{cliente.preferenciasComida}</div>
                      )}
                    </td>

                    <td className="p-4">
                      <div className="space-y-1">
                        {cliente.telefono && (
                          <div className="flex items-center text-sm text-gray-600">
                            <Phone className="w-3 h-3 mr-1" />
                            {cliente.telefono}
                          </div>
                        )}
                        {cliente.email && (
                          <div className="flex items-center text-sm text-gray-600">
                            <Mail className="w-3 h-3 mr-1" />
                            {cliente.email}
                          </div>
                        )}
                        {cliente.direccion && (
                          <div className="flex items-center text-sm text-gray-600">
                            <MapPin className="w-3 h-3 mr-1" />
                            {cliente.direccion}
                          </div>
                        )}
                      </div>
                    </td>

                    <td className="p-4">
                      {cliente.cedula ? (
                        <div className="text-sm text-gray-600">{cliente.cedula}</div>
                      ) : (
                        <span className="text-stone-gray text-sm">Sin cédula</span>
                      )}
                    </td>

                    <td className="p-4">
                      <Badge variant={cliente.estado === 'Activo' ? 'success' : 'danger'}>
                        {cliente.estado}
                      </Badge>
                    </td>

                    <td className="p-4 text-gray-600">
                      <div className="flex items-center text-sm">
                        <Calendar className="w-3 h-3 mr-1" />
                        {new Date(cliente.fechaRegistro).toLocaleDateString('es-DO')}
                      </div>
                    </td>

                    <td className="p-4">
                      <div className="flex items-center justify-center space-x-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setSelectedCliente(cliente)}
                        >
                          <Eye className="w-4 h-4" />
                        </Button>

                        <Button variant="ghost" size="sm">
                          <Edit className="w-4 h-4 text-blue-600" />
                        </Button>

                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => toggleClienteStatus(cliente.clienteID, cliente.estado)}
                        >
                          {cliente.estado === 'Activo' ? (
                            <UserX className="w-4 h-4 text-red-600" />
                          ) : (
                            <UserCheck className="w-4 h-4 text-green-600" />
                          )}
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

      {/* Modal crear cliente */}
      <CreateClienteForm
        isOpen={showCreateForm}
        onClose={() => setShowCreateForm(false)}
        onSuccess={handleClienteCreated}
      />
    </div>
  );
};

export default ClientesPage;
