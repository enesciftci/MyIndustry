# MyIndustry Observability (ELK + Filebeat)

Centralized logging for MyIndustry backend services using Elasticsearch, Kibana, and Filebeat.

## Architecture

```
App containers (stdout JSON logs)
        ↓
Docker json-file log driver
        ↓
Filebeat (docker autodiscover)
        ↓
Elasticsearch
        ↓
Kibana (Discover / dashboards)
```

Applications emit structured JSON logs via Serilog (`RenderedCompactJsonFormatter`) to stdout.
Each HTTP request gets an `X-Correlation-Id` header (generated or forwarded) and the value is
included in every log event as `CorrelationId`.

## Local development

### 1. Start the application stack

```bash
docker compose up -d
```

### 2. Start the ELK stack

```bash
docker compose --profile elk up -d
```

If the app stack is already running, `--profile elk` adds only the ELK services.

### 3. Verify services

| Service       | URL                    |
|---------------|------------------------|
| Elasticsearch | http://localhost:9200  |
| Kibana        | http://localhost:5601  |

Check Filebeat is shipping logs:

```bash
curl http://localhost:9200/myindustry-logs-*/_search?size=1&pretty
```

### 4. Optional: Docker labels for Filebeat hints

Add these labels to app services in `compose.yaml` for Elastic autodiscover hints:

```yaml
labels:
  co.elastic.logs/enabled: "true"
  co.elastic.logs/json.add_error_key: "true"
  app: myindustry-api
```

Filebeat also matches containers by name (`myindustry-api`, `myindustry-identity`, etc.)
via templates in `observability/filebeat/filebeat.yml`, so labels are optional.

## Kibana setup

1. Open Kibana at http://localhost:5601
2. Go to **Stack Management → Index Patterns** (or **Data Views** in Kibana 8.x)
3. Create a data view:
   - Name: `myindustry-logs`
   - Index pattern: `myindustry-logs-*`
   - Timestamp field: `@timestamp`
4. Open **Discover** and verify log events appear

### Useful Discover filters

| Filter | KQL |
|--------|-----|
| All errors | `@l: "Error" or Level: "Error"` |
| API service | `Application: "MyIndustry.Api"` |
| Identity service | `Application: "MyIndustry.Identity.Api"` |
| Gateway | `Application: "MyIndustry.Gateway"` |
| Queue worker | `Application: "MyIndustry.Queue"` |
| Trace a request | `CorrelationId: "<your-correlation-id>"` |
| Specific MediatR command | `MediatRRequestName: "AddFavoriteCommand"` |
| All MediatR queries | `MediatRRequestType: "Query"` |
| Failed handler result | `MediatRResponseSuccess: false` |
| HTTP + handler trace | `CorrelationId: "..."` AND `MediatRRequestName: "..."` |

### Tracing a request across services

1. Send a request with a custom header:

```bash
curl -H "X-Correlation-Id: test-trace-001" http://localhost:5002/api/v1/...
```

2. In Kibana Discover, filter: `CorrelationId: "test-trace-001"`
3. You should see log lines from Gateway, API, and/or Identity with the same ID.

### MediatR command/query logging

The API logs every MediatR handler invocation with sanitized parameters:

| Field | Example |
|-------|---------|
| `MediatRRequestName` | `AddFavoriteCommand` |
| `MediatRRequestType` | `Command` or `Query` |
| `MediatRRequestPayload` | `{ "UserId": "...", "ServiceId": "..." }` (masked) |
| `MediatRElapsedMs` | `12` |
| `MediatRResponseSuccess` | `true` |
| `MediatRResponseMessageCode` | `0000` |

Sensitive fields (password, token, email, etc.) are masked automatically. Disable payload logging in production via `MediatRLogging:LogRequestPayload: false` in appsettings.

## Production (Dokploy)

### Prerequisites (host)

Before the first ELK deploy on a Dokploy node:

```bash
# Required for Elasticsearch on Linux
sudo sysctl -w vm.max_map_count=262144
echo "vm.max_map_count=262144" | sudo tee -a /etc/sysctl.conf
```

Recommended RAM: at least **2 GB free** for Elasticsearch on a shared host (8 GB total with other apps is fine with default `512m` heap).

### Dokploy environment variables

| Variable | Required | Notes |
|----------|----------|-------|
| `ELASTIC_PASSWORD` | Yes | Strong password; prefer `A-Za-z0-9` (no shell-special chars) |
| `DOMAIN` | Yes | Used by Traefik: `kibana.${DOMAIN}` |
| `KIBANA_SYSTEM_PASSWORD` | No | Defaults to `ELASTIC_PASSWORD` on first boot |
| `ES_JAVA_OPTS` | No | Default `-Xms512m -Xmx512m`; raise on dedicated nodes |
| `ES_MEMORY_LIMIT` | No | Default `1536m` container memory cap |

Deploy the observability stack as a separate Dokploy compose project:

```bash
docker compose -f docker-compose.observability.yaml up -d
```

### First deploy / redeploy checklist

1. Set `ELASTIC_PASSWORD` and `DOMAIN` in Dokploy stack env.
2. Ensure `vm.max_map_count >= 262144` on the host (see above).
3. Deploy from Dokploy (compose path: `docker-compose.observability.yaml`).
4. Verify:

```bash
docker inspect myindustry-elasticsearch --format '{{.State.Health.Status}}'
docker ps --filter name=myindustry-
curl -u elastic:$ELASTIC_PASSWORD http://localhost:9200/_cluster/health
```

Expected: Elasticsearch **healthy**, Kibana and Filebeat **Up**.

### Diagnose unhealthy Elasticsearch

```bash
docker logs myindustry-elasticsearch --tail 100
docker inspect myindustry-elasticsearch --format '{{json .State.Health}}' | jq
docker exec myindustry-elasticsearch printenv ELASTIC_PASSWORD
sysctl vm.max_map_count
free -h
```

| Log / symptom | Action |
|---------------|--------|
| `authentication failed` / healthcheck 401 | Fix `ELASTIC_PASSWORD` in Dokploy, or reset volume (below) |
| `max virtual memory areas vm.max_map_count` | Run `sysctl -w vm.max_map_count=262144` |
| `OutOfMemoryError` | Lower other services or set `ES_JAVA_OPTS=-Xms384m -Xmx384m` |
| Password changed after first install | Reset data volume (wipes logs) |

### Reset Elasticsearch data (password change)

Only if you changed `ELASTIC_PASSWORD` after the first successful boot:

```bash
docker compose -p <your-dokploy-project-name> down
docker volume rm myindustry-elasticsearch-data
# Redeploy from Dokploy with the new ELASTIC_PASSWORD
```

App services (`docker-compose.dokploy.yaml`) can optionally include Filebeat hint labels:

```yaml
labels:
  co.elastic.logs/enabled: "true"
  co.elastic.logs/json.add_error_key: "true"
  app: myindustry-api
```

Filebeat on the Dokploy node reads all `myindustry-*` container logs via Docker socket.

### Log retention (10 days)

Index Lifecycle Management (ILM) deletes daily indices older than **10 days**:

- Policy: `myindustry-logs-policy` (`observability/elasticsearch/ilm-myindustry-logs.json`)
- Filebeat loads the policy on startup via `setup.ilm.policy_file`
- Index pattern: `myindustry-logs-YYYY.MM.DD`

After redeploying Filebeat, verify in Kibana: **Stack Management → Index Lifecycle Policies**.

To apply the policy to indices created before this change:

```bash
curl -u elastic:$ELASTIC_PASSWORD -X PUT \
  "http://localhost:9200/myindustry-logs-*/_settings" \
  -H 'Content-Type: application/json' \
  -d '{"index.lifecycle.name": "myindustry-logs-policy"}'
```

### Kibana access (production)

Kibana is exposed via Traefik at `kibana.${DOMAIN}` when using `docker-compose.observability.yaml`.
Adjust the Traefik labels for your Dokploy/Traefik setup.

## Log format

Example Serilog compact JSON line:

```json
{
  "@t": "2026-07-06T09:00:00.0000000Z",
  "@l": "Information",
  "@mt": "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
  "Application": "MyIndustry.Api",
  "Environment": "Production",
  "CorrelationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "RequestMethod": "GET",
  "RequestPath": "/api/v1/services",
  "StatusCode": 200,
  "Elapsed": 45.2
}
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Elasticsearch unhealthy (Dokploy) | See [Diagnose unhealthy Elasticsearch](#diagnose-unhealthy-elasticsearch); check healthcheck uses container `$ELASTIC_PASSWORD` |
| No logs in Elasticsearch | Check `docker logs myindustry-filebeat`; ensure containers are named `myindustry-*` |
| Elasticsearch won't start | Set `vm.max_map_count=262144`; allocate ~2 GB RAM; check `docker logs myindustry-elasticsearch` |
| Kibana/Filebeat not starting | Wait for ES healthy; `depends_on: service_healthy` blocks until ES passes healthcheck |
| JSON fields not parsed | Verify apps output single-line JSON; Filebeat `decode_json_fields` handles Docker-wrapped messages |
| CorrelationId missing in Queue logs | Ensure publisher sends `X-Correlation-Id` header via MassTransit |

## Files

| File | Purpose |
|------|---------|
| `compose.yaml` | Local stack (`--profile elk` for ELK, `--profile debug` for direct API/Identity ports) |
| `docker-compose.observability.yaml` | Production ELK stack for Dokploy |
| `observability/elasticsearch/ilm-myindustry-logs.json` | ILM policy — delete indices after 10 days |
| `observability/filebeat/filebeat.yml` | Local Filebeat config |
| `observability/filebeat/filebeat.dokploy.yml` | Production Filebeat config (with ES auth) |
| `MyIndustry.Container/` | Shared CorrelationId middleware and Serilog setup |
