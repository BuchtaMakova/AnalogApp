using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Queries;

public sealed record GetLensesQuery(bool IncludeInactive = false) : IRequest<IReadOnlyList<LensDto>>;

public sealed class GetLensesQueryHandler : IRequestHandler<GetLensesQuery, IReadOnlyList<LensDto>>
{
    private readonly IApplicationDbContext _db;

    public GetLensesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<LensDto>> Handle(GetLensesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Lenses.AsNoTracking();
        if (!request.IncludeInactive)
        {
            query = query.Where(l => l.IsActive);
        }

        var lenses = await query.OrderBy(l => l.Brand).ThenBy(l => l.Model).ToListAsync(cancellationToken);
        return lenses.Select(GearMappings.ToDto).ToList();
    }
}

public sealed record GetLensByIdQuery(Guid Id) : IRequest<LensDto>;

public sealed class GetLensByIdQueryHandler : IRequestHandler<GetLensByIdQuery, LensDto>
{
    private readonly IApplicationDbContext _db;

    public GetLensByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<LensDto> Handle(GetLensByIdQuery request, CancellationToken cancellationToken)
    {
        var lens = await _db.Lenses.AsNoTracking().FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Lens), request.Id);

        return GearMappings.ToDto(lens);
    }
}
