# API Documentation - Tickets Service

Documentación completa de los endpoints REST expuestos por el servicio.

## 📍 Base URL

- **Desarrollo Local**: `http://localhost:5005`
- **Docker**: `http://localhost:5005`
- **Swagger UI**: `http://localhost:5005/swagger`

## 🔑 Autenticación

> **Nota**: El servicio actualmente no implementa autenticación. En producción debería integrarse con un sistema de Identity/OAuth2.

## 📋 Endpoints

### 1. Generar Tickets

Crea tickets asociados a una reserva, validando disponibilidad del evento.

**Endpoint**: `POST /api/tickets/generar`

**Descripción**: Genera uno o más tickets para un evento específico. Valida que el evento esté publicado y que haya capacidad disponible.

**Request Body**:

```json
{
  "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reservaId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "asistenteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "fechaActualUtc": "2024-12-20T10:30:00Z",
  "items": [
    {
      "tipo": 1,
      "precio": 150.00,
      "asientoId": "e8a0e3c2-4d8f-4e95-a7a3-9f1b5e6d7c8a",
      "seccionNombre": "VIP",
      "codigoQrValor": "EVT-3fa85f64-TKT-abc123-hash456",
      "codigoQrImagen": "iVBORw0KGgoAAAANSUhEUgAAAAUA..."
    },
    {
      "tipo": 0,
      "precio": 50.00,
      "asientoId": null,
      "seccionNombre": "General",
      "codigoQrValor": "EVT-3fa85f64-TKT-def789-hash012",
      "codigoQrImagen": "iVBORw0KGgoAAAANSUhEUgAAAAUA..."
    }
  ]
}
```

**Campos del Request**:

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `eventoId` | UUID | Sí | ID del evento |
| `reservaId` | UUID | Sí | ID de la reserva asociada |
| `asistenteId` | UUID | Sí | ID del usuario asistente |
| `fechaActualUtc` | DateTime | No | Fecha actual en UTC (default: ahora) |
| `items` | Array | Sí | Lista de tickets a generar |
| `items[].tipo` | Enum | Sí | Tipo de ticket (0=General, 1=VIP, 2=PrimeraFila, 3=AccesoCompleto, 4=Cortesia) |
| `items[].precio` | Decimal | Sí | Precio del ticket (debe ser > 0) |
| `items[].asientoId` | UUID | No | ID del asiento (si es numerado) |
| `items[].seccionNombre` | String | Condicional | Nombre de la sección (obligatorio si hay asientoId) |
| `items[].codigoQrValor` | String | Sí | Valor del código QR |
| `items[].codigoQrImagen` | Base64 | Sí | Imagen del QR en base64 |

**Response**: `201 Created`

```json
{
  "ticketIds": [
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "b2c3d4e5-f6a7-8901-bcde-f12345678901"
  ]
}
```

**Errores Comunes**:

- `400 Bad Request`: Validación fallida (precio <= 0, campos vacíos, etc.)
- `400 Bad Request`: Evento no publicado
- `400 Bad Request`: Capacidad insuficiente

**Ejemplo con cURL**:

```bash
curl -X POST http://localhost:5005/api/tickets/generar \
  -H "Content-Type: application/json" \
  -d '{
    "eventoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "reservaId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "asistenteId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "fechaActualUtc": "2024-12-20T10:30:00Z",
    "items": [{
      "tipo": 1,
      "precio": 150.00,
      "seccionNombre": "VIP",
      "codigoQrValor": "EVT-3fa85f64-TKT-abc123-hash456",
      "codigoQrImagen": "iVBORw0KGgoAAAANSUhEUgAAAAUA"
    }]
  }'
```

---

### 2. Confirmar Tickets

Confirma tickets tras un pago exitoso, cambiando su estado de Pendiente a Confirmado.

**Endpoint**: `POST /api/tickets/confirmar`

**Descripción**: Activa los tickets después de que el pago haya sido confirmado. Solo tickets en estado "Pendiente" pueden ser confirmados.

**Request Body**:

```json
{
  "pagoId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "fechaConfirmacionUtc": "2024-12-20T10:35:00Z",
  "ticketIds": [
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "b2c3d4e5-f6a7-8901-bcde-f12345678901"
  ]
}
```

**Campos del Request**:

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `pagoId` | UUID | Sí | ID del pago confirmado |
| `fechaConfirmacionUtc` | DateTime | Sí | Fecha de confirmación en UTC |
| `ticketIds` | Array[UUID] | Sí | Lista de IDs de tickets a confirmar |

**Response**: `204 No Content`

**Errores Comunes**:

- `400 Bad Request`: Ticket no encontrado
- `400 Bad Request`: Ticket no está en estado Pendiente
- `400 Bad Request`: PagoId vacío o inválido

**Ejemplo con cURL**:

```bash
curl -X POST http://localhost:5005/api/tickets/confirmar \
  -H "Content-Type: application/json" \
  -d '{
    "pagoId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "fechaConfirmacionUtc": "2024-12-20T10:35:00Z",
    "ticketIds": [
      "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    ]
  }'
```

---

### 3. Validar Ticket (Check-in)

Marca un ticket como usado durante el proceso de ingreso al evento.

**Endpoint**: `POST /api/tickets/validar`

**Descripción**: Valida un ticket mediante su código QR y lo marca como "Usado". Solo tickets en estado "Confirmado" pueden ser validados. Un ticket solo puede validarse una vez.

**Request Body**:

```json
{
  "codigoQr": "EVT-3fa85f64-TKT-abc123-hash456",
  "ubicacionValidacion": "Entrada Principal - Puerta A",
  "usuarioValidadorId": "2f3e4d5c-6b7a-8901-cdef-234567890abc",
  "fechaValidacionUtc": "2024-12-21T18:15:00Z"
}
```

**Campos del Request**:

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `codigoQr` | String | Sí | Código QR del ticket |
| `ubicacionValidacion` | String | Sí | Ubicación física donde se valida (ej: "Puerta 3") |
| `usuarioValidadorId` | UUID | Sí | ID del usuario que realiza la validación |
| `fechaValidacionUtc` | DateTime | Sí | Fecha/hora de validación en UTC |

**Response**: `200 OK`

```json
{}
```

**Errores Comunes**:

- `400 Bad Request`: Código QR no encontrado
- `400 Bad Request`: Ticket no está en estado Confirmado (ya usado o cancelado)
- `400 Bad Request`: Ticket ya fue validado previamente

**Ejemplo con cURL**:

```bash
curl -X POST http://localhost:5005/api/tickets/validar \
  -H "Content-Type: application/json" \
  -d '{
    "codigoQr": "EVT-3fa85f64-TKT-abc123-hash456",
    "ubicacionValidacion": "Entrada Principal - Puerta A",
    "usuarioValidadorId": "2f3e4d5c-6b7a-8901-cdef-234567890abc",
    "fechaValidacionUtc": "2024-12-21T18:15:00Z"
  }'
```

---

### 4. Cancelar Ticket

Cancela un ticket y libera su capacidad asociada.

**Endpoint**: `POST /api/tickets/cancelar`

**Descripción**: Cancela un ticket en estado "Pendiente" o "Confirmado". Los tickets ya usados no pueden cancelarse.

**Request Body**:

```json
{
  "ticketId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "razon": "Solicitud del cliente por cambio de planes",
  "fechaCancelacionUtc": "2024-12-20T14:00:00Z"
}
```

**Campos del Request**:

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `ticketId` | UUID | Sí | ID del ticket a cancelar |
| `razon` | String | Sí | Motivo de la cancelación (mínimo 5 caracteres) |
| `fechaCancelacionUtc` | DateTime | Sí | Fecha de cancelación en UTC |

**Response**: `204 No Content`

**Errores Comunes**:

- `400 Bad Request`: Ticket no encontrado
- `400 Bad Request`: Ticket ya está usado (no se puede cancelar)
- `400 Bad Request`: Razón vacía o muy corta

**Ejemplo con cURL**:

```bash
curl -X POST http://localhost:5005/api/tickets/cancelar \
  -H "Content-Type: application/json" \
  -d '{
    "ticketId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "razon": "Solicitud del cliente por cambio de planes",
    "fechaCancelacionUtc": "2024-12-20T14:00:00Z"
  }'
```

---

### 5. Verificar Acceso

Consulta si un usuario tiene acceso válido a un evento.

**Endpoint**: `GET /api/tickets/check-access`

**Descripción**: Verifica si un usuario tiene un ticket activo (Confirmado o Usado) para un evento específico.

**Query Parameters**:

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `eventId` | UUID | Sí | ID del evento |
| `userId` | UUID | Sí | ID del usuario |

**Response**: `200 OK`

```json
{
  "hasAccess": true,
  "ticketId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "ticketType": "VIP",
  "status": "Confirmado"
}
```

**Campos del Response**:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `hasAccess` | Boolean | Indica si el usuario tiene acceso |
| `ticketId` | UUID | ID del ticket encontrado (null si no tiene acceso) |
| `ticketType` | String | Tipo de ticket (null si no tiene acceso) |
| `status` | String | Estado del ticket (null si no tiene acceso) |

**Ejemplo con cURL**:

```bash
curl -X GET "http://localhost:5005/api/tickets/check-access?eventId=3fa85f64-5717-4562-b3fc-2c963f66afa6&userId=f47ac10b-58cc-4372-a567-0e02b2c3d479"
```

**Ejemplo sin acceso**:

```json
{
  "hasAccess": false,
  "ticketId": null,
  "ticketType": null,
  "status": null
}
```

---

## 📊 Enumeraciones

### TipoTicket

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | General | Acceso estándar |
| 1 | VIP | Acceso premium con beneficios |
| 2 | PrimeraFila | Asiento preferencial |
| 3 | AccesoCompleto | Acceso ilimitado a todas las áreas |
| 4 | Cortesia | Entrada gratuita |

### EstadoTicket

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Pendiente | Creado, esperando confirmación de pago |
| 1 | Confirmado | Válido para uso en el evento |
| 2 | Cancelado | Cancelado (reembolso o expiración) |
| 3 | Usado | Ya utilizado (check-in realizado) |

---

## 🔄 Flujos de Uso Completos

### Flujo 1: Compra de Tickets

```
1. POST /api/tickets/generar
   → Estado: Pendiente
   
2. Usuario realiza el pago (payments-service)
   
3. POST /api/tickets/confirmar
   → Estado: Confirmado
   
4. Usuario recibe tickets por email (notifications-service)
```

### Flujo 2: Ingreso al Evento

```
1. Usuario presenta código QR
   
2. POST /api/tickets/validar
   → Estado: Usado
   
3. Sistema registra entrada
```

### Flujo 3: Cancelación

```
1. GET /api/tickets/check-access (verificar que existe)
   
2. POST /api/tickets/cancelar
   → Estado: Cancelado
   
3. Se procesa reembolso (payments-service)
```

---

## 🧪 Testing con Postman/Swagger

### Colección de Prueba

1. **Accede a Swagger UI**: http://localhost:5005/swagger
2. Todos los endpoints están documentados e incluyen un botón "Try it out"
3. Los ejemplos de JSON están pre-cargados
4. Puedes copiar los UUIDs generados de las respuestas para usar en llamadas posteriores

### Secuencia de Prueba Recomendada

```bash
# 1. Generar tickets
POST /api/tickets/generar
# Copiar ticketIds del response

# 2. Confirmar tickets (usar ticketIds del paso 1)
POST /api/tickets/confirmar

# 3. Verificar acceso
GET /api/tickets/check-access?eventId={eventId}&userId={userId}

# 4. Validar ticket (usar codigoQr del paso 1)
POST /api/tickets/validar

# 5. Intentar validar de nuevo (debería fallar)
POST /api/tickets/validar
```

---

## 📝 Notas Importantes

### Sobre Códigos QR

Actualmente, el servicio **espera que el cliente genere el código QR**. Esto es una limitación conocida (ver [Deuda Técnica](architecture.md#deuda-técnica-detectada)).

En un escenario ideal:
- El servicio debería generar el código QR automáticamente
- El formato debería ser: `EVT-{EventoId}-TKT-{TicketId}-{HMAC}`
- El HMAC previene falsificación

### Sobre Concurrencia

El servicio valida capacidad antes de generar tickets, pero **no hay locks distribuidos**. En escenarios de alta concurrencia, podría ocurrir sobreventa.

Mitigación actual: Las transacciones de base de datos garantizan consistencia eventual.

### Sobre Eventos de Dominio

Los eventos de dominio (`TicketsGenerados`, `TicketsConfirmados`, etc.) se recolectan pero **no se publican** actualmente a un message broker. Esta funcionalidad está planificada.

---

## 🔗 Referencias

- [Arquitectura](architecture.md)
- [Configuración](setup.md)
- [Swagger UI](http://localhost:5005/swagger)
