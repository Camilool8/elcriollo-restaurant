import { useForm as useReactHookForm, UseFormReturn, FieldValues } from 'react-hook-form';
import { useState } from 'react';

interface UseFormOptions<T extends FieldValues> {
  defaultValues?: Partial<T>;
  onSubmit: (data: T) => Promise<void> | void;
  onSuccess?: () => void;
  onError?: (error: any) => void;
}

export function useForm<T extends FieldValues>(
  options: UseFormOptions<T>
): UseFormReturn<T> & {
  isSubmitting: boolean;
  submitError: string | null;
  handleSubmit: any;
} {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const form = useReactHookForm<T>({
    defaultValues: options.defaultValues as any,
  });

  const handleSubmit = async (data: T) => {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      await options.onSubmit(data);
      options.onSuccess?.();
    } catch (error: any) {
      const errorMessage = error.message || 'Error en el formulario';
      setSubmitError(errorMessage);
      options.onError?.(error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    ...form,
    isSubmitting,
    submitError,
    handleSubmit: form.handleSubmit(handleSubmit) as any,
  };
}

export default useForm;
