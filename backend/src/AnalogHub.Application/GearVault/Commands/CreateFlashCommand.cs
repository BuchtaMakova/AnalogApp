using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using FluentValidation;
using MediatR;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record CreateFlashCommand(
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    DateOnly? AcquiredOn,
    decimal? GuideNumber,
    bool HasTtl) : IRequest<FlashDto>;

public sealed class CreateFlashCommandValidator : AbstractValidator<CreateFlashCommand>
{
    public CreateFlashCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
    }
}

public sealed class CreateFlashCommandHandler : IRequestHandler<CreateFlashCommand, FlashDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateFlashCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<FlashDto> Handle(CreateFlashCommand request, CancellationToken cancellationToken)
    {
        var flash = new Domain.Entities.Gear.Flash
        {
            UserId = _currentUser.UserId,
            Name = request.Name,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            MountType = request.MountType,
            Notes = request.Notes,
            AcquiredOn = request.AcquiredOn,
            GuideNumber = request.GuideNumber,
            HasTtl = request.HasTtl
        };

        _db.Flashes.Add(flash);
        await _db.SaveChangesAsync(cancellationToken);

        return GearMappings.ToDto(flash);
    }
}
