namespace AnalogHub.Infrastructure.Auth;

/// <summary>
/// Bound from configuration section "Jwt". <see cref="Secret"/> must be at least 32 characters
/// (HMAC-SHA256 key) and is provided via user-secrets/environment, never committed to config files.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = "AnalogHub";
    public string Audience { get; set; } = "AnalogHub";
    public int ExpiryMinutes { get; set; } = 120;
}
