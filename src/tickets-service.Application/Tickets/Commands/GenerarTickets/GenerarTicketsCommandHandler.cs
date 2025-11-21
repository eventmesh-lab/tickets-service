using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Ports;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Domain.Tickets.ValueObjects;

namespace tickets_service.Application.Tickets.Commands.GenerarTickets;

public sealed class GenerarTicketsCommandHandler : IRequestHandler<GenerarTicketsCommand, GenerarTicketsResult>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventAvailabilityGateway _eventGateway;

    public GenerarTicketsCommandHandler(
        ITicketRepository ticketRepository,
        IEventAvailabilityGateway eventGateway)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _eventGateway = eventGateway ?? throw new ArgumentNullException(nameof(eventGateway));
    }

    public async Task<GenerarTicketsResult> Handle(GenerarTicketsCommand request, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("Debe existir al menos un ticket a generar.", nameof(request.Items));
        }

        // 1. Validar que el evento esté publicado
        await _eventGateway.EnsureEventoPublicadoAsync(request.EventoId, cancellationToken);

        // 2. Validar disponibilidad (capacidad)
        var disponibilidad = request.Items
            .Select(item => new DisponibilidadSolicitud(item.SeccionNombre, item.AsientoId, 1))
            .ToArray();
        
        await _eventGateway.EnsureCapacidadDisponibleAsync(request.EventoId, disponibilidad, cancellationToken);

        // 3. Generar tickets
        var createdIds = new List<Guid>(request.Items.Count);
        foreach (var item in request.Items)
        {
            var codigo = CodigoQR.Create(item.CodigoQrValor, item.CodigoQrImagen);
            var ticket = Ticket.Crear(
                request.EventoId,
                request.ReservaId,
                request.AsistenteId,
                item.Tipo,
                codigo,
                item.Precio,
                item.AsientoId,
                item.SeccionNombre,
                request.FechaActualUtc);

            await _ticketRepository.AddAsync(ticket, cancellationToken);
            createdIds.Add(ticket.Id);
        }

        return new GenerarTicketsResult(createdIds);
    }
}

