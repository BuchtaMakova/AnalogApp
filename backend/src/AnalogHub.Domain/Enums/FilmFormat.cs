namespace AnalogHub.Domain.Enums;

/// <summary>Physical film gauge. Values map to real-world format names, not roll length.</summary>
public enum FilmFormat
{
    Unknown = 0,
    ThirtyFiveMm = 35,
    OneTwentyMm = 120,
    OneTenMm = 110,
    LargeFormat4x5 = 400,
    LargeFormat8x10 = 800
}
