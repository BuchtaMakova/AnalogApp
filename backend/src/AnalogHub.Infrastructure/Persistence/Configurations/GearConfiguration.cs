using AnalogHub.Domain.Entities.Gear;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the shared "Gear" table for the TPH hierarchy (discriminator + common columns).
/// EF Core applies this alongside the per-subtype configs (<see cref="CameraBodyConfiguration"/>,
/// <see cref="LensConfiguration"/>, <see cref="FlashConfiguration"/>) since they all target the
/// same mapped table, indexed by <see cref="IEntityTypeConfiguration{T}"/>'s generic parameter.
/// </summary>
public sealed class GearConfiguration : IEntityTypeConfiguration<Gear>
{
    public void Configure(EntityTypeBuilder<Gear> builder)
    {
        builder.ToTable("Gear");

        builder.HasDiscriminator<string>("GearType")
            .HasValue<CameraBody>("CameraBody")
            .HasValue<Lens>("Lens")
            .HasValue<Flash>("Flash");

        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Brand).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Model).HasMaxLength(100).IsRequired();
        builder.Property(g => g.SerialNumber).HasMaxLength(100);
        builder.Property(g => g.MountType).HasMaxLength(50);

        builder.HasIndex(g => new { g.Brand, g.Model });
    }
}
