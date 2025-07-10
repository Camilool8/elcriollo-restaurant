import React, { useState } from 'react';
import {
  CheckCircle,
  XCircle,
  PlayCircle,
  Settings,
  Calendar,
  Users,
  Clock,
  MapPin,
  Star,
  Zap,
  Bug,
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
  onReservar?: (mesa: Mesa) => void;
  onDebug?: (mesaId: number) => Promise<any>;
}

export const MesaActions: React.FC<MesaActionsProps> = ({
  mesa,
  isOpen,
  onClose,
  onLiberar,
  onOcupar,
  onCambiarEstado,
  onMarcarMantenimiento,
  onReservar,
  onDebug,
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
          description: 'Iniciar servicio para nuevos clientes',
          icon: <PlayCircle className="w-5 h-5" />,
          color:
            'bg-gradient-to-r from-dominican-red to-red-600 hover:from-red-600 hover:to-red-700',
          action: () => handleAccion(() => onOcupar(mesa.mesaID)),
          priority: 'high',
        });
        acciones.push({
          key: 'reservar',
          label: 'Reservar Mesa',
          description: 'Reservar para cliente específico',
          icon: <Calendar className="w-5 h-5" />,
          color:
            'bg-gradient-to-r from-dominican-blue to-blue-600 hover:from-blue-600 hover:to-blue-700',
          action: () => {
            onReservar?.(mesa);
            onClose();
          },
          priority: 'medium',
        });
        break;

      case 'Ocupada':
        acciones.push({
          key: 'liberar',
          label: 'Liberar Mesa',
          description: 'Finalizar servicio y liberar mesa',
          icon: <CheckCircle className="w-5 h-5" />,
          color:
            'bg-gradient-to-r from-green-500 to-green-600 hover:from-green-600 hover:to-green-700',
          action: () => handleAccion(() => onLiberar(mesa.mesaID)),
          priority: 'high',
        });
        break;

      case 'Reservada':
        acciones.push({
          key: 'ocupar',
          label: 'Confirmar Llegada',
          description: 'Cliente llegó, iniciar servicio',
          icon: <PlayCircle className="w-5 h-5" />,
          color:
            'bg-gradient-to-r from-dominican-red to-red-600 hover:from-red-600 hover:to-red-700',
          action: () =>
            handleAccion(async () => {
              const liberada = await onLiberar(mesa.mesaID);
              if (liberada) {
                return await onOcupar(mesa.mesaID);
              }
              return false;
            }),
          priority: 'high',
        });
        acciones.push({
          key: 'liberar',
          label: 'Cancelar Reserva',
          description: 'Cliente no llegó, liberar mesa',
          icon: <XCircle className="w-5 h-5" />,
          color: 'bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700',
          action: () => handleAccion(() => onLiberar(mesa.mesaID)),
          priority: 'medium',
        });
        break;

      case 'Mantenimiento':
        acciones.push({
          key: 'liberar',
          label: 'Finalizar Mantenimiento',
          description: 'Mesa lista para uso',
          icon: <CheckCircle className="w-5 h-5" />,
          color:
            'bg-gradient-to-r from-green-500 to-green-600 hover:from-green-600 hover:to-green-700',
          action: () =>
            handleAccion(() => onCambiarEstado(mesa.mesaID, 'Libre', 'Mantenimiento completado')),
          priority: 'high',
        });
        break;
    }

    // Acción de mantenimiento solo para mesas libres
    if (mesa.estado === 'Libre') {
      acciones.push({
        key: 'mantenimiento',
        label: 'Marcar Mantenimiento',
        description: 'Mesa necesita reparación o limpieza',
        icon: <Settings className="w-5 h-5" />,
        color:
          'bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700',
        action: () => setShowMantenimientoForm(true),
        priority: 'low',
      });
    }

    return acciones;
  };

  const acciones = getAccionesDisponibles();

  if (showMantenimientoForm) {
    return (
      <Modal isOpen={isOpen} onClose={onClose} title="Marcar Mesa en Mantenimiento" size="md">
        <div className="space-y-6">
          {/* Información de la mesa */}
          <Card className="bg-gradient-to-r from-yellow-50 to-amber-50 border-yellow-200">
            <div className="flex items-center space-x-3">
              <div className="flex items-center justify-center w-10 h-10 bg-yellow-100 rounded-full">
                <Settings className="w-5 h-5 text-yellow-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Mesa {mesa.numeroMesa}</h3>
                <p className="text-sm text-gray-600">Marcar para mantenimiento</p>
              </div>
            </div>
          </Card>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Motivo del mantenimiento *
            </label>
            <textarea
              value={motivoMantenimiento}
              onChange={(e) => setMotivoMantenimiento(e.target.value)}
              placeholder="Ej: Limpieza profunda, reparación de silla, cambio de mantel, etc."
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent resize-none text-sm"
              rows={4}
              required
            />
            <p className="text-xs text-gray-500 mt-1">
              Describe el motivo para que el equipo sepa qué hacer
            </p>
          </div>

          <div className="flex space-x-3">
            <Button
              onClick={() => {
                setShowMantenimientoForm(false);
                setMotivoMantenimiento('');
              }}
              variant="outline"
              fullWidth
              disabled={loading}
              className="border-gray-300 text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleMantenimiento}
              variant="primary"
              fullWidth
              disabled={loading || !motivoMantenimiento.trim()}
              isLoading={loading}
              className="bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700"
            >
              Confirmar Mantenimiento
            </Button>
          </div>
        </div>
      </Modal>
    );
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={`Gestionar Mesa ${mesa.numeroMesa}`} size="lg">
      <div className="space-y-6">
        {/* Información de la mesa */}
        <Card className="bg-gradient-to-r from-blue-50 to-indigo-50 border-blue-200">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="flex items-center justify-center w-12 h-12 bg-white rounded-full shadow-sm border-2 border-dominican-blue">
                <span className="text-lg font-bold text-dominican-blue">{mesa.numeroMesa}</span>
              </div>
              <div>
                <h3 className="text-xl font-semibold text-gray-900">Mesa {mesa.numeroMesa}</h3>
                <div className="flex items-center space-x-4 text-sm text-gray-600 mt-1">
                  <div className="flex items-center space-x-1">
                    <Users className="w-4 h-4" />
                    <span>{mesa.capacidad} personas</span>
                  </div>
                  <div className="flex items-center space-x-1">
                    <MapPin className="w-4 h-4" />
                    <span className="capitalize">{mesa.ubicacion}</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Estado actual */}
            <div className="text-right">
              <div
                className={`inline-flex items-center space-x-2 px-3 py-1 rounded-full text-sm font-medium ${
                  mesa.estado === 'Libre'
                    ? 'bg-green-100 text-green-800'
                    : mesa.estado === 'Ocupada'
                      ? 'bg-red-100 text-red-800'
                      : mesa.estado === 'Reservada'
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-yellow-100 text-yellow-800'
                }`}
              >
                {mesa.estado === 'Libre' && <CheckCircle className="w-4 h-4" />}
                {mesa.estado === 'Ocupada' && <XCircle className="w-4 h-4" />}
                {mesa.estado === 'Reservada' && <Calendar className="w-4 h-4" />}
                {mesa.estado === 'Mantenimiento' && <Settings className="w-4 h-4" />}
                <span>{mesa.estado}</span>
              </div>
            </div>
          </div>
        </Card>

        {/* Acciones disponibles */}
        <div className="space-y-3">
          <h4 className="text-lg font-semibold text-gray-900 flex items-center space-x-2">
            <Zap className="w-5 h-5 text-dominican-blue" />
            <span>Acciones Disponibles</span>
          </h4>

          <div className="grid grid-cols-1 gap-3">
            {acciones.map((accion) => (
              <button
                key={accion.key}
                onClick={accion.action}
                disabled={loading}
                className={`${accion.color} text-white rounded-lg p-4 text-left transition-all duration-200 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed`}
              >
                <div className="flex items-center space-x-3">
                  <div className="flex-shrink-0">{accion.icon}</div>
                  <div className="flex-1">
                    <div className="font-semibold text-sm">{accion.label}</div>
                    <div className="text-xs opacity-90 mt-1">{accion.description}</div>
                  </div>
                  {accion.priority === 'high' && <Star className="w-4 h-4 text-yellow-300" />}
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Acciones secundarias */}
        <div className="border-t border-gray-200 pt-4">
          <div className="flex space-x-3">
            <Button
              onClick={onClose}
              variant="outline"
              fullWidth
              disabled={loading}
              className="border-gray-300 text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </Button>
            <Button
              onClick={() => window.location.reload()}
              variant="ghost"
              disabled={loading}
              className="text-gray-500 hover:text-gray-700"
            >
              <Clock className="w-4 h-4 mr-2" />
              Actualizar
            </Button>
            {onDebug && (
              <Button
                onClick={() => onDebug(mesa.mesaID)}
                variant="ghost"
                disabled={loading}
                className="text-purple-500 hover:text-purple-700"
                title="Debug mesa"
              >
                <Bug className="w-4 h-4 mr-2" />
                Debug
              </Button>
            )}
          </div>
        </div>
      </div>
    </Modal>
  );
};
