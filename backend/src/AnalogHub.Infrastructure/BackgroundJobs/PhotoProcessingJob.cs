using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Enums;
using Blurhash.ImageSharp;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AnalogHub.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire job body: downloads the original from storage, generates a WebP preview + thumbnail,
/// computes a BlurHash placeholder, and writes the results back onto the Photo row. Invoked via
/// <see cref="HangfirePhotoProcessingJobService"/>.
/// </summary>
public sealed class PhotoProcessingJob
{
    private const int PreviewMaxDimension = 2048;
    private const int ThumbnailMaxDimension = 480;

    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<PhotoProcessingJob> _logger;

    public PhotoProcessingJob(IApplicationDbContext db, IFileStorageService fileStorage, ILogger<PhotoProcessingJob> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid photoId, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos.FindAsync([photoId], cancellationToken);
        if (photo is null)
        {
            _logger.LogWarning("PhotoProcessingJob: photo {PhotoId} no longer exists, skipping", photoId);
            return;
        }

        photo.ProcessingStatus = PhotoProcessingStatus.Processing;
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await using var originalStream = await _fileStorage.DownloadAsync(photo.OriginalStorageKey, cancellationToken);
            using var image = await Image.LoadAsync(originalStream, cancellationToken);

            photo.WidthPx = image.Width;
            photo.HeightPx = image.Height;

            using (var rgbImage = image.CloneAs<Rgb24>())
            {
                photo.BlurHash = Blurhasher.Encode(rgbImage, componentsX: 4, componentsY: 3);
            }

            var previewKey = DerivativeKey(photo.OriginalStorageKey, "preview");
            var thumbnailKey = DerivativeKey(photo.OriginalStorageKey, "thumb");

            await using (var previewBytes = EncodeWebp(image, PreviewMaxDimension))
            {
                await _fileStorage.UploadAsync(previewKey, previewBytes, "image/webp", cancellationToken);
            }

            await using (var thumbBytes = EncodeWebp(image, ThumbnailMaxDimension))
            {
                await _fileStorage.UploadAsync(thumbnailKey, thumbBytes, "image/webp", cancellationToken);
            }

            photo.PreviewStorageKey = previewKey;
            photo.ThumbnailStorageKey = thumbnailKey;
            photo.ProcessingStatus = PhotoProcessingStatus.Ready;
            photo.ProcessingError = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PhotoProcessingJob failed for photo {PhotoId}", photoId);
            photo.ProcessingStatus = PhotoProcessingStatus.Failed;
            photo.ProcessingError = ex.Message;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static MemoryStream EncodeWebp(Image source, int maxDimension)
    {
        using var resized = source.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxDimension, maxDimension)
        }));

        var buffer = new MemoryStream();
        resized.Save(buffer, new WebpEncoder { Quality = 82 });
        buffer.Position = 0;
        return buffer;
    }

    private static string DerivativeKey(string originalKey, string suffix)
    {
        var withoutExtension = Path.ChangeExtension(originalKey, null);
        return $"{withoutExtension}-{suffix}.webp";
    }
}
