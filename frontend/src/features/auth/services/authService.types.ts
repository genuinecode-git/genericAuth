import { ApiError } from '@shared/types';

/**
 * Auth service error with additional context
 */
export interface AuthServiceError extends ApiError {
  code?: string;
  field?: string;
}

/**
 * Auth service response wrapper
 */
export interface AuthServiceResponse<T> {
  success: boolean;
  data?: T;
  error?: AuthServiceError;
}
