using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;

namespace AnalogHub.Application.GearVault;

internal static class GearMappings
{
    public static CameraBodyDto ToDto(CameraBody c) => new(
        c.Id, c.Name, c.Brand, c.Model, c.SerialNumber, c.MountType, c.Notes, c.IsActive, c.AcquiredOn,
        c.SupportedFormats);

    public static LensDto ToDto(Lens l) => new(
        l.Id, l.Name, l.Brand, l.Model, l.SerialNumber, l.MountType, l.Notes, l.IsActive, l.AcquiredOn,
        l.FocalLengthMinMm, l.FocalLengthMaxMm, l.MaxAperture, l.MinAperture);

    public static FlashDto ToDto(Flash f) => new(
        f.Id, f.Name, f.Brand, f.Model, f.SerialNumber, f.MountType, f.Notes, f.IsActive, f.AcquiredOn,
        f.GuideNumber, f.HasTtl);
}
