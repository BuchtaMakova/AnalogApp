using AnalogHub.Domain.Entities.Gear;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class FlashConfiguration : IEntityTypeConfiguration<Flash>
{
    public void Configure(EntityTypeBuilder<Flash> builder)
    {
        builder.Property(f => f.GuideNumber).HasPrecision(5, 1);

        builder.HasMany(f => f.Photos)
            .WithOne(p => p.Flash)
            .HasForeignKey(p => p.FlashId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
