using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Commands.AnalyzePhoto;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AnalogHub.Tests.Photos;

public sealed class AnalyzePhotoCommandHandlerTests
{
    private static FilmRoll CreateFilmRoll() => new()
    {
        Name = "Ilford HP5 Plus",
        Brand = "Ilford",
        Format = FilmFormat.ThirtyFiveMm,
        NominalIso = 400,
        FrameCount = 36,
        Status = FilmRollStatus.Developed
    };

    private static PhotoCritiqueResult CreateCritiqueResult() => new(
        Model: "test-vision-model",
        CompositionScore: 8,
        CompositionNotes: "Strong rule-of-thirds framing.",
        LightingNotes: "Soft, directional light.",
        PosingNotes: null,
        Recommendations: ["Move closer to fill the frame."],
        SuggestedTags: ["street", "candid"],
        RawResponseJson: "{}");

    private static (Mock<IVisionAnalysisService> Vision, Mock<IFileStorageService> Storage, Mock<IDateTimeProvider> Clock)
        CreateMocks(PhotoCritiqueResult critique)
    {
        var vision = new Mock<IVisionAnalysisService>();
        vision.Setup(v => v.AnalyzePhotoAsync(It.IsAny<string>(), It.IsAny<VisionAnalysisContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(critique);

        var storage = new Mock<IFileStorageService>();
        storage.Setup(f => f.GetPresignedDownloadUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/preview.webp");

        var clock = new Mock<IDateTimeProvider>();
        clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));

        return (vision, storage, clock);
    }

    [Fact]
    public async Task Handle_FirstRun_CreatesCritiqueAndAttachesSuggestedTagsAsAiSuggested()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        var photo = new Photo
        {
            FilmRoll = filmRoll,
            FilmRollId = filmRoll.Id,
            OriginalStorageKey = "photos/roll/original.jpg",
            PreviewStorageKey = "photos/roll/preview.webp",
            ContentType = "image/jpeg",
            FileSizeBytes = 2_000_000,
            ProcessingStatus = PhotoProcessingStatus.Ready
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var critique = CreateCritiqueResult();
        var (vision, storage, clock) = CreateMocks(critique);
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object);

        var result = await handler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        result.Critique.CompositionScore.Should().Be(8);
        result.Critique.Model.Should().Be("test-vision-model");
        result.AppliedTags.Should().BeEquivalentTo(["street", "candid"]);

        var savedCritique = await db.PhotoAiCritiques.FirstOrDefaultAsync(c => c.PhotoId == photo.Id);
        savedCritique.Should().NotBeNull();
        savedCritique!.CompositionNotes.Should().Be("Strong rule-of-thirds framing.");

        var savedTags = await db.PhotoTags.Where(pt => pt.PhotoId == photo.Id).ToListAsync();
        savedTags.Should().HaveCount(2);
        savedTags.Should().OnlyContain(pt => pt.IsAiSuggested);
    }

    [Fact]
    public async Task Handle_UsesPreviewStorageKey_NotOriginal_WhenPreviewExists()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        var photo = new Photo
        {
            FilmRoll = filmRoll,
            FilmRollId = filmRoll.Id,
            OriginalStorageKey = "original.jpg",
            PreviewStorageKey = "preview.webp",
            ContentType = "image/jpeg",
            FileSizeBytes = 1000
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var (vision, storage, clock) = CreateMocks(CreateCritiqueResult());
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object);

        await handler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        storage.Verify(f => f.GetPresignedDownloadUrlAsync("preview.webp", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RerunOnAlreadyAnalyzedPhoto_ReplacesThePreviousCritiqueRatherThanDuplicating()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        var photo = new Photo
        {
            FilmRoll = filmRoll,
            FilmRollId = filmRoll.Id,
            OriginalStorageKey = "original.jpg",
            ContentType = "image/jpeg",
            FileSizeBytes = 1000
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var (vision1, storage1, clock1) = CreateMocks(CreateCritiqueResult() with { CompositionScore = 5, Model = "first-pass" });
        var firstHandler = new AnalyzePhotoCommandHandler(db, vision1.Object, storage1.Object, clock1.Object);
        await firstHandler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        var (vision2, storage2, clock2) = CreateMocks(CreateCritiqueResult() with { CompositionScore = 9, Model = "second-pass" });
        var secondHandler = new AnalyzePhotoCommandHandler(db, vision2.Object, storage2.Object, clock2.Object);
        var result = await secondHandler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        result.Critique.CompositionScore.Should().Be(9);
        result.Critique.Model.Should().Be("second-pass");

        var allCritiques = await db.PhotoAiCritiques.Where(c => c.PhotoId == photo.Id).ToListAsync();
        allCritiques.Should().HaveCount(1, "re-running analysis should replace the existing critique, not append a new row");
    }

    [Fact]
    public async Task Handle_PhotoDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var (vision, storage, clock) = CreateMocks(CreateCritiqueResult());
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object);

        var act = () => handler.Handle(new AnalyzePhotoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
