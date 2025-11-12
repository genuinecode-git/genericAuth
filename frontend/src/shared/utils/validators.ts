import { ValidationResult } from '../types';

/**
 * Email validation regex pattern
 */
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Validates if a value is not empty
 */
export const validateRequired = (value: string, fieldName = 'This field'): ValidationResult => {
  const trimmedValue = value.trim();
  return {
    isValid: trimmedValue.length > 0,
    error: trimmedValue.length === 0 ? `${fieldName} is required` : undefined,
  };
};

/**
 * Validates email format
 */
export const validateEmail = (email: string): ValidationResult => {
  const trimmedEmail = email.trim();

  if (trimmedEmail.length === 0) {
    return {
      isValid: false,
      error: 'Email is required',
    };
  }

  const isValid = EMAIL_REGEX.test(trimmedEmail);
  return {
    isValid,
    error: isValid ? undefined : 'Please enter a valid email address',
  };
};

/**
 * Validates password strength
 */
export const validatePassword = (password: string, minLength = 8): ValidationResult => {
  if (password.length === 0) {
    return {
      isValid: false,
      error: 'Password is required',
    };
  }

  if (password.length < minLength) {
    return {
      isValid: false,
      error: `Password must be at least ${minLength} characters`,
    };
  }

  return {
    isValid: true,
  };
};

/**
 * Validates minimum length
 */
export const validateMinLength = (
  value: string,
  minLength: number,
  fieldName = 'This field'
): ValidationResult => {
  const isValid = value.length >= minLength;
  return {
    isValid,
    error: isValid ? undefined : `${fieldName} must be at least ${minLength} characters`,
  };
};

/**
 * Validates maximum length
 */
export const validateMaxLength = (
  value: string,
  maxLength: number,
  fieldName = 'This field'
): ValidationResult => {
  const isValid = value.length <= maxLength;
  return {
    isValid,
    error: isValid ? undefined : `${fieldName} must not exceed ${maxLength} characters`,
  };
};

/**
 * Validates that two values match (e.g., password confirmation)
 */
export const validateMatch = (
  value1: string,
  value2: string,
  fieldName = 'Values'
): ValidationResult => {
  const isValid = value1 === value2;
  return {
    isValid,
    error: isValid ? undefined : `${fieldName} do not match`,
  };
};

/**
 * Validates URL format
 */
export const validateUrl = (url: string): ValidationResult => {
  try {
    new URL(url);
    return { isValid: true };
  } catch {
    return {
      isValid: false,
      error: 'Please enter a valid URL',
    };
  }
};

/**
 * Combines multiple validation results
 */
export const combineValidations = (...results: ValidationResult[]): ValidationResult => {
  const firstError = results.find((result) => !result.isValid);
  return firstError || { isValid: true };
};

/**
 * Password strength calculator
 * Returns: weak, medium, strong
 */
export const calculatePasswordStrength = (password: string): 'weak' | 'medium' | 'strong' => {
  let strength = 0;

  if (password.length >= 8) strength++;
  if (password.length >= 12) strength++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
  if (/\d/.test(password)) strength++;
  if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

  if (strength <= 2) return 'weak';
  if (strength <= 4) return 'medium';
  return 'strong';
};
