using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Domain.Tickets.ValueObjects;

namespace tickets_service.Application.Tickets;

/// <summary>
/// Servicio de aplicación que orquesta los casos de uso del agregado Ticket sin depender de CQRS.
/// </summary>
public sealed class TicketApplicationService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventAvailabilityGateway _eventGateway;

    public TicketApplicationService(
        ITicketRepository ticketRepository,
        IEventAvailabilityGateway eventGateway)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _eventGateway = eventGateway ?? throw new ArgumentNullException(nameof(eventGateway));
    }

    /// <summary>Genera tickets pendientes delegando las reglas al dominio.</summary>
    public async Task<GenerarTicketsResult> GenerarAsync(GenerarTicketsRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("Debe existir al menos un ticket a generar.", nameof(request.Items));
        }

        await _eventGateway.EnsureEventoPublicadoAsync(request.EventoId, cancellationToken);

        var disponibilidad = request.Items
            .Select(item => new DisponibilidadSolicitud(item.SeccionNombre, item.AsientoId, 1))
            .ToArray();
        await _eventGateway.EnsureCapacidadDisponibleAsync(request.EventoId, disponibilidad, cancellationToken);

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

    /// <summary>Confirma una colección de tickets vinculada a un pago.</summary>
    public async Task ConfirmarAsync(ConfirmarTicketsRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
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

    /// <summary>Procesa la validación (check-in) de un ticket confirmado.</summary>
    public async Task ValidarAsync(ValidarTicketRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.CodigoQr))
        {
            throw new ArgumentException("El código QR es obligatorio.", nameof(request.CodigoQr));
        }

        var ticket = await _ticketRepository.GetByCodigoQrAsync(request.CodigoQr, cancellationToken)
            ?? throw new InvalidOperationException("El ticket indicado no existe.");

        ticket.Validar(request.UbicacionValidacion, request.UsuarioValidadorId, request.FechaValidacionUtc);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
    }

    /// <summary>Cancela un ticket pendiente/confirmado.</summary>
    public async Task CancelarAsync(CancelarTicketRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException($"El ticket {request.TicketId} no existe.");

        ticket.Cancelar(request.Razon, request.FechaCancelacionUtc);
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
    }
}

