using System;
using System.Collections.Generic;
using tickets_service.Domain.Abstractions;

namespace tickets_service.Domain.Tickets.Events;

/// <summary>Evento emitido al generar tickets para una reserva.</summary>
public sealed record TicketsGenerados : IDomainEvent
{
    public TicketsGenerados(
        Guid reservaId,
        Guid eventoId,
        IReadOnlyCollection<Guid> ticketIds,
        int cantidad,
        DateTime occurredOn)
    {
        ReservaId = reservaId;
        EventoId = eventoId;
        TicketIds = ticketIds;
        Cantidad = cantidad;
        OccurredOn = occurredOn;
    }

    public Guid ReservaId { get; }
    public Guid EventoId { get; }
    public IReadOnlyCollection<Guid> TicketIds { get; }
    public int Cantidad { get; }
    public DateTime OccurredOn { get; }
}

