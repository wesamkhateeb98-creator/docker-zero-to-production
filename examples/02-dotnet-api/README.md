# Example 02 — Notes API (.NET 10 + Postgres + Redis)

> A small production-shaped API used by docs 18–42: Compose for dev, a hardened prod stack, CI, zero-downtime rollout, backups, and tests — all run locally.

```mermaid
flowchart LR
    U["client"] -->|":443"| C["caddy"]
    C --> A["api"]
    A --> R[("redis")]
    A --> D[("postgres")]
    M["migrator"] -.->|"once per deploy"| D
```

## Endpoints
| Method | Path | Does |
|---|---|---|
| GET | `/` | Service, `APP_VERSION`, container ID |
| GET | `/notes` | Cache-aside: Redis → Postgres (`X-Cache: HIT/MISS`) |
| POST | `/notes` | `{"text":"…"}` → insert + invalidate cache |
| GET | `/health` | Postgres + Redis reachable → `Healthy` / 503 |

## Dev
```bash
docker compose up -d --build --wait
curl -X POST localhost:8080/notes -H 'Content-Type: application/json' -d '{"text":"hi"}'
curl -i localhost:8080/notes
```

## Prod Rehearsal on Your Laptop
```bash
docker build --target runtime  --build-arg APP_VERSION=v1 -t local/notes-api:v1 .
docker build --target migrator -t local/notes-migrator:v1 .
# secrets/ → see secrets/README.md ; .env → DOMAIN=localhost REGISTRY=local
SKIP_PULL=1 ./deploy.sh v1
curl -k https://localhost/
./probe.sh &  SKIP_PULL=1 ./rollout.sh v2 ; touch probe.stop   # 0 failed requests
./rollback.sh
./backup.sh
```

## Tests
```bash
cd tests/Api.Tests && dotnet test      # Testcontainers: real postgres:17-alpine
```

## Files
| File | Purpose | Doc |
|---|---|---|
| `Dockerfile` | `runtime` + `migrator` targets | [31](../../docs/07-dotnet-vps/31-dotnet-dockerfile.md) |
| `compose.yml` | Dev: build, ports, healthchecks | [20](../../docs/05-compose/20-multi-service-app.md) |
| `compose.prod.yml` | Prod: images by tag, secrets, limits, logs | [32](../../docs/07-dotnet-vps/32-compose-prod.md) |
| `Caddyfile` | HTTPS + reverse proxy | [27](../../docs/06-modern-tools/27-caddy.md) |
| `deploy.sh` / `rollout.sh` / `rollback.sh` | Deploy, zero-downtime, undo | [35](../../docs/07-dotnet-vps/35-zero-downtime.md), [36](../../docs/07-dotnet-vps/36-rollback.md) |
| `backup.sh` | `pg_dump` + rotation | [40](../../docs/08-anti-patterns/40-data.md) |
| `probe.sh` | Measures failed requests during deploys | [35](../../docs/07-dotnet-vps/35-zero-downtime.md) |
| `.github/workflows/deploy.yml` | Build → scan → push → rollout | [26](../../docs/06-modern-tools/26-github-actions-ghcr.md), [34](../../docs/07-dotnet-vps/34-cicd.md) |
| `tests/Api.Tests` | Testcontainers integration test | [29](../../docs/06-modern-tools/29-testcontainers.md) |
| `secrets/README.md` | How to create the secret files | [32](../../docs/07-dotnet-vps/32-compose-prod.md) |
