import React, { useCallback, useEffect } from 'react';
import Box from '@mui/material/Box';
import Checkbox from '@mui/material/Checkbox';
import FormControlLabel from '@mui/material/FormControlLabel';
import Link from '@mui/material/Link';
import Typography from '@mui/material/Typography';
import { TextField } from '@shared/components/forms/TextField';
import { PasswordField } from '@shared/components/forms/PasswordField';
import { Button } from '@shared/components/buttons';
import { Alert } from '@shared/components/feedback';
import { Stack } from '@shared/components/layout';
import { useLoginForm } from '../../hooks/useLoginForm';
import { authConfig } from '@config/auth.config';
import { SocialLoginButtons } from '../SocialLoginButtons';
import { SocialProvider } from '../../types';
import { LoginFormProps } from './LoginForm.types';

/**
 * Login form component with validation and error handling
 */
export const LoginForm = React.memo<LoginFormProps>(
  ({
    onSuccess,
    onError,
    showSocialLogin = authConfig.enableSocialLogin,
    showRememberMe = authConfig.ui.showRememberMe,
    showForgotPassword = true,
    onForgotPassword,
  }) => {
    const {
      email,
      password,
      rememberMe,
      errors,
      isLoading,
      isSuccess,
      isError,
      data,
      error,
      handleEmailChange,
      handlePasswordChange,
      handleRememberMeChange,
      handleEmailBlur,
      handlePasswordBlur,
      handleSubmit,
    } = useLoginForm();

    // Handle successful login
    useEffect(() => {
      if (isSuccess && data) {
        onSuccess?.(data);
      }
    }, [isSuccess, data, onSuccess]);

    // Handle login error
    useEffect(() => {
      if (isError && error) {
        onError?.(error as Error);
      }
    }, [isError, error, onError]);

    const handleFormSubmit = useCallback(
      async (event: React.FormEvent) => {
        event.preventDefault();
        await handleSubmit();
      },
      [handleSubmit]
    );

    const handleSocialLogin = useCallback((provider: SocialProvider) => {
      // TODO: Implement social login flow
      console.log('Social login with:', provider);
    }, []);

    const handleForgotPasswordClick = useCallback(
      (event: React.MouseEvent) => {
        event.preventDefault();
        onForgotPassword?.();
      },
      [onForgotPassword]
    );

    return (
      <Box
        component="form"
        onSubmit={handleFormSubmit}
        noValidate
        sx={{ width: '100%' }}
      >
        <Stack spacing={3}>
          {/* Error Alert */}
          {isError && error && (
            <Alert severity="error" dismissible>
              {(error as any)?.message || 'An error occurred during login'}
            </Alert>
          )}

          {/* Social Login - Moved to top */}
          {showSocialLogin && (
            <SocialLoginButtons
              onSocialLogin={handleSocialLogin}
              loading={isLoading}
            />
          )}

          {/* Email Field */}
          <TextField
            id="email"
            name="email"
            label="Email address"
            placeholder="Enter your email"
            type="email"
            autoComplete="email"
            autoFocus
            required
            value={email}
            onChange={(e) => handleEmailChange(e.target.value)}
            onBlur={handleEmailBlur}
            error={!!errors.email}
            helperText={errors.email}
            disabled={isLoading}
            sx={{
              '& .MuiOutlinedInput-root': {
                borderRadius: '8px',
              },
            }}
          />

          {/* Password Field */}
          <PasswordField
            id="password"
            name="password"
            label="Password"
            placeholder="Enter your password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => handlePasswordChange(e.target.value)}
            onBlur={handlePasswordBlur}
            error={!!errors.password}
            helperText={errors.password}
            disabled={isLoading}
            sx={{
              '& .MuiOutlinedInput-root': {
                borderRadius: '8px',
              },
            }}
          />

          {/* Remember Me & Forgot Password */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            {showRememberMe && (
              <FormControlLabel
                control={
                  <Checkbox
                    checked={rememberMe}
                    onChange={(e) => handleRememberMeChange(e.target.checked)}
                    color="primary"
                    disabled={isLoading}
                  />
                }
                label={
                  <Typography variant="body2" color="text.secondary">
                    Remember Me
                  </Typography>
                }
              />
            )}

            {showForgotPassword && (
              <Link
                href="#"
                variant="body2"
                onClick={handleForgotPasswordClick}
                sx={{
                  textDecoration: 'none',
                  color: 'primary.main',
                  fontWeight: 500,
                  '&:hover': {
                    textDecoration: 'underline',
                  },
                }}
              >
                Forgot Password?
              </Link>
            )}
          </Box>

          {/* Submit Button */}
          <Button
            type="submit"
            variant="contained"
            color="primary"
            size="large"
            fullWidth
            loading={isLoading}
            loadingText="Signing in..."
            sx={{
              height: 48,
              borderRadius: '8px',
              textTransform: 'none',
              fontSize: '15px',
              fontWeight: 600,
            }}
          >
            Sign In
          </Button>

          {/* Footer Text */}
          <Typography
            variant="caption"
            sx={{
              textAlign: 'center',
              color: 'text.secondary',
              fontSize: '12px',
              lineHeight: 1.5,
              mt: 2,
            }}
          >
            Protected by reCAPTCHA and subject to the Heimdallr{' '}
            <Link
              href="/privacy"
              sx={{
                color: 'text.secondary',
                textDecoration: 'underline',
                '&:hover': {
                  color: 'primary.main',
                },
              }}
            >
              Privacy Policy
            </Link>{' '}
            and{' '}
            <Link
              href="/terms"
              sx={{
                color: 'text.secondary',
                textDecoration: 'underline',
                '&:hover': {
                  color: 'primary.main',
                },
              }}
            >
              Terms of Service
            </Link>
          </Typography>
        </Stack>
      </Box>
    );
  }
);

LoginForm.displayName = 'LoginForm';
