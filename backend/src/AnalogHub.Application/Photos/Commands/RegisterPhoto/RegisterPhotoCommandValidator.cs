using FluentValidation;

namespace AnalogHub.Application.Photos.Commands.RegisterPhoto;

public sealed class RegisterPhotoCommandValidator : AbstractValidator<RegisterPhotoCommand>
{
    public RegisterPhotoCommandValidator()
    {
        RuleFor(x => x.FilmRollId).NotEmpty();

        RuleFor(x => x.StorageKey)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(PhotoContentTypes.Allowed.Contains)
            .WithMessage($"ContentType must be one of: {string.Join(", ", PhotoContentTypes.Allowed)}.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(PhotoContentTypes.MaxFileSizeBytes);

        RuleFor(x => x.FrameNumber)
            .GreaterThan(0)
            .When(x => x.FrameNumber.HasValue);

        RuleFor(x => x.Exif!.IsoUsed)
            .GreaterThan(0)
            .When(x => x.Exif?.IsoUsed is not null);
    }
}
