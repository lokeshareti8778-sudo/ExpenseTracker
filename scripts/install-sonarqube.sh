#!/usr/bin/env bash
set -euo pipefail

SONAR_DIR="/opt/sonarqube"

if ! command -v docker >/dev/null 2>&1; then
  apt-get update
  apt-get install -y docker.io docker-compose-plugin
  systemctl enable --now docker
fi

mkdir -p "$SONAR_DIR"
cat > "$SONAR_DIR/docker-compose.yml" <<'YAML'
services:
  sonarqube-db:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: sonar
      POSTGRES_PASSWORD: change-this-password
      POSTGRES_DB: sonarqube
    volumes:
      - sonarqube-db:/var/lib/postgresql/data
    restart: unless-stopped

  sonarqube:
    image: sonarqube:community
    depends_on:
      - sonarqube-db
    environment:
      SONAR_JDBC_URL: jdbc:postgresql://sonarqube-db:5432/sonarqube
      SONAR_JDBC_USERNAME: sonar
      SONAR_JDBC_PASSWORD: change-this-password
    ports:
      - "9000:9000"
    volumes:
      - sonarqube-data:/opt/sonarqube/data
      - sonarqube-extensions:/opt/sonarqube/extensions
      - sonarqube-logs:/opt/sonarqube/logs
    restart: unless-stopped

volumes:
  sonarqube-db:
  sonarqube-data:
  sonarqube-extensions:
  sonarqube-logs:
YAML

sysctl -w vm.max_map_count=524288
printf 'vm.max_map_count=524288\n' > /etc/sysctl.d/99-sonarqube.conf
cd "$SONAR_DIR"
docker compose up -d
printf 'SonarQube is starting at http://%s:9000\n' "$(hostname -I | awk '{print $1}')"
