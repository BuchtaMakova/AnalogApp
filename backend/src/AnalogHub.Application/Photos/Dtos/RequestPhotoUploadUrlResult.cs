namespace AnalogHub.Application.Photos.Dtos;

public sealed record RequestPhotoUploadUrlResult(string UploadUrl, string StorageKey, DateTimeOffset ExpiresAtUtc);
