import { useState, useCallback, useMemo } from 'react';
import { useAsync } from '@shared/hooks';
import { LoginCredentials, LoginResponse } from '../types';
import { authService } from '../services/authService';
import { validateEmail, validatePassword } from '@shared/utils/validators';
import { authConfig } from '@config/auth.config';

interface FormErrors {
  email?: string;
  password?: string;
  general?: string;
}

interface FormTouched {
  email: boolean;
  password: boolean;
}

/**
 * Custom hook for managing login form state and validation
 */
export const useLoginForm = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [touched, setTouched] = useState<FormTouched>({
    email: false,
    password: false,
  });

  const loginAsync = useAsync<LoginResponse>();

  // Field validation
  const errors = useMemo<FormErrors>(() => {
    const newErrors: FormErrors = {};

    if (touched.email) {
      const emailValidation = validateEmail(email);
      if (!emailValidation.isValid) {
        newErrors.email = emailValidation.error;
      }
    }

    if (touched.password) {
      const passwordValidation = validatePassword(
        password,
        authConfig.validation.passwordMinLength
      );
      if (!passwordValidation.isValid) {
        newErrors.password = passwordValidation.error;
      }
    }

    return newErrors;
  }, [email, password, touched]);

  // Form validity check
  const isValid = useMemo(() => {
    const emailValidation = validateEmail(email);
    const passwordValidation = validatePassword(
      password,
      authConfig.validation.passwordMinLength
    );

    return emailValidation.isValid && passwordValidation.isValid;
  }, [email, password]);

  // Handle field blur
  const handleEmailBlur = useCallback(() => {
    setTouched((prev) => ({ ...prev, email: true }));
  }, []);

  const handlePasswordBlur = useCallback(() => {
    setTouched((prev) => ({ ...prev, password: true }));
  }, []);

  // Handle field changes
  const handleEmailChange = useCallback((value: string) => {
    setEmail(value);
  }, []);

  const handlePasswordChange = useCallback((value: string) => {
    setPassword(value);
  }, []);

  const handleRememberMeChange = useCallback((checked: boolean) => {
    setRememberMe(checked);
  }, []);

  // Handle form submission
  const handleSubmit = useCallback(async () => {
    // Mark all fields as touched
    setTouched({
      email: true,
      password: true,
    });

    // Validate all fields
    if (!isValid) {
      return;
    }

    const credentials: LoginCredentials = {
      email,
      password,
      rememberMe,
    };

    const result = await loginAsync.execute(() => authService.login(credentials));

    if (!result) {
      // Set general error from service
      return;
    }

    return result;
  }, [email, password, rememberMe, isValid, loginAsync]);

  // Reset form
  const resetForm = useCallback(() => {
    setEmail('');
    setPassword('');
    setRememberMe(false);
    setTouched({
      email: false,
      password: false,
    });
    loginAsync.reset();
  }, [loginAsync]);

  return {
    // Form values
    email,
    password,
    rememberMe,

    // Validation
    errors,
    touched,
    isValid,

    // Async state
    isLoading: loginAsync.isPending,
    isSuccess: loginAsync.isSuccess,
    isError: loginAsync.isError,
    data: loginAsync.data,
    error: loginAsync.error,

    // Handlers
    handleEmailChange,
    handlePasswordChange,
    handleRememberMeChange,
    handleEmailBlur,
    handlePasswordBlur,
    handleSubmit,
    resetForm,
  };
};

export type UseLoginFormReturn = ReturnType<typeof useLoginForm>;
