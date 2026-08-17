using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Common.Utils;
using AnalogHub.Application.Photos.Dtos;
using AnalogHub.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands.AnalyzePhoto;

/// <summary>
/// Runs the multimodal vision critique for a photo: composition, light/flash handling, posing (for
/// portraits) and forward-looking recommendations, then persists the structured result and attaches
/// AI-suggested tags. Re-running replaces the previous critique rather than appending history.
/// </summary>
public sealed record AnalyzePhotoCommand(Guid PhotoId) : IRequest<AnalyzePhotoResult>;

public sealed class AnalyzePhotoCommandValidator : AbstractValidator<AnalyzePhotoCommand>
{
    public AnalyzePhotoCommandValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty();
    }
}

public sealed class AnalyzePhotoCommandHandler : IRequestHandler<AnalyzePhotoCommand, AnalyzePhotoResult>
{
    private static readonly TimeSpan ImageUrlExpiry = TimeSpan.FromMinutes(10);

    private readonly IApplicationDbContext _db;
    private readonly IVisionAnalysisService _visionAnalysis;
    private readonly IFileStorageService _fileStorage;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AnalyzePhotoCommandHandler(
        IApplicationDbContext db,
        IVisionAnalysisService visionAnalysis,
        IFileStorageService fileStorage,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _visionAnalysis = visionAnalysis;
        _fileStorage = fileStorage;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AnalyzePhotoResult> Handle(AnalyzePhotoCommand request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos
            .Include(p => p.FilmRoll)
            .Include(p => p.CameraBody)
            .Include(p => p.Lens)
            .Include(p => p.Critique)
            .Include(p => p.PhotoTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.PhotoId);

        var storageKey = photo.PreviewStorageKey ?? photo.OriginalStorageKey;
        var imageUrl = await _fileStorage.GetPresignedDownloadUrlAsync(storageKey, ImageUrlExpiry, cancellationToken);

        var context = new VisionAnalysisContext(
            CameraModel: photo.CameraBody is null ? null : $"{photo.CameraBody.Brand} {photo.CameraBody.Model}",
            LensModel: photo.Lens is null ? null : $"{photo.Lens.Brand} {photo.Lens.Model}",
            FilmStock: $"{photo.FilmRoll.Brand} {photo.FilmRoll.Name}",
            IsoUsed: photo.Exif.IsoUsed ?? photo.FilmRoll.ExposedAtIso ?? photo.FilmRoll.NominalIso,
            FlashFired: photo.Exif.FlashFired ?? (photo.FlashId is not null));

        var critiqueResult = await _visionAnalysis.AnalyzePhotoAsync(imageUrl, context, cancellationToken);

        if (photo.Critique is null)
        {
            photo.Critique = new PhotoAiCritique { PhotoId = photo.Id };
            _db.PhotoAiCritiques.Add(photo.Critique);
        }

        photo.Critique.Model = critiqueResult.Model;
        photo.Critique.CompositionScore = critiqueResult.CompositionScore;
        photo.Critique.CompositionNotes = critiqueResult.CompositionNotes;
        photo.Critique.LightingNotes = critiqueResult.LightingNotes;
        photo.Critique.PosingNotes = critiqueResult.PosingNotes;
        photo.Critique.RecommendationsJson = System.Text.Json.JsonSerializer.Serialize(critiqueResult.Recommendations);
        photo.Critique.SuggestedTagsJson = System.Text.Json.JsonSerializer.Serialize(critiqueResult.SuggestedTags);
        photo.Critique.RawResponseJson = critiqueResult.RawResponseJson;

        var appliedTags = await AttachSuggestedTagsAsync(photo, critiqueResult.SuggestedTags, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        var critiqueDto = new PhotoCritiqueDto(
            photo.Id,
            critiqueResult.Model,
            critiqueResult.CompositionScore,
            critiqueResult.CompositionNotes,
            critiqueResult.LightingNotes,
            critiqueResult.PosingNotes,
            critiqueResult.Recommendations,
            critiqueResult.SuggestedTags,
            _dateTimeProvider.UtcNow);

        return new AnalyzePhotoResult(critiqueDto, appliedTags);
    }

    private async Task<IReadOnlyList<string>> AttachSuggestedTagsAsync(
        Photo photo,
        IReadOnlyList<string> suggestedTagNames,
        CancellationToken cancellationToken)
    {
        var applied = new List<string>();

        foreach (var tagName in suggestedTagNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var slug = Slugify.Generate(tagName);
            if (string.IsNullOrEmpty(slug))
            {
                continue;
            }

            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
            if (tag is null)
            {
                tag = new Tag { Name = tagName, Slug = slug };
                _db.Tags.Add(tag);
            }

            var alreadyLinked = photo.PhotoTags.Any(pt => pt.TagId == tag.Id || pt.Tag.Slug == slug);
            if (!alreadyLinked)
            {
                photo.PhotoTags.Add(new PhotoTag { Photo = photo, Tag = tag, IsAiSuggested = true });
            }

            applied.Add(tag.Name);
        }

        return applied;
    }
}
