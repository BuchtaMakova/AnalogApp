using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands;

public sealed record UpdatePhotoRatingCommand(Guid PhotoId, byte Rating) : IRequest;

public sealed class UpdatePhotoRatingCommandValidator : AbstractValidator<UpdatePhotoRatingCommand>
{
    public UpdatePhotoRatingCommandValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween((byte)0, (byte)5);
    }
}

public sealed class UpdatePhotoRatingCommandHandler : IRequestHandler<UpdatePhotoRatingCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdatePhotoRatingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdatePhotoRatingCommand request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.PhotoId);

        photo.Rating = request.Rating;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
