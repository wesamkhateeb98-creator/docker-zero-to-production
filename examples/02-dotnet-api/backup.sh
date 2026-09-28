#!/usr/bin/env bash
# cron: 0 3 * * * /home/deploy/notes/backup.sh
set -euo pipefail
cd "$(dirname "$0")"
mkdir -p backups

FILE="backups/notes-$(date +%F).sql.gz"
docker compose -f compose.prod.yml exec -T db pg_dump -U notes notes | gzip > "$FILE"
find backups -name '*.sql.gz' -mtime +14 -delete
# Off-site copy (configure once with: rclone config)
# rclone copy backups remote:notes-backups
echo "💾 $FILE ($(du -h "$FILE" | cut -f1))"
