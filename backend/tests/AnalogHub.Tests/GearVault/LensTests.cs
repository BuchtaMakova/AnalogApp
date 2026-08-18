using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.GearVault.Commands;
using AnalogHub.Application.GearVault.Queries;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AnalogHub.Tests.GearVault;

public sealed class LensTests
{
    [Fact]
    public async Task Create_ValidRequest_PersistsLensWithOpticalSpecs()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var handler = new CreateLensCommandHandler(db, currentUser);

        var command = new CreateLensCommand(
            "Summicron 50mm", "Leica", "Summicron-M 50mm f/2", null, "Leica M", null, null,
            FocalLengthMinMm: 50m, FocalLengthMaxMm: 50m, MaxAperture: 2.0m, MinAperture: 16.0m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.FocalLengthMinMm.Should().Be(50m);
        result.MaxAperture.Should().Be(2.0m);
        var saved = await db.Lenses.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(currentUser.UserId);
    }

    [Fact]
    public async Task Update_ExistingLens_OverwritesOpticalSpecs()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var lens = new Lens
        {
            UserId = currentUser.UserId,
            Name = "Kit zoom", Brand = "Nikon", Model = "18-55mm", FocalLengthMinMm = 18m, FocalLengthMaxMm = 55m
        };
        db.Lenses.Add(lens);
        await db.SaveChangesAsync();

        var handler = new UpdateLensCommandHandler(db, currentUser);
        var command = new UpdateLensCommand(
            lens.Id, "Kit zoom", "Nikon", "18-55mm", null, null, null, IsActive: true, AcquiredOn: null,
            FocalLengthMinMm: 18m, FocalLengthMaxMm: 55m, MaxAperture: 3.5m, MinAperture: 22m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.MaxAperture.Should().Be(3.5m);
    }

    [Fact]
    public async Task Update_LensDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new UpdateLensCommandHandler(db, new FakeCurrentUserService());
        var command = new UpdateLensCommand(Guid.NewGuid(), "X", "X", "X", null, null, null, true, null, null, null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingLens_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var lens = new Lens { UserId = currentUser.UserId, Name = "50mm", Brand = "Nikon", Model = "50mm f/1.8D" };
        db.Lenses.Add(lens);
        await db.SaveChangesAsync();

        var handler = new DeleteLensCommandHandler(db, currentUser);
        await handler.Handle(new DeleteLensCommand(lens.Id), CancellationToken.None);

        (await db.Lenses.FindAsync(lens.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_LensDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteLensCommandHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeleteLensCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_LensOwnedByAnotherUser_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var owner = new FakeCurrentUserService();
        var lens = new Lens { UserId = owner.UserId, Name = "50mm", Brand = "Nikon", Model = "50mm f/1.8D" };
        db.Lenses.Add(lens);
        await db.SaveChangesAsync();

        var handler = new DeleteLensCommandHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new DeleteLensCommand(lens.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        (await db.Lenses.FindAsync(lens.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetLenses_OrdersByBrandThenModel()
    {
        using var db = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        db.Lenses.AddRange(
            new Lens { UserId = currentUser.UserId, Name = "B-lens", Brand = "Zeiss", Model = "50mm", IsActive = true },
            new Lens { UserId = currentUser.UserId, Name = "A-lens", Brand = "Canon", Model = "50mm", IsActive = true });
        await db.SaveChangesAsync();

        var handler = new GetLensesQueryHandler(db, currentUser);
        var result = await handler.Handle(new GetLensesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Brand.Should().Be("Canon");
    }

    [Fact]
    public async Task GetLenses_OnlyReturnsTheCurrentUsersOwnLenses()
    {
        using var db = TestDbContextFactory.Create();
        var userA = new FakeCurrentUserService();
        var userB = new FakeCurrentUserService();
        db.Lenses.AddRange(
            new Lens { UserId = userA.UserId, Name = "Mine", Brand = "A", Model = "1" },
            new Lens { UserId = userB.UserId, Name = "Not mine", Brand = "B", Model = "2" });
        await db.SaveChangesAsync();

        var handler = new GetLensesQueryHandler(db, userA);
        var result = await handler.Handle(new GetLensesQuery(IncludeInactive: true), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("Mine");
    }

    [Fact]
    public async Task GetLensById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetLensByIdQueryHandler(db, new FakeCurrentUserService());

        var act = () => handler.Handle(new GetLensByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
