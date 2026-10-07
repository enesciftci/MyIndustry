# Backend patterns (inspect before proposing)

Do not invent layers. Cite paths you actually opened.

## Placement

| Concern | Location |
|---------|----------|
| HTTP | `MyIndustry.Api/Controllers/v1/` |
| Handlers | `MyIndustry.ApplicationService/Handler/{Feature}/{Operation}/` |
| Domain | `MyIndustry.Domain` |
| EF / repos | `MyIndustry.Repository` (+ `Migrations/`) |
| Identity | `MyIndustry.Identity.*` |
| Gateway routes | `MyIndustry.Gateway` Ocelot JSON |
| Shared infra | `MyIndustry.Container` |
| Tests | `MyIndustry.Tests/Unit|Smoke|Integration` |

## Conventions

- Route shape: `api/v{version}/[controller]s`
- Envelope: `ResponseBase` via `BaseController`
- Prefer existing Container helpers over new middleware
- Match NuGet major versions already in sibling projects (EF/JWT 8.0.x)
- Coverage expectation ~80%+ for changed areas (`TEST_COVERAGE.md`, `docs/testing.md`)

## Health / smoke references

- `MapHealthChecks("/health")` in Api, Identity.Api, Gateway `Program.cs`
- Smoke: `MyIndustry.Tests/Smoke/HealthCheckSmokeTests.cs`
