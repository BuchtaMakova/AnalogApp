using AnalogHub.Application.Common.Interfaces;

namespace AnalogHub.Tests.Common;

/// <summary>Fresh random owner per instance unless overridden — lets tests construct a second
/// instance with a different UserId to prove cross-user isolation.</summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; } = Guid.NewGuid();
}
