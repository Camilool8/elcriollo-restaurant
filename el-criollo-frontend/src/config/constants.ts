export const APP_CONFIG = {
  name: 'El Criollo',
  version: '2.0.0',
  description: 'Sistema POS para Restaurante Dominicano',
  author: 'El Criollo Team',

  // URLs
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'https://elcriolloapi.cjoga.cloud/api',

  // Timeouts
  requestTimeout: parseInt(import.meta.env.VITE_REQUEST_TIMEOUT) || 10000,
  tokenRefreshBuffer: parseInt(import.meta.env.VITE_TOKEN_REFRESH_BUFFER) || 60000,

  // UI
  toastDuration: parseInt(import.meta.env.VITE_TOAST_DURATION) || 3000,
  defaultPageSize: parseInt(import.meta.env.VITE_DEFAULT_PAGE_SIZE) || 10,

  // Configuración dominicana
  currency: import.meta.env.VITE_CURRENCY_SYMBOL || 'RD$',
  itbisRate: parseFloat(import.meta.env.VITE_ITBIS_RATE) || 0.18,
  countryCode: import.meta.env.VITE_COUNTRY_CODE || 'DO',
  timezone: import.meta.env.VITE_TIMEZONE || 'America/Santo_Domingo',

  // Features flags
  enableDebug: import.meta.env.VITE_DEBUG_MODE === 'true',
  enableDevtools: import.meta.env.VITE_ENABLE_DEVTOOLS === 'true',
  enableDemoCredentials: import.meta.env.VITE_ENABLE_DEMO_CREDENTIALS === 'true',
} as const;

// Roles del sistema
export const ROLES = {
  ADMINISTRADOR: 'Administrador',
  CAJERO: 'Cajero',
  MESERO: 'Mesero',
  RECEPCION: 'Recepcion',
  COCINA: 'Cocina',
} as const;

// Estados del sistema
export const ESTADOS = {
  ACTIVO: 'Activo',
  INACTIVO: 'Inactivo',
  PENDIENTE: 'Pendiente',
  COMPLETADO: 'Completado',
  CANCELADO: 'Cancelado',
} as const;

// Rutas principales
export const ROUTES = {
  LOGIN: '/login',
  DASHBOARD: '/dashboard',
  ADMIN: '/admin',
  ADMIN_USUARIOS: '/admin/usuarios',
  ADMIN_CLIENTES: '/admin/clientes',
  ADMIN_EMPLEADOS: '/admin/empleados',
  ADMIN_PRODUCTOS: '/admin/productos',
  ADMIN_REPORTES: '/admin/reportes',
  ADMIN_CONFIGURACION: '/admin/configuracion',
  ADMIN_MESAS: '/admin/mesas',
  MESAS: '/mesas',
  MESAS_GESTION: '/mesas/gestion',
} as const;

// Configuración de desarrollo
export const DEV_CONFIG = {
  demoCredentials: {
    username: 'thecuevas0123_',
    password: 'thepikachu0123_',
  },
  mockDataEnabled: APP_CONFIG.enableDebug,
  verboseLogging: APP_CONFIG.enableDebug,
} as const;

// ====================================
// src/config/theme.ts - Configuración del tema
// ====================================

// Tema dominicano
export const DOMINICAN_THEME = {
  colors: {
    primary: '#CF142B', // Rojo dominicano
    secondary: '#002D62', // Azul dominicano
    accent: '#FFD700', // Dorado caribeño
    success: '#228B22', // Verde palmera
    warning: '#FF6B35', // Naranja atardecer
    error: '#EF4444', // Rojo error
    info: '#3B82F6', // Azul información

    // Neutros
    gray: {
      50: '#F9FAFB',
      100: '#F3F4F6',
      200: '#E5E7EB',
      300: '#D1D5DB',
      400: '#9CA3AF',
      500: '#6B7280',
      600: '#4B5563',
      700: '#374151',
      800: '#1F2937',
      900: '#111827',
    },

    // Fondos
    background: '#F5F5DC', // Beige cálido
    surface: '#FFFFFF', // Blanco
    surfaceAlt: '#F8F9FA', // Gris muy claro
  },

  // Tipografía
  fonts: {
    sans: ['Inter', 'system-ui', 'sans-serif'],
    heading: ['Poppins', 'system-ui', 'sans-serif'],
    mono: ['JetBrains Mono', 'monospace'],
  },

  // Espaciado
  spacing: {
    xs: '0.25rem',
    sm: '0.5rem',
    md: '1rem',
    lg: '1.5rem',
    xl: '2rem',
    '2xl': '3rem',
  },

  // Bordes
  borderRadius: {
    sm: '0.25rem',
    md: '0.5rem',
    lg: '0.75rem',
    xl: '1rem',
  },

  // Sombras
  shadows: {
    sm: '0 1px 2px rgba(0, 0, 0, 0.05)',
    md: '0 4px 6px rgba(0, 0, 0, 0.1)',
    lg: '0 10px 15px rgba(0, 0, 0, 0.1)',
    caribbean: '0 4px 6px rgba(207, 20, 43, 0.1)',
  },
} as const;

export default DOMINICAN_THEME;
