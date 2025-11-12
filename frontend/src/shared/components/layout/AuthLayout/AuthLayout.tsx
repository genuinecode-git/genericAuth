import React from 'react';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import { AuthLayoutProps } from './AuthLayout.types';

/**
 * Full-page centered layout for authentication pages
 */
export const AuthLayout = React.memo<AuthLayoutProps>(
  ({
    children,
    background,
    logo,
    showLogo = true,
    sx,
  }) => {
    const backgroundStyle = background
      ? background.startsWith('linear-gradient') || background.startsWith('radial-gradient')
        ? { background }
        : { backgroundImage: `url(${background})`, backgroundSize: 'cover', backgroundPosition: 'center' }
      : { backgroundColor: 'background.default' };

    return (
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          py: { xs: 4, sm: 6 },
          px: { xs: 2, sm: 3 },
          ...backgroundStyle,
          ...sx,
        }}
      >
        <Container
          maxWidth="sm"
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 4,
          }}
        >
          {showLogo && logo && (
            <Box
              sx={{
                mb: 2,
                display: 'flex',
                justifyContent: 'center',
              }}
            >
              {typeof logo === 'string' ? (
                <Box
                  component="img"
                  src={logo}
                  alt="Logo"
                  sx={{
                    maxWidth: { xs: 120, sm: 160 },
                    height: 'auto',
                  }}
                />
              ) : (
                logo
              )}
            </Box>
          )}
          {children}
        </Container>
      </Box>
    );
  }
);

AuthLayout.displayName = 'AuthLayout';
