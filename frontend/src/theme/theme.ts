import { createTheme, ThemeOptions } from '@mui/material/styles';
import { palette } from './palette';
import { typography } from './typography';
import { breakpoints } from './breakpoints';
import { components } from './components';

const themeOptions: ThemeOptions = {
  palette,
  typography,
  breakpoints,
  components,
  spacing: 8, // 8px grid
  shape: {
    borderRadius: 8,
  },
};

export const theme = createTheme(themeOptions);
