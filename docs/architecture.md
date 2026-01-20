# Arquitectura del Tickets Service

Este documento explica cómo funciona internamente el servicio de tickets, su flujo de datos, dependencias externas y el modelo de datos implementado.

## 📊 Flujo de Datos

### Visión General

El servicio implementa una **arquitectura hexagonal (Ports & Adapters)** con **CQRS** utilizando MediatR. Las peticiones HTTP atraviesan las siguientes capas:

```
HTTP Request
    ↓
Controller (Minimal API Endpoints)
    ↓
Command/Query (MediatR)
    ↓
Command/Query Handler (Application Layer)
    ↓
Domain Service / Aggregate Root (Domain Layer)
    ↓
Repository (Infrastructure Layer)
    ↓
Database (PostgreSQL)
```

### Flujo Detallado por Caso de Uso

#### 1. Generar Tickets (POST /api/tickets/generar)

```
1. Minimal API recibe GenerarTicketsRequest
   ↓
2. Mapea a GenerarTicketsCommand
   ↓
3. MediatR enruta al GenerarTicketsCommandHandler
   ↓
4. Handler valida con FluentValidation
   ↓
5. Consulta IEventAvailabilityGateway (HTTP → events-service)
   - Verifica que evento esté "Publicado"
   - Verifica capacidad disponible por sección
   ↓
6. Por cada item, crea Ticket.Crear() (Domain)
   - Genera CodigoQR único
   - Estado inicial: Pendiente
   - Emite evento de dominio: TicketsGenerados
   ↓
7. Persiste tickets vía ITicketRepository
   ↓
8. Retorna GenerarTicketsResult con IDs generados
```

**Validaciones aplicadas:**
- EventoId, ReservaId, AsistenteId no vacíos
- Precio > 0
- Si hay asiento, debe tener sección
- Capacidad disponible suficiente

#### 2. Confirmar Tickets (POST /api/tickets/confirmar)

```
1. Minimal API recibe ConfirmarTicketsRequest
   ↓
2. Mapea a ConfirmarTicketsCommand
   ↓
3. Handler recupera tickets del repositorio
   ↓
4. Invoca Ticket.Confirmar(pagoId, fecha) en cada ticket
   - Valida estado == Pendiente
   - Cambia estado a Confirmado
   - Emite evento: TicketsConfirmados
   ↓
5. Persiste cambios
   ↓
6. Retorna 204 No Content
```

#### 3. Validar Ticket (POST /api/tickets/validar)

```
1. Minimal API recibe ValidarTicketRequest
   ↓
2. Mapea a ValidarTicketCommand
   ↓
3. Handler busca ticket por CodigoQR
   ↓
4. Invoca Ticket.Validar(ubicacion, validadorId, fecha)
   - Valida estado == Confirmado
   - Cambia estado a Usado
   - Registra ubicación y validador
   - Emite evento: TicketValidado
   ↓
5. Persiste cambios
   ↓
6. Retorna 200 OK
```

#### 4. Cancelar Ticket (POST /api/tickets/cancelar)

```
1. Minimal API recibe CancelarTicketRequest
   ↓
2. Mapea a CancelarTicketCommand
   ↓
3. Handler recupera ticket
   ↓
4. Invoca Ticket.Cancelar(razon, fecha)
   - Valida estado != Usado
   - Cambia estado a Cancelado
   - Emite evento: TicketCancelado
   ↓
5. Persiste cambios
   ↓
6. Retorna 204 No Content
```

#### 5. Verificar Acceso (GET /api/tickets/check-access)

```
1. Minimal API recibe query params: eventId, userId
   ↓
2. Mapea a CheckAccessQuery
   ↓
3. Handler busca ticket activo del usuario para el evento
   ↓
4. Retorna información de acceso:
   - hasAccess: true/false
   - ticketId, ticketType, status
```

## 🔗 Dependencias Externas

### 1. PostgreSQL Database

**Propósito**: Persistencia de tickets y estado del agregado.

**Conexión**: 
- Vía Entity Framework Core + Npgsql
- Connection string configurable por variable de entorno `ConnectionStrings__TicketsDb`

**Tablas principales**:
- `Tickets`: Almacena el agregado raíz
- Índices en: `codigo_qr_valor`, `evento_id`, `asistente_id`

### 2. Events Service (Microservicio Externo)

**Propósito**: Consultar disponibilidad de eventos y capacidad por sección.

**Puerto/Adaptador**: 
- `IEventAvailabilityGateway` (Puerto en Domain)
- `HttpEventAvailabilityGateway` (Adaptador HTTP en Infrastructure)
- Implementación alternativa: `NoopEventAvailabilityGateway` (stub para testing)

**Endpoints consumidos**:
- `GET /api/eventos/{eventoId}` - Obtiene estado del evento y secciones

**Configuración**:
- Variable de entorno: `EventsServiceUrl` (ej: `http://events-service:80`)
- HttpClient registrado en DI con BaseAddress

**Estructura de respuesta esperada** (inferida del código):
```json
{
  "id": "uuid",
  "nombre": "Concierto de Rock",
  "estado": "Publicado",
  "secciones": [
    {
      "nombre": "VIP",
      "capacidad": 100
    },
    {
      "nombre": "General",
      "capacidad": 500
    }
  ]
}
```

### 3. Futuros Consumidores (Event-Driven)

Aunque no implementado aún en este código, el diseño prevé publicar eventos de dominio a:
- **Payments Service**: Para vincular tickets con pagos
- **Notifications Service**: Para enviar tickets por email
- **Analytics Service**: Para métricas de venta y asistencia

**Mecanismo**: 
- Los eventos de dominio (`TicketsGenerados`, `TicketsConfirmados`, `TicketValidado`, `TicketCancelado`) se recolectan en `AggregateRoot.DomainEvents`
- Un dispatcher de eventos (no implementado aún) los publicaría a RabbitMQ o similar

## 🗄️ Modelo de Datos

### Entidad Principal: Ticket

**Agregado Raíz** que modela el ciclo de vida completo de un ticket.

```csharp
public sealed class Ticket : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid ReservaId { get; private set; }
    public Guid AsistenteId { get; private set; }
    public TipoTicket Tipo { get; private set; }
    public CodigoQR CodigoQR { get; private set; }
    public decimal PrecioPagado { get; private set; }
    public Guid? AsientoId { get; private set; }
    public string? SeccionNombre { get; private set; }
    public EstadoTicket Estado { get; private set; }
    public DateTime FechaEmision { get; private set; }
    public DateTime? FechaValidacion { get; private set; }
    public string? UbicacionValidacion { get; private set; }
    public Guid? UsuarioValidadorId { get; private set; }
    public Guid? PagoId { get; private set; }
}
```

**Invariantes de dominio**:
- `EventoId`, `ReservaId`, `AsistenteId` obligatorios
- `PrecioPagado` > 0
- Si `AsientoId` presente, `SeccionNombre` obligatorio
- `CodigoQR` único e inmutable
- Transiciones de estado controladas por métodos del agregado

### Value Objects

#### EstadoTicket (enum)
- **Pendiente** (0): Ticket creado, esperando confirmación de pago
- **Confirmado** (1): Ticket válido para ingreso
- **Cancelado** (2): Ticket cancelado (reembolso o expiración)
- **Usado** (3): Ticket utilizado (check-in realizado)

#### TipoTicket (enum)
- **General** (0): Acceso estándar
- **VIP** (1): Acceso premium
- **PrimeraFila** (2): Asiento preferencial
- **AccesoCompleto** (3): Acceso ilimitado a todas las áreas
- **Cortesia** (4): Entrada gratuita

#### CodigoQR
```csharp
public sealed class CodigoQR
{
    public string Valor { get; }      // Ej: "EVT-uuid-TKT-uuid-hash"
    public byte[] Imagen { get; }     // QR en formato binario (PNG/SVG)
}
```

**Formato del valor** (planeado):
```
EVT-{EventoId}-TKT-{TicketId}-{HMACSHA256}
```

El hash HMAC previene falsificación del código QR.

### Eventos de Dominio

#### TicketsGenerados
```csharp
public record TicketsGenerados(
    Guid ReservaId,
    Guid EventoId,
    IReadOnlyCollection<Guid> TicketIds,
    int Cantidad,
    DateTime FechaEmisionUtc
);
```

#### TicketsConfirmados
```csharp
public record TicketsConfirmados(
    Guid ReservaId,
    Guid AsistenteId,
    Guid PagoId,
    IReadOnlyCollection<Guid> TicketIds,
    DateTime FechaConfirmacionUtc
);
```

#### TicketValidado
```csharp
public record TicketValidado(
    Guid TicketId,
    Guid EventoId,
    string UbicacionValidacion,
    Guid UsuarioValidadorId,
    DateTime FechaValidacionUtc
);
```

#### TicketCancelado
```csharp
public record TicketCancelado(
    Guid TicketId,
    Guid? AsientoId,
    string Razon,
    DateTime FechaCancelacionUtc
);
```

## 🗂️ Estructura de Capas

### 1. tickets-service.Api
**Responsabilidad**: Exponer endpoints HTTP y mapear DTOs.

**Componentes**:
- `Program.cs`: Configuración de DI, middleware y definición de endpoints con Minimal APIs
- `TicketsEndpoints`: Mapeo de requests a Commands/Queries de MediatR

**Dependencias**:
- Swashbuckle (Swagger/OpenAPI)
- Referencias a Application, Domain e Infrastructure

### 2. tickets-service.Application
**Responsabilidad**: Casos de uso y orquestación (CQRS).

**Componentes**:
- **Commands**: `GenerarTicketsCommand`, `ConfirmarTicketsCommand`, `ValidarTicketCommand`, `CancelarTicketCommand`
- **Handlers**: Ejecutan lógica de aplicación, llaman al dominio y repositorios
- **Queries**: `CheckAccessQuery` con su handler
- **Validators**: FluentValidation para cada comando
- **Contracts**: DTOs de entrada/salida

**Dependencias**:
- MediatR
- FluentValidation
- Referencia a Domain

### 3. tickets-service.Domain
**Responsabilidad**: Lógica de negocio pura (DDD).

**Componentes**:
- **Aggregates**: `Ticket` (raíz)
- **Value Objects**: `CodigoQR`, `EstadoTicket`, `TipoTicket`
- **Events**: `TicketsGenerados`, `TicketsConfirmados`, etc.
- **Ports**: `ITicketRepository`, `IEventAvailabilityGateway`
- **Abstractions**: `AggregateRoot`, `IDomainEvent`

**Sin dependencias externas** (capa pura).

### 4. tickets-service.Infrastructure
**Responsabilidad**: Adaptadores y persistencia.

**Componentes**:
- **Persistence**: `TicketsDbContext` (EF Core)
- **Repositories**: `TicketRepository` implementa `ITicketRepository`
- **Gateways**: 
  - `HttpEventAvailabilityGateway` (cliente HTTP real)
  - `NoopEventAvailabilityGateway` (stub para testing)

**Dependencias**:
- Entity Framework Core
- Npgsql.EntityFrameworkCore.PostgreSQL

## 🔐 Seguridad y Validaciones

### Validaciones de Entrada (FluentValidation)

Cada comando tiene un validator asociado:

**GenerarTicketsCommandValidator**:
- EventoId, ReservaId, AsistenteId no vacíos
- Items no nulo y con al menos un elemento
- Por cada item: precio > 0, código QR válido

**ConfirmarTicketsCommandValidator**:
- PagoId no vacío
- TicketIds no nulo y con elementos
- FechaConfirmacionUtc válida

**ValidarTicketCommandValidator**:
- CodigoQr no vacío
- UbicacionValidacion no vacía
- UsuarioValidadorId no vacío

**CancelarTicketCommandValidator**:
- TicketId no vacío
- Razon no vacía (mínimo 5 caracteres)

### Invariantes de Dominio

El agregado `Ticket` protege sus invariantes:
- No permite transiciones de estado inválidas
- Valida que un ticket solo se use una vez
- Asegura que asientos numerados tengan sección asignada

### Prevención de Sobreventa

El `HttpEventAvailabilityGateway` implementa la lógica:
```csharp
public async Task EnsureCapacidadDisponibleAsync(
    Guid eventoId,
    IReadOnlyCollection<DisponibilidadSolicitud> solicitudes,
    CancellationToken cancellationToken)
{
    // 1. Consulta capacidad desde events-service
    // 2. Cuenta tickets activos en esta base de datos
    // 3. Valida: tickets_activos + solicitados <= capacidad
    // 4. Lanza excepción si no hay capacidad
}
```

**Riesgo actual**: No hay bloqueo distribuido. En alta concurrencia, dos transacciones simultáneas podrían generar sobreventa. 

**Mitigación futura**: Usar locks distribuidos (Redis) o sagas.

## ⚠️ Deuda Técnica Detectada

### 1. Fallback de Capacidad Infinita

**Ubicación**: `HttpEventAvailabilityGateway.cs:69`

```csharp
// Si no hay secciones, asumimos capacidad infinita
capacidadTotal = int.MaxValue;
```

**Problema**: Si el evento no tiene secciones definidas, se asume capacidad ilimitada, lo cual es un comportamiento peligroso.

**Impacto**: Podría permitir sobreventa masiva en eventos sin secciones.

**Solución recomendada**: 
- Definir un campo `CapacidadGlobal` en el DTO de eventos
- Lanzar excepción si evento no tiene ni secciones ni capacidad global

### 2. Falta de Dispatcher de Eventos de Dominio

**Observación**: Los eventos se recolectan en `AggregateRoot.DomainEvents` pero nunca se publican.

**Ubicación**: Al momento del `SaveChanges` en el repositorio, no hay interceptor que publique eventos.

**Impacto**: 
- Los eventos de integración no se emiten
- Otros servicios (payments, notifications) no se enteran de cambios

**Solución recomendada**:
- Implementar un `DomainEventDispatcher` en Infrastructure
- Registrar un interceptor de SaveChanges que publique a RabbitMQ/Kafka

### 3. Código QR sin Firma Digital

**Observación**: La estructura `CodigoQR` almacena un valor string, pero no hay implementación de generación con HMAC/hash.

**Ubicación**: El handler espera que el cliente envíe el código QR ya generado.

**Impacto**: 
- Falta de protección contra falsificación
- El cliente podría enviar códigos QR inventados

**Solución recomendada**:
- Crear un `ICodigoQRGenerator` en Domain
- Implementar `HmacCodigoQRGenerator` en Infrastructure
- Generar el código en el handler, no esperarlo del cliente

### 4. Falta de Auditoría de Intentos de Validación

**Observación**: El modelo prevé una tabla `validaciones_tickets` pero no está implementada.

**Impacto**: 
- No se registran intentos fallidos de validación
- Difícil detectar fraude o patrones de ataque

**Solución recomendada**:
- Crear entidad `ValidacionTicket` en Domain
- Registrar todos los intentos (exitosos y fallidos) con IP, timestamp, etc.

### 5. Falta de Transacciones Distribuidas

**Problema**: La validación de capacidad (`EnsureCapacidadDisponibleAsync`) y la creación de tickets no están en una transacción distribuida.

**Escenario de falla**:
1. Thread A valida capacidad → OK (50 tickets disponibles)
2. Thread B valida capacidad → OK (50 tickets disponibles)
3. Thread A guarda 50 tickets
4. Thread B guarda 50 tickets
5. Resultado: 100 tickets generados con capacidad para 50

**Impacto**: Sobreventa en escenarios de alta concurrencia.

**Solución recomendada**:
- Implementar locks optimistas con `RowVersion` en Entity Framework
- O usar Saga Pattern con compensaciones
- O lock distribuido (Redis) durante la generación

### 6. Falta de Configuración de Índices Únicos

**Observación**: El código comenta que debería haber un índice único para `asiento_id` activo:

```csharp
// Si hay asientos numerados, evitar duplicados activos por asiento
// (la restricción exacta puede modelarse con una restricción parcial por estado <> 'Cancelado')
```

**Impacto**: Dos tickets activos podrían crearse para el mismo asiento.

**Solución recomendada**:
- Crear índice único filtrado en PostgreSQL:
```sql
CREATE UNIQUE INDEX uq_tickets_asiento_activo
  ON tickets(asiento_id)
  WHERE asiento_id IS NOT NULL 
    AND estado IN ('Pendiente', 'Confirmado');
```

### 7. Falta de Validación de Usuario Asistente

**Observación**: El `AsistenteId` se acepta sin validar su existencia en `users-service`.

**Impacto**: Podrían crearse tickets para usuarios inexistentes.

**Solución recomendada**:
- Crear `IUserGateway` en Domain
- Implementar `HttpUserGateway` en Infrastructure
- Validar existencia del usuario antes de crear tickets

---

## 📚 Referencias

- [API Documentation](api.md)
- [Setup Guide](setup.md)
- [Domain Architecture](ARCHITECTURE.md)
