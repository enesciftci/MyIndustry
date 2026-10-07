# Claude Planner / Architect

Upstream of the Cursor AI Loop: **analyze → specify → GitHub Issue → `ai-task`**.

Cursor remains the implementer (`ai-loop.yml` + `.ai-loop/orchestrator`). Claude does not open implementer PRs.

```text
HUMAN REQUIREMENT (workflow_dispatch)
        ↓
Claude Planner (.github/workflows/claude-planner.yml)
        ↓
Analyze Backend + Frontend
        ↓
Implementation-ready GitHub Issue
        ↓
repo:* then ai-task (via AI_LOOP_GH_TOKEN)
        ↓
Existing Cursor AI Loop
```

## Role split

| Agent | Role |
|-------|------|
| Claude | Architect / Planner — issue only |
| Cursor | Developer / Implementer — code, verify, PR |
| Human | Review / merge |

## Workflow

- File: [`.github/workflows/claude-planner.yml`](../.github/workflows/claude-planner.yml)
- Trigger: `workflow_dispatch` input `requirement`
- Checkouts backend + sibling UI (`MyIndustry.UI/`)
- Invokes Claude Code Action (`anthropics/claude-code-action@v1`) with `/create-development-issue`
- Skill: [`.claude/skills/create-development-issue/SKILL.md`](../.claude/skills/create-development-issue/SKILL.md)

If UI checkout fails: workflow exits with **`FRONTEND_CONTEXT_UNAVAILABLE`**.

## Issue hosting

| Scope | Issue created in |
|-------|------------------|
| `backend` / `both` | this repo (`MyIndustry`) |
| `frontend` | `MyIndustry.UI` |

Labels: `claude-planner`, `repo:backend|frontend|both`, then **`ai-task` last**.

## Why `AI_LOOP_GH_TOKEN` is required

GitHub does **not** start new workflow runs for events created with the default `GITHUB_TOKEN`. Labeling with `ai-task` via `GITHUB_TOKEN` would **not** start `ai-loop.yml`.

Claude Planner therefore **requires** secret `AI_LOOP_GH_TOKEN` (PAT) for:

- Checking out the private/sibling UI repo
- Creating issues and applying labels (so `issues.labeled` fires Cursor’s loop)

## Secrets

| Secret | Purpose |
|--------|---------|
| `ANTHROPIC_API_KEY` | Claude Code Action |
| `AI_LOOP_GH_TOKEN` | UI checkout + issue/label events that trigger `ai-loop` |
| `CURSOR_API_KEY` | Unchanged — used only by Cursor `ai-loop.yml` |

Optional variable: `AI_LOOP_UI_REPO` (`owner/MyIndustry.UI`). Defaults to `{owner}/MyIndustry.UI`.

Never commit secret values. Logs may print secret **lengths** only.

## Duplicate prevention

Fingerprint = SHA-256 of normalized requirement, embedded as:

```html
<!-- claude-planner-fingerprint: HASH -->
```

Open issues with label `claude-planner` and the same fingerprint skip creation (`PLANNER_DUPLICATE_SKIPPED`) and do **not** re-apply `ai-task`.

## Project instructions

| Repo | Paths |
|------|--------|
| Backend | `CLAUDE.md`, `.claude/rules/`, `.claude/skills/` |
| Frontend | `CLAUDE.md`, `.claude/rules/`, `.claude/skills/` (mirror; no planner workflow) |

## Smoke test

1. Actions → **Claude Planner** → Run workflow with a small non-critical requirement (e.g. health endpoint analysis + test proposal).
2. Confirm issue created with fingerprint + `repo:*` + `ai-task`.
3. Confirm **AI Loop** workflow starts on that issue.
4. Re-run the same requirement → duplicate skipped.

## Related

- Cursor loop: [`ai-loop-autonomous.md`](./ai-loop-autonomous.md)
- Agent implementer guide: [`ai-agent-guide.md`](./ai-agent-guide.md)
- Backend `AGENTS.md` / Frontend `AGENTS.md`
