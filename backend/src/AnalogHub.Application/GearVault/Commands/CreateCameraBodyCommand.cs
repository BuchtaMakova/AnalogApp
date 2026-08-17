using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record CreateCameraBodyCommand(
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    DateOnly? AcquiredOn,
    IReadOnlyList<FilmFormat> SupportedFormats) : IRequest<CameraBodyDto>;

public sealed class CreateCameraBodyCommandValidator : AbstractValidator<CreateCameraBodyCommand>
{
    public CreateCameraBodyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.MountType).MaximumLength(50);
    }
}

public sealed class CreateCameraBodyCommandHandler : IRequestHandler<CreateCameraBodyCommand, CameraBodyDto>
{
    private readonly IApplicationDbContext _db;

    public CreateCameraBodyCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CameraBodyDto> Handle(CreateCameraBodyCommand request, CancellationToken cancellationToken)
    {
        var cameraBody = new Domain.Entities.Gear.CameraBody
        {
            Name = request.Name,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            MountType = request.MountType,
            Notes = request.Notes,
            AcquiredOn = request.AcquiredOn,
            SupportedFormats = request.SupportedFormats.ToList()
        };

        _db.CameraBodies.Add(cameraBody);
        await _db.SaveChangesAsync(cancellationToken);

        return GearMappings.ToDto(cameraBody);
    }
}
