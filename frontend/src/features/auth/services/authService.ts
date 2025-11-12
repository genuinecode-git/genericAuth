import axios, { AxiosError, AxiosInstance } from 'axios';
import { authConfig } from '@config/auth.config';
import {
  LoginCredentials,
  LoginResponse,
  RegisterData,
  ForgotPasswordRequest,
  ResetPasswordData,
  SocialLoginRequest,
} from '../types';
import { AuthServiceError, AuthServiceResponse } from './authService.types';
import { AUTH_ERROR_MESSAGES } from '../utils/constants';

/**
 * Authentication service for API communication
 */
class AuthService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: authConfig.apiBaseUrl,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  /**
   * Setup axios interceptors for request/response handling
   */
  private setupInterceptors(): void {
    // Request interceptor - add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = this.getToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - handle errors
    this.api.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        if (error.response?.status === 401) {
          this.clearToken();
          // Optionally redirect to login
        }
        return Promise.reject(error);
      }
    );
  }

  /**
   * Handle API errors and convert to service error format
   */
  private handleError(error: unknown): AuthServiceError {
    if (axios.isAxiosError(error)) {
      const axiosError = error as AxiosError<any>;

      if (axiosError.response) {
        // Server responded with error
        return {
          message: axiosError.response.data?.message || AUTH_ERROR_MESSAGES.SERVER_ERROR,
          statusCode: axiosError.response.status,
          errors: axiosError.response.data?.errors,
          code: axiosError.response.data?.code,
        };
      } else if (axiosError.request) {
        // Request made but no response
        return {
          message: AUTH_ERROR_MESSAGES.NETWORK_ERROR,
          statusCode: 0,
        };
      }
    }

    // Unknown error
    return {
      message: AUTH_ERROR_MESSAGES.UNKNOWN_ERROR,
    };
  }

  /**
   * Store authentication token
   */
  private storeToken(token: string): void {
    localStorage.setItem(authConfig.tokenKey, token);
  }

  /**
   * Get authentication token
   */
  private getToken(): string | null {
    return localStorage.getItem(authConfig.tokenKey);
  }

  /**
   * Clear authentication token
   */
  private clearToken(): void {
    localStorage.removeItem(authConfig.tokenKey);
  }

  /**
   * Login with email and password
   */
  async login(credentials: LoginCredentials): Promise<AuthServiceResponse<LoginResponse>> {
    try {
      const response = await this.api.post<LoginResponse>(
        authConfig.endpoints.login,
        credentials
      );

      if (response.data.token) {
        this.storeToken(response.data.token);
      }

      return {
        success: true,
        data: response.data,
      };
    } catch (error) {
      return {
        success: false,
        error: this.handleError(error),
      };
    }
  }

  /**
   * Register new user
   */
  async register(data: RegisterData): Promise<AuthServiceResponse<LoginResponse>> {
    try {
      const response = await this.api.post<LoginResponse>(
        authConfig.endpoints.register,
        data
      );

      if (response.data.token) {
        this.storeToken(response.data.token);
      }

      return {
        success: true,
        data: response.data,
      };
    } catch (error) {
      return {
        success: false,
        error: this.handleError(error),
      };
    }
  }

  /**
   * Logout user
   */
  async logout(): Promise<AuthServiceResponse<void>> {
    try {
      await this.api.post(authConfig.endpoints.logout);
      this.clearToken();

      return {
        success: true,
      };
    } catch (error) {
      // Clear token even if logout fails
      this.clearToken();

      return {
        success: false,
        error: this.handleError(error),
      };
    }
  }

  /**
   * Request password reset
   */
  async forgotPassword(
    data: ForgotPasswordRequest
  ): Promise<AuthServiceResponse<void>> {
    try {
      await this.api.post(authConfig.endpoints.forgotPassword, data);

      return {
        success: true,
      };
    } catch (error) {
      return {
        success: false,
        error: this.handleError(error),
      };
    }
  }

  /**
   * Reset password with token
   */
  async resetPassword(
    data: ResetPasswordData
  ): Promise<AuthServiceResponse<void>> {
    try {
      await this.api.post(authConfig.endpoints.resetPassword, data);

      return {
        success: true,
      };
    } catch (error) {
      return {
        success: false,
        error: this.handleError(error),
      };
    }
  }

  /**
   * Social login
   */
  async socialLogin(
    data: SocialLoginRequest
  ): Promise<AuthServiceResponse<LoginResponse>> {
    try {
      const response = await this.api.post<LoginResponse>(
        `${authConfig.endpoints.login}/social`,
        data
      );

      if (response.data.token) {
        this.storeToken(response.data.token);
      }

      return {
        success: true,
        data: response.data,
      };
    } catch (error) {
      return {
        success: false,
        error: this.handleError(error),
      };
    }
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  /**
   * Get current auth token
   */
  getCurrentToken(): string | null {
    return this.getToken();
  }
}

// Export singleton instance
export const authService = new AuthService();
