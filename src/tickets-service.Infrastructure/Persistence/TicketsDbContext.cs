using Microsoft.EntityFrameworkCore;
using tickets_service.Infrastructure.Persistence.Entities;

namespace tickets_service.Infrastructure.Persistence;

/// <summary>DbContext principal para la capa de infraestructura.</summary>
public sealed class TicketsDbContext : DbContext
{
    public TicketsDbContext(DbContextOptions<TicketsDbContext> options)
        : base(options)
    {
    }

    public DbSet<TicketRecord> Tickets => Set<TicketRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketsDbContext).Assembly);
    }
}

