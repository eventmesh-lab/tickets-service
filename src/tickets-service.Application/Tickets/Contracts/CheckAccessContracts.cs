using System;
using MediatR;

namespace tickets_service.Application.Tickets.Contracts;

public sealed record CheckAccessQuery(Guid EventoId, Guid UsuarioId) : IRequest<CheckAccessResult>;

public sealed record CheckAccessResult(bool HasAccess, Guid? TicketId, string? TicketType, string Status);
