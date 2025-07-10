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
  ClipboardList,
  Package,
  UtensilsCrossed,
} from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';

interface AdminLayoutProps {
  children?: ReactNode;
}

const AdminLayout: React.FC<AdminLayoutProps> = ({ children }) => {
  const { state, logout, hasAnyRole } = useAuth();
  const location = useLocation();

  const allNavigation = [
    {
      name: 'Dashboard',
      href: '/admin',
      icon: Home,
      roles: ['Administrador', 'Cajero', 'Mesero', 'Recepcion', 'Cocina'],
    },
    {
      name: 'Mesas',
      href: '/mesas',
      icon: ClipboardList,
      roles: ['Administrador', 'Cajero', 'Mesero', 'Recepcion'],
    },
    { name: 'Usuarios', href: '/admin/usuarios', icon: Users, roles: ['Administrador'] },
    {
      name: 'Clientes',
      href: '/admin/clientes',
      icon: UserPlus,
      roles: ['Administrador', 'Cajero', 'Recepcion'],
    },
    { name: 'Empleados', href: '/admin/empleados', icon: Briefcase, roles: ['Administrador'] },
    {
      name: 'Productos',
      href: '/admin/productos',
      icon: UtensilsCrossed,
      roles: ['Administrador'],
    },
    { name: 'Categorías', href: '/admin/categorias', icon: Package, roles: ['Administrador'] },
    { name: 'Reportes', href: '/admin/reportes', icon: BarChart3, roles: ['Administrador'] },
  ];

  const navigation = allNavigation.filter((item) => hasAnyRole(item.roles));

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="min-h-screen bg-warm-beige flex">
      {/* Sidebar */}
      <div className="w-64 bg-white shadow-lg border-r">
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
      </div>

      {/* Main content */}
      <div className="flex-1 overflow-auto">
        <main className="p-6">{children || <Outlet />}</main>
      </div>
    </div>
  );
};

export default AdminLayout;
