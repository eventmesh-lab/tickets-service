using System;
using System.Text;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.ValueObjects;

namespace tickets_service.Domain.Tests.Tickets;

#nullable enable
/// <summary>
/// Utilidad para crear instancias del agregado Ticket en distintos estados.
/// </summary>
internal static class TicketMother
{
    internal static DateTime FechaReferencia => new(2024, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    internal static CreationRequest DatosBase(Action<CreationRequest>? configure = null)
    {
        var request = new CreationRequest
        {
            EventoId = Guid.NewGuid(),
            ReservaId = Guid.NewGuid(),
            AsistenteId = Guid.NewGuid(),
            Tipo = TipoTicket.General,
            CodigoQR = CodigoQr(),
            Precio = 150m,
            AsientoId = null,
            SeccionNombre = "General",
            FechaActual = FechaReferencia
        };

        configure?.Invoke(request);
        return request;
    }

    internal static CodigoQR CodigoQr(string suffix = "001")
    {
        var valor = $"EVT-{Guid.NewGuid():N}-TKT-{suffix}-HASH";
        var payload = Encoding.UTF8.GetBytes($"payload-{suffix}");
        return CodigoQR.Create(valor, payload);
    }

    internal static Ticket CrearPendiente(Action<CreationRequest>? configure = null)
    {
        var request = DatosBase(configure);

        return Ticket.Crear(
            request.EventoId,
            request.ReservaId,
            request.AsistenteId,
            request.Tipo,
            request.CodigoQR,
            request.Precio,
            request.AsientoId,
            request.SeccionNombre,
            request.FechaActual);
    }

    internal sealed class CreationRequest
    {
        public Guid EventoId { get; set; }
        public Guid ReservaId { get; set; }
        public Guid AsistenteId { get; set; }
        public TipoTicket Tipo { get; set; }
        public CodigoQR CodigoQR { get; set; } = null!;
        public decimal Precio { get; set; }
        public Guid? AsientoId { get; set; }
        public string? SeccionNombre { get; set; }
        public DateTime FechaActual { get; set; }
    }
#nullable restore
}