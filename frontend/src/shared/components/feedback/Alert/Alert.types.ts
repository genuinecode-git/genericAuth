import { AlertProps as MuiAlertProps } from '@mui/material/Alert';

export interface AlertProps extends MuiAlertProps {
  /**
   * Whether the alert can be dismissed
   */
  dismissible?: boolean;
  /**
   * Callback when alert is dismissed
   */
  onDismiss?: () => void;
}
