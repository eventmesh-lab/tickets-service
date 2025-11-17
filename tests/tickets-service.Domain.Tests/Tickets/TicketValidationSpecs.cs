using System;
using System.Linq;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Events;
using Xunit;

namespace tickets_service.Domain.Tests.Tickets;

/// <summary>
/// Pruebas del caso ValidarTicket.
/// </summary>
public class TicketValidationSpecs
{
    [Fact]
    public void ValidarTicketConfirmado_MarcaComoUsadoYEmiteEvento()
    {
        var ticket = TicketMother.CrearPendiente();
        ticket.Confirmar(Guid.NewGuid(), TicketMother.FechaReferencia.AddMinutes(10));

        var ubicacion = "Entrada Principal";
        var usuarioValidador = Guid.NewGuid();
        var fechaValidacion = TicketMother.FechaReferencia.AddHours(1);

        ticket.Validar(ubicacion, usuarioValidador, fechaValidacion);

        Assert.Equal(EstadoTicket.Usado, ticket.Estado);
        Assert.Equal(ubicacion, ticket.UbicacionValidacion);
        Assert.Equal(fechaValidacion, ticket.FechaValidacion);

        var evt = Assert.Single(ticket.DomainEvents.OfType<TicketValidado>());
        Assert.Equal(ticket.Id, evt.TicketId);
        Assert.Equal(ubicacion, evt.UbicacionValidacion);
        Assert.Equal(fechaValidacion, evt.FechaValidacion);
        Assert.Equal(usuarioValidador, evt.UsuarioValidadorId);
    }

    [Fact]
    public void ValidarTicketNoConfirmado_LanzaInvalidOperationException()
    {
        var ticket = TicketMother.CrearPendiente();

        Assert.Throws<InvalidOperationException>(() =>
            ticket.Validar("EntradaA", Guid.NewGuid(), TicketMother.FechaReferencia.AddMinutes(30)));
    }
}