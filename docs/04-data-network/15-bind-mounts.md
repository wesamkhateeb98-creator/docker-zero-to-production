# Bind Mounts

> A bind mount maps a **host folder or file** into the container — changes show up on both sides instantly.

## Problem
Dev loop: rebuilding the image on every code change is slow; you want the container to see your editor's files.

```mermaid
flowchart LR
    H["Host D:\\src"] <-->|"-v D:\\src:/src"| C["Container /src"]
    C --> W["dotnet watch → reload"]
```

## Example — dev hot reload
```bash
docker run --rm -it -p 8080:8080 \
  -v "$PWD":/src -w /src \
  -e DOTNET_USE_POLLING_FILE_WATCHER=1 \
  mcr.microsoft.com/dotnet/sdk:10.0-alpine \
  dotnet watch run --urls http://0.0.0.0:8080
```

## Numbers — `dotnet build` in a container, measured (Windows host)
| Source location | Run 1 | Run 2 |
|---|---|---|
| Bind mount from `D:\` | 68.7 s | 26.0 s |
| Named volume | 33.6 s | 16.1 s |

→ Windows ↔ Linux file sharing costs **1.6–2×**. Keep code inside WSL (`\\wsl$\…`) or use a volume.

## Volume vs Bind Mount
| | Named volume | Bind mount |
|---|---|---|
| Managed by | Docker | You |
| Portable | ✅ same on every host | ❌ host path |
| Speed on Windows/macOS | Fast | Slower |
| Best for | Prod data | Dev source, one config file |

## Surprises — measured
| Action | Result |
|---|---|
| Mount empty dir over `/app` | Image's `/app` **hidden** → `The command could not be loaded` |
| Mount a host file that doesn't exist | Docker creates a **directory** with that name (Docker Desktop: `-v` and `--mount`; Linux Engine: `--mount` errors instead) |
| `-v ./src:/src:ro` then write | `Read-only file system` ✅ |

## Key Points
- Bind = dev, volume = data
- Add `:ro` when the app only reads
- Mounts hide what's underneath

## Pitfall
❌ `-v ./appsettings.json:/app/appsettings.json` with a typo in the path → ✅ You get an empty **folder**, app fails to read config; check `ls -ld` on the host first

---
← [14 Volumes](14-volumes.md) · [Next → 16 Networking](16-networking.md)
