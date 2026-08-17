namespace AnalogHub.Application.Common.Interfaces;

/// <summary>
/// Enqueues the background pipeline (thumbnail/preview generation, BlurHash, dimension probing)
/// that runs once a photo's original file has landed in storage. Backed by Hangfire in Infrastructure.
/// </summary>
public interface IPhotoProcessingJobService
{
    /// <summary>Enqueues processing for a photo and returns the background job id.</summary>
    string EnqueueProcessPhoto(Guid photoId);
}
