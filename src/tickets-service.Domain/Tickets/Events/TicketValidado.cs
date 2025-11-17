using System;
using tickets_service.Domain.Abstractions;

namespace tickets_service.Domain.Tickets.Events;

/// <summary>Evento emitido cuando un ticket es validado (check-in).</summary>
public sealed record TicketValidado : IDomainEvent
{
    public TicketValidado(Guid ticketId, Guid eventoId, string ubicacion, Guid usuarioValidadorId, DateTime fechaValidacion)
    {
        TicketId = ticketId;
        EventoId = eventoId;
        UbicacionValidacion = ubicacion;
        UsuarioValidadorId = usuarioValidadorId;
        FechaValidacion = fechaValidacion;
        OccurredOn = fechaValidacion;
    }

    public Guid TicketId { get; }
    public Guid EventoId { get; }
    public string UbicacionValidacion { get; }
    public Guid UsuarioValidadorId { get; }
    public DateTime FechaValidacion { get; }
    public DateTime OccurredOn { get; }
}

