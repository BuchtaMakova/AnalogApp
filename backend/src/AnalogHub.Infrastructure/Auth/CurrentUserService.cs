using System.Security.Claims;
using AnalogHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AnalogHub.Infrastructure.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            // "sub" is mapped to ClaimTypes.NameIdentifier by the JWT bearer handler's default
            // inbound claim mapping (see JwtTokenGenerator, DependencyInjection — MapInboundClaims
            // isn't overridden). Every endpoint requires authentication by default, so a handler
            // running without a valid claim here means the pipeline let an unauthenticated request
            // through — a bug worth failing loudly on, not a case to handle gracefully.
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new InvalidOperationException("No authenticated user id was found on the current request.");
        }
    }
}
