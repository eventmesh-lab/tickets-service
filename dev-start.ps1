# dev-start.ps1
# Script para iniciar el servicio en modo desarrollo local (sin Docker)

Write-Host "=== Tickets Service - Desarrollo Local ===" -ForegroundColor Cyan
Write-Host ""

# Detener Docker si está corriendo
Write-Host "1. Deteniendo contenedores Docker..." -ForegroundColor Yellow
docker-compose down 2>$null

# Iniciar solo la base de datos
Write-Host ""
Write-Host "2. Iniciando base de datos PostgreSQL..." -ForegroundColor Yellow
docker-compose up db -d

# Esperar a que PostgreSQL esté listo
Write-Host ""
Write-Host "3. Esperando a que PostgreSQL esté listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Limpiar artefactos previos
Write-Host ""
Write-Host "4. Limpiando artefactos de compilación..." -ForegroundColor Yellow
dotnet clean tickets-service.sln --verbosity quiet

# Restaurar dependencias
Write-Host ""
Write-Host "5. Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore tickets-service.sln --verbosity quiet

# Compilar solución
Write-Host ""
Write-Host "6. Compilando solución..." -ForegroundColor Yellow
dotnet build tickets-service.sln --verbosity quiet

# Verificar que la compilación fue exitosa
if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Listo para desarrollo local!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Opciones para ejecutar el servicio:" -ForegroundColor Cyan
    Write-Host "  1. Desde tu IDE (F5 o Debug)"
    Write-Host "  2. Manualmente: cd src/tickets-service.Api; dotnet run"
    Write-Host "  3. Con hot reload: cd src/tickets-service.Api; dotnet watch run"
    Write-Host ""
    Write-Host "El servicio estará disponible en: http://localhost:5005"
    Write-Host "PostgreSQL está corriendo en: localhost:5432"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ Error durante la compilación. Verifica los errores arriba." -ForegroundColor Red
    exit 1
}


