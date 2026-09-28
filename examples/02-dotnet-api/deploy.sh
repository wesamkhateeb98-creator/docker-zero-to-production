#!/usr/bin/env bash
# Usage on the VPS:  ./deploy.sh sha-abc1234
# Local rehearsal:   SKIP_PULL=1 ./deploy.sh v1
set -euo pipefail
cd "$(dirname "$0")"

export IMAGE_TAG="$1"
C="docker compose -f compose.prod.yml"

[ "${SKIP_PULL:-0}" = 1 ] || $C pull api migrator   # SKIP_PULL=1 → rehearse locally with local images
$C run --rm migrator                  # 1. schema first (must stay backward-compatible)
$C up -d --remove-orphans --wait      # 2. then the app; --wait blocks until healthy
echo "$(date -u +%FT%TZ) $IMAGE_TAG" >> deployed.log   # 3. history for rollback
docker image prune -f --filter "until=168h" >/dev/null
echo "✅ deployed $IMAGE_TAG"
