using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.GearVault.Commands;
using AnalogHub.Application.GearVault.Queries;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AnalogHub.Tests.GearVault;

public sealed class FlashTests
{
    [Fact]
    public async Task Create_ValidRequest_PersistsFlashWithGuideNumberAndTtl()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var handler = new CreateFlashCommandHandler(db, currentUser);

        var command = new CreateFlashCommand("SB-28", "Nikon", "SB-28", null, null, null, null, GuideNumber: 36m, HasTtl: true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.GuideNumber.Should().Be(36m);
        result.HasTtl.Should().BeTrue();
        var saved = await db.Flashes.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(currentUser.UserId);
    }

    [Fact]
    public async Task Update_ExistingFlash_OverwritesGuideNumberAndTtl()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var flash = new Flash
        {
            UserId = currentUser.UserId,
            Name = "Old flash", Brand = "Generic", Model = "GF-1", GuideNumber = 20m, HasTtl = false
        };
        db.Flashes.Add(flash);
        await db.SaveChangesAsync();

        var handler = new UpdateFlashCommandHandler(db, currentUser);
        var command = new UpdateFlashCommand(
            flash.Id, "Old flash", "Generic", "GF-1", null, null, null, IsActive: true, AcquiredOn: null,
            GuideNumber: 60m, HasTtl: true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.GuideNumber.Should().Be(60m);
        result.HasTtl.Should().BeTrue();
    }

    [Fact]
    public async Task Update_FlashDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new UpdateFlashCommandHandler(db, new FakeCurrentUserService());
        var command = new UpdateFlashCommand(Guid.NewGuid(), "X", "X", "X", null, null, null, true, null, null, false);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingFlash_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var flash = new Flash { UserId = currentUser.UserId, Name = "V860III", Brand = "Godox", Model = "V860III" };
        db.Flashes.Add(flash);
        await db.SaveChangesAsync();

        var handler = new DeleteFlashCommandHandler(db, currentUser);
        await handler.Handle(new DeleteFlashCommand(flash.Id), CancellationToken.None);

        (await db.Flashes.FindAsync(flash.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_FlashDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteFlashCommandHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeleteFlashCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_FlashOwnedByAnotherUser_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var owner = new FakeCurrentUserService();
        var flash = new Flash { UserId = owner.UserId, Name = "V860III", Brand = "Godox", Model = "V860III" };
        db.Flashes.Add(flash);
        await db.SaveChangesAsync();

        var handler = new DeleteFlashCommandHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeleteFlashCommand(flash.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        (await db.Flashes.FindAsync(flash.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetFlashes_ExcludesInactiveByDefault()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        db.Flashes.AddRange(
            new Flash { UserId = currentUser.UserId, Name = "Active", Brand = "A", Model = "1", IsActive = true },
            new Flash { UserId = currentUser.UserId, Name = "Retired", Brand = "B", Model = "2", IsActive = false });
        await db.SaveChangesAsync();

        var handler = new GetFlashesQueryHandler(db, currentUser);
        var result = await handler.Handle(new GetFlashesQuery(), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetFlashes_OnlyReturnsTheCurrentUsersOwnFlashes()
    {
        using var db = TestDbContextFactory.Create();
        var userA = new FakeCurrentUserService();
        var userB = new FakeCurrentUserService();
        db.Flashes.AddRange(
            new Flash { UserId = userA.UserId, Name = "Mine", Brand = "A", Model = "1" },
            new Flash { UserId = userB.UserId, Name = "Not mine", Brand = "B", Model = "2" });
        await db.SaveChangesAsync();

        var handler = new GetFlashesQueryHandler(db, userA);
        var result = await handler.Handle(new GetFlashesQuery(IncludeInactive: true), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("Mine");
    }

    [Fact]
    public async Task GetFlashById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetFlashByIdQueryHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new GetFlashByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
