using MediatR;
using tickets_service.Domain.Tickets.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace tickets_service.Application.Tickets.Commands.ValidarTicket;

public sealed class ValidarTicketCommandHandler : IRequestHandler<ValidarTicketCommand>
{
    private readonly ITicketRepository _ticketRepository;

    public ValidarTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task Handle(ValidarTicketCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoQr))
        {
            throw new ArgumentException("El código QR es obligatorio.", nameof(request.CodigoQr));
        }

        var ticket = await _ticketRepository.GetByCodigoQrAsync(request.CodigoQr, cancellationToken)
            ?? throw new InvalidOperationException("El ticket indicado no existe.");

        ticket.Validar(request.UbicacionValidacion, request.UsuarioValidadorId, request.FechaValidacionUtc);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
    }
}

