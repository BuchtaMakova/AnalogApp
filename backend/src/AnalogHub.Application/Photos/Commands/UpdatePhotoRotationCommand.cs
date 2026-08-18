using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands;

/// <summary>Sets the non-destructive display rotation (0/90/180/270°) for scans that came in sideways.</summary>
public sealed record UpdatePhotoRotationCommand(Guid PhotoId, int RotationDegrees) : IRequest;

public sealed class UpdatePhotoRotationCommandValidator : AbstractValidator<UpdatePhotoRotationCommand>
{
    public UpdatePhotoRotationCommandValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty();
        RuleFor(x => x.RotationDegrees).Must(d => d is 0 or 90 or 180 or 270)
            .WithMessage("Rotation must be 0, 90, 180 or 270 degrees.");
    }
}

public sealed class UpdatePhotoRotationCommandHandler : IRequestHandler<UpdatePhotoRotationCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdatePhotoRotationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdatePhotoRotationCommand request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.PhotoId);

        photo.RotationDegrees = request.RotationDegrees;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
