using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tickets_service.Domain.Tickets.Ports;

namespace tickets_service.Infrastructure.Gateways;

/// <summary>
/// Implementación temporal que asume disponibilidad; reemplazar por cliente real de events-service.
/// </summary>
public sealed class NoopEventAvailabilityGateway : IEventAvailabilityGateway
{
    public Task EnsureEventoPublicadoAsync(Guid eventoId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task EnsureCapacidadDisponibleAsync(Guid eventoId, IReadOnlyCollection<DisponibilidadSolicitud> solicitudes, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

