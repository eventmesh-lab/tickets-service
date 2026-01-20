# Tickets Service

Microservicio de gestión de tickets y validación de acceso para eventos en EventMesh.

## 📋 Descripción

Este servicio resuelve el problema de negocio de **generar, gestionar y validar tickets digitales con códigos QR** para eventos. Permite:

- Emisión de tickets asociados a reservas
- Confirmación tras pago exitoso
- Validación de acceso en tiempo real (check-in)
- Gestión del ciclo de vida completo del ticket
- Prevención de fraude mediante códigos QR únicos

## 📚 Tabla de Contenidos

- **[Arquitectura](docs/architecture.md)** - Flujo de datos, dependencias y modelo de dominio
- **[API](docs/api.md)** - Documentación de endpoints con ejemplos
- **[Setup](docs/setup.md)** - Configuración detallada y variables de entorno
- **[Guía de Desarrollo](docs/DESARROLLO.md)** - Flujos de trabajo y troubleshooting
- **[Lenguaje Ubicuo](docs/lenguaje-ubicuo.md)** - Términos del dominio

## 🛠️ Stack Tecnológico

- **.NET 8.0** - Framework principal
- **ASP.NET Core Minimal APIs** - Capa de presentación
- **MediatR** - Patrón CQRS
- **FluentValidation** - Validación de comandos
- **Entity Framework Core 8** - ORM
- **PostgreSQL 15** - Base de datos
- **Docker & Docker Compose** - Containerización

## 🚀 Quick Start

```bash
# Opción 1: Docker (recomendado para testing rápido)
docker-compose up --build

# Opción 2: Desarrollo local con .NET
./dev-start.sh  # Linux/Mac/Git Bash
./dev-start.ps1  # Windows PowerShell

# Opción 3: Docker con hot reload
./docker-dev-start.sh  # Linux/Mac/Git Bash
./docker-dev-start.ps1  # Windows PowerShell
```

El servicio estará disponible en:
- **API**: http://localhost:5005
- **Swagger UI**: http://localhost:5005/swagger

## 📖 Más Información

Ver documentación completa en la carpeta [docs/](docs/).
