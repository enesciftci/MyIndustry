# AGENTS.md — MyIndustry (Backend)

Instructions for AI agents working on this repository. Follow existing architecture. Prefer simplicity and safety over novelty.

## Architecture

Polyrepo backend for MyIndustry. Sibling frontend: `../MyIndustry.UI`.

| Area | Projects |
|------|----------|
| Main API | `MyIndustry.Api` → `ApplicationService` (MediatR) → `Domain` → `Repository` (EF Core) |
| Cross-cutting | `MyIndustry.Container` (JWT, CORS, Serilog, middleware) |
| Identity | `MyIndustry.Identity.Api` / `Domain` / `Repository` |
| Gateway | `MyIndustry.Gateway` (Ocelot) |
| Messaging | `MyIndustry.Queue`, `Queue.Message`, communicators (Redis, RabbitMQ, Core API) |
| Tests | `MyIndustry.Tests` (`Unit/`, `Smoke/`, `Integration/`) |

Patterns: CQRS via MediatR handlers, repository + unit of work, versioned MVC controllers (`api/v{version}/...`), unified `ResponseBase` envelope.

## Backend rules

- Put business use cases in `MyIndustry.ApplicationService/Handler/{Feature}/{Operation}/`.
- Keep domain logic in `MyIndustry.Domain`; infrastructure in `Repository` / communicators.
- Controllers stay thin: map HTTP → MediatR → `CreateResponse`.
- Prefer existing helpers in `MyIndustry.Container` over new cross-cutting code.
- Do not introduce Clean Architecture renames or new layers without an explicit task.

## Naming conventions

- Commands/Queries: `{OperationName}.cs`, `{OperationName}Handler.cs`, `{OperationName}Result.cs`
- Controllers: `{Resource}Controller` under `Controllers/v1/`
- Tests mirror area: `Unit/`, `Smoke/`, `Integration/`

## Directory structure (do not invent parallel trees)

```text
MyIndustry.Api/
MyIndustry.ApplicationService/Handler/
MyIndustry.Domain/
MyIndustry.Repository/
MyIndustry.Container/
MyIndustry.Identity.*/
MyIndustry.Gateway/
MyIndustry.Queue*/
MyIndustry.Tests/
scripts/
docs/
.ai-loop/
```

## Testing rules

- Add or update tests for behavior you change.
- Prefer xUnit + FluentAssertions + Moq; use existing factories in `MyIndustry.Tests/Fixtures/`.
- Run targeted tests first, then `./scripts/verify`.
- **Never** delete tests, skip/disable tests, or weaken assertions to make CI green.
- Coverage target: see `TEST_COVERAGE.md` / `docs/testing.md` (backend ~80%+).

## Dependency rules

- Do not add NuGet packages unless required for the task and no existing package covers it.
- Match existing package versions in sibling projects (EF/JWT 8.0.x, etc.).
- No central package management migration unless explicitly tasked.

## Database migration rules

- Use EF Core migrations in `MyIndustry.Repository/Migrations` or Identity migrations.
- Prefer additive, reversible migrations. Never drop production data casually.
- Do not set `RESET_DATABASE=true` outside local throwaway environments.
- Do not edit applied migration files; add a new migration instead.
- Local helpers: `MyIndustry.Api/Makefile` (`ef-add-migration`, `ef-update-db`).

## API compatibility

- Keep `/api/v1/...` contracts stable unless the task requires a breaking change (document in PR).
- Preserve auth: JWT claims (`uid`, `email`, `type`), `AdminOnly`, internal API key header where used.
- Gateway routes live in Ocelot JSON; update when exposing new public paths.

## Security

- Never commit secrets, `.env`, private keys, connection strings with credentials, or cloud keys.
- Use `env.example` / Dokploy env docs as templates only.
- Do not log tokens, passwords, or PII. Use existing masking helpers where present.
- Do not weaken CORS, auth, rate limits, or AllowedHosts for convenience.

## Logging

- Use Serilog patterns already configured in the host.
- Prefer structured logs; avoid dumping full request bodies with sensitive fields.

## Error handling

- Use existing domain exceptions and API response patterns.
- Do not swallow exceptions silently in handlers.

## Configuration / secrets

- Configuration via `appsettings*.json` + environment variables (`Section__Key`).
- Never put production secrets in source. Do not load real production credentials into agent context.

## Git rules

- Work on `feature/...` (or `fix/...`) branches. **Never** commit or merge directly to `main`.
- Do not amend pushed commits or force-push `main`/`master`.
- Run `dotnet build` (and `./scripts/verify` when finishing a task) before considering work done.
- Do not commit `.ai-loop/state/*.json`.

## PR rules

- Use the PR template. Include verification output from `./scripts/verify` or CI.
- Call out DB migrations, breaking API changes, and AI-generated scope.
- Human review is required before merge.

## AI must NOT

1. Delete, skip, or disable tests
2. Change assertions only to pass tests
3. Add unnecessary dependencies or frameworks
4. Refactor unrelated architecture “while here”
5. Commit secrets or credentials
6. Touch production resources / Dokploy live config from the agent
7. Irreversibly alter databases (drop/reset prod or shared data)
8. Push or merge to `main`
9. Expand scope far beyond the issue/task
10. Disable CI checks or use `--no-verify` to bypass hooks

## Verification

```bash
./scripts/verify          # local default
./scripts/ci              # CI parity (coverage)
```

See `docs/ai-loop.md` and `docs/ai-agent-guide.md`.

## Sibling frontend

UI changes belong in `MyIndustry.UI`. Cross-cutting API+UI tasks need coordinated PRs in both repos.
