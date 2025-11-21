using MediatR;
using tickets_service.Application.Tickets.Contracts;
using System;
using System.Collections.Generic;

namespace tickets_service.Application.Tickets.Commands.GenerarTickets;

/// <summary>
/// Comando para generar tickets de una reserva.
/// </summary>
public sealed record GenerarTicketsCommand(
    Guid EventoId,
    Guid ReservaId,
    Guid AsistenteId,
    DateTime FechaActualUtc,
    IReadOnlyCollection<GenerarTicketItem> Items) : IRequest<GenerarTicketsResult>;

