import React from 'react';
import MuiButton from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import { ButtonProps } from './Button.types';

/**
 * Enhanced Button component with loading state
 */
export const Button = React.memo<ButtonProps>(
  ({
    loading = false,
    loadingText,
    disabled,
    children,
    startIcon,
    endIcon,
    ...props
  }) => {
    return (
      <MuiButton
        {...props}
        disabled={disabled || loading}
        startIcon={loading ? undefined : startIcon}
        endIcon={loading ? undefined : endIcon}
      >
        {loading ? (
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
            }}
          >
            <CircularProgress
              size={20}
              sx={{
                color: 'inherit',
              }}
            />
            {loadingText || children}
          </Box>
        ) : (
          children
        )}
      </MuiButton>
    );
  }
);

Button.displayName = 'Button';
