using System;
using System.Collections.Generic;
using tickets_service.Domain.Abstractions;

namespace tickets_service.Domain.Tickets.Events;

/// <summary>Evento emitido al confirmar el pago de uno o más tickets.</summary>
public sealed record TicketsConfirmados : IDomainEvent
{
    public TicketsConfirmados(Guid reservaId, Guid asistenteId, Guid pagoId, IReadOnlyCollection<Guid> ticketIds, DateTime occurredOn)
    {
        ReservaId = reservaId;
        AsistenteId = asistenteId;
        PagoId = pagoId;
        TicketIds = ticketIds;
        OccurredOn = occurredOn;
    }

    public Guid ReservaId { get; }
    public Guid AsistenteId { get; }
    public Guid PagoId { get; }
    public IReadOnlyCollection<Guid> TicketIds { get; }
    public DateTime OccurredOn { get; }
}

