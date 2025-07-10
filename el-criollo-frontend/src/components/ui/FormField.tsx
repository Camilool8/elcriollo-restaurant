import React, { forwardRef, ReactNode } from 'react';
import { Input } from './Input';
import {
  validarCedulaDominicana,
  validarTelefonoDominicano,
  formatearCedula,
  formatearTelefono,
} from '@/utils/dominicanValidations';

interface FormFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  helperText?: string;
  required?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  validation?: 'cedula' | 'telefono' | 'email' | 'precio';
  autoFormat?: boolean;
  fullWidth?: boolean;
}

export const FormField = forwardRef<HTMLInputElement, FormFieldProps>(
  (
    {
      label,
      error,
      helperText,
      required = false,
      validation,
      autoFormat = false,
      fullWidth,
      onChange,
      ...props
    },
    ref
  ) => {
    const [internalError, setInternalError] = React.useState<string | null>(null);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      let value = e.target.value;

      // Auto-formateo
      if (autoFormat) {
        switch (validation) {
          case 'cedula':
            value = formatearCedula(value);
            break;
          case 'telefono':
            value = formatearTelefono(value);
            break;
        }
      }

      // Validación en tiempo real
      if (validation && value) {
        let validationError: string | null = null;

        switch (validation) {
          case 'cedula':
            if (!validarCedulaDominicana(value)) {
              validationError = 'Formato de cédula inválido (123-1234567-1)';
            }
            break;
          case 'telefono':
            if (!validarTelefonoDominicano(value)) {
              validationError = 'Formato de teléfono inválido (809-123-4567)';
            }
            break;
          case 'email':
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
              validationError = 'Formato de email inválido';
            }
            break;
          case 'precio':
            const precio = parseFloat(value);
            if (isNaN(precio) || precio <= 0) {
              validationError = 'Debe ser un precio válido mayor a 0';
            }
            break;
        }

        setInternalError(validationError);
      } else {
        setInternalError(null);
      }

      // Llamar onChange original
      const modifiedEvent = {
        ...e,
        target: { ...e.target, value },
      };
      onChange?.(modifiedEvent);
    };

    const displayError = error || internalError;

    return (
      <Input
        ref={ref}
        label={required ? `${label} *` : label}
        error={displayError || undefined}
        helperText={!displayError ? helperText : undefined}
        fullWidth={fullWidth}
        onChange={handleChange}
        {...props}
      />
    );
  }
);

FormField.displayName = 'FormField';

export default FormField;
