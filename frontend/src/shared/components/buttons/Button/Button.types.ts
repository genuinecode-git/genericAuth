import { ButtonProps as MuiButtonProps } from '@mui/material/Button';

export interface ButtonProps extends MuiButtonProps {
  /**
   * Loading state - shows spinner and disables button
   */
  loading?: boolean;
  /**
   * Custom loading text to display when loading
   */
  loadingText?: string;
}
