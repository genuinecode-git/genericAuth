import React, { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import Typography from '@mui/material/Typography';
import Link from '@mui/material/Link';
import Box from '@mui/material/Box';
import { LoginForm } from '../components/LoginForm';
import { FeaturePanel } from '../components/FeaturePanel';
import { LoginResponse } from '../types';

/**
 * Login page with split-screen layout
 */
export const LoginPage: React.FC = () => {
  const navigate = useNavigate();

  const handleLoginSuccess = useCallback(
    (data: LoginResponse) => {
      console.log('Login successful:', data);
      // Navigate to dashboard or home page
      navigate('/dashboard');
    },
    [navigate]
  );

  const handleLoginError = useCallback((error: Error) => {
    console.error('Login failed:', error);
    // Error is already displayed in the form
  }, []);

  const handleForgotPassword = useCallback(() => {
    navigate('/forgot-password');
  }, [navigate]);

  const handleSignUpClick = useCallback(
    (event: React.MouseEvent) => {
      event.preventDefault();
      navigate('/register');
    },
    [navigate]
  );

  return (
    <Box
      sx={{
        display: 'flex',
        minHeight: '100vh',
        flexDirection: { xs: 'column', md: 'row' },
      }}
    >
      {/* Left Panel - Purple Feature Section */}
      <Box
        sx={{
          width: { xs: '100%', md: '45%' },
          display: { xs: 'none', md: 'block' },
        }}
      >
        <FeaturePanel />
      </Box>

      {/* Right Panel - White Login Form Section */}
      <Box
        sx={{
          width: { xs: '100%', md: '55%' },
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#FFFFFF',
          p: { xs: 3, sm: 4, md: 6 },
        }}
      >
        <Box
          sx={{
            width: '100%',
            maxWidth: 400,
          }}
        >
          {/* Welcome Heading */}
          <Typography
            variant="h4"
            sx={{
              fontWeight: 700,
              fontSize: '28px',
              color: '#333333',
              mb: 3,
            }}
          >
            Welcome back
          </Typography>

          {/* Login Form */}
          <LoginForm
            onSuccess={handleLoginSuccess}
            onError={handleLoginError}
            onForgotPassword={handleForgotPassword}
          />

          {/* Sign Up Link */}
          <Box
            sx={{
              display: 'flex',
              gap: 1,
              justifyContent: 'center',
              mt: 3,
            }}
          >
            <Typography variant="body2" color="text.secondary">
              Don't have an account?
            </Typography>
            <Link
              href="#"
              variant="body2"
              onClick={handleSignUpClick}
              sx={{
                textDecoration: 'none',
                color: 'primary.main',
                fontWeight: 600,
                '&:hover': {
                  textDecoration: 'underline',
                },
              }}
            >
              Sign up
            </Link>
          </Box>
        </Box>
      </Box>
    </Box>
  );
};
