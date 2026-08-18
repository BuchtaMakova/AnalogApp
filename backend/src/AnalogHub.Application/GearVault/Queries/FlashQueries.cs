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
    private readonly ICurrentUserService _currentUser;

    public GetFlashesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<FlashDto>> Handle(GetFlashesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Flashes.AsNoTracking().Where(f => f.UserId == _currentUser.UserId);
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
    private readonly ICurrentUserService _currentUser;

    public GetFlashByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<FlashDto> Handle(GetFlashByIdQuery request, CancellationToken cancellationToken)
    {
        var flash = await _db.Flashes.AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == request.Id && f.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Flash), request.Id);

        return GearMappings.ToDto(flash);
    }
}
