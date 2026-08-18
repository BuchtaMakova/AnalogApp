using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Domain.Entities.Gear;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.GearVault.Queries;

public sealed record GetCameraBodiesQuery(bool IncludeInactive = false) : IRequest<IReadOnlyList<CameraBodyDto>>;

public sealed class GetCameraBodiesQueryHandler : IRequestHandler<GetCameraBodiesQuery, IReadOnlyList<CameraBodyDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCameraBodiesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CameraBodyDto>> Handle(GetCameraBodiesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.CameraBodies.AsNoTracking().Where(c => c.UserId == _currentUser.UserId);
        if (!request.IncludeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var bodies = await query.OrderBy(c => c.Brand).ThenBy(c => c.Model).ToListAsync(cancellationToken);
        return bodies.Select(GearMappings.ToDto).ToList();
    }
}

public sealed record GetCameraBodyByIdQuery(Guid Id) : IRequest<CameraBodyDto>;

public sealed class GetCameraBodyByIdQueryHandler : IRequestHandler<GetCameraBodyByIdQuery, CameraBodyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCameraBodyByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CameraBodyDto> Handle(GetCameraBodyByIdQuery request, CancellationToken cancellationToken)
    {
        var cameraBody = await _db.CameraBodies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(CameraBody), request.Id);

        return GearMappings.ToDto(cameraBody);
    }
}
