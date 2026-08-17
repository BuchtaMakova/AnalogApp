using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Commands;

public sealed record UpdateCameraBodyCommand(
    Guid Id,
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    bool IsActive,
    DateOnly? AcquiredOn,
    IReadOnlyList<FilmFormat> SupportedFormats) : IRequest<CameraBodyDto>;

public sealed class UpdateCameraBodyCommandValidator : AbstractValidator<UpdateCameraBodyCommand>
{
    public UpdateCameraBodyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.MountType).MaximumLength(50);
    }
}

public sealed class UpdateCameraBodyCommandHandler : IRequestHandler<UpdateCameraBodyCommand, CameraBodyDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateCameraBodyCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CameraBodyDto> Handle(UpdateCameraBodyCommand request, CancellationToken cancellationToken)
    {
        var cameraBody = await _db.CameraBodies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CameraBody), request.Id);

        cameraBody.Name = request.Name;
        cameraBody.Brand = request.Brand;
        cameraBody.Model = request.Model;
        cameraBody.SerialNumber = request.SerialNumber;
        cameraBody.MountType = request.MountType;
        cameraBody.Notes = request.Notes;
        cameraBody.IsActive = request.IsActive;
        cameraBody.AcquiredOn = request.AcquiredOn;
        cameraBody.SupportedFormats = request.SupportedFormats.ToList();

        await _db.SaveChangesAsync(cancellationToken);

        return GearMappings.ToDto(cameraBody);
    }
}
