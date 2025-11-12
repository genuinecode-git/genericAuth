import React from 'react';
import MuiGrid from '@mui/material/Grid';
import { GridProps } from './Grid.types';

/**
 * Re-export of MUI Grid with type definitions
 */
export const Grid = React.memo<GridProps>((props) => {
  return <MuiGrid {...props} />;
});

Grid.displayName = 'Grid';
