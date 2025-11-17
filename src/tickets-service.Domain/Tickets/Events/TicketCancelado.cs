using System;
using tickets_service.Domain.Abstractions;

namespace tickets_service.Domain.Tickets.Events;

/// <summary>Evento emitido cuando un ticket cambia a estado cancelado.</summary>
public sealed record TicketCancelado : IDomainEvent
{
    public TicketCancelado(Guid ticketId, Guid? asientoId, string razon, DateTime occurredOn)
    {
        TicketId = ticketId;
        AsientoId = asientoId;
        Razon = razon;
        OccurredOn = occurredOn;
    }

    public Guid TicketId { get; }
    public Guid? AsientoId { get; }
    public string Razon { get; }
    public DateTime OccurredOn { get; }
}

