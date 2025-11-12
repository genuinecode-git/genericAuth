import React from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { FeatureCardProps } from './FeatureCard.types';

/**
 * Feature card component for displaying feature information with icon
 */
export const FeatureCard = React.memo<FeatureCardProps>(
  ({ icon: Icon, title, description }) => {
    return (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'flex-start',
          gap: 2,
          py: 2,
        }}
      >
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: 48,
            height: 48,
            flexShrink: 0,
          }}
        >
          <Icon
            sx={{
              fontSize: 32,
              color: 'white',
            }}
          />
        </Box>
        <Box sx={{ flex: 1 }}>
          <Typography
            variant="h6"
            sx={{
              color: 'white',
              fontWeight: 600,
              fontSize: '18px',
              mb: 0.5,
            }}
          >
            {title}
          </Typography>
          <Typography
            variant="body2"
            sx={{
              color: 'rgba(255, 255, 255, 0.9)',
              fontSize: '14px',
              lineHeight: 1.5,
            }}
          >
            {description}
          </Typography>
        </Box>
      </Box>
    );
  }
);

FeatureCard.displayName = 'FeatureCard';
