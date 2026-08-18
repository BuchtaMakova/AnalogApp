using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnalogHub.Infrastructure.Persistence.Seed;

/// <summary>
/// Entry point called from Program.cs after migrations to populate demo data. Only the Knowledge
/// Base seed (shared/global, not owned by any one user) runs here — the gear/film-roll demo data is
/// per-user content now, so it's seeded once for the bootstrap admin at registration time instead
/// (see RegisterCommandHandler). Idempotent.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalogHubDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await KnowledgeBaseSeeder.SeedAsync(db, sender, logger, cancellationToken);
    }
}
