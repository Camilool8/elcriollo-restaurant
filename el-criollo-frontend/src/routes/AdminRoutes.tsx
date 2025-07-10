import React from 'react';
import { Routes, Route } from 'react-router-dom';
import AdminLayout from '@/components/layout/AdminLayout';
import AdminDashboard from '@/pages/admin/AdminDashboard';
import UsuariosPage from '@/pages/admin/UsuariosPage';
import ClientesPage from '@/pages/admin/ClientesPage';
import EmpleadosPage from '@/pages/admin/EmpleadosPage';
import ProductosPage from '@/pages/admin/ProductosPage';
import CategoriasPage from '@/pages/admin/CategoriasPage';
import ReportesVentasPage from '@/pages/admin/ReportesVentasPage';

const AdminRoutes: React.FC = () => {
  return (
    <AdminLayout>
      <Routes>
        <Route index element={<AdminDashboard />} />
        <Route path="usuarios" element={<UsuariosPage />} />
        <Route path="clientes" element={<ClientesPage />} />
        <Route path="empleados" element={<EmpleadosPage />} />
        <Route path="productos" element={<ProductosPage />} />
        <Route path="categorias" element={<CategoriasPage />} />
        <Route path="reportes" element={<ReportesVentasPage />} />
      </Routes>
    </AdminLayout>
  );
};

export default AdminRoutes;
