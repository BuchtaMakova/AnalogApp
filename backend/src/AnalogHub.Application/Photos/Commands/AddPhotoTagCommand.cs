using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Common.Utils;
using AnalogHub.Application.Tags.Dtos;
using AnalogHub.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Photos.Commands;

/// <summary>Manually attaches a tag to a photo (find-or-create by slug); idempotent if already linked.</summary>
public sealed record AddPhotoTagCommand(Guid PhotoId, string TagName) : IRequest<PhotoTagDto>;

public sealed class AddPhotoTagCommandValidator : AbstractValidator<AddPhotoTagCommand>
{
    public AddPhotoTagCommandValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty();
        RuleFor(x => x.TagName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddPhotoTagCommandHandler : IRequestHandler<AddPhotoTagCommand, PhotoTagDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddPhotoTagCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PhotoTagDto> Handle(AddPhotoTagCommand request, CancellationToken cancellationToken)
    {
        var photo = await _db.Photos.Include(p => p.PhotoTags)
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId && p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Photo), request.PhotoId);

        var slug = Slugify.Generate(request.TagName);

        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        if (tag is null)
        {
            tag = new Tag { Name = request.TagName, Slug = slug };
            _db.Tags.Add(tag);
        }

        var existingLink = photo.PhotoTags.FirstOrDefault(pt => pt.TagId == tag.Id);
        if (existingLink is null)
        {
            photo.PhotoTags.Add(new PhotoTag { PhotoId = photo.Id, TagId = tag.Id, IsAiSuggested = false });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new PhotoTagDto(tag.Id, tag.Name, tag.Slug, existingLink?.IsAiSuggested ?? false);
    }
}
