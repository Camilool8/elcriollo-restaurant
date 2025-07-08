import React, { ReactNode } from 'react';
import { Link, useLocation, Outlet } from 'react-router-dom';
import {
  Home,
  Users,
  UserPlus,
  Briefcase,
  BarChart3,
  Settings,
  LogOut,
  Coffee,
} from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/Button';

interface AdminLayoutProps {
  children?: ReactNode;
}

const AdminLayout: React.FC<AdminLayoutProps> = ({ children }) => {
  const { state, logout } = useAuth();
  const location = useLocation();

  const navigation = [
    { name: 'Dashboard', href: '/admin', icon: Home },
    { name: 'Usuarios', href: '/admin/usuarios', icon: Users },
    { name: 'Clientes', href: '/admin/clientes', icon: UserPlus },
    { name: 'Empleados', href: '/admin/empleados', icon: Briefcase },
    { name: 'Reportes', href: '/admin/reportes', icon: BarChart3 },
    { name: 'Configuración', href: '/admin/configuracion', icon: Settings },
  ];

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="min-h-screen bg-warm-beige flex">
      {/* Sidebar */}
      <div className="w-64 bg-white shadow-lg border-r">
        {/* Logo */}
        <div className="p-6 border-b">
          <Link to="/admin" className="flex items-center">
            <div className="w-10 h-10 bg-dominican-red rounded-lg flex items-center justify-center mr-3">
              <Coffee className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="font-heading font-bold text-dominican-blue">El Criollo</h1>
              <p className="text-xs text-stone-gray">Admin Panel</p>
            </div>
          </Link>
        </div>

        {/* Navigation */}
        <nav className="mt-6 px-4">
          <div className="space-y-2">
            {navigation.map((item) => {
              const isActive =
                location.pathname === item.href ||
                (item.href !== '/admin' && location.pathname.startsWith(item.href));

              return (
                <Link
                  key={item.name}
                  to={item.href}
                  className={`
                    flex items-center px-3 py-2 rounded-lg text-sm font-medium smooth-transition
                    ${
                      isActive
                        ? 'bg-dominican-red text-white'
                        : 'text-stone-gray hover:bg-gray-100 hover:text-dominican-blue'
                    }
                  `}
                >
                  <item.icon className="w-5 h-5 mr-3" />
                  {item.name}
                </Link>
              );
            })}
          </div>
        </nav>

        {/* User info */}
        <div className="absolute bottom-0 w-64 p-4 border-t bg-gray-50">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-900">
                {state.user?.usuario || state.user?.username}
              </p>
              <p className="text-xs text-dominican-red">
                {state.user?.nombreRol || state.user?.rol}
              </p>
            </div>

            <Button variant="ghost" size="sm" onClick={handleLogout}>
              <LogOut className="w-4 h-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Main content */}
      <div className="flex-1 overflow-auto">
        <main className="p-6">{children || <Outlet />}</main>
      </div>
    </div>
  );
};

export default AdminLayout;
