/**
 * Authentication configuration
 */
export const authConfig = {
  /**
   * API base URL
   */
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',

  /**
   * Token storage key
   */
  tokenKey: import.meta.env.VITE_AUTH_TOKEN_KEY || 'auth_token',

  /**
   * Whether social login is enabled
   */
  enableSocialLogin: import.meta.env.VITE_ENABLE_SOCIAL_LOGIN === 'true',

  /**
   * API endpoints
   */
  endpoints: {
    login: '/auth/login',
    logout: '/auth/logout',
    refresh: '/auth/refresh',
    register: '/auth/register',
    forgotPassword: '/auth/forgot-password',
    resetPassword: '/auth/reset-password',
    verifyEmail: '/auth/verify-email',
  },

  /**
   * Validation rules
   */
  validation: {
    passwordMinLength: 8,
    passwordMaxLength: 128,
    emailMaxLength: 255,
  },

  /**
   * UI configuration
   */
  ui: {
    showPasswordStrength: true,
    showRememberMe: true,
  },
} as const;

export type AuthConfig = typeof authConfig;
