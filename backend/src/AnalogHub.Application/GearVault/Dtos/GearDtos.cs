using AnalogHub.Domain.Enums;

namespace AnalogHub.Application.GearVault.Dtos;

public sealed record CameraBodyDto(
    Guid Id,
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string? MountType,
    string? Notes,
    bool IsActive,
    DateOnly? AcquiredOn,
    IReadOnlyList<FilmFormat> SupportedFormats);

public sealed record LensDto(
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
    decimal? MinAperture);

public sealed record FlashDto(
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
    bool HasTtl);
