using System;
using System.Linq;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Events;
using tickets_service.Domain.Tickets.ValueObjects;
using Xunit;

namespace tickets_service.Domain.Tests.Tickets;

/// <summary>
/// Pruebas del caso GenerarTickets con foco en el agregado Ticket.
/// </summary>
public class TicketGenerationSpecs
{
    [Fact]
    public void CrearTicketPendiente_RegistraEventoDeGeneracion()
    {
        // Act
        var ticket = TicketMother.CrearPendiente();

        // Assert
        Assert.Equal(EstadoTicket.Pendiente, ticket.Estado);
        Assert.Equal(TicketMother.FechaReferencia, ticket.FechaEmision);

        var evt = Assert.Single(ticket.DomainEvents.OfType<TicketsGenerados>());
        Assert.Equal(ticket.EventoId, evt.EventoId);
        Assert.Equal(ticket.ReservaId, evt.ReservaId);
        Assert.Contains(ticket.Id, evt.TicketIds);
        Assert.Equal(evt.Cantidad, evt.TicketIds.Count);
    }

    [Fact]
    public void CrearTicket_AsientoSinSeccion_LanzaInvalidOperationException()
    {
        var request = TicketMother.DatosBase(r =>
        {
            r.AsientoId = Guid.NewGuid();
            r.SeccionNombre = null;
        });

        Assert.Throws<InvalidOperationException>(() =>
            Ticket.Crear(
                request.EventoId,
                request.ReservaId,
                request.AsistenteId,
                request.Tipo,
                request.CodigoQR,
                request.Precio,
                request.AsientoId,
                request.SeccionNombre,
                request.FechaActual));
    }

    [Fact]
    public void CrearTicket_SinCodigoQr_LanzaArgumentNullException()
    {
        var request = TicketMother.DatosBase();

        Assert.Throws<ArgumentNullException>(() =>
            Ticket.Crear(
                request.EventoId,
                request.ReservaId,
                request.AsistenteId,
                request.Tipo,
                codigoQR: null!,
                request.Precio,
                request.AsientoId,
                request.SeccionNombre,
                request.FechaActual));
    }
}