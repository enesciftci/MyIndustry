# Testing — MyIndustry Backend

Canonical command reference for humans and AI agents. Historical checklist: root [`TESTING.md`](../TESTING.md), [`TEST_COVERAGE.md`](../TEST_COVERAGE.md).

## Prerequisites

- .NET **8** SDK/runtime (`Microsoft.NETCore.App 8.x`). CI installs `8.0.x`.
- Repo pins SDK via [`global.json`](../global.json) (`rollForward: latestFeature`).
- `./scripts/verify` selects a DOTNET_ROOT that contains net8 (prefers `/usr/local/share/dotnet` over a broken `$HOME/.dotnet`). Do not use `DOTNET_ROLL_FORWARD=LatestMajor` for smoke tests.

## Quick start

```bash
./scripts/verify     # restore → build → tests → security (best-effort) → secret scan
./scripts/ci         # same with coverage (CI parity)
./scripts/test       # tests only
./scripts/build      # build only
```

Optional filter:

```bash
TEST_FILTER="FullyQualifiedName~Service" ./scripts/test
```

## Test pyramid

| Folder | Purpose |
|--------|---------|
| `MyIndustry.Tests/Unit/` | Handlers, middleware, helpers (Moq + FluentAssertions) |
| `MyIndustry.Tests/Smoke/` | HTTP smoke via `WebApplicationFactory` |
| `MyIndustry.Tests/Integration/` | Handler + InMemory DB flows |
| `Fixtures/`, `Helpers/` | Factories and builders |

xUnit runs serially (`xunit.runner.json`: `maxParallelThreads: 1`).

## Coverage

```bash
dotnet test MyIndustry.Tests/MyIndustry.Tests.csproj --collect:"XPlat Code Coverage"
```

Target: **80%+** backend line coverage. Update `TEST_COVERAGE.md` when adding handler/endpoint coverage.

## Frontend / E2E

Frontend unit + Playwright live in `../MyIndustry.UI`. See that repo’s `docs/testing.md`.

## CI

GitHub Actions: `.github/workflows/tests.yml` runs `./scripts/ci` on push/PR to `main`/`develop` and `workflow_dispatch`.

## Rules for AI

- Do not delete, skip, or weaken tests to green the build.
- Prefer targeted tests while iterating; full `./scripts/verify` before PR.
