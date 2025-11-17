using System;
using System.Linq;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Events;
using Xunit;

namespace tickets_service.Domain.Tests.Tickets;

/// <summary>
/// Pruebas del caso ConfirmarTickets.
/// </summary>
public class TicketConfirmationSpecs
{
    [Fact]
    public void ConfirmarPendiente_CambiaEstadoYEmiteEvento()
    {
        var ticket = TicketMother.CrearPendiente();
        var pagoId = Guid.NewGuid();
        var fechaConfirmacion = TicketMother.FechaReferencia.AddMinutes(15);

        ticket.Confirmar(pagoId, fechaConfirmacion);

        Assert.Equal(EstadoTicket.Confirmado, ticket.Estado);

        var evt = Assert.Single(ticket.DomainEvents.OfType<TicketsConfirmados>());
        Assert.Equal(ticket.ReservaId, evt.ReservaId);
        Assert.Equal(ticket.AsistenteId, evt.AsistenteId);
        Assert.Contains(ticket.Id, evt.TicketIds);
        Assert.Equal(pagoId, evt.PagoId);
    }

    [Fact]
    public void ConfirmarTicketNoPendiente_LanzaInvalidOperationException()
    {
        var ticket = TicketMother.CrearPendiente();
        ticket.Confirmar(Guid.NewGuid(), TicketMother.FechaReferencia.AddMinutes(5));

        Assert.Throws<InvalidOperationException>(() =>
            ticket.Confirmar(Guid.NewGuid(), TicketMother.FechaReferencia.AddMinutes(10)));
    }
}