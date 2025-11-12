import { ReactNode } from 'react';

/**
 * Props for the LegalLayout component
 */
export interface LegalLayoutProps {
  /**
   * The title of the legal document (e.g., "Terms of Service", "Privacy Policy")
   */
  title: string;

  /**
   * The date when the document was last updated
   */
  lastUpdated: string;

  /**
   * The content of the legal document
   */
  children: ReactNode;

  /**
   * Optional breadcrumb text for navigation
   * @default "Back to Login"
   */
  breadcrumbText?: string;

  /**
   * Optional breadcrumb link
   * @default "/login"
   */
  breadcrumbLink?: string;
}

/**
 * Props for legal document sections
 */
export interface LegalSectionProps {
  /**
   * Section title
   */
  title: string;

  /**
   * Section content
   */
  children: ReactNode;

  /**
   * Optional section ID for anchor links
   */
  id?: string;
}
