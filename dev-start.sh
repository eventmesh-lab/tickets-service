#!/bin/bash

# dev-start.sh
# Script para iniciar el servicio en modo desarrollo local (sin Docker)

echo "=== Tickets Service - Desarrollo Local ==="
echo ""

# Detener Docker si está corriendo
echo "1. Deteniendo contenedores Docker..."
docker-compose down 2>/dev/null

# Iniciar solo la base de datos
echo ""
echo "2. Iniciando base de datos PostgreSQL..."
docker-compose up db -d

# Esperar a que PostgreSQL esté listo
echo ""
echo "3. Esperando a que PostgreSQL esté listo..."
sleep 5

# Limpiar artefactos previos
echo ""
echo "4. Limpiando artefactos de compilación..."
dotnet clean tickets-service.sln --verbosity quiet

# Restaurar dependencias
echo ""
echo "5. Restaurando dependencias..."
dotnet restore tickets-service.sln --verbosity quiet

# Compilar solución
echo ""
echo "6. Compilando solución..."
dotnet build tickets-service.sln --verbosity quiet

# Verificar que la compilación fue exitosa
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Listo para desarrollo local!"
    echo ""
    echo "Opciones para ejecutar el servicio:"
    echo "  1. Desde tu IDE (F5 o Debug)"
    echo "  2. Manualmente: cd src/tickets-service.Api && dotnet run"
    echo "  3. Con hot reload: cd src/tickets-service.Api && dotnet watch run"
    echo ""
    echo "El servicio estará disponible en: http://localhost:5005"
    echo "PostgreSQL está corriendo en: localhost:5432"
    echo ""
else
    echo ""
    echo "❌ Error durante la compilación. Verifica los errores arriba."
    exit 1
fi


