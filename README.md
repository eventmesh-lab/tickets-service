# Tickets Service

Microservicio responsable de la generación, gestión y validación de tickets (entradas) con códigos QR únicos. Implementa la lógica del agregado `Ticket` y su ciclo de vida según el lenguaje ubicuo del proyecto.

- Bounded Context: Reservas y Ticketing
- Repositorio raíz: `eventmesh-lab/tickets-service`

Para una descripción funcional y técnica más detallada, revisa la documentación en `docs/` (arquitectura, comandos/eventos, API y persistencia).

## Estructura

- `src/`
  - `tickets-service.Domain/`
  - `tickets-service.Application/`
  - `tickets-service.Infrastructure/`
  - `tickets-service.Api/`
- `tests/`
  - `tickets-service.Domain.Tests/`
  - `tickets-service.Application.Tests/`

## Desarrollo rápido

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build --no-restore

# Ejecutar tests
dotnet test --no-build

# (Opcional) Ejecutar con Docker
docker compose up -d --build
```

## Endpoints principales

- `POST /api/tickets/generar`: Genera tickets para una reserva
- `POST /api/tickets/confirmar`: Confirma tickets tras pago exitoso
- `POST /api/tickets/validar`: Valida ticket por código QR (check-in)
- `GET /api/tickets/{id}`: Detalle de ticket
- `GET /api/tickets/asistente/{asistenteId}`: Tickets por asistente
- `GET /api/tickets/evento/{eventoId}/estadisticas`: Estadísticas por evento

Más detalles y ejemplos en `docs/ARCHITECTURE.md`.

## Tecnologías

- .NET 8, Minimal APIs, MediatR (CQRS)
- PostgreSQL (EF Core 8)
- RabbitMQ (eventos de dominio)
- QRCoder/ZXing.Net (generación de QR)
- Serilog, OpenTelemetry

## Documentación

- Arquitectura y dominio: `docs/ARCHITECTURE.md`
- Guía de la plantilla original: `docs/TEMPLATE.md` (contenido movido desde el README anterior)

## Notas

- Este README fue adaptado al servicio. La guía de uso de la plantilla se conservó en `docs/TEMPLATE.md`.
