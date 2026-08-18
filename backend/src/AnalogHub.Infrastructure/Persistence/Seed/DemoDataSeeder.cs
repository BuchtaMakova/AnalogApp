using AnalogHub.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnalogHub.Infrastructure.Persistence.Seed;

public sealed class DemoDataSeeder : IDemoDataSeeder
{
    private readonly AnalogHubDbContext _db;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(AnalogHubDbContext db, ILogger<DemoDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task SeedForFirstUserAsync(Guid userId, CancellationToken cancellationToken)
        => GearAndFilmRollSeeder.SeedAsync(_db, userId, _logger, cancellationToken);
}
