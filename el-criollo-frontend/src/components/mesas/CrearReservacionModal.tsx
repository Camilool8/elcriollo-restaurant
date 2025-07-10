import React, { useState, useEffect } from 'react';
import { Calendar, Clock, Users, User, Phone, Mail, MapPin } from 'lucide-react';
import { showErrorToast, showSuccessToast } from '@/utils/toastUtils';

// Components
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

// Services
import { reservacionService } from '@/services/reservacionService';
import { clienteService } from '@/services/clienteService';

// Types
import type { Mesa, Cliente } from '@/types';
import type { CrearReservacionRequest } from '@/types/reservacion';

interface CrearReservacionModalProps {
  mesa: Mesa;
  isOpen: boolean;
  onClose: () => void;
  onReservacionCreada: () => void;
}

interface FormData {
  clienteID: number;
  cantidadPersonas: number;
  fechaHora: string;
  duracionEstimada: number;
  observaciones: string;
}

interface Errors {
  [key: string]: string;
}

export const CrearReservacionModal: React.FC<CrearReservacionModalProps> = ({
  mesa,
  isOpen,
  onClose,
  onReservacionCreada,
}) => {
  const [formData, setFormData] = useState<FormData>({
    clienteID: 0,
    cantidadPersonas: mesa.capacidad,
    fechaHora: '',
    duracionEstimada: 120,
    observaciones: '',
  });

  const [errors, setErrors] = useState<Errors>({});
  const [loading, setLoading] = useState(false);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loadingClientes, setLoadingClientes] = useState(false);

  // ============================================================================
  // EFECTOS
  // ============================================================================

  useEffect(() => {
    if (isOpen) {
      cargarClientes();
    }
  }, [isOpen]);

  // ============================================================================
  // FUNCIONES AUXILIARES
  // ============================================================================

  const cargarClientes = async () => {
    try {
      setLoadingClientes(true);
      const data = await clienteService.getClientes();
      setClientes(data);
    } catch (error) {
      console.error('Error cargando clientes:', error);
      showErrorToast('Error al cargar los clientes');
    } finally {
      setLoadingClientes(false);
    }
  };

  const validarFormulario = (): boolean => {
    const nuevosErrores: Errors = {};

    if (!formData.clienteID) {
      nuevosErrores.clienteID = 'Debe seleccionar un cliente';
    }

    if (formData.cantidadPersonas <= 0) {
      nuevosErrores.cantidadPersonas = 'La cantidad de personas debe ser mayor a 0';
    }

    if (formData.cantidadPersonas > mesa.capacidad) {
      nuevosErrores.cantidadPersonas = `La cantidad de personas no puede exceder la capacidad de la mesa (${mesa.capacidad})`;
    }

    if (!formData.fechaHora) {
      nuevosErrores.fechaHora = 'Debe seleccionar una fecha y hora';
    }

    if (formData.duracionEstimada <= 0) {
      nuevosErrores.duracionEstimada = 'La duración estimada debe ser mayor a 0';
    }

    setErrors(nuevosErrores);
    return Object.keys(nuevosErrores).length === 0;
  };

  // ============================================================================
  // HANDLERS
  // ============================================================================

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validarFormulario()) {
      return;
    }

    try {
      setLoading(true);

      const reservacionData: CrearReservacionRequest = {
        clienteID: formData.clienteID,
        mesaID: mesa.mesaID,
        cantidadPersonas: formData.cantidadPersonas,
        fechaHora: formData.fechaHora,
        duracionEstimada: formData.duracionEstimada,
        observaciones: formData.observaciones.trim() || undefined,
      };

      await reservacionService.crearReservacion(reservacionData);

      showSuccessToast('¡Reservación creada exitosamente! 🎉');
      onReservacionCreada();
      handleClose();
    } catch (error: any) {
      console.error('Error creando reservación:', error);
      showErrorToast(error.message || 'Error al crear la reservación');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setFormData({
      clienteID: 0,
      cantidadPersonas: mesa.capacidad,
      fechaHora: '',
      duracionEstimada: 120,
      observaciones: '',
    });
    setErrors({});
    onClose();
  };

  // ============================================================================
  // RENDERIZADO
  // ============================================================================

  const clienteSeleccionado = clientes.find((c) => c.clienteID === formData.clienteID);

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={`Crear Reservación - Mesa ${mesa.numeroMesa}`}
      size="lg"
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Información de la mesa */}
        <Card className="p-4 bg-blue-50">
          <div className="flex items-center space-x-3">
            <MapPin className="w-5 h-5 text-blue-600" />
            <div>
              <h4 className="font-semibold text-blue-900">Mesa {mesa.numeroMesa}</h4>
              <p className="text-sm text-blue-700">
                Capacidad: {mesa.capacidad} personas • Ubicación:{' '}
                {mesa.ubicacion || 'No especificada'}
              </p>
            </div>
          </div>
        </Card>

        {/* Cliente */}
        <div>
          <label htmlFor="clienteID" className="block text-sm font-medium text-gray-700 mb-2">
            Cliente *
          </label>
          <select
            id="clienteID"
            value={formData.clienteID}
            onChange={(e) => setFormData({ ...formData, clienteID: Number(e.target.value) })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue"
            disabled={loadingClientes}
          >
            <option value={0}>Seleccione un cliente...</option>
            {clientes.map((cliente) => (
              <option key={cliente.clienteID} value={cliente.clienteID}>
                {cliente.nombreCompleto}
                {cliente.telefono && ` - ${cliente.telefono}`}
              </option>
            ))}
          </select>
          {errors.clienteID && <p className="mt-1 text-sm text-red-600">{errors.clienteID}</p>}
        </div>

        {/* Información del cliente seleccionado */}
        {clienteSeleccionado && (
          <Card className="p-3 bg-gray-50">
            <div className="flex items-center space-x-3">
              <User className="w-4 h-4 text-gray-600" />
              <div className="flex-1">
                <p className="font-medium text-gray-900">{clienteSeleccionado.nombreCompleto}</p>
                {clienteSeleccionado.telefono && (
                  <p className="text-sm text-gray-600 flex items-center">
                    <Phone className="w-3 h-3 mr-1" />
                    {clienteSeleccionado.telefono}
                  </p>
                )}
                {clienteSeleccionado.email && (
                  <p className="text-sm text-gray-600 flex items-center">
                    <Mail className="w-3 h-3 mr-1" />
                    {clienteSeleccionado.email}
                  </p>
                )}
              </div>
            </div>
          </Card>
        )}

        {/* Cantidad de personas */}
        <div>
          <label
            htmlFor="cantidadPersonas"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Cantidad de Personas *
          </label>
          <div className="relative">
            <Users className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            <Input
              type="number"
              id="cantidadPersonas"
              value={formData.cantidadPersonas}
              onChange={(e) =>
                setFormData({ ...formData, cantidadPersonas: Number(e.target.value) })
              }
              min={1}
              max={mesa.capacidad}
              className="pl-10"
              placeholder={`Máximo ${mesa.capacidad} personas`}
            />
          </div>
          {errors.cantidadPersonas && (
            <p className="mt-1 text-sm text-red-600">{errors.cantidadPersonas}</p>
          )}
        </div>

        {/* Fecha y hora */}
        <div>
          <label htmlFor="fechaHora" className="block text-sm font-medium text-gray-700 mb-2">
            Fecha y Hora *
          </label>
          <div className="relative">
            <Calendar className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            <Input
              type="datetime-local"
              id="fechaHora"
              value={formData.fechaHora}
              onChange={(e) => setFormData({ ...formData, fechaHora: e.target.value })}
              className="pl-10"
            />
          </div>
          {errors.fechaHora && <p className="mt-1 text-sm text-red-600">{errors.fechaHora}</p>}
        </div>

        {/* Duración estimada */}
        <div>
          <label
            htmlFor="duracionEstimada"
            className="block text-sm font-medium text-gray-700 mb-2"
          >
            Duración Estimada (minutos) *
          </label>
          <div className="relative">
            <Clock className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            <Input
              type="number"
              id="duracionEstimada"
              value={formData.duracionEstimada}
              onChange={(e) =>
                setFormData({ ...formData, duracionEstimada: Number(e.target.value) })
              }
              min={30}
              max={480}
              className="pl-10"
              placeholder="120 minutos"
            />
          </div>
          {errors.duracionEstimada && (
            <p className="mt-1 text-sm text-red-600">{errors.duracionEstimada}</p>
          )}
        </div>

        {/* Observaciones */}
        <div>
          <label htmlFor="observaciones" className="block text-sm font-medium text-gray-700 mb-2">
            Observaciones
          </label>
          <textarea
            id="observaciones"
            value={formData.observaciones}
            onChange={(e) => setFormData({ ...formData, observaciones: e.target.value })}
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-dominican-blue"
            placeholder="Notas especiales, preferencias, etc."
          />
        </div>

        {/* Botones */}
        <div className="flex justify-end space-x-3 pt-4">
          <Button type="button" variant="outline" onClick={handleClose} disabled={loading}>
            Cancelar
          </Button>
          <Button type="submit" disabled={loading}>
            {loading ? (
              <>
                <LoadingSpinner size="sm" />
                Creando...
              </>
            ) : (
              <>
                <Calendar className="w-4 h-4 mr-2" />
                Crear Reservación
              </>
            )}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
