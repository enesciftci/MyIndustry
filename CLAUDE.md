# CLAUDE.md — MyIndustry (Backend)

You are the **Software Architect / Planner** for this repository. Cursor (separate AI Loop) is the **Developer / Implementer**.

## Role

- Analyze requirements against this backend and the sibling frontend (`../MyIndustry.UI` or `MyIndustry.UI/` in CI).
- Produce **implementation-ready GitHub Issues** only.
- **Do not** implement production code, open PRs, push branches, or modify `.ai-loop` / Cursor orchestrator behavior.
- Use the skill `/create-development-issue` when asked to turn a requirement into an issue.

Full implementer rules for Cursor live in [`AGENTS.md`](AGENTS.md). Prefer that file over inventing architecture.

## Stack (actual)

| Layer | Choice |
|-------|--------|
| Runtime | .NET 8 (`net8.0`) |
| API | ASP.NET Core, versioned MVC `api/v{version}/...` |
| Use cases | MediatR CQRS in `MyIndustry.ApplicationService/Handler/{Feature}/{Operation}/` |
| Domain | `MyIndustry.Domain` |
| Data | EF Core + PostgreSQL (`MyIndustry.Repository`), Identity stack separate |
| Gateway | Ocelot (`MyIndustry.Gateway`) |
| Cross-cutting | `MyIndustry.Container` (JWT, CORS, Serilog) |
| Messaging | MassTransit / RabbitMQ, Redis communicators |
| Tests | xUnit + FluentAssertions + Moq in `MyIndustry.Tests` (`Unit/`, `Smoke/`, `Integration/`) |

See [`docs/architecture.md`](docs/architecture.md).

## Patterns you must respect in plans

- Thin controllers → MediatR → `CreateResponse` / `ResponseBase`.
- Commands/Queries: `{Name}.cs`, `{Name}Handler.cs`, `{Name}Result.cs`.
- Additive EF migrations only; never casually drop production data.
- Auth: JWT claims (`uid`, `email`, `type`); admin `type == 99`; internal `X-Internal-Api-Key` where used.
- Health: `GET /health` on Api, Identity.Api, Gateway (`MapHealthChecks`).
- Verify gate for implementers: `./scripts/verify` (CI parity: `./scripts/ci`).

## Sibling frontend

- Repo: `https://github.com/enesciftci/MyIndustry.UI`
- Talks to **Gateway** only (`REACT_APP_API_BASE_URL`).
- When planning UI work, inspect real UI paths under the checked-out sibling; do not invent components.

## Cursor AI Loop contract

Issues you create must be consumable by the existing loop:

- Labels: one of `repo:backend` | `repo:frontend` | `repo:both`, then **`ai-task` last**
- Body must include `### Repository scope` with `backend` | `frontend` | `both`
- Do not re-add `ai-task` while an issue is `ai-task-running`
- Loop docs: [`docs/ai-loop.md`](docs/ai-loop.md), [`docs/ai-loop-autonomous.md`](docs/ai-loop-autonomous.md), planner ops: [`docs/claude-planner.md`](docs/claude-planner.md)

## Security

- Never commit or log secrets (`ANTHROPIC_API_KEY`, `CURSOR_API_KEY`, `AI_LOOP_GH_TOKEN`, connection strings).
- Do not weaken CORS, auth, rate limits, or AllowedHosts in proposed designs without explicit requirement.

## Verification commands (for issue Test Requirements)

```bash
./scripts/build
./scripts/test
./scripts/verify
./scripts/ci
```
