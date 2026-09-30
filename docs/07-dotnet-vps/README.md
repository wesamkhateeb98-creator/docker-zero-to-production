# Phase 7 — .NET API → VPS

> The whole path, end to end, using [examples/02-dotnet-api](../../examples/02-dotnet-api) — rehearsed locally with the same files.

```mermaid
flowchart TB
    subgraph P1["Prepare"]
        direction LR
        A["30 VPS Setup"] --> B["31 Dockerfile"] --> C["32 compose.prod.yml"]
    end
    subgraph P2["Ship"]
        direction LR
        D["33 EF Migrations"] --> E["34 CI/CD"] --> Z["35 Zero-downtime"] --> F["36 Rollback"]
    end
    P1 --> P2 --> G["37 Checklist ✅"]
```

| # | Idea | One line | File |
|---|---|---|---|
| 30 | VPS Setup | User, SSH, firewall, Docker, daemon.json | [30](30-vps-setup.md) |
| 31 | .NET Dockerfile | 3 targets: runtime, migrator, build | [31](31-dotnet-dockerfile.md) |
| 32 | compose.prod.yml | Every production line explained, incl. secrets | [32](32-compose-prod.md) |
| 33 | EF Core Migrations | Bundle as a one-shot job | [33](33-ef-migrations.md) |
| 34 | CI/CD | Push → GHCR → rollout, first-time setup | [34](34-cicd.md) |
| 35 | Zero-downtime | 17.4 s outage → 0 s | [35](35-zero-downtime.md) |
| 36 | Rollback | Previous SHA, zero downtime | [36](36-rollback.md) |
| 37 | Checklist | 20 checks before go-live | [37](37-checklist.md) |

## Rehearsed Locally — measured
| Step | Result |
|---|---|
| `SKIP_PULL=1 ./deploy.sh v1` | Migrator applied `InitialCreate`, all healthy, 57 s |
| `https://localhost/` via Caddy | `{"version":"v1"}`, HTTP → 308 |
| `./rollout.sh v2` under load | 0 / 67 failed |
| `./rollback.sh` | 0 / 61 failed, back on v1 |
| `./backup.sh` + restore drill | 50 / 50 rows |

---
← [Phase 6 — Modern Tools](../06-modern-tools/README.md) · [Next phase → 8 25 Anti-patterns](../08-anti-patterns/README.md)
