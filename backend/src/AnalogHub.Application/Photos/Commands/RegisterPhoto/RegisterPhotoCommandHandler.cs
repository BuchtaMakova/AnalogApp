using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Dtos;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using AnalogHub.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands.RegisterPhoto;

public sealed class RegisterPhotoCommandHandler : IRequestHandler<RegisterPhotoCommand, PhotoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPhotoProcessingJobService _processingJobs;

    public RegisterPhotoCommandHandler(IApplicationDbContext db, IPhotoProcessingJobService processingJobs)
    {
        _db = db;
        _processingJobs = processingJobs;
    }

    public async Task<PhotoDto> Handle(RegisterPhotoCommand request, CancellationToken cancellationToken)
    {
        var filmRollExists = await _db.FilmRolls.AnyAsync(r => r.Id == request.FilmRollId, cancellationToken);
        if (!filmRollExists)
        {
            throw new NotFoundException(nameof(FilmRoll), request.FilmRollId);
        }

        await EnsureGearExistsAsync(request.CameraBodyId, _db.CameraBodies, nameof(CameraBody), cancellationToken);
        await EnsureGearExistsAsync(request.LensId, _db.Lenses, nameof(Lens), cancellationToken);
        await EnsureGearExistsAsync(request.FlashId, _db.Flashes, nameof(Flash), cancellationToken);

        var photo = new Photo
        {
            FilmRollId = request.FilmRollId,
            CameraBodyId = request.CameraBodyId,
            LensId = request.LensId,
            FlashId = request.FlashId,
            FrameNumber = request.FrameNumber,
            CaptureDateUtc = request.CaptureDateUtc,
            OriginalStorageKey = request.StorageKey,
            FileSizeBytes = request.FileSizeBytes,
            ContentType = request.ContentType,
            ProcessingStatus = PhotoProcessingStatus.Uploaded,
            Exif = MapExif(request.Exif)
        };

        _db.Photos.Add(photo);
        await _db.SaveChangesAsync(cancellationToken);

        _processingJobs.EnqueueProcessPhoto(photo.Id);

        return MapToDto(photo);
    }

    private static async Task EnsureGearExistsAsync<TGear>(
        Guid? gearId,
        DbSet<TGear> gearSet,
        string entityName,
        CancellationToken cancellationToken)
        where TGear : Gear
    {
        if (gearId is null)
        {
            return;
        }

        var exists = await gearSet.AnyAsync(g => g.Id == gearId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(entityName, gearId);
        }
    }

    private static ExifData MapExif(ExifDataDto? dto) => dto is null
        ? new ExifData()
        : new ExifData
        {
            Aperture = dto.Aperture,
            ShutterSpeed = dto.ShutterSpeed,
            IsoUsed = dto.IsoUsed,
            FocalLengthMm = dto.FocalLengthMm,
            FlashFired = dto.FlashFired,
            MeteringMode = dto.MeteringMode,
            ScannerModel = dto.ScannerModel,
            GpsLatitude = dto.GpsLatitude,
            GpsLongitude = dto.GpsLongitude
        };

    private static PhotoDto MapToDto(Photo photo) => new(
        photo.Id,
        photo.FilmRollId,
        photo.CameraBodyId,
        photo.LensId,
        photo.FlashId,
        photo.FrameNumber,
        photo.CaptureDateUtc,
        photo.OriginalStorageKey,
        photo.PreviewStorageKey,
        photo.ThumbnailStorageKey,
        photo.BlurHash,
        photo.WidthPx,
        photo.HeightPx,
        photo.FileSizeBytes,
        photo.ContentType,
        photo.Rating,
        photo.ProcessingStatus,
        new ExifDataDto(
            photo.Exif.Aperture,
            photo.Exif.ShutterSpeed,
            photo.Exif.IsoUsed,
            photo.Exif.FocalLengthMm,
            photo.Exif.FlashFired,
            photo.Exif.MeteringMode,
            photo.Exif.ScannerModel,
            photo.Exif.GpsLatitude,
            photo.Exif.GpsLongitude),
        photo.CreatedAtUtc);
}
