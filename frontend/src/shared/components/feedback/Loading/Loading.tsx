import React from 'react';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import Backdrop from '@mui/material/Backdrop';
import { LoadingProps } from './Loading.types';

/**
 * Loading indicator with optional message and overlay modes
 */
export const Loading = React.memo<LoadingProps>(
  ({
    message,
    centered = true,
    overlay = false,
    size = 40,
    ...props
  }) => {
    const content = (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 2,
        }}
      >
        <CircularProgress size={size} {...props} />
        {message && (
          <Typography variant="body2" color="text.secondary">
            {message}
          </Typography>
        )}
      </Box>
    );

    if (overlay) {
      return (
        <Backdrop
          open
          sx={{
            color: '#fff',
            zIndex: (theme) => theme.zIndex.drawer + 1,
            backgroundColor: 'rgba(0, 0, 0, 0.7)',
          }}
        >
          {content}
        </Backdrop>
      );
    }

    if (centered) {
      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '200px',
            width: '100%',
          }}
        >
          {content}
        </Box>
      );
    }

    return content;
  }
);

Loading.displayName = 'Loading';
