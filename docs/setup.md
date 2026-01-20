# Setup Guide - Tickets Service

Guía detallada de configuración, variables de entorno, Docker y scripts disponibles.

## 📋 Prerequisitos

### Software Requerido

- **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Descargar](https://www.docker.com/products/docker-desktop)
- **Docker Compose** (incluido en Docker Desktop)
- **PostgreSQL 15** (solo si ejecutas sin Docker)

### Software Opcional

- **Visual Studio 2022** o **VS Code** con extensión C#
- **Git** para control de versiones
- **pgAdmin** o **DBeaver** para gestión de base de datos

---

## 🔧 Variables de Entorno

### Tabla Completa de Variables

| Variable | Tipo | Requerida | Default | Descripción |
|----------|------|-----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | String | No | `Production` | Entorno de ejecución (`Development`, `Staging`, `Production`) |
| `ConnectionStrings__TicketsDb` | String | Sí | N/A | Cadena de conexión a PostgreSQL |
| `EventsServiceUrl` | String | Sí | N/A | URL base del microservicio de eventos |
| `Logging__LogLevel__Default` | String | No | `Information` | Nivel de logging general |
| `Logging__LogLevel__Microsoft.AspNetCore` | String | No | `Warning` | Nivel de logging de ASP.NET Core |

### ConnectionStrings__TicketsDb

**Formato**:
```
Host={host};Port={port};Database={database};Username={username};Password={password}
```

**Ejemplos**:

```bash
# Desarrollo Local
ConnectionStrings__TicketsDb="Host=localhost;Port=5432;Database=tickets_service;Username=tickets;Password=tickets"

# Docker Compose (nombre del servicio como host)
ConnectionStrings__TicketsDb="Host=db;Port=5432;Database=tickets_service;Username=tickets;Password=tickets"

# Producción (con SSL)
ConnectionStrings__TicketsDb="Host=prod-db.example.com;Port=5432;Database=tickets_service;Username=tickets_prod;Password=SecurePass123;SSL Mode=Require"
```

### EventsServiceUrl

URL base del microservicio `events-service` para consultar disponibilidad.

**Ejemplos**:

```bash
# Desarrollo Local
EventsServiceUrl="http://localhost:5002"

# Docker Compose
EventsServiceUrl="http://events-service:80"

# Producción
EventsServiceUrl="https://events-api.eventmesh.com"
```

---

## 🐳 Configuración Docker

### Dockerfile Multi-Stage

El proyecto usa un Dockerfile con 3 stages:

#### Stage 1: Build
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```
- Copia archivos del proyecto
- Restaura dependencias con `dotnet restore`
- Compila en modo Release con `dotnet publish`

#### Stage 2: Development
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
```
- Basado en SDK (no runtime) para soportar hot reload
- Monta código fuente como volumen
- Ejecuta `dotnet watch run` para desarrollo interactivo

#### Stage 3: Runtime (Producción)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
```
- Imagen optimizada solo con runtime (más pequeña)
- Copia artefactos compilados del stage de build
- Ejecuta la aplicación con `dotnet tickets_service.Api.dll`

### docker-compose.yml

El proyecto define 3 servicios:

#### Servicio: db (PostgreSQL)

```yaml
db:
  image: postgres:15-alpine
  container_name: tickets-db
  ports:
    - "5434:5432"  # Mapeado a 5434 para evitar conflictos
  environment:
    POSTGRES_USER: tickets
    POSTGRES_PASSWORD: tickets
    POSTGRES_DB: tickets_service
  volumes:
    - tickets_pg_data:/var/lib/postgresql/data
```

**Notas**:
- Puerto externo `5434` (no `5432`) para evitar conflictos con PostgreSQL local
- Los datos persisten en el volumen `tickets_pg_data`

#### Servicio: api (Producción)

```yaml
api:
  build:
    context: .
    dockerfile: Dockerfile
    target: runtime
  ports:
    - "5005:80"
  depends_on:
    - db
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__TicketsDb: "Host=db;Port=5432;..."
    EventsServiceUrl: "http://events-service:80"
```

**Uso**: `docker-compose up api`

#### Servicio: api-dev (Desarrollo con Hot Reload)

```yaml
api-dev:
  build:
    target: development
  volumes:
    - ./src:/app/src:ro
  profiles:
    - dev
```

**Características**:
- Monta código fuente como volumen read-only
- Cambios en código se reflejan automáticamente
- Usa perfil `dev` (no se inicia por defecto)

**Uso**: `docker-compose --profile dev up api-dev`

### Construcción de Imagen

```bash
# Construcción manual (producción)
docker build -t tickets-service:latest --target runtime .

# Construcción manual (desarrollo)
docker build -t tickets-service:dev --target development .

# Construcción con docker-compose
docker-compose build api
```

### Gestión de Contenedores

```bash
# Iniciar servicios
docker-compose up

# Iniciar en background
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes (⚠️ borra datos)
docker-compose down -v

# Reconstruir imágenes
docker-compose up --build
```

---

## 📜 Scripts Helper

El proyecto incluye scripts en shell y PowerShell para simplificar operaciones comunes.

### dev-start.sh / dev-start.ps1

**Propósito**: Preparar entorno para desarrollo local (sin Docker, solo PostgreSQL dockerizado).

**Qué hace**:
1. Detiene contenedores Docker si están corriendo
2. Inicia solo el servicio `db` (PostgreSQL)
3. Limpia artefactos de compilación: `dotnet clean`
4. Compila el proyecto: `dotnet build`
5. Muestra instrucciones para ejecutar desde el IDE

**Uso**:
```bash
# Linux/Mac/Git Bash
./dev-start.sh

# Windows PowerShell
./dev-start.ps1
```

**Cuándo usar**: Desarrollo activo con debugging en Visual Studio o VS Code.

---

### docker-start.sh / docker-start.ps1

**Propósito**: Levantar todo el stack en Docker (modo producción).

**Qué hace**:
1. Detiene contenedores previos
2. Ejecuta `docker-compose up --build` para construir y levantar `api` + `db`

**Uso**:
```bash
# Linux/Mac/Git Bash
./docker-start.sh

# Windows PowerShell
./docker-start.ps1
```

**Cuándo usar**: Testing de integración, CI/CD, o demostración a stakeholders.

---

### docker-dev-start.sh / docker-dev-start.ps1

**Propósito**: Levantar el stack en modo desarrollo con hot reload.

**Qué hace**:
1. Detiene contenedores previos
2. Ejecuta `docker-compose --profile dev up api-dev`
3. Monta código fuente como volumen para hot reload

**Uso**:
```bash
# Linux/Mac/Git Bash
./docker-dev-start.sh

# Windows PowerShell
./docker-dev-start.ps1
```

**Cuándo usar**: Desarrollo en Docker sin salir del contenedor (útil para ambientes no-Windows).

---

### clean-all.sh / clean-all.ps1

**Propósito**: Limpieza profunda de artefactos y contenedores.

**Qué hace**:
1. Detiene y elimina contenedores Docker
2. Limpia artefactos: `dotnet clean`
3. Elimina carpetas `bin/` y `obj/` recursivamente

**Uso**:
```bash
# Linux/Mac/Git Bash
./clean-all.sh

# Windows PowerShell
./clean-all.ps1
```

**Cuándo usar**: Resolver problemas de archivos bloqueados o estado inconsistente.

---

## 🛠️ Scripts de .NET

### Compilación

```bash
# Compilar todo el solution
dotnet build tickets-service.sln

# Compilar en Release
dotnet build tickets-service.sln -c Release

# Compilar solo un proyecto
dotnet build src/tickets-service.Api/tickets-service.Api.csproj
```

### Ejecución

```bash
# Ejecutar API (desde carpeta del proyecto)
cd src/tickets-service.Api
dotnet run

# Ejecutar con hot reload (watch mode)
dotnet watch run

# Ejecutar especificando puerto
dotnet run --urls "http://localhost:5005"
```

### Testing

```bash
# Ejecutar todos los tests
dotnet test tickets-service.sln

# Ejecutar tests de un proyecto específico
dotnet test tests/tickets-service.Domain.Tests/

# Ejecutar con cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar con logs detallados
dotnet test --logger "console;verbosity=detailed"
```

### Migraciones de Entity Framework

```bash
# Ver comandos disponibles
dotnet ef --help

# Agregar migración
dotnet ef migrations add MigrationName \
  --project src/tickets-service.Infrastructure \
  --startup-project src/tickets-service.Api

# Aplicar migraciones
dotnet ef database update \
  --project src/tickets-service.Infrastructure \
  --startup-project src/tickets-service.Api

# Generar script SQL
dotnet ef migrations script \
  --project src/tickets-service.Infrastructure \
  --startup-project src/tickets-service.Api \
  --output migrations.sql

# Revertir última migración
dotnet ef database update PreviousMigrationName \
  --project src/tickets-service.Infrastructure \
  --startup-project src/tickets-service.Api
```

---

## 🗄️ Configuración de Base de Datos

### PostgreSQL Local (sin Docker)

**Instalación**:
```bash
# Ubuntu/Debian
sudo apt-get install postgresql-15

# macOS (Homebrew)
brew install postgresql@15

# Windows (Installer)
# Descargar de https://www.postgresql.org/download/windows/
```

**Crear base de datos**:
```sql
CREATE DATABASE tickets_service;
CREATE USER tickets WITH PASSWORD 'tickets';
GRANT ALL PRIVILEGES ON DATABASE tickets_service TO tickets;
```

**Configurar conexión**:
```bash
export ConnectionStrings__TicketsDb="Host=localhost;Port=5432;Database=tickets_service;Username=tickets;Password=tickets"
```

### PostgreSQL en Docker (standalone)

```bash
docker run -d \
  --name tickets-postgres \
  -e POSTGRES_USER=tickets \
  -e POSTGRES_PASSWORD=tickets \
  -e POSTGRES_DB=tickets_service \
  -p 5432:5432 \
  -v tickets_data:/var/lib/postgresql/data \
  postgres:15-alpine
```

### Backup y Restore

```bash
# Backup
docker exec tickets-db pg_dump -U tickets tickets_service > backup.sql

# Restore
docker exec -i tickets-db psql -U tickets tickets_service < backup.sql
```

---

## 🌍 Configuración por Entorno

### Desarrollo Local

**appsettings.Development.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "TicketsDb": "Host=localhost;Port=5432;Database=tickets_service;Username=tickets;Password=tickets"
  },
  "EventsServiceUrl": "http://localhost:5002"
}
```

### Docker Compose

Variables configuradas en `docker-compose.yml`:
```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Development
  ConnectionStrings__TicketsDb: "Host=db;Port=5432;..."
  EventsServiceUrl: "http://events-service:80"
```

### Producción (Kubernetes)

**ConfigMap**:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: tickets-service-config
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  EventsServiceUrl: "http://events-service.default.svc.cluster.local"
```

**Secret**:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: tickets-service-secret
type: Opaque
stringData:
  ConnectionStrings__TicketsDb: "Host=prod-db;Port=5432;Database=tickets_service;Username=tickets;Password=SecurePassword123"
```

---

## 🚨 Troubleshooting

### Error: "The file is locked"

**Síntoma**: 
```
The process cannot access the file '...\tickets_service.Api.dll' because it is being used by another process.
```

**Solución**:
```bash
# Opción 1: Script de limpieza
./clean-all.sh  # o .ps1

# Opción 2: Manual
docker-compose down
dotnet clean tickets-service.sln
```

### Error: "Unable to connect to database"

**Verificaciones**:
```bash
# 1. Verificar que PostgreSQL esté corriendo
docker-compose ps

# 2. Verificar logs de la base de datos
docker-compose logs db

# 3. Probar conexión manualmente
docker exec -it tickets-db psql -U tickets -d tickets_service

# 4. Verificar variables de entorno
echo $ConnectionStrings__TicketsDb
```

### Error: "Port 5005 already in use"

**Solución**:
```bash
# Encontrar proceso usando el puerto
# Linux/Mac
lsof -i :5005
kill -9 <PID>

# Windows
netstat -ano | findstr :5005
taskkill /PID <PID> /F

# O cambiar puerto en docker-compose.yml
ports:
  - "5006:80"  # Usar 5006 en lugar de 5005
```

### Error: "EventsServiceUrl not configured"

**Síntoma**:
```
InvalidOperationException: EventsServiceUrl no está configurado.
```

**Solución**:
```bash
# Agregar variable de entorno
export EventsServiceUrl="http://localhost:5002"

# O en docker-compose.yml
environment:
  EventsServiceUrl: "http://events-service:80"
```

---

## 📚 Referencias

- [Arquitectura](architecture.md)
- [API Documentation](api.md)
- [.NET CLI Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
