using System;

namespace tickets_service.Application.Tickets.Contracts;

/// <summary>Entrada para el proceso de check-in.</summary>
public sealed record ValidarTicketRequest(
    string CodigoQr,
    string UbicacionValidacion,
    Guid UsuarioValidadorId,
    DateTime FechaValidacionUtc);

