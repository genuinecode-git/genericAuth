import { LoginResponse } from '../../types';

export interface LoginFormProps {
  /**
   * Callback when login is successful
   */
  onSuccess?: (data: LoginResponse) => void;
  /**
   * Callback when login fails
   */
  onError?: (error: Error) => void;
  /**
   * Whether to show social login buttons
   */
  showSocialLogin?: boolean;
  /**
   * Whether to show remember me checkbox
   */
  showRememberMe?: boolean;
  /**
   * Whether to show forgot password link
   */
  showForgotPassword?: boolean;
  /**
   * Callback when forgot password is clicked
   */
  onForgotPassword?: () => void;
}
