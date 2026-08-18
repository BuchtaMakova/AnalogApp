using AnalogHub.Application.Albums.Dtos;
using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Albums.Commands;

public sealed record UpdateAlbumCommand(Guid Id, string Name, string? Description, Guid? CoverPhotoId) : IRequest<AlbumDto>;

public sealed class UpdateAlbumCommandValidator : AbstractValidator<UpdateAlbumCommand>
{
    public UpdateAlbumCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateAlbumCommandHandler : IRequestHandler<UpdateAlbumCommand, AlbumDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateAlbumCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AlbumDto> Handle(UpdateAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _db.Albums.FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Album), request.Id);

        if (request.CoverPhotoId is { } coverPhotoId)
        {
            var coverExists = await _db.Photos.AnyAsync(p => p.Id == coverPhotoId && p.UserId == _currentUser.UserId, cancellationToken);
            if (!coverExists)
            {
                throw new NotFoundException(nameof(Domain.Entities.Photo), coverPhotoId);
            }
        }

        album.Name = request.Name;
        album.Description = request.Description;
        album.CoverPhotoId = request.CoverPhotoId;

        await _db.SaveChangesAsync(cancellationToken);

        var photoCount = await _db.AlbumPhotos.CountAsync(ap => ap.AlbumId == album.Id, cancellationToken);

        return new AlbumDto(album.Id, album.Name, album.Description, album.CoverPhotoId, null, photoCount, album.CreatedAtUtc);
    }
}
