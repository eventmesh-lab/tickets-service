using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using tickets_service.Application.Tickets;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Domain.Tickets.ValueObjects;
using Xunit;

namespace tickets_service.Application.Tests.Tickets;

public class TicketApplicationServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepository = new();
    private readonly Mock<IEventAvailabilityGateway> _eventGateway = new();
    private readonly TicketApplicationService _service;

    public TicketApplicationServiceTests()
    {
        _service = new TicketApplicationService(_ticketRepository.Object, _eventGateway.Object);
    }

    [Fact]
    public async Task GenerarAsync_PersisteTicketsYDevuelveIds()
    {
        var request = new GenerarTicketsRequest(
            EventoId: Guid.NewGuid(),
            ReservaId: Guid.NewGuid(),
            AsistenteId: Guid.NewGuid(),
            FechaActualUtc: DateTime.UtcNow,
            Items: new List<GenerarTicketItem>
            {
                new(TipoTicket.General, 120m, null, "General", "QR-1", new byte[] {1}),
                new(TipoTicket.VIP, 350m, Guid.NewGuid(), "VIP", "QR-2", new byte[] {2})
            });

        var result = await _service.GenerarAsync(request);

        Assert.Equal(2, result.TicketIds.Count);
        _eventGateway.Verify(g => g.EnsureEventoPublicadoAsync(request.EventoId, It.IsAny<CancellationToken>()), Times.Once);
        _eventGateway.Verify(g => g.EnsureCapacidadDisponibleAsync(request.EventoId, It.IsAny<IReadOnlyCollection<DisponibilidadSolicitud>>(), It.IsAny<CancellationToken>()), Times.Once);
        _ticketRepository.Verify(r => r.AddAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ConfirmarAsync_TicketNoExiste_Lanza()
    {
        var request = new ConfirmarTicketsRequest(Guid.NewGuid(), DateTime.UtcNow, new List<Guid> { Guid.NewGuid() });
        _ticketRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ConfirmarAsync(request));
    }

    [Fact]
    public async Task ValidarAsync_TicketEncontrado_ActualizaRepositorio()
    {
        var ticket = CrearTicketPendiente();
        ticket.Confirmar(Guid.NewGuid(), DateTime.UtcNow);
        _ticketRepository.Setup(r => r.GetByCodigoQrAsync("QR-V", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var request = new ValidarTicketRequest("QR-V", "Entrada A", Guid.NewGuid(), DateTime.UtcNow);
        await _service.ValidarAsync(request);

        _ticketRepository.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(EstadoTicket.Usado, ticket.Estado);
    }

    [Fact]
    public async Task CancelarAsync_TicketPendiente_ActualizaEstado()
    {
        var ticket = CrearTicketPendiente();
        _ticketRepository.Setup(r => r.GetByIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var request = new CancelarTicketRequest(ticket.Id, "Solicitud del asistente", DateTime.UtcNow);
        await _service.CancelarAsync(request);

        _ticketRepository.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(EstadoTicket.Cancelado, ticket.Estado);
    }

    private static Ticket CrearTicketPendiente()
    {
        var codigo = CodigoQR.Create("EVT-X-TKT-Y", new byte[] { 3 });
        return Ticket.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoTicket.General, codigo, 100m, null, "General", DateTime.UtcNow);
    }
}

