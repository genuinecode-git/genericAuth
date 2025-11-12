import { TextFieldProps } from '../TextField/TextField.types';

export interface PasswordFieldProps extends Omit<TextFieldProps, 'type'> {
  /**
   * Whether to show password strength indicator
   */
  showStrengthIndicator?: boolean;
  /**
   * Custom strength calculation function
   */
  calculateStrength?: (password: string) => 'weak' | 'medium' | 'strong';
}

export type PasswordStrength = 'weak' | 'medium' | 'strong';
