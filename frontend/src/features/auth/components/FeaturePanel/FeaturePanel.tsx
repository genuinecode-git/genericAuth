import React from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import SecurityIcon from '@mui/icons-material/Security';
import ShieldIcon from '@mui/icons-material/Shield';
import FlashOnIcon from '@mui/icons-material/FlashOn';
import LanguageIcon from '@mui/icons-material/Language';
import { FeatureCard } from '../FeatureCard';

/**
 * Left panel component with branding and feature cards
 */
export const FeaturePanel = React.memo(() => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: '#6B46C1',
        minHeight: { xs: 'auto', md: '100vh' },
        p: { xs: 4, md: 6 },
        width: '100%',
      }}
    >
      {/* Branding Section */}
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          mb: 6,
        }}
      >
        <SecurityIcon
          sx={{
            fontSize: 64,
            color: 'white',
            mb: 2,
          }}
        />
        <Typography
          variant="h3"
          sx={{
            color: 'white',
            fontWeight: 700,
            fontSize: { xs: '28px', md: '32px' },
            mb: 1,
            textAlign: 'center',
          }}
        >
          Heimdallr
        </Typography>
        <Typography
          variant="body1"
          sx={{
            color: 'rgba(255, 255, 255, 0.85)',
            fontSize: '14px',
            textAlign: 'center',
          }}
        >
          Your Guardian to Secure Access
        </Typography>
      </Box>

      {/* Feature Cards */}
      <Box
        sx={{
          width: '100%',
          maxWidth: 400,
        }}
      >
        <FeatureCard
          icon={ShieldIcon}
          title="Attack Prevention"
          description="Defend against unauthorized access"
        />
        <FeatureCard
          icon={FlashOnIcon}
          title="Lightning Fast"
          description="Quick and secure login process"
        />
        <FeatureCard
          icon={LanguageIcon}
          title="Universal Access"
          description="Access from anywhere, anytime"
        />
      </Box>
    </Box>
  );
});

FeaturePanel.displayName = 'FeaturePanel';
