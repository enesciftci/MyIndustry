# Dokploy environment migration

How to move from legacy per-service env vars to the [`docker-compose.dokploy.yaml`](docker-compose.dokploy.yaml) source-env model.

## How it works

Compose sets container env from each service `environment` block. Variables in Dokploy (e.g. `DB_HOST`) are used for `${...}` substitution in compose. Keys **not** listed in compose (e.g. `R2__*`) may not reach containers unless Dokploy injects project env globally or you add them on the **Api service** in Dokploy.

Template: [`env.example`](env.example)

---

## Source env (Dokploy app stack)

Copy from [`env.example`](env.example). Example with typical Dokploy hostnames:

```env
JWT_SIGNING_KEY=YourSecureRandomKeyAtLeast32CharsLong
ALLOWED_HOSTS=gateway.your-domain.com;api.your-domain.com
INTERNAL_API_KEY=your-strong-random-api-key
ADMIN_EMAIL=admin@your-domain.com
ADMIN_PASSWORD=YourSecureAdminPassword
PASSWORD_RESET_ALLOWED_BASE_URL=https://your-frontend-domain.com

DB_HOST=myindustry-database-ss66da
DB_PASSWORD=your-postgres-password

REDIS_HOST=myindustry-redis-kmrpdg
REDIS_PASSWORD=your-redis-password

CORS_ALLOWED_ORIGIN=https://your-frontend-domain.com

RABBITMQ_HOST=your-rabbitmq-container-name
RABBITMQ_USER=your-rabbitmq-user
RABBITMQ_PASSWORD=your-rabbitmq-password
```

Optional: `JWT_ISSUER`, `RECAPTCHA_SECRET_KEY`

---

## Remove from Dokploy (compose sets these)

| Remove | Reason |
|--------|--------|
| `ConnectionStrings__MyIndustry=Host=...` | Built from `DB_HOST` + `DB_PASSWORD` |
| `ConnectionStrings__Redis=...` | Built from `REDIS_HOST` + `REDIS_PASSWORD` |
| `IdentityUrl=...` | Fixed in compose: `http://myindustry-identity:8080` |
| `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS` | Set in compose |
| `Cors__AllowedOrigins__0=...` | Mapped from `CORS_ALLOWED_ORIGIN` |
| `Jwt__SigningKey=...` | Mapped from `JWT_SIGNING_KEY` |

---

## R2 (Api service only)

Add on **myindustry-api** in Dokploy (not in compose). Use exact key names from [`R2Options.cs`](MyIndustry.Api/Services/R2Options.cs):

```env
R2__AccountId=your-cloudflare-account-id
R2__AccessKeyId=your-r2-access-key-id
R2__SecretAccessKey=your-r2-secret-access-key
R2__BucketName=myindustry-uploads
R2__PublicBaseUrl=https://pub-xxxx.r2.dev
```

Common mistakes:

| Wrong | Fix |
|-------|-----|
| `R2__SecretKey` | Use `R2__SecretAccessKey` |
| Trailing comma (`value,`) | No comma after value |
| `...r2.cloudflarestorage.com` as PublicBaseUrl | Use R2.dev or custom public CDN URL |

---

## Second CORS origin (www)

Compose injects only `Cors__AllowedOrigins__0` from `CORS_ALLOWED_ORIGIN`.

- **Option A:** Redirect www to apex in DNS/CDN; single `CORS_ALLOWED_ORIGIN`
- **Option B:** On Api, Identity, and Gateway in Dokploy, add `Cors__AllowedOrigins__1=https://www.your-frontend-domain.com`

---

## Deploy checklist

1. Update app stack env from [`env.example`](env.example)
2. Add R2 vars on Api service if using Cloudflare R2
3. Remove duplicate legacy vars (table above)
4. **Rebuild + Deploy** (commit `f6c257d` or later)
5. Verify:

```bash
docker exec myindustry-api printenv | grep -iE 'Jwt|Cors|Allowed|REDIS|ConnectionStrings|R2'
docker logs myindustry-api --tail 30
docker logs myindustry-identity --tail 30
docker logs myindustry-gateway --tail 30
```

Expected on Api: `Jwt__SigningKey`, `Cors__AllowedOrigins__0`, `AllowedHosts`, `ConnectionStrings__Redis`, `ConnectionStrings__MyIndustry`, and R2 keys if configured.

---

## Missing vars vs startup errors

| Missing | Symptom |
|---------|---------|
| `JWT_SIGNING_KEY` | `Jwt:SigningKey must be set...` |
| `CORS_ALLOWED_ORIGIN` | `Cors:AllowedOrigins must be set in Production` |
| `ALLOWED_HOSTS` | `AllowedHosts must be set to specific domains` |
| `DB_HOST` / `DB_PASSWORD` | Empty or wrong DB connection string |
| `REDIS_HOST` / `REDIS_PASSWORD` | `ConnectionStrings:Redis must be set...` |
| `RABBITMQ_*` | Identity/Queue RabbitMQ connection failures |
| `ADMIN_*`, `PASSWORD_RESET_ALLOWED_BASE_URL` | Admin seed skipped; broken reset links |

See also: [`SECURITY.md`](SECURITY.md) production checklist.
