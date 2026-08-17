using AnalogHub.Application.Auth.Commands;
using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace AnalogHub.Tests.Auth;

public sealed class AuthTests
{
    private static (Mock<IPasswordHasher> Hasher, Mock<IJwtTokenGenerator> Jwt) CreateMocks()
    {
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenGenerator>();
        jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(("fake-jwt-token", DateTimeOffset.UtcNow.AddHours(2)));
        return (hasher, jwt);
    }

    [Fact]
    public async Task Register_FirstUser_BecomesAdmin()
    {
        using var db = TestDbContextFactory.Create();
        var (hasher, jwt) = CreateMocks();
        hasher.Setup(h => h.Hash("password123")).Returns("hashed-password123");
        var handler = new RegisterCommandHandler(db, hasher.Object, jwt.Object);

        var result = await handler.Handle(new RegisterCommand("First@Example.com", "password123"), CancellationToken.None);

        result.Role.Should().Be(nameof(UserRole.Admin));
        result.Token.Should().Be("fake-jwt-token");

        var user = await db.Users.FindAsync(result.UserId);
        user.Should().NotBeNull();
        user!.Email.Should().Be("first@example.com");
        user.PasswordHash.Should().Be("hashed-password123");
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Register_SecondUser_GetsUserRole()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Email = "existing@example.com", PasswordHash = "x", Role = UserRole.Admin });
        await db.SaveChangesAsync();

        var (hasher, jwt) = CreateMocks();
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        var handler = new RegisterCommandHandler(db, hasher.Object, jwt.Object);

        var result = await handler.Handle(new RegisterCommand("second@example.com", "password123"), CancellationToken.None);

        result.Role.Should().Be(nameof(UserRole.User));
    }

    [Fact]
    public async Task Register_EmailAlreadyRegistered_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Email = "taken@example.com", PasswordHash = "x", Role = UserRole.User });
        await db.SaveChangesAsync();

        var (hasher, jwt) = CreateMocks();
        var handler = new RegisterCommandHandler(db, hasher.Object, jwt.Object);

        var act = () => handler.Handle(new RegisterCommand("taken@example.com", "password123"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Email = "user@example.com", PasswordHash = "hashed-secret", Role = UserRole.User });
        await db.SaveChangesAsync();

        var (hasher, jwt) = CreateMocks();
        hasher.Setup(h => h.Verify("secret", "hashed-secret")).Returns(true);
        var handler = new LoginCommandHandler(db, hasher.Object, jwt.Object);

        var result = await handler.Handle(new LoginCommand("user@example.com", "secret"), CancellationToken.None);

        result.Token.Should().Be("fake-jwt-token");
        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsAuthenticationFailedException()
    {
        using var db = TestDbContextFactory.Create();
        var (hasher, jwt) = CreateMocks();
        var handler = new LoginCommandHandler(db, hasher.Object, jwt.Object);

        var act = () => handler.Handle(new LoginCommand("nobody@example.com", "secret"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsAuthenticationFailedException()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { Email = "user@example.com", PasswordHash = "hashed-secret", Role = UserRole.User });
        await db.SaveChangesAsync();

        var (hasher, jwt) = CreateMocks();
        hasher.Setup(h => h.Verify("wrong", "hashed-secret")).Returns(false);
        var handler = new LoginCommandHandler(db, hasher.Object, jwt.Object);

        var act = () => handler.Handle(new LoginCommand("user@example.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
