#!/usr/bin/env bash
set -euo pipefail

DEPLOY_DIR="${1:-/opt/expense-tracker/current}"
cd "$DEPLOY_DIR"

export COMPOSE_PROJECT_NAME=expense-tracker
docker compose up -d --build --remove-orphans
systemctl restart expense-tracker.service

for attempt in {1..12}; do
  if curl --fail --silent --show-error http://127.0.0.1:8080/swagger/index.html >/dev/null; then
    echo "ExpenseTracker.API deployment verified."
    exit 0
  fi
  sleep 5
done

echo "Deployment verification failed." >&2
docker compose logs --tail=100 api
exit 1
