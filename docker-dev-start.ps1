# docker-dev-start.ps1
# Script para iniciar el servicio con Docker en modo desarrollo con hot reload

Write-Host "=== Tickets Service - Docker Desarrollo (Hot Reload) ===" -ForegroundColor Cyan
Write-Host ""

# Detener contenedores existentes
Write-Host "1. Deteniendo contenedores existentes..." -ForegroundColor Yellow
docker-compose down

# Limpiar artefactos locales
Write-Host ""
Write-Host "2. Limpiando artefactos de compilación local..." -ForegroundColor Yellow
dotnet clean tickets-service.sln --verbosity quiet 2>$null

# Construir imagen de desarrollo
Write-Host ""
Write-Host "3. Construyendo imagen Docker (modo desarrollo)..." -ForegroundColor Yellow
docker-compose build --no-cache api-dev

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "4. Iniciando servicios con Docker (modo desarrollo)..." -ForegroundColor Yellow
    docker-compose --profile dev up -d
    
    Write-Host ""
    Write-Host "5. Esperando a que los servicios inicien..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    Write-Host ""
    Write-Host "✅ Servicios iniciados en modo desarrollo!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Servicios disponibles:" -ForegroundColor Cyan
    Write-Host "  - API: http://localhost:5005"
    Write-Host "  - PostgreSQL: localhost:5432"
    Write-Host ""
    Write-Host "Características del modo desarrollo:" -ForegroundColor Cyan
    Write-Host "  ✓ Hot reload habilitado (dotnet watch)"
    Write-Host "  ✓ Código fuente montado como volumen"
    Write-Host "  ✓ Cambios se reflejan automáticamente"
    Write-Host ""
    Write-Host "Comandos útiles:" -ForegroundColor Cyan
    Write-Host "  - Ver logs: docker-compose --profile dev logs -f api-dev"
    Write-Host "  - Detener: docker-compose --profile dev down"
    Write-Host "  - Estado: docker-compose ps"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ Error durante la construcción de la imagen Docker." -ForegroundColor Red
    exit 1
}


