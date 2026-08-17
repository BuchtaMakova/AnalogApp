using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AnalogHub.Infrastructure.Auth;

/// <summary>
/// Wraps ASP.NET Core Identity's PBKDF2 hasher without pulling in the full Identity stores/UI —
/// just the well-tested hashing algorithm.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(default!, password);

    public bool Verify(string password, string passwordHash)
        => _inner.VerifyHashedPassword(default!, passwordHash, password) != PasswordVerificationResult.Failed;
}
