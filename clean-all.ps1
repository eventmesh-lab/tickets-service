# clean-all.ps1
# Script para limpieza profunda de artefactos de compilación y contenedores Docker

Write-Host "=== Tickets Service - Limpieza Profunda ===" -ForegroundColor Cyan
Write-Host ""

# Detener todos los contenedores
Write-Host "1. Deteniendo todos los contenedores Docker..." -ForegroundColor Yellow
docker-compose down -v

# Limpiar artefactos de .NET
Write-Host ""
Write-Host "2. Limpiando artefactos de compilación .NET..." -ForegroundColor Yellow
dotnet clean tickets-service.sln --verbosity quiet

# Eliminar carpetas bin y obj manualmente
Write-Host ""
Write-Host "3. Eliminando carpetas bin y obj..." -ForegroundColor Yellow
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory -Force | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Limpiar imágenes Docker no utilizadas
Write-Host ""
Write-Host "4. Limpiando imágenes y volúmenes Docker no utilizados..." -ForegroundColor Yellow
docker system prune -f

# Eliminar caché de NuGet (opcional, comentado por defecto)
# Write-Host ""
# Write-Host "5. Limpiando caché de NuGet..." -ForegroundColor Yellow
# dotnet nuget locals all --clear

Write-Host ""
Write-Host "✅ Limpieza completa realizada!" -ForegroundColor Green
Write-Host ""
Write-Host "Para volver a trabajar:" -ForegroundColor Cyan
Write-Host "  - Desarrollo local: ./dev-start.ps1"
Write-Host "  - Docker producción: ./docker-start.ps1"
Write-Host "  - Docker desarrollo: ./docker-dev-start.ps1"
Write-Host ""


