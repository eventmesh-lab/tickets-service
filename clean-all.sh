#!/bin/bash

# clean-all.sh
# Script para limpieza profunda de artefactos de compilación y contenedores Docker

echo "=== Tickets Service - Limpieza Profunda ==="
echo ""

# Detener todos los contenedores
echo "1. Deteniendo todos los contenedores Docker..."
docker-compose down -v

# Limpiar artefactos de .NET
echo ""
echo "2. Limpiando artefactos de compilación .NET..."
dotnet clean tickets-service.sln --verbosity quiet

# Eliminar carpetas bin y obj manualmente
echo ""
echo "3. Eliminando carpetas bin y obj..."
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null

# Limpiar imágenes Docker no utilizadas
echo ""
echo "4. Limpiando imágenes y volúmenes Docker no utilizados..."
docker system prune -f

# Eliminar caché de NuGet (opcional, comentado por defecto)
# echo ""
# echo "5. Limpiando caché de NuGet..."
# dotnet nuget locals all --clear

echo ""
echo "✅ Limpieza completa realizada!"
echo ""
echo "Para volver a trabajar:"
echo "  - Desarrollo local: ./dev-start.sh"
echo "  - Docker producción: ./docker-start.sh"
echo "  - Docker desarrollo: ./docker-dev-start.sh"
echo ""


