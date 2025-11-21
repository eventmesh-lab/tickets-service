using System;
using System.Threading;
using System.Threading.Tasks;
using tickets_service.Domain.Tickets;

namespace tickets_service.Domain.Tickets.Repositories;

/// <summary>
/// Puerto de persistencia para la agregación Ticket.
/// </summary>
public interface ITicketRepository
{
    /// <summary>Registra un nuevo ticket generado.</summary>
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un ticket por su identificador.</summary>
    Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

    /// <summary>Busca un ticket mediante su código QR.</summary>
    Task<Ticket?> GetByCodigoQrAsync(string codigoQr, CancellationToken cancellationToken = default);

    /// <summary>Persiste los cambios realizados sobre el agregado.</summary>
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta los tickets activos (Pendiente/Confirmado) para un evento, opcionalmente filtrando por sección.
    /// </summary>
    Task<int> GetActiveTicketCountAsync(Guid eventoId, string? seccionNombre, CancellationToken cancellationToken = default);
}

