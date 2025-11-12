import { TextFieldProps as MuiTextFieldProps } from '@mui/material/TextField';

export interface TextFieldProps extends Omit<MuiTextFieldProps, 'variant'> {
  /**
   * Maximum character count for the field
   */
  maxLength?: number;
  /**
   * Whether to show character count below the field
   */
  showCharCount?: boolean;
  /**
   * Custom validation function
   */
  validate?: (value: string) => string | undefined;
  /**
   * Whether to validate on blur
   */
  validateOnBlur?: boolean;
}
