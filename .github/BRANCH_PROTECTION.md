# Branch Protection Rules Setup Guide

## Overview

This guide explains how to configure branch protection rules on GitHub to enforce the Git workflow and maintain code quality.

## Why Branch Protection?

Branch protection rules ensure:
- ✅ All changes go through Pull Requests
- ✅ Code is reviewed before merging
- ✅ CI/CD checks pass before merging
- ✅ Code coverage requirements are met
- ✅ No accidental direct commits to protected branches
- ✅ Consistent code quality across the team

## Setting Up Protection Rules

### Step 1: Access Repository Settings

1. Navigate to your GitHub repository
2. Click **Settings** tab
3. Click **Branches** in the left sidebar
4. Click **Add branch protection rule**

---

## Main Branch Protection

### Configuration for `main` Branch

**Branch name pattern**: `main`

#### Protection Settings

##### Pull Request Requirements
- ✅ **Require a pull request before merging**
  - ✅ **Require approvals**: `1` (minimum)
  - ✅ **Dismiss stale pull request approvals when new commits are pushed**
  - ✅ **Require review from Code Owners** (optional, if you have CODEOWNERS file)

##### Status Check Requirements
- ✅ **Require status checks to pass before merging**
  - ✅ **Require branches to be up to date before merging**
  - **Status checks that are required**:
    - ✅ `build-test-coverage` (from GitHub Actions CI/CD)
    - Add this after first workflow run

##### Commit Restrictions
- ✅ **Require signed commits** (recommended, optional)
- ✅ **Require linear history** (optional - for clean history)

##### Force Push & Deletion
- ✅ **Do not allow bypassing the above settings**
- ✅ **Restrict who can push to matching branches** (optional)
- ❌ **Allow force pushes** - Keep DISABLED
- ❌ **Allow deletions** - Keep DISABLED

##### Rules Applied To
- ✅ **Include administrators** - Admins also must follow rules

### Visual Guide for Main Branch

```
Repository → Settings → Branches → Add Rule

Branch name pattern: main

☑ Require a pull request before merging
  ☑ Require approvals: 1
  ☑ Dismiss stale pull request approvals when new commits are pushed
  ☐ Require review from Code Owners (optional)

☑ Require status checks to pass before merging
  ☑ Require branches to be up to date before merging
  Search for required status checks:
    ☑ build-test-coverage

☐ Require conversation resolution before merging (optional)
☐ Require signed commits (optional)
☐ Require linear history (optional)

☑ Do not allow bypassing the above settings
☐ Restrict who can push to matching branches (optional)
☐ Allow force pushes - KEEP DISABLED
☐ Allow deletions - KEEP DISABLED

☑ Include administrators

[Create] or [Save changes]
```

---

## Development Branch Protection

### Configuration for `development` Branch

**Branch name pattern**: `development`

#### Protection Settings

##### Pull Request Requirements
- ✅ **Require a pull request before merging**
  - **Require approvals**: `0` or `1` (more lenient than main)
  - ☐ Dismiss stale approvals (optional)

##### Status Check Requirements
- ✅ **Require status checks to pass before merging**
  - ✅ **Require branches to be up to date before merging**
  - **Status checks that are required**:
    - ✅ `build-test-coverage`

##### Force Push & Deletion
- ❌ **Allow force pushes** - Keep DISABLED
- ❌ **Allow deletions** - Keep DISABLED

### Visual Guide for Development Branch

```
Branch name pattern: development

☑ Require a pull request before merging
  Require approvals: 0 (or 1 for stricter workflow)

☑ Require status checks to pass before merging
  ☑ Require branches to be up to date before merging
  Search for required status checks:
    ☑ build-test-coverage

☐ Allow force pushes - KEEP DISABLED
☐ Allow deletions - KEEP DISABLED

[Create] or [Save changes]
```

---

## Feature Branch Rules (Optional)

### Configuration for `feature/*`, `bugfix/*`, `hotfix/*`

These branches typically don't need protection rules as they are short-lived and merge into `development`.

However, you can optionally add:

**Branch name pattern**: `feature/*` (repeat for `bugfix/*` and `hotfix/*`)

- ☐ No specific protection needed
- Feature branches can be deleted after merge
- Force push is allowed on feature branches (for rebasing)

---

## Rulesets (Modern Alternative)

GitHub offers a newer "Rulesets" feature that provides more flexibility:

### To Use Rulesets:

1. Go to **Settings → Rules → Rulesets**
2. Click **New ruleset → New branch ruleset**
3. Configure similar to branch protection rules above

### Advantages of Rulesets:
- More flexible targeting
- Can apply to multiple branches with patterns
- Better organization
- More granular permissions

---

## Status Checks Configuration

### Adding Required Status Checks

Status checks will only appear after they've run at least once:

1. **First-time setup**:
   - Create a test branch
   - Push to trigger CI/CD workflow
   - Let the workflow complete
   - The status check `build-test-coverage` will now be available

2. **Add to branch protection**:
   - Go back to branch protection rules
   - Search for `build-test-coverage` in status checks
   - Select it as required

### Custom Status Checks

You can add multiple status checks:
- `build-test-coverage` - Main CI/CD pipeline
- `security-scan` - If you add security scanning
- `lint` - If you add linting checks
- `deploy-preview` - If you add preview deployments

---

## CODEOWNERS File (Optional)

Create a `.github/CODEOWNERS` file to automatically request reviews:

```
# CODEOWNERS file

# Default owners for everything
* @your-github-username

# Specific ownership for layers
/src/GenericAuth.Domain/ @domain-experts
/src/GenericAuth.Infrastructure/ @infrastructure-team
/.github/ @devops-team

# Documentation
*.md @documentation-team

# CI/CD
/.github/workflows/ @devops-team
```

Then enable in branch protection:
- ✅ **Require review from Code Owners**

---

## Testing Branch Protection

### Verify Protection is Working

1. **Test direct push to main** (should fail):
```bash
git checkout main
git commit --allow-empty -m "test"
git push origin main
# Should get: "protected branch hook declined"
```

2. **Test PR requirement**:
```bash
# Create feature branch
git checkout -b feature/test-protection
git commit --allow-empty -m "test"
git push origin feature/test-protection
# Create PR on GitHub - should be able to create
# Try to merge without approval/checks - should fail
```

3. **Test status check requirement**:
   - Create PR with failing tests
   - Verify it cannot be merged
   - Fix tests
   - Verify it can now be merged

---

## Common Configurations

### Strict Configuration (Large Teams)
```
Main Branch:
- Require 2 approvals
- Require Code Owners review
- Require all status checks
- Require branches to be up to date
- Require linear history
- Include administrators

Development Branch:
- Require 1 approval
- Require all status checks
- Require branches to be up to date
```

### Moderate Configuration (Small Teams)
```
Main Branch:
- Require 1 approval
- Require status checks
- Require branches to be up to date
- Include administrators

Development Branch:
- Require status checks
- No approval required (trust team)
```

### Minimal Configuration (Solo/Learning)
```
Main Branch:
- Require status checks only
- No approvals (for solo development)

Development Branch:
- Require status checks only
```

---

## Troubleshooting

### Can't Merge PR - Status Check Not Found

**Problem**: Required status check doesn't appear

**Solution**:
1. Ensure workflow has run at least once
2. Check workflow name matches exactly
3. Wait a few minutes and refresh

### Can't Add Status Check to Protection

**Problem**: Status check not in dropdown

**Solution**:
1. Trigger workflow by pushing to any branch
2. Wait for workflow to complete
3. Go back to branch protection settings
4. Search for status check name (case-sensitive)

### Accidentally Locked Out of Main

**Problem**: Can't merge your own PRs due to approval requirement

**Solution** (if you're an admin):
1. Temporarily disable "Include administrators"
2. Merge your PR
3. Re-enable "Include administrators"

Or better: Have another team member review

### Status Check Always Failing

**Problem**: CI/CD pipeline always fails

**Solution**:
1. Check GitHub Actions logs
2. Run tests locally first
3. Ensure code coverage ≥ 80%
4. Fix issues before pushing

---

## Monitoring Protected Branches

### View Protection Status

1. Go to **Settings → Branches**
2. See all protected branches with rules
3. Edit or delete rules as needed

### Audit Protection Changes

1. Go to **Settings → Audit log** (for organizations)
2. Filter by "protected_branch" events
3. See who changed what and when

---

## Best Practices

### Do's ✅
- ✅ Protect `main` branch with strict rules
- ✅ Protect `development` with moderate rules
- ✅ Require status checks on all protected branches
- ✅ Apply rules to administrators
- ✅ Require branches to be up to date
- ✅ Review and update rules periodically

### Don'ts ❌
- ❌ Don't allow force push to `main` or `development`
- ❌ Don't allow deletion of `main` or `development`
- ❌ Don't bypass protection rules regularly
- ❌ Don't skip status checks for "quick fixes"
- ❌ Don't disable protection temporarily without good reason

---

## Migration Guide

If you already have code in `main` without protection:

1. **Create development branch**:
```bash
git checkout main
git pull origin main
git checkout -b development
git push origin development
```

2. **Set up protection on GitHub** (as described above)

3. **Update default branch** (optional):
   - Settings → Branches → Default branch
   - Change from `main` to `development`
   - Update (will warn about impacts)

4. **Notify team**:
   - Send message about new workflow
   - Share Git Workflow documentation
   - Provide training if needed

---

## Additional Resources

- [GitHub Branch Protection Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches)
- [GitHub Rulesets Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets)
- [CODEOWNERS Documentation](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners)

---

## Support

For questions about branch protection:
1. Check this guide
2. Consult Git Workflow documentation
3. Ask team lead or admin
4. Open an issue in the repository
