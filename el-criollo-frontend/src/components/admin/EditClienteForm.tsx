import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Mail, Phone, User, MapPin, Calendar, Heart } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { Cliente } from '@/types';
import { UpdateClienteRequest } from '@/types/requests';
import { clienteService } from '@/services/clienteService';
import { toast } from 'react-toastify';

interface EditClienteFormProps {
  cliente: Cliente;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const EditClienteForm: React.FC<EditClienteFormProps> = ({
  cliente,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    reset,
  } = useForm<UpdateClienteRequest>({
    defaultValues: {
      nombreCompleto: cliente.nombreCompleto,
      email: cliente.email,
      telefono: cliente.telefono,
      direccion: cliente.direccion,
      fechaNacimiento: cliente.fechaNacimiento
        ? new Date(cliente.fechaNacimiento).toISOString().split('T')[0]
        : '',
    },
  });

  useEffect(() => {
    reset({
      nombreCompleto: cliente.nombreCompleto,
      email: cliente.email,
      telefono: cliente.telefono,
      direccion: cliente.direccion,
      fechaNacimiento: cliente.fechaNacimiento
        ? new Date(cliente.fechaNacimiento).toISOString().split('T')[0]
        : '',
    });
  }, [cliente, reset]);

  const handleFormSubmit = async (data: UpdateClienteRequest) => {
    setIsLoading(true);
    try {
      await clienteService.updateCliente(cliente.clienteID, data);
      toast.success('¡Cliente actualizado exitosamente!');
      onSuccess();
      onClose();
    } catch (error: any) {
      toast.error(error.message || 'Error al actualizar el cliente.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Editar Cliente">
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="md:col-span-2">
            <Input
              {...register('nombreCompleto', { required: 'El nombre es requerido' })}
              label="Nombre Completo *"
              placeholder="Juan Pérez"
              leftIcon={<User className="w-5 h-5" />}
              error={errors.nombreCompleto?.message}
            />
          </div>
          <Input
            {...register('telefono', {
              pattern: {
                value: /^\d{3}-\d{3}-\d{4}$/,
                message: 'Formato: 809-123-4567',
              },
            })}
            label="Teléfono"
            placeholder="809-123-4567"
            leftIcon={<Phone className="w-5 h-5" />}
            error={errors.telefono?.message}
          />
          <Input
            {...register('email', {
              pattern: {
                value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                message: 'Email inválido',
              },
            })}
            type="email"
            label="Email"
            placeholder="cliente@ejemplo.com"
            leftIcon={<Mail className="w-5 h-5" />}
            error={errors.email?.message}
          />
          <div className="md:col-span-2">
            <Input
              {...register('direccion')}
              label="Dirección"
              placeholder="Dirección completa"
              leftIcon={<MapPin className="w-5 h-5" />}
              error={errors.direccion?.message}
            />
          </div>
          <Input
            {...register('fechaNacimiento')}
            type="date"
            label="Fecha de Nacimiento"
            leftIcon={<Calendar className="w-5 h-5" />}
            error={errors.fechaNacimiento?.message}
          />
        </div>
        <div className="flex justify-end space-x-3 pt-4 border-t">
          <Button type="button" variant="outline" onClick={onClose} disabled={isLoading}>
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
    </Modal>
  );
};
