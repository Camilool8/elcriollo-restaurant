import React, { useState, useEffect } from 'react';
import {
  Calendar,
  Clock,
  Users,
  User,
  Phone,
  Mail,
  MapPin,
  AlertCircle,
  CheckCircle,
  XCircle,
} from 'lucide-react';
import { toast } from 'react-toastify';
import { Modal, Card, Button, Input } from '@/components';
import { reservacionService } from '@/services/reservacionService';
import { clienteService } from '@/services/clienteService';
import type { Mesa } from '@/types/mesa';
import type { Cliente } from '@/types/cliente';
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

interface FormErrors {
  clienteID?: string;
  cantidadPersonas?: string;
  fechaHora?: string;
  duracionEstimada?: string;
  observaciones?: string;
}

export const CrearReservacionModal: React.FC<CrearReservacionModalProps> = ({
  mesa,
  isOpen,
  onClose,
  onReservacionCreada,
}) => {
  const [loading, setLoading] = useState(false);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loadingClientes, setLoadingClientes] = useState(true);
  const [horariosDisponibles, setHorariosDisponibles] = useState<string[]>([]);
  const [loadingHorarios, setLoadingHorarios] = useState(false);

  const [formData, setFormData] = useState<FormData>({
    clienteID: 0,
    cantidadPersonas: mesa.capacidad,
    fechaHora: '',
    duracionEstimada: 120, // 2 horas por defecto
    observaciones: '',
  });

  const [errors, setErrors] = useState<FormErrors>({});

  // ============================================================================
  // EFECTOS
  // ============================================================================

  useEffect(() => {
    if (isOpen) {
      cargarClientes();
    }
  }, [isOpen]);

  useEffect(() => {
    if (formData.fechaHora) {
      cargarHorariosDisponibles();
    }
  }, [formData.fechaHora]);

  // ============================================================================
  // FUNCIONES
  // ============================================================================

  const cargarClientes = async () => {
    try {
      setLoadingClientes(true);
      const data = await clienteService.getClientes();
      setClientes(data);
    } catch (error) {
      console.error('Error cargando clientes:', error);
      toast.error('Error al cargar los clientes');
    } finally {
      setLoadingClientes(false);
    }
  };

  const cargarHorariosDisponibles = async () => {
    if (!formData.fechaHora) return;

    try {
      setLoadingHorarios(true);
      const fecha = formData.fechaHora.split('T')[0];
      const horarios = await reservacionService.getHorariosDisponibles(mesa.mesaID, fecha);
      setHorariosDisponibles(horarios);
    } catch (error) {
      console.error('Error cargando horarios:', error);
      toast.error('Error al cargar los horarios disponibles');
    } finally {
      setLoadingHorarios(false);
    }
  };

  const validarFormulario = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.clienteID) {
      newErrors.clienteID = 'Debe seleccionar un cliente';
    }

    if (!formData.cantidadPersonas || formData.cantidadPersonas < 1) {
      newErrors.cantidadPersonas = 'La cantidad de personas debe ser mayor a 0';
    }

    if (formData.cantidadPersonas > mesa.capacidad) {
      newErrors.cantidadPersonas = `La mesa solo tiene capacidad para ${mesa.capacidad} personas`;
    }

    if (!formData.fechaHora) {
      newErrors.fechaHora = 'Debe seleccionar fecha y hora';
    }

    if (formData.duracionEstimada < 30 || formData.duracionEstimada > 480) {
      newErrors.duracionEstimada = 'La duración debe estar entre 30 y 480 minutos';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

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

      toast.success('¡Reservación creada exitosamente! 🎉');
      onReservacionCreada();
      handleClose();
    } catch (error: any) {
      console.error('Error creando reservación:', error);
      toast.error(error.message || 'Error al crear la reservación');
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

  const clienteSeleccionado = clientes.find((c) => c.clienteID === formData.clienteID);

  // ============================================================================
  // RENDER
  // ============================================================================

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={`Reservar Mesa ${mesa.numeroMesa}`}
      size="lg"
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Información de la mesa */}
        <Card className="bg-gradient-to-r from-blue-50 to-indigo-50 border-blue-200">
          <div className="flex items-center space-x-4">
            <div className="flex items-center justify-center w-12 h-12 bg-white rounded-full shadow-sm border-2 border-dominican-blue">
              <span className="text-lg font-bold text-dominican-blue">{mesa.numeroMesa}</span>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900">Mesa {mesa.numeroMesa}</h3>
              <div className="flex items-center space-x-4 text-sm text-gray-600 mt-1">
                <div className="flex items-center space-x-1">
                  <Users className="w-4 h-4" />
                  <span>Capacidad: {mesa.capacidad} personas</span>
                </div>
                <div className="flex items-center space-x-1">
                  <MapPin className="w-4 h-4" />
                  <span className="capitalize">{mesa.ubicacion}</span>
                </div>
              </div>
            </div>
          </div>
        </Card>

        {/* Selección de cliente */}
        <div className="space-y-3">
          <label className="block text-sm font-semibold text-gray-700 flex items-center space-x-2">
            <User className="w-4 h-4 text-dominican-blue" />
            <span>Cliente *</span>
          </label>

          {loadingClientes ? (
            <div className="flex items-center justify-center py-4">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-dominican-blue"></div>
              <span className="ml-2 text-sm text-gray-600">Cargando clientes...</span>
            </div>
          ) : (
            <select
              value={formData.clienteID}
              onChange={(e) => setFormData({ ...formData, clienteID: Number(e.target.value) })}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm ${
                errors.clienteID ? 'border-red-300' : 'border-gray-300'
              }`}
            >
              <option value={0}>Seleccionar cliente</option>
              {clientes.map((cliente) => (
                <option key={cliente.clienteID} value={cliente.clienteID}>
                  {cliente.nombreCompleto} - {cliente.telefono || 'Sin teléfono'}
                </option>
              ))}
            </select>
          )}

          {errors.clienteID && (
            <p className="text-sm text-red-600 flex items-center space-x-1">
              <AlertCircle className="w-4 h-4" />
              <span>{errors.clienteID}</span>
            </p>
          )}

          {/* Información del cliente seleccionado */}
          {clienteSeleccionado && (
            <div className="bg-green-50 border border-green-200 rounded-lg p-3">
              <div className="flex items-center space-x-2 mb-2">
                <CheckCircle className="w-4 h-4 text-green-600" />
                <span className="font-medium text-green-800">Cliente seleccionado</span>
              </div>
              <div className="text-sm text-green-700">
                <div className="flex items-center space-x-2">
                  <User className="w-4 h-4" />
                  <span>{clienteSeleccionado.nombreCompleto}</span>
                </div>
                {clienteSeleccionado.telefono && (
                  <div className="flex items-center space-x-2 mt-1">
                    <Phone className="w-4 h-4" />
                    <span>{clienteSeleccionado.telefono}</span>
                  </div>
                )}
                {clienteSeleccionado.email && (
                  <div className="flex items-center space-x-2 mt-1">
                    <Mail className="w-4 h-4" />
                    <span>{clienteSeleccionado.email}</span>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Cantidad de personas */}
        <div className="space-y-3">
          <label className="block text-sm font-semibold text-gray-700 flex items-center space-x-2">
            <Users className="w-4 h-4 text-dominican-blue" />
            <span>Cantidad de personas *</span>
          </label>

          <input
            type="number"
            min="1"
            max={mesa.capacidad}
            value={formData.cantidadPersonas}
            onChange={(e) => setFormData({ ...formData, cantidadPersonas: Number(e.target.value) })}
            className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm ${
              errors.cantidadPersonas ? 'border-red-300' : 'border-gray-300'
            }`}
          />

          {errors.cantidadPersonas && (
            <p className="text-sm text-red-600 flex items-center space-x-1">
              <AlertCircle className="w-4 h-4" />
              <span>{errors.cantidadPersonas}</span>
            </p>
          )}

          <p className="text-xs text-gray-500">Máximo {mesa.capacidad} personas para esta mesa</p>
        </div>

        {/* Fecha y hora */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-3">
            <label className="block text-sm font-semibold text-gray-700 flex items-center space-x-2">
              <Calendar className="w-4 h-4 text-dominican-blue" />
              <span>Fecha y hora *</span>
            </label>

            <input
              type="datetime-local"
              value={formData.fechaHora}
              onChange={(e) => setFormData({ ...formData, fechaHora: e.target.value })}
              min={new Date().toISOString().slice(0, 16)}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm ${
                errors.fechaHora ? 'border-red-300' : 'border-gray-300'
              }`}
            />

            {errors.fechaHora && (
              <p className="text-sm text-red-600 flex items-center space-x-1">
                <AlertCircle className="w-4 h-4" />
                <span>{errors.fechaHora}</span>
              </p>
            )}
          </div>

          <div className="space-y-3">
            <label className="block text-sm font-semibold text-gray-700 flex items-center space-x-2">
              <Clock className="w-4 h-4 text-dominican-blue" />
              <span>Duración estimada (minutos) *</span>
            </label>

            <input
              type="number"
              min="30"
              max="480"
              step="30"
              value={formData.duracionEstimada}
              onChange={(e) =>
                setFormData({ ...formData, duracionEstimada: Number(e.target.value) })
              }
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm ${
                errors.duracionEstimada ? 'border-red-300' : 'border-gray-300'
              }`}
            />

            {errors.duracionEstimada && (
              <p className="text-sm text-red-600 flex items-center space-x-1">
                <AlertCircle className="w-4 h-4" />
                <span>{errors.duracionEstimada}</span>
              </p>
            )}

            <p className="text-xs text-gray-500">Entre 30 minutos y 8 horas</p>
          </div>
        </div>

        {/* Observaciones */}
        <div className="space-y-3">
          <label className="block text-sm font-semibold text-gray-700">Observaciones</label>

          <textarea
            value={formData.observaciones}
            onChange={(e) => setFormData({ ...formData, observaciones: e.target.value })}
            placeholder="Ej: Cumpleaños, aniversario, mesa cerca de la ventana, etc."
            rows={3}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm resize-none"
          />

          <p className="text-xs text-gray-500">Información adicional para el personal</p>
        </div>

        {/* Botones */}
        <div className="flex space-x-3 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={handleClose}
            fullWidth
            disabled={loading}
            className="border-gray-300 text-gray-700 hover:bg-gray-50"
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            variant="primary"
            fullWidth
            disabled={loading || !formData.clienteID || !formData.fechaHora}
            isLoading={loading}
            className="bg-gradient-to-r from-dominican-blue to-blue-600 hover:from-blue-600 hover:to-blue-700"
          >
            Crear Reservación
          </Button>
        </div>
      </form>
    </Modal>
  );
};
