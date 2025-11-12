import React from 'react';
import MuiStack from '@mui/material/Stack';
import { StackProps } from './Stack.types';

/**
 * Re-export of MUI Stack with type definitions
 */
export const Stack = React.memo<StackProps>((props) => {
  return <MuiStack {...props} />;
});

Stack.displayName = 'Stack';
