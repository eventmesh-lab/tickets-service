# docker-start.ps1
# Script para iniciar el servicio con Docker en modo producción

Write-Host "=== Tickets Service - Docker Producción ===" -ForegroundColor Cyan
Write-Host ""

# Detener contenedores existentes
Write-Host "1. Deteniendo contenedores existentes..." -ForegroundColor Yellow
docker-compose down

# Limpiar artefactos locales para evitar conflictos
Write-Host ""
Write-Host "2. Limpiando artefactos de compilación local..." -ForegroundColor Yellow
dotnet clean tickets-service.sln --verbosity quiet 2>$null

# Construir y ejecutar con Docker
Write-Host ""
Write-Host "3. Construyendo imagen Docker..." -ForegroundColor Yellow
docker-compose build --no-cache api

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "4. Iniciando servicios con Docker..." -ForegroundColor Yellow
    docker-compose up -d
    
    Write-Host ""
    Write-Host "5. Esperando a que los servicios inicien..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    Write-Host ""
    Write-Host "✅ Servicios iniciados correctamente!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Servicios disponibles:" -ForegroundColor Cyan
    Write-Host "  - API: http://localhost:5005"
    Write-Host "  - PostgreSQL: localhost:5432"
    Write-Host ""
    Write-Host "Comandos útiles:" -ForegroundColor Cyan
    Write-Host "  - Ver logs: docker-compose logs -f api"
    Write-Host "  - Detener: docker-compose down"
    Write-Host "  - Estado: docker-compose ps"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ Error durante la construcción de la imagen Docker." -ForegroundColor Red
    exit 1
}


