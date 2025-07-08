import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Mail, Phone, User, MapPin } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import { Empleado } from '@/types';
import { UpdateEmpleadoRequest } from '@/types/requests';
import { empleadoService } from '@/services/empleadoService';
import { toast } from 'react-toastify';

interface EditEmpleadoFormProps {
  empleado: Empleado;
  onSuccess: () => void;
  onCancel: () => void;
}

export const EditEmpleadoForm: React.FC<EditEmpleadoFormProps> = ({
  empleado,
  onSuccess,
  onCancel,
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    reset,
  } = useForm<UpdateEmpleadoRequest>({
    defaultValues: {
      nombreCompleto: empleado.nombreCompleto,
      email: empleado.email,
      telefono: empleado.telefono,
      direccion: empleado.direccion,
    },
  });

  useEffect(() => {
    reset({
      nombreCompleto: empleado.nombreCompleto,
      email: empleado.email,
      telefono: empleado.telefono,
      direccion: empleado.direccion,
    });
  }, [empleado, reset]);

  const handleFormSubmit = async (data: UpdateEmpleadoRequest) => {
    setIsLoading(true);
    try {
      await empleadoService.updateEmpleado(empleado.empleadoID, data);
      toast.success('¡Empleado actualizado exitosamente!');
      onSuccess();
    } catch (error: any) {
      toast.error(error.message || 'Error al actualizar el empleado.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      <Card padding="md">
        <div className="space-y-4">
          <Input
            {...register('nombreCompleto', {
              required: 'El nombre es requerido',
            })}
            label="Nombre Completo *"
            placeholder="Juan Pérez"
            leftIcon={<User className="w-5 h-5" />}
            error={errors.nombreCompleto?.message}
          />
          <Input
            {...register('email', {
              required: 'El email es requerido',
              pattern: {
                value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
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
            {...register('telefono', {
              pattern: {
                value: /^\d{3}-\d{3}-\d{4}$/,
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
        </div>
      </Card>

      <div className="flex justify-end space-x-3">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isLoading}>
          Cancelar
        </Button>
        <Button
          type="submit"
          variant="primary"
          isLoading={isLoading}
          disabled={isLoading || !isDirty}
        >
          Guardar Cambios
        </Button>
      </div>
    </form>
  );
};
