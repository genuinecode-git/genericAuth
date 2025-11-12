import { SvgIconComponent } from '@mui/icons-material';

export interface FeatureCardProps {
  /**
   * Icon component to display
   */
  icon: SvgIconComponent;

  /**
   * Title of the feature
   */
  title: string;

  /**
   * Description of the feature
   */
  description: string;
}
