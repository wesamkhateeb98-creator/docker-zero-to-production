# compose.prod.yml

> The production file pulls images by tag, exposes only Caddy, and gives every service limits, logs, and restarts.

## Problem
Each production concern from Phase 7 has to land somewhere concrete. Here is where.

```mermaid
flowchart LR
    I["Internet"] -->|":80 :443"| C["caddy"]
    subgraph notes_default
        C --> A["api<br/>512M"]
        A --> D[("db<br/>512M")]
        A --> R[("redis<br/>128M")]
        M["migrator<br/>profile: tools"] -.-> D
    end
    A --- K[("dpkeys")]
    D --- P[("pgdata")]
    C --- CD[("caddy_data")]
```

## Example — [compose.prod.yml](../../examples/02-dotnet-api/compose.prod.yml) (excerpt)
```yaml
api:
  image: ${REGISTRY:?}/notes-api:${IMAGE_TAG:?set IMAGE_TAG}
  restart: unless-stopped
  environment:
    ConnectionStrings__Redis: redis:6379
    ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"
  secrets: [{ source: db_connection, target: ConnectionStrings__Db }]
  volumes: ["dpkeys:/app/keys"]
  stop_grace_period: 30s
  deploy: { resources: { limits: { memory: 512M, cpus: "1.0" } } }
  depends_on:
    db: { condition: service_healthy }
    redis: { condition: service_healthy }
  logging: *logging
```

## Line → Reason
| Setting | Reason | Doc |
|---|---|---|
| `image: …:${IMAGE_TAG:?}` | Never build on server; fail if tag unset | [25](../07-deployment/25-dev-vs-prod.md) |
| No `ports:` except Caddy | DB/Redis/API private | [16](../04-data-network/16-networking.md) |
| `secrets:` + `*_FILE` | 0 passwords in `inspect` | [26](../07-deployment/26-secrets.md) |
| `dpkeys` volume | Logins survive redeploys | below |
| `stop_grace_period: 30s` | .NET drains requests | [10](../03-containers/10-lifecycle.md) |
| `limits.memory` | Leak can't kill the host | [13](../03-containers/13-limits.md) |
| `logging: local 10m×3` | Disk can't fill | [30](../07-deployment/30-logging.md) |
| `restart: unless-stopped` | Recover from crashes | [28](../07-deployment/28-restart-health.md) |
| `redis --maxmemory 64mb allkeys-lru` | Cache evicts instead of OOM | — |
| `migrator` in `profiles: [tools]` | Runs only when called | [45](45-ef-migrations.md) |
| No `container_name:` | Allows `--scale api=2` rollout | [29](../07-deployment/29-zero-downtime.md) |

## Data Protection Keys
| Without `dpkeys` volume | With it |
|---|---|
| New container = new key ring | Keys persist in `/app/keys` |
| Every deploy: auth cookies & antiforgery tokens invalid → users logged out | Seamless |

## Verify
```bash
docker compose -f compose.prod.yml config -q && echo valid
docker compose -f compose.prod.yml ps
docker port notes-db-1        # (nothing) ✅
```

## Key Points
- `.env` holds only non-secrets
- `secrets/*.txt` never in git
- Anchors (`&logging`) avoid repetition

## Pitfall
❌ `ports: ["8080:8080"]` on the api "for debugging" → ✅ Bypasses Caddy (no TLS) and blocks `--scale api=2` (port conflict); use `docker compose exec api wget -qO- localhost:8080/health`

---
← [43 .NET Dockerfile](43-dotnet-dockerfile.md) · [Next → 45 EF Migrations](45-ef-migrations.md)
