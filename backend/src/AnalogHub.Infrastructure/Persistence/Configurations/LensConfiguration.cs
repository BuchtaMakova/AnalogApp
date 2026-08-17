using AnalogHub.Domain.Entities.Gear;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class LensConfiguration : IEntityTypeConfiguration<Lens>
{
    public void Configure(EntityTypeBuilder<Lens> builder)
    {
        builder.Property(l => l.FocalLengthMinMm).HasPrecision(6, 1);
        builder.Property(l => l.FocalLengthMaxMm).HasPrecision(6, 1);
        builder.Property(l => l.MaxAperture).HasPrecision(4, 1);
        builder.Property(l => l.MinAperture).HasPrecision(4, 1);

        builder.HasMany(l => l.Photos)
            .WithOne(p => p.Lens)
            .HasForeignKey(p => p.LensId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
