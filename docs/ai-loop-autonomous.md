# Autonomous AI Loop — Backend

This document describes the **orchestrated** loop (GitHub Action + Cursor SDK Cloud Agent) built on top of `./scripts/verify` and `.ai-loop/*`.

## Flow

```text
Issue (+ label ai-task)
  → GitHub Action (.github/workflows/ai-loop.yml)
  → Cursor Cloud Agent starts from main (workOnCurrentBranch=false)
  → Cursor creates its own branch (cursor/...)
  → Orchestrator reads result.git.branches[] into .ai-loop/state
  → GHA runner checks out that branch and runs ./scripts/verify
  → FAIL → same agent + same branch → Agent.send (self-correct) → verify again
  → PASS → gh pr create head=<cursor-branch> base=main (prefer AI_LOOP_GH_TOKEN)
  → Independent GitHub CI (tests.yml) on the PR — separate from agent verify
```

Do **not** pre-create `ai/<issue>-*` for Cloud Agent (Cursor validates that ref and often fails).

Outer loop code: [`.ai-loop/orchestrator/`](../.ai-loop/orchestrator/).

## Agent verify vs GitHub CI

| Layer | Role |
|-------|------|
| Agent `./scripts/verify` | Pre-PR quality gate inside the loop |
| GitHub `tests.yml` (`./scripts/ci`) | Independent merge gate on the PR |

Agent verify **PASS** does not replace PR CI.

## Self-correction harness

Set `AI_LOOP_SELF_CORRECTION_TEST=1` (workflow_dispatch input `self_correction_test`, or label `ai-self-correction-test`).

On the **first** verify for that task, the orchestrator injects a deterministic failure (`CONTROLLED_SELF_CORRECTION_TEST`), records `controlled_failure_injected` / `failure_injected` in `.ai-loop/state/<task-id>.json`, then runs the normal fail → fix → verify path. Later verifies run real `./scripts/verify`.

This does **not** affect normal local `./scripts/verify` or production deploys when the env flag is unset.

Local mock + harness:

```bash
cd .ai-loop/orchestrator
AI_LOOP_MOCK_AGENT=1 AI_LOOP_SKIP_GH=1 AI_LOOP_DRY_GIT=1 \
  AI_LOOP_SELF_CORRECTION_TEST=1 \
  ISSUE_NUMBER=998 REPO_KIND=backend \
  node src/run-loop.mjs
```

Expect: iteration 1 controlled fail → iteration 2 real verify pass; same mock agent id and `cursor/mock-issue-*` branch.

## AUTOMATED

| Step | How |
|------|-----|
| Trigger on `ai-task` label | `ai-loop.yml` `on: issues: types: [labeled]` |
| Concurrency per issue | `concurrency.group: ai-loop-<repo>-<issue>` |
| Label swap to prevent re-entry | `ai-task` → `ai-task-running` → `ai-task-done` / `ai-task-failed` |
| Branch `cursor/...` | Created by Cursor Cloud Agent; discovered via `result.git.branches` |
| Cloud agent start / multi-turn | `@cursor/sdk` `Agent.create({ startingRef: main, workOnCurrentBranch: false })` + same-agent `send` |
| Verification | Existing `./scripts/verify` on the GHA runner |
| Iteration / same-failure limits | `.ai-loop/config.env` + orchestrator fingerprints |
| State recording | `scripts/ai-loop-init` / `ai-loop-record` |
| PR creation | `gh pr create` with Loop Result body |
| PR CI ensure | Prefer PAT; else `workflow_dispatch` fallback on `tests.yml` |
| CI failure comment on AI PRs | `ai-loop-ci-feedback.yml` (`ai/` and `cursor/` heads) |

## MANUAL CONFIGURATION REQUIRED

1. Repository secret **`CURSOR_API_KEY`** (Dashboard → Integrations / service account). Never commit it.
2. Cursor account behind the key must have **GitHub connected** with access to this repo.
3. Create labels: `ai-task`, `ai-task-running`, `ai-task-done`, `ai-task-failed`, `ai-self-correction-test`, `repo:backend`, `repo:frontend`, `repo:both`.
4. **PR create + PR CI:** add secret **`AI_LOOP_GH_TOKEN`** (PAT with `contents` + `pull_requests` + `actions:write` for dispatch). Default `GITHUB_TOKEN` may create a PR but **does not trigger** other workflows (`pull_request` CI suppressed). Optionally also enable Settings → Actions → General → *Allow GitHub Actions to create and approve pull requests*.
5. Optional cross-repo: repo variable `AI_LOOP_UI_REPO` (`owner/MyIndustry.UI`) + same PAT with access to UI.
6. Branch protection on `main`: require PR + status check **Run Tests**. **Do not** enable auto-merge for AI PRs.
7. **Do not** rely on Cursor Automations “Issue label” UI. This workflow uses GitHub Actions instead.

## Two-repo

| Scope | Behavior |
|-------|----------|
| `backend` / `repo:backend` | This repo’s loop only |
| `frontend` / `repo:frontend` | Skipped here; run in UI repo |
| `both` / `repo:both` | Backend loop first; then dispatch UI `ai-loop.yml` if PAT configured |

## Local mock (no API key)

```bash
cd .ai-loop/orchestrator
npm ci
AI_LOOP_MOCK_AGENT=1 AI_LOOP_SKIP_GH=1 AI_LOOP_DRY_GIT=1 \
  ISSUE_NUMBER=999 REPO_KIND=backend \
  node src/run-loop.mjs
```

## Security

- `CURSOR_API_KEY` only via Actions `env` from secrets
- Never echo the key; logs may print length only
- No merge to `main` from the loop

## Related

- Protocol / verify: [`ai-loop.md`](./ai-loop.md)
- Agent phases: [`ai-agent-guide.md`](./ai-agent-guide.md)
