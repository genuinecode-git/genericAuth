import React from 'react';
import { Typography, Box } from '@mui/material';
import { LegalLayout, LegalSection } from '../components';

/**
 * Privacy Policy page for Heimdallr authentication application
 * Comprehensive privacy policy covering data collection, usage, and user rights
 */
export const PrivacyPolicyPage: React.FC = () => {
  return (
    <LegalLayout title="Privacy Policy" lastUpdated="November 12, 2025">
      {/* Introduction */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="body1" paragraph>
          At Heimdallr, we take your privacy seriously. This Privacy Policy
          explains how we collect, use, disclose, and safeguard your
          information when you use our authentication service.
        </Typography>
        <Typography variant="body1" paragraph>
          Please read this privacy policy carefully. By using Heimdallr, you
          agree to the collection and use of information in accordance with
          this policy. If you do not agree with our policies and practices,
          please do not use our service.
        </Typography>
      </Box>

      {/* Section 1: Information We Collect */}
      <LegalSection
        title="1. Information We Collect"
        id="information-collected"
      >
        <Typography variant="h3" component="h3" sx={{ mt: 2, mb: 1.5 }}>
          1.1 Personal Information
        </Typography>
        <Typography variant="body1" paragraph>
          When you create an account with Heimdallr, we collect the following
          personal information:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Email Address:</strong> Used as your unique identifier
              and for account-related communications
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Password:</strong> Stored as a cryptographically hashed
              value using industry-standard algorithms (never stored in plain
              text)
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Display Name:</strong> Optional name for personalizing
              your account
            </Typography>
          </li>
        </Box>

        <Typography variant="h3" component="h3" sx={{ mt: 3, mb: 1.5 }}>
          1.2 Authentication Data
        </Typography>
        <Typography variant="body1" paragraph>
          We automatically collect certain information when you use our service:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Login Timestamps:</strong> Date and time of each login
              attempt
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Session Information:</strong> Authentication tokens and
              session identifiers
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>IP Addresses:</strong> For security and fraud prevention
              purposes
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Device Information:</strong> Browser type, operating
              system, and device identifiers
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Login History:</strong> Record of successful and failed
              authentication attempts
            </Typography>
          </li>
        </Box>

        <Typography variant="h3" component="h3" sx={{ mt: 3, mb: 1.5 }}>
          1.3 OAuth Provider Data
        </Typography>
        <Typography variant="body1" paragraph>
          If you authenticate using third-party OAuth providers (Google,
          Microsoft, GitHub), we may receive:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Profile information (name, email, profile picture) as authorized
              by you
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              OAuth tokens for maintaining authenticated sessions
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Basic account information provided by the OAuth provider
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph>
          We only request the minimum permissions necessary to provide our
          authentication service. Your OAuth provider's privacy policy also
          applies to data shared through their service.
        </Typography>
      </LegalSection>

      {/* Section 2: How We Use Your Information */}
      <LegalSection title="2. How We Use Your Information" id="information-use">
        <Typography variant="body1" paragraph>
          We use the information we collect for the following purposes:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Authentication and Authorization:</strong> To verify your
              identity and provide secure access to services
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Account Management:</strong> To create, maintain, and
              manage your account
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Security:</strong> To detect and prevent fraud,
              unauthorized access, and security threats
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Service Improvement:</strong> To analyze usage patterns
              and improve our authentication service
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Communication:</strong> To send you important service
              updates, security alerts, and account notifications
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Legal Compliance:</strong> To comply with applicable
              laws, regulations, and legal processes
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Support:</strong> To respond to your inquiries and
              provide customer support
            </Typography>
          </li>
        </Box>
      </LegalSection>

      {/* Section 3: Data Storage and Security */}
      <LegalSection title="3. Data Storage and Security" id="data-security">
        <Typography variant="body1" paragraph>
          We implement industry-standard security measures to protect your
          personal information:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Encryption:</strong> All data transmitted between your
              device and our servers is encrypted using TLS/SSL protocols
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Password Hashing:</strong> Passwords are hashed using
              bcrypt or similar cryptographic algorithms and are never stored
              in plain text
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Access Controls:</strong> Strict access controls limit
              who can access your personal information
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Regular Security Audits:</strong> We conduct regular
              security assessments and vulnerability testing
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Secure Infrastructure:</strong> Data is stored in secure,
              access-controlled data centers
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          However, no method of transmission over the Internet or electronic
          storage is 100% secure. While we strive to use commercially
          acceptable means to protect your personal information, we cannot
          guarantee its absolute security.
        </Typography>
      </LegalSection>

      {/* Section 4: Third-Party Services */}
      <LegalSection title="4. Third-Party Services" id="third-party">
        <Typography variant="body1" paragraph>
          Heimdallr may integrate with third-party OAuth providers and services:
        </Typography>

        <Typography variant="h3" component="h3" sx={{ mt: 2, mb: 1.5 }}>
          4.1 OAuth Providers
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Google:</strong> Subject to{' '}
              <a
                href="https://policies.google.com/privacy"
                target="_blank"
                rel="noopener noreferrer"
              >
                Google Privacy Policy
              </a>
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Microsoft:</strong> Subject to{' '}
              <a
                href="https://privacy.microsoft.com/privacystatement"
                target="_blank"
                rel="noopener noreferrer"
              >
                Microsoft Privacy Statement
              </a>
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>GitHub:</strong> Subject to{' '}
              <a
                href="https://docs.github.com/en/site-policy/privacy-policies/github-privacy-statement"
                target="_blank"
                rel="noopener noreferrer"
              >
                GitHub Privacy Statement
              </a>
            </Typography>
          </li>
        </Box>

        <Typography variant="h3" component="h3" sx={{ mt: 3, mb: 1.5 }}>
          4.2 Analytics and Monitoring
        </Typography>
        <Typography variant="body1" paragraph>
          We may use analytics services to help us understand how our service
          is used. These services may collect information about your usage
          patterns, but we ensure that any third-party analytics tools we use
          comply with privacy regulations and data protection standards.
        </Typography>
      </LegalSection>

      {/* Section 5: Your Rights (GDPR Compliance) */}
      <LegalSection title="5. Your Rights" id="your-rights">
        <Typography variant="body1" paragraph>
          Under data protection laws, including GDPR and CCPA, you have the
          following rights regarding your personal information:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Right to Access:</strong> You can request a copy of the
              personal information we hold about you
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Right to Rectification:</strong> You can request
              correction of inaccurate or incomplete personal information
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Right to Erasure:</strong> You can request deletion of
              your personal information (subject to legal retention requirements)
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Right to Restriction:</strong> You can request that we
              restrict processing of your personal information in certain
              circumstances
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Right to Data Portability:</strong> You can request a
              machine-readable copy of your personal information
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Right to Object:</strong> You can object to our
              processing of your personal information in certain circumstances
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Right to Withdraw Consent:</strong> You can withdraw
              consent for data processing at any time
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          To exercise any of these rights, please contact us at{' '}
          <a href="mailto:privacy@heimdallr.com">privacy@heimdallr.com</a>. We
          will respond to your request within the timeframe required by
          applicable law (typically within 30 days).
        </Typography>
      </LegalSection>

      {/* Section 6: Data Retention */}
      <LegalSection title="6. Data Retention" id="data-retention">
        <Typography variant="body1" paragraph>
          We retain your personal information for as long as necessary to
          provide our service and fulfill the purposes outlined in this privacy
          policy. Specifically:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              <strong>Active Accounts:</strong> Account data is retained while
              your account is active
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Closed Accounts:</strong> After account deletion, we
              retain minimal data for a limited period to comply with legal
              obligations
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Security Logs:</strong> Authentication logs and security
              data are retained for up to 90 days for security and fraud
              prevention
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              <strong>Legal Requirements:</strong> Some data may be retained
              longer to comply with legal, regulatory, or accounting requirements
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          When personal information is no longer needed, we securely delete or
          anonymize it in accordance with data protection standards.
        </Typography>
      </LegalSection>

      {/* Section 7: Cookies and Tracking */}
      <LegalSection title="7. Cookies and Tracking" id="cookies">
        <Typography variant="body1" paragraph>
          Heimdallr uses cookies and similar tracking technologies to maintain
          your authenticated session and improve your experience:
        </Typography>

        <Typography variant="h3" component="h3" sx={{ mt: 2, mb: 1.5 }}>
          7.1 Essential Cookies
        </Typography>
        <Typography variant="body1" paragraph>
          These cookies are necessary for the service to function and cannot be
          disabled:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Session cookies for authentication state
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Security tokens for CSRF protection
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Authentication tokens for maintaining logged-in sessions
            </Typography>
          </li>
        </Box>

        <Typography variant="h3" component="h3" sx={{ mt: 3, mb: 1.5 }}>
          7.2 Analytics Cookies
        </Typography>
        <Typography variant="body1" paragraph>
          With your consent, we may use analytics cookies to understand how
          users interact with our service. These help us improve functionality
          and user experience. You can opt out of analytics cookies through
          your browser settings.
        </Typography>

        <Typography variant="h3" component="h3" sx={{ mt: 3, mb: 1.5 }}>
          7.3 Managing Cookies
        </Typography>
        <Typography variant="body1" paragraph>
          Most web browsers allow you to control cookies through settings.
          However, disabling essential cookies may prevent you from using
          certain features of the service.
        </Typography>
      </LegalSection>

      {/* Section 8: International Data Transfers */}
      <LegalSection
        title="8. International Data Transfers"
        id="data-transfers"
      >
        <Typography variant="body1" paragraph>
          Your information may be transferred to and processed in countries
          other than your country of residence. These countries may have
          different data protection laws.
        </Typography>
        <Typography variant="body1" paragraph>
          When we transfer personal information internationally, we ensure
          appropriate safeguards are in place, such as:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Standard contractual clauses approved by regulatory authorities
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Adequacy decisions confirming adequate data protection levels
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Binding corporate rules for transfers within our organization
            </Typography>
          </li>
        </Box>
      </LegalSection>

      {/* Section 9: Children's Privacy */}
      <LegalSection title="9. Children's Privacy" id="childrens-privacy">
        <Typography variant="body1" paragraph>
          Heimdallr is not intended for use by children under the age of 13 (or
          16 in the European Economic Area). We do not knowingly collect
          personal information from children.
        </Typography>
        <Typography variant="body1" paragraph>
          If we become aware that we have collected personal information from a
          child without parental consent, we will take steps to delete that
          information promptly. If you believe we have collected information
          from a child, please contact us immediately.
        </Typography>
      </LegalSection>

      {/* Section 10: Data Breach Notification */}
      <LegalSection title="10. Data Breach Notification" id="data-breach">
        <Typography variant="body1" paragraph>
          In the event of a data breach that may compromise your personal
          information, we will:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Notify affected users without undue delay (within 72 hours where
              required by law)
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Provide information about the nature of the breach and data
              affected
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Outline steps taken to mitigate the breach
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Recommend actions you can take to protect yourself
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Notify relevant regulatory authorities as required by law
            </Typography>
          </li>
        </Box>
      </LegalSection>

      {/* Section 11: Changes to Privacy Policy */}
      <LegalSection title="11. Changes to Privacy Policy" id="policy-changes">
        <Typography variant="body1" paragraph>
          We may update this Privacy Policy from time to time to reflect
          changes in our practices or applicable laws. We will notify you of
          any material changes by:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Posting the updated policy on this page with a new "Last Updated"
              date
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Sending an email notification to your registered email address
              for significant changes
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Displaying a prominent notice within the service
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          We encourage you to review this Privacy Policy periodically. Your
          continued use of the service after changes are posted constitutes
          your acceptance of the updated policy.
        </Typography>
      </LegalSection>

      {/* Section 12: Contact Information */}
      <LegalSection title="12. Contact Information" id="contact">
        <Typography variant="body1" paragraph>
          If you have questions, concerns, or requests regarding this Privacy
          Policy or our data practices, please contact us:
        </Typography>
        <Box sx={{ pl: 2 }}>
          <Typography variant="body1" paragraph>
            <strong>Privacy Officer:</strong>
          </Typography>
          <Typography variant="body1" paragraph>
            <strong>Email:</strong>{' '}
            <a href="mailto:privacy@heimdallr.com">privacy@heimdallr.com</a>
          </Typography>
          <Typography variant="body1" paragraph>
            <strong>General Support:</strong>{' '}
            <a href="mailto:support@heimdallr.com">support@heimdallr.com</a>
          </Typography>
          <Typography variant="body1" paragraph>
            <strong>GitHub:</strong>{' '}
            <a
              href="https://github.com/heimdallr-auth"
              target="_blank"
              rel="noopener noreferrer"
            >
              https://github.com/heimdallr-auth
            </a>
          </Typography>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 3 }}>
          For EU residents, you also have the right to lodge a complaint with
          your local data protection authority if you believe your data
          protection rights have been violated.
        </Typography>
      </LegalSection>

      {/* Final Statement */}
      <Box sx={{ mt: 6, p: 3, backgroundColor: 'grey.50', borderRadius: 2 }}>
        <Typography
          variant="body2"
          sx={{ color: 'text.secondary', fontStyle: 'italic' }}
        >
          Your privacy is important to us. We are committed to protecting your
          personal information and being transparent about how we use it. If
          you have any questions or concerns about this Privacy Policy, please
          don't hesitate to contact us.
        </Typography>
      </Box>
    </LegalLayout>
  );
};

PrivacyPolicyPage.displayName = 'PrivacyPolicyPage';
