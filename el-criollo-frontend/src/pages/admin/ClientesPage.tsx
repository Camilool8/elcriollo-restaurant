import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { Search, Plus, Edit, Eye, Phone, Mail, UserCheck, UserX } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import CreateClienteForm from '@/components/admin/CreateClienteForm';
import { EditClienteForm } from '@/components/admin/EditClienteForm';
import { ClienteDetails } from '@/components/admin/ClienteDetails';
import { clienteService } from '@/services/clienteService';
import { Cliente } from '@/types';

const ClientesPage: React.FC = () => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [originalClientes, setOriginalClientes] = useState<Cliente[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingCliente, setEditingCliente] = useState<Cliente | null>(null);
  const [viewingCliente, setViewingCliente] = useState<Cliente | null>(null);

  useEffect(() => {
    loadClientes();
  }, []);

  const loadClientes = async () => {
    setIsLoading(true);
    try {
      const response = await clienteService.getClientes();
      const allClientes = response || [];
      setOriginalClientes(allClientes);
      setClientes(allClientes.filter((c) => c.estado)); // Show only active clients by default
    } catch (error: any) {
      console.error('Error cargando clientes:', error);
      toast.error('Error al cargar clientes');
      setClientes([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = () => {
    const query = searchQuery.trim().toLowerCase();

    const filtered = originalClientes.filter((cliente) => {
      const isActive = cliente.estado;
      if (!isActive) return false;

      if (!query) return true;

      return (
        cliente.nombreCompleto.toLowerCase().includes(query) ||
        (cliente.telefono && cliente.telefono.includes(query)) ||
        (cliente.email && cliente.email.toLowerCase().includes(query)) ||
        (cliente.cedula && cliente.cedula.includes(query))
      );
    });

    setClientes(filtered);
  };

  const handleClienteCreated = (newCliente: Cliente) => {
    const newOriginals = [newCliente, ...originalClientes];
    setOriginalClientes(newOriginals);
    setClientes(newOriginals.filter((c) => c.estado));
  };

  const handleClienteUpdated = () => {
    loadClientes();
  };

  const toggleClienteStatus = async (clienteId: number, currentStatus: boolean) => {
    try {
      if (currentStatus) {
        await clienteService.deactivateCliente(clienteId);
        toast.success('Cliente desactivado');
      } else {
        toast.info('Función de reactivación no disponible');
      }
      loadClientes();
    } catch (error: any) {
      toast.error(error.message || 'Error al cambiar estado del cliente');
    }
  };

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

      {/* Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-dominican-blue">{clientes.length}</p>
          <p className="text-sm text-stone-gray">Total Clientes</p>
        </Card>
        <Card className="text-center" padding="sm">
          <p className="text-2xl font-bold text-green-600">
            {clientes.filter((c) => c.estado).length}
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

      {/* Filtros */}
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
                setClientes(originalClientes.filter((c) => c.estado));
              }}
            >
              Limpiar
            </Button>
          </div>
        </div>
      </Card>

      {/* Tabla de clientes */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="border-b">
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Nombre
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Contacto
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Dirección
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
                      <div className="font-medium text-gray-900">{cliente.nombreCompleto}</div>
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
                      </div>
                    </td>
                    <td className="p-4 text-sm text-stone-gray">
                      {cliente.direccion || 'No especificada'}
                    </td>
                    <td className="p-4 text-sm text-stone-gray">
                      {new Date(cliente.fechaRegistro).toLocaleDateString()}
                    </td>
                    <td className="p-4 text-center">
                      <div className="flex justify-center space-x-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          title="Ver detalles"
                          onClick={() => setViewingCliente(cliente)}
                        >
                          <Eye className="w-4 h-4 text-blue-600" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          title="Editar cliente"
                          onClick={() => setEditingCliente(cliente)}
                        >
                          <Edit className="w-4 h-4 text-yellow-600" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          title={cliente.estado ? 'Desactivar cliente' : 'Activar cliente'}
                          onClick={() => toggleClienteStatus(cliente.clienteID, cliente.estado)}
                        >
                          {cliente.estado ? (
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

      {/* Forms & Modals */}
      {showCreateForm && (
        <CreateClienteForm
          isOpen={showCreateForm}
          onClose={() => setShowCreateForm(false)}
          onSuccess={handleClienteCreated}
        />
      )}
      {editingCliente && (
        <EditClienteForm
          cliente={editingCliente}
          isOpen={!!editingCliente}
          onClose={() => setEditingCliente(null)}
          onSuccess={handleClienteUpdated}
        />
      )}
      {viewingCliente && (
        <ClienteDetails
          cliente={viewingCliente}
          isOpen={!!viewingCliente}
          onClose={() => setViewingCliente(null)}
        />
      )}
    </div>
  );
};

export default ClientesPage;
