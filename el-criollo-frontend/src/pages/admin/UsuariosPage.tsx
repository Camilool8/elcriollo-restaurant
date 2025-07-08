import React from 'react';
import { toast } from 'react-toastify';
import { UserPlus } from 'lucide-react';
import CreateUserForm from '@/components/admin/CreateUserForm';
import { CreateUsuarioRequest } from '@/types/requests';
import { UsuarioResponse } from '@/types';
import { adminUserService } from '@/services/adminService';

const UsuariosPage: React.FC = () => {
  const handleUserCreated = (newUser: UsuarioResponse) => {
    toast.success(`Usuario "${newUser.usuario}" creado exitosamente.`);
    // Optionally, you can reset the form or redirect here
  };

  const handleCreateUserError = (error: Error) => {
    toast.error(error.message || 'Error al crear el usuario.');
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <UserPlus className="w-8 h-8 text-dominican-blue" />
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Crear Nuevo Usuario
          </h1>
          <p className="text-stone-gray mt-1">
            Complete el formulario para registrar un nuevo empleado y su cuenta de usuario en el
            sistema.
          </p>
        </div>
      </div>

      <CreateUserForm
        onSubmit={async (data: CreateUsuarioRequest) => {
          try {
            const newUser = await adminUserService.createUser(data);
            handleUserCreated(newUser);
            return true; // Indicate success
          } catch (error: any) {
            handleCreateUserError(error);
            return false; // Indicate failure
          }
        }}
      />
    </div>
  );
};

export default UsuariosPage;
