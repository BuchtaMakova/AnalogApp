namespace AnalogHub.Application.Common.Interfaces;

/// <summary>
/// Populates a freshly-registered bootstrap admin's library with sample gear and film rolls, so the
/// app feels populated immediately for whoever explores it first. Every user has their own private
/// library, so this only ever makes sense to run once, for the first-ever registered user — see
/// RegisterCommand.
/// </summary>
public interface IDemoDataSeeder
{
    Task SeedForFirstUserAsync(Guid userId, CancellationToken cancellationToken);
}
