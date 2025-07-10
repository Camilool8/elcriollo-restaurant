import { toast } from 'react-toastify';
import { authService } from '@/services/authService';

// ====================================
// FUNCIÓN PARA VERIFICAR SI ES ADMIN
// ====================================
const isUserAdmin = (): boolean => {
  try {
    const user = authService.getStoredUser();
    return user?.rol === 'Administrador';
  } catch {
    return false;
  }
};

// ====================================
// FUNCIÓN PARA MOSTRAR TOAST DE ERROR CONDICIONAL
// ====================================
export const showErrorToast = (message: string): void => {
  // Solo mostrar toast de error si el usuario es administrador
  if (isUserAdmin()) {
    toast.error(message);
  }
};

// ====================================
// FUNCIÓN PARA MOSTRAR TOAST DE ERROR SIEMPRE
// ====================================
export const showErrorToastAlways = (message: string): void => {
  // Mostrar toast de error sin importar el rol (para casos críticos)
  toast.error(message);
};

// ====================================
// FUNCIÓN PARA MOSTRAR TOAST DE ÉXITO
// ====================================
export const showSuccessToast = (message: string): void => {
  // Los toasts de éxito se muestran a todos los usuarios
  toast.success(message);
};

// ====================================
// FUNCIÓN PARA MOSTRAR TOAST DE INFO
// ====================================
export const showInfoToast = (message: string): void => {
  // Los toasts de info se muestran a todos los usuarios
  toast.info(message);
};

// ====================================
// FUNCIÓN PARA MOSTRAR TOAST DE WARNING
// ====================================
export const showWarningToast = (message: string): void => {
  // Los toasts de warning se muestran a todos los usuarios
  toast.warning(message);
};
