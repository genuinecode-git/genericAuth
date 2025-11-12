import { CircularProgressProps } from '@mui/material/CircularProgress';

export interface LoadingProps extends CircularProgressProps {
  /**
   * Loading message to display below spinner
   */
  message?: string;
  /**
   * Whether to center the loading indicator
   */
  centered?: boolean;
  /**
   * Whether to show as overlay (full screen)
   */
  overlay?: boolean;
}
