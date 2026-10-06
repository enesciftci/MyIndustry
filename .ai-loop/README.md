# `.ai-loop` — Backend loop state

## Files

| Path | Purpose |
|------|---------|
| `config.env` | Guardrails (committed) |
| `templates/task-state.json` | Schema example (committed) |
| `state/<task-id>.json` | Live task state (**gitignored**) |
| `orchestrator/` | GitHub Action outer loop (Cursor SDK + verify) |
| `logs/` | Verify / orchestrator logs (**gitignored**) |

## State schema

```json
{
  "task": "Short task summary",
  "task_id": "issue-123",
  "iteration": 0,
  "status": "pending",
  "started_at": "",
  "ended_at": "",
  "changes": [],
  "tests": [],
  "failures": [],
  "attempts": [],
  "commands": [],
  "next_action": "",
  "final_status": ""
}
```

## Commands

```bash
./scripts/ai-loop-init --task-id issue-123 --task "Add seller filter"
./scripts/ai-loop-record --task-id issue-123 --status failed --failure "..." --next-action "fix handler"
```

Never log secrets, tokens, or credentials into state files.
