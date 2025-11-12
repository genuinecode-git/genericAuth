import React, { useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import { FormContainerProps } from './FormContainer.types';

/**
 * Container component for forms with consistent styling and layout
 */
export const FormContainer = React.memo<FormContainerProps>(
  ({
    title,
    subtitle,
    children,
    onSubmit,
    maxWidth = 480,
    elevation = 3,
    sx,
    footer,
  }) => {
    const handleSubmit = useCallback(
      (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        onSubmit?.(event);
      },
      [onSubmit]
    );

    return (
      <Card
        elevation={elevation}
        sx={{
          width: '100%',
          maxWidth,
          ...sx,
        }}
      >
        <CardContent
          sx={{
            p: { xs: 3, sm: 4 },
            '&:last-child': {
              pb: { xs: 3, sm: 4 },
            },
          }}
        >
          {(title || subtitle) && (
            <Box sx={{ mb: 3, textAlign: 'center' }}>
              {title && (
                <Typography
                  variant="h5"
                  component="h1"
                  sx={{
                    fontWeight: 600,
                    mb: subtitle ? 1 : 0,
                  }}
                >
                  {title}
                </Typography>
              )}
              {subtitle && (
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ mt: 1 }}
                >
                  {subtitle}
                </Typography>
              )}
            </Box>
          )}

          <Box
            component="form"
            onSubmit={handleSubmit}
            noValidate
            sx={{ width: '100%' }}
          >
            {children}
          </Box>

          {footer && (
            <Box sx={{ mt: 3, textAlign: 'center' }}>
              {footer}
            </Box>
          )}
        </CardContent>
      </Card>
    );
  }
);

FormContainer.displayName = 'FormContainer';
