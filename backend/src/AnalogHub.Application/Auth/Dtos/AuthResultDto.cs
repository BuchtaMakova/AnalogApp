namespace AnalogHub.Application.Auth.Dtos;

public sealed record AuthResultDto(
    string Token,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    string Role);
