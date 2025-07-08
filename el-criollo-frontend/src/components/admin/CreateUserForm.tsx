import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import {
  User,
  Lock,
  Mail,
  Phone,
  MapPin,
  Calendar,
  DollarSign,
  Briefcase,
  Hash,
  ArrowRight,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import { CreateUsuarioRequest } from '@/types/requests';
import { DOMINICAN_PATTERNS } from '@/types/requests';

interface CreateUserFormProps {
  onSubmit: (data: CreateUsuarioRequest) => Promise<boolean>;
  initialData?: Partial<CreateUsuarioRequest>;
}

const ROLES = [
  { id: 1, name: 'Administrador' },
  { id: 2, name: 'Cajero' },
  { id: 3, name: 'Mesero' },
  { id: 4, name: 'Recepcion' },
  { id: 5, name: 'Cocina' },
];

const CreateUserForm: React.FC<CreateUserFormProps> = ({ onSubmit, initialData }) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    watch,
    reset,
  } = useForm<CreateUsuarioRequest>({
    defaultValues: initialData,
  });

  const password = watch('password');

  useEffect(() => {
    if (initialData) {
      reset(initialData);
    }
  }, [initialData, reset]);

  const handleFormSubmit = async (data: CreateUsuarioRequest) => {
    setIsLoading(true);
    const success = await onSubmit(data);
    setIsLoading(false);
    if (success) {
      reset();
    }
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card padding="md">
          <h3 className="font-heading font-semibold text-dominican-blue mb-4 flex items-center">
            <User className="w-5 h-5 mr-2" />
            Información de Usuario
          </h3>
          <div className="space-y-4">
            <Input
              {...register('username', {
                required: 'El nombre de usuario es requerido',
                minLength: { value: 3, message: 'Mínimo 3 caracteres' },
                maxLength: { value: 50, message: 'Máximo 50 caracteres' },
                pattern: {
                  value: /^[a-zA-Z0-9_]+$/,
                  message: 'Solo letras, números y guiones bajos.',
                },
              })}
              label="Nombre de Usuario *"
              placeholder="ej: jose_perez"
              leftIcon={<User className="w-5 h-5" />}
              error={errors.username?.message}
            />
            <Input
              {...register('email', {
                required: 'El email es requerido',
                pattern: {
                  value: DOMINICAN_PATTERNS.email,
                  message: 'Formato de email inválido.',
                },
              })}
              type="email"
              label="Email *"
              placeholder="usuario@ejemplo.com"
              leftIcon={<Mail className="w-5 h-5" />}
              error={errors.email?.message}
            />
            <Input
              {...register('password', {
                required: 'La contraseña es requerida',
                pattern: {
                  value: DOMINICAN_PATTERNS.password,
                  message:
                    'Debe tener 8+ caracteres, mayúscula, minúscula, número y símbolo (@$!%*?&._#-)',
                },
              })}
              type="password"
              label="Contraseña *"
              placeholder="Contraseña segura"
              leftIcon={<Lock className="w-5 h-5" />}
              error={errors.password?.message}
            />
            <Input
              {...register('confirmarPassword', {
                required: 'Debe confirmar la contraseña',
                validate: (value) => value === password || 'Las contraseñas no coinciden.',
              })}
              type="password"
              label="Confirmar Contraseña *"
              placeholder="Repetir contraseña"
              leftIcon={<Lock className="w-5 h-5" />}
              error={errors.confirmarPassword?.message}
            />
            <div>
              <label htmlFor="rolId" className="block text-sm font-medium text-gray-700 mb-1">
                Rol del Sistema *
              </label>
              <select
                id="rolId"
                {...register('rolId', {
                  required: 'El rol es requerido',
                  valueAsNumber: true,
                })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue"
              >
                <option value="">Seleccione un rol...</option>
                {ROLES.map((rol) => (
                  <option key={rol.id} value={rol.id}>
                    {rol.name}
                  </option>
                ))}
              </select>
              {errors.rolId && <p className="mt-1 text-sm text-red-600">{errors.rolId.message}</p>}
            </div>
          </div>
        </Card>

        <Card padding="md">
          <h3 className="font-heading font-semibold text-dominican-blue mb-4 flex items-center">
            <Briefcase className="w-5 h-5 mr-2" />
            Información de Empleado
          </h3>
          <div className="space-y-4">
            <Input
              {...register('cedula', {
                required: 'La cédula es requerida',
                pattern: {
                  value: DOMINICAN_PATTERNS.cedula,
                  message: 'Formato de cédula inválido (ej: 001-0000001-1)',
                },
              })}
              label="Cédula *"
              placeholder="001-0000001-1"
              leftIcon={<Hash className="w-5 h-5" />}
              error={errors.cedula?.message}
            />
            <Input
              {...register('nombre', { required: 'El nombre es requerido' })}
              label="Nombre *"
              placeholder="Juan"
              leftIcon={<User className="w-5 h-5" />}
              error={errors.nombre?.message}
            />
            <Input
              {...register('apellido', { required: 'El apellido es requerido' })}
              label="Apellido *"
              placeholder="Pérez"
              leftIcon={<User className="w-5 h-5" />}
              error={errors.apellido?.message}
            />
            <Input
              {...register('telefono', {
                pattern: {
                  value: DOMINICAN_PATTERNS.telefono,
                  message: 'Formato de teléfono inválido (ej: 809-123-4567)',
                },
              })}
              label="Teléfono"
              placeholder="809-123-4567"
              leftIcon={<Phone className="w-5 h-5" />}
              error={errors.telefono?.message}
            />
            <Input
              {...register('direccion')}
              label="Dirección"
              placeholder="Calle Falsa 123, Santo Domingo"
              leftIcon={<MapPin className="w-5 h-5" />}
              error={errors.direccion?.message}
            />
            <Input
              {...register('salario', { valueAsNumber: true })}
              type="number"
              label="Salario"
              placeholder="25000"
              leftIcon={<DollarSign className="w-5 h-5" />}
              error={errors.salario?.message}
            />
            <Input
              {...register('fechaIngreso')}
              type="date"
              label="Fecha de Ingreso"
              leftIcon={<Calendar className="w-5 h-5" />}
              error={errors.fechaIngreso?.message}
            />
          </div>
        </Card>
      </div>

      <div className="flex justify-end">
        <Button
          type="submit"
          variant="primary"
          isLoading={isLoading}
          disabled={isLoading || !isDirty}
          rightIcon={<ArrowRight className="w-4 h-4" />}
        >
          Crear Usuario
        </Button>
      </div>
    </form>
  );
};

export default CreateUserForm;
