# Refactor: Before → After

> A 15-line file that "works" has 10 problems. Fix them one by one — both versions pass `docker compose config`.

## Problem
Compose only warns about 1 of the 10 problems below (`version`). The rest fail later: lost data, leaked passwords, broken startups.

```mermaid
flowchart LR
    B["before.yml<br/>15 lines · 10 issues"] -->|"10 fixes"| A["after.yml<br/>25 lines · 0 issues"]
```

## ❌ Before
```yaml
version: "3.8"
services:
  api:
    build: .
    container_name: api
    ports: ["8080:8080"]
    environment:
      ConnectionStrings__Db: Host=db;Password=S3cret
    depends_on: [db]
  db:
    image: postgres
    container_name: db
    ports: ["5432:5432"]
    environment:
      POSTGRES_PASSWORD: S3cret
```

## ✅ After
```yaml
name: notes                                                    # ①
x-defaults: &defaults                                          # ⑨ ⑩
  restart: unless-stopped
  logging: { driver: local, options: { max-size: "10m", max-file: "3" } }
  deploy: { resources: { limits: { memory: 512M } } }
services:
  api:
    <<: *defaults
    image: ghcr.io/you/notes-api:${IMAGE_TAG:?set IMAGE_TAG}   # ②
    secrets: [{ source: db_connection, target: ConnectionStrings__Db }]   # ⑤
    ports: ["8080:8080"]
    networks: [frontend, backend]                              # ⑦
    depends_on: { db: { condition: service_healthy } }         # ⑥
    healthcheck: { test: ["CMD", "wget", "-qO-", "http://localhost:8080/health"], interval: 10s }
  db:
    <<: *defaults
    image: postgres:17-alpine                                  # ③
    environment: { POSTGRES_PASSWORD_FILE: /run/secrets/db_password }
    secrets: [db_password]
    networks: [backend]                                        # ⑧ no ports
    volumes: ["pgdata:/var/lib/postgresql/data"]               # ④
    healthcheck: { test: ["CMD-SHELL", "pg_isready -U postgres"], interval: 2s, retries: 30 }
networks: { frontend: {}, backend: { internal: true } }
volumes: { pgdata: {} }
secrets:
  db_password:   { file: ./secrets/db_password.txt }
  db_connection: { file: ./secrets/db_connection.txt }
```

## The 10 Fixes
| # | Problem in "before" | Fix |
|---|---|---|
| ① | Obsolete `version` | `name: notes` |
| ② | `build:` + no tag → can't roll back | `image:` + `${IMAGE_TAG:?}` |
| ③ | `postgres` = whatever is newest | `postgres:17-alpine` |
| ④ | No volume → data dies with the container | Named volume `pgdata` |
| ⑤ | Password in env → visible in `docker inspect` | `secrets:` + `_FILE` |
| ⑥ | `depends_on: [db]` → `Failed to connect` | `service_healthy` + healthchecks |
| ⑦ ⑧ | DB port public, one flat network | No DB `ports`; `backend: internal` |
| ⑨ | No restart / limits / log caps | One `x-defaults` anchor for all |
| ⑩ | `container_name` → can't scale | Removed (Compose names them) |

## Key Points
- "Valid" ≠ "good"
- Anchors: add a service = 1 line of defaults

## Pitfall
❌ `<<: *defaults` + your own `deploy:` → ✅ Measured: the service key **replaces** the anchor key — `memory: 512M` vanished, only `cpus` stayed; use separate anchors per key

---
← [23 Compose Style Guide](23-compose-style-guide.md) · [Next → 25 Compose Dev Workflow](25-compose-dev-workflow.md)
