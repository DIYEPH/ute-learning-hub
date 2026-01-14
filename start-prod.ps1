Write-Host "--> Starting UTE Learning Hub (Production)..." -ForegroundColor Green

$composeFiles = "-f docker-compose.yml -f docker-compose.prod.yml"
$envFile = "--env-file .env.production"

if (!(Test-Path ".env.production")) {
    Write-Host "[ERROR] .env.production not found!" -ForegroundColor Red
    exit 1
}

Invoke-Expression "docker-compose $envFile $composeFiles build --parallel"
Invoke-Expression "docker-compose $envFile $composeFiles up -d backend frontend ai nginx"

Write-Host "--> Services started!" -ForegroundColor Green
Invoke-Expression "docker-compose $envFile $composeFiles ps"
