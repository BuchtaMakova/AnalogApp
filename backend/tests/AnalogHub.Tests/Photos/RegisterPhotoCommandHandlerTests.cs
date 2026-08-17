using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Commands.RegisterPhoto;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace AnalogHub.Tests.Photos;

public sealed class RegisterPhotoCommandHandlerTests
{
    private static FilmRoll CreateFilmRoll() => new()
    {
        Name = "Kodak Portra 400",
        Brand = "Kodak",
        Format = FilmFormat.ThirtyFiveMm,
        NominalIso = 400,
        FrameCount = 36,
        Status = FilmRollStatus.Loaded
    };

    [Fact]
    public async Task Handle_ValidRequest_CreatesPhotoInUploadedStatusAndEnqueuesProcessing()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        db.FilmRolls.Add(filmRoll);
        await db.SaveChangesAsync();

        var jobService = new Mock<IPhotoProcessingJobService>();
        var handler = new RegisterPhotoCommandHandler(db, jobService.Object);

        var command = new RegisterPhotoCommand(
            filmRoll.Id, "photos/roll/frame.jpg", "image/jpeg", 4_200_000,
            CameraBodyId: null, LensId: null, FlashId: null, FrameNumber: 12,
            CaptureDateUtc: null, Exif: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.FilmRollId.Should().Be(filmRoll.Id);
        result.ProcessingStatus.Should().Be(PhotoProcessingStatus.Uploaded);
        result.OriginalStorageKey.Should().Be("photos/roll/frame.jpg");
        result.FrameNumber.Should().Be(12);

        var savedPhoto = await db.Photos.FindAsync(result.Id);
        savedPhoto.Should().NotBeNull();
        savedPhoto!.ProcessingStatus.Should().Be(PhotoProcessingStatus.Uploaded);

        jobService.Verify(j => j.EnqueueProcessPhoto(result.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExifData_MapsAllFieldsOntoThePhoto()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        db.FilmRolls.Add(filmRoll);
        await db.SaveChangesAsync();

        var handler = new RegisterPhotoCommandHandler(db, Mock.Of<IPhotoProcessingJobService>());

        var exif = new AnalogHub.Application.Photos.Dtos.ExifDataDto(
            Aperture: "f/2.8", ShutterSpeed: "1/125", IsoUsed: 400, FocalLengthMm: 50m,
            FlashFired: false, MeteringMode: "Matrix", ScannerModel: "Noritsu HS-1800",
            GpsLatitude: null, GpsLongitude: null);

        var command = new RegisterPhotoCommand(
            filmRoll.Id, "photos/roll/frame.jpg", "image/jpeg", 4_200_000,
            null, null, null, null, null, exif);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Exif.Aperture.Should().Be("f/2.8");
        result.Exif.IsoUsed.Should().Be(400);
        result.Exif.ScannerModel.Should().Be("Noritsu HS-1800");
    }

    [Fact]
    public async Task Handle_FilmRollDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new RegisterPhotoCommandHandler(db, Mock.Of<IPhotoProcessingJobService>());

        var command = new RegisterPhotoCommand(
            Guid.NewGuid(), "photos/roll/frame.jpg", "image/jpeg", 1000,
            null, null, null, null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CameraBodyDoesNotExist_ThrowsNotFoundExceptionAndDoesNotEnqueueProcessing()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        db.FilmRolls.Add(filmRoll);
        await db.SaveChangesAsync();

        var jobService = new Mock<IPhotoProcessingJobService>();
        var handler = new RegisterPhotoCommandHandler(db, jobService.Object);

        var command = new RegisterPhotoCommand(
            filmRoll.Id, "photos/roll/frame.jpg", "image/jpeg", 1000,
            CameraBodyId: Guid.NewGuid(), null, null, null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        jobService.Verify(j => j.EnqueueProcessPhoto(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCameraBodyLensAndFlash_LinksAllThreeToThePhoto()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = CreateFilmRoll();
        var cameraBody = new CameraBody { Name = "F100", Brand = "Nikon", Model = "F100" };
        var lens = new Lens { Name = "50mm", Brand = "Nikon", Model = "50mm f/1.8D" };
        var flash = new Flash { Name = "SB-28", Brand = "Nikon", Model = "SB-28" };
        db.FilmRolls.Add(filmRoll);
        db.CameraBodies.Add(cameraBody);
        db.Lenses.Add(lens);
        db.Flashes.Add(flash);
        await db.SaveChangesAsync();

        var handler = new RegisterPhotoCommandHandler(db, Mock.Of<IPhotoProcessingJobService>());

        var command = new RegisterPhotoCommand(
            filmRoll.Id, "photos/roll/frame.jpg", "image/jpeg", 1000,
            cameraBody.Id, lens.Id, flash.Id, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.CameraBodyId.Should().Be(cameraBody.Id);
        result.LensId.Should().Be(lens.Id);
        result.FlashId.Should().Be(flash.Id);
    }
}
