import React from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import {
  LogOut,
  Settings,
  BarChart3,
  Users,
  UtensilsCrossed,
  CreditCard,
  Calendar,
  ChefHat,
} from 'lucide-react';

const DashboardPage: React.FC = () => {
  const { state, logout, isAdmin, isCajero, isMesero, isRecepcion, isCocina } = useAuth();

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="min-h-screen bg-warm-beige">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <h1 className="text-2xl font-heading font-bold text-dominican-blue">El Criollo 🇩🇴</h1>
            </div>

            <div className="flex items-center space-x-4">
              <div className="text-right">
                <p className="text-sm font-medium text-gray-900">{state.user?.usuario}</p>
                <p className="text-xs text-dominican-red">{state.user?.nombreRol}</p>
              </div>

              <Button
                variant="outline"
                size="sm"
                onClick={handleLogout}
                leftIcon={<LogOut className="w-4 h-4" />}
              >
                Salir
              </Button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Bienvenida */}
        <div className="mb-8">
          <h2 className="text-3xl font-heading font-bold text-dominican-blue mb-2">
            ¡Bienvenido, {state.user?.usuario}!
          </h2>
          <p className="text-stone-gray">
            Panel de control del Sistema POS - Restaurante Dominicano El Criollo
          </p>
        </div>

        {/* Métricas rápidas */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <Card className="text-center" hover>
            <div className="w-12 h-12 bg-dominican-red bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
              <BarChart3 className="w-6 h-6 text-dominican-red" />
            </div>
            <h3 className="font-heading font-semibold text-dominican-blue">Ventas Hoy</h3>
            <p className="text-2xl font-bold text-dominican-red">RD$ 0.00</p>
          </Card>

          <Card className="text-center" hover>
            <div className="w-12 h-12 bg-dominican-blue bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
              <UtensilsCrossed className="w-6 h-6 text-dominican-blue" />
            </div>
            <h3 className="font-heading font-semibold text-dominican-blue">Órdenes</h3>
            <p className="text-2xl font-bold text-dominican-blue">0</p>
          </Card>

          <Card className="text-center" hover>
            <div className="w-12 h-12 bg-caribbean-gold bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
              <Users className="w-6 h-6 text-yellow-600" />
            </div>
            <h3 className="font-heading font-semibold text-dominican-blue">Clientes</h3>
            <p className="text-2xl font-bold text-yellow-600">0</p>
          </Card>

          <Card className="text-center" hover>
            <div className="w-12 h-12 bg-palm-green bg-opacity-10 rounded-lg flex items-center justify-center mx-auto mb-3">
              <Calendar className="w-6 h-6 text-palm-green" />
            </div>
            <h3 className="font-heading font-semibold text-dominican-blue">Reservas</h3>
            <p className="text-2xl font-bold text-palm-green">0</p>
          </Card>
        </div>

        {/* Acciones rápidas por rol */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Panel izquierdo - Acciones principales */}
          <Card>
            <h3 className="text-xl font-heading font-semibold text-dominican-blue mb-4">
              Acciones Rápidas
            </h3>

            <div className="space-y-3">
              {/* Administrador */}
              {isAdmin() && (
                <>
                  <Button variant="primary" fullWidth leftIcon={<Settings className="w-5 h-5" />}>
                    Gestión de Usuarios
                  </Button>
                  <Button
                    variant="secondary"
                    fullWidth
                    leftIcon={<BarChart3 className="w-5 h-5" />}
                  >
                    Reportes y Analytics
                  </Button>
                </>
              )}

              {/* Cajero */}
              {(isCajero() || isAdmin()) && (
                <Button variant="primary" fullWidth leftIcon={<CreditCard className="w-5 h-5" />}>
                  Gestión de Facturas
                </Button>
              )}

              {/* Mesero */}
              {(isMesero() || isAdmin()) && (
                <Button
                  variant="primary"
                  fullWidth
                  leftIcon={<UtensilsCrossed className="w-5 h-5" />}
                >
                  Tomar Orden
                </Button>
              )}

              {/* Recepción */}
              {(isRecepcion() || isAdmin()) && (
                <Button variant="primary" fullWidth leftIcon={<Calendar className="w-5 h-5" />}>
                  Gestión de Reservas
                </Button>
              )}

              {/* Cocina */}
              {(isCocina() || isAdmin()) && (
                <Button variant="primary" fullWidth leftIcon={<ChefHat className="w-5 h-5" />}>
                  Panel de Cocina
                </Button>
              )}
            </div>
          </Card>

          {/* Panel derecho - Estado del sistema */}
          <Card>
            <h3 className="text-xl font-heading font-semibold text-dominican-blue mb-4">
              Estado del Sistema
            </h3>

            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Servidor API</span>
                <span className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-sm">
                  ✅ Conectado
                </span>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-gray-600">Base de Datos</span>
                <span className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-sm">
                  ✅ Operativa
                </span>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-gray-600">Última Sincronización</span>
                <span className="text-sm text-gray-500">Hace pocos segundos</span>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-gray-600">Versión del Sistema</span>
                <span className="text-sm text-gray-500">v1.0.0</span>
              </div>
            </div>
          </Card>
        </div>

        {/* Footer del dashboard */}
        <div className="mt-12 text-center text-stone-gray text-sm">
          <p>🇩🇴 Sistema POS El Criollo - Autenticidad Dominicana en cada funcionalidad</p>
          <p className="mt-1">Desarrollado para restaurantes con sabor caribeño</p>
        </div>
      </main>
    </div>
  );
};

export default DashboardPage;
