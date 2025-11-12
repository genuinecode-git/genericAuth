# GenericAuth Frontend

A production-ready React + TypeScript + Material-UI authentication interface.

## Features

- Complete login screen with validation
- Material-UI v5+ component library
- TypeScript strict mode
- Responsive design (mobile-first)
- Form validation (field-level & form-level)
- Loading states and error handling
- Social login support (Google, Facebook, GitHub, Microsoft)
- Password strength indicator
- Remember me functionality
- Accessible (WCAG compliant)

## Tech Stack

- **React 18** - UI library
- **TypeScript 5** - Type safety
- **Material-UI v5** - Component library
- **React Router v6** - Routing
- **Axios** - HTTP client
- **Emotion** - CSS-in-JS styling

## Project Structure

```
src/
├── theme/                    # MUI theme configuration
│   ├── palette.ts           # Color palette
│   ├── typography.ts        # Typography settings
│   ├── breakpoints.ts       # Responsive breakpoints
│   ├── components.ts        # Component overrides
│   └── theme.ts            # Main theme export
│
├── shared/                  # Shared/reusable code
│   ├── components/
│   │   ├── forms/          # Form components
│   │   │   ├── TextField/
│   │   │   ├── PasswordField/
│   │   │   └── FormContainer/
│   │   ├── buttons/        # Button components
│   │   │   └── Button/
│   │   ├── layout/         # Layout components
│   │   │   ├── AuthLayout/
│   │   │   ├── Grid/
│   │   │   └── Stack/
│   │   └── feedback/       # Feedback components
│   │       ├── Alert/
│   │       └── Loading/
│   ├── hooks/              # Custom hooks
│   │   └── useAsync.ts
│   ├── utils/              # Utility functions
│   │   └── validators.ts
│   └── types/              # Shared TypeScript types
│       └── common.types.ts
│
├── features/               # Feature modules
│   └── auth/
│       ├── types/         # Auth-specific types
│       ├── utils/         # Auth utilities
│       ├── hooks/         # Auth hooks
│       │   └── useLoginForm.ts
│       ├── services/      # API services
│       │   └── authService.ts
│       ├── components/    # Auth components
│       │   ├── LoginForm/
│       │   └── SocialLoginButtons/
│       └── pages/         # Auth pages
│           └── LoginPage.tsx
│
├── config/                # App configuration
│   └── auth.config.ts
│
├── App.tsx               # Main app component
└── index.tsx            # Entry point
```

## Getting Started

### Prerequisites

- Node.js 16+ and npm/yarn

### Installation

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Copy environment template:
```bash
cp .env.template .env
```

4. Update `.env` with your configuration:
```env
REACT_APP_API_BASE_URL=http://localhost:5000/api
REACT_APP_AUTH_TOKEN_KEY=auth_token
REACT_APP_ENABLE_SOCIAL_LOGIN=true
```

### Development

Start the development server:
```bash
npm start
```

The app will open at [http://localhost:3000](http://localhost:3000)

### Building for Production

Create production build:
```bash
npm run build
```

Build output will be in the `build/` directory.

### Testing

Run tests:
```bash
npm test
```

## Component Usage

### Using Shared Components

```typescript
import { TextField, PasswordField, Button } from '@shared/components';

// TextField with validation
<TextField
  label="Email"
  type="email"
  value={email}
  onChange={(e) => setEmail(e.target.value)}
  error={!!errors.email}
  helperText={errors.email}
/>

// PasswordField with strength indicator
<PasswordField
  label="Password"
  value={password}
  onChange={(e) => setPassword(e.target.value)}
  showStrengthIndicator
/>

// Button with loading state
<Button
  variant="contained"
  loading={isLoading}
  loadingText="Signing in..."
>
  Sign In
</Button>
```

### Using Auth Components

```typescript
import { LoginForm } from '@features/auth/components';
import { LoginResponse } from '@features/auth/types';

<LoginForm
  onSuccess={(data: LoginResponse) => {
    console.log('Login successful:', data);
    navigate('/dashboard');
  }}
  onError={(error) => {
    console.error('Login failed:', error);
  }}
  showSocialLogin
  showRememberMe
/>
```

## Theme Customization

Modify theme settings in `src/theme/`:

- **palette.ts** - Colors and color schemes
- **typography.ts** - Font settings
- **breakpoints.ts** - Responsive breakpoints
- **components.ts** - Component default styles

## API Integration

The `authService` handles all authentication API calls:

```typescript
import { authService } from '@features/auth/services/authService';

// Login
const result = await authService.login({
  email: 'user@example.com',
  password: 'password123',
});

if (result.success) {
  console.log('Token:', result.data?.token);
}
```

## Form Validation

Use built-in validators or create custom ones:

```typescript
import { validateEmail, validatePassword } from '@shared/utils/validators';

const emailValidation = validateEmail('user@example.com');
if (!emailValidation.isValid) {
  console.error(emailValidation.error);
}
```

## Custom Hooks

### useAsync

Manage async operations with loading/error states:

```typescript
import { useAsync } from '@shared/hooks';

const loginAsync = useAsync<LoginResponse>();

await loginAsync.execute(() => authService.login(credentials));

console.log(loginAsync.isPending); // loading state
console.log(loginAsync.data);      // response data
console.log(loginAsync.error);     // error if any
```

### useLoginForm

Complete login form state management:

```typescript
import { useLoginForm } from '@features/auth/hooks/useLoginForm';

const {
  email,
  password,
  errors,
  isLoading,
  handleEmailChange,
  handlePasswordChange,
  handleSubmit,
} = useLoginForm();
```

## Responsive Design

All components are mobile-first responsive:

```typescript
// Use MUI breakpoints
sx={{
  padding: { xs: 2, sm: 3, md: 4 },
  fontSize: { xs: '0.875rem', md: '1rem' },
}}
```

## Accessibility

- All interactive elements are keyboard navigable
- Proper ARIA labels and roles
- Screen reader friendly
- Focus management
- Color contrast compliance (WCAG AA)

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `REACT_APP_API_BASE_URL` | Backend API URL | `http://localhost:5000/api` |
| `REACT_APP_AUTH_TOKEN_KEY` | LocalStorage key for token | `auth_token` |
| `REACT_APP_ENABLE_SOCIAL_LOGIN` | Enable social login | `true` |

## License

MIT
