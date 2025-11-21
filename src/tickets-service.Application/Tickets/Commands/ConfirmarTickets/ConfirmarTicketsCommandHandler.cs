using MediatR;
using tickets_service.Domain.Tickets.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace tickets_service.Application.Tickets.Commands.ConfirmarTickets;

public sealed class ConfirmarTicketsCommandHandler : IRequestHandler<ConfirmarTicketsCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public ConfirmarTicketsCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task Handle(ConfirmarTicketsCommand request, CancellationToken cancellationToken)
    {
        if (request.TicketIds is null || request.TicketIds.Count == 0)
        {
            throw new ArgumentException("Se requieren tickets para confirmar.", nameof(request.TicketIds));
        }

        foreach (var ticketId in request.TicketIds)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken)
                ?? throw new InvalidOperationException($"El ticket {ticketId} no existe.");

            ticket.Confirmar(request.PagoId, request.FechaConfirmacionUtc);
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        }
    }
}

