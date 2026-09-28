#!/usr/bin/env bash
# Zero-downtime deploy on ONE server: start the new container next to the old one,
# wait until it is healthy, then remove the old one.
#   ./rollout.sh sha-abc1234        (SKIP_PULL=1 for local rehearsal)
set -euo pipefail
cd "$(dirname "$0")"

export IMAGE_TAG="$1"
C="docker compose -f compose.prod.yml"

[ "${SKIP_PULL:-0}" = 1 ] || $C pull api migrator
$C run --rm migrator

OLD=$($C ps -q api)
$C up -d --no-deps --no-recreate --scale api=2 --wait api    # old + new, both healthy
docker stop -t 30 $OLD >/dev/null && docker rm $OLD >/dev/null  # drain + remove old
$C up -d --no-deps --no-recreate --scale api=1 api
echo "$(date -u +%FT%TZ) $IMAGE_TAG" >> deployed.log
echo "✅ rolled out $IMAGE_TAG with zero downtime"
