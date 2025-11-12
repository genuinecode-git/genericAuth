import { SocialProvider } from '../../types';

export interface SocialLoginButtonsProps {
  /**
   * Callback when social login is clicked
   */
  onSocialLogin?: (provider: SocialProvider) => void;
  /**
   * Whether social login is in progress
   */
  loading?: boolean;
  /**
   * Which providers to show (if not specified, shows all)
   */
  providers?: SocialProvider[];
  /**
   * Whether to show divider with "or" text
   */
  showDivider?: boolean;
}
