import React from 'react';
import { Button } from './Button';
import { RefreshCw, Play, Pause, Clock, Database } from 'lucide-react';

interface RefreshControl {
  isEnabled: boolean;
  isRefreshing: boolean;
  lastRefresh: Date | null;
  onToggle: () => void;
  onRefresh: () => void;
  label: string;
}

interface UnifiedAutoRefreshControlProps {
  controls: RefreshControl[];
  interval?: number;
  className?: string;
}

export const UnifiedAutoRefreshControl: React.FC<UnifiedAutoRefreshControlProps> = ({
  controls,
  interval = 30000,
  className = '',
}) => {
  const isAnyEnabled = controls.some((control) => control.isEnabled);
  const isAnyRefreshing = controls.some((control) => control.isRefreshing);
  const lastRefresh = controls.reduce(
    (latest, control) => {
      if (!control.lastRefresh) return latest;
      if (!latest) return control.lastRefresh;
      return control.lastRefresh > latest ? control.lastRefresh : latest;
    },
    null as Date | null
  );

  const handleToggleAll = () => {
    const newState = !isAnyEnabled;
    controls.forEach((control) => {
      if (control.isEnabled !== newState) {
        control.onToggle();
      }
    });
  };

  const handleRefreshAll = () => {
    controls.forEach((control) => {
      control.onRefresh();
    });
  };

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
        onClick={handleToggleAll}
        className={`flex items-center gap-1 ${
          isAnyEnabled ? 'text-green-600 hover:text-green-700' : 'text-gray-500 hover:text-gray-700'
        }`}
        title={isAnyEnabled ? 'Desactivar auto-refresh' : 'Activar auto-refresh'}
      >
        {isAnyEnabled ? (
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
        onClick={handleRefreshAll}
        disabled={isAnyRefreshing}
        className="flex items-center gap-1 text-blue-600 hover:text-blue-700"
        title="Refrescar todos los datos"
      >
        <RefreshCw className={`w-4 h-4 ${isAnyRefreshing ? 'animate-spin' : ''}`} />
        <span className="text-xs">Refrescar</span>
      </Button>

      {/* Información del estado */}
      <div className="flex items-center gap-1 text-xs text-gray-500">
        <Database className="w-3 h-3" />
        <span>{isAnyEnabled ? `Cada ${formatInterval(interval)}` : 'Manual'}</span>
      </div>

      {/* Última actualización */}
      <div className="text-xs text-gray-400">Última: {formatLastRefresh(lastRefresh)}</div>

      {/* Indicadores de estado por tipo de dato */}
      <div className="flex items-center gap-1">
        {controls.map((control, index) => (
          <div
            key={index}
            className={`w-2 h-2 rounded-full ${
              control.isRefreshing
                ? 'bg-blue-500 animate-pulse'
                : control.isEnabled
                  ? 'bg-green-500'
                  : 'bg-gray-400'
            }`}
            title={`${control.label}: ${control.isEnabled ? 'Auto' : 'Manual'}`}
          />
        ))}
      </div>
    </div>
  );
};
