using AnalogHub.Application.Auth.Dtos;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnalogHub.Application.Auth.Commands;

public sealed record RegisterCommand(string Email, string Password) : IRequest<AuthResultDto>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDemoDataSeeder _demoDataSeeder;

    public RegisterCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IDemoDataSeeder demoDataSeeder)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _demoDataSeeder = demoDataSeeder;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var alreadyExists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (alreadyExists)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "This email is already registered.")
            });
        }

        // Bootstrap convenience for a single-tenant portfolio deployment: the very first account
        // becomes Admin so there's no separate seeding step to get an initial admin user.
        var isFirstUser = !await _db.Users.AnyAsync(cancellationToken);

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = isFirstUser ? UserRole.Admin : UserRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        if (isFirstUser)
        {
            await _demoDataSeeder.SeedForFirstUserAsync(user.Id, cancellationToken);
        }

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResultDto(token, expiresAtUtc, user.Id, user.Email, user.Role.ToString());
    }
}
