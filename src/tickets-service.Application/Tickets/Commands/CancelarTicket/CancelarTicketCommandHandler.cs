using MediatR;
using tickets_service.Domain.Tickets.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace tickets_service.Application.Tickets.Commands.CancelarTicket;

public sealed class CancelarTicketCommandHandler : IRequestHandler<CancelarTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public CancelarTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task Handle(CancelarTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"El ticket {request.TicketId} no existe.");

        ticket.Cancelar(request.Razon, request.FechaCancelacionUtc);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
    }
}

