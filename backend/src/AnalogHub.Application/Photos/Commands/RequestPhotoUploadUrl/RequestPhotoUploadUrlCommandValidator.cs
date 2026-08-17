using FluentValidation;

namespace AnalogHub.Application.Photos.Commands.RequestPhotoUploadUrl;

public sealed class RequestPhotoUploadUrlCommandValidator : AbstractValidator<RequestPhotoUploadUrlCommand>
{
    public RequestPhotoUploadUrlCommandValidator()
    {
        RuleFor(x => x.FilmRollId).NotEmpty();

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(PhotoContentTypes.Allowed.Contains)
            .WithMessage($"ContentType must be one of: {string.Join(", ", PhotoContentTypes.Allowed)}.");
    }
}
