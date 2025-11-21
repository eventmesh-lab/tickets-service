using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using tickets_service.Domain.Tickets.Ports;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Infrastructure.Gateways.Dtos;

namespace tickets_service.Infrastructure.Gateways;

public sealed class HttpEventAvailabilityGateway : IEventAvailabilityGateway
{
    private readonly HttpClient _httpClient;
    private readonly ITicketRepository _ticketRepository;

    public HttpEventAvailabilityGateway(HttpClient httpClient, ITicketRepository ticketRepository)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task EnsureEventoPublicadoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        var evento = await GetEventoAsync(eventoId, cancellationToken);

        if (!string.Equals(evento.Estado, "Publicado", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"El evento {eventoId} no está publicado (Estado: {evento.Estado}).");
        }
    }

    public async Task EnsureCapacidadDisponibleAsync(
        Guid eventoId,
        IReadOnlyCollection<DisponibilidadSolicitud> solicitudes,
        CancellationToken cancellationToken = default)
    {
        var evento = await GetEventoAsync(eventoId, cancellationToken);

        foreach (var solicitud in solicitudes)
        {
            int capacidadTotal;
            
            if (!string.IsNullOrWhiteSpace(solicitud.SeccionNombre))
            {
                var seccion = evento.Secciones?.FirstOrDefault(s => s.Nombre.Equals(solicitud.SeccionNombre, StringComparison.OrdinalIgnoreCase));
                if (seccion is null)
                {
                    throw new InvalidOperationException($"La sección '{solicitud.SeccionNombre}' no existe en el evento {eventoId}.");
                }
                capacidadTotal = seccion.Capacidad;
            }
            else
            {
                // Si no hay sección, asumimos capacidad global o suma de secciones?
                // Por simplicidad y robustez, si el evento tiene secciones, se debería pedir por sección.
                // Si no tiene secciones, usamos un campo de capacidad global si existiera (el DTO actual no lo tiene explícito fuera de secciones, 
                // pero asumiremos que si no hay secciones, la capacidad es ilimitada o manejada de otra forma. 
                // Dado el JSON de ejemplo, parece que la capacidad está en las secciones).
                // AJUSTE: Si el evento tiene secciones, requerimos sección.
                if (evento.Secciones != null && evento.Secciones.Any())
                {
                     throw new InvalidOperationException($"El evento tiene secciones definidas, debe especificar una sección.");
                }
                
                // Fallback: si no hay secciones, asumimos capacidad infinita o lógica simple (TODO: revisar si el evento tiene capacidad global)
                capacidadTotal = int.MaxValue; 
            }

            var ticketsActivos = await _ticketRepository.GetActiveTicketCountAsync(eventoId, solicitud.SeccionNombre, cancellationToken);

            if (ticketsActivos + solicitud.Cantidad > capacidadTotal)
            {
                throw new InvalidOperationException($"No hay capacidad suficiente en '{solicitud.SeccionNombre ?? "General"}'. Disponibles: {capacidadTotal - ticketsActivos}, Solicitados: {solicitud.Cantidad}.");
            }
        }
    }

    private async Task<ExternalEventDto> GetEventoAsync(Guid eventoId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ExternalEventDto>($"api/eventos/{eventoId}", cancellationToken);
            return response ?? throw new InvalidOperationException("La respuesta del servicio de eventos fue nula.");
        }
        catch (HttpRequestException ex)
        {
             throw new InvalidOperationException($"Error al consultar el servicio de eventos: {ex.Message}", ex);
        }
    }
}
