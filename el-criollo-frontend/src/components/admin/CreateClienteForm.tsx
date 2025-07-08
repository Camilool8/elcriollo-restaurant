import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import { UserPlus, UserIcon, Phone, Mail, MapPin, Calendar, Heart } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { CreateClienteRequest } from '@/types/requests';
import { clienteService } from '@/services/clienteService';
import { DOMINICAN_PATTERNS } from '@/types/requests';

interface CreateClienteFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (cliente: any) => void;
}

const CreateClienteForm: React.FC<CreateClienteFormProps> = ({ isOpen, onClose, onSuccess }) => {
  const [isLoading, setIsLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateClienteRequest>();

  const onSubmit = async (data: CreateClienteRequest) => {
    setIsLoading(true);

    try {
      const createRequest: CreateClienteRequest = {
        cedula: data.cedula?.trim() || undefined,
        nombre: data.nombre.trim(),
        apellido: data.apellido.trim(),
        telefono: data.telefono?.trim() || undefined,
        email: data.email?.trim() || undefined,
        direccion: data.direccion?.trim() || undefined,
        fechaNacimiento: data.fechaNacimiento || undefined,
        preferenciasComida: data.preferenciasComida?.trim() || undefined,
      };

      const newCliente = await clienteService.createCliente(createRequest);

      toast.success('¡Cliente registrado exitosamente! 🎉');
      onSuccess(newCliente);
      reset();
      onClose();
    } catch (error: any) {
      console.error('Error creando cliente:', error);
      toast.error(error.message || 'Error al registrar cliente');
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Registrar Nuevo Cliente" size="md">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="text-center mb-4">
          <div className="w-16 h-16 bg-caribbean-gold bg-opacity-10 rounded-full flex items-center justify-center mx-auto mb-2">
            <UserPlus className="w-8 h-8 text-yellow-600" />
          </div>
          <p className="text-stone-gray">Registra un nuevo cliente en el sistema</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            {...register('nombre', {
              required: 'El nombre es requerido',
              maxLength: { value: 50, message: 'Máximo 50 caracteres' },
            })}
            label="Nombre *"
            placeholder="Nombre del cliente"
            error={errors.nombre?.message}
            fullWidth
          />

          <Input
            {...register('apellido', {
              required: 'El apellido es requerido',
              maxLength: { value: 50, message: 'Máximo 50 caracteres' },
            })}
            label="Apellido *"
            placeholder="Apellido del cliente"
            error={errors.apellido?.message}
            fullWidth
          />

          <Input
            {...register('cedula', {
              pattern: {
                value: DOMINICAN_PATTERNS.cedula,
                message: 'Formato: 123-1234567-1',
              },
            })}
            label="Cédula"
            placeholder="123-1234567-1"
            leftIcon={<UserIcon className="w-5 h-5" />}
            error={errors.cedula?.message}
            fullWidth
          />

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

          <div className="md:col-span-2">
            <Input
              {...register('email', {
                pattern: {
                  value: DOMINICAN_PATTERNS.email,
                  message: 'Email inválido',
                },
              })}
              type="email"
              label="Email"
              placeholder="cliente@ejemplo.com"
              leftIcon={<Mail className="w-5 h-5" />}
              error={errors.email?.message}
              fullWidth
            />
          </div>

          <div className="md:col-span-2">
            <Input
              {...register('direccion')}
              label="Dirección"
              placeholder="Dirección completa"
              leftIcon={<MapPin className="w-5 h-5" />}
              error={errors.direccion?.message}
              fullWidth
            />
          </div>

          <Input
            {...register('fechaNacimiento')}
            type="date"
            label="Fecha de Nacimiento"
            leftIcon={<Calendar className="w-5 h-5" />}
            error={errors.fechaNacimiento?.message}
            fullWidth
          />

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              <Heart className="w-4 h-4 inline mr-1" />
              Preferencias de Comida
            </label>
            <textarea
              {...register('preferenciasComida')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue focus:border-dominican-blue"
              rows={3}
              placeholder="Ej: Sin picante, vegetariano, alérgico a mariscos..."
            />
          </div>
        </div>

        {/* Botones */}
        <div className="flex justify-end space-x-3 pt-4 border-t">
          <Button type="button" variant="outline" onClick={handleClose} disabled={isLoading}>
            Cancelar
          </Button>

          <Button type="submit" variant="primary" isLoading={isLoading} disabled={isLoading}>
            Registrar Cliente
          </Button>
        </div>
      </form>
    </Modal>
  );
};

export default CreateClienteForm;
