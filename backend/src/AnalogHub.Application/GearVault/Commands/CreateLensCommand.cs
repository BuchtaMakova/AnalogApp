using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using FluentValidation;
using MediatR;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record CreateLensCommand(
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    DateOnly? AcquiredOn,
    decimal? FocalLengthMinMm,
    decimal? FocalLengthMaxMm,
    decimal? MaxAperture,
    decimal? MinAperture) : IRequest<LensDto>;

public sealed class CreateLensCommandValidator : AbstractValidator<CreateLensCommand>
{
    public CreateLensCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FocalLengthMaxMm)
            .GreaterThanOrEqualTo(x => x.FocalLengthMinMm!.Value)
            .When(x => x.FocalLengthMinMm.HasValue && x.FocalLengthMaxMm.HasValue);
    }
}

public sealed class CreateLensCommandHandler : IRequestHandler<CreateLensCommand, LensDto>
{
    private readonly IApplicationDbContext _db;

    private readonly ICurrentUserService _currentUser;

    public CreateLensCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<LensDto> Handle(CreateLensCommand request, CancellationToken cancellationToken)
    {
        var lens = new Domain.Entities.Gear.Lens
        {
            UserId = _currentUser.UserId,
            Name = request.Name,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            MountType = request.MountType,
            Notes = request.Notes,
            AcquiredOn = request.AcquiredOn,
            FocalLengthMinMm = request.FocalLengthMinMm,
            FocalLengthMaxMm = request.FocalLengthMaxMm,
            MaxAperture = request.MaxAperture,
            MinAperture = request.MinAperture
        };

        _db.Lenses.Add(lens);
        await _db.SaveChangesAsync(cancellationToken);

        return GearMappings.ToDto(lens);
    }
}
