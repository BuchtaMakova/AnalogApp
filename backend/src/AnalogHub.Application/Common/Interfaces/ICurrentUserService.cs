namespace AnalogHub.Application.Common.Interfaces;

/// <summary>
/// The authenticated user making the current request — every Gear/FilmRoll/Album/Photo command and
/// query is scoped to this id, since each user has their own private library, never a shared one.
/// Only ever called from within an authenticated request (every endpoint requires a token by
/// default), so a missing/unparseable id is a bug, not a normal condition — see the implementation.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
}
