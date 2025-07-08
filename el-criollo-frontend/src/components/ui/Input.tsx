import { InputHTMLAttributes, forwardRef, ReactNode } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  fullWidth?: boolean;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  (
    { label, error, helperText, leftIcon, rightIcon, fullWidth = false, className = '', ...props },
    ref
  ) => {
    const hasError = Boolean(error);

    const inputClasses = `
    w-full px-3 py-2 border rounded-lg 
    focus:outline-none focus:ring-2 focus:ring-offset-1
    smooth-transition
    ${
      hasError
        ? 'border-red-500 focus:ring-red-500 focus:border-red-500'
        : 'border-gray-300 focus:ring-dominican-blue focus:border-dominican-blue'
    }
    ${leftIcon ? 'pl-10' : ''}
    ${rightIcon ? 'pr-10' : ''}
    ${props.disabled ? 'bg-gray-100 cursor-not-allowed' : 'bg-white'}
  `;

    return (
      <div className={fullWidth ? 'w-full' : ''}>
        {label && <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>}

        <div className="relative">
          {leftIcon && (
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <span className="text-gray-400">{leftIcon}</span>
            </div>
          )}

          <input ref={ref} className={`${inputClasses} ${className}`} {...props} />

          {rightIcon && (
            <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
              <span className="text-gray-400">{rightIcon}</span>
            </div>
          )}
        </div>

        {error && <p className="mt-1 text-sm text-red-600">{error}</p>}

        {helperText && !error && <p className="mt-1 text-sm text-gray-500">{helperText}</p>}
      </div>
    );
  }
);

Input.displayName = 'Input';

export { Input };
