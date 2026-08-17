using AnalogHub.Application.Albums.Commands;
using AnalogHub.Application.Albums.Queries;
using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace AnalogHub.Tests.Albums;

public sealed class AlbumTests
{
    private static Photo CreatePhoto(FilmRoll filmRoll) => new()
    {
        FilmRoll = filmRoll,
        FilmRollId = filmRoll.Id,
        OriginalStorageKey = "original.jpg",
        ThumbnailStorageKey = "thumb.webp",
        ContentType = "image/jpeg",
        FileSizeBytes = 1000
    };

    private static Mock<IFileStorageService> CreateFileStorageMock()
    {
        var mock = new Mock<IFileStorageService>();
        mock.Setup(f => f.GetPresignedDownloadUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/thumb.webp");
        return mock;
    }

    [Fact]
    public async Task Create_ValidRequest_PersistsAlbumWithZeroPhotos()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateAlbumCommandHandler(db);

        var result = await handler.Handle(new CreateAlbumCommand("Summer 2026", "Best of the season"), CancellationToken.None);

        result.Name.Should().Be("Summer 2026");
        result.PhotoCount.Should().Be(0);
        (await db.Albums.FindAsync(result.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ExistingAlbum_OverwritesNameAndDescription()
    {
        using var db = TestDbContextFactory.Create();
        var album = new Album { Name = "Old name", Description = "Old" };
        db.Albums.Add(album);
        await db.SaveChangesAsync();

        var handler = new UpdateAlbumCommandHandler(db);
        var result = await handler.Handle(new UpdateAlbumCommand(album.Id, "New name", "New description", null), CancellationToken.None);

        result.Name.Should().Be("New name");
        result.Description.Should().Be("New description");
    }

    [Fact]
    public async Task Update_WithNonExistentCoverPhoto_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var album = new Album { Name = "Album" };
        db.Albums.Add(album);
        await db.SaveChangesAsync();

        var handler = new UpdateAlbumCommandHandler(db);
        var command = new UpdateAlbumCommand(album.Id, "Album", null, Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_AlbumDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new UpdateAlbumCommandHandler(db);

        var act = () => handler.Handle(new UpdateAlbumCommand(Guid.NewGuid(), "X", null, null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingAlbum_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var album = new Album { Name = "To delete" };
        db.Albums.Add(album);
        await db.SaveChangesAsync();

        var handler = new DeleteAlbumCommandHandler(db);
        await handler.Handle(new DeleteAlbumCommand(album.Id), CancellationToken.None);

        (await db.Albums.FindAsync(album.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_AlbumDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteAlbumCommandHandler(db);

        var act = () => handler.Handle(new DeleteAlbumCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddPhotoToAlbum_ValidIds_CreatesLinkWithIncrementingSortOrder()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        var album = new Album { Name = "Album" };
        var photo1 = CreatePhoto(filmRoll);
        var photo2 = CreatePhoto(filmRoll);
        db.FilmRolls.Add(filmRoll);
        db.Albums.Add(album);
        db.Photos.AddRange(photo1, photo2);
        await db.SaveChangesAsync();

        var handler = new AddPhotoToAlbumCommandHandler(db);
        await handler.Handle(new AddPhotoToAlbumCommand(album.Id, photo1.Id), CancellationToken.None);
        await handler.Handle(new AddPhotoToAlbumCommand(album.Id, photo2.Id), CancellationToken.None);

        var links = db.AlbumPhotos.Where(ap => ap.AlbumId == album.Id).OrderBy(ap => ap.SortOrder).ToList();
        links.Should().HaveCount(2);
        links[0].PhotoId.Should().Be(photo1.Id);
        links[0].SortOrder.Should().Be(0);
        links[1].SortOrder.Should().Be(1);
    }

    [Fact]
    public async Task AddPhotoToAlbum_AlreadyLinked_IsIdempotent()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        var album = new Album { Name = "Album" };
        var photo = CreatePhoto(filmRoll);
        db.FilmRolls.Add(filmRoll);
        db.Albums.Add(album);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var handler = new AddPhotoToAlbumCommandHandler(db);
        await handler.Handle(new AddPhotoToAlbumCommand(album.Id, photo.Id), CancellationToken.None);
        await handler.Handle(new AddPhotoToAlbumCommand(album.Id, photo.Id), CancellationToken.None);

        db.AlbumPhotos.Count(ap => ap.AlbumId == album.Id).Should().Be(1);
    }

    [Fact]
    public async Task AddPhotoToAlbum_AlbumDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        var photo = CreatePhoto(filmRoll);
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var handler = new AddPhotoToAlbumCommandHandler(db);
        var act = () => handler.Handle(new AddPhotoToAlbumCommand(Guid.NewGuid(), photo.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemovePhotoFromAlbum_ExistingLink_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        var album = new Album { Name = "Album" };
        var photo = CreatePhoto(filmRoll);
        db.FilmRolls.Add(filmRoll);
        db.Albums.Add(album);
        db.Photos.Add(photo);
        db.AlbumPhotos.Add(new AlbumPhoto { AlbumId = album.Id, PhotoId = photo.Id, SortOrder = 0 });
        await db.SaveChangesAsync();

        var handler = new RemovePhotoFromAlbumCommandHandler(db);
        await handler.Handle(new RemovePhotoFromAlbumCommand(album.Id, photo.Id), CancellationToken.None);

        db.AlbumPhotos.Any(ap => ap.AlbumId == album.Id && ap.PhotoId == photo.Id).Should().BeFalse();
    }

    [Fact]
    public async Task RemovePhotoFromAlbum_LinkDoesNotExist_DoesNothingSilently()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new RemovePhotoFromAlbumCommandHandler(db);

        var act = () => handler.Handle(new RemovePhotoFromAlbumCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAlbums_ReturnsPhotoCountAndResolvedCoverUrl()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        var photo = CreatePhoto(filmRoll);
        var album = new Album { Name = "Album", CoverPhotoId = photo.Id, CoverPhoto = photo };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        db.Albums.Add(album);
        db.AlbumPhotos.Add(new AlbumPhoto { AlbumId = album.Id, PhotoId = photo.Id, SortOrder = 0 });
        await db.SaveChangesAsync();

        var fileStorage = CreateFileStorageMock();
        var handler = new GetAlbumsQueryHandler(db, fileStorage.Object);

        var result = await handler.Handle(new GetAlbumsQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].PhotoCount.Should().Be(1);
        result[0].CoverPhotoThumbnailUrl.Should().Be("https://storage.example.com/thumb.webp");
    }

    [Fact]
    public async Task GetAlbumById_ReturnsPhotosInSortOrder()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        var album = new Album { Name = "Album" };
        var photo1 = CreatePhoto(filmRoll);
        var photo2 = CreatePhoto(filmRoll);
        db.FilmRolls.Add(filmRoll);
        db.Albums.Add(album);
        db.Photos.AddRange(photo1, photo2);
        db.AlbumPhotos.AddRange(
            new AlbumPhoto { AlbumId = album.Id, PhotoId = photo2.Id, SortOrder = 0 },
            new AlbumPhoto { AlbumId = album.Id, PhotoId = photo1.Id, SortOrder = 1 });
        await db.SaveChangesAsync();

        var fileStorage = CreateFileStorageMock();
        var handler = new GetAlbumByIdQueryHandler(db, fileStorage.Object);

        var result = await handler.Handle(new GetAlbumByIdQuery(album.Id), CancellationToken.None);

        result.Photos.Should().HaveCount(2);
        result.Photos[0].Id.Should().Be(photo2.Id);
    }

    [Fact]
    public async Task GetAlbumById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetAlbumByIdQueryHandler(db, CreateFileStorageMock().Object);

        var act = () => handler.Handle(new GetAlbumByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
