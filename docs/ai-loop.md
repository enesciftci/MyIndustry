# AI Loop Engineering — MyIndustry Backend

## What is Loop Engineering?

A controlled cycle where an AI agent takes a task, plans, implements, runs automated verification, and either opens a PR or fixes failures—within hard safety limits—then a human reviews and merges.

```text
Task → Analyze → Plan → Implement → Verify
         ↑                    │
         └── fix ←── FAIL ────┤
                              └── PASS → PR → Human → Merge
```

## How it works in this repository

### Autonomous path (preferred)

See [`ai-loop-autonomous.md`](./ai-loop-autonomous.md):

1. Issue created with AI Task template (label `ai-task`).
2. `.github/workflows/ai-loop.yml` starts (requires `CURSOR_API_KEY` secret).
3. Orchestrator creates `ai/<issue>-<slug>`, starts Cursor Cloud Agent via SDK.
4. Runner executes `./scripts/verify`; failures are fed back to the agent until limits.
5. On success, opens a PR — **human merges** (no auto-merge).

### Manual / IDE agent path

1. Issue created with `.github/ISSUE_TEMPLATE` (AI Task).
2. Agent reads `AGENTS.md` and relevant code.
3. Agent implements on `ai/...` or `feature/...`.
4. Agent runs `./scripts/verify` (or `./scripts/ci`).
5. Failures are recorded in `.ai-loop/state/<task-id>.json` and fixed until pass or guardrails stop the loop.
6. PR opened; GitHub Actions re-runs checks; human merges.

## Creating a task

Use the **AI Task** issue template. **Acceptance Criteria** are required. Include constraints, tests, and Definition of Done.

## How the agent works

See `docs/ai-agent-guide.md` (Understand → Plan → Implement → Verify → Self-correct → Final verify → PR).

## Verification

| Command | Purpose |
|---------|---------|
| `./scripts/build` | `dotnet build` |
| `./scripts/test` | `dotnet test` |
| `./scripts/validate` | build + test + security packages |
| `./scripts/verify` | full local gate + secret scan |
| `./scripts/ci` | CI parity with coverage |

Machine-readable lines:

```text
[PASS] Backend Build
[FAIL] Backend Tests
```

On failure, the script prints the command and exit code.

## Failure feedback

1. Read the failing stage and logs.
2. Identify root cause (do not blindly retry the same change).
3. Fix code or tests appropriately (never weaken assertions).
4. Re-run verification.
5. Append attempt to loop state via `scripts/ai-loop-record`.

## Maximum iterations

Configured in `.ai-loop/config.env`:

- `MAX_ITERATIONS=5`
- `MAX_RETRIES_PER_TEST=3`
- `MAX_EXECUTION_TIME_MINUTES=45`
- `MAX_CHANGED_FILES=40`
- `COMMAND_TIMEOUT_SECONDS=900`

When a limit is hit, stop and report; do not continue silently.

## Creating a PR

Use `.github/pull_request_template.md`. Paste verify summary. Flag AI-generated changes, DB migrations, and risks.

## Human approval

Merge to `main` only after CI green + human review. See `docs/development-workflow.md` for branch protection checklist.

## What AI must not do

Listed in `AGENTS.md` (no test deletion, no secrets, no direct `main`, no prod DB resets, no scope creep).

## Local vs CI

| | Local `./scripts/verify` | CI `./scripts/ci` |
|--|--------------------------|-------------------|
| Coverage | optional | collected + artifact |
| Network security scan | best-effort (may SKIP) | attempted |
| Environment | developer machine | `ubuntu-latest`, .NET 8 |

Always treat CI as the merge gate even if local verify passed.
