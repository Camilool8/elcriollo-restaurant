// ====================================
// src/components/mesas/MesaFilters.tsx - Componente de filtros de mesas
// ====================================

import React, { useState } from 'react';
import {
  Filter,
  X,
  Users,
  MapPin,
  RefreshCw,
  Search,
  CheckCircle,
  XCircle,
  PauseCircle,
  Settings,
} from 'lucide-react';
import type { EstadoMesa, FiltrosMesa } from '@/types/mesa';
import { UBICACIONES_RESTAURANTE } from '@/types/mesa';
import { Button, Card } from '@/components';

interface MesaFiltersProps {
  filtros: FiltrosMesa;
  onFiltrosChange: (filtros: FiltrosMesa) => void;
  onRefresh: () => void;
  loading?: boolean;
  totalMesas: number;
  mesasFiltradas: number;
}

export const MesaFilters: React.FC<MesaFiltersProps> = ({
  filtros,
  onFiltrosChange,
  onRefresh,
  loading = false,
  totalMesas,
  mesasFiltradas,
}) => {
  const [showFilters, setShowFilters] = useState(false);

  const handleEstadoChange = (estado: EstadoMesa | undefined) => {
    onFiltrosChange({ ...filtros, estado });
  };

  const handleCapacidadChange = (tipo: 'minima' | 'maxima', valor: string) => {
    const capacidad = valor ? parseInt(valor) : undefined;
    if (tipo === 'minima') {
      onFiltrosChange({ ...filtros, capacidadMinima: capacidad });
    } else {
      onFiltrosChange({ ...filtros, capacidadMaxima: capacidad });
    }
  };

  const handleUbicacionChange = (ubicacion: string) => {
    onFiltrosChange({
      ...filtros,
      ubicacion: ubicacion === 'todas' ? undefined : ubicacion,
    });
  };

  const limpiarFiltros = () => {
    onFiltrosChange({});
    setShowFilters(false);
  };

  const hayFiltrosActivos = Object.keys(filtros).some(
    (key) => filtros[key as keyof FiltrosMesa] !== undefined
  );

  const estadosConIconos = [
    {
      valor: 'Libre',
      label: 'Libres',
      icon: <CheckCircle className="w-4 h-4" />,
      color: 'text-green-600 bg-green-50 border-green-200',
    },
    {
      valor: 'Ocupada',
      label: 'Ocupadas',
      icon: <XCircle className="w-4 h-4" />,
      color: 'text-red-600 bg-red-50 border-red-200',
    },
    {
      valor: 'Reservada',
      label: 'Reservadas',
      icon: <PauseCircle className="w-4 h-4" />,
      color: 'text-blue-600 bg-blue-50 border-blue-200',
    },
    {
      valor: 'Mantenimiento',
      label: 'Mantenimiento',
      icon: <Settings className="w-4 h-4" />,
      color: 'text-yellow-600 bg-yellow-50 border-yellow-200',
    },
  ];

  return (
    <Card className="bg-white shadow-sm">
      <div className="flex items-center justify-between mb-4">
        {/* Título y contador */}
        <div className="flex items-center space-x-3">
          <h3 className="text-lg font-heading font-semibold text-dominican-blue">
            Gestión de Mesas
          </h3>
          <span className="text-sm text-gray-600">
            {mesasFiltradas} de {totalMesas} mesas
          </span>
        </div>

        {/* Controles principales */}
        <div className="flex items-center space-x-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={onRefresh}
            disabled={loading}
            className="text-gray-600 hover:text-dominican-blue"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            <span className="hidden sm:inline ml-1">Actualizar</span>
          </Button>

          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowFilters(!showFilters)}
            className={`text-gray-600 hover:text-dominican-blue ${
              hayFiltrosActivos ? 'bg-dominican-blue bg-opacity-10 text-dominican-blue' : ''
            }`}
          >
            <Filter className="w-4 h-4" />
            <span className="hidden sm:inline ml-1">Filtros</span>
            {hayFiltrosActivos && (
              <span className="ml-1 bg-dominican-red text-white rounded-full w-5 h-5 text-xs flex items-center justify-center">
                !
              </span>
            )}
          </Button>
        </div>
      </div>

      {/* Filtros rápidos por estado */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 mb-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => handleEstadoChange(undefined)}
          className={`justify-start p-3 h-auto ${
            !filtros.estado ? 'bg-gray-50 border border-gray-200' : ''
          }`}
        >
          <Search className="w-4 h-4 mr-2" />
          <div className="text-left">
            <div className="font-medium text-sm">Todas</div>
            <div className="text-xs text-gray-500">{totalMesas} mesas</div>
          </div>
        </Button>

        {estadosConIconos.map((estado) => (
          <Button
            key={estado.valor}
            variant="ghost"
            size="sm"
            onClick={() => handleEstadoChange(estado.valor as EstadoMesa)}
            className={`justify-start p-3 h-auto ${
              filtros.estado === estado.valor ? `${estado.color} border` : 'hover:bg-gray-50'
            }`}
          >
            {estado.icon}
            <div className="text-left ml-2">
              <div className="font-medium text-sm">{estado.label}</div>
              <div className="text-xs opacity-70">Filtrar</div>
            </div>
          </Button>
        ))}
      </div>

      {/* Panel de filtros avanzados */}
      {showFilters && (
        <Card className="border-t border-gray-200 mt-4 pt-4 bg-gray-50/50 rounded-t-none -mx-4 -mb-4 px-4 pb-4">
          <div className="flex items-center justify-between mb-4">
            <h4 className="font-medium text-gray-900">Filtros Avanzados</h4>
            {hayFiltrosActivos && (
              <Button
                variant="ghost"
                size="sm"
                onClick={limpiarFiltros}
                className="text-red-600 hover:text-red-700"
              >
                <X className="w-4 h-4 mr-1" />
                Limpiar
              </Button>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Filtro por capacidad */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Users className="w-4 h-4 inline mr-1" />
                Capacidad
              </label>
              <div className="flex space-x-2">
                <input
                  type="number"
                  placeholder="Min"
                  min="1"
                  max="20"
                  value={filtros.capacidadMinima || ''}
                  onChange={(e) => handleCapacidadChange('minima', e.target.value)}
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-dominican-blue"
                />
                <input
                  type="number"
                  placeholder="Max"
                  min="1"
                  max="20"
                  value={filtros.capacidadMaxima || ''}
                  onChange={(e) => handleCapacidadChange('maxima', e.target.value)}
                  className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-dominican-blue"
                />
              </div>
            </div>

            {/* Filtro por ubicación */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <MapPin className="w-4 h-4 inline mr-1" />
                Ubicación
              </label>
              <select
                value={filtros.ubicacion || 'todas'}
                onChange={(e) => handleUbicacionChange(e.target.value)}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-dominican-blue"
              >
                <option value="todas">Todas las ubicaciones</option>
                {UBICACIONES_RESTAURANTE.map((ubicacion) => (
                  <option key={ubicacion} value={ubicacion}>
                    {ubicacion}
                  </option>
                ))}
              </select>
            </div>

            {/* Filtro solo disponibles */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Disponibilidad</label>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={filtros.soloDisponibles || false}
                  onChange={(e) =>
                    onFiltrosChange({
                      ...filtros,
                      soloDisponibles: e.target.checked || undefined,
                    })
                  }
                  className="mr-2 text-dominican-blue focus:ring-dominican-blue"
                />
                <span className="text-sm text-gray-700">Solo mesas disponibles</span>
              </label>
            </div>
          </div>
        </Card>
      )}
    </Card>
  );
};
