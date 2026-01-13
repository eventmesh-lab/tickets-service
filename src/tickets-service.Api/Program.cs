using MediatR;
using Microsoft.EntityFrameworkCore;
using tickets_service.Application;
using tickets_service.Application.Tickets.Commands.CancelarTicket;
using tickets_service.Application.Tickets.Commands.ConfirmarTickets;
using tickets_service.Application.Tickets.Commands.GenerarTickets;
using tickets_service.Application.Tickets.Commands.ValidarTicket;
using tickets_service.Application.Tickets.Contracts;
using tickets_service.Application.Tickets.UseCases;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Domain.Tickets.Ports;
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

var eventsServiceUrl = builder.Configuration["EventsServiceUrl"] 
    ?? throw new InvalidOperationException("EventsServiceUrl no está configurado.");

builder.Services.AddHttpClient<IEventAvailabilityGateway, HttpEventAvailabilityGateway>(client =>
{
    client.BaseAddress = new Uri(eventsServiceUrl);
});

// Registrar capa de aplicación (MediatR + Validators)
builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger siempre habilitado para facilitar el desarrollo y testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tickets Service API v1");
    c.RoutePrefix = "swagger";
});

// Redirigir la raíz a Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapTicketsEndpoints();

app.Run();

static class TicketsEndpoints
{
    public static void MapTicketsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tickets");

        group.MapPost("/generar", async (GenerarTicketsRequest request, ISender sender, CancellationToken ct) =>
        {
            // Mapeo de Request a Command
            var command = new GenerarTicketsCommand(
                request.EventoId,
                request.ReservaId,
                request.AsistenteId,
                request.FechaActualUtc == default ? DateTime.UtcNow : request.FechaActualUtc,
                request.Items
            );

            var result = await sender.Send(command, ct);
            return Results.Created($"/api/tickets/reserva/{request.ReservaId}", result);
        })
        .WithName("GenerarTickets")
        .Produces<GenerarTicketsResult>(StatusCodes.Status201Created);

        group.MapPost("/confirmar", async (ConfirmarTicketsRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new ConfirmarTicketsCommand(
                request.PagoId,
                request.FechaConfirmacionUtc,
                request.TicketIds
            );

            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("ConfirmarTickets")
        .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/validar", async (ValidarTicketRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new ValidarTicketCommand(
                request.CodigoQr,
                request.UbicacionValidacion,
                request.UsuarioValidadorId,
                request.FechaValidacionUtc
            );

            await sender.Send(command, ct);
            return Results.Ok();
        })
        .WithName("ValidarTicket")
        .Produces(StatusCodes.Status200OK);

        group.MapPost("/cancelar", async (CancelarTicketRequest request, ISender sender, CancellationToken ct) =>
        {
            var command = new CancelarTicketCommand(
                request.TicketId,
                request.Razon,
                request.FechaCancelacionUtc
            );

            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("CancelarTicket")
        .Produces(StatusCodes.Status204NoContent);

        // GET /api/tickets/check-access?eventId={id}&userId={id}
        group.MapGet("/check-access", async (Guid eventId, Guid userId, ISender sender, CancellationToken ct) =>
        {
            var query = new CheckAccessQuery(eventId, userId);
            var result = await sender.Send(query, ct);
            return Results.Ok(new
            {
                hasAccess = result.HasAccess,
                ticketId = result.TicketId,
                ticketType = result.TicketType,
                status = result.Status
            });
        })
        .WithName("CheckAccess")
        .Produces(StatusCodes.Status200OK);
    }
}

public partial class Program;
