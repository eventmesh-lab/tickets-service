using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using tickets_service.Infrastructure.Persistence.Entities;

namespace tickets_service.Infrastructure.Persistence.Configurations;

/// <summary>Mapa de EF Core para la tabla tickets.</summary>
internal sealed class TicketConfiguration : IEntityTypeConfiguration<TicketRecord>
{
    public void Configure(EntityTypeBuilder<TicketRecord> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.CodigoQrValor).IsUnique();
        builder.Property(t => t.CodigoQrValor).IsRequired().HasMaxLength(500);
        builder.Property(t => t.SeccionNombre).HasMaxLength(100);
        builder.Property(t => t.UbicacionValidacion).HasMaxLength(200);

        builder.Property(t => t.CodigoQrImagen).IsRequired();
        builder.Property(t => t.PrecioPagado).HasColumnType("decimal(10,2)");
    }
}

