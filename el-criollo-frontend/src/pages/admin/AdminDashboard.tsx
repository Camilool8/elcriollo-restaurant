import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Users,
  UserPlus,
  Briefcase,
  BarChart3,
  Settings,
  Database,
  ShieldCheck,
  DollarSign,
  UtensilsCrossed,
  Package,
  Clock,
  Calendar,
  Activity,
  Award,
  Star,
} from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/contexts/AuthContext';
import { dashboardService, DashboardResponse } from '@/services/dashboardService';
import { formatearPrecio } from '@/utils/dominicanValidations';

const AdminDashboard: React.FC = () => {
  const { state } = useAuth();
  const [stats, setStats] = useState<DashboardResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // ====================================
  // EFECTOS
  // ====================================

  useEffect(() => {
    loadDashboardData();
  }, []);

  // ====================================
  // FUNCIONES
  // ====================================

  const loadDashboardData = async () => {
    setIsLoading(true);
    try {
      const statsData = await dashboardService.getDashboardStats();
      setStats(statsData);
    } catch (error) {
      console.error('Error cargando dashboard:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // ====================================
  // RENDER
  // ====================================

  if (isLoading) {
    return (
      <div className="min-h-96 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin w-8 h-8 border-2 border-dominican-red border-t-transparent rounded-full mx-auto mb-4" />
          <p className="text-stone-gray">Cargando dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header con gradiente */}
      <div className="relative overflow-hidden rounded-xl bg-gradient-to-r from-dominican-red via-dominican-blue to-caribbean-gold p-8 text-white">
        <div className="absolute inset-0 bg-black bg-opacity-20"></div>
        <div className="relative z-10">
          <div className="flex justify-between items-start">
            <div>
              <div className="flex items-center mb-2">
                <div className="w-12 h-12 bg-white bg-opacity-20 rounded-lg flex items-center justify-center mr-4">
                  <Award className="w-6 h-6 text-white" />
                </div>
                <div>
                  <h1 className="text-4xl font-heading font-bold">Panel de Administración</h1>
                  <p className="text-white text-opacity-90">
                    Sistema de Gestión Restaurante El Criollo
                  </p>
                </div>
              </div>
              <p className="text-white text-opacity-80 mt-2">
                Bienvenido, <span className="font-semibold">{state.user?.usuario}</span> - Centro de
                Control
              </p>
            </div>

            <div className="text-right">
              <div className="bg-white bg-opacity-20 rounded-lg p-4">
                <p className="text-sm text-white text-opacity-90">
                  {new Date().toLocaleDateString('es-DO', {
                    weekday: 'long',
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </p>
                <p className="text-2xl font-bold">
                  {new Date().toLocaleTimeString('es-DO', {
                    hour: '2-digit',
                    minute: '2-digit',
                  })}
                </p>
                <div className="flex items-center mt-2">
                  <div className="w-2 h-2 bg-green-400 rounded-full mr-2"></div>
                  <span className="text-xs">Sistema Activo</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Métricas principales con mejor diseño */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="text-center border-l-4 border-l-dominican-red" hover>
          <div className="w-16 h-16 bg-gradient-to-br from-dominican-red to-red-600 rounded-xl flex items-center justify-center mx-auto mb-4 shadow-lg">
            <DollarSign className="w-8 h-8 text-white" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue text-lg">Ventas Hoy</h3>
          <p className="text-3xl font-bold text-dominican-red mb-2">
            {stats ? formatearPrecio(stats.ventasHoy) : 'Cargando...'}
          </p>
          <p className="text-sm text-stone-gray">Facturado del día</p>
        </Card>

        <Card className="text-center border-l-4 border-l-dominican-blue" hover>
          <div className="w-16 h-16 bg-gradient-to-br from-dominican-blue to-blue-600 rounded-xl flex items-center justify-center mx-auto mb-4 shadow-lg">
            <UtensilsCrossed className="w-8 h-8 text-white" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue text-lg">
            Órdenes Procesadas
          </h3>
          <p className="text-3xl font-bold text-dominican-blue mb-2">{stats?.ordenesHoy || 0}</p>
          <p className="text-sm text-stone-gray">
            Promedio:{' '}
            {stats && stats.ordenesHoy > 0
              ? formatearPrecio(stats.ventasHoy / stats.ordenesHoy)
              : 'N/A'}
          </p>
        </Card>

        <Card className="text-center border-l-4 border-l-caribbean-gold" hover>
          <div className="w-16 h-16 bg-gradient-to-br from-caribbean-gold to-yellow-500 rounded-xl flex items-center justify-center mx-auto mb-4 shadow-lg">
            <UtensilsCrossed className="w-8 h-8 text-white" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue text-lg">Mesas Ocupadas</h3>
          <p className="text-3xl font-bold text-yellow-600 mb-2">
            {stats?.mesasOcupadas || 0}/{stats?.totalMesas || 0}
          </p>
          <p className="text-sm text-stone-gray">
            {stats ? Math.round(stats.porcentajeOcupacion) : 0}% de ocupación
          </p>
        </Card>

        <Card className="text-center border-l-4 border-l-palm-green" hover>
          <div className="w-16 h-16 bg-gradient-to-br from-palm-green to-green-500 rounded-xl flex items-center justify-center mx-auto mb-4 shadow-lg">
            <Activity className="w-8 h-8 text-white" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue text-lg">
            Órdenes Activas
          </h3>
          <p className="text-3xl font-bold text-palm-green mb-2">{stats?.ordenesActivas || 0}</p>
          <p className="text-sm text-stone-gray">En preparación</p>
        </Card>
      </div>

      {/* Métricas adicionales */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card className="text-center" hover>
          <div className="flex items-center justify-center mb-3">
            <Clock className="w-5 h-5 text-dominican-blue mr-2" />
            <h4 className="font-semibold text-dominican-blue">Ticket Promedio</h4>
          </div>
          <div className="text-2xl font-bold text-dominican-red">
            {stats && stats.ordenesHoy > 0
              ? formatearPrecio(stats.ventasHoy / stats.ordenesHoy)
              : 'RD$ 0'}
          </div>
          <p className="text-sm text-stone-gray">Por orden</p>
        </Card>

        <Card className="text-center" hover>
          <div className="flex items-center justify-center mb-3">
            <Star className="w-5 h-5 text-dominican-blue mr-2" />
            <h4 className="font-semibold text-dominican-blue">Capacidad Total</h4>
          </div>
          <div className="text-2xl font-bold text-palm-green">
            {stats ? Math.round(stats.porcentajeOcupacion) : 0}%
          </div>
          <p className="text-sm text-stone-gray">Ocupación actual</p>
        </Card>
      </div>

      {/* Acciones rápidas mejoradas */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Gestión de Personal */}
        <Card className="border-l-4 border-l-dominican-red">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-gradient-to-br from-dominican-red to-red-600 rounded-lg flex items-center justify-center mr-4">
                <ShieldCheck className="w-6 h-6 text-white" />
              </div>
              <div>
                <h3 className="text-xl font-heading font-semibold text-dominican-blue">
                  Gestión de Personal
                </h3>
                <p className="text-stone-gray text-sm">Administración de usuarios y empleados</p>
              </div>
            </div>
          </div>

          <div className="space-y-3">
            <Link to="/admin/usuarios">
              <Button variant="primary" fullWidth leftIcon={<Users className="w-4 h-4" />}>
                <div className="text-left">
                  <div className="font-semibold">Gestión de Usuarios</div>
                  <div className="text-xs opacity-80">Administrar accesos y permisos</div>
                </div>
              </Button>
            </Link>

            <Link to="/admin/empleados">
              <Button variant="secondary" fullWidth leftIcon={<Briefcase className="w-4 h-4" />}>
                <div className="text-left">
                  <div className="font-semibold">Gestión de Empleados</div>
                  <div className="text-xs opacity-80">Personal y horarios</div>
                </div>
              </Button>
            </Link>

            <Link to="/admin/clientes">
              <Button variant="outline" fullWidth leftIcon={<UserPlus className="w-4 h-4" />}>
                <div className="text-left">
                  <div className="font-semibold">Gestión de Clientes</div>
                  <div className="text-xs opacity-80">Base de datos de clientes</div>
                </div>
              </Button>
            </Link>
          </div>
        </Card>

        {/* Operaciones y Reportes */}
        <Card className="border-l-4 border-l-dominican-blue">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center">
              <div className="w-12 h-12 bg-gradient-to-br from-dominican-blue to-blue-600 rounded-lg flex items-center justify-center mr-4">
                <Database className="w-6 h-6 text-white" />
              </div>
              <div>
                <h3 className="text-xl font-heading font-semibold text-dominican-blue">
                  Operaciones y Reportes
                </h3>
                <p className="text-stone-gray text-sm">Analytics y configuración del sistema</p>
              </div>
            </div>
          </div>

          <div className="space-y-3">
            <Link to="/admin/reportes">
              <Button variant="secondary" fullWidth leftIcon={<BarChart3 className="w-4 h-4" />}>
                <div className="text-left">
                  <div className="font-semibold">Reportes de Ventas</div>
                  <div className="text-xs opacity-80">Analytics y métricas detalladas</div>
                </div>
              </Button>
            </Link>

            <Link to="/admin/productos">
              <Button
                variant="outline"
                fullWidth
                leftIcon={<UtensilsCrossed className="w-4 h-4" />}
              >
                <div className="text-left">
                  <div className="font-semibold">Gestión de Productos</div>
                  <div className="text-xs opacity-80">Menú y inventario</div>
                </div>
              </Button>
            </Link>

            <Link to="/admin/categorias">
              <Button variant="outline" fullWidth leftIcon={<Package className="w-4 h-4" />}>
                <div className="text-left">
                  <div className="font-semibold">Gestión de Categorías</div>
                  <div className="text-xs opacity-80">Organización del menú</div>
                </div>
              </Button>
            </Link>
          </div>
        </Card>
      </div>

      {/* Footer oficial */}
      <Card className="text-center bg-gradient-to-r from-dominican-blue to-dominican-red text-white">
        <div className="flex items-center justify-center mb-2">
          <div className="w-8 h-8 bg-white bg-opacity-20 rounded-lg flex items-center justify-center mr-3">
            <Award className="w-4 h-4 text-white" />
          </div>
          <h3 className="text-lg font-semibold">Restaurante El Criollo</h3>
        </div>
        <p className="text-white text-opacity-90 mb-2">Sistema de Gestión Integral - Versión 2.0</p>
        <div className="flex items-center justify-center space-x-4 text-sm text-white text-opacity-80">
          <span>🇩🇴 República Dominicana</span>
          <span>•</span>
          <span>Sistema Operativo</span>
          <span>•</span>
          <span>Última actualización: {new Date().toLocaleDateString()}</span>
        </div>
      </Card>
    </div>
  );
};

export default AdminDashboard;
