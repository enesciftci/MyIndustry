---
name: create-development-issue
description: >-
  Analyze MyIndustry backend and MyIndustry.UI, then create an
  implementation-ready GitHub Issue with repo scope labels and ai-task.
  Use for Claude Planner / Architect mode. Never implements production code.
allowed-tools: Read,Grep,Glob,Bash(gh *),Bash(git *),Bash(sha256sum *),Bash(shasum *),Bash(printf *),Bash(echo *),Bash(cat *)
---

# Create development issue (Architect / Planner)

You produce a **GitHub Issue** for the existing Cursor AI Loop. You do **not** implement features, open PRs, or edit production source.

## Inputs

Expect from the caller:

- `REQUIREMENT` — natural language requirement
- `FINGERPRINT` — sha256 of normalized requirement (if provided, use it; else compute)
- Workspace with backend at `.` and frontend at `MyIndustry.UI/` (or `../MyIndustry.UI`)

## Process

1. **Normalize / fingerprint**
   - If `FINGERPRINT` env or prompt value exists, use it.
   - Else: lowercase, collapse whitespace, sha256 → hex.
   - Search open issues with label `claude-planner` whose body contains `claude-planner-fingerprint: <HASH>`.
   - If found: print `PLANNER_DUPLICATE_SKIPPED #<n>` and stop (do not create another issue, do not re-apply `ai-task`).

2. **Analyze both repositories**
   - Backend: handlers, controllers, domain, migrations, tests, Gateway/Ocelot as relevant.
   - Frontend under `MyIndustry.UI/`: pages, `src/api.js`, auth, tests/e2e as relevant.
   - If frontend directory is missing: fail with `FRONTEND_CONTEXT_UNAVAILABLE` (do not invent UI architecture).
   - Cite real paths. Do not invent modules.

3. **Decide scope**
   - `backend` | `frontend` | `both` from actual impact.
   - Prefer minimal scope.

4. **Draft implementation-ready issue body** using the contract in `.claude/rules/issue-contract.md`:
   - Include fingerprint HTML comment
   - Include `### Repository scope` line
   - Backend / Frontend / Database / API Contract subsections (use “None” when N/A)
   - Concrete acceptance criteria and tests (unit/integration/e2e as appropriate)
   - Constraints, non-goals, definition of done

5. **Create the issue with `gh` (PAT must be in `GH_TOKEN`)**
   - Host repo: backend for `backend`/`both`; `--repo owner/MyIndustry.UI` for `frontend`
   - Title: concise, prefixed `[Plan] ` when helpful
   - Create with labels `claude-planner` and the matching `repo:*` **without** `ai-task` yet
   - Then: `gh issue edit <n> --add-label ai-task` **last**

6. **Stdout markers (required for CI summary)**

```text
PLANNER_ISSUE_CREATED #<n>
PLANNER_REPO <owner/name>
PLANNER_SCOPE <backend|frontend|both>
PLANNER_LABELS claude-planner,repo:<scope>,ai-task
PLANNER_SUMMARY <one line>
PLANNER_ARCHITECTURE_IMPACT <one line>
PLANNER_ACCEPTANCE <one line or short list>
PLANNER_TEST_STRATEGY <one line>
```

## Hard rules

- No production code edits
- No PR creation
- No Cursor loop reconfiguration
- Do not apply `ai-task` before the body and scope label are correct
- Do not create duplicate issues for the same fingerprint
