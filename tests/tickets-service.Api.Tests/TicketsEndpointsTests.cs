using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Domain.Tickets.ValueObjects;
using tickets_service.Infrastructure.Persistence;
using Xunit;

namespace tickets_service.Api.Tests;

public class TicketsEndpointsTests : IClassFixture<TicketsApiFactory>
{
    private readonly TicketsApiFactory _factory;

    public TicketsEndpointsTests(TicketsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostGenerar_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        var payload = new GenerarTicketsRequest(
            EventoId: Guid.NewGuid(),
            ReservaId: Guid.NewGuid(),
            AsistenteId: Guid.NewGuid(),
            FechaActualUtc: DateTime.UtcNow,
            Items: new[]
            {
                new GenerarTicketItem(TipoTicket.General, 100m, null, "General", "API-QR", new byte[] { 1, 2 })
            });

        var response = await client.PostAsJsonAsync("/api/tickets/generar", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<GenerarTicketsResult>();
        Assert.NotNull(content);
        Assert.Single(content!.TicketIds);
    }
}

public sealed class TicketsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TicketsDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TicketsDbContext>(options =>
                options.UseInMemoryDatabase($"tickets-api-tests-{Guid.NewGuid()}"));
        });
    }
}

