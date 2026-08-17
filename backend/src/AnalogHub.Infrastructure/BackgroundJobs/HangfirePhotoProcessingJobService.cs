using AnalogHub.Application.Common.Interfaces;
using Hangfire;

namespace AnalogHub.Infrastructure.BackgroundJobs;

public sealed class HangfirePhotoProcessingJobService : IPhotoProcessingJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfirePhotoProcessingJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string EnqueueProcessPhoto(Guid photoId)
    {
        return _backgroundJobClient.Enqueue<PhotoProcessingJob>(job => job.ProcessAsync(photoId, CancellationToken.None));
    }
}
