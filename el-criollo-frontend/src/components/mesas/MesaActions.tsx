import React, { useState } from 'react';
import {
  CheckCircle,
  XCircle,
  PlayCircle,
  Settings,
  AlertTriangle,
  Calendar,
  Users,
  Utensils,
} from 'lucide-react';
import type { Mesa } from '@/types/mesa';
import { Button, Modal, Card } from '@/components';

interface MesaActionsProps {
  mesa: Mesa;
  isOpen: boolean;
  onClose: () => void;
  onLiberar: (mesaId: number) => Promise<boolean>;
  onOcupar: (mesaId: number) => Promise<boolean>;
  onCambiarEstado: (mesaId: number, nuevoEstado: string, motivo?: string) => Promise<boolean>;
  onMarcarMantenimiento: (mesaId: number, motivo: string) => Promise<boolean>;
}

export const MesaActions: React.FC<MesaActionsProps> = ({
  mesa,
  isOpen,
  onClose,
  onLiberar,
  onOcupar,
  onCambiarEstado,
  onMarcarMantenimiento,
}) => {
  const [showMantenimientoForm, setShowMantenimientoForm] = useState(false);
  const [motivoMantenimiento, setMotivoMantenimiento] = useState('');
  const [loading, setLoading] = useState(false);

  const handleAccion = async (accion: () => Promise<boolean>) => {
    setLoading(true);
    try {
      const success = await accion();
      if (success) {
        onClose();
      }
    } finally {
      setLoading(false);
    }
  };

  const handleMantenimiento = async () => {
    if (!motivoMantenimiento.trim()) {
      return;
    }

    await handleAccion(async () => {
      const success = await onMarcarMantenimiento(mesa.mesaID, motivoMantenimiento);
      if (success) {
        setMotivoMantenimiento('');
        setShowMantenimientoForm(false);
      }
      return success;
    });
  };

  const getAccionesDisponibles = () => {
    const acciones = [];

    // Acciones según el estado actual
    switch (mesa.estado) {
      case 'Libre':
        acciones.push({
          key: 'ocupar',
          label: 'Ocupar Mesa',
          icon: <PlayCircle className="w-5 h-5" />,
          color: 'bg-dominican-red hover:bg-red-700',
          action: () => handleAccion(() => onOcupar(mesa.mesaID)),
        });
        acciones.push({
          key: 'reservar',
          label: 'Marcar como Reservada',
          icon: <Calendar className="w-5 h-5" />,
          color: 'bg-dominican-blue hover:bg-blue-700',
          action: () =>
            handleAccion(() => onCambiarEstado(mesa.mesaID, 'Reservada', 'Marcada manualmente')),
        });
        break;

      case 'Ocupada':
        acciones.push({
          key: 'liberar',
          label: 'Liberar Mesa',
          icon: <CheckCircle className="w-5 h-5" />,
          color: 'bg-palm-green hover:bg-green-700',
          action: () => handleAccion(() => onLiberar(mesa.mesaID)),
        });
        break;

      case 'Reservada':
        acciones.push({
          key: 'ocupar',
          label: 'Confirmar Llegada',
          icon: <PlayCircle className="w-5 h-5" />,
          color: 'bg-dominican-red hover:bg-red-700',
          action: () =>
            handleAccion(async () => {
              const liberada = await onLiberar(mesa.mesaID);
              if (liberada) {
                return await onOcupar(mesa.mesaID);
              }
              return false;
            }),
        });
        acciones.push({
          key: 'liberar',
          label: 'Cancelar Reserva',
          icon: <XCircle className="w-5 h-5" />,
          color: 'bg-gray-600 hover:bg-gray-700',
          action: () => handleAccion(() => onLiberar(mesa.mesaID)),
        });
        break;

      case 'Mantenimiento':
        acciones.push({
          key: 'liberar',
          label: 'Finalizar Mantenimiento',
          icon: <CheckCircle className="w-5 h-5" />,
          color: 'bg-palm-green hover:bg-green-700',
          action: () =>
            handleAccion(() => onCambiarEstado(mesa.mesaID, 'Libre', 'Mantenimiento completado')),
        });
        break;
    }

    // Acción de mantenimiento solo para mesas libres
    if (mesa.estado === 'Libre') {
      acciones.push({
        key: 'mantenimiento',
        label: 'Marcar Mantenimiento',
        icon: <Settings className="w-5 h-5" />,
        color: 'bg-amber-600 hover:bg-amber-700',
        action: () => setShowMantenimientoForm(true),
      });
    }

    return acciones;
  };

  const acciones = getAccionesDisponibles();

  if (showMantenimientoForm) {
    return (
      <Modal isOpen={isOpen} onClose={onClose} title="Marcar Mesa en Mantenimiento" size="md">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-stone-gray mb-2">
              Motivo del mantenimiento *
            </label>
            <textarea
              value={motivoMantenimiento}
              onChange={(e) => setMotivoMantenimiento(e.target.value)}
              placeholder="Ej: Limpieza profunda, reparación de silla, etc."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent resize-none"
              rows={3}
              required
            />
          </div>

          <div className="flex space-x-3">
            <Button
              onClick={() => {
                setShowMantenimientoForm(false);
                setMotivoMantenimiento('');
              }}
              variant="secondary"
              fullWidth
              disabled={loading}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleMantenimiento}
              variant="primary"
              fullWidth
              disabled={loading || !motivoMantenimiento.trim()}
              isLoading={loading}
            >
              Confirmar
            </Button>
          </div>
        </div>
      </Modal>
    );
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={`Gestionar Mesa ${mesa.numeroMesa}`} size="md">
      <div className="space-y-6">
        {/* Información de la mesa */}
        <Card className="bg-gray-50">
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="font-medium text-stone-gray">Estado:</span>
              <span
                className={`ml-2 px-2 py-1 rounded-full text-xs font-medium
                ${mesa.estado === 'Libre' ? 'bg-green-100 text-green-800' : ''}
                ${mesa.estado === 'Ocupada' ? 'bg-red-100 text-red-800' : ''}
                ${mesa.estado === 'Reservada' ? 'bg-blue-100 text-blue-800' : ''}
                ${mesa.estado === 'Mantenimiento' ? 'bg-yellow-100 text-yellow-800' : ''}
              `}
              >
                {mesa.estado}
              </span>
            </div>
            <div>
              <span className="font-medium text-stone-gray">Capacidad:</span>
              <span className="ml-2 text-gray-900">
                <Users className="w-4 h-4 inline mr-1" />
                {mesa.capacidad} personas
              </span>
            </div>
            {mesa.ubicacion && (
              <div className="col-span-2">
                <span className="font-medium text-stone-gray">Ubicación:</span>
                <span className="ml-2 text-gray-900">{mesa.ubicacion}</span>
              </div>
            )}
          </div>

          {/* Información específica del estado */}
          {mesa.estado === 'Ocupada' && mesa.clienteActual && (
            <div className="mt-4 pt-4 border-t border-gray-200">
              <div className="flex items-center space-x-2 text-sm">
                <Users className="w-4 h-4 text-gray-500" />
                <span className="font-medium">Cliente:</span>
                <span>{mesa.clienteActual.nombreCompleto}</span>
              </div>
              {mesa.ordenActual && (
                <div className="flex items-center space-x-2 text-sm mt-2">
                  <Utensils className="w-4 h-4 text-gray-500" />
                  <span className="font-medium">Orden:</span>
                  <span>{mesa.ordenActual.numeroOrden}</span>
                  <span className="text-green-600 font-medium">
                    RD${mesa.ordenActual.totalCalculado.toFixed(2)}
                  </span>
                </div>
              )}
              {mesa.tiempoOcupada && (
                <div className="text-sm text-gray-600 mt-2">
                  ⏱️ Tiempo ocupada: {mesa.tiempoOcupada}
                </div>
              )}
            </div>
          )}

          {mesa.estado === 'Reservada' && mesa.reservacionActual && (
            <div className="mt-4 pt-4 border-t border-gray-200">
              <div className="text-sm space-y-2">
                <div>
                  <span className="font-medium">Reserva:</span>{' '}
                  {mesa.reservacionActual.numeroReservacion}
                </div>
                <div>
                  <span className="font-medium">Personas:</span>{' '}
                  {mesa.reservacionActual.cantidadPersonas}
                </div>
                {mesa.tiempoHastaReserva && (
                  <div>
                    <span className="font-medium">Tiempo:</span> En {mesa.tiempoHastaReserva}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Alertas */}
          {(mesa.necesitaLimpieza || mesa.requiereAtencion) && (
            <div className="mt-4 pt-4 border-t border-gray-200">
              <div className="flex items-center space-x-2">
                <AlertTriangle className="w-4 h-4 text-amber-500" />
                <span className="text-sm font-medium text-amber-700">Requiere atención:</span>
              </div>
              <div className="mt-1 text-sm text-gray-600">
                {mesa.necesitaLimpieza && <div>• Necesita limpieza</div>}
                {mesa.requiereAtencion && <div>• Atención especial requerida</div>}
              </div>
            </div>
          )}
        </Card>

        {/* Acciones disponibles */}
        <div className="space-y-3">
          <h4 className="font-medium text-gray-900">Acciones disponibles:</h4>

          {acciones.map((accion) => (
            <Button
              key={accion.key}
              onClick={accion.action}
              className={`w-full justify-start ${accion.color} text-white`}
              disabled={loading}
              isLoading={loading}
            >
              {accion.icon}
              <span className="ml-3">{accion.label}</span>
            </Button>
          ))}

          {acciones.length === 0 && (
            <div className="text-center text-gray-500 py-4">
              No hay acciones disponibles para esta mesa
            </div>
          )}
        </div>
      </div>
    </Modal>
  );
};
