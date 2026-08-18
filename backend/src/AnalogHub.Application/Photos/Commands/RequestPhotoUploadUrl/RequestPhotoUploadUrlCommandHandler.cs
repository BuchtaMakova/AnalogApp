using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands.RequestPhotoUploadUrl;

public sealed class RequestPhotoUploadUrlCommandHandler
    : IRequestHandler<RequestPhotoUploadUrlCommand, RequestPhotoUploadUrlResult>
{
    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(15);

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public RequestPhotoUploadUrlCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _db = db;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<RequestPhotoUploadUrlResult> Handle(
        RequestPhotoUploadUrlCommand request,
        CancellationToken cancellationToken)
    {
        var filmRollExists = await _db.FilmRolls
            .AnyAsync(r => r.Id == request.FilmRollId && r.UserId == _currentUser.UserId, cancellationToken);

        if (!filmRollExists)
        {
            throw new NotFoundException(nameof(Domain.Entities.FilmRoll), request.FilmRollId);
        }

        var extension = Path.GetExtension(request.OriginalFileName);
        var objectKey = $"photos/{request.FilmRollId}/{Guid.NewGuid()}{extension}";

        var presigned = await _fileStorage.CreatePresignedUploadUrlAsync(
            objectKey,
            request.ContentType,
            UploadUrlExpiry,
            cancellationToken);

        return new RequestPhotoUploadUrlResult(presigned.UploadUrl, presigned.ObjectKey, presigned.ExpiresAtUtc);
    }
}
