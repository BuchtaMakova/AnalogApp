using AnalogHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalogHub.Infrastructure.Persistence.Configurations;

public sealed class FilmRollConfiguration : IEntityTypeConfiguration<FilmRoll>
{
    public void Configure(EntityTypeBuilder<FilmRoll> builder)
    {
        builder.ToTable("FilmRolls");

        builder.Property(r => r.Name).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Brand).HasMaxLength(100).IsRequired();
        builder.Property(r => r.LabName).HasMaxLength(150);
        builder.Property(r => r.Format).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasMany(r => r.Photos)
            .WithOne(p => p.FilmRoll)
            .HasForeignKey(p => p.FilmRollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);

        builder.HasOne<User>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.UserId);
    }
}
