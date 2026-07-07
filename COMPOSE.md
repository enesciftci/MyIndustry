# Docker Compose layout

Three compose files cover local development and Dokploy production.

## Files

| File | When to use |
|------|-------------|
| `compose.yaml` | Local development (profiles: `debug`, `elk`) |
| `docker-compose.dokploy.yaml` | Dokploy app stack (API, Identity, Gateway, Queue) |
| `docker-compose.observability.yaml` | Dokploy ELK stack (separate service) |

## Local development

Default — app + infra; API and Identity are internal, gateway on `:5002`:

```bash
docker compose up -d
```

Direct API/Identity host ports for debugging:

```bash
docker compose --profile debug up -d
```

Local ELK stack:

```bash
docker compose --profile elk up -d
```

Both debug ports and ELK:

```bash
docker compose --profile debug --profile elk up -d
```

## Dokploy

| Service | Compose path |
|---------|--------------|
| Application | `docker-compose.dokploy.yaml` |
| Observability (ELK) | `docker-compose.observability.yaml` |

API and Identity have no public host ports in production; traffic goes through the gateway.

See also: [`observability/README.md`](observability/README.md), [`SECURITY.md`](SECURITY.md).
