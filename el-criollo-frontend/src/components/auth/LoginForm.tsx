import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { toast } from 'react-toastify';
import { Eye, EyeOff, User, Lock, Coffee } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { LoginRequest, UsuarioResponse } from '@/types';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import { authService } from '@/services/authService';

// ====================================
// TIPOS
// ====================================

interface LoginFormData {
  username: string;
  password: string;
  recordarSesion: boolean;
}

// ====================================
// COMPONENTE PRINCIPAL
// ====================================

const getRedirectPath = (user: UsuarioResponse | null): string => {
  if (!user) return '/login';
  if (user.rol === 'Administrador') {
    return '/admin';
  }
  return '/mesas';
};

const LoginForm: React.FC = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login, state } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // React Hook Form
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<LoginFormData>({
    defaultValues: {
      username: '',
      password: '',
      recordarSesion: false,
    },
  });

  // ====================================
  // EFECTOS
  // ====================================

  // Redirigir si ya está autenticado
  useEffect(() => {
    if (state.isAuthenticated && !state.isLoading) {
      const from = (location.state as any)?.from || getRedirectPath(state.user);
      navigate(from, { replace: true });
    }
  }, [state.isAuthenticated, state.isLoading, state.user, navigate, location]);

  // Auto-completar credenciales demo para desarrollo
  useEffect(() => {
    if (import.meta.env.DEV) {
      setValue('username', 'thecuevas0123_');
      setValue('password', 'thepikachu0123_');
    }
  }, [setValue]);

  // ====================================
  // HANDLERS
  // ====================================

  const onSubmit = async (data: LoginFormData) => {
    setIsSubmitting(true);

    try {
      const loginRequest: LoginRequest = {
        username: data.username.trim(),
        password: data.password,
        recordarSesion: data.recordarSesion,
      };

      const success = await login(loginRequest);

      if (success) {
        // La redirección se maneja en el useEffect de arriba
        // pero lo hacemos aquí también por si el estado no se actualiza a tiempo
        const redirectPath = getRedirectPath(authService.getStoredUser());
        navigate(redirectPath, { replace: true });
      }
    } catch (error: any) {
      console.error('Error en login:', error);
      toast.error('Error inesperado en el login');
    } finally {
      setIsSubmitting(false);
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const fillDemoCredentials = () => {
    setValue('username', 'thecuevas0123_');
    setValue('password', 'thepikachu0123_');
    toast.info('Credenciales demo cargadas');
  };

  // ====================================
  // RENDER
  // ====================================

  return (
    <div className="min-h-screen bg-gradient-to-br from-dominican-red via-dominican-blue to-dominican-red flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo y Título */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-white rounded-full mb-4 shadow-lg">
            <Coffee className="w-8 h-8 text-dominican-red" />
          </div>

          <h1 className="text-3xl font-heading font-bold text-white mb-2">El Criollo</h1>

          <p className="text-white text-opacity-90 text-lg">🇩🇴 Restaurante Dominicano</p>

          <p className="text-white text-opacity-75 text-sm mt-2">Sistema de Gestión POS</p>
        </div>

        {/* Formulario de Login */}
        <Card className="backdrop-blur-sm bg-white bg-opacity-95" padding="lg">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Título del formulario */}
            <div className="text-center">
              <h2 className="text-2xl font-heading font-semibold text-dominican-blue">
                Iniciar Sesión
              </h2>
              <p className="text-stone-gray mt-1">Ingresa tus credenciales para acceder</p>
            </div>

            {/* Campo Usuario */}
            <div>
              <Input
                {...register('username', {
                  required: 'El usuario es requerido',
                  minLength: {
                    value: 3,
                    message: 'El usuario debe tener al menos 3 caracteres',
                  },
                })}
                type="text"
                label="Usuario"
                placeholder="Ingresa tu usuario"
                leftIcon={<User className="w-5 h-5" />}
                error={errors.username?.message}
                fullWidth
                autoComplete="username"
                autoFocus
              />
            </div>

            {/* Campo Contraseña */}
            <div>
              <Input
                {...register('password', {
                  required: 'La contraseña es requerida',
                  minLength: {
                    value: 6,
                    message: 'La contraseña debe tener al menos 6 caracteres',
                  },
                })}
                type={showPassword ? 'text' : 'password'}
                label="Contraseña"
                placeholder="Ingresa tu contraseña"
                leftIcon={<Lock className="w-5 h-5" />}
                rightIcon={
                  <button
                    type="button"
                    onClick={togglePasswordVisibility}
                    className="text-gray-400 hover:text-gray-600 smooth-transition"
                  >
                    {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                  </button>
                }
                error={errors.password?.message}
                fullWidth
                autoComplete="current-password"
              />
            </div>

            {/* Recordar Sesión */}
            <div className="flex items-center">
              <input
                {...register('recordarSesion')}
                type="checkbox"
                id="recordarSesion"
                className="w-4 h-4 text-dominican-red bg-gray-100 border-gray-300 rounded focus:ring-dominican-red focus:ring-2"
              />
              <label htmlFor="recordarSesion" className="ml-2 text-sm text-gray-700">
                Recordar mi sesión
              </label>
            </div>

            {/* Mostrar error general */}
            {state.error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3">
                <p className="text-red-600 text-sm text-center">{state.error}</p>
              </div>
            )}

            {/* Botón de Login */}
            <Button
              type="submit"
              variant="primary"
              size="lg"
              fullWidth
              isLoading={isSubmitting || state.isLoading}
              disabled={isSubmitting || state.isLoading}
            >
              {isSubmitting || state.isLoading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
            </Button>

            {/* Demo credentials (solo en desarrollo) */}
            {import.meta.env.DEV && (
              <div className="border-t pt-4">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  fullWidth
                  onClick={fillDemoCredentials}
                >
                  🎓 Usar Credenciales Demo
                </Button>

                <div className="mt-2 text-xs text-center text-gray-500">
                  <p>Usuario: thecuevas0123_</p>
                  <p>Contraseña: thepikachu0123_</p>
                </div>
              </div>
            )}
          </form>
        </Card>

        {/* Información adicional */}
        <div className="text-center mt-6 text-white text-opacity-75 text-sm">
          <p>¿Problemas para acceder?</p>
          <p>Contacta al administrador del sistema</p>
        </div>

        {/* Footer con información del sistema */}
        <div className="text-center mt-8 text-white text-opacity-60 text-xs">
          <p>El Criollo POS v1.0</p>
          <p>Sistema de gestión para restaurantes dominicanos</p>
        </div>
      </div>
    </div>
  );
};

export default LoginForm;
