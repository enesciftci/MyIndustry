# GitHub Issue contract (Cursor-compatible)

## Required body sections

```markdown
<!-- claude-planner-fingerprint: HASH -->

### Repository scope
backend | frontend | both

# Goal

# Context

# Existing Architecture

# Proposed Implementation

## Backend

## Frontend

## Database

## API Contract

# Acceptance Criteria

# Test Requirements

# Constraints

# Non-goals

# Definition of Done
```

## Scope labels (existing model — do not invent new ones)

| Scope | Labels (order) |
|-------|----------------|
| Backend only | `claude-planner`, `repo:backend`, then `ai-task` |
| Frontend only | `claude-planner`, `repo:frontend`, then `ai-task` |
| Both | `claude-planner`, `repo:both`, then `ai-task` |

Apply **`ai-task` last** so the Cursor loop starts only after the issue is complete.

## Issue repository

| Scope | Create issue in |
|-------|-----------------|
| `backend` or `both` | `enesciftci/MyIndustry` (or current backend repo) |
| `frontend` | `enesciftci/MyIndustry.UI` |

## Quality bar

- Implementation-ready: modules, handlers/endpoints, entities, UI pages, tests named concretely from repo inspection
- Never a one-liner like “Add favorites”
- Acceptance criteria must be checkable; include `./scripts/verify` (and UI verify when in scope)
- Definition of Done includes human review (no auto-merge)
