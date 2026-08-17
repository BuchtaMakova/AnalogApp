using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Tags.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Tags.Queries;

public sealed record GetTagsQuery : IRequest<IReadOnlyList<TagDto>>;

public sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, IReadOnlyList<TagDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTagsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Slug))
            .ToListAsync(cancellationToken);
    }
}
