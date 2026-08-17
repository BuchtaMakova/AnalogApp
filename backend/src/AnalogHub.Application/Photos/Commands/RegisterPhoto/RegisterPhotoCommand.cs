using AnalogHub.Application.Photos.Dtos;
using MediatR;

namespace AnalogHub.Application.Photos.Commands.RegisterPhoto;

/// <summary>
/// Step 2 of the upload flow: called once the client has finished streaming the original file to
/// the presigned URL from <see cref="RequestPhotoUploadUrl.RequestPhotoUploadUrlCommand"/>. Creates
/// the Photo row and enqueues the background job that generates preview/thumbnail derivatives and
/// the BlurHash placeholder.
/// </summary>
public sealed record RegisterPhotoCommand(
    Guid FilmRollId,
    string StorageKey,
    string ContentType,
    long FileSizeBytes,
    Guid? CameraBodyId,
    Guid? LensId,
    Guid? FlashId,
    int? FrameNumber,
    DateTimeOffset? CaptureDateUtc,
    ExifDataDto? Exif) : IRequest<PhotoDto>;
