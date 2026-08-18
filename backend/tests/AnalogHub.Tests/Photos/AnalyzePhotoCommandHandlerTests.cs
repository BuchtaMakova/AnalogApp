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
    private static FilmRoll CreateFilmRoll(Guid ownerId) => new()
    {
        UserId = ownerId,
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
        storage.Setup(f => f.GetPresignedDownloadUrlAsync(
                It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<string?>()))
            .ReturnsAsync("https://storage.example.com/preview.webp");

        var clock = new Mock<IDateTimeProvider>();
        clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));

        return (vision, storage, clock);
    }

    [Fact]
    public async Task Handle_FirstRun_CreatesCritiqueAndAttachesSuggestedTagsAsAiSuggested()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var filmRoll = CreateFilmRoll(currentUser.UserId);
        var photo = new Photo
        {
            UserId = currentUser.UserId,
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
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object, currentUser);

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
        var currentUser = new FakeCurrentUserService();
        var filmRoll = CreateFilmRoll(currentUser.UserId);
        var photo = new Photo
        {
            UserId = currentUser.UserId,
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
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object, currentUser);

        await handler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        storage.Verify(f => f.GetPresignedDownloadUrlAsync(
            "preview.webp", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RequestsImageUrl_NotBrowserFacing()
    {
        // The API fetches this URL itself to relay bytes to the vision model, so it needs a host
        // reachable from the API's own network — regressing this to forBrowser: true breaks real
        // vision analysis behind a reverse proxy / Docker network split (ServiceUrl != PublicServiceUrl).
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var filmRoll = CreateFilmRoll(currentUser.UserId);
        var photo = new Photo
        {
            UserId = currentUser.UserId,
            FilmRoll = filmRoll,
            FilmRollId = filmRoll.Id,
            OriginalStorageKey = "original.jpg",
            ContentType = "image/jpeg",
            FileSizeBytes = 1000
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var (vision, storage, clock) = CreateMocks(CreateCritiqueResult());
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object, currentUser);

        await handler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        storage.Verify(f => f.GetPresignedDownloadUrlAsync(
            It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>(), false, It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RerunOnAlreadyAnalyzedPhoto_ReplacesThePreviousCritiqueRatherThanDuplicating()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var filmRoll = CreateFilmRoll(currentUser.UserId);
        var photo = new Photo
        {
            UserId = currentUser.UserId,
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
        var firstHandler = new AnalyzePhotoCommandHandler(db, vision1.Object, storage1.Object, clock1.Object, currentUser);
        await firstHandler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        var (vision2, storage2, clock2) = CreateMocks(CreateCritiqueResult() with { CompositionScore = 9, Model = "second-pass" });
        var secondHandler = new AnalyzePhotoCommandHandler(db, vision2.Object, storage2.Object, clock2.Object, currentUser);
        var result = await secondHandler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        result.Critique.CompositionScore.Should().Be(9);
        result.Critique.Model.Should().Be("second-pass");

        var allCritiques = await db.PhotoAiCritiques.Where(c => c.PhotoId == photo.Id).ToListAsync();
        allCritiques.Should().HaveCount(1, "re-running analysis should replace the existing critique, not append a new row");
    }

    [Fact]
    public async Task Handle_RerunWithDifferentSuggestedTags_ReplacesAiTagsButKeepsManualTags()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var filmRoll = CreateFilmRoll(currentUser.UserId);
        var photo = new Photo
        {
            UserId = currentUser.UserId,
            FilmRoll = filmRoll,
            FilmRollId = filmRoll.Id,
            OriginalStorageKey = "original.jpg",
            ContentType = "image/jpeg",
            FileSizeBytes = 1000
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var (vision1, storage1, clock1) = CreateMocks(CreateCritiqueResult() with { SuggestedTags = ["street", "candid"] });
        var firstHandler = new AnalyzePhotoCommandHandler(db, vision1.Object, storage1.Object, clock1.Object, currentUser);
        await firstHandler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        // A tag the user added by hand, independent of AI suggestions — must survive re-analysis.
        var manualTag = new Tag { Name = "favorite", Slug = "favorite" };
        db.Tags.Add(manualTag);
        await db.SaveChangesAsync();
        db.PhotoTags.Add(new PhotoTag { PhotoId = photo.Id, TagId = manualTag.Id, IsAiSuggested = false });
        await db.SaveChangesAsync();

        var (vision2, storage2, clock2) = CreateMocks(CreateCritiqueResult() with { SuggestedTags = ["urban", "night"] });
        var secondHandler = new AnalyzePhotoCommandHandler(db, vision2.Object, storage2.Object, clock2.Object, currentUser);
        var result = await secondHandler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        result.AppliedTags.Should().BeEquivalentTo(["urban", "night"]);

        var savedTags = await db.PhotoTags.Where(pt => pt.PhotoId == photo.Id).Include(pt => pt.Tag).ToListAsync();
        savedTags.Select(pt => pt.Tag.Slug).Should().BeEquivalentTo(
            ["urban", "night", "favorite"],
            "stale AI tags from the first run (street, candid) must be gone, but the manually-added tag must survive");
        savedTags.Single(pt => pt.Tag.Slug == "favorite").IsAiSuggested.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PhotoDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var (vision, storage, clock) = CreateMocks(CreateCritiqueResult());
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object, new FakeCurrentUserService());

        var act = () => handler.Handle(new AnalyzePhotoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_PhotoOwnedByAnotherUser_ThrowsNotFoundExceptionWithoutCallingVisionModel()
    {
        using var db = TestDbContextFactory.Create();
        var owner = new FakeCurrentUserService();
        var filmRoll = CreateFilmRoll(owner.UserId);
        var photo = new Photo
        {
            UserId = owner.UserId,
            FilmRoll = filmRoll,
            FilmRollId = filmRoll.Id,
            OriginalStorageKey = "original.jpg",
            ContentType = "image/jpeg",
            FileSizeBytes = 1000
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var (vision, storage, clock) = CreateMocks(CreateCritiqueResult());
        var handler = new AnalyzePhotoCommandHandler(db, vision.Object, storage.Object, clock.Object, new FakeCurrentUserService());

        var act = () => handler.Handle(new AnalyzePhotoCommand(photo.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        vision.Verify(
            v => v.AnalyzePhotoAsync(It.IsAny<string>(), It.IsAny<VisionAnalysisContext>(), It.IsAny<CancellationToken>()),
            Times.Never, "someone else's photo bytes must never reach the vision model");
    }
}
