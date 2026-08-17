using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnalogHub.Infrastructure.Persistence.Seed;

/// <summary>Entry point called from Program.cs after migrations to populate demo data. Both steps are idempotent.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalogHubDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await GearAndFilmRollSeeder.SeedAsync(db, logger, cancellationToken);
        await KnowledgeBaseSeeder.SeedAsync(db, sender, logger, cancellationToken);
    }
}
