# Trivy — Image Scanning

> Scanners match every OS package and NuGet dependency in the image against CVE databases.

## Problem
Your code is fine, but the base image ships an OpenSSL with a known CVE — and nobody checks until an audit.

```mermaid
flowchart LR
    I["image"] --> T["trivy"]
    DB[("CVE DB<br/>~daily updates")] --> T
    T -->|"CRITICAL found"| F["exit 1 → CI fails ❌"]
    T -->|"clean"| P["push ✅"]
```

## Example
```bash
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock aquasec/trivy \
  image --scanners vuln --severity HIGH,CRITICAL --ignore-unfixed --exit-code 1 notes-api:v1
```

## Measured — 2026-09-28
| Image | Findings |
|---|---|
| `hello-dotnet:naive` (`sdk:10.0`, Ubuntu) | 41 (5 LOW, 36 MEDIUM) |
| `aspnet:10.0` (Ubuntu) | 14 |
| `aspnet:10.0-noble-chiseled` | 8 |
| `aspnet:10.0-alpine` apps (incl. Npgsql, StackExchange.Redis) | **0** |
Image choice by CVE count → [31](../07-dotnet-vps/31-dotnet-dockerfile.md).

## What It Scans
| Layer | Example target |
|---|---|
| OS packages | `alpine 3.24.2`, `ubuntu 24.04` |
| .NET deps | `app/Api.deps.json` (every NuGet package) |
| Runtime | `Microsoft.AspNetCore.App.deps.json` |

## Why Trivy
| | |
|---|---|
| Cost | Free, open source, no login |
| Runs as | One container: `aquasec/trivy` — laptop, CI, server |
| Also scans | Secrets in images, IaC files, SBOM output |

## Key Points
- Scan before push, in CI
- `--ignore-unfixed` → only actionable findings
- Rebuild weekly to pick up patched bases

## Pitfall
❌ Scanner errors treated as "no findings" → ✅ Measured: a DB download **timeout** made a wrapper script print "0 vulnerabilities" for all 5 images; real count was 41 for one. Let the scanner's own exit code fail the job

---
← [27 Caddy](27-caddy.md) · [Next → 29 Testcontainers](29-testcontainers.md)
