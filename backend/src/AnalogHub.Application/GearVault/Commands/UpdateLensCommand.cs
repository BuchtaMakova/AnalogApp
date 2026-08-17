using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record UpdateLensCommand(
    Guid Id,
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    bool IsActive,
    DateOnly? AcquiredOn,
    decimal? FocalLengthMinMm,
    decimal? FocalLengthMaxMm,
    decimal? MaxAperture,
    decimal? MinAperture) : IRequest<LensDto>;

public sealed class UpdateLensCommandValidator : AbstractValidator<UpdateLensCommand>
{
    public UpdateLensCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FocalLengthMaxMm)
            .GreaterThanOrEqualTo(x => x.FocalLengthMinMm!.Value)
            .When(x => x.FocalLengthMinMm.HasValue && x.FocalLengthMaxMm.HasValue);
    }
}

public sealed class UpdateLensCommandHandler : IRequestHandler<UpdateLensCommand, LensDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateLensCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<LensDto> Handle(UpdateLensCommand request, CancellationToken cancellationToken)
    {
        var lens = await _db.Lenses.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Lens), request.Id);

        lens.Name = request.Name;
        lens.Brand = request.Brand;
        lens.Model = request.Model;
        lens.SerialNumber = request.SerialNumber;
        lens.MountType = request.MountType;
        lens.Notes = request.Notes;
        lens.IsActive = request.IsActive;
        lens.AcquiredOn = request.AcquiredOn;
        lens.FocalLengthMinMm = request.FocalLengthMinMm;
        lens.FocalLengthMaxMm = request.FocalLengthMaxMm;
        lens.MaxAperture = request.MaxAperture;
        lens.MinAperture = request.MinAperture;

        await _db.SaveChangesAsync(cancellationToken);

        return GearMappings.ToDto(lens);
    }
}
