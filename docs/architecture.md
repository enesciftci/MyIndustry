# Architecture — MyIndustry Backend

## Overview

Microservices-lite .NET 8 backend: Main API, Identity API, Ocelot Gateway, and a Queue worker. Shared cross-cutting code lives in `MyIndustry.Container`.

```text
Client (MyIndustry.UI)
        │
        ▼
   Gateway (Ocelot)
     │         │
     ▼         ▼
  Main API   Identity API
     │         │
     ▼         ▼
  Postgres   Postgres (Identity)
     │
  Redis / RabbitMQ / R2 (as configured)
```

## Main bounded context

| Project | Responsibility |
|---------|----------------|
| `MyIndustry.Api` | HTTP, versioning, migrations at startup, seeding |
| `MyIndustry.ApplicationService` | MediatR commands/queries/handlers |
| `MyIndustry.Domain` | Aggregates, value objects, domain exceptions |
| `MyIndustry.Repository` | EF Core `MyIndustryDbContext`, repositories, UoW |

## Identity

Parallel stack: `Identity.Api`, `Identity.Domain`, `Identity.Repository`. Issues JWT; MassTransit publish for side effects. `Identity.ApplicationService` is currently unused.

## Gateway & messaging

- **Gateway:** JWT validation, route forwarding (`ocelot.*.json`)
- **Queue:** MassTransit consumers (email/SMS/view count, etc.)
- **Communicators:** Redis, RabbitMQ, Core API HTTP adapters

## API conventions

- Route shape: `api/v{version}/[controller]s`
- Envelope: `ResponseBase` via `BaseController`
- Auth: Bearer JWT; admin policy `type == 99`; internal routes use `X-Internal-Api-Key`
- Health: `GET /health`

## Data

- PostgreSQL via Npgsql
- Migrations under respective `Migrations/` folders
- Tests use EF InMemory + `ASPNETCORE_ENVIRONMENT=Testing`

## Observability

- Serilog (compact JSON)
- Optional ELK / Filebeat stacks — see `observability/` and `docker-compose.observability.yaml`

## Related docs

- Local compose: `COMPOSE.md`
- Env for Dokploy: `DOKPLOY-ENV.md`
- Security notes: `SECURITY.md`
- AI workflow: `docs/ai-loop.md`
