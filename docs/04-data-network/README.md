# Phase 4 — Data & Network

> Keep data alive across containers, and let containers find each other.

```mermaid
flowchart LR
    A["14 Volumes"] --> B["15 Bind Mounts"]
    B --> C["16 Networking"]
    C --> D["17 Container DNS"]
```

| # | Idea | One line | File |
|---|---|---|---|
| 14 | Volumes | Data that survives `docker rm` | [14](14-volumes.md) |
| 15 | Bind Mounts | Host folder inside a container | [15](15-bind-mounts.md) |
| 16 | Networking | Drivers, isolation, published ports | [16](16-networking.md) |
| 17 | Container DNS | `db:5432` instead of IPs | [17](17-dns.md) |

**Prerequisites:** [Phase 3 — Containers](../03-containers/README.md)

---
← [Phase 3](../03-containers/README.md) · [Next phase → 05 Compose](../05-compose/README.md)
