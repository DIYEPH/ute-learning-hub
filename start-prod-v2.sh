#!/bin/bash
# Docker Compose V2 (docker compose) - Ubuntu 24.04+

echo "--> Starting UTE Learning Hub (Production)..."

if [ ! -f ".env.production" ]; then
    echo "[ERROR] .env.production not found!"
    exit 1
fi

docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production build --parallel
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d backend frontend ai nginx

echo "--> Services started!"
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production ps
