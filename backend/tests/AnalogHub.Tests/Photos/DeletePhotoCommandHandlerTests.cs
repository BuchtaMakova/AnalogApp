using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Commands;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AnalogHub.Tests.Photos;

public sealed class DeletePhotoCommandHandlerTests
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

    [Fact]
    public async Task Handle_ExistingPhoto_RemovesItAndDeletesAllStorageObjects()
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
            ThumbnailStorageKey = "thumb.webp",
            ContentType = "image/jpeg",
            FileSizeBytes = 1000
        };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        await db.SaveChangesAsync();

        var storage = new Mock<IFileStorageService>();
        var handler = new DeletePhotoCommandHandler(db, storage.Object, currentUser);

        await handler.Handle(new DeletePhotoCommand(photo.Id), CancellationToken.None);

        (await db.Photos.FindAsync(photo.Id)).Should().BeNull();
        storage.Verify(f => f.DeleteAsync("original.jpg", It.IsAny<CancellationToken>()), Times.Once);
        storage.Verify(f => f.DeleteAsync("preview.webp", It.IsAny<CancellationToken>()), Times.Once);
        storage.Verify(f => f.DeleteAsync("thumb.webp", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PhotoWithoutDerivatives_OnlyDeletesTheOriginal()
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

        var storage = new Mock<IFileStorageService>();
        var handler = new DeletePhotoCommandHandler(db, storage.Object, currentUser);

        await handler.Handle(new DeletePhotoCommand(photo.Id), CancellationToken.None);

        storage.Verify(f => f.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        storage.Verify(f => f.DeleteAsync("original.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PhotoDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var storage = new Mock<IFileStorageService>();
        var handler = new DeletePhotoCommandHandler(db, storage.Object, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeletePhotoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        storage.Verify(f => f.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PhotoOwnedByAnotherUser_ThrowsNotFoundExceptionAndLeavesStorageUntouched()
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

        var storage = new Mock<IFileStorageService>();
        var handler = new DeletePhotoCommandHandler(db, storage.Object, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeletePhotoCommand(photo.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        storage.Verify(f => f.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        (await db.Photos.FindAsync(photo.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_PhotoLinkedToTagsAndAlbums_CascadeDeletesJoinRows()
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
        var tag = new Tag { Name = "street", Slug = "street" };
        var album = new Album { UserId = currentUser.UserId, Name = "Best of 2026" };
        db.FilmRolls.Add(filmRoll);
        db.Photos.Add(photo);
        db.Tags.Add(tag);
        db.Albums.Add(album);
        await db.SaveChangesAsync();

        db.PhotoTags.Add(new PhotoTag { PhotoId = photo.Id, TagId = tag.Id });
        db.AlbumPhotos.Add(new AlbumPhoto { AlbumId = album.Id, PhotoId = photo.Id, SortOrder = 0 });
        await db.SaveChangesAsync();

        var storage = new Mock<IFileStorageService>();
        var handler = new DeletePhotoCommandHandler(db, storage.Object, currentUser);

        await handler.Handle(new DeletePhotoCommand(photo.Id), CancellationToken.None);

        (await db.PhotoTags.AnyAsync(pt => pt.PhotoId == photo.Id)).Should().BeFalse();
        (await db.AlbumPhotos.AnyAsync(ap => ap.PhotoId == photo.Id)).Should().BeFalse();
    }
}
