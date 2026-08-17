using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.FilmRolls.Dtos;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.FilmRolls.Queries;

public sealed record GetFilmRollsQuery(FilmRollStatus? Status = null) : IRequest<IReadOnlyList<FilmRollDto>>;

public sealed class GetFilmRollsQueryHandler : IRequestHandler<GetFilmRollsQuery, IReadOnlyList<FilmRollDto>>
{
    private readonly IApplicationDbContext _db;

    public GetFilmRollsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FilmRollDto>> Handle(GetFilmRollsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.FilmRolls.AsNoTracking().Include(r => r.CameraBody).AsQueryable();
        if (request.Status is { } status)
        {
            query = query.Where(r => r.Status == status);
        }

        var rolls = await query.OrderByDescending(r => r.DateLoaded).ToListAsync(cancellationToken);
        return rolls.Select(r => FilmRollMappings.ToDto(r)).ToList();
    }
}

public sealed record GetFilmRollByIdQuery(Guid Id) : IRequest<FilmRollDto>;

public sealed class GetFilmRollByIdQueryHandler : IRequestHandler<GetFilmRollByIdQuery, FilmRollDto>
{
    private readonly IApplicationDbContext _db;

    public GetFilmRollByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<FilmRollDto> Handle(GetFilmRollByIdQuery request, CancellationToken cancellationToken)
    {
        var filmRoll = await _db.FilmRolls.AsNoTracking().Include(r => r.CameraBody)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FilmRoll), request.Id);

        return FilmRollMappings.ToDto(filmRoll);
    }
}
