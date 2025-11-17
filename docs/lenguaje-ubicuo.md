# Lenguaje ubicuo - Plataforma de Gestion y Publicacion de Eventos

Este documento define el lenguaje ubicuo propuesto para el dominio de la plataforma de gestion, publicacion y monetizacion de eventos fisicos y digitales. Su proposito es unificar la terminologia entre expertos de dominio, analistas y desarrolladores para mantener coherencia semantica durante el diseno e implementacion.

## 1. Conceptos del dominio
- **Evento:** Actividad organizada (concierto, conferencia, festival, streaming) publicada en la plataforma y asociada a un venue o canal digital.
- **Organizador:** Persona o empresa que crea y administra eventos. Responsable del pago de publicacion.
- **Asistente:** Usuario final que compra entradas y participa en eventos. Puede interactuar en foros y acceder a streaming.
- **Venue (recinto):** Espacio fisico donde se realiza el evento. Define capacidad, zonas y accesos.
- **Publicacion:** Proceso de activacion del evento en el catalogo tras pago confirmado; el estado previo es borrador.
- **Ticket (entrada):** Documento digital con codigo QR que otorga derecho de acceso a un evento. Asociado a un pago y a una reserva.
- **Reserva:** Bloqueo temporal de tickets hasta completar el pago para prevenir sobreventa.
- **Pago de publicacion:** Transaccion que habilita la visibilidad del evento; sin pago confirmado no se publica.
- **Transmision (streaming):** Sesion digital en vivo vinculada a un evento digital o hibrido, con acceso restringido por ticket.
- **Comunidad o foro:** Espacio de interaccion de asistentes y organizadores dentro de un evento, moderado y auditado.
- **Moderador:** Usuario autorizado a revisar o eliminar publicaciones en foros; rol asociado al organizador.
- **Administrador:** Personal del sistema con permisos globales que supervisa integridad y cumplimiento.
- **Proveedor externo:** Entidad que ofrece servicios adicionales (transporte, catering, merchandising) integrada via colas o API.
- **Factura:** Documento emitido tras un pago exitoso con informacion fiscal.
- **Check-in:** Proceso de validacion de acceso en puerta o digital que marca un ticket como usado.

## 2. Agregados principales
- **Evento:** Agrupa publicacion, foro, transmision y tickets; representa el nucleo del negocio.
- **Organizador:** Agrupa cuenta, lista de eventos y transacciones; gestiona portafolio y pagos.
- **Asistente:** Agrupa perfil, compras, entradas y comentarios; engloba la interaccion y participacion en eventos.
- **Pago:** Agrupa factura, transaccion y estado; encapsula la logica de cobros.
- **Venue:** Agrupa zonas, aforo y disponibilidad; administra recintos fisicos.

## 3. Objetos de valor
- **PrecioEntrada:** Valor monetario por tipo de ticket.
- **FechaEvento:** Fecha y hora exactas de inicio.
- **DuracionEvento:** Tiempo estimado de transmision o presentacion.
- **EstadoEvento:** {Borrador, PendientePago, Publicado, EnCurso, Finalizado}.
- **EstadoTicket:** {Pendiente, Confirmado, Cancelado, Usado}.
- **EstadoPago:** {Pendiente, Confirmado, Fallido}.
- **CodigoQR:** Identificador unico de ticket o acceso.

## 4. Comandos del dominio
- **CrearEvento():** El organizador registra un nuevo evento en estado borrador.
- **PagarPublicacion():** El organizador realiza el pago que habilita la publicacion del evento.
- **PublicarEvento():** El sistema marca el evento como publicado tras confirmar el pago.
- **EditarEvento():** El organizador modifica datos antes de la publicacion.
- **ComprarTicket():** El asistente inicia el proceso de reserva y pago.
- **ConfirmarPago():** El sistema activa el ticket y emite la factura.
- **ValidarTicket():** El venue o sistema marca el ticket como usado.
- **IniciarTransmision():** El organizador activa una sesion de streaming.
- **UnirseATransmision():** El asistente accede a una transmision valida.
- **PublicarComentario():** El asistente crea una publicacion en el foro del evento.
- **EliminarComentario():** El moderador o administrador remueve contenido.
- **GenerarReporte():** El administrador produce informes analiticos.

## 5. Eventos de dominio
- **EventoCreado:** Se genera un evento en estado borrador.
- **PagoPublicacionConfirmado:** El evento pasa a publicado.
- **EventoPublicado:** Se actualiza el catalogo y se disparan notificaciones.
- **TicketComprado:** Se crea la reserva y el codigo QR.
- **PagoEntradaConfirmado:** El ticket queda activado.
- **TicketValidado:** Queda registrado el acceso.
- **TransmisionIniciada:** Se habilita el acceso de streaming.
- **ComentarioPublicado:** Se registra una nueva publicacion en el foro.
- **ComentarioEliminado:** Se registra una accion de moderacion.

## 6. Servicios de dominio
- **ServicioDePagos:** Orquesta y valida las transacciones de publicacion y tickets.
- **ServicioDePublicacion:** Coordina pago, validacion y visibilidad de eventos.
- **ServicioDeStreaming:** Controla el acceso seguro y el registro de asistencia digital.
- **ServicioDeForos:** Administra hilos, notificaciones y moderacion.
- **ServicioDeTickets:** Genera, valida y marca el uso de tickets QR.
- **ServicioDeNotificaciones:** Envia correos y alertas en tiempo real.
- **ServicioDeReportes:** Consolida informacion de ventas y participacion.

## 7. Decisiones abiertas del lenguaje ubicuo
- **Publicacion:** Definir si incluye promociones externas o solo visibilidad en el catalogo.
- **Evento digital:** Determinar si contempla contenido grabado o solo transmisiones en vivo.
- **Reserva:** Aclarar si genera un ticket provisional o solo bloquea inventario.
- **Proveedor externo:** Definir si se gestiona en la plataforma o via integraciones externas.
- **Moderador:** Determinar si puede ser externo al organizador o designado por la plataforma.
