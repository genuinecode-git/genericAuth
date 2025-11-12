/**
 * Base validation result for form fields
 */
export interface ValidationResult {
  isValid: boolean;
  error?: string;
}

/**
 * Generic form field state
 */
export interface FormFieldState<T = string> {
  value: T;
  error?: string;
  touched: boolean;
}

/**
 * Async operation states
 */
export type AsyncStatus = 'idle' | 'pending' | 'success' | 'error';

/**
 * Async state management
 */
export interface AsyncState<T = unknown, E = Error> {
  status: AsyncStatus;
  data?: T;
  error?: E;
}

/**
 * API error response structure
 */
export interface ApiError {
  message: string;
  statusCode?: number;
  errors?: Record<string, string[]>;
}

/**
 * Generic API response wrapper
 */
export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  error?: ApiError;
}

/**
 * Pagination parameters
 */
export interface PaginationParams {
  page: number;
  pageSize: number;
}

/**
 * Sort parameters
 */
export interface SortParams {
  field: string;
  direction: 'asc' | 'desc';
}
