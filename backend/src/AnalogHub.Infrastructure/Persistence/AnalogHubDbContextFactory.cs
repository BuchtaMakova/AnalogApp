using AnalogHub.Infrastructure.Common;
using AnalogHub.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AnalogHub.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add` run directly against this project without spinning up the API host.
/// Connection string is read from ANALOGHUB_CONNECTION_STRING, falling back to a local dev default.
/// </summary>
public sealed class AnalogHubDbContextFactory : IDesignTimeDbContextFactory<AnalogHubDbContext>
{
    public AnalogHubDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ANALOGHUB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5433;Database=analoghub_db;Username=postgres;Password=postgrespassword";

        var optionsBuilder = new DbContextOptionsBuilder<AnalogHubDbContext>()
            .UseNpgsql(connectionString, o => o.UseVector());

        return new AnalogHubDbContext(optionsBuilder.Options, new AuditableEntitySaveChangesInterceptor(new SystemDateTimeProvider()));
    }
}
