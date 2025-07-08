import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

// Contexts
import { AuthProvider } from '@/contexts/AuthContext';

// Components
import { ProtectedRoute } from '@/components/auth';
import LoginPage from '@/pages/LoginPage';
import DashboardPage from '@/pages/DashboardPage';

// Styles
import '@/styles/globals.css';

// ====================================
// COMPONENTE PRINCIPAL
// ====================================

const App: React.FC = () => {
  return (
    <Router>
      <AuthProvider>
        <div className="App">
          {/* Rutas principales */}
          <Routes>
            {/* Ruta raíz - redirige al dashboard */}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />

            {/* Login público */}
            <Route path="/login" element={<LoginPage />} />

            {/* Dashboard protegido */}
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <DashboardPage />
                </ProtectedRoute>
              }
            />

            {/* Rutas administrativas */}
            {/* TODO: Implementar estas rutas en las siguientes fases */}
            {/*
            <Route 
              path="/admin/*" 
              element={
                <AdminRoute>
                  <AdminRoutes />
                </AdminRoute>
              } 
            />
            
            <Route 
              path="/caja/*" 
              element={
                <CajeroRoute>
                  <CajeroRoutes />
                </CajeroRoute>
              } 
            />
            
            <Route 
              path="/mesero/*" 
              element={
                <MeseroRoute>
                  <MeseroRoutes />
                </MeseroRoute>
              } 
            />
            
            <Route 
              path="/recepcion/*" 
              element={
                <RecepcionRoute>
                  <RecepcionRoutes />
                </RecepcionRoute>
              } 
            />
            
            <Route 
              path="/cocina/*" 
              element={
                <CocinaRoute>
                  <CocinaRoutes />
                </CocinaRoute>
              } 
            />
            */}

            {/* Ruta catch-all para 404 */}
            <Route path="*" element={<NotFoundPage />} />
          </Routes>

          {/* Notificaciones Toast */}
          <ToastContainer
            position="top-right"
            autoClose={3000}
            hideProgressBar={false}
            newestOnTop={false}
            closeOnClick
            rtl={false}
            pauseOnFocusLoss
            draggable
            pauseOnHover
            theme="light"
            toastClassName="dominican-toast"
          />
        </div>
      </AuthProvider>
    </Router>
  );
};

export default App;

// ====================================
// PÁGINA 404 - NOT FOUND
// ====================================

const NotFoundPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-warm-beige flex items-center justify-center p-4">
      <div className="text-center">
        <div className="w-24 h-24 mx-auto mb-6 bg-dominican-red bg-opacity-10 rounded-full flex items-center justify-center">
          <span className="text-4xl">🔍</span>
        </div>

        <h1 className="text-4xl font-heading font-bold text-dominican-blue mb-4">
          Página No Encontrada
        </h1>

        <p className="text-stone-gray mb-6 max-w-md">
          La página que buscas no existe o ha sido movida. Regresa al dashboard para continuar
          navegando.
        </p>

        <button
          onClick={() => window.history.back()}
          className="px-6 py-3 bg-dominican-red text-white rounded-lg hover:bg-red-700 smooth-transition font-medium"
        >
          Regresar
        </button>
      </div>
    </div>
  );
};
