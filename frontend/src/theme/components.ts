import { Components, Theme } from '@mui/material/styles';

export const components: Components<Omit<Theme, 'components'>> = {
  MuiButton: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        textTransform: 'none',
        fontWeight: 500,
        boxShadow: 'none',
        '&:hover': {
          boxShadow: 'none',
        },
      },
      sizeMedium: {
        height: 48,
        padding: '12px 24px',
      },
      sizeLarge: {
        height: 56,
        padding: '16px 32px',
        fontSize: '1rem',
      },
      contained: {
        '&:hover': {
          boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.1)',
        },
      },
    },
    defaultProps: {
      disableElevation: true,
    },
  },
  MuiTextField: {
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          borderRadius: 8,
        },
      },
    },
    defaultProps: {
      variant: 'outlined',
      fullWidth: true,
    },
  },
  MuiOutlinedInput: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
          borderWidth: 2,
        },
      },
      input: {
        height: '56px',
        padding: '0 14px',
        boxSizing: 'border-box',
      },
    },
  },
  MuiInputLabel: {
    styleOverrides: {
      root: {
        '&.Mui-focused': {
          fontWeight: 500,
        },
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        boxShadow: '0px 4px 12px rgba(0, 0, 0, 0.1)',
      },
    },
    defaultProps: {
      elevation: 3,
    },
  },
  MuiAlert: {
    styleOverrides: {
      root: {
        borderRadius: 8,
      },
      standardError: {
        backgroundColor: '#ffebee',
        color: '#c62828',
      },
      standardWarning: {
        backgroundColor: '#fff3e0',
        color: '#e65100',
      },
      standardInfo: {
        backgroundColor: '#e3f2fd',
        color: '#01579b',
      },
      standardSuccess: {
        backgroundColor: '#e8f5e9',
        color: '#1b5e20',
      },
    },
  },
  MuiCircularProgress: {
    styleOverrides: {
      root: {
        animationDuration: '1.4s',
      },
    },
  },
  MuiIconButton: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        '&:hover': {
          backgroundColor: 'rgba(0, 0, 0, 0.04)',
        },
      },
    },
  },
  MuiDivider: {
    styleOverrides: {
      root: {
        borderColor: 'rgba(0, 0, 0, 0.12)',
      },
    },
  },
};
