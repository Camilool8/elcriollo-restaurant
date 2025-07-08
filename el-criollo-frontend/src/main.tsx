import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

// Estilos globales
import './styles/globals.css';
import './styles/components.css';

// Verificar que el elemento root existe
const rootElement = document.getElementById('root');
if (!rootElement) {
  throw new Error('Root element not found');
}

// Crear y renderizar la aplicación
ReactDOM.createRoot(rootElement).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
