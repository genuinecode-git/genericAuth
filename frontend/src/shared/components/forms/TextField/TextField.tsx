import React, { useState, useCallback, useMemo } from 'react';
import MuiTextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { TextFieldProps } from './TextField.types';

/**
 * Enhanced TextField component with validation and character count
 */
export const TextField = React.memo<TextFieldProps>(
  ({
    maxLength,
    showCharCount = false,
    validate,
    validateOnBlur = true,
    value = '',
    onChange,
    onBlur,
    error: externalError,
    helperText: externalHelperText,
    ...props
  }) => {
    const [internalError, setInternalError] = useState<string | undefined>();
    const [touched, setTouched] = useState(false);

    const handleBlur = useCallback(
      (event: React.FocusEvent<HTMLInputElement>) => {
        setTouched(true);

        if (validateOnBlur && validate && typeof value === 'string') {
          const errorMessage = validate(value);
          setInternalError(errorMessage);
        }

        onBlur?.(event);
      },
      [validate, validateOnBlur, value, onBlur]
    );

    const handleChange = useCallback(
      (event: React.ChangeEvent<HTMLInputElement>) => {
        const newValue = event.target.value;

        // Enforce max length
        if (maxLength && newValue.length > maxLength) {
          return;
        }

        // Validate on change if already touched
        if (touched && validate) {
          const errorMessage = validate(newValue);
          setInternalError(errorMessage);
        }

        onChange?.(event);
      },
      [maxLength, touched, validate, onChange]
    );

    const hasError = externalError || (touched && !!internalError);
    const displayHelperText = externalHelperText || (touched ? internalError : undefined);

    const characterCount = useMemo(() => {
      const currentLength = typeof value === 'string' ? value.length : 0;
      return maxLength ? `${currentLength}/${maxLength}` : `${currentLength}`;
    }, [value, maxLength]);

    return (
      <Box sx={{ width: '100%' }}>
        <MuiTextField
          {...props}
          value={value}
          onChange={handleChange}
          onBlur={handleBlur}
          error={hasError}
          helperText={displayHelperText}
          inputProps={{
            ...props.inputProps,
            maxLength,
          }}
        />
        {showCharCount && (
          <Typography
            variant="caption"
            sx={{
              display: 'block',
              textAlign: 'right',
              mt: 0.5,
              color: 'text.secondary',
            }}
          >
            {characterCount}
          </Typography>
        )}
      </Box>
    );
  }
);

TextField.displayName = 'TextField';
