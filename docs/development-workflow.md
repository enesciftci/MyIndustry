# Development Workflow — MyIndustry Backend

## Preferred flow

```text
main
  → feature/<short-name>  (or fix/<short-name>)
  → implement + ./scripts/verify
  → Pull Request
  → GitHub Actions
  → Human review
  → merge
```

AI agents must **never** commit or merge directly to `main`.

## Task intake

1. Create a GitHub Issue with the **AI Task** template (Acceptance Criteria required).
2. Agent (or human) branches from up-to-date `main`.
3. Follow phases in `docs/ai-agent-guide.md`.
4. Record loop state under `.ai-loop/state/` (gitignored).
5. Open PR with the PR template; paste verification summary.

## Local verification

```bash
./scripts/verify
./scripts/ci          # before relying on CI parity / coverage
dotnet build          # minimum before commit (Cursor rule)
```

## Manual GitHub configuration (repo settings)

These cannot be applied from the codebase alone. A human with admin access should configure:

1. **Branch protection on `main`**
   - Require a pull request before merging
   - Require status checks to pass (the backend test job from `tests.yml`)
   - Require conversation resolution (optional but recommended)
   - Do not allow bypass for admins in production teams if possible
2. **Dismiss stale reviews** when new commits are pushed
3. **Block force pushes** to `main`
4. **Secret scanning** / push protection (GitHub Advanced Security or available equivalent)
5. Restrict who can merge to `main`

This document describes the intended policy; it does not claim those settings are already enabled.

## Cross-repo changes

If API contracts change, open a coordinated PR in `MyIndustry.UI` and note the dependency in both PR descriptions.

## Deploy

Deployment remains Dokploy + compose files. AI must not change production credentials or live Dokploy settings as part of a normal feature task.
