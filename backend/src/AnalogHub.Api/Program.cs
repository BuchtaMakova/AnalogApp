using AnalogHub.Api.Middleware;
using AnalogHub.Application;
using AnalogHub.Infrastructure;
using AnalogHub.Infrastructure.Persistence;
using AnalogHub.Infrastructure.Persistence.Seed;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text.Json.Serialization;

// Bootstrap logger: catches anything that goes wrong before the full Serilog pipeline (which needs
// configuration) is up, e.g. a bad connection string during host build.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AnalogHub.Api");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfig) =>
    {
        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", "AnalogHub.Api");

        // Readable console for local dev; compact JSON in Production so logs are easy to ship to
        // an aggregator (Seq, ELK, CloudWatch, ...) without a separate formatting step.
        if (context.HostingEnvironment.IsDevelopment())
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        }
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the JWT returned by POST /api/auth/login."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHealthChecks();

    // Every endpoint requires a valid JWT unless explicitly marked [AllowAnonymous] (auth endpoints,
    // health/metrics probes) — safer default than opting in controller-by-controller.
    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

    // Metrics only (not traces) — enough to demonstrate real request/runtime instrumentation via a
    // scrapeable /metrics endpoint without needing an external collector for this project's scope.
    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter());

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Self-contained demo/portfolio app: migrations and demo-data seeding run on every startup
    // rather than via a separate release pipeline step. Both are idempotent. Disable seeding with
    // "Seed:Enabled": false in config if you don't want the sample gear/film rolls/articles.
    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<AnalogHubDbContext>().Database.MigrateAsync();
    }

    if (app.Configuration.GetValue("Seed:Enabled", true))
    {
        await DbSeeder.SeedAsync(app.Services);
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health").AllowAnonymous();
    app.MapPrometheusScrapingEndpoint("/metrics").AllowAnonymous();

    // Exposes background-job internals (payloads, retry, delete) with no built-in auth wiring here —
    // fine for local inspection, not for a Production container reachable from outside.
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard("/hangfire");
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is thrown by WebApplicationFactory<Program> during test host startup —
    // not a real failure, so it's excluded to keep integration test output clean.
    Log.Fatal(ex, "AnalogHub.Api terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
