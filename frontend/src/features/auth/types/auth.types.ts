/**
 * User information
 */
export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  emailVerified: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * Login credentials
 */
export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

/**
 * Login response from API
 */
export interface LoginResponse {
  user: User;
  token: string;
  refreshToken?: string;
  expiresIn: number;
}

/**
 * Registration data
 */
export interface RegisterData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName?: string;
  lastName?: string;
}

/**
 * Password reset request
 */
export interface ForgotPasswordRequest {
  email: string;
}

/**
 * Password reset data
 */
export interface ResetPasswordData {
  token: string;
  password: string;
  confirmPassword: string;
}

/**
 * Social login providers
 */
export type SocialProvider = 'google' | 'facebook' | 'github' | 'microsoft';

/**
 * Social login request
 */
export interface SocialLoginRequest {
  provider: SocialProvider;
  token: string;
}

/**
 * Authentication state
 */
export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
  loading: boolean;
  error: string | null;
}
