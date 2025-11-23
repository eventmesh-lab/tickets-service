# Tickets Service

Servicio de gestión de tickets y reservas para EventMesh.

## 🚀 Inicio Rápido

### Prerequisitos

- .NET 8.0 SDK
- Docker y Docker Compose
- PostgreSQL (si ejecutas localmente sin Docker)

### Scripts Helper Disponibles

El proyecto incluye scripts para simplificar las operaciones comunes de desarrollo:

#### Linux/Mac/Git Bash

```bash
# Desarrollo local (sin Docker)
./dev-start.sh

# Docker en modo producción
./docker-start.sh

# Docker en modo desarrollo con hot reload
./docker-dev-start.sh

# Limpieza profunda
./clean-all.sh
```

#### Windows PowerShell

```powershell
# Desarrollo local (sin Docker)
./dev-start.ps1

# Docker en modo producción
./docker-start.ps1

# Docker en modo desarrollo con hot reload
./docker-dev-start.ps1

# Limpieza profunda
./clean-all.ps1
```

## 📖 Documentación

- **[Guía de Desarrollo](docs/DESARROLLO.md)**: Flujos de trabajo, troubleshooting y mejores prácticas
- **[Arquitectura](docs/ARCHITECTURE.md)**: Diseño y arquitectura del servicio
- **[Lenguaje Ubicuo](docs/lenguaje-ubicuo.md)**: Términos del dominio
- **[Ficha del Servicio](docs/tickets-service.md)**: Descripción completa del servicio

## 🏗️ Arquitectura

El proyecto sigue los principios de:

- **Arquitectura Hexagonal (Ports & Adapters)**
- **Domain-Driven Design (DDD)**
- **CQRS** con MediatR
- **Event-Driven Architecture**

### Estructura de Capas

``` shell
src/
├── tickets-service.Api/           # API REST (Minimal APIs)
├── tickets-service.Application/   # Casos de uso (CQRS)
├── tickets-service.Domain/        # Lógica de negocio
└── tickets-service.Infrastructure/# Adaptadores (DB, MQ, HTTP)

tests/
├── tickets-service.Api.Tests/
├── tickets-service.Application.Tests/
├── tickets-service.Domain.Tests/
└── tickets-service.Infrastructure.IntegrationTests/
```

## 🔧 Desarrollo

### Opción 1: Desarrollo Local (Recomendado)

**Para desarrollo activo con debugging:**

1. Ejecutar el script:

   ```bash
   ./dev-start.sh  # Linux/Mac/Git Bash
   # O
   ./dev-start.ps1  # PowerShell
   ```

2. El script:
   - Detiene Docker si está corriendo
   - Inicia solo PostgreSQL
   - Limpia y compila el proyecto
   - Te prepara para ejecutar desde el IDE

3. Ejecutar desde tu IDE (F5) o manualmente:

   ```bash
   cd src/tickets-service.Api
   dotnet run
   ```

### Opción 2: Docker (Producción)

**Para testing de integración o CI/CD:**

```bash
./docker-start.sh  # Linux/Mac/Git Bash
# O
./docker-start.ps1  # PowerShell
```

O manualmente:

```bash
docker-compose up --build
```

### Opción 3: Docker (Desarrollo con Hot Reload)

**Para desarrollo con Docker pero con hot reload:**

```bash
./docker-dev-start.sh  # Linux/Mac/Git Bash
# O
./docker-dev-start.ps1  # PowerShell
```

O manualmente:

```bash
docker-compose --profile dev up api-dev
```

## 🧪 Testing

```bash
# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test tests/tickets-service.Domain.Tests/

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## ⚠️ Troubleshooting

### Error: "The file is locked by: tickets_service.Api"

**Solución rápida:**

```bash
./clean-all.sh   # Linux/Mac/Git Bash
# O
./clean-all.ps1  # PowerShell
```

**Solución manual:**

1. Detener Docker: `docker-compose down`
2. Limpiar: `dotnet clean tickets-service.sln`
3. Recompilar: `dotnet build tickets-service.sln`

Ver más detalles en [docs/DESARROLLO.md](docs/DESARROLLO.md#troubleshooting).

## 🌐 Endpoints

Una vez corriendo, el servicio está disponible en:

- **API**: http://localhost:5005
- **Swagger** (Development): http://localhost:5005/swagger

## 📊 Base de Datos

### PostgreSQL (Docker)

- **Host**: localhost
- **Puerto**: 5432
- **Base de datos**: tickets_service
- **Usuario**: tickets
- **Password**: tickets

### Migraciones

```bash
# Crear migración
dotnet ef migrations add MigrationName --project src/tickets-service.Infrastructure --startup-project src/tickets-service.Api

# Aplicar migraciones
dotnet ef database update --project src/tickets-service.Infrastructure --startup-project src/tickets-service.Api
```

## 🔑 Variables de Entorno

### Desarrollo Local

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__TicketsDb="Host=localhost;Port=5432;Database=tickets_service;Username=tickets;Password=tickets"
```

### Docker

Las variables están configuradas en `docker-compose.yml`.

## 📝 Comandos Útiles

```bash
# Verificar estado de Docker
docker-compose ps

# Ver logs
docker-compose logs -f api

# Limpiar todo
./clean-all.sh  # o clean-all.ps1

# Compilar
dotnet build tickets-service.sln

# Ejecutar con hot reload
cd src/tickets-service.Api
dotnet watch run
```

## 📄 Licencia

[MIT License](LICENSE)

## 🔗 Enlaces

- [Documentación Central](https://eventmesh-lab.github.io/org-docs/services/tickets-service/)
- [Guía Técnica Global](https://eventmesh-lab.github.io/org-docs/guia-tecnica/)
- [EventMesh Lab](https://github.com/eventmesh-lab)

---

**Nota**: Consulta [docs/DESARROLLO.md](docs/DESARROLLO.md) para documentación detallada de desarrollo y troubleshooting.
