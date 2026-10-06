# AI Agent Guide — MyIndustry Backend

## Before you start

1. Read the GitHub Issue (Goal, Requirements, Acceptance Criteria).
2. Read `AGENTS.md`.
3. Load `.ai-loop/config.env` limits.
4. Init state: `./scripts/ai-loop-init --task-id <id> --task "<summary>"`

Do not include `.env`, production credentials, private keys, or SSH keys in context.

## Phase 1 — Understand

- Locate related handlers, controllers, domain types, and tests.
- Check migrations if data model changes are implied.
- Note API compatibility and security surface.
- List risks (auth, data loss, breaking clients).

## Phase 2 — Plan

Write a short plan (4–8 steps) before editing. Keep scope to the issue.

## Phase 3 — Implement

- Touch only necessary files.
- Follow MediatR handler and controller patterns.
- Add/update tests for new behavior.

## Phase 4 — Verify

```bash
# targeted
TEST_FILTER="FullyQualifiedName~YourArea" ./scripts/test

# full
./scripts/verify
```

## Phase 5 — Self-correct

On failure:

1. Read failure output.
2. Determine root cause.
3. Fix code (not the test assertion unless the test was wrong and Acceptance Criteria agree).
4. Record: `./scripts/ai-loop-record --task-id <id> --status failed --failure "..." --next-action "..."`
5. Respect `MAX_ITERATIONS` / retry limits. Stop if the same failure repeats without a new hypothesis.

## Phase 6 — Final verification

```bash
./scripts/ci
```

Do not mark the task done until this passes (or CI equivalent is green).

## Phase 7 — PR

- Branch: `feature/...` from `main`
- Fill PR template (summary, why, tests, verification, risks, DB/breaking/AI checklist)
- Never merge yourself

## Observability (loop logs)

Record in state (no secrets): task id, iteration, times, files changed, commands, test results, failure reason, next action, final status.

## Cost / runaway protection

Stop when any guardrail in `.ai-loop/config.env` is exceeded. Prefer fewer, correct iterations over speculative refactors.
