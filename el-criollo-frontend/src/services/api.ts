import axios, { AxiosInstance, AxiosResponse, AxiosError } from 'axios';
import { toast } from 'react-toastify';

// Configuración base de la API
const API_BASE_URL = 'https://elcriolloapi.cjoga.cloud/api';
const REQUEST_TIMEOUT = 10000; // 10 segundos

// Crear instancia de Axios
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: REQUEST_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

// ====================================
// INTERCEPTOR DE REQUEST
// ====================================
apiClient.interceptors.request.use(
  (config) => {
    // Agregar token JWT si está disponible
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Log de requests en desarrollo
    if (import.meta.env.DEV) {
      console.log('🚀 API Request:', {
        method: config.method?.toUpperCase(),
        url: config.url,
        data: config.data,
      });
    }

    return config;
  },
  (error) => {
    console.error('❌ Request Error:', error);
    return Promise.reject(error);
  }
);

// ====================================
// INTERCEPTOR DE RESPONSE
// ====================================
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    // Log de responses exitosos en desarrollo
    if (import.meta.env.DEV) {
      console.log('✅ API Response:', {
        status: response.status,
        url: response.config.url,
        data: response.data,
      });
    }

    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as any;

    // Log de errores
    console.error('❌ API Error:', {
      status: error.response?.status,
      url: error.config?.url,
      message: error.message,
      data: error.response?.data,
    });

    // Manejar token expirado (401)
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
          const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
            refreshToken,
          });

          const { token, refreshToken: newRefreshToken } = response.data;

          // Actualizar tokens en localStorage
          localStorage.setItem('authToken', token);
          localStorage.setItem('refreshToken', newRefreshToken);

          // Reintentar request original con nuevo token
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        console.error('❌ Token refresh failed:', refreshError);

        // Limpiar tokens y redirigir al login
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');

        // Notificar al usuario
        toast.error('Sesión expirada. Por favor, inicia sesión nuevamente.');

        // Redirigir al login
        window.location.href = '/login';

        return Promise.reject(refreshError);
      }
    }

    // Manejar otros errores HTTP
    switch (error.response?.status) {
      case 400:
        toast.error('Datos inválidos. Verifica la información ingresada.');
        break;
      case 403:
        toast.error('No tienes permisos para realizar esta acción.');
        break;
      case 404:
        toast.error('Recurso no encontrado.');
        break;
      case 500:
        toast.error('Error interno del servidor. Intenta más tarde.');
        break;
      case 503:
        toast.error('Servicio no disponible. Intenta más tarde.');
        break;
      default:
        if (error.code === 'ECONNABORTED') {
          toast.error('Tiempo de espera agotado. Verifica tu conexión.');
        } else if (error.code === 'ERR_NETWORK') {
          toast.error('Error de conexión. Verifica tu internet.');
        } else {
          toast.error('Error inesperado. Intenta nuevamente.');
        }
    }

    return Promise.reject(error);
  }
);

// ====================================
// UTILIDADES DE API
// ====================================

// Función para obtener mensaje de error legible
export const getErrorMessage = (error: any): string => {
  if (error.response?.data?.message) {
    return error.response.data.message;
  }
  if (error.response?.data?.errors && Array.isArray(error.response.data.errors)) {
    return error.response.data.errors.join(', ');
  }
  if (error.message) {
    return error.message;
  }
  return 'Error inesperado';
};

// Función para validar si el token está expirado
export const isTokenExpired = (token: string): boolean => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const currentTime = Date.now() / 1000;
    return payload.exp < currentTime;
  } catch {
    return true;
  }
};

// Función para limpiar datos de autenticación
export const clearAuthData = (): void => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('user');
};

// Función para verificar si hay internet
export const checkInternetConnection = async (): Promise<boolean> => {
  try {
    await fetch('https://www.google.com/favicon.ico', {
      mode: 'no-cors',
      cache: 'no-cache',
    });
    return true;
  } catch {
    return false;
  }
};

// ====================================
// MÉTODOS HTTP WRAPPER
// ====================================

export const api = {
  // GET request
  get: async <T>(url: string, params?: object): Promise<T> => {
    const response = await apiClient.get(url, { params });
    return response.data;
  },

  // POST request
  post: async <T>(url: string, data?: object): Promise<T> => {
    const response = await apiClient.post(url, data);
    return response.data;
  },

  // PUT request
  put: async <T>(url: string, data?: object): Promise<T> => {
    const response = await apiClient.put(url, data);
    return response.data;
  },

  // PATCH request
  patch: async <T>(url: string, data?: object): Promise<T> => {
    const response = await apiClient.patch(url, data);
    return response.data;
  },

  // DELETE request
  delete: async <T>(url: string): Promise<T> => {
    const response = await apiClient.delete(url);
    return response.data;
  },
};

export default apiClient;
