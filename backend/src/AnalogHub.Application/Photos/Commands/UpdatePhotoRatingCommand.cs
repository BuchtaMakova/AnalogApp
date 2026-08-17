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

    public UpdatePhotoRatingCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdatePhotoRatingCommand request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == request.PhotoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.PhotoId);

        photo.Rating = request.Rating;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
