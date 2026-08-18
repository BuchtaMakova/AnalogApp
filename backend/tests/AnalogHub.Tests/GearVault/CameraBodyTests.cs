using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.GearVault.Commands;
using AnalogHub.Application.GearVault.Queries;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AnalogHub.Tests.GearVault;

public sealed class CameraBodyTests
{
    [Fact]
    public async Task Create_ValidRequest_PersistsCameraBodyWithSupportedFormats()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var handler = new CreateCameraBodyCommandHandler(db, currentUser);

        var command = new CreateCameraBodyCommand(
            "Leica M6", "Leica", "M6", "12345", "Leica M", "Rangefinder", new DateOnly(2020, 1, 1),
            [FilmFormat.ThirtyFiveMm]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Leica M6");
        result.SupportedFormats.Should().ContainSingle().Which.Should().Be(FilmFormat.ThirtyFiveMm);
        result.IsActive.Should().BeTrue();

        var saved = await db.CameraBodies.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(currentUser.UserId);
    }

    [Fact]
    public async Task Update_ExistingCameraBody_OverwritesAllFieldsAndFormats()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var cameraBody = new CameraBody
        {
            UserId = currentUser.UserId,
            Name = "Nikon F100", Brand = "Nikon", Model = "F100",
            SupportedFormats = [FilmFormat.ThirtyFiveMm]
        };
        db.CameraBodies.Add(cameraBody);
        await db.SaveChangesAsync();

        var handler = new UpdateCameraBodyCommandHandler(db, currentUser);
        var command = new UpdateCameraBodyCommand(
            cameraBody.Id, "Nikon F100 (retired)", "Nikon", "F100", "SN-1", "Nikon F", "Retired",
            IsActive: false, AcquiredOn: null, [FilmFormat.ThirtyFiveMm, FilmFormat.OneTwentyMm]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Nikon F100 (retired)");
        result.IsActive.Should().BeFalse();
        result.SupportedFormats.Should().BeEquivalentTo([FilmFormat.ThirtyFiveMm, FilmFormat.OneTwentyMm]);
    }

    [Fact]
    public async Task Update_CameraBodyDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new UpdateCameraBodyCommandHandler(db, new FakeCurrentUserService());
        var command = new UpdateCameraBodyCommand(Guid.NewGuid(), "X", "X", "X", null, null, null, true, null, []);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_CameraBodyOwnedByAnotherUser_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var owner = new FakeCurrentUserService();
        var cameraBody = new CameraBody { UserId = owner.UserId, Name = "Nikon F100", Brand = "Nikon", Model = "F100" };
        db.CameraBodies.Add(cameraBody);
        await db.SaveChangesAsync();

        var otherUser = new FakeCurrentUserService();
        var handler = new UpdateCameraBodyCommandHandler(db, otherUser);
        var command = new UpdateCameraBodyCommand(cameraBody.Id, "Hijacked", "X", "X", null, null, null, true, null, []);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>("a camera body owned by another user must be invisible, not just unwritable");
    }

    [Fact]
    public async Task Delete_ExistingCameraBody_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var cameraBody = new CameraBody { UserId = currentUser.UserId, Name = "Pentax 645N", Brand = "Pentax", Model = "645N" };
        db.CameraBodies.Add(cameraBody);
        await db.SaveChangesAsync();

        var handler = new DeleteCameraBodyCommandHandler(db, currentUser);
        await handler.Handle(new DeleteCameraBodyCommand(cameraBody.Id), CancellationToken.None);

        (await db.CameraBodies.FindAsync(cameraBody.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_CameraBodyDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteCameraBodyCommandHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeleteCameraBodyCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_CameraBodyOwnedByAnotherUser_ThrowsNotFoundExceptionAndLeavesItIntact()
    {
        using var db = TestDbContextFactory.Create();
        var owner = new FakeCurrentUserService();
        var cameraBody = new CameraBody { UserId = owner.UserId, Name = "Pentax 645N", Brand = "Pentax", Model = "645N" };
        db.CameraBodies.Add(cameraBody);
        await db.SaveChangesAsync();

        var otherUser = new FakeCurrentUserService();
        var handler = new DeleteCameraBodyCommandHandler(db, otherUser);

        var act = () => handler.Handle(new DeleteCameraBodyCommand(cameraBody.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        (await db.CameraBodies.FindAsync(cameraBody.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetCameraBodies_DefaultsToActiveOnly()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        db.CameraBodies.AddRange(
            new CameraBody { UserId = currentUser.UserId, Name = "Active", Brand = "A", Model = "1", IsActive = true },
            new CameraBody { UserId = currentUser.UserId, Name = "Inactive", Brand = "B", Model = "2", IsActive = false });
        await db.SaveChangesAsync();

        var handler = new GetCameraBodiesQueryHandler(db, currentUser);

        var activeOnly = await handler.Handle(new GetCameraBodiesQuery(IncludeInactive: false), CancellationToken.None);
        var all = await handler.Handle(new GetCameraBodiesQuery(IncludeInactive: true), CancellationToken.None);

        activeOnly.Should().ContainSingle().Which.Name.Should().Be("Active");
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCameraBodies_OnlyReturnsTheCurrentUsersOwnGear()
    {
        using var db = TestDbContextFactory.Create();
        var userA = new FakeCurrentUserService();
        var userB = new FakeCurrentUserService();
        db.CameraBodies.AddRange(
            new CameraBody { UserId = userA.UserId, Name = "Mine", Brand = "A", Model = "1" },
            new CameraBody { UserId = userB.UserId, Name = "Not mine", Brand = "B", Model = "2" });
        await db.SaveChangesAsync();

        var handler = new GetCameraBodiesQueryHandler(db, userA);

        var result = await handler.Handle(new GetCameraBodiesQuery(IncludeInactive: true), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("Mine");
    }

    [Fact]
    public async Task GetCameraBodyById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetCameraBodyByIdQueryHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new GetCameraBodyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCameraBodyById_OwnedByAnotherUser_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var owner = new FakeCurrentUserService();
        var cameraBody = new CameraBody { UserId = owner.UserId, Name = "Leica M6", Brand = "Leica", Model = "M6" };
        db.CameraBodies.Add(cameraBody);
        await db.SaveChangesAsync();

        var handler = new GetCameraBodyByIdQueryHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new GetCameraBodyByIdQuery(cameraBody.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
