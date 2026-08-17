using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Infrastructure.AI.Gemini;
using AnalogHub.Infrastructure.AI.Mock;
using AnalogHub.Infrastructure.Auth;
using AnalogHub.Infrastructure.BackgroundJobs;
using AnalogHub.Infrastructure.Common;
using AnalogHub.Infrastructure.Persistence;
using AnalogHub.Infrastructure.Persistence.Interceptors;
using AnalogHub.Infrastructure.Storage;
using Amazon.S3;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AnalogHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AnalogHubDb")
            ?? throw new InvalidOperationException("Connection string 'AnalogHubDb' was not found.");

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<AnalogHubDbContext>((sp, options) =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector())
                .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AnalogHubDbContext>());

        services.Configure<S3StorageOptions>(configuration.GetSection(S3StorageOptions.SectionName));
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = configuration.GetSection(S3StorageOptions.SectionName).Get<S3StorageOptions>()
                ?? throw new InvalidOperationException("Storage configuration section is missing.");

            var config = new AmazonS3Config { ForcePathStyle = true };
            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                config.ServiceURL = options.ServiceUrl;
                // The SDK doesn't infer the presigned-URL scheme from ServiceURL alone — without
                // this, http:// endpoints (e.g. local MinIO) still get signed as https://.
                config.UseHttp = options.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(options.Region);
            }

            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        });
        services.AddScoped<IFileStorageService, S3FileStorageService>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
        services.AddHangfireServer();
        services.AddScoped<IPhotoProcessingJobService, HangfirePhotoProcessingJobService>();
        services.AddScoped<PhotoProcessingJob>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");
        if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
        {
            // Fail fast rather than silently signing tokens with an empty key. Set via
            // `dotnet user-secrets set "Jwt:Secret" "..."` locally; docker-compose.yml supplies a
            // demo default through the JWT_SECRET env var.
            throw new InvalidOperationException(
                "Jwt:Secret is not configured. Run: dotnet user-secrets set \"Jwt:Secret\" \"<random-32+-char-value>\" --project src/AnalogHub.Api");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        // Demo mode: swap the whole AI provider for deterministic, no-API-key-required stand-ins so
        // the app is fully explorable straight from a fresh clone. Real deployments set
        // "UseMockAi": false (or leave it unset — that's the default) and configure Gemini:ApiKey.
        if (configuration.GetValue("UseMockAi", false))
        {
            services.AddScoped<IVisionAnalysisService, MockVisionAnalysisService>();
            services.AddScoped<IEmbeddingService, MockEmbeddingService>();
            services.AddScoped<IChatCompletionService, MockChatCompletionService>();
        }
        else
        {
            services.AddHttpClient<GeminiClient>();
            services.AddHttpClient(); // plain IHttpClientFactory, used to download photo bytes for vision analysis
            services.AddScoped<IVisionAnalysisService, GeminiVisionAnalysisService>();
            services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
            services.AddScoped<IChatCompletionService, GeminiChatCompletionService>();
        }

        return services;
    }
}
