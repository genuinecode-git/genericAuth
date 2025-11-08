# Git Workflow & Branching Strategy

## Overview

This project follows a **Git Flow** branching strategy with pull request workflow to ensure code quality and maintain a stable `main` branch.

## Branch Structure

```
main (protected)
├── development (default branch for development)
│   ├── feature/feature-name
│   ├── bugfix/bug-name
│   └── hotfix/hotfix-name
```

### Branch Descriptions

#### **`main`** (Protected)
- **Purpose**: Production-ready code only
- **Protection**: Direct commits disabled
- **Merges**: Only via Pull Requests from `development`
- **CI/CD**: Full pipeline runs on every push
- **Status**: Should always be deployable

#### **`development`** (Default Development Branch)
- **Purpose**: Integration branch for features
- **Protection**: Recommended to protect
- **Merges**: Features merge here first
- **CI/CD**: Full pipeline runs on every push
- **Status**: Latest development changes

#### **`feature/*`** (Feature Branches)
- **Purpose**: New features and enhancements
- **Naming**: `feature/feature-name` (e.g., `feature/user-authentication`)
- **Base**: Created from `development`
- **Merge to**: `development` via Pull Request
- **Lifetime**: Deleted after merge

#### **`bugfix/*`** (Bug Fix Branches)
- **Purpose**: Bug fixes for development
- **Naming**: `bugfix/bug-description` (e.g., `bugfix/login-error`)
- **Base**: Created from `development`
- **Merge to**: `development` via Pull Request
- **Lifetime**: Deleted after merge

#### **`hotfix/*`** (Hotfix Branches)
- **Purpose**: Critical production fixes
- **Naming**: `hotfix/issue-description` (e.g., `hotfix/security-patch`)
- **Base**: Created from `main`
- **Merge to**: Both `main` AND `development`
- **Lifetime**: Deleted after merge

---

## Workflow Process

### 1. Starting New Work

#### For Features or Bug Fixes

```bash
# Make sure you're on development and up to date
git checkout development
git pull origin development

# Create a new feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b bugfix/bug-description
```

#### For Hotfixes

```bash
# Start from main for critical production fixes
git checkout main
git pull origin main

# Create hotfix branch
git checkout -b hotfix/critical-issue
```

---

### 2. Working on Your Branch

```bash
# Make changes to your code
# ...

# Stage changes
git add .

# Commit with meaningful message
git commit -m "feat: Add user authentication feature"

# Push to remote
git push origin feature/your-feature-name
```

#### Commit Message Convention

Follow **Conventional Commits** format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `perf`: Performance improvements

**Examples:**
```bash
git commit -m "feat(auth): Add JWT token generation"
git commit -m "fix(api): Resolve null reference in user controller"
git commit -m "docs: Update API documentation"
git commit -m "test: Add unit tests for password hasher"
```

---

### 3. Creating a Pull Request

#### Step 1: Push Your Branch
```bash
git push origin feature/your-feature-name
```

#### Step 2: Create PR on GitHub
1. Go to repository on GitHub
2. Click **"Pull requests"** tab
3. Click **"New pull request"**
4. Select:
   - **Base**: `development` (for features/bugfixes)
   - **Compare**: `feature/your-feature-name`
5. Click **"Create pull request"**

#### Step 3: Fill PR Template

**Title Format:**
```
feat: Add user authentication with JWT
```

**Description:**
```markdown
## Description
Brief description of what this PR does.

## Changes
- Added JWT token generation
- Implemented password hashing
- Created login endpoint

## Testing
- [ ] Unit tests added
- [ ] Integration tests added
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] All tests pass locally
- [ ] Coverage meets 80% threshold
- [ ] Documentation updated
- [ ] No breaking changes
```

---

### 4. PR Review Process

#### Automated Checks
Once you create a PR, GitHub Actions will automatically:
1. ✅ Build the solution
2. ✅ Run all tests
3. ✅ Check code coverage (must be ≥ 80%)
4. ✅ Generate coverage report

**PR Status Indicators:**
- 🟢 **Green checkmark**: All checks passed
- 🔴 **Red X**: One or more checks failed
- 🟡 **Yellow circle**: Checks in progress

#### Required Checks
Before merging, ensure:
- ✅ All CI/CD checks pass
- ✅ Code coverage ≥ 80%
- ✅ No merge conflicts
- ✅ At least one approval (recommended)
- ✅ All review comments addressed

#### Code Review Guidelines

**For Reviewers:**
- Check code quality and adherence to standards
- Verify test coverage for new code
- Ensure documentation is updated
- Test locally if needed
- Provide constructive feedback

**For Authors:**
- Respond to all comments
- Make requested changes
- Re-request review after updates
- Keep PR focused and small

---

### 5. Merging Pull Requests

#### Merge Strategy

Use **"Squash and merge"** for clean history:

1. Click **"Squash and merge"** button
2. Edit commit message if needed
3. Confirm merge
4. Delete branch after merge

**Merge Commit Message:**
```
feat: Add user authentication with JWT (#123)

- Implemented JWT token generation
- Added password hashing with PBKDF2
- Created login and register endpoints
- Added comprehensive unit tests
- Code coverage: 85%
```

#### Post-Merge Actions

```bash
# Switch back to development
git checkout development

# Pull latest changes
git pull origin development

# Delete local feature branch (optional)
git branch -d feature/your-feature-name

# The remote branch is automatically deleted after merge
```

---

### 6. Merging to Main (Release)

When `development` is stable and ready for release:

#### Step 1: Create PR from Development to Main

```bash
# Ensure development is up to date
git checkout development
git pull origin development
```

#### Step 2: Create Release PR
1. Go to GitHub repository
2. Create Pull Request:
   - **Base**: `main`
   - **Compare**: `development`
3. Title: `release: Version X.Y.Z`

#### Step 3: Review and Merge
- All checks must pass
- Perform final review
- Get required approvals
- Merge using **"Create a merge commit"**

#### Step 4: Tag Release
```bash
# After merging to main
git checkout main
git pull origin main

# Create version tag
git tag -a v1.0.0 -m "Release version 1.0.0"

# Push tag
git push origin v1.0.0
```

---

## Hotfix Workflow

For critical production issues:

```bash
# 1. Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-security-fix

# 2. Make fix and commit
git add .
git commit -m "fix: Resolve critical security vulnerability"
git push origin hotfix/critical-security-fix

# 3. Create PR to main
# (Follow normal PR process)

# 4. After merging to main, also merge to development
git checkout development
git pull origin development
git merge main
git push origin development
```

---

## Branch Protection Rules (Recommended)

### Main Branch Protection

Enable on GitHub: **Settings → Branches → Add rule for `main`**

Recommended settings:
- ✅ Require pull request before merging
- ✅ Require approvals (1 minimum)
- ✅ Dismiss stale approvals when new commits are pushed
- ✅ Require status checks to pass before merging
  - ✅ `build-test-coverage` (GitHub Actions)
- ✅ Require branches to be up to date before merging
- ✅ Require conversation resolution before merging
- ✅ Include administrators
- ✅ Restrict who can push to matching branches
- ✅ Allow force pushes: **NO**
- ✅ Allow deletions: **NO**

### Development Branch Protection

Enable on GitHub: **Settings → Branches → Add rule for `development`**

Recommended settings:
- ✅ Require pull request before merging
- ✅ Require status checks to pass before merging
  - ✅ `build-test-coverage`
- ✅ Require branches to be up to date before merging
- ✅ Allow force pushes: **NO**

---

## Best Practices

### 1. Keep Branches Updated

```bash
# Regularly sync your feature branch with development
git checkout feature/your-feature
git fetch origin
git rebase origin/development

# Resolve conflicts if any
# Then force push (only on feature branches!)
git push origin feature/your-feature --force-with-lease
```

### 2. Small, Focused Pull Requests

- ✅ **DO**: One feature/fix per PR
- ✅ **DO**: Keep PRs under 400 lines of changes
- ❌ **DON'T**: Mix multiple features in one PR
- ❌ **DON'T**: Include unrelated changes

### 3. Write Descriptive Commit Messages

```bash
# Good ✅
git commit -m "feat(auth): Add JWT token refresh mechanism

- Implement refresh token generation
- Add token rotation on refresh
- Update tests for new flow"

# Bad ❌
git commit -m "fix stuff"
git commit -m "wip"
git commit -m "updates"
```

### 4. Test Before Creating PR

```bash
# Always run locally before pushing
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

### 5. Delete Branches After Merge

- Remove merged branches to keep repository clean
- GitHub can auto-delete after merge
- Delete local branches: `git branch -d feature/name`

---

## Quick Reference

### Common Commands

```bash
# Start new feature
git checkout development
git pull origin development
git checkout -b feature/new-feature

# Update feature branch with development
git checkout feature/new-feature
git fetch origin
git rebase origin/development

# Create PR
git push origin feature/new-feature
# Then create PR on GitHub

# After PR is merged
git checkout development
git pull origin development
git branch -d feature/new-feature
```

### Branch Naming Conventions

| Type | Format | Example |
|------|--------|---------|
| Feature | `feature/description` | `feature/add-user-roles` |
| Bug Fix | `bugfix/description` | `bugfix/fix-login-error` |
| Hotfix | `hotfix/description` | `hotfix/security-patch` |
| Documentation | `docs/description` | `docs/update-readme` |
| Performance | `perf/description` | `perf/optimize-queries` |

---

## Troubleshooting

### Merge Conflicts

```bash
# If conflicts occur during rebase
git rebase origin/development

# Fix conflicts in files, then:
git add .
git rebase --continue

# Or abort rebase
git rebase --abort
```

### Accidentally Committed to Wrong Branch

```bash
# Move commits to correct branch
git checkout correct-branch
git cherry-pick <commit-hash>

# Remove from wrong branch
git checkout wrong-branch
git reset --hard HEAD~1
```

### Need to Update PR After Review

```bash
# Make changes
git add .
git commit -m "fix: Address PR review comments"
git push origin feature/your-feature

# PR automatically updates
```

---

## Resources

- [Git Flow Workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [GitHub Flow Guide](https://guides.github.com/introduction/flow/)
- [Writing Good Commit Messages](https://chris.beams.io/posts/git-commit/)

---

## Contact

For questions about the workflow, open an issue or contact the team lead.
