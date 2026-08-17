using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Infrastructure.AI.Gemini;
using AnalogHub.Infrastructure.AI.Mock;
using AnalogHub.Infrastructure.BackgroundJobs;
using AnalogHub.Infrastructure.Common;
using AnalogHub.Infrastructure.Persistence;
using AnalogHub.Infrastructure.Persistence.Interceptors;
using AnalogHub.Infrastructure.Storage;
using Amazon.S3;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
