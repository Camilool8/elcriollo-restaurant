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
  Sliders,
  TrendingUp,
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
      color: 'bg-gradient-to-r from-green-500 to-green-600',
      hoverColor: 'hover:from-green-600 hover:to-green-700',
      textColor: 'text-green-700',
      bgColor: 'bg-green-50',
      borderColor: 'border-green-200',
    },
    {
      valor: 'Ocupada',
      label: 'Ocupadas',
      icon: <XCircle className="w-4 h-4" />,
      color: 'bg-gradient-to-r from-red-500 to-red-600',
      hoverColor: 'hover:from-red-600 hover:to-red-700',
      textColor: 'text-red-700',
      bgColor: 'bg-red-50',
      borderColor: 'border-red-200',
    },
    {
      valor: 'Reservada',
      label: 'Reservadas',
      icon: <PauseCircle className="w-4 h-4" />,
      color: 'bg-gradient-to-r from-blue-500 to-blue-600',
      hoverColor: 'hover:from-blue-600 hover:to-blue-700',
      textColor: 'text-blue-700',
      bgColor: 'bg-blue-50',
      borderColor: 'border-blue-200',
    },
    {
      valor: 'Mantenimiento',
      label: 'Mantenimiento',
      icon: <Settings className="w-4 h-4" />,
      color: 'bg-gradient-to-r from-yellow-500 to-yellow-600',
      hoverColor: 'hover:from-yellow-600 hover:to-yellow-700',
      textColor: 'text-yellow-700',
      bgColor: 'bg-yellow-50',
      borderColor: 'border-yellow-200',
    },
  ];

  return (
    <Card className="bg-gradient-to-br from-white to-gray-50 shadow-sm border-0">
      {/* Header con estadísticas */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center space-x-4">
          <div className="flex items-center space-x-3">
            <div className="flex items-center justify-center w-10 h-10 bg-gradient-to-r from-dominican-blue to-blue-600 rounded-full">
              <TrendingUp className="w-5 h-5 text-white" />
            </div>
            <div>
              <h3 className="text-xl font-heading font-semibold text-gray-900">Gestión de Mesas</h3>
              <p className="text-sm text-gray-600">
                {mesasFiltradas} de {totalMesas} mesas mostradas
              </p>
            </div>
          </div>
        </div>

        {/* Controles principales */}
        <div className="flex items-center space-x-3">
          <Button
            variant="ghost"
            size="sm"
            onClick={onRefresh}
            disabled={loading}
            className="text-gray-600 hover:text-dominican-blue hover:bg-blue-50 transition-colors"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            <span className="hidden sm:inline ml-2">Actualizar</span>
          </Button>

          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowFilters(!showFilters)}
            className={`text-gray-600 hover:text-dominican-blue transition-all duration-200 ${
              hayFiltrosActivos
                ? 'bg-gradient-to-r from-dominican-blue to-blue-600 text-white hover:from-blue-600 hover:to-blue-700 shadow-lg'
                : 'hover:bg-blue-50 border border-gray-200 hover:border-dominican-blue-200'
            }`}
          >
            <Sliders className="w-4 h-4" />
            <span className="hidden sm:inline ml-2">Filtros</span>
            {hayFiltrosActivos && (
              <span className="ml-2 bg-white text-dominican-blue rounded-full w-5 h-5 text-xs flex items-center justify-center font-bold">
                !
              </span>
            )}
            <svg
              className={`w-4 h-4 ml-1 transition-transform duration-200 ${showFilters ? 'rotate-180' : ''}`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </Button>
        </div>
      </div>

      {/* Filtros rápidos por estado */}
      <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => handleEstadoChange(undefined)}
          className={`justify-start p-4 h-auto transition-all duration-200 ${
            !filtros.estado
              ? 'bg-gradient-to-r from-dominican-blue to-blue-600 text-white shadow-lg border-2 border-dominican-blue'
              : 'hover:bg-gray-50 border border-gray-200 hover:border-dominican-blue-200'
          }`}
        >
          <Search className="w-5 h-5 mr-3" />
          <div className="text-left">
            <div className="font-semibold text-sm">Todas</div>
            <div className="text-xs opacity-90">{totalMesas} mesas</div>
          </div>
        </Button>

        {estadosConIconos.map((estado) => (
          <Button
            key={estado.valor}
            variant="ghost"
            size="sm"
            onClick={() => handleEstadoChange(estado.valor as EstadoMesa)}
            className={`justify-start p-4 h-auto transition-all duration-200 ${
              filtros.estado === estado.valor
                ? `${estado.bgColor} ${estado.borderColor} border-2 shadow-lg`
                : 'hover:bg-gray-50 border border-gray-200 hover:border-gray-300'
            }`}
          >
            <div className={`w-5 h-5 mr-3 ${estado.textColor}`}>{estado.icon}</div>
            <div className="text-left">
              <div className="font-semibold text-sm text-gray-900">{estado.label}</div>
              <div className="text-xs text-gray-500">Filtrar</div>
            </div>
          </Button>
        ))}
      </div>

      {/* Panel de filtros avanzados */}
      {showFilters && (
        <Card className="border-t border-gray-200 mt-6 pt-6 bg-gradient-to-br from-gray-50 to-white rounded-lg">
          <div className="flex items-center justify-between mb-6">
            <h4 className="text-lg font-semibold text-gray-900 flex items-center space-x-2">
              <Filter className="w-5 h-5 text-dominican-blue" />
              <span>Filtros Avanzados</span>
            </h4>
            {hayFiltrosActivos && (
              <Button
                variant="ghost"
                size="sm"
                onClick={limpiarFiltros}
                className="text-red-600 hover:text-red-700 hover:bg-red-50 transition-colors"
              >
                <X className="w-4 h-4 mr-2" />
                Limpiar Filtros
              </Button>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Filtro por capacidad */}
            <div className="space-y-3">
              <label className="block text-sm font-semibold text-gray-700 flex items-center space-x-2">
                <Users className="w-4 h-4 text-dominican-blue" />
                <span>Capacidad</span>
              </label>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Mínima</label>
                  <input
                    type="number"
                    min="1"
                    max="20"
                    placeholder="1"
                    value={filtros.capacidadMinima || ''}
                    onChange={(e) => handleCapacidadChange('minima', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Máxima</label>
                  <input
                    type="number"
                    min="1"
                    max="20"
                    placeholder="20"
                    value={filtros.capacidadMaxima || ''}
                    onChange={(e) => handleCapacidadChange('maxima', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm"
                  />
                </div>
              </div>
            </div>

            {/* Filtro por ubicación */}
            <div className="space-y-3">
              <label className="block text-sm font-semibold text-gray-700 flex items-center space-x-2">
                <MapPin className="w-4 h-4 text-dominican-blue" />
                <span>Ubicación</span>
              </label>

              <select
                value={filtros.ubicacion || 'todas'}
                onChange={(e) => handleUbicacionChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-dominican-blue focus:border-transparent text-sm bg-white"
              >
                <option value="todas">Todas las ubicaciones</option>
                {UBICACIONES_RESTAURANTE.map((ubicacion) => (
                  <option key={ubicacion} value={ubicacion}>
                    {ubicacion}
                  </option>
                ))}
              </select>
            </div>

            {/* Estadísticas rápidas */}
            <div className="space-y-3">
              <label className="block text-sm font-semibold text-gray-700">Resumen</label>

              <div className="bg-gradient-to-r from-blue-50 to-indigo-50 p-4 rounded-lg border border-blue-200">
                <div className="text-center">
                  <div className="text-2xl font-bold text-dominican-blue">{mesasFiltradas}</div>
                  <div className="text-xs text-gray-600">mesas encontradas</div>
                </div>

                {hayFiltrosActivos && (
                  <div className="mt-2 text-center">
                    <div className="text-xs text-gray-500">de {totalMesas} total</div>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Filtros activos */}
          {hayFiltrosActivos && (
            <div className="mt-6 pt-4 border-t border-gray-200">
              <h5 className="text-sm font-medium text-gray-700 mb-3">Filtros Activos:</h5>
              <div className="flex flex-wrap gap-2">
                {filtros.estado && (
                  <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    Estado: {filtros.estado}
                    <button
                      onClick={() => handleEstadoChange(undefined)}
                      className="ml-2 text-blue-600 hover:text-blue-800"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                )}
                {filtros.capacidadMinima && (
                  <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    Mín: {filtros.capacidadMinima}
                    <button
                      onClick={() => handleCapacidadChange('minima', '')}
                      className="ml-2 text-green-600 hover:text-green-800"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                )}
                {filtros.capacidadMaxima && (
                  <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    Máx: {filtros.capacidadMaxima}
                    <button
                      onClick={() => handleCapacidadChange('maxima', '')}
                      className="ml-2 text-green-600 hover:text-green-800"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                )}
                {filtros.ubicacion && (
                  <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                    Ubicación: {filtros.ubicacion}
                    <button
                      onClick={() => handleUbicacionChange('todas')}
                      className="ml-2 text-purple-600 hover:text-purple-800"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                )}
              </div>
            </div>
          )}
        </Card>
      )}
    </Card>
  );
};
