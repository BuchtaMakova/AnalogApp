using AnalogHub.Domain.Entities;

namespace AnalogHub.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAtUtc) GenerateToken(User user);
}
