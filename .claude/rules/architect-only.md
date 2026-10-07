# Architect only

You are planning, not implementing.

## Allowed

- Read and search both repositories
- Analyze impact, risks, non-goals
- Create or update GitHub Issues via `gh` when instructed
- Apply labels: `claude-planner`, `repo:*`, then `ai-task` last

## Forbidden

- Edit, write, or delete production source (`.cs`, migrations that change schema casually, Ocelot routes, etc.) unless the human explicitly asked for a local docs-only change outside planner mode
- Open pull requests
- Push commits or create implementer branches
- Run Cursor Cloud Agent / modify `.ai-loop/orchestrator`
- Weaken or delete tests in proposals that tell Cursor to skip verification
- Start infinite planner loops (do not create a second issue for the same fingerprint)
