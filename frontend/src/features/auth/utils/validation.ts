import { validateEmail, validatePassword } from '@shared/utils/validators';
import { authConfig } from '@config/auth.config';
import { AUTH_ERROR_MESSAGES } from './constants';

/**
 * Validate login form credentials
 */
export const validateLoginCredentials = (email: string, password: string) => {
  const errors: Record<string, string> = {};

  // Validate email
  const emailValidation = validateEmail(email);
  if (!emailValidation.isValid) {
    errors.email = emailValidation.error || AUTH_ERROR_MESSAGES.EMAIL_INVALID;
  }

  // Validate password
  const passwordValidation = validatePassword(
    password,
    authConfig.validation.passwordMinLength
  );
  if (!passwordValidation.isValid) {
    errors.password = passwordValidation.error || AUTH_ERROR_MESSAGES.PASSWORD_REQUIRED;
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};

/**
 * Validate registration form data
 */
export const validateRegistrationData = (
  email: string,
  password: string,
  confirmPassword: string
) => {
  const errors: Record<string, string> = {};

  // Validate email
  const emailValidation = validateEmail(email);
  if (!emailValidation.isValid) {
    errors.email = emailValidation.error || AUTH_ERROR_MESSAGES.EMAIL_INVALID;
  }

  // Validate password
  const passwordValidation = validatePassword(
    password,
    authConfig.validation.passwordMinLength
  );
  if (!passwordValidation.isValid) {
    errors.password = passwordValidation.error || AUTH_ERROR_MESSAGES.PASSWORD_TOO_SHORT;
  }

  // Validate password confirmation
  if (password !== confirmPassword) {
    errors.confirmPassword = 'Passwords do not match';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};
