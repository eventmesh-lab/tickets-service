using Microsoft.EntityFrameworkCore;
using tickets_service.Application.Tickets;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Infrastructure.Gateways;
using tickets_service.Infrastructure.Persistence;
using tickets_service.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("TicketsDb")
    ?? builder.Configuration["ConnectionStrings:TicketsDb"]
    ?? "Host=localhost;Port=5432;Database=tickets_service;Username=postgres;Password=postgres";

builder.Services.AddDbContext<TicketsDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IEventAvailabilityGateway, NoopEventAvailabilityGateway>();
builder.Services.AddScoped<TicketApplicationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapTicketsEndpoints();

app.Run();

static class TicketsEndpoints
{
    public static void MapTicketsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tickets");

        group.MapPost("/generar", async (GenerarTicketsRequest request, TicketApplicationService service, CancellationToken ct) =>
        {
            var normalized = request with
            {
                FechaActualUtc = request.FechaActualUtc == default ? DateTime.UtcNow : request.FechaActualUtc
            };

            var result = await service.GenerarAsync(normalized, ct);
            return Results.Created($"/api/tickets/reserva/{request.ReservaId}", result);
        })
        .WithName("GenerarTickets")
        .Produces<GenerarTicketsResult>(StatusCodes.Status201Created);

        group.MapPost("/confirmar", async (ConfirmarTicketsRequest request, TicketApplicationService service, CancellationToken ct) =>
        {
            await service.ConfirmarAsync(request, ct);
            return Results.NoContent();
        })
        .WithName("ConfirmarTickets")
        .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/validar", async (ValidarTicketRequest request, TicketApplicationService service, CancellationToken ct) =>
        {
            await service.ValidarAsync(request, ct);
            return Results.Ok();
        })
        .WithName("ValidarTicket")
        .Produces(StatusCodes.Status200OK);

        group.MapPost("/cancelar", async (CancelarTicketRequest request, TicketApplicationService service, CancellationToken ct) =>
        {
            await service.CancelarAsync(request, ct);
            return Results.NoContent();
        })
        .WithName("CancelarTicket")
        .Produces(StatusCodes.Status204NoContent);
    }
}

public partial class Program;
