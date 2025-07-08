import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Users,
  UserPlus,
  Briefcase,
  BarChart3,
  Settings,
  Database,
  TrendingUp,
  ShieldCheck,
  AlertTriangle,
  Clock,
  DollarSign,
  UtensilsCrossed,
  Calendar,
  Package,
} from 'lucide-react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { useAuth } from '@/contexts/AuthContext';
import { dashboardService, DashboardStats, AlertaOperacional } from '@/services/dashboardService';
import { formatearPrecio } from '@/utils/dominicanValidations';

const AdminDashboard: React.FC = () => {
  const { state } = useAuth();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [alertas, setAlertas] = useState<AlertaOperacional[]>([]);
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
      const [statsData, alertasData] = await Promise.all([
        dashboardService.getDashboardStats(),
        dashboardService.getAlertasOperacionales(),
      ]);

      setStats(statsData);
      setAlertas(alertasData);
    } catch (error) {
      console.error('Error cargando dashboard:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const getPrioridadColor = (prioridad: string) => {
    switch (prioridad) {
      case 'alta':
        return 'danger';
      case 'media':
        return 'warning';
      case 'baja':
        return 'info';
      default:
        return 'secondary';
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
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-heading font-bold text-dominican-blue">
            Panel de Administración
          </h1>
          <p className="text-stone-gray mt-1">Gestión completa del sistema El Criollo</p>
          <p className="text-sm text-dominican-red mt-1">Bienvenido, {state.user?.usuario}</p>
        </div>

        <div className="text-right">
          <p className="text-sm text-stone-gray">
            {new Date().toLocaleDateString('es-DO', {
              weekday: 'long',
              year: 'numeric',
              month: 'long',
              day: 'numeric',
            })}
          </p>
          <p className="font-medium text-dominican-blue">
            {new Date().toLocaleTimeString('es-DO', {
              hour: '2-digit',
              minute: '2-digit',
            })}
          </p>
        </div>
      </div>

      {/* Alertas operacionales */}
      {alertas.length > 0 && (
        <Card className="border-l-4 border-l-red-500">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-heading font-semibold text-dominican-blue flex items-center">
              <AlertTriangle className="w-5 h-5 mr-2 text-red-500" />
              Alertas Operacionales
            </h3>
            <Badge variant="danger">{alertas.length}</Badge>
          </div>

          <div className="space-y-2">
            {alertas.slice(0, 3).map((alerta) => (
              <div
                key={alerta.id}
                className="flex items-center justify-between p-3 bg-red-50 rounded-lg"
              >
                <div>
                  <p className="font-medium text-red-800">{alerta.titulo}</p>
                  <p className="text-sm text-red-600">{alerta.mensaje}</p>
                </div>
                <Badge variant={getPrioridadColor(alerta.prioridad)}>{alerta.prioridad}</Badge>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Métricas principales */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-dominican-red bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <DollarSign className="w-6 h-6 text-dominican-red" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Ventas Hoy</h3>
          <p className="text-2xl font-bold text-dominican-red">
            {stats ? formatearPrecio(stats.ventasHoy) : 'Cargando...'}
          </p>
          {stats && stats.crecimientoVentas > 0 && (
            <p className="text-sm text-green-600 flex items-center justify-center mt-1">
              <TrendingUp className="w-3 h-3 mr-1" />+{stats.crecimientoVentas}% vs ayer
            </p>
          )}
        </Card>

        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-dominican-blue bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <UtensilsCrossed className="w-6 h-6 text-dominican-blue" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Órdenes</h3>
          <p className="text-2xl font-bold text-dominican-blue">{stats?.ordenesHoy || 0}</p>
          <p className="text-sm text-stone-gray">
            Ticket promedio: {stats ? formatearPrecio(stats.ticketPromedio) : 'N/A'}
          </p>
        </Card>

        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-caribbean-gold bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <Users className="w-6 h-6 text-yellow-600" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Clientes Hoy</h3>
          <p className="text-2xl font-bold text-yellow-600">{stats?.clientesHoy || 0}</p>
          <p className="text-sm text-stone-gray">Mesas ocupadas: {stats?.mesasOcupadas || 0}</p>
        </Card>

        <Card className="text-center" hover>
          <div className="w-12 h-12 bg-palm-green bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
            <Calendar className="w-6 h-6 text-palm-green" />
          </div>
          <h3 className="font-heading font-semibold text-dominican-blue">Reservas Hoy</h3>
          <p className="text-2xl font-bold text-palm-green">{stats?.reservacionesHoy || 0}</p>
          <p className="text-sm text-stone-gray">
            {stats?.facturasPendientes || 0} facturas pendientes
          </p>
        </Card>
      </div>

      {/* Estadísticas adicionales */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="text-center" padding="sm">
          <div className="flex items-center justify-center mb-2">
            <Briefcase className="w-5 h-5 text-dominican-blue mr-2" />
            <span className="font-medium text-dominican-blue">Personal Activo</span>
          </div>
          <p className="text-xl font-bold text-dominican-blue">{stats?.empleadosActivos || 0}</p>
        </Card>

        <Card className="text-center" padding="sm">
          <div className="flex items-center justify-center mb-2">
            <Package className="w-5 h-5 text-red-600 mr-2" />
            <span className="font-medium text-red-600">Stock Bajo</span>
          </div>
          <p className="text-xl font-bold text-red-600">{stats?.productosBajoStock || 0}</p>
        </Card>

        <Card className="text-center" padding="sm">
          <div className="flex items-center justify-center mb-2">
            <Clock className="w-5 h-5 text-yellow-600 mr-2" />
            <span className="font-medium text-yellow-600">Pendientes</span>
          </div>
          <p className="text-xl font-bold text-yellow-600">{stats?.facturasPendientes || 0}</p>
        </Card>
      </div>

      {/* Acciones rápidas */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Gestión de Personal */}
        <Card>
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-xl font-heading font-semibold text-dominican-blue flex items-center">
              <Users className="w-5 h-5 mr-2" />
              Gestión de Personal
            </h3>
            <ShieldCheck className="w-6 h-6 text-dominican-red" />
          </div>

          <p className="text-stone-gray mb-4">Administra usuarios, empleados y roles del sistema</p>

          <div className="space-y-3">
            <Link to="/admin/usuarios">
              <Button variant="primary" fullWidth leftIcon={<Users className="w-4 h-4" />}>
                Gestión de Usuarios
              </Button>
            </Link>

            <Link to="/admin/empleados">
              <Button variant="secondary" fullWidth leftIcon={<Briefcase className="w-4 h-4" />}>
                Gestión de Empleados
              </Button>
            </Link>

            <Link to="/admin/clientes">
              <Button variant="outline" fullWidth leftIcon={<UserPlus className="w-4 h-4" />}>
                Gestión de Clientes
              </Button>
            </Link>
          </div>
        </Card>

        {/* Operaciones */}
        <Card>
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-xl font-heading font-semibold text-dominican-blue flex items-center">
              <BarChart3 className="w-5 h-5 mr-2" />
              Operaciones y Reportes
            </h3>
            <Database className="w-6 h-6 text-dominican-blue" />
          </div>

          <p className="text-stone-gray mb-4">Reportes, analytics y configuración del sistema</p>

          <div className="space-y-3">
            <Link to="/admin/reportes">
              <Button variant="secondary" fullWidth leftIcon={<BarChart3 className="w-4 h-4" />}>
                Ver Reportes
              </Button>
            </Link>

            <Link to="/admin/productos">
              <Button
                variant="outline"
                fullWidth
                leftIcon={<UtensilsCrossed className="w-4 h-4" />}
              >
                Gestión de Productos
              </Button>
            </Link>

            <Link to="/admin/configuracion">
              <Button variant="outline" fullWidth leftIcon={<Settings className="w-4 h-4" />}>
                Configuración
              </Button>
            </Link>
          </div>
        </Card>
      </div>

      {/* Footer */}
      <Card className="text-center" padding="sm">
        <p className="text-stone-gray text-sm">🇩🇴 Sistema POS El Criollo - Fase 2 Completada</p>
        <p className="text-stone-gray text-xs mt-1">
          Gestión administrativa implementada con éxito
        </p>
      </Card>
    </div>
  );
};

export default AdminDashboard;
