using System;
using System.Collections.Generic;
using tickets_service.Domain.Abstractions;
using tickets_service.Domain.Tickets.Events;
using tickets_service.Domain.Tickets.ValueObjects;

namespace tickets_service.Domain.Tickets;

/// <summary>
/// Agregado raíz que modela el ciclo de vida de un ticket de evento.
/// </summary>
public sealed class Ticket : AggregateRoot
{
    private Ticket()
    {
    }

    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid ReservaId { get; private set; }
    public Guid AsistenteId { get; private set; }
    public TipoTicket Tipo { get; private set; }
    public CodigoQR CodigoQR { get; private set; } = null!;
    public decimal PrecioPagado { get; private set; }
    public Guid? AsientoId { get; private set; }
    public string? SeccionNombre { get; private set; }
    public EstadoTicket Estado { get; private set; }
    public DateTime FechaEmision { get; private set; }
    public DateTime? FechaValidacion { get; private set; }
    public string? UbicacionValidacion { get; private set; }
    public Guid? UsuarioValidadorId { get; private set; }
    public Guid? PagoId { get; private set; }

    /// <summary>
    /// Crea un ticket en estado pendiente y emite el evento correspondiente.
    /// </summary>
    public static Ticket Crear(
        Guid eventoId,
        Guid reservaId,
        Guid asistenteId,
        TipoTicket tipo,
        CodigoQR codigoQR,
        decimal precio,
        Guid? asientoId,
        string? seccionNombre,
        DateTime fechaEmisionUtc)
    {
        if (eventoId == Guid.Empty) throw new ArgumentException("EventoId es obligatorio.", nameof(eventoId));
        if (reservaId == Guid.Empty) throw new ArgumentException("ReservaId es obligatorio.", nameof(reservaId));
        if (asistenteId == Guid.Empty) throw new ArgumentException("AsistenteId es obligatorio.", nameof(asistenteId));
        if (codigoQR is null) throw new ArgumentNullException(nameof(codigoQR));
        if (precio <= 0) throw new ArgumentException("El precio debe ser mayor a cero.", nameof(precio));
        if (asientoId.HasValue && string.IsNullOrWhiteSpace(seccionNombre))
        {
            throw new InvalidOperationException("Un asiento numerado requiere una sección asignada.");
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            EventoId = eventoId,
            ReservaId = reservaId,
            AsistenteId = asistenteId,
            Tipo = tipo,
            CodigoQR = codigoQR,
            PrecioPagado = precio,
            AsientoId = asientoId,
            SeccionNombre = seccionNombre,
            Estado = EstadoTicket.Pendiente,
            FechaEmision = fechaEmisionUtc
        };

        ticket.Raise(new TicketsGenerados(
            ticket.ReservaId,
            ticket.EventoId,
            new List<Guid> { ticket.Id },
            fechaEmisionUtc));

        return ticket;
    }

    /// <summary>
    /// Restaura un ticket desde persistencia sin emitir eventos.
    /// </summary>
    public static Ticket Restaurar(
        Guid id,
        Guid eventoId,
        Guid reservaId,
        Guid asistenteId,
        TipoTicket tipo,
        CodigoQR codigoQr,
        decimal precioPagado,
        Guid? asientoId,
        string? seccionNombre,
        EstadoTicket estado,
        DateTime fechaEmision,
        Guid? pagoId,
        DateTime? fechaValidacion,
        string? ubicacionValidacion,
        Guid? usuarioValidadorId)
    {
        return new Ticket
        {
            Id = id,
            EventoId = eventoId,
            ReservaId = reservaId,
            AsistenteId = asistenteId,
            Tipo = tipo,
            CodigoQR = codigoQr,
            PrecioPagado = precioPagado,
            AsientoId = asientoId,
            SeccionNombre = seccionNombre,
            Estado = estado,
            FechaEmision = fechaEmision,
            PagoId = pagoId,
            FechaValidacion = fechaValidacion,
            UbicacionValidacion = ubicacionValidacion,
            UsuarioValidadorId = usuarioValidadorId
        };
    }

    /// <summary>
    /// Confirma el ticket tras un pago exitoso.
    /// </summary>
    public void Confirmar(Guid pagoId, DateTime fechaConfirmacionUtc)
    {
        if (pagoId == Guid.Empty) throw new ArgumentException("PagoId es obligatorio.", nameof(pagoId));
        if (Estado != EstadoTicket.Pendiente)
        {
            throw new InvalidOperationException("Solo los tickets pendientes pueden confirmarse.");
        }

        Estado = EstadoTicket.Confirmado;
        PagoId = pagoId;

        Raise(new TicketsConfirmados(
            ReservaId,
            AsistenteId,
            pagoId,
            new List<Guid> { Id },
            fechaConfirmacionUtc));
    }

    /// <summary>
    /// Marca el ticket como usado durante el proceso de check-in.
    /// </summary>
    public void Validar(string ubicacion, Guid usuarioValidadorId, DateTime fechaValidacionUtc)
    {
        if (Estado != EstadoTicket.Confirmado)
        {
            throw new InvalidOperationException("Solo los tickets confirmados pueden validarse.");
        }

        if (string.IsNullOrWhiteSpace(ubicacion))
        {
            throw new ArgumentException("La ubicación de validación es obligatoria.", nameof(ubicacion));
        }

        if (usuarioValidadorId == Guid.Empty)
        {
            throw new ArgumentException("El usuario validador es obligatorio.", nameof(usuarioValidadorId));
        }

        Estado = EstadoTicket.Usado;
        UbicacionValidacion = ubicacion;
        UsuarioValidadorId = usuarioValidadorId;
        FechaValidacion = fechaValidacionUtc;

        Raise(new TicketValidado(
            Id,
            EventoId,
            ubicacion,
            usuarioValidadorId,
            fechaValidacionUtc));
    }

    /// <summary>
    /// Cancela un ticket pendiente o confirmado.
    /// </summary>
    public void Cancelar(string razon, DateTime fechaCancelacionUtc)
    {
        if (Estado == EstadoTicket.Usado)
        {
            throw new InvalidOperationException("No es posible cancelar un ticket usado.");
        }

        if (Estado == EstadoTicket.Cancelado)
        {
            throw new InvalidOperationException("El ticket ya está cancelado.");
        }

        if (string.IsNullOrWhiteSpace(razon))
        {
            throw new ArgumentException("Debe indicarse la razón de cancelación.", nameof(razon));
        }

        Estado = EstadoTicket.Cancelado;

        Raise(new TicketCancelado(
            Id,
            AsientoId,
            razon,
            fechaCancelacionUtc));
    }
}
