using AnalogHub.Api.Middleware;
using AnalogHub.Application;
using AnalogHub.Infrastructure;
using AnalogHub.Infrastructure.Persistence;
using AnalogHub.Infrastructure.Persistence.Seed;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Self-contained demo/portfolio app: migrations and demo-data seeding run on every startup rather
// than via a separate release pipeline step. Both are idempotent. Disable seeding with
// "Seed:Enabled": false in config if you don't want the sample gear/film rolls/articles.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AnalogHubDbContext>().Database.MigrateAsync();
}

if (app.Configuration.GetValue("Seed:Enabled", true))
{
    await DbSeeder.SeedAsync(app.Services);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.UseHangfireDashboard("/hangfire");

app.Run();

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
