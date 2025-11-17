using System;
using System.Collections.Generic;

namespace tickets_service.Application.Tickets.Contracts;

/// <summary>Entrada para confirmar tickets tras pago.</summary>
public sealed record ConfirmarTicketsRequest(
    Guid PagoId,
    DateTime FechaConfirmacionUtc,
    IReadOnlyCollection<Guid> TicketIds);

