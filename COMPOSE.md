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

### App stack prerequisites (Dokploy)

Before deploying `docker-compose.dokploy.yaml`:

1. **Environment:** copy [`env.example`](env.example) into Dokploy app stack env. See [`DOKPLOY-ENV.md`](DOKPLOY-ENV.md) if migrating from legacy `ConnectionStrings__*` / `Jwt__*` vars.
2. **R2 (optional):** add `R2__*` on **myindustry-api** service only — see [`DOKPLOY-ENV.md`](DOKPLOY-ENV.md).
3. **Network:** `dokploy-network` must exist (created by Dokploy).
4. **Deploy:** Rebuild + deploy after env changes; verify with `docker exec myindustry-api printenv`.

See also: [`SECURITY.md`](SECURITY.md) production checklist.

### ELK prerequisites (observability stack)

Before deploying `docker-compose.observability.yaml` on the Dokploy node:

1. **Host:** `vm.max_map_count=262144` (see [`observability/README.md`](observability/README.md))
2. **Dokploy env:** `ELASTIC_PASSWORD`, `DOMAIN` (optional: `ES_JAVA_OPTS`, `ES_MEMORY_LIMIT`)
3. **Network:** `dokploy-network` must exist (created by Dokploy)
4. **Bootstrap:** `es-setup` one-shot service syncs `kibana_system` password before Kibana starts
5. **After password change:** remove volume `myindustry-elasticsearch-data` before redeploy

See also: [`observability/README.md`](observability/README.md), [`SECURITY.md`](SECURITY.md).
