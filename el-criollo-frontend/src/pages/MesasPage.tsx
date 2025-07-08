import React, { useState, useMemo } from 'react';
import { BarChart3, AlertTriangle, Clock, TrendingUp, Users, MapPin } from 'lucide-react';
import { useMesas } from '@/hooks/useMesas';
import { MesaCard } from '@/components/mesas/MesaCard';
import { MesaFilters } from '@/components/mesas/MesaFilters';
import { Card, Button, Badge } from '@/components';
import LoadingSpinner from '@/components/ui/LoadingSpinner';
import type { FiltrosMesa, Mesa, EstadoMesa } from '@/types/mesa';

export const MesasPage: React.FC = () => {
  const [filtros, setFiltros] = useState<FiltrosMesa>({});

  const {
    mesas,
    estadisticas,
    loading,
    error,
    refrescar,
    liberarMesa,
    ocuparMesa,
    cambiarEstadoMesa,
    marcarMantenimiento,
    mesasQueNecesitanAtencion,
  } = useMesas({ autoRefresh: true, refreshInterval: 30000 });

  // Filtrar mesas según los filtros aplicados
  const mesasFiltradas = useMemo(() => {
    let resultado = [...mesas];

    if (filtros.estado) {
      resultado = resultado.filter((mesa) => mesa.estado === filtros.estado);
    }

    if (filtros.capacidadMinima) {
      resultado = resultado.filter((mesa) => mesa.capacidad >= filtros.capacidadMinima!);
    }

    if (filtros.capacidadMaxima) {
      resultado = resultado.filter((mesa) => mesa.capacidad <= filtros.capacidadMaxima!);
    }

    if (filtros.ubicacion) {
      resultado = resultado.filter((mesa) => mesa.ubicacion === filtros.ubicacion);
    }

    if (filtros.soloDisponibles) {
      resultado = resultado.filter((mesa) => mesa.estado === 'Libre');
    }

    return resultado;
  }, [mesas, filtros]);

  // Agrupar mesas por ubicación para mejor organización
  const mesasPorUbicacion = useMemo(() => {
    const grupos: Record<string, Mesa[]> = {};

    mesasFiltradas.forEach((mesa) => {
      const ubicacion = mesa.ubicacion || 'Sin ubicación';
      if (!grupos[ubicacion]) {
        grupos[ubicacion] = [];
      }
      grupos[ubicacion].push(mesa);
    });

    // Ordenar mesas por número dentro de cada ubicación
    Object.keys(grupos).forEach((ubicacion) => {
      grupos[ubicacion].sort((a, b) => a.numeroMesa - b.numeroMesa);
    });

    return grupos;
  }, [mesasFiltradas]);

  const handleMantenimiento = async (mesaId: number, motivo: string) => {
    return await marcarMantenimiento(mesaId, { motivo });
  };

  const handleCambiarEstado = async (mesaId: number, nuevoEstado: string, motivo?: string) => {
    return await cambiarEstadoMesa(mesaId, { nuevoEstado: nuevoEstado as EstadoMesa, motivo });
  };

  if (loading && mesas.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <LoadingSpinner size="lg" />
        <span className="ml-3 text-gray-600">Cargando estado de mesas...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Card className="text-center p-8 max-w-md">
          <AlertTriangle className="w-12 h-12 text-red-500 mx-auto mb-4" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Error al cargar mesas</h3>
          <p className="text-gray-600 mb-4">{error}</p>
          <Button onClick={refrescar} variant="primary">
            Reintentar
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header con estadísticas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-dominican-blue bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <BarChart3 className="w-6 h-6 text-dominican-blue" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Total Mesas</h3>
          <p className="text-3xl font-bold text-gray-900">{estadisticas.totalMesas}</p>
          <p className="text-sm text-gray-600">En el restaurante</p>
        </Card>

        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-palm-green bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <Users className="w-6 h-6 text-palm-green" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Ocupación</h3>
          <p className="text-3xl font-bold text-palm-green">
            {estadisticas.porcentajeOcupacion.toFixed(0)}%
          </p>
          <p className="text-sm text-gray-600">
            {estadisticas.mesasOcupadas + estadisticas.mesasReservadas} mesas activas
          </p>
        </Card>

        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-dominican-blue bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <Clock className="w-6 h-6 text-dominican-blue" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Disponibles</h3>
          <p className="text-3xl font-bold text-dominican-blue">{estadisticas.mesasLibres}</p>
          <p className="text-sm text-gray-600">Mesas libres</p>
        </Card>

        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-amber-500 bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <AlertTriangle className="w-6 h-6 text-amber-600" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Atención</h3>
          <p className="text-3xl font-bold text-amber-600">{mesasQueNecesitanAtencion.length}</p>
          <p className="text-sm text-gray-600">Requieren atención</p>
        </Card>
      </div>

      {/* Alertas y notificaciones */}
      {mesasQueNecesitanAtencion.length > 0 && (
        <Card className="border-l-4 border-amber-500 bg-amber-50">
          <div className="flex items-start">
            <AlertTriangle className="w-5 h-5 text-amber-600 mt-0.5" />
            <div className="ml-3">
              <h4 className="font-medium text-amber-800">Mesas que requieren atención</h4>
              <div className="mt-2 space-x-2">
                {mesasQueNecesitanAtencion().map((mesa) => (
                  <Badge key={mesa.mesaID} variant="warning" className="mr-2 mb-1">
                    Mesa {mesa.numeroMesa}
                    {mesa.necesitaLimpieza && ' 🧹'}
                    {mesa.requiereAtencion && ' ⚠️'}
                  </Badge>
                ))}
              </div>
            </div>
          </div>
        </Card>
      )}

      {/* Filtros */}
      <MesaFilters
        filtros={filtros}
        onFiltrosChange={setFiltros}
        onRefresh={refrescar}
        loading={loading}
        totalMesas={mesas.length}
        mesasFiltradas={mesasFiltradas.length}
      />

      {/* Vista de mesas por ubicación */}
      <div className="space-y-8">
        {Object.entries(mesasPorUbicacion).length === 0 ? (
          <Card className="text-center py-12">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <MapPin className="w-8 h-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">No se encontraron mesas</h3>
            <p className="text-gray-600 mb-4">Ajusta los filtros para ver las mesas disponibles</p>
            <Button onClick={() => setFiltros({})} variant="secondary">
              Limpiar filtros
            </Button>
          </Card>
        ) : (
          Object.entries(mesasPorUbicacion).map(([ubicacion, mesasUbicacion]) => (
            <div key={ubicacion} className="space-y-4">
              {/* Header de ubicación */}
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <MapPin className="w-5 h-5 text-dominican-blue" />
                  <h3 className="text-xl font-heading font-semibold text-dominican-blue">
                    {ubicacion}
                  </h3>
                  <Badge variant="secondary">
                    {mesasUbicacion.length} mesa{mesasUbicacion.length !== 1 ? 's' : ''}
                  </Badge>
                </div>

                {/* Estadísticas rápidas de la ubicación */}
                <div className="flex space-x-4 text-sm text-gray-600">
                  <span className="flex items-center">
                    <div className="w-2 h-2 bg-green-400 rounded-full mr-1"></div>
                    {mesasUbicacion.filter((m) => m.estado === 'Libre').length} libres
                  </span>
                  <span className="flex items-center">
                    <div className="w-2 h-2 bg-red-400 rounded-full mr-1"></div>
                    {mesasUbicacion.filter((m) => m.estado === 'Ocupada').length} ocupadas
                  </span>
                  <span className="flex items-center">
                    <div className="w-2 h-2 bg-blue-400 rounded-full mr-1"></div>
                    {mesasUbicacion.filter((m) => m.estado === 'Reservada').length} reservadas
                  </span>
                </div>
              </div>

              {/* Grid de mesas */}
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-6 gap-4">
                {mesasUbicacion.map((mesa) => (
                  <MesaCard
                    key={mesa.mesaID}
                    mesa={mesa}
                    onLiberar={liberarMesa}
                    onOcupar={ocuparMesa}
                    onCambiarEstado={handleCambiarEstado}
                    onMarcarMantenimiento={handleMantenimiento}
                  />
                ))}
              </div>
            </div>
          ))
        )}
      </div>

      {/* Información adicional */}
      <Card className="bg-gradient-to-r from-dominican-blue to-dominican-blue text-white">
        <div className="flex items-center justify-between">
          <div>
            <h4 className="font-heading font-semibold text-lg mb-2">
              🇩🇴 El Criollo - Sistema de Mesas
            </h4>
            <p className="text-blue-100">
              Gestión en tiempo real del estado de todas las mesas del restaurante. Los datos se
              actualizan automáticamente cada 30 segundos.
            </p>
          </div>
          <div className="text-right text-blue-100">
            <div className="flex items-center justify-end space-x-1 mb-1">
              <Clock className="w-4 h-4" />
              <span className="text-sm">
                Última actualización: {new Date().toLocaleTimeString('es-DO')}
              </span>
            </div>
            <div className="flex items-center justify-end space-x-1">
              <TrendingUp className="w-4 h-4" />
              <span className="text-sm">
                Ocupación promedio: {estadisticas.porcentajeOcupacion.toFixed(1)}%
              </span>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};
