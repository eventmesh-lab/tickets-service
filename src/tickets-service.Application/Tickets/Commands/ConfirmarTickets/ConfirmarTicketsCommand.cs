using MediatR;
using System;
using System.Collections.Generic;

namespace tickets_service.Application.Tickets.Commands.ConfirmarTickets;

/// <summary>
/// Comando para confirmar tickets tras un pago exitoso.
/// </summary>
public sealed record ConfirmarTicketsCommand(
    Guid PagoId,
    DateTime FechaConfirmacionUtc,
    IReadOnlyCollection<Guid> TicketIds) : IRequest;

