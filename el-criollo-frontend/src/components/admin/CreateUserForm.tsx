import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import {
  User,
  Lock,
  Mail,
  UserIcon,
  Phone,
  MapPin,
  Calendar,
  DollarSign,
  Briefcase,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import { Modal } from '@/components/ui/Modal';
import { CreateUsuarioRequest, Rol } from '@/types/requests';
import { adminUserService } from '@/services/adminService';
import { DOMINICAN_PATTERNS } from '@/types/requests';

// ====================================
// TIPOS
// ====================================

interface CreateUserFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (user: any) => void;
}

interface UserFormData {
  // Usuario
  usuario: string;
  email: string;
  contrasena: string;
  confirmarContrasena: string;
  rolID: number;

  // Empleado
  cedula: string;
  nombre: string;
  apellido: string;
  sexo: string;
  direccion: string;
  telefono: string;
  fechaNacimiento: string;
  salario: number;
  departamento: string;
}

// ====================================
// COMPONENTE PRINCIPAL
// ====================================

const CreateUserForm: React.FC<CreateUserFormProps> = ({ isOpen, onClose, onSuccess }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [roles, setRoles] = useState<Rol[]>([]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    reset,
  } = useForm<UserFormData>();

  const password = watch('contrasena');

  // ====================================
  // EFECTOS
  // ====================================

  useEffect(() => {
    if (isOpen) {
      loadRoles();
    }
  }, [isOpen]);

  // ====================================
  // FUNCIONES
  // ====================================

  const loadRoles = async () => {
    try {
      const rolesData = await adminUserService.getRoles();
      setRoles(rolesData);
    } catch (error) {
      console.error('Error cargando roles:', error);
      // Roles por defecto si no se pueden cargar
      setRoles([
        { rolID: 1, nombreRol: 'Administrador', estado: true },
        { rolID: 2, nombreRol: 'Cajero', estado: true },
        { rolID: 3, nombreRol: 'Mesero', estado: true },
        { rolID: 4, nombreRol: 'Recepcion', estado: true },
        { rolID: 5, nombreRol: 'Cocina', estado: true },
      ]);
    }
  };

  const onSubmit = async (data: UserFormData) => {
    setIsLoading(true);

    try {
      const createRequest: CreateUsuarioRequest = {
        // Datos del usuario
        usuario: data.usuario.trim(),
        email: data.email.trim(),
        contrasena: data.contrasena,
        rolID: Number(data.rolID),

        // Datos del empleado
        cedula: data.cedula.trim(),
        nombre: data.nombre.trim(),
        apellido: data.apellido.trim(),
        sexo: data.sexo,
        direccion: data.direccion?.trim() || undefined,
        telefono: data.telefono?.trim() || undefined,
        fechaNacimiento: data.fechaNacimiento || undefined,
        salario: data.salario ? Number(data.salario) : undefined,
        departamento: data.departamento?.trim() || undefined,
      };

      const newUser = await adminUserService.createUser(createRequest);

      toast.success('¡Usuario creado exitosamente! 🎉');
      onSuccess(newUser);
      reset();
      onClose();
    } catch (error: any) {
      console.error('Error creando usuario:', error);
      toast.error(error.message || 'Error al crear usuario');
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  // ====================================
  // RENDER
  // ====================================

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Crear Nuevo Usuario" size="lg">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Información del Usuario */}
        <Card padding="sm" className="bg-blue-50">
          <h3 className="font-heading font-semibold text-dominican-blue mb-4 flex items-center">
            <User className="w-5 h-5 mr-2" />
            Información de Usuario
          </h3>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              {...register('usuario', {
                required: 'El usuario es requerido',
                minLength: { value: 3, message: 'Mínimo 3 caracteres' },
                maxLength: { value: 50, message: 'Máximo 50 caracteres' },
              })}
              label="Usuario *"
              placeholder="Nombre de usuario"
              leftIcon={<User className="w-5 h-5" />}
              error={errors.usuario?.message}
              fullWidth
            />

            <Input
              {...register('email', {
                required: 'El email es requerido',
                pattern: {
                  value: DOMINICAN_PATTERNS.email,
                  message: 'Email inválido',
                },
              })}
              type="email"
              label="Email *"
              placeholder="usuario@ejemplo.com"
              leftIcon={<Mail className="w-5 h-5" />}
              error={errors.email?.message}
              fullWidth
            />

            <Input
              {...register('contrasena', {
                required: 'La contraseña es requerida',
                minLength: { value: 6, message: 'Mínimo 6 caracteres' },
              })}
              type="password"
              label="Contraseña *"
              placeholder="Contraseña segura"
              leftIcon={<Lock className="w-5 h-5" />}
              error={errors.contrasena?.message}
              fullWidth
            />

            <Input
              {...register('confirmarContrasena', {
                required: 'Confirma la contraseña',
                validate: (value) => value === password || 'Las contraseñas no coinciden',
              })}
              type="password"
              label="Confirmar Contraseña *"
              placeholder="Repetir contraseña"
              leftIcon={<Lock className="w-5 h-5" />}
              error={errors.confirmarContrasena?.message}
              fullWidth
            />

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Rol *</label>
              <select
                {...register('rolID', {
                  required: 'Selecciona un rol',
                })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
              >
                <option value="">Seleccionar rol...</option>
                {roles.map((rol) => (
                  <option key={rol.rolID} value={rol.rolID}>
                    {rol.nombreRol}
                  </option>
                ))}
              </select>
              {errors.rolID && <p className="mt-1 text-sm text-red-600">{errors.rolID.message}</p>}
            </div>
          </div>
        </Card>

        {/* Información del Empleado */}
        <Card padding="sm" className="bg-green-50">
          <h3 className="font-heading font-semibold text-dominican-blue mb-4 flex items-center">
            <Briefcase className="w-5 h-5 mr-2" />
            Información de Empleado
          </h3>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              {...register('cedula', {
                required: 'La cédula es requerida',
                pattern: {
                  value: DOMINICAN_PATTERNS.cedula,
                  message: 'Formato: 123-1234567-1',
                },
              })}
              label="Cédula *"
              placeholder="123-1234567-1"
              leftIcon={<UserIcon className="w-5 h-5" />}
              error={errors.cedula?.message}
              fullWidth
            />

            <Input
              {...register('nombre', {
                required: 'El nombre es requerido',
                maxLength: { value: 50, message: 'Máximo 50 caracteres' },
              })}
              label="Nombre *"
              placeholder="Nombre del empleado"
              error={errors.nombre?.message}
              fullWidth
            />

            <Input
              {...register('apellido', {
                required: 'El apellido es requerido',
                maxLength: { value: 50, message: 'Máximo 50 caracteres' },
              })}
              label="Apellido *"
              placeholder="Apellido del empleado"
              error={errors.apellido?.message}
              fullWidth
            />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Sexo</label>
              <select
                {...register('sexo')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
              >
                <option value="">Seleccionar...</option>
                <option value="Masculino">Masculino</option>
                <option value="Femenino">Femenino</option>
                <option value="Otro">Otro</option>
              </select>
            </div>

            <Input
              {...register('telefono', {
                pattern: {
                  value: DOMINICAN_PATTERNS.telefono,
                  message: 'Formato: 809-123-4567',
                },
              })}
              label="Teléfono"
              placeholder="809-123-4567"
              leftIcon={<Phone className="w-5 h-5" />}
              error={errors.telefono?.message}
              fullWidth
            />

            <Input
              {...register('direccion')}
              label="Dirección"
              placeholder="Dirección completa"
              leftIcon={<MapPin className="w-5 h-5" />}
              error={errors.direccion?.message}
              fullWidth
            />

            <Input
              {...register('fechaNacimiento')}
              type="date"
              label="Fecha de Nacimiento"
              leftIcon={<Calendar className="w-5 h-5" />}
              error={errors.fechaNacimiento?.message}
              fullWidth
            />

            <Input
              {...register('salario', {
                min: { value: 0, message: 'Debe ser mayor a 0' },
              })}
              type="number"
              step="0.01"
              label="Salario (RD$)"
              placeholder="0.00"
              leftIcon={<DollarSign className="w-5 h-5" />}
              error={errors.salario?.message}
              fullWidth
            />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Departamento</label>
              <select
                {...register('departamento')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
              >
                <option value="">Seleccionar...</option>
                <option value="Administración">Administración</option>
                <option value="Cocina">Cocina</option>
                <option value="Servicio">Servicio</option>
                <option value="Caja">Caja</option>
                <option value="Recepción">Recepción</option>
                <option value="Limpieza">Limpieza</option>
                <option value="Seguridad">Seguridad</option>
              </select>
            </div>
          </div>
        </Card>

        {/* Botones */}
        <div className="flex justify-end space-x-3 pt-4 border-t">
          <Button type="button" variant="outline" onClick={handleClose} disabled={isLoading}>
            Cancelar
          </Button>

          <Button type="submit" variant="primary" isLoading={isLoading} disabled={isLoading}>
            Crear Usuario
          </Button>
        </div>
      </form>
    </Modal>
  );
};

export default CreateUserForm;
