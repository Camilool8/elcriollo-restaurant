import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { Search, Plus, Eye, RotateCcw, UserCheck, UserX } from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import CreateUserForm from '@/components/admin/CreateUserForm';
import { adminUserService } from '@/services/adminService';
import { User, SearchUsuarioParams } from '@/types';

const UsuariosPage: React.FC = () => {
  const [usuarios, setUsuarios] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);

  // ====================================
  // EFECTOS
  // ====================================

  useEffect(() => {
    loadUsuarios();
  }, []);

  // ====================================
  // FUNCIONES
  // ====================================

  const loadUsuarios = async (params?: SearchUsuarioParams) => {
    setIsLoading(true);
    try {
      const response = await adminUserService.getUsers(params);
      setUsuarios(response.items || []);
    } catch (error: any) {
      console.error('Error cargando usuarios:', error);
      toast.error('Error al cargar usuarios');
      // Datos de ejemplo para desarrollo
      setUsuarios([
        {
          usuarioID: 1,
          usuario: 'thecuevas0123_',
          email: 'josejoga.opx@gmail.com',
          rolID: 1,
          nombreRol: 'Administrador',
          estado: true,
          fechaCreacion: '2024-01-01T00:00:00Z',
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = () => {
    if (searchQuery.trim()) {
      loadUsuarios({ query: searchQuery.trim() });
    } else {
      loadUsuarios();
    }
  };

  const handleUserCreated = (newUser: User) => {
    setUsuarios((prev) => [newUser, ...prev]);
  };

  const toggleUserStatus = async (userId: number, currentStatus: boolean) => {
    try {
      if (currentStatus) {
        await adminUserService.deactivateUser(userId);
        toast.success('Usuario desactivado');
      } else {
        await adminUserService.activateUser(userId);
        toast.success('Usuario activado');
      }
      loadUsuarios();
    } catch (error: any) {
      toast.error(error.message || 'Error al cambiar estado del usuario');
    }
  };

  const getRoleBadgeVariant = (role: string) => {
    switch (role) {
      case 'Administrador':
        return 'danger';
      case 'Cajero':
        return 'primary';
      case 'Mesero':
        return 'success';
      case 'Recepcion':
        return 'warning';
      case 'Cocina':
        return 'info';
      default:
        return 'secondary';
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
            Gestión de Usuarios
          </h1>
          <p className="text-stone-gray mt-1">Administra usuarios del sistema y sus roles</p>
        </div>

        <Button
          variant="primary"
          leftIcon={<Plus className="w-4 h-4" />}
          onClick={() => setShowCreateForm(true)}
        >
          Crear Usuario
        </Button>
      </div>

      {/* Filtros y búsqueda */}
      <Card>
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <Input
              placeholder="Buscar por usuario, email o nombre..."
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
                loadUsuarios();
              }}
            >
              Limpiar
            </Button>
          </div>
        </div>
      </Card>

      {/* Lista de usuarios */}
      <Card>
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="border-b">
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Usuario
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Email
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Rol
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Estado
                </th>
                <th className="text-left p-4 font-heading font-semibold text-dominican-blue">
                  Fecha Creación
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
                    Cargando usuarios...
                  </td>
                </tr>
              ) : usuarios.length === 0 ? (
                <tr>
                  <td colSpan={6} className="text-center p-8 text-stone-gray">
                    No se encontraron usuarios
                  </td>
                </tr>
              ) : (
                usuarios.map((usuario) => (
                  <tr key={usuario.usuarioID} className="border-b hover:bg-gray-50">
                    <td className="p-4">
                      <div className="font-medium text-gray-900">{usuario.usuario}</div>
                    </td>

                    <td className="p-4 text-gray-600">{usuario.email}</td>

                    <td className="p-4">
                      <Badge variant={getRoleBadgeVariant(usuario.nombreRol)}>
                        {usuario.nombreRol}
                      </Badge>
                    </td>

                    <td className="p-4">
                      <Badge variant={usuario.estado ? 'success' : 'danger'}>
                        {usuario.estado ? 'Activo' : 'Inactivo'}
                      </Badge>
                    </td>

                    <td className="p-4 text-gray-600">
                      {new Date(usuario.fechaCreacion).toLocaleDateString('es-DO')}
                    </td>

                    <td className="p-4">
                      <div className="flex items-center justify-center space-x-2">
                        <Button variant="ghost" size="sm" onClick={() => setSelectedUser(usuario)}>
                          <Eye className="w-4 h-4" />
                        </Button>

                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => toggleUserStatus(usuario.usuarioID, usuario.estado)}
                        >
                          {usuario.estado ? (
                            <UserX className="w-4 h-4 text-red-600" />
                          ) : (
                            <UserCheck className="w-4 h-4 text-green-600" />
                          )}
                        </Button>

                        <Button variant="ghost" size="sm">
                          <RotateCcw className="w-4 h-4 text-blue-600" />
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

      {/* Modal crear usuario */}
      <CreateUserForm
        isOpen={showCreateForm}
        onClose={() => setShowCreateForm(false)}
        onSuccess={handleUserCreated}
      />
    </div>
  );
};

export default UsuariosPage;
