#!/bin/bash

echo "--> Starting UTE Learning Hub (Production)..."

# Check if .env.production exists
if [ ! -f .env.production ]; then
    echo "--> Error: .env.production file not found!"
    echo "Please configure .env.production first."
    exit 1
fi

# Load environment variables
export $(cat .env.production | grep -v '^#' | xargs)

# Start services (WITHOUT SQL Server - using Azure SQL)
echo "--> Starting 6 services (Redis, Kafka, Backend, Frontend, AI, Nginx)..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d \
  redis zookeeper kafka backend frontend ai nginx

echo "--> All services started!"
echo ""
echo "--> Service status:"
docker-compose ps

echo ""
echo "--> NOTE: Using Azure SQL Database (external)"
echo "   Make sure your connection string is configured in .env.production"
echo ""
echo "--> View logs:"
echo "  docker-compose logs -f backend"
echo "  docker-compose logs -f frontend"
echo ""
echo "--> Access:"
echo "  Frontend: http://localhost"
echo "  Backend API: http://localhost/api"
