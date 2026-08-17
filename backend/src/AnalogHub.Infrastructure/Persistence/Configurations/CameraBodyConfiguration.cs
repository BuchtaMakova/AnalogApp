using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class CameraBodyConfiguration : IEntityTypeConfiguration<CameraBody>
{
    public void Configure(EntityTypeBuilder<CameraBody> builder)
    {
        // Native Postgres integer array; avoids registering FilmFormat as a Postgres enum type.
        builder.Property(c => c.SupportedFormats)
            .HasConversion(
                v => v.Select(f => (int)f).ToArray(),
                v => v.Select(i => (FilmFormat)i).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<FilmFormat>>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (hash, f) => HashCode.Combine(hash, f)),
                v => v.ToList()));

        builder.HasMany(c => c.FilmRolls)
            .WithOne(r => r.CameraBody)
            .HasForeignKey(r => r.CameraBodyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Photos)
            .WithOne(p => p.CameraBody)
            .HasForeignKey(p => p.CameraBodyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
