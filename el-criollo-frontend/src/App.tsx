import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

// Contexts
import { AuthProvider } from '@/contexts/AuthContext';

// Components y Pages
import { ProtectedRoute, AdminRoute } from '@/components/auth';
import LoginPage from '@/pages/LoginPage';
import DashboardPage from '@/pages/DashboardPage';
import AdminRoutes from '@/routes/AdminRoutes';
import { useAuth } from '@/contexts/AuthContext';
import { MesasPage } from '@/pages/MesasPage';
import { AuthenticatedLayout } from '@/components/layout/AuthenticatedLayout';

// ====================================
// COMPONENTE PRINCIPAL
// ====================================

const App: React.FC = () => {
  return (
    <Router>
      <AuthProvider>
        <div className="App min-h-screen bg-warm-beige">
          {/* Rutas principales */}
          <Routes>
            {/* Login es público */}
            <Route path="/login" element={<LoginPage />} />

            {/* Rutas que requieren autenticación */}
            <Route
              element={
                <ProtectedRoute>
                  <AuthenticatedLayout />
                </ProtectedRoute>
              }
            >
              <Route path="/" element={<SmartRedirect />} />
              <Route path="/mesas" element={<MesasPage />} />
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route
                path="/admin/*"
                element={
                  <AdminRoute>
                    <AdminRoutes />
                  </AdminRoute>
                }
              />
            </Route>

            {/* Ruta catch-all para 404 */}
            <Route path="*" element={<NotFoundPage />} />
          </Routes>

          {/* Notificaciones Toast configuradas para tema dominicano */}
          <ToastContainer
            position="top-right"
            autoClose={3000}
            hideProgressBar={false}
            newestOnTop
            closeOnClick
            rtl={false}
            pauseOnFocusLoss
            draggable
            pauseOnHover
            theme="colored"
            className="dominican-toast"
            toastClassName="font-sans"
            bodyClassName="text-sm"
            progressStyle={{ background: '#CF142B' }}
          />
        </div>
      </AuthProvider>
    </Router>
  );
};

// ====================================
// COMPONENTE SMART REDIRECT
// ====================================

const SmartRedirect: React.FC = () => {
  const { state, isAdmin } = useAuth();

  // Mientras carga la autenticación
  if (state.isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-warm-beige">
        <div className="text-center">
          <div className="animate-spin w-8 h-8 border-2 border-dominican-red border-t-transparent rounded-full mx-auto mb-4" />
          <p className="text-dominican-blue font-medium">Cargando El Criollo...</p>
        </div>
      </div>
    );
  }

  // Si no está autenticado, ir al login
  if (!state.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Si es admin, ir al panel de administración
  if (isAdmin()) {
    return <Navigate to="/admin" replace />;
  }

  // Para otros roles, ir al dashboard básico
  return <Navigate to="/dashboard" replace />;
};

// ====================================
// PÁGINA 404 MEJORADA
// ====================================

const NotFoundPage: React.FC = () => {
  const { state, isAdmin } = useAuth();

  const getHomeLink = () => {
    if (!state.isAuthenticated) return '/login';
    if (isAdmin()) return '/admin';
    return '/dashboard';
  };

  const getHomeLinkText = () => {
    if (!state.isAuthenticated) return 'Ir al Login';
    if (isAdmin()) return 'Panel de Administración';
    return 'Dashboard Principal';
  };

  return (
    <div className="min-h-screen bg-warm-beige flex items-center justify-center p-4">
      <div className="text-center max-w-md">
        {/* Icono 404 */}
        <div className="w-24 h-24 mx-auto mb-6 bg-dominican-red bg-opacity-10 rounded-full flex items-center justify-center">
          <span className="text-4xl">🔍</span>
        </div>

        {/* Título */}
        <h1 className="text-4xl font-heading font-bold text-dominican-blue mb-4">
          404 - Página No Encontrada
        </h1>

        {/* Descripción */}
        <p className="text-stone-gray mb-6">
          La página que buscas no existe o ha sido movida.
          {state.isAuthenticated
            ? ' Regresa al panel principal para continuar navegando.'
            : ' Por favor, inicia sesión para acceder al sistema.'}
        </p>

        {/* Acciones */}
        <div className="space-y-3">
          <a href={getHomeLink()}>
            <button className="w-full px-6 py-3 bg-dominican-red text-white rounded-lg hover:bg-red-700 smooth-transition font-medium">
              {getHomeLinkText()}
            </button>
          </a>

          <button
            onClick={() => window.history.back()}
            className="w-full px-6 py-3 border border-dominican-blue text-dominican-blue rounded-lg hover:bg-blue-50 smooth-transition font-medium"
          >
            Regresar
          </button>
        </div>

        {/* Footer */}
        <div className="mt-8 text-center text-stone-gray text-sm">
          <p>🇩🇴 El Criollo Restaurant POS</p>
          <p className="mt-1">Sistema de gestión con sabor dominicano</p>
        </div>
      </div>
    </div>
  );
};

export default App;
