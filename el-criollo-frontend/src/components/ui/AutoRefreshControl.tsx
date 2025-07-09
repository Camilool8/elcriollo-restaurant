import React from 'react';
import { Button } from './Button';
import { RefreshCw, Play, Pause, Clock } from 'lucide-react';

interface AutoRefreshControlProps {
  isEnabled: boolean;
  isRefreshing: boolean;
  lastRefresh: Date | null;
  onToggle: () => void;
  onRefresh: () => void;
  interval?: number;
  className?: string;
}

export const AutoRefreshControl: React.FC<AutoRefreshControlProps> = ({
  isEnabled,
  isRefreshing,
  lastRefresh,
  onToggle,
  onRefresh,
  interval = 30000,
  className = '',
}) => {
  const formatLastRefresh = (date: Date | null) => {
    if (!date) return 'Nunca';
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    const seconds = Math.floor((diff % 60000) / 1000);

    if (minutes > 0) {
      return `Hace ${minutes}m ${seconds}s`;
    }
    return `Hace ${seconds}s`;
  };

  const formatInterval = (ms: number) => {
    const seconds = Math.floor(ms / 1000);
    if (seconds >= 60) {
      const minutes = Math.floor(seconds / 60);
      return `${minutes}m`;
    }
    return `${seconds}s`;
  };

  return (
    <div className={`flex items-center gap-2 p-2 bg-gray-50 rounded-lg ${className}`}>
      {/* Botón de toggle auto-refresh */}
      <Button
        variant="ghost"
        size="sm"
        onClick={onToggle}
        className={`flex items-center gap-1 ${
          isEnabled ? 'text-green-600 hover:text-green-700' : 'text-gray-500 hover:text-gray-700'
        }`}
        title={isEnabled ? 'Desactivar auto-refresh' : 'Activar auto-refresh'}
      >
        {isEnabled ? (
          <>
            <Pause className="w-4 h-4" />
            <span className="text-xs">Auto</span>
          </>
        ) : (
          <>
            <Play className="w-4 h-4" />
            <span className="text-xs">Manual</span>
          </>
        )}
      </Button>

      {/* Botón de refresh manual */}
      <Button
        variant="ghost"
        size="sm"
        onClick={onRefresh}
        disabled={isRefreshing}
        className="flex items-center gap-1 text-blue-600 hover:text-blue-700"
        title="Refrescar ahora"
      >
        <RefreshCw className={`w-4 h-4 ${isRefreshing ? 'animate-spin' : ''}`} />
        <span className="text-xs">Refrescar</span>
      </Button>

      {/* Información del estado */}
      <div className="flex items-center gap-1 text-xs text-gray-500">
        <Clock className="w-3 h-3" />
        <span>{isEnabled ? `Cada ${formatInterval(interval)}` : 'Manual'}</span>
      </div>

      {/* Última actualización */}
      <div className="text-xs text-gray-400">Última: {formatLastRefresh(lastRefresh)}</div>
    </div>
  );
};
