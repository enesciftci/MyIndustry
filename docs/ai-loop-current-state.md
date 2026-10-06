# AI Loop — Current State (Backend)

Snapshot of the MyIndustry backend repository before / as AI Loop Engineering was introduced.

## Repository layout

- **Type:** Polyrepo (backend only). Frontend: sibling `MyIndustry.UI`.
- **Solution:** `MyIndustry.sln` (.NET 8)
- **Deploy:** Dokploy + `docker-compose.dokploy.yaml` (not GitHub deploy)

## Tech stack

| Layer | Choice |
|-------|--------|
| Runtime | .NET 8 / ASP.NET Core |
| API | Versioned MVC controllers |
| Application | MediatR CQRS handlers |
| Data | EF Core + PostgreSQL (Npgsql) |
| Auth | JWT + ASP.NET Identity (Identity API) |
| Gateway | Ocelot |
| Messaging | MassTransit + RabbitMQ |
| Cache | Redis |
| Logging | Serilog |

## Tests

| Kind | Location | Runner |
|------|----------|--------|
| Unit | `MyIndustry.Tests/Unit/` | xUnit |
| Smoke | `MyIndustry.Tests/Smoke/` | WebApplicationFactory |
| Integration | `MyIndustry.Tests/Integration/` | InMemory EF |
| E2E | Frontend repo (Playwright) | N/A here |

Commands: `dotnet build`, `dotnet test MyIndustry.Tests/MyIndustry.Tests.csproj`.

## Build / lint / static analysis

- Build: `dotnet build`
- No `.editorconfig` / StyleCop / Sonar enforced in-repo
- “Static analysis” for the loop = successful build (+ nullable where enabled)

## Docker

- `compose.yaml` — local Postgres, Redis, RabbitMQ, API, Identity, Gateway, Queue
- Per-service Dockerfiles; root Dockerfile builds API
- See `COMPOSE.md`

## Configuration

- `appsettings.json` / `appsettings.Development.json`
- Production host allowlists under `MyIndustry.Container/`
- Template: `env.example`
- Secrets via environment / Dokploy (see `DOKPLOY-ENV.md`)

## CI/CD (pre-loop)

- `.github/workflows/tests.yml`: push/PR to `main`/`develop` → restore, build, test + coverage
- No Issue/PR templates
- No unified `./scripts/verify`

## Documentation (pre-loop)

- `TESTING.md`, `TEST_COVERAGE.md`, `SECURITY.md`, `COMPOSE.md`, `DOKPLOY-ENV.md`
- No root `README.md` / `AGENTS.md` / `docs/` AI loop guides

## Coding conventions

- Cursor rule: build before commit (`.cursor/rules/build-before-commit.mdc`)
- Handler folder convention under ApplicationService

## Gaps addressed by AI Loop Engineering

1. Agent onboarding (`AGENTS.md`)
2. Single verification entry point (`scripts/verify`)
3. Loop state + guardrails (`.ai-loop/`)
4. Issue / PR templates
5. Stronger CI (dispatch, artifacts, security/secret scan)
6. Loop documentation under `docs/`
