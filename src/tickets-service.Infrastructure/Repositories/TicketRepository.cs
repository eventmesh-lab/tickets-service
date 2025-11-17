using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using tickets_service.Domain.Tickets;
using tickets_service.Domain.Tickets.Repositories;
using tickets_service.Infrastructure.Persistence;
using tickets_service.Infrastructure.Persistence.Entities;

namespace tickets_service.Infrastructure.Repositories;

/// <summary>
/// Implementación con EF Core del puerto ITicketRepository.
/// </summary>
public sealed class TicketRepository : ITicketRepository
{
    private readonly TicketsDbContext _context;

    public TicketRepository(TicketsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        if (ticket is null) throw new ArgumentNullException(nameof(ticket));

        _context.Tickets.Add(TicketRecord.FromDomain(ticket));
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<Ticket?> GetByCodigoQrAsync(string codigoQr, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tickets.AsNoTracking()
            .FirstOrDefaultAsync(t => t.CodigoQrValor == codigoQr, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        if (ticket is null) throw new ArgumentNullException(nameof(ticket));

        var entity = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken)
            ?? throw new InvalidOperationException($"El ticket {ticket.Id} no existe en persistencia.");

        entity.Apply(ticket);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

