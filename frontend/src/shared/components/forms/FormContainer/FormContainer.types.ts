import { ReactNode } from 'react';
import { SxProps, Theme } from '@mui/material/styles';

export interface FormContainerProps {
  /**
   * Form title
   */
  title?: string;
  /**
   * Form subtitle or description
   */
  subtitle?: string;
  /**
   * Form content
   */
  children: ReactNode;
  /**
   * Form submission handler
   */
  onSubmit?: (event: React.FormEvent<HTMLFormElement>) => void;
  /**
   * Maximum width of the form container
   */
  maxWidth?: number | string;
  /**
   * Card elevation
   */
  elevation?: number;
  /**
   * Additional sx styles
   */
  sx?: SxProps<Theme>;
  /**
   * Footer content (e.g., links, additional buttons)
   */
  footer?: ReactNode;
}
