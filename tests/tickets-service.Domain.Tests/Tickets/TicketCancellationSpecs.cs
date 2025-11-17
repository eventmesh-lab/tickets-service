using System;
using System.Linq;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Events;
using tickets_service.Domain.Tickets.ValueObjects;
using Xunit;

namespace tickets_service.Domain.Tests.Tickets;

/// <summary>
/// Pruebas del caso CancelarTicket.
/// </summary>
public class TicketCancellationSpecs
{
    [Fact]
    public void CancelarTicketPendiente_CambiaEstadoYPublicaEvento()
    {
        var ticket = TicketMother.CrearPendiente(r =>
        {
            r.AsientoId = Guid.NewGuid();
            r.SeccionNombre = "VIP";
        });

        var razon = "Solicitado por el asistente";
        var fechaCancelacion = TicketMother.FechaReferencia.AddMinutes(20);

        ticket.Cancelar(razon, fechaCancelacion);

        Assert.Equal(EstadoTicket.Cancelado, ticket.Estado);

        var evt = Assert.Single(ticket.DomainEvents.OfType<TicketCancelado>());
        Assert.Equal(ticket.Id, evt.TicketId);
        Assert.Equal(ticket.AsientoId, evt.AsientoId);
        Assert.Equal(razon, evt.Razon);
    }

    [Fact]
    public void CancelarTicketUsado_LanzaInvalidOperationException()
    {
        var ticket = TicketMother.CrearPendiente();
        ticket.Confirmar(Guid.NewGuid(), TicketMother.FechaReferencia.AddMinutes(5));
        ticket.Validar("Puerta 1", Guid.NewGuid(), TicketMother.FechaReferencia.AddMinutes(30));

        Assert.Throws<InvalidOperationException>(() =>
            ticket.Cancelar("No aplica", TicketMother.FechaReferencia.AddMinutes(35)));
    }
}