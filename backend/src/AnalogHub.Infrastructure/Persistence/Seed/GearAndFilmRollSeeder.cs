using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnalogHub.Infrastructure.Persistence.Seed;

/// <summary>Idempotent demo data: a handful of camera bodies, lenses, flashes and film rolls to make the UI feel real out of the box.</summary>
internal static class GearAndFilmRollSeeder
{
    public static async Task SeedAsync(AnalogHubDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        if (await db.CameraBodies.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Gear/film roll seed skipped — CameraBodies already populated.");
            return;
        }

        var leicaM6 = new CameraBody
        {
            Name = "Leica M6",
            Brand = "Leica",
            Model = "M6",
            MountType = "Leica M",
            SupportedFormats = [FilmFormat.ThirtyFiveMm],
            AcquiredOn = new DateOnly(2019, 6, 12),
        };

        var nikonF100 = new CameraBody
        {
            Name = "Nikon F100",
            Brand = "Nikon",
            Model = "F100",
            MountType = "Nikon F",
            SupportedFormats = [FilmFormat.ThirtyFiveMm],
            AcquiredOn = new DateOnly(2021, 3, 2),
        };

        var pentax645n = new CameraBody
        {
            Name = "Pentax 645N",
            Brand = "Pentax",
            Model = "645N",
            MountType = "Pentax 645",
            SupportedFormats = [FilmFormat.OneTwentyMm],
            AcquiredOn = new DateOnly(2022, 9, 20),
        };

        var summicron50 = new Lens
        {
            Name = "Summicron 50mm f/2",
            Brand = "Leica",
            Model = "Summicron-M 50mm f/2",
            MountType = "Leica M",
            FocalLengthMinMm = 50,
            FocalLengthMaxMm = 50,
            MaxAperture = 2.0m,
            MinAperture = 16.0m,
        };

        var nikkor50 = new Lens
        {
            Name = "Nikkor 50mm f/1.8D",
            Brand = "Nikon",
            Model = "AF Nikkor 50mm f/1.8D",
            MountType = "Nikon F",
            FocalLengthMinMm = 50,
            FocalLengthMaxMm = 50,
            MaxAperture = 1.8m,
            MinAperture = 22.0m,
        };

        var pentax75 = new Lens
        {
            Name = "SMC FA 75mm f/2.8",
            Brand = "Pentax",
            Model = "SMC FA 645 75mm f/2.8",
            MountType = "Pentax 645",
            FocalLengthMinMm = 75,
            FocalLengthMaxMm = 75,
            MaxAperture = 2.8m,
            MinAperture = 22.0m,
        };

        var sb28 = new Flash
        {
            Name = "Nikon SB-28",
            Brand = "Nikon",
            Model = "SB-28",
            GuideNumber = 36,
            HasTtl = true,
        };

        var godoxV860 = new Flash
        {
            Name = "Godox V860III",
            Brand = "Godox",
            Model = "V860III",
            GuideNumber = 60,
            HasTtl = true,
        };

        db.CameraBodies.AddRange(leicaM6, nikonF100, pentax645n);
        db.Lenses.AddRange(summicron50, nikkor50, pentax75);
        db.Flashes.AddRange(sb28, godoxV860);

        db.FilmRolls.AddRange(
            new FilmRoll
            {
                Name = "Kodak Portra 400",
                Brand = "Kodak",
                Format = FilmFormat.ThirtyFiveMm,
                NominalIso = 400,
                FrameCount = 36,
                Status = FilmRollStatus.Scanned,
                CameraBody = leicaM6,
                DateLoaded = new DateOnly(2026, 6, 1),
                DateFinished = new DateOnly(2026, 6, 15),
                DateDeveloped = new DateOnly(2026, 6, 20),
                LabName = "The Darkroom Lab",
                Notes = "Golden-hour street portraits downtown.",
            },
            new FilmRoll
            {
                Name = "Ilford HP5 Plus (pushed to 800)",
                Brand = "Ilford",
                Format = FilmFormat.ThirtyFiveMm,
                NominalIso = 400,
                ExposedAtIso = 800,
                FrameCount = 36,
                Status = FilmRollStatus.Developed,
                CameraBody = nikonF100,
                DateLoaded = new DateOnly(2026, 7, 3),
                DateFinished = new DateOnly(2026, 7, 10),
                DateDeveloped = new DateOnly(2026, 7, 12),
                LabName = "Home darkroom",
                DeveloperNotes = "Ilfotec DD-X 1+4, 9:30 at 20°C for the push.",
            },
            new FilmRoll
            {
                Name = "Kodak Ektar 100",
                Brand = "Kodak",
                Format = FilmFormat.OneTwentyMm,
                NominalIso = 100,
                FrameCount = 16,
                Status = FilmRollStatus.Loaded,
                CameraBody = pentax645n,
                DateLoaded = new DateOnly(2026, 8, 10),
                Notes = "Landscape trip roll — loaded, still shooting.",
            });

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded 3 camera bodies, 3 lenses, 2 flashes and 3 film rolls.");
    }
}
