using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Domain.Tickets.Repositories;

namespace tickets_service.Application.Tickets.UseCases;

public sealed class CheckAccessQueryHandler : IRequestHandler<CheckAccessQuery, CheckAccessResult>
{
    private readonly ITicketRepository _repository;

    public CheckAccessQueryHandler(ITicketRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<CheckAccessResult> Handle(CheckAccessQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetTicketForAccessAsync(request.EventoId, request.UsuarioId, cancellationToken);
        if (ticket is null)
        {
            return new CheckAccessResult(false, null, null, "Ninguno");
        }

        var status = ticket.Estado.ToString();
        return new CheckAccessResult(true, ticket.Id, ticket.Tipo.ToString(), status);
    }
}
