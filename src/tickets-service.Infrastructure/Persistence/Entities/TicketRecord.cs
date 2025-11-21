using System;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.ValueObjects;

namespace tickets_service.Infrastructure.Persistence.Entities;

/// <summary>Representación de persistencia para el agregado Ticket.</summary>
public sealed class TicketRecord
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public Guid ReservaId { get; set; }
    public Guid AsistenteId { get; set; }
    public int Tipo { get; set; }
    public string CodigoQrValor { get; set; } = string.Empty;
    public byte[] CodigoQrImagen { get; set; } = Array.Empty<byte>();
    public decimal PrecioPagado { get; set; }
    public Guid? AsientoId { get; set; }
    public string? SeccionNombre { get; set; }
    public int Estado { get; set; }
    public DateTime FechaEmision { get; set; }
    public Guid? PagoId { get; set; }
    public DateTime? FechaValidacion { get; set; }
    public string? UbicacionValidacion { get; set; }
    public Guid? UsuarioValidadorId { get; set; }

    public static TicketRecord FromDomain(Ticket ticket)
    {
        return new TicketRecord
        {
            Id = ticket.Id,
            EventoId = ticket.EventoId,
            ReservaId = ticket.ReservaId,
            AsistenteId = ticket.AsistenteId,
            Tipo = (int)ticket.Tipo,
            CodigoQrValor = ticket.CodigoQR.Valor,
            CodigoQrImagen = ticket.CodigoQR.Imagen,
            PrecioPagado = ticket.PrecioPagado,
            AsientoId = ticket.AsientoId,
            SeccionNombre = ticket.SeccionNombre,
            Estado = (int)ticket.Estado,
            FechaEmision = ticket.FechaEmision,
            PagoId = ticket.PagoId,
            FechaValidacion = ticket.FechaValidacion,
            UbicacionValidacion = ticket.UbicacionValidacion,
            UsuarioValidadorId = ticket.UsuarioValidadorId
        };
    }

    public Ticket ToDomain()
    {
        var codigo = CodigoQR.Create(CodigoQrValor, CodigoQrImagen);
        return Ticket.Restaurar(
            Id,
            EventoId,
            ReservaId,
            AsistenteId,
            (TipoTicket)Tipo,
            codigo,
            PrecioPagado,
            AsientoId,
            SeccionNombre,
            (EstadoTicket)Estado,
            FechaEmision,
            PagoId,
            FechaValidacion,
            UbicacionValidacion,
            UsuarioValidadorId);
    }

    public void Apply(Ticket ticket)
    {
        Tipo = (int)ticket.Tipo;
        CodigoQrValor = ticket.CodigoQR.Valor;
        CodigoQrImagen = ticket.CodigoQR.Imagen;
        PrecioPagado = ticket.PrecioPagado;
        AsientoId = ticket.AsientoId;
        SeccionNombre = ticket.SeccionNombre;
        Estado = (int)ticket.Estado;
        FechaEmision = ticket.FechaEmision;
        PagoId = ticket.PagoId;
        FechaValidacion = ticket.FechaValidacion;
        UbicacionValidacion = ticket.UbicacionValidacion;
        UsuarioValidadorId = ticket.UsuarioValidadorId;
    }
}

