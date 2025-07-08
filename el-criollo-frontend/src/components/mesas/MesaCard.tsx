// ====================================
// src/components/mesas/MesaCard.tsx - Componente de tarjeta de mesa
// ====================================

import React, { useState } from 'react';
import {
  Users,
  Clock,
  MapPin,
  AlertTriangle,
  MoreVertical,
  CheckCircle,
  XCircle,
  PlayCircle,
  PauseCircle,
  Settings,
} from 'lucide-react';
import type { Mesa } from '@/types/mesa';
import { COLORES_ESTADO_MESA } from '@/types/mesa';
import { Button, Card } from '@/components';
import { MesaActions } from './MesaActions';

interface MesaCardProps {
  mesa: Mesa;
  onLiberar: (mesaId: number) => Promise<boolean>;
  onOcupar: (mesaId: number) => Promise<boolean>;
  onCambiarEstado: (mesaId: number, nuevoEstado: string, motivo?: string) => Promise<boolean>;
  onMarcarMantenimiento: (mesaId: number, motivo: string) => Promise<boolean>;
  className?: string;
}

export const MesaCard: React.FC<MesaCardProps> = ({
  mesa,
  onLiberar,
  onOcupar,
  onCambiarEstado,
  onMarcarMantenimiento,
  className = '',
}) => {
  const [showActions, setShowActions] = useState(false);
  const colores = COLORES_ESTADO_MESA[mesa.estado];

  const getIconoEstado = () => {
    switch (mesa.estado) {
      case 'Libre':
        return <CheckCircle className="w-4 h-4 text-white" />;
      case 'Ocupada':
        return <PlayCircle className="w-4 h-4 text-white" />;
      case 'Reservada':
        return <PauseCircle className="w-4 h-4 text-white" />;
      case 'Mantenimiento':
        return <Settings className="w-4 h-4 text-white" />;
      default:
        return null;
    }
  };

  const handleCardClick = (e: React.MouseEvent) => {
    // Solo abrir acciones si no se hizo click en un botón
    if (!(e.target as HTMLElement).closest('button')) {
      setShowActions(true);
    }
  };

  return (
    <>
      <Card
        className={`
          relative overflow-hidden transition-all duration-200 cursor-pointer
          ${colores.bg} ${colores.border} border-2
          hover:shadow-lg hover:scale-105 transform
          ${mesa.requiereAtencion ? 'ring-2 ring-amber-400 ring-opacity-60' : ''}
          ${className}
        `}
        onClick={handleCardClick}
      >
        {/* Header de la mesa */}
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center space-x-2">
            {getIconoEstado()}
            <span className={`font-bold text-lg ${colores.text}`}>Mesa {mesa.numeroMesa}</span>
          </div>

          <div className="flex items-center space-x-1">
            {/* Indicadores de alerta */}
            {mesa.necesitaLimpieza && (
              <div
                className="w-2 h-2 bg-yellow-400 rounded-full animate-pulse"
                title="Necesita limpieza"
              />
            )}
            {mesa.requiereAtencion && (
              <AlertTriangle
                className="w-4 h-4 text-yellow-300 animate-pulse"
                aria-label="Requiere atención"
              />
            )}

            {/* Botón de acciones */}
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                setShowActions(true);
              }}
              className="text-white hover:bg-white hover:bg-opacity-20 p-1"
            >
              <MoreVertical className="w-4 h-4" />
            </Button>
          </div>
        </div>

        {/* Información de capacidad y ubicación */}
        <div className="space-y-2 mb-3">
          <div className="flex items-center space-x-1">
            <Users className={`w-4 h-4 ${colores.text}`} />
            <span className={`text-sm ${colores.text}`}>{mesa.capacidad} personas</span>
          </div>

          {mesa.ubicacion && (
            <div className="flex items-center space-x-1">
              <MapPin className={`w-4 h-4 ${colores.text}`} />
              <span className={`text-sm ${colores.text}`}>{mesa.ubicacion}</span>
            </div>
          )}
        </div>

        {/* Contenido específico por estado */}
        <div className="space-y-2">
          {mesa.estado === 'Ocupada' && mesa.clienteActual && (
            <div className={`text-sm ${colores.text}`}>
              <div className="font-medium">👤 {mesa.clienteActual.nombreCompleto}</div>
              {mesa.ordenActual && <div>🍽️ {mesa.ordenActual.numeroOrden}</div>}
              {mesa.tiempoOcupada && (
                <div className="flex items-center space-x-1 mt-1">
                  <Clock className="w-3 h-3" />
                  <span>{mesa.tiempoOcupada}</span>
                </div>
              )}
            </div>
          )}

          {mesa.estado === 'Reservada' && mesa.reservacionActual && (
            <div className={`text-sm ${colores.text}`}>
              <div className="font-medium">📅 {mesa.reservacionActual.numeroReservacion}</div>
              <div>👥 {mesa.reservacionActual.cantidadPersonas} personas</div>
              {mesa.tiempoHastaReserva && (
                <div className="flex items-center space-x-1 mt-1">
                  <Clock className="w-3 h-3" />
                  <span>En {mesa.tiempoHastaReserva}</span>
                </div>
              )}
            </div>
          )}

          {mesa.estado === 'Libre' && (
            <div className={`text-sm ${colores.text} text-center font-medium`}>✅ Disponible</div>
          )}

          {mesa.estado === 'Mantenimiento' && (
            <div className={`text-sm ${colores.text} text-center`}>🔧 En mantenimiento</div>
          )}
        </div>

        {/* Botones de acción rápida para mesa libre */}
        {mesa.estado === 'Libre' && (
          <div className="mt-3 pt-3 border-t border-white border-opacity-30">
            <div className="flex space-x-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  onOcupar(mesa.mesaID);
                }}
                className="flex-1 text-white hover:bg-white hover:bg-opacity-20 text-xs"
              >
                Ocupar
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  setShowActions(true);
                }}
                className="flex-1 text-white hover:bg-white hover:bg-opacity-20 text-xs"
              >
                Más...
              </Button>
            </div>
          </div>
        )}

        {/* Botón de liberación para mesa ocupada */}
        {mesa.estado === 'Ocupada' && (
          <div className="mt-3 pt-3 border-t border-white border-opacity-30">
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                onLiberar(mesa.mesaID);
              }}
              className="w-full text-white hover:bg-white hover:bg-opacity-20 text-xs"
            >
              <XCircle className="w-3 h-3 mr-1" />
              Liberar Mesa
            </Button>
          </div>
        )}

        {/* Indicador de estado en la esquina */}
        <div
          className={`
          absolute top-2 right-2 w-3 h-3 rounded-full
          ${mesa.estado === 'Libre' ? 'bg-green-400' : ''}
          ${mesa.estado === 'Ocupada' ? 'bg-red-400' : ''}
          ${mesa.estado === 'Reservada' ? 'bg-blue-400' : ''}
          ${mesa.estado === 'Mantenimiento' ? 'bg-yellow-400' : ''}
        `}
        />
      </Card>

      {/* Modal de acciones */}
      <MesaActions
        mesa={mesa}
        isOpen={showActions}
        onClose={() => setShowActions(false)}
        onLiberar={onLiberar}
        onOcupar={onOcupar}
        onCambiarEstado={onCambiarEstado}
        onMarcarMantenimiento={onMarcarMantenimiento}
      />
    </>
  );
};
