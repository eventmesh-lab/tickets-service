#!/bin/bash

# docker-start.sh
# Script para iniciar el servicio con Docker en modo producción

echo "=== Tickets Service - Docker Producción ==="
echo ""

# Detener contenedores existentes
echo "1. Deteniendo contenedores existentes..."
docker-compose down

# Limpiar artefactos locales para evitar conflictos
echo ""
echo "2. Limpiando artefactos de compilación local..."
dotnet clean tickets-service.sln --verbosity quiet 2>/dev/null

# Construir y ejecutar con Docker
echo ""
echo "3. Construyendo imagen Docker..."
docker-compose build --no-cache api

if [ $? -eq 0 ]; then
    echo ""
    echo "4. Iniciando servicios con Docker..."
    docker-compose up -d
    
    echo ""
    echo "5. Esperando a que los servicios inicien..."
    sleep 5
    
    echo ""
    echo "✅ Servicios iniciados correctamente!"
    echo ""
    echo "Servicios disponibles:"
    echo "  - API: http://localhost:5005"
    echo "  - PostgreSQL: localhost:5432"
    echo ""
    echo "Comandos útiles:"
    echo "  - Ver logs: docker-compose logs -f api"
    echo "  - Detener: docker-compose down"
    echo "  - Estado: docker-compose ps"
    echo ""
else
    echo ""
    echo "❌ Error durante la construcción de la imagen Docker."
    exit 1
fi


