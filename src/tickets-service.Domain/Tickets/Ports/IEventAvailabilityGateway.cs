using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace tickets_service.Domain.Tickets.Ports;

/// <summary>
/// Puerto hacia events-service para validar estado y disponibilidad.
/// </summary>
public interface IEventAvailabilityGateway
{
    /// <summary>Confirma que el evento está publicado y puede emitir tickets.</summary>
    Task EnsureEventoPublicadoAsync(Guid eventoId, CancellationToken cancellationToken = default);

    /// <summary>Valida capacidad total o por sección para los tickets solicitados.</summary>
    Task EnsureCapacidadDisponibleAsync(
        Guid eventoId,
        IReadOnlyCollection<DisponibilidadSolicitud> solicitudes,
        CancellationToken cancellationToken = default);
}

/// <summary>Describe la cantidad requerida por sección o asiento.</summary>
public sealed record DisponibilidadSolicitud(string? SeccionNombre, Guid? AsientoId, int Cantidad);

