# 🐳 Docker — From Zero to Production

> Learn Docker visually: one idea per file, diagram first, runnable .NET examples, **measured** numbers.
> Ends with a .NET API on a VPS with zero-downtime deploys — plus 25 anti-patterns to avoid.

**Docker 29 · .NET 10 · Alpine · Postgres 17 · Redis 8 · Caddy 2 · Compose v2** — 8 phases · 42 docs · 2 runnable examples

---

## 🗺️ Learning Path

```mermaid
flowchart TB
    subgraph L["① Learn the basics"]
        direction LR
        P1["1 Foundations"] --> P2["2 Images"] --> P3["3 Containers"]
    end
    subgraph B["② Build real apps"]
        direction LR
        P4["4 Data & Network"] --> P5["5 Compose"]
    end
    subgraph S["③ Ship to production"]
        direction LR
        P6["6 Modern Tools"] --> P7["7 .NET → VPS 🎯"]
    end
    L --> B --> S --> P8["8 · 25 Anti-patterns"]
    classDef goal fill:#fef3c7,stroke:#d97706,color:#78350f
    class P7 goal
```

| Phase | You'll be able to… | Docs |
|---|---|---|
| [1 Foundations](docs/01-foundations/README.md) | Explain containers vs VMs, install Docker | 01–04 |
| [2 Images](docs/02-images/README.md) | Write a small, cached, multi-stage Dockerfile | 05–09 |
| [3 Containers](docs/03-containers/README.md) | Run, inspect, configure, and limit containers | 10–13 |
| [4 Data & Network](docs/04-data-network/README.md) | Keep data safe, connect containers by name | 14–17 |
| [5 Compose](docs/05-compose/README.md) | Run a full stack with one command — and write the file **well** | 18–25 |
| [6 Modern Tools](docs/06-modern-tools/README.md) | Use the one best tool per job: CI, HTTPS, scanning, tests | 26–29 |
| [7 .NET → VPS](docs/07-dotnet-vps/README.md) 🎯 | Ship a .NET API with CI/CD, zero downtime, rollback | 30–37 |
| [8 Anti-patterns](docs/08-anti-patterns/README.md) | Spot 25 common mistakes in any project | 38–42 |

---

## 🔬 Headline Findings — measured in this repo

| Finding | Result | Doc |
|---|---|---|
| Multi-stage build | **953 MB → 123 MB**, 41 CVEs → 0 | [08](docs/02-images/08-multi-stage.md) |
| Your code in a 123 MB image | **143 kB** — the only layer a deploy changes | [07](docs/02-images/07-layers-cache.md) |
| Layer order (3 NuGet packages) | Rebuild **107–137 s → 19–45 s** | [07](docs/02-images/07-layers-cache.md) |
| No `.dockerignore` on Windows | Build **fails** | [09](docs/02-images/09-dockerignore.md) |
| Shell-form `ENTRYPOINT` | Stop **10.8 s, exit 137** vs 1.1 s, exit 0 | [10](docs/03-containers/10-lifecycle.md) |
| .NET memory leak at the limit | 500 errors, container still **"healthy"** | [13](docs/03-containers/13-limits.md) |
| Postgres without a volume | Data gone + **80 MB** orphan volumes | [14](docs/04-data-network/14-volumes.md) |
| `$VAR` in a healthcheck | Compose substitutes **your** env → `pg_isready -U` (empty) | [22](docs/05-compose/22-healthcheck-recipes.md) |
| `compose up -d` redeploy | **17.4 s outage** → `rollout.sh`: **0** failed | [35](docs/07-dotnet-vps/35-zero-downtime.md) |
| Unhealthy + restart policy | Restarted **0 times** | [39](docs/08-anti-patterns/39-runtime.md) |

---

## 🚀 Quick Start

```bash
git clone https://github.com/wesamkhateeb98-creator/docker-zero-to-production.git
cd docker-zero-to-production/examples/02-dotnet-api
docker compose up -d --build --wait

curl -X POST localhost:8080/notes -H 'Content-Type: application/json' -d '{"text":"hello"}'
curl -i localhost:8080/notes        # X-Cache: MISS → run again → HIT
docker compose down                 # add -v to also delete the data
```

---

## 📚 All 42 Docs

<details>
<summary><b>1 · Foundations</b> — 01–04</summary>

| # | Doc | One line |
|---|---|---|
| 01 | [What is Docker](docs/01-foundations/01-what-is-docker.md) | App + deps in one image; when to skip it |
| 02 | [VM vs Container](docs/01-foundations/02-vm-vs-container.md) | Shared kernel; microVMs in between |
| 03 | [Architecture](docs/01-foundations/03-architecture.md) | CLI → dockerd → containerd → runc |
| 04 | [Install](docs/01-foundations/04-install.md) | Desktop vs Engine; Windows gotchas |

</details>

<details>
<summary><b>2 · Images</b> — 05–09</summary>

| # | Doc | One line |
|---|---|---|
| 05 | [Image vs Container](docs/02-images/05-image-vs-container.md) | Class vs object; shared layers |
| 06 | [Dockerfile](docs/02-images/06-dockerfile.md) | First Dockerfile and its 4 problems |
| 07 | [Layers & Cache](docs/02-images/07-layers-cache.md) | Stable first, volatile last |
| 08 | [Multi-stage](docs/02-images/08-multi-stage.md) | Build with SDK, ship runtime |
| 09 | [.dockerignore](docs/02-images/09-dockerignore.md) | Small context, working builds |

</details>

<details>
<summary><b>3 · Containers</b> — 10–13</summary>

| # | Doc | One line |
|---|---|---|
| 10 | [Lifecycle](docs/03-containers/10-lifecycle.md) | States, exit codes, restart policies |
| 11 | [Commands](docs/03-containers/11-commands.md) | Daily cheatsheet + which shell exists |
| 12 | [ENV & Ports](docs/03-containers/12-env-ports.md) | Config precedence, port binding |
| 13 | [Resource Limits](docs/03-containers/13-limits.md) | Managed vs native OOM |

</details>

<details>
<summary><b>4 · Data & Network</b> — 14–17</summary>

| # | Doc | One line |
|---|---|---|
| 14 | [Volumes](docs/04-data-network/14-volumes.md) | Data that survives `docker rm` |
| 15 | [Bind Mounts](docs/04-data-network/15-bind-mounts.md) | Host folders; Windows speed cost |
| 16 | [Networking](docs/04-data-network/16-networking.md) | Drivers and isolation |
| 17 | [Container DNS](docs/04-data-network/17-dns.md) | `db:5432`, not IPs or `localhost` |

</details>

<details>
<summary><b>5 · Compose</b> — 18–25</summary>

| # | Doc | One line |
|---|---|---|
| 18 | [Compose Basics](docs/05-compose/18-compose-basics.md) | One file, ten commands |
| 19 | [Anatomy](docs/05-compose/19-compose-anatomy.md) | 5 top-level blocks, 8 questions per service |
| 20 | [Multi-service App](docs/05-compose/20-multi-service-app.md) | API + Postgres + Redis + migrator |
| 21 | [Healthchecks & depends_on](docs/05-compose/21-healthchecks-depends-on.md) | Who checks, who waits, in what order |
| 22 | [Healthcheck Recipes](docs/05-compose/22-healthcheck-recipes.md) | Copy-paste checks, `$` vs `$$`, timing math |
| 23 | [Style Guide](docs/05-compose/23-compose-style-guide.md) | 15 rules, each linked to evidence |
| 24 | [Before → After](docs/05-compose/24-compose-refactor.md) | 15 lines, 10 problems, 10 fixes |
| 25 | [Dev Workflow](docs/05-compose/25-compose-dev-workflow.md) | Override, profiles, `watch` |

</details>

<details>
<summary><b>6 · Modern Tools</b> — 26–29</summary>

| # | Doc | One line |
|---|---|---|
| 26 | [GitHub Actions + GHCR](docs/06-modern-tools/26-github-actions-ghcr.md) | Build, scan, push, deploy |
| 27 | [Caddy](docs/06-modern-tools/27-caddy.md) | HTTPS + reverse proxy in 5 lines |
| 28 | [Trivy](docs/06-modern-tools/28-trivy.md) | CVE scans that fail the build |
| 29 | [Testcontainers](docs/06-modern-tools/29-testcontainers.md) | Real Postgres in integration tests |

</details>

<details>
<summary><b>7 · .NET → VPS</b> 🎯 — 30–37</summary>

| # | Doc | One line |
|---|---|---|
| 30 | [VPS Setup](docs/07-dotnet-vps/30-vps-setup.md) | SSH, firewall, Docker, daemon.json |
| 31 | [.NET Dockerfile](docs/07-dotnet-vps/31-dotnet-dockerfile.md) | `runtime` + `migrator` targets |
| 32 | [compose.prod.yml](docs/07-dotnet-vps/32-compose-prod.md) | Every production line, incl. secrets |
| 33 | [EF Migrations](docs/07-dotnet-vps/33-ef-migrations.md) | Bundle as a one-shot job |
| 34 | [CI/CD](docs/07-dotnet-vps/34-cicd.md) | Push → GHCR → rollout |
| 35 | [Zero-downtime](docs/07-dotnet-vps/35-zero-downtime.md) | 17.4 s outage → 0 s |
| 36 | [Rollback](docs/07-dotnet-vps/36-rollback.md) | Previous SHA, zero downtime |
| 37 | [Checklist](docs/07-dotnet-vps/37-checklist.md) | 20 checks before go-live |

</details>

<details>
<summary><b>8 · Anti-patterns</b> — 38–42</summary>

| # | Doc | Covers |
|---|---|---|
| 38 | [Dockerfile](docs/08-anti-patterns/38-dockerfile.md) | #1–5 |
| 39 | [Runtime](docs/08-anti-patterns/39-runtime.md) | #6–10 |
| 40 | [Data](docs/08-anti-patterns/40-data.md) | #11–15 |
| 41 | [Network & Security](docs/08-anti-patterns/41-network-security.md) | #16–20 |
| 42 | [Deploy & CI](docs/08-anti-patterns/42-deploy-ci.md) | #21–25 |

</details>

---

## 🧪 Examples

| Folder | What's inside | Used in |
|---|---|---|
| [01-hello-dotnet](examples/01-hello-dotnet) | Minimal API + naive vs multi-stage Dockerfiles | 05–17 |
| [02-dotnet-api](examples/02-dotnet-api) | Notes API (EF Core, Postgres, Redis), dev + prod Compose, Caddy, deploy scripts, CI, tests | 18–42 |

---

## ✍️ How Each Doc Is Written

```mermaid
flowchart TB
    subgraph X["Explain"]
        direction LR
        A["Title + one-liner"] --> B["Problem"] --> C["Diagram"]
    end
    subgraph Y["Prove"]
        direction LR
        D["Example"] --> E["Numbers"] --> F["Pitfall ❌ → ✅"]
    end
    X --> Y
```

| Label | Meaning |
|---|---|
| **measured** | Run on Docker Desktop 29 (Windows, 4 CPUs, 7.7 GiB) — your numbers differ, ratios hold |
| **typical** | Common range, not measured here |
| **validated** | Config checked (`compose config`, `actionlint`), not run long-term |
