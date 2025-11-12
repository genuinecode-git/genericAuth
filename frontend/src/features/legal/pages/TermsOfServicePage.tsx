import React from 'react';
import { Typography, Box } from '@mui/material';
import { LegalLayout, LegalSection } from '../components';

/**
 * Terms of Service page for Heimdallr authentication application
 * Built on MIT License foundation with comprehensive terms and conditions
 */
export const TermsOfServicePage: React.FC = () => {
  return (
    <LegalLayout title="Terms of Service" lastUpdated="November 12, 2025">
      {/* Introduction */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="body1" paragraph>
          Welcome to Heimdallr. By accessing and using this authentication
          service, you accept and agree to be bound by the terms and provisions
          of this agreement. If you do not agree to these terms, please do not
          use this service.
        </Typography>
        <Typography variant="body1" paragraph>
          Heimdallr is an open-source authentication platform provided under the
          MIT License. This service is provided "as is" without warranty of any
          kind.
        </Typography>
      </Box>

      {/* Section 1: Acceptance of Terms */}
      <LegalSection title="1. Acceptance of Terms" id="acceptance">
        <Typography variant="body1" paragraph>
          By creating an account or using any feature of the Heimdallr
          authentication service, you acknowledge that you have read,
          understood, and agree to be bound by these Terms of Service, along
          with our Privacy Policy.
        </Typography>
        <Typography variant="body1" paragraph>
          These terms apply to all users of the service, including but not
          limited to individual users, organizations, and developers
          integrating Heimdallr into their applications.
        </Typography>
        <Typography variant="body1" paragraph>
          We reserve the right to update or modify these terms at any time
          without prior notice. Your continued use of the service after any
          such changes constitutes your acceptance of the new terms.
        </Typography>
      </LegalSection>

      {/* Section 2: MIT License */}
      <LegalSection title="2. Software License (MIT License)" id="license">
        <Typography variant="body1" paragraph>
          Heimdallr is released under the MIT License. The following license
          terms apply to the software and source code:
        </Typography>

        <Box
          component="pre"
          sx={{
            backgroundColor: 'grey.100',
            p: 3,
            borderRadius: 2,
            overflow: 'auto',
            fontSize: '0.875rem',
            lineHeight: 1.7,
            fontFamily: 'monospace',
            whiteSpace: 'pre-wrap',
            wordBreak: 'break-word',
          }}
        >
          {`MIT License

Copyright (c) 2025 Heimdallr

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.`}
        </Box>

        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          This means you are free to use, copy, modify, merge, publish,
          distribute, sublicense, and/or sell copies of the software, subject
          to the conditions stated above.
        </Typography>
      </LegalSection>

      {/* Section 3: User Responsibilities */}
      <LegalSection title="3. User Responsibilities" id="responsibilities">
        <Typography variant="body1" paragraph>
          As a user of Heimdallr, you agree to:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Provide accurate, current, and complete information during the
              registration process
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Maintain the security of your password and accept all risks of
              unauthorized access to your account
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Notify us immediately of any unauthorized use of your account or
              any other breach of security
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Not use the service for any illegal or unauthorized purpose
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Not attempt to gain unauthorized access to any portion of the
              service or any other systems or networks
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Not use the service to transmit any malicious code, viruses, or
              harmful content
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Comply with all applicable local, state, national, and
              international laws and regulations
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          You are solely responsible for your conduct and any data, text,
          information, usernames, graphics, or other content that you submit,
          post, or display on or through the service.
        </Typography>
      </LegalSection>

      {/* Section 4: Service Availability */}
      <LegalSection title="4. Service Availability" id="availability">
        <Typography variant="body1" paragraph>
          We strive to provide reliable and continuous service, but we do not
          guarantee that the service will be available at all times or that it
          will be free from errors, delays, or interruptions.
        </Typography>
        <Typography variant="body1" paragraph>
          The service may be temporarily unavailable for scheduled maintenance,
          upgrades, emergency repairs, or due to circumstances beyond our
          control such as network outages, equipment failures, or force majeure
          events.
        </Typography>
        <Typography variant="body1" paragraph>
          We reserve the right to modify, suspend, or discontinue any aspect of
          the service at any time, with or without notice, without liability to
          you or any third party.
        </Typography>
      </LegalSection>

      {/* Section 5: Account Termination */}
      <LegalSection title="5. Account Termination" id="termination">
        <Typography variant="body1" paragraph>
          You may terminate your account at any time by contacting us or using
          the account deletion feature if available. Upon termination, your
          right to use the service will immediately cease.
        </Typography>
        <Typography variant="body1" paragraph>
          We reserve the right to suspend or terminate your account and access
          to the service at our sole discretion, without notice, for conduct
          that we believe violates these Terms of Service, is harmful to other
          users, or is otherwise objectionable.
        </Typography>
        <Typography variant="body1" paragraph>
          Upon termination, we will retain your data only as required by law or
          for legitimate business purposes, as outlined in our Privacy Policy.
        </Typography>
      </LegalSection>

      {/* Section 6: Limitation of Liability */}
      <LegalSection title="6. Limitation of Liability" id="liability">
        <Typography variant="body1" paragraph>
          TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, IN NO EVENT SHALL
          HEIMDALLR, ITS AFFILIATES, OFFICERS, DIRECTORS, EMPLOYEES, AGENTS, OR
          LICENSORS BE LIABLE FOR ANY INDIRECT, INCIDENTAL, SPECIAL,
          CONSEQUENTIAL, OR PUNITIVE DAMAGES, INCLUDING WITHOUT LIMITATION, LOSS
          OF PROFITS, DATA, USE, GOODWILL, OR OTHER INTANGIBLE LOSSES,
          RESULTING FROM:
        </Typography>
        <Box component="ul" sx={{ pl: 3 }}>
          <li>
            <Typography variant="body1">
              Your access to or use of or inability to access or use the service
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Any conduct or content of any third party on the service
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Any content obtained from the service
            </Typography>
          </li>
          <li>
            <Typography variant="body1">
              Unauthorized access, use, or alteration of your transmissions or
              content
            </Typography>
          </li>
        </Box>
        <Typography variant="body1" paragraph sx={{ mt: 2 }}>
          Whether based on warranty, contract, tort (including negligence), or
          any other legal theory, whether or not we have been informed of the
          possibility of such damage.
        </Typography>
      </LegalSection>

      {/* Section 7: Disclaimer of Warranties */}
      <LegalSection title="7. Disclaimer of Warranties" id="warranties">
        <Typography variant="body1" paragraph>
          THE SERVICE IS PROVIDED ON AN "AS IS" AND "AS AVAILABLE" BASIS
          WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
          BUT NOT LIMITED TO IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR
          A PARTICULAR PURPOSE, NON-INFRINGEMENT, OR COURSE OF PERFORMANCE.
        </Typography>
        <Typography variant="body1" paragraph>
          Heimdallr does not warrant that the service will function
          uninterrupted, secure, or available at any particular time or
          location; that any errors or defects will be corrected; that the
          service is free of viruses or other harmful components; or that the
          results of using the service will meet your requirements.
        </Typography>
      </LegalSection>

      {/* Section 8: Intellectual Property */}
      <LegalSection title="8. Intellectual Property" id="intellectual-property">
        <Typography variant="body1" paragraph>
          The Heimdallr service, including its original content, features, and
          functionality, is owned by Heimdallr and is protected by
          international copyright, trademark, patent, trade secret, and other
          intellectual property laws.
        </Typography>
        <Typography variant="body1" paragraph>
          The MIT License grants you specific rights to use, modify, and
          distribute the software as outlined in Section 2. These rights do not
          extend to unauthorized use of trademarks, service marks, or trade
          dress.
        </Typography>
      </LegalSection>

      {/* Section 9: Third-Party Services */}
      <LegalSection title="9. Third-Party Services" id="third-party">
        <Typography variant="body1" paragraph>
          Our service may contain links to third-party websites or services
          (such as OAuth providers like Google, Microsoft, or GitHub) that are
          not owned or controlled by Heimdallr.
        </Typography>
        <Typography variant="body1" paragraph>
          We have no control over and assume no responsibility for the content,
          privacy policies, or practices of any third-party websites or
          services. You acknowledge and agree that Heimdallr shall not be
          responsible or liable for any damage or loss caused by your use of
          any such content, goods, or services available through third-party
          sites or services.
        </Typography>
      </LegalSection>

      {/* Section 10: Changes to Terms */}
      <LegalSection title="10. Changes to Terms" id="changes">
        <Typography variant="body1" paragraph>
          We reserve the right to modify or replace these Terms of Service at
          any time at our sole discretion. If a revision is material, we will
          provide at least 30 days' notice prior to any new terms taking effect.
        </Typography>
        <Typography variant="body1" paragraph>
          What constitutes a material change will be determined at our sole
          discretion. By continuing to access or use our service after those
          revisions become effective, you agree to be bound by the revised
          terms.
        </Typography>
      </LegalSection>

      {/* Section 11: Governing Law */}
      <LegalSection title="11. Governing Law" id="governing-law">
        <Typography variant="body1" paragraph>
          These Terms shall be governed by and construed in accordance with the
          laws applicable in your jurisdiction, without regard to its conflict
          of law provisions.
        </Typography>
        <Typography variant="body1" paragraph>
          Any disputes arising from these terms or your use of the service will
          be resolved through binding arbitration, except where prohibited by
          law.
        </Typography>
      </LegalSection>

      {/* Section 12: Contact Information */}
      <LegalSection title="12. Contact Information" id="contact">
        <Typography variant="body1" paragraph>
          If you have any questions about these Terms of Service, please
          contact us at:
        </Typography>
        <Box sx={{ pl: 2 }}>
          <Typography variant="body1" paragraph>
            <strong>Email:</strong>{' '}
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
      </LegalSection>

      {/* Final Statement */}
      <Box sx={{ mt: 6, p: 3, backgroundColor: 'grey.50', borderRadius: 2 }}>
        <Typography
          variant="body2"
          sx={{ color: 'text.secondary', fontStyle: 'italic' }}
        >
          By using Heimdallr, you acknowledge that you have read and understood
          these Terms of Service and agree to be bound by them. Thank you for
          using our authentication service.
        </Typography>
      </Box>
    </LegalLayout>
  );
};

TermsOfServicePage.displayName = 'TermsOfServicePage';
