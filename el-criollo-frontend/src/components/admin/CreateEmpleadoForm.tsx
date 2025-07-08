import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import {
  UserPlus,
  UserIcon,
  Phone,
  Mail,
  MapPin,
  Calendar,
  DollarSign,
  Briefcase,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { FormField } from '@/components/ui/FormField';
import { Modal } from '@/components/ui/Modal';
import { CreateEmpleadoRequest } from '@/types/requests';
import { empleadoService } from '@/services/empleadoService';
import { DEPARTAMENTOS_RD, SEXOS } from '@/utils/dominicanValidations';

interface CreateEmpleadoFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

const CreateEmpleadoForm: React.FC<CreateEmpleadoFormProps> = ({ isOpen, onClose, onSuccess }) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateEmpleadoRequest>();

  const onSubmit = async (data: CreateEmpleadoRequest) => {
    setIsLoading(true);

    try {
      const createRequest: CreateEmpleadoRequest = {
        cedula: data.cedula.trim(),
        nombre: data.nombre.trim(),
        apellido: data.apellido.trim(),
        sexo: data.sexo || undefined,
        direccion: data.direccion?.trim() || undefined,
        telefono: data.telefono?.trim() || undefined,
        email: data.email?.trim() || undefined,
        fechaNacimiento: data.fechaNacimiento || undefined,
        salario: data.salario ? Number(data.salario) : undefined,
        departamento: data.departamento?.trim() || undefined,
      };

      await empleadoService.createEmpleado(createRequest);

      toast.success('¡Empleado agregado exitosamente! 💼');
      onSuccess();
      reset();
      onClose();
    } catch (error: any) {
      console.error('Error creando empleado:', error);
      toast.error(error.message || 'Error al agregar empleado');
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Agregar Nuevo Empleado" size="lg">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <div className="text-center mb-4">
          <div className="w-16 h-16 bg-dominican-blue bg-opacity-10 rounded-full flex items-center justify-center mx-auto mb-2">
            <UserPlus className="w-8 h-8 text-dominican-blue" />
          </div>
          <p className="text-stone-gray">Agrega un nuevo miembro al equipo</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Información básica */}
          <FormField
            {...register('cedula', {
              required: 'La cédula es requerida',
            })}
            label="Cédula"
            placeholder="123-1234567-1"
            leftIcon={<UserIcon className="w-5 h-5" />}
            error={errors.cedula?.message}
            validation="cedula"
            autoFormat
            fullWidth
          />

          <FormField
            {...register('nombre', {
              required: 'El nombre es requerido',
              maxLength: { value: 50, message: 'Máximo 50 caracteres' },
            })}
            label="Nombre"
            placeholder="Nombre del empleado"
            error={errors.nombre?.message}
            required
            fullWidth
          />

          <FormField
            {...register('apellido', {
              required: 'El apellido es requerido',
              maxLength: { value: 50, message: 'Máximo 50 caracteres' },
            })}
            label="Apellido"
            placeholder="Apellido del empleado"
            error={errors.apellido?.message}
            required
            fullWidth
          />

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Sexo</label>
            <select
              {...register('sexo')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
            >
              <option value="">Seleccionar...</option>
              {SEXOS.map((sexo) => (
                <option key={sexo} value={sexo}>
                  {sexo}
                </option>
              ))}
            </select>
          </div>

          {/* Contacto */}
          <FormField
            {...register('telefono')}
            label="Teléfono"
            placeholder="809-123-4567"
            leftIcon={<Phone className="w-5 h-5" />}
            error={errors.telefono?.message}
            validation="telefono"
            autoFormat
            fullWidth
          />

          <FormField
            {...register('email')}
            type="email"
            label="Email"
            placeholder="empleado@ejemplo.com"
            leftIcon={<Mail className="w-5 h-5" />}
            error={errors.email?.message}
            validation="email"
            fullWidth
          />

          <div className="md:col-span-2">
            <FormField
              {...register('direccion')}
              label="Dirección"
              placeholder="Dirección completa"
              leftIcon={<MapPin className="w-5 h-5" />}
              error={errors.direccion?.message}
              fullWidth
            />
          </div>

          {/* Información laboral */}
          <FormField
            {...register('fechaNacimiento')}
            type="date"
            label="Fecha de Nacimiento"
            leftIcon={<Calendar className="w-5 h-5" />}
            error={errors.fechaNacimiento?.message}
            fullWidth
          />

          <FormField
            {...register('salario', {
              min: { value: 0, message: 'Debe ser mayor a 0' },
            })}
            type="number"
            step="0.01"
            label="Salario (RD$)"
            placeholder="0.00"
            leftIcon={<DollarSign className="w-5 h-5" />}
            error={errors.salario?.message}
            validation="precio"
            fullWidth
          />

          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              <Briefcase className="w-4 h-4 inline mr-1" />
              Departamento
            </label>
            <select
              {...register('departamento')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
            >
              <option value="">Seleccionar departamento...</option>
              {DEPARTAMENTOS_RD.map((departamento) => (
                <option key={departamento} value={departamento}>
                  {departamento}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Botones */}
        <div className="flex justify-end space-x-3 pt-4 border-t">
          <Button type="button" variant="outline" onClick={handleClose} disabled={isLoading}>
            Cancelar
          </Button>

          <Button type="submit" variant="primary" isLoading={isLoading} disabled={isLoading}>
            Agregar Empleado
          </Button>
        </div>
      </form>
    </Modal>
  );
};

export { CreateEmpleadoForm };
