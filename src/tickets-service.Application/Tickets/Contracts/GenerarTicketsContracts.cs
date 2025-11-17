using System;
using System.Collections.Generic;
using tickets_service.Domain.Tickets.ValueObjects;

namespace tickets_service.Application.Tickets.Contracts;

/// <summary>DTO de entrada para la generación de tickets.</summary>
public sealed record GenerarTicketsRequest(
    Guid EventoId,
    Guid ReservaId,
    Guid AsistenteId,
    DateTime FechaActualUtc,
    IReadOnlyCollection<GenerarTicketItem> Items);

/// <summary>Detalle por ticket que se generará.</summary>
public sealed record GenerarTicketItem(
    TipoTicket Tipo,
    decimal Precio,
    Guid? AsientoId,
    string? SeccionNombre,
    string CodigoQrValor,
    byte[] CodigoQrImagen);

/// <summary>Resultado básico de la generación.</summary>
public sealed record GenerarTicketsResult(IReadOnlyCollection<Guid> TicketIds);

