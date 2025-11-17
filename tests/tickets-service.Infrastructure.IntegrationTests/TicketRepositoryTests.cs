using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Domain.Tickets.ValueObjects;
using tickets_service.Infrastructure.Persistence;
using tickets_service.Infrastructure.Repositories;
using Xunit;

namespace tickets_service.Infrastructure.IntegrationTests;

public class TicketRepositoryTests
{

    [Fact]
    public async Task AddAndGetById_PersisteEstadoDelTicket()
    {
        await using var context = CreateContext();
        ITicketRepository repository = new TicketRepository(context);
        var ticket = CrearTicket();

        await repository.AddAsync(ticket);
        var stored = await repository.GetByIdAsync(ticket.Id);

        Assert.NotNull(stored);
        Assert.Equal(ticket.CodigoQR.Valor, stored!.CodigoQR.Valor);
    }

    [Fact]
    public async Task GetByCodigoQr_RetornaTicketCorrecto()
    {
        await using var context = CreateContext();
        ITicketRepository repository = new TicketRepository(context);
        var ticket = CrearTicket();
        await repository.AddAsync(ticket);

        var stored = await repository.GetByCodigoQrAsync(ticket.CodigoQR.Valor);

        Assert.NotNull(stored);
        Assert.Equal(ticket.Id, stored!.Id);
    }

    [Fact]
    public async Task UpdateAsync_GuardaCambiosDelDominio()
    {
        await using var context = CreateContext();
        ITicketRepository repository = new TicketRepository(context);
        var ticket = CrearTicket();
        await repository.AddAsync(ticket);

        ticket.Confirmar(Guid.NewGuid(), DateTime.UtcNow);
        await repository.UpdateAsync(ticket);

        var stored = await repository.GetByIdAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Equal(EstadoTicket.Confirmado, stored!.Estado);
    }

    private static Ticket CrearTicket()
    {
        var codigo = CodigoQR.Create($"EVT-{Guid.NewGuid():N}", new byte[] { 1, 2, 3 });
        return Ticket.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoTicket.General, codigo, 100, null, "General", DateTime.UtcNow);
    }

    private static TicketsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TicketsDbContext>()
            .UseInMemoryDatabase($"tickets-tests-{Guid.NewGuid()}")
            .Options;

        return new TicketsDbContext(options);
    }
}

