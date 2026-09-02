#!/usr/bin/env bash
set -euo pipefail

APP_ROOT="${APP_ROOT:-/opt/expense-tracker}"
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
RELEASE_ID="$(date -u +%Y%m%d%H%M%S)"
RELEASE_DIR="$APP_ROOT/releases/$RELEASE_ID"

if [[ ! -d "$SCRIPT_DIR/app" ]]; then
  echo "Published API directory not found: $SCRIPT_DIR/app" >&2
  exit 1
fi

if ! id expense-tracker >/dev/null 2>&1; then
  useradd --system --home-dir "$APP_ROOT" --no-create-home --shell /usr/sbin/nologin expense-tracker
fi

install -d -m 0755 "$RELEASE_DIR"
cp -a "$SCRIPT_DIR/app/." "$RELEASE_DIR/"
chown -R expense-tracker:expense-tracker "$RELEASE_DIR"
ln -sfn "$RELEASE_DIR" "$APP_ROOT/current"
install -m 0644 "$SCRIPT_DIR/expense-tracker.service" /etc/systemd/system/expense-tracker.service

systemctl daemon-reload
systemctl enable expense-tracker.service
systemctl restart expense-tracker.service

for attempt in {1..12}; do
  if curl --fail --silent --show-error http://127.0.0.1:8080/swagger/index.html >/dev/null; then
    echo "ExpenseTracker.API deployment verified."
    exit 0
  fi
  sleep 5
done

echo "Deployment verification failed." >&2
journalctl -u expense-tracker.service --no-pager -n 100
exit 1
