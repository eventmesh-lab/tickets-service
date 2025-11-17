using System;

namespace tickets_service.Application.Tickets.Contracts;

/// <summary>Entrada para cancelar un ticket individual.</summary>
public sealed record CancelarTicketRequest(
    Guid TicketId,
    string Razon,
    DateTime FechaCancelacionUtc);

