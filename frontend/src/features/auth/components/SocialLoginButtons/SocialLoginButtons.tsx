import React, { useCallback } from 'react';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import Typography from '@mui/material/Typography';
import { Button } from '@shared/components/buttons';
import { Stack } from '@shared/components/layout';
import GoogleIcon from '@mui/icons-material/Google';
import FacebookIcon from '@mui/icons-material/Facebook';
import GitHubIcon from '@mui/icons-material/GitHub';
import MicrosoftIcon from '@mui/icons-material/Microsoft';
import { SocialProvider } from '../../types';
import { SocialLoginButtonsProps } from './SocialLoginButtons.types';

/**
 * Provider configuration with icons and labels
 */
const providerConfig = {
  google: {
    label: 'Continue with Google',
    icon: GoogleIcon,
    color: '#DB4437',
  },
  facebook: {
    label: 'Continue with Facebook',
    icon: FacebookIcon,
    color: '#1877F2',
  },
  github: {
    label: 'Continue with GitHub',
    icon: GitHubIcon,
    color: '#333333',
  },
  microsoft: {
    label: 'Continue with Microsoft',
    icon: MicrosoftIcon,
    color: '#00A4EF',
  },
} as const;

/**
 * Social login buttons component
 */
export const SocialLoginButtons = React.memo<SocialLoginButtonsProps>(
  ({
    onSocialLogin,
    loading = false,
    providers = ['google', 'microsoft', 'github'],
    showDivider = true,
  }) => {
    const handleProviderClick = useCallback(
      (provider: SocialProvider) => {
        if (!loading) {
          onSocialLogin?.(provider);
        }
      },
      [loading, onSocialLogin]
    );

    if (providers.length === 0) {
      return null;
    }

    return (
      <Box sx={{ width: '100%' }}>
        <Stack spacing={2}>
          {providers.map((provider) => {
            const config = providerConfig[provider];
            const Icon = config.icon;

            return (
              <Button
                key={provider}
                variant="outlined"
                fullWidth
                size="large"
                disabled={loading}
                onClick={() => handleProviderClick(provider)}
                startIcon={<Icon />}
                sx={{
                  height: 48,
                  borderColor: '#E0E0E0',
                  borderRadius: '8px',
                  color: 'text.primary',
                  textTransform: 'none',
                  fontSize: '15px',
                  fontWeight: 500,
                  justifyContent: 'flex-start',
                  pl: 3,
                  '&:hover': {
                    borderColor: 'primary.main',
                    backgroundColor: 'action.hover',
                  },
                }}
              >
                {config.label}
              </Button>
            );
          })}
        </Stack>

        {showDivider && (
          <Divider sx={{ my: 3 }}>
            <Typography variant="body2" color="text.secondary">
              Or continue via email
            </Typography>
          </Divider>
        )}
      </Box>
    );
  }
);

SocialLoginButtons.displayName = 'SocialLoginButtons';
