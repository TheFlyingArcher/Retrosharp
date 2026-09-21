#!/bin/bash
# Forced command for the deploy-retrosharp SSH key (see authorized_keys on the
# app server). Pulls and starts the three GHCR images tagged $VERSION, which
# arrives via SSH-forwarded environment (AcceptEnv VERSION in sshd_config).
#
# Copy to /home/deploy-retrosharp/deploy.sh on the app server:
#   sudo chown root:deploy-retrosharp /home/deploy-retrosharp/deploy.sh
#   sudo chmod 750 /home/deploy-retrosharp/deploy.sh
set -euo pipefail

if [ -z "${VERSION:-}" ]; then
  echo "VERSION not set (expected via SSH-forwarded environment)" >&2
  exit 1
fi

cd /opt/retrosharp

COMPOSE_FILES="-f docker-compose.yml -f docker-compose.pi.yml -f docker-compose.deploy.yml"

docker compose $COMPOSE_FILES pull
docker compose $COMPOSE_FILES up -d
