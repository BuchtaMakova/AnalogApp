using AnalogHub.Application.Photos.Dtos;
using MediatR;

namespace AnalogHub.Application.Photos.Commands.RequestPhotoUploadUrl;

/// <summary>
/// Step 1 of the upload flow: the client asks for a presigned S3/R2 PUT URL before it streams the
/// original file directly to storage. The returned StorageKey is then passed to
/// <see cref="RegisterPhoto.RegisterPhotoCommand"/> once the upload completes.
/// </summary>
public sealed record RequestPhotoUploadUrlCommand(
    Guid FilmRollId,
    string OriginalFileName,
    string ContentType) : IRequest<RequestPhotoUploadUrlResult>;
