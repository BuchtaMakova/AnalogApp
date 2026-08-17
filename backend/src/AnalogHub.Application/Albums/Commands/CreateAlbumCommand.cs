using AnalogHub.Application.Albums.Dtos;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AnalogHub.Application.Albums.Commands;

public sealed record CreateAlbumCommand(string Name, string? Description) : IRequest<AlbumDto>;

public sealed class CreateAlbumCommandValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class CreateAlbumCommandHandler : IRequestHandler<CreateAlbumCommand, AlbumDto>
{
    private readonly IApplicationDbContext _db;

    public CreateAlbumCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AlbumDto> Handle(CreateAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = new Album { Name = request.Name, Description = request.Description };

        _db.Albums.Add(album);
        await _db.SaveChangesAsync(cancellationToken);

        return new AlbumDto(album.Id, album.Name, album.Description, null, null, 0, album.CreatedAtUtc);
    }
}
