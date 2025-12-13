Write-Host "🚀 Starting UTE Learning Hub..." -ForegroundColor Green

# Load environment from .env.production
docker-compose --env-file .env.production -f docker-compose.yml -f docker-compose.prod.yml up -d redis zookeeper kafka backend frontend ai nginx

Write-Host "✅ Services started!" -ForegroundColor Green
docker-compose ps
