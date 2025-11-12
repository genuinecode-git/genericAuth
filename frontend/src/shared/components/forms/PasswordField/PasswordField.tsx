import React, { useState, useCallback, useMemo } from 'react';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import LinearProgress from '@mui/material/LinearProgress';
import Typography from '@mui/material/Typography';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import { TextField } from '../TextField';
import { PasswordFieldProps, PasswordStrength } from './PasswordField.types';
import { calculatePasswordStrength as defaultCalculateStrength } from '../../../utils/validators';

/**
 * Password field with show/hide toggle and optional strength indicator
 */
export const PasswordField = React.memo<PasswordFieldProps>(
  ({
    showStrengthIndicator = false,
    calculateStrength = defaultCalculateStrength,
    value = '',
    ...props
  }) => {
    const [showPassword, setShowPassword] = useState(false);

    const handleToggleVisibility = useCallback(() => {
      setShowPassword((prev) => !prev);
    }, []);

    const handleMouseDownPassword = useCallback((event: React.MouseEvent<HTMLButtonElement>) => {
      event.preventDefault();
    }, []);

    const strength: PasswordStrength | null = useMemo(() => {
      if (!showStrengthIndicator || typeof value !== 'string' || value.length === 0) {
        return null;
      }
      return calculateStrength(value);
    }, [showStrengthIndicator, value, calculateStrength]);

    const strengthConfig = useMemo(() => {
      if (!strength) return null;

      const configs = {
        weak: { value: 33, color: 'error.main', label: 'Weak' },
        medium: { value: 66, color: 'warning.main', label: 'Medium' },
        strong: { value: 100, color: 'success.main', label: 'Strong' },
      };

      return configs[strength];
    }, [strength]);

    return (
      <Box sx={{ width: '100%' }}>
        <TextField
          {...props}
          type={showPassword ? 'text' : 'password'}
          value={value}
          InputProps={{
            ...props.InputProps,
            endAdornment: (
              <InputAdornment position="end">
                <IconButton
                  aria-label="toggle password visibility"
                  onClick={handleToggleVisibility}
                  onMouseDown={handleMouseDownPassword}
                  edge="end"
                >
                  {showPassword ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              </InputAdornment>
            ),
          }}
        />
        {showStrengthIndicator && strength && strengthConfig && (
          <Box sx={{ mt: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
              <LinearProgress
                variant="determinate"
                value={strengthConfig.value}
                sx={{
                  flex: 1,
                  height: 4,
                  borderRadius: 2,
                  backgroundColor: 'grey.200',
                  '& .MuiLinearProgress-bar': {
                    backgroundColor: strengthConfig.color,
                    borderRadius: 2,
                  },
                }}
              />
              <Typography
                variant="caption"
                sx={{
                  color: strengthConfig.color,
                  fontWeight: 500,
                  minWidth: 60,
                }}
              >
                {strengthConfig.label}
              </Typography>
            </Box>
          </Box>
        )}
      </Box>
    );
  }
);

PasswordField.displayName = 'PasswordField';
