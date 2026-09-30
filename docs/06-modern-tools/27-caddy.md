# Caddy — Reverse Proxy + HTTPS

> One container owns ports 80/443, terminates TLS, and forwards to the app on the private network.

## Problem
The app shouldn't handle certificates, HTTP→HTTPS redirects, compression, or be exposed directly.

## Every Request
```mermaid
sequenceDiagram
    participant U as Browser
    participant H as Caddy :80
    participant S as Caddy :443
    participant A as api :8080
    U->>H: GET http://…
    H-->>U: 308 → https://…
    U->>S: TLS handshake (certificate)
    U->>S: GET / (encrypted)
    S->>A: GET / + X-Forwarded-*
    A-->>S: 200 (plain HTTP)
    S-->>U: 200 (TLS + gzip)
```

## First Start — getting the certificate (automatic)
```mermaid
sequenceDiagram
    participant C as Caddy
    participant L as Let's Encrypt
    participant D as DNS
    C->>L: order cert for notes.example.com
    L->>D: resolve notes.example.com
    D-->>L: VPS IP (A record)
    L->>C: challenge on :80 / :443
    C-->>L: proof
    L-->>C: certificate (~90 days)
    Note over C: stored in /data, renewed automatically
```
→ Needs: DNS A record → VPS **before** start, ports 80 + 443 open, `/data` on a volume.

## Example — [Caddyfile](../../examples/02-dotnet-api/Caddyfile)
```
{$DOMAIN} {
	encode zstd gzip
	reverse_proxy api:8080 {
		health_uri /health
		health_interval 5s
	}
}
```
`DOMAIN=notes.example.com` → real certificate, auto-renewed. Nothing else to configure.

## Measured — local rehearsal (`DOMAIN=localhost`)
```bash
curl -sk https://localhost/        # {"service":"notes-api","version":"v1",…}  (-k: Caddy's local CA)
curl -s -o /dev/null -w '%{http_code} %{redirect_url}' http://localhost/
# 308 https://localhost/
```

## .NET Behind a Proxy
```yaml
environment:
  ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"   # trust X-Forwarded-Proto/For
```
| Without it | With it |
|---|---|
| `Request.Scheme` = `http` | `https` |
| `RemoteIpAddress` = Caddy's IP | Real client IP |
| Redirect URLs use `http://` | Correct |

## Key Points
- Only the proxy publishes ports
- DNS A record must point first
- Persist `caddy_data` (certificates)
- Chosen over Traefik (needs `docker.sock` + labels) and Nginx (manual certbot)

## Pitfall
❌ No volume for `/data` → ✅ Every restart requests new certificates → Let's Encrypt rate limits lock you out

---
← [26 GHCR + GitHub Actions](26-github-actions-ghcr.md) · [Next → 28 Trivy](28-trivy.md)
