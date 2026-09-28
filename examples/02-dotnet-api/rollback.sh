#!/usr/bin/env bash
# Usage: ./rollback.sh            → previous tag from deployed.log
#        ./rollback.sh sha-xyz    → a specific tag
# Re-uses rollout.sh → zero downtime. Old migrator = no-op (its migrations are already applied).
set -euo pipefail
cd "$(dirname "$0")"

TAG="${1:-$(tail -n 2 deployed.log | head -n 1 | cut -d' ' -f2)}"
echo "↩️  rolling back to $TAG"
exec ./rollout.sh "$TAG"
