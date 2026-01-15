#!/bin/bash
DOMAIN="utelearninghub.duckdns.org"
EMAIL="dinhyenphan@gmail.com"

mkdir -p certbot/www certbot/conf

docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d nginx
sleep 5

docker run --rm \
    -v "$(pwd)/certbot/www:/var/www/certbot" \
    -v "$(pwd)/certbot/conf:/etc/letsencrypt" \
    certbot/certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email $EMAIL \
    --agree-tos \
    --no-eff-email \
    -d $DOMAIN

echo "Done! Now run: cp nginx/nginx-ssl.conf nginx/nginx.conf && docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production restart nginx"
