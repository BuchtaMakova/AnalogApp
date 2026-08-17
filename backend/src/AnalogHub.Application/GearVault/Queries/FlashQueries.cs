using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Queries;

public sealed record GetFlashesQuery(bool IncludeInactive = false) : IRequest<IReadOnlyList<FlashDto>>;

public sealed class GetFlashesQueryHandler : IRequestHandler<GetFlashesQuery, IReadOnlyList<FlashDto>>
{
    private readonly IApplicationDbContext _db;

    public GetFlashesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FlashDto>> Handle(GetFlashesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Flashes.AsNoTracking();
        if (!request.IncludeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        var flashes = await query.OrderBy(f => f.Brand).ThenBy(f => f.Model).ToListAsync(cancellationToken);
        return flashes.Select(GearMappings.ToDto).ToList();
    }
}

public sealed record GetFlashByIdQuery(Guid Id) : IRequest<FlashDto>;

public sealed class GetFlashByIdQueryHandler : IRequestHandler<GetFlashByIdQuery, FlashDto>
{
    private readonly IApplicationDbContext _db;

    public GetFlashByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<FlashDto> Handle(GetFlashByIdQuery request, CancellationToken cancellationToken)
    {
        var flash = await _db.Flashes.AsNoTracking().FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Flash), request.Id);

        return GearMappings.ToDto(flash);
    }
}
