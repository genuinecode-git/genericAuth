import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { theme } from './theme';
import { LoginPage } from './features/auth/pages';
import { TermsOfServicePage, PrivacyPolicyPage } from './features/legal/pages';

/**
 * Main App component with routing and theme configuration
 */
const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/dashboard" element={<DashboardPlaceholder />} />
          <Route path="/register" element={<RegisterPlaceholder />} />
          <Route path="/forgot-password" element={<ForgotPasswordPlaceholder />} />
          <Route path="/terms" element={<TermsOfServicePage />} />
          <Route path="/privacy" element={<PrivacyPolicyPage />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
};

/**
 * Placeholder components for routes not yet implemented
 */
const DashboardPlaceholder: React.FC = () => (
  <div style={{ padding: '40px', textAlign: 'center' }}>
    <h1>Dashboard</h1>
    <p>You have successfully logged in!</p>
    <a href="/login">Back to Login</a>
  </div>
);

const RegisterPlaceholder: React.FC = () => (
  <div style={{ padding: '40px', textAlign: 'center' }}>
    <h1>Register</h1>
    <p>Registration page coming soon</p>
    <a href="/login">Back to Login</a>
  </div>
);

const ForgotPasswordPlaceholder: React.FC = () => (
  <div style={{ padding: '40px', textAlign: 'center' }}>
    <h1>Forgot Password</h1>
    <p>Password reset page coming soon</p>
    <a href="/login">Back to Login</a>
  </div>
);

export default App;
