#!/bin/bash
# Docker Compose V2 (docker compose) - Ubuntu 24.04+

echo "--> Starting UTE Learning Hub (Production)..."

COMPOSE_FILES="-f docker-compose.yml -f docker-compose.prod.yml"
ENV_FILE="--env-file .env.production"

if [ ! -f ".env.production" ]; then
    echo "[ERROR] .env.production not found!"
    exit 1
fi

docker compose $ENV_FILE $COMPOSE_FILES build --parallel
docker compose $ENV_FILE $COMPOSE_FILES up -d backend frontend ai nginx

echo "--> Services started!"
docker compose $ENV_FILE $COMPOSE_FILES ps
