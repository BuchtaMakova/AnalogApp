using AnalogHub.Domain.Common;
using AnalogHub.Domain.Enums;

namespace AnalogHub.Domain.Entities;

public sealed class User : AuditableEntity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.User;
}
