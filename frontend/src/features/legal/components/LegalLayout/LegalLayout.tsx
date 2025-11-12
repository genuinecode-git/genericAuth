import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Link,
  Divider,
  IconButton,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { LegalLayoutProps, LegalSectionProps } from './LegalLayout.types';

/**
 * Shared layout component for legal documents (Terms of Service, Privacy Policy)
 * Provides consistent styling, navigation, and structure
 */
export const LegalLayout: React.FC<LegalLayoutProps> = ({
  title,
  lastUpdated,
  children,
  breadcrumbText = 'Back to Login',
  breadcrumbLink = '/login',
}) => {
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  // Scroll to top on mount
  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  const handleBackClick = (event: React.MouseEvent) => {
    event.preventDefault();
    navigate(breadcrumbLink);
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        backgroundColor: 'background.default',
        pb: 8,
      }}
    >
      {/* Header Bar */}
      <Box
        sx={{
          backgroundColor: 'primary.main',
          color: 'primary.contrastText',
          py: 3,
          px: 2,
          boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
        }}
      >
        <Container maxWidth="md">
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 2,
            }}
          >
            <IconButton
              onClick={handleBackClick}
              sx={{
                color: 'primary.contrastText',
                '&:hover': {
                  backgroundColor: 'rgba(255, 255, 255, 0.1)',
                },
              }}
              aria-label={breadcrumbText}
            >
              <ArrowBackIcon />
            </IconButton>
            <Typography
              variant={isMobile ? 'h5' : 'h4'}
              component="h1"
              sx={{
                fontWeight: 700,
                flexGrow: 1,
              }}
            >
              {title}
            </Typography>
          </Box>
        </Container>
      </Box>

      {/* Main Content */}
      <Container maxWidth="md" sx={{ mt: 4 }}>
        <Box
          sx={{
            backgroundColor: 'background.paper',
            borderRadius: 2,
            boxShadow: '0 1px 3px rgba(0, 0, 0, 0.12)',
            p: { xs: 3, sm: 4, md: 6 },
          }}
        >
          {/* Breadcrumb Navigation */}
          <Box sx={{ mb: 3 }}>
            <Link
              href={breadcrumbLink}
              onClick={handleBackClick}
              sx={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 0.5,
                color: 'primary.main',
                textDecoration: 'none',
                fontSize: '14px',
                fontWeight: 500,
                '&:hover': {
                  textDecoration: 'underline',
                },
              }}
            >
              <ArrowBackIcon sx={{ fontSize: 16 }} />
              {breadcrumbText}
            </Link>
          </Box>

          <Divider sx={{ mb: 4 }} />

          {/* Document Content */}
          <Box
            sx={{
              '& h2': {
                fontSize: { xs: '1.5rem', sm: '1.75rem' },
                fontWeight: 700,
                color: 'text.primary',
                mt: 4,
                mb: 2,
              },
              '& h3': {
                fontSize: { xs: '1.125rem', sm: '1.25rem' },
                fontWeight: 600,
                color: 'text.primary',
                mt: 3,
                mb: 1.5,
              },
              '& p': {
                fontSize: '1rem',
                lineHeight: 1.7,
                color: 'text.secondary',
                mb: 2,
              },
              '& ul, & ol': {
                pl: { xs: 2, sm: 3 },
                mb: 2,
              },
              '& li': {
                fontSize: '1rem',
                lineHeight: 1.7,
                color: 'text.secondary',
                mb: 1,
              },
              '& pre': {
                backgroundColor: 'grey.100',
                p: 2,
                borderRadius: 1,
                overflow: 'auto',
                fontSize: '0.875rem',
                lineHeight: 1.6,
                mb: 2,
              },
              '& code': {
                backgroundColor: 'grey.100',
                px: 0.5,
                py: 0.25,
                borderRadius: 0.5,
                fontSize: '0.875rem',
                fontFamily: 'monospace',
              },
              '& a': {
                color: 'primary.main',
                textDecoration: 'underline',
                '&:hover': {
                  color: 'primary.dark',
                },
              },
            }}
          >
            {children}
          </Box>

          <Divider sx={{ mt: 6, mb: 3 }} />

          {/* Footer */}
          <Box
            sx={{
              display: 'flex',
              flexDirection: { xs: 'column', sm: 'row' },
              justifyContent: 'space-between',
              alignItems: { xs: 'flex-start', sm: 'center' },
              gap: 2,
            }}
          >
            <Typography
              variant="body2"
              sx={{
                color: 'text.secondary',
                fontSize: '0.875rem',
              }}
            >
              Last Updated: {lastUpdated}
            </Typography>
            <Link
              href={breadcrumbLink}
              onClick={handleBackClick}
              sx={{
                color: 'primary.main',
                textDecoration: 'none',
                fontSize: '0.875rem',
                fontWeight: 500,
                '&:hover': {
                  textDecoration: 'underline',
                },
              }}
            >
              {breadcrumbText}
            </Link>
          </Box>
        </Box>
      </Container>
    </Box>
  );
};

/**
 * Reusable section component for legal documents
 */
export const LegalSection: React.FC<LegalSectionProps> = ({
  title,
  children,
  id,
}) => {
  return (
    <Box id={id} sx={{ mb: 4 }}>
      <Typography
        variant="h2"
        component="h2"
        sx={{
          fontSize: { xs: '1.5rem', sm: '1.75rem' },
          fontWeight: 700,
          color: 'text.primary',
          mb: 2,
          scrollMarginTop: '100px', // For anchor link offset
        }}
      >
        {title}
      </Typography>
      <Box>{children}</Box>
    </Box>
  );
};

LegalLayout.displayName = 'LegalLayout';
LegalSection.displayName = 'LegalSection';
