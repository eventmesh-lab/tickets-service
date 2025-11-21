using MediatR;
using System;

namespace tickets_service.Application.Tickets.Commands.ValidarTicket;

/// <summary>
/// Comando para procesar la validación (check-in) de un ticket.
/// </summary>
public sealed record ValidarTicketCommand(
    string CodigoQr,
    string UbicacionValidacion,
    Guid UsuarioValidadorId,
    DateTime FechaValidacionUtc) : IRequest;

