using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record UpdateFlashCommand(
    Guid Id,
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    bool IsActive,
    DateOnly? AcquiredOn,
    decimal? GuideNumber,
    bool HasTtl) : IRequest<FlashDto>;

public sealed class UpdateFlashCommandValidator : AbstractValidator<UpdateFlashCommand>
{
    public UpdateFlashCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateFlashCommandHandler : IRequestHandler<UpdateFlashCommand, FlashDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateFlashCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<FlashDto> Handle(UpdateFlashCommand request, CancellationToken cancellationToken)
    {
        var flash = await _db.Flashes.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flash), request.Id);

        flash.Name = request.Name;
        flash.Brand = request.Brand;
        flash.Model = request.Model;
        flash.SerialNumber = request.SerialNumber;
        flash.MountType = request.MountType;
        flash.Notes = request.Notes;
        flash.IsActive = request.IsActive;
        flash.AcquiredOn = request.AcquiredOn;
        flash.GuideNumber = request.GuideNumber;
        flash.HasTtl = request.HasTtl;

        await _db.SaveChangesAsync(cancellationToken);

        return GearMappings.ToDto(flash);
    }
}
