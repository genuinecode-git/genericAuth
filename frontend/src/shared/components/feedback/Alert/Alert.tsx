import React, { useCallback } from 'react';
import MuiAlert from '@mui/material/Alert';
import IconButton from '@mui/material/IconButton';
import CloseIcon from '@mui/icons-material/Close';
import { AlertProps } from './Alert.types';

/**
 * Enhanced Alert component with dismissible functionality
 */
export const Alert = React.memo<AlertProps>(
  ({
    dismissible = false,
    onDismiss,
    onClose,
    children,
    action,
    ...props
  }) => {
    const handleClose = useCallback(
      (event: React.SyntheticEvent) => {
        onClose?.(event);
        onDismiss?.();
      },
      [onClose, onDismiss]
    );

    const actionContent = dismissible && !action ? (
      <IconButton
        aria-label="close"
        color="inherit"
        size="small"
        onClick={handleClose}
      >
        <CloseIcon fontSize="inherit" />
      </IconButton>
    ) : action;

    return (
      <MuiAlert
        {...props}
        action={actionContent}
        onClose={dismissible || onClose ? handleClose : undefined}
      >
        {children}
      </MuiAlert>
    );
  }
);

Alert.displayName = 'Alert';
