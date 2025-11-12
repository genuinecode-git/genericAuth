import { ReactNode } from 'react';
import { SxProps, Theme } from '@mui/material/styles';

export interface AuthLayoutProps {
  /**
   * Content to display in the layout
   */
  children: ReactNode;
  /**
   * Background image URL or gradient
   */
  background?: string;
  /**
   * Logo component or image URL
   */
  logo?: ReactNode | string;
  /**
   * Whether to show the logo
   */
  showLogo?: boolean;
  /**
   * Additional sx styles for the container
   */
  sx?: SxProps<Theme>;
}
