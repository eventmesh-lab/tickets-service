# Tickets Service

## 1. Descripción

Microservicio responsable de la **generación, gestión y validación de tickets (entradas)** con códigos QR únicos. Implementa la lógica del agregado `Ticket` según el lenguaje ubicuo. El ticket representa el documento digital que otorga derecho de acceso a un evento.

**Bounded Context:** Reservas y Ticketing

**Repository:** `eventmesh-lab/tickets-service`

---

## 2. Responsabilidades

- Generar tickets con códigos QR únicos
- Gestionar el ciclo de vida de tickets (Pendiente, Confirmado, Usado, Cancelado)
- Validar tickets en acceso físico (check-in)
- Asociar tickets a reservas y pagos
- Prevenir duplicación y fraude
- Registrar auditoría de validaciones

---

## 3. Modelo de Dominio

### 3.1 Agregado: Ticket

**Root Aggregate:** `Ticket`

#### Entidades

##### Ticket
```csharp
public class Ticket : AggregateRoot
{
    public Guid Id { get; private set; }
    public CodigoQR CodigoQR { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid ReservaId { get; private set; }
    public Guid AsistenteId { get; private set; }
    public TipoTicket Tipo { get; private set; }
    public EstadoTicket Estado { get; private set; }
    public Guid? AsientoId { get; private set; }
    public string SeccionNombre { get; private set; }
    public decimal PrecioPagado { get; private set; }
    public DateTime FechaEmision { get; private set; }
    public DateTime? FechaValidacion { get; private set; }
    public string UbicacionValidacion { get; private set; }
}
```

#### Value Objects

##### CodigoQR
```csharp
public record CodigoQR
{
    public string Valor { get; init; }
    public byte[] ImagenQR { get; init; }
    
    // Formato: EVT-{EventoId}-TKT-{TicketId}-{Hash}
    // El hash es una firma digital para prevenir falsificación
}
```

##### EstadoTicket
```csharp
public enum EstadoTicket
{
    Pendiente,    // Ticket creado, esperando confirmación de pago
    Confirmado,   // Ticket válido para uso
    Cancelado,    // Ticket cancelado (reembolso o expiración)
    Usado         // Ticket ya utilizado (check-in realizado)
}
```

##### TipoTicket
```csharp
public enum TipoTicket
{
    General,
    VIP,
    PrimeraFila,
    AccesoCompleto,
    Cortesia
}
```

---

## 4. Comandos del Dominio

### GenerarTickets
**Descripción:** Genera tickets asociados a una reserva confirmada.

**Input:**
```csharp
public record GenerarTicketsCommand
{
    public Guid ReservaId { get; init; }
    public Guid EventoId { get; init; }
    public Guid AsistenteId { get; init; }
    public List<TicketItemDto> Items { get; init; }
}

public record TicketItemDto
{
    public TipoTicket Tipo { get; init; }
    public Guid? AsientoId { get; init; }
    public string SeccionNombre { get; init; }
    public decimal Precio { get; init; }
}
```

**Validaciones:**
- La reserva debe existir y estar confirmada
- El evento debe estar en estado `Publicado`
- Cada asiento solo puede tener un ticket activo

**Emite:** `TicketsGenerados`

**Estado resultante:** `Pendiente`

---

### ChequearDisponibilidad

**Descripción:** Consulta disponibilidad de tickets por evento y (opcional) por sección.

**Input:**

```csharp
public record ChequearDisponibilidadQuery
{
        public Guid EventoId { get; init; }
        public Guid? SeccionId { get; init; }
}
```

**Output (ejemplo):**

```json
{
    "eventoId": "...",
    "capacidadTotal": 1000,
    "vendidos": 750,
    "reservados": 50,
    "disponibles": 200,
    "porSeccion": [
        { "seccionId": "A", "capacidad": 300, "vendidos": 250, "disponibles": 50 },
        { "seccionId": "B", "capacidad": 700, "vendidos": 500, "disponibles": 200 }
    ]
}
```

---

### ConfirmarTickets

**Descripción:** Activa los tickets tras confirmación de pago.

**Input:**

```csharp
public record ConfirmarTicketsCommand
{
    public Guid ReservaId { get; init; }
    public Guid PagoId { get; init; }
    public List<Guid> TicketIds { get; init; }
}
```

**Validaciones:**

- Los tickets deben estar en estado `Pendiente`
- El pago debe estar confirmado

**Emite:** `TicketsConfirmados`

**Estado resultante:** `Confirmado`

---

### ValidarTicket

**Descripción:** Marca el ticket como usado al realizar check-in.

**Input:**

```csharp
public record ValidarTicketCommand
{
    public string CodigoQR { get; init; }
    public string UbicacionValidacion { get; init; }
    public Guid UsuarioValidadorId { get; init; }
}
```

**Validaciones:**

- El código QR debe ser válido y no falsificado
- El ticket debe estar en estado `Confirmado`
- El evento debe estar en estado `EnCurso` o permitir entrada anticipada
- El ticket no debe haber sido usado previamente

**Emite:** `TicketValidado`

**Estado resultante:** `Usado`

---

### CancelarTicket

**Descripción:** Cancela un ticket y libera el asiento.

**Input:**

```csharp
public record CancelarTicketCommand
{
    public Guid TicketId { get; init; }
    public string Razon { get; init; }
}
```

**Validaciones:**

- El ticket debe estar en estado `Pendiente` o `Confirmado`
- No debe haber sido usado

**Emite:** `TicketCancelado`

**Estado resultante:** `Cancelado`

---

## 5. Eventos de Dominio

### TicketsGenerados

```csharp
public record TicketsGenerados : DomainEvent
{
    public Guid ReservaId { get; init; }
    public Guid EventoId { get; init; }
    public List<Guid> TicketIds { get; init; }
    public int Cantidad { get; init; }
}
```

**Suscriptores:**

- `payments-service`: Registra items para facturación

---

### TicketsConfirmados

```csharp
public record TicketsConfirmados : DomainEvent
{
    public Guid ReservaId { get; init; }
    public Guid AsistenteId { get; init; }
    public List<TicketConfirmadoDto> Tickets { get; init; }
}
```

**Suscriptores:**

- `notifications-service`: Envía tickets por correo
- `analytics-service`: Registra venta

---

### TicketValidado

```csharp
public record TicketValidado : DomainEvent
{
    public Guid TicketId { get; init; }
    public Guid EventoId { get; init; }
    public DateTime FechaValidacion { get; init; }
    public string UbicacionValidacion { get; init; }
}
```

**Suscriptores:**

- `analytics-service`: Registra asistencia real
- `notifications-service`: Notifica entrada exitosa

---

### TicketCancelado

```csharp
public record TicketCancelado : DomainEvent
{
    public Guid TicketId { get; init; }
    public Guid AsientoId { get; init; }
    public string Razon { get; init; }
}
```

**Suscriptores:**

- `reservations-service`: Libera asiento

---

## 6. Reglas de Negocio

1. **Código QR único:** Cada ticket debe tener un código QR único e irrepetible, generado con firma digital.
2. **Validación única:** Un ticket solo puede ser validado (usado) una vez. Intentos posteriores deben ser rechazados.
3. **Validación temporal:** Los tickets solo pueden ser validados durante el periodo del evento o en una ventana de tiempo pre-configurada.
4. **Asiento exclusivo:** Un asiento numerado solo puede tener un ticket activo (Confirmado) a la vez.
5. **Cancelación limitada:** Los tickets solo pueden ser cancelados antes del inicio del evento.
6. **Trazabilidad:** Todos los intentos de validación (exitosos o fallidos) deben quedar registrados en auditoría.
7. **Capacidad del evento:** No se pueden generar/confirmar más tickets que la capacidad disponible del evento. La capacidad puede ser total o por sección/área y proviene del `events-service`.
8. **Estado del evento:** La venta/emisión de tickets solo procede si el evento está `Publicado` (y la validación si está `EnCurso` o conforme a la ventana definida).

---

## 7. Servicios de Dominio

### ServicioDeTickets

```csharp
public interface IServicioDeTickets
{
    Task<CodigoQR> GenerarCodigoQR(Guid ticketId, Guid eventoId);
    Task<bool> ValidarFirmaQR(string codigoQR);
    Task<Result> ValidarAcceso(string codigoQR, string ubicacion);
}
```

---

## 8. Integraciones

### Comunicación Asíncrona (RabbitMQ)

**Exchange:** `tickets.domain.events`

**Publica:**

- `TicketsGenerados`
- `TicketsConfirmados`
- `TicketValidado`
- `TicketCancelado`

**Consume:**

- `ReservaConfirmada` (desde `reservations-service`)
- `PagoConfirmado` (desde `payments-service`)

### Events Service (catálogo y capacidad)

- **Lecturas sincrónicas:** durante generación/confirmación se consulta `events-service` (REST/GRPC) para:
    - Estado del evento (`Publicado`, `EnCurso`)
    - Capacidad total y por sección (si aplica)
    - Mapa de secciones/asientos si existe
- **Regla clave:** tickets-service calcula disponibilidad como `capacidad - (confirmados + pendientes reservados)`; bloquea sobreventa con transacciones y, si hay asientos, con unicidad por `asiento_id`.

### Users Service (identidad del asistente)

- **Validación:** al generar/confirmar tickets se verifica la existencia del `AsistenteId` en `users-service` y su estado (p. ej. activo).
- **Enriquecimiento:** opcionalmente se sincroniza email/nombre para inclusión en notificaciones o QR payload.

---

## 9. Persistencia

**Base de Datos:** PostgreSQL

### Tabla: tickets

```sql
CREATE TABLE tickets (
    id UUID PRIMARY KEY,
    codigo_qr VARCHAR(500) NOT NULL UNIQUE,
    evento_id UUID NOT NULL,
    reserva_id UUID NOT NULL,
    asistente_id UUID NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    estado VARCHAR(20) NOT NULL,
    asiento_id UUID,
    seccion_nombre VARCHAR(100),
    precio_pagado DECIMAL(10,2) NOT NULL,
    fecha_emision TIMESTAMP NOT NULL,
    fecha_validacion TIMESTAMP,
    ubicacion_validacion VARCHAR(200),
    version INT NOT NULL
);

CREATE INDEX idx_tickets_codigo_qr ON tickets(codigo_qr);
CREATE INDEX idx_tickets_evento_id ON tickets(evento_id);
CREATE INDEX idx_tickets_asistente_id ON tickets(asistente_id);

-- Si hay asientos numerados, evitar duplicados activos por asiento
-- (la restricción exacta puede modelarse con una restricción parcial por estado <> 'Cancelado')
-- Ejemplo aproximado (según soporte de la versión de PostgreSQL):
-- CREATE UNIQUE INDEX uq_tickets_asiento_activo
--   ON tickets(asiento_id)
--   WHERE asiento_id IS NOT NULL AND estado IN ('Pendiente','Confirmado');
```

### Tabla: validaciones_tickets (Auditoría)

```sql
CREATE TABLE validaciones_tickets (
    id UUID PRIMARY KEY,
    ticket_id UUID NOT NULL REFERENCES tickets(id),
    codigo_qr VARCHAR(500) NOT NULL,
    fecha_intento TIMESTAMP NOT NULL,
    exitoso BOOLEAN NOT NULL,
    ubicacion VARCHAR(200),
    usuario_validador_id UUID,
    mensaje_error TEXT,
    ip_address VARCHAR(50)
);
```

---

## 10. API Endpoints

### POST /api/tickets/generar
Genera tickets para una reserva.

### POST /api/tickets/confirmar

Confirma tickets tras pago exitoso.

### POST /api/tickets/validar

Valida un ticket mediante código QR (check-in).

**Request:**

```json
{
  "codigoQR": "EVT-abc123-TKT-def456-hash789",
  "ubicacion": "Entrada Principal",
  "usuarioValidadorId": "uuid"
}
```

**Response:** `200 OK` (Ticket válido) o `400 Bad Request` (ya usado/inválido)

---

### GET /api/tickets/{id}

Obtiene detalles de un ticket específico.

### GET /api/tickets/asistente/{asistenteId}

Lista todos los tickets de un asistente.

### GET /api/tickets/evento/{eventoId}/estadisticas

Retorna estadísticas de tickets por evento (vendidos, usados, cancelados).

---

## 11. Generación de Código QR

**Librería:** `QRCoder` o `ZXing.Net`

**Formato del código:**

```text
EVT-{EventoId}-TKT-{TicketId}-{HMACSHA256}
```

**Ejemplo:**

```text
EVT-a1b2c3d4-TKT-e5f6g7h8-9i0j1k2l3m4n5o6p
```

El hash HMAC garantiza que el código no pueda ser falsificado sin la clave secreta del sistema.

---

## 12. Tecnologías

- **.NET 8** (Minimal APIs)
- **Entity Framework Core 8** (PostgreSQL)
- **MediatR** (CQRS)
- **QRCoder** (Generación de QR)
- **RabbitMQ.Client**
- **Serilog**
- **OpenTelemetry**

---

## 13. Observabilidad

### Métricas

- `tickets_generados_total`: Contador de tickets generados
- `tickets_validados_total`: Contador de validaciones exitosas
- `tickets_rechazados_total`: Contador de validaciones fallidas
- `tiempo_validacion_ms`: Latencia de validación de QR

---

## 14. Referencias

- Lenguaje Ubicuo (org-docs): `../../../org-docs/docs/lenguaje-ubicuo.md`
- Tickets Service (org-docs): `../../../org-docs/docs/services/tickets-service.md`
- Events Service (org-docs): `../../../org-docs/docs/services/events-service.md`
