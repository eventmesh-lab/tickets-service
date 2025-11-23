#!/bin/bash

# docker-dev-start.sh
# Script para iniciar el servicio con Docker en modo desarrollo con hot reload

echo "=== Tickets Service - Docker Desarrollo (Hot Reload) ==="
echo ""

# Detener contenedores existentes
echo "1. Deteniendo contenedores existentes..."
docker-compose down

# Limpiar artefactos locales
echo ""
echo "2. Limpiando artefactos de compilación local..."
dotnet clean tickets-service.sln --verbosity quiet 2>/dev/null

# Construir imagen de desarrollo
echo ""
echo "3. Construyendo imagen Docker (modo desarrollo)..."
docker-compose build --no-cache api-dev

if [ $? -eq 0 ]; then
    echo ""
    echo "4. Iniciando servicios con Docker (modo desarrollo)..."
    docker-compose --profile dev up -d
    
    echo ""
    echo "5. Esperando a que los servicios inicien..."
    sleep 5
    
    echo ""
    echo "✅ Servicios iniciados en modo desarrollo!"
    echo ""
    echo "Servicios disponibles:"
    echo "  - API: http://localhost:5005"
    echo "  - PostgreSQL: localhost:5432"
    echo ""
    echo "Características del modo desarrollo:"
    echo "  ✓ Hot reload habilitado (dotnet watch)"
    echo "  ✓ Código fuente montado como volumen"
    echo "  ✓ Cambios se reflejan automáticamente"
    echo ""
    echo "Comandos útiles:"
    echo "  - Ver logs: docker-compose --profile dev logs -f api-dev"
    echo "  - Detener: docker-compose --profile dev down"
    echo "  - Estado: docker-compose ps"
    echo ""
else
    echo ""
    echo "❌ Error durante la construcción de la imagen Docker."
    exit 1
fi


