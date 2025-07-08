import React from 'react';
import { Outlet, Link } from 'react-router-dom';
import { LogOut, Coffee } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/Button';

export const AuthenticatedLayout: React.FC = () => {
  const { state, logout } = useAuth();

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="min-h-screen bg-warm-beige">
      <header className="bg-white shadow-md sticky top-0 z-20">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-2 flex justify-between items-center">
          {/* Logo */}
          <Link to="/" className="flex items-center">
            <div className="w-10 h-10 bg-dominican-red rounded-lg flex items-center justify-center mr-3">
              <Coffee className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="font-heading font-bold text-dominican-blue">El Criollo</h1>
              <p className="text-xs text-stone-gray hidden sm:block">Sistema de Gestión</p>
            </div>
          </Link>

          {/* User Menu */}
          {state.user && (
            <div className="flex items-center space-x-4">
              <div className="text-right">
                <p className="text-sm font-medium text-gray-900">{state.user.usuario}</p>
                <p className="text-xs text-dominican-red capitalize">{state.user.rol}</p>
              </div>
              <Button
                variant="ghost"
                size="icon"
                onClick={handleLogout}
                aria-label="Cerrar sesión"
                className="text-gray-600 hover:bg-red-50 hover:text-dominican-red"
              >
                <LogOut className="w-5 h-5" />
              </Button>
            </div>
          )}
        </div>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
};
