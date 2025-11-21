using MediatR;
using System;

namespace tickets_service.Application.Tickets.Commands.CancelarTicket;

/// <summary>
/// Comando para cancelar un ticket.
/// </summary>
public sealed record CancelarTicketCommand(
    Guid TicketId,
    string Razon,
    DateTime FechaCancelacionUtc) : IRequest;

