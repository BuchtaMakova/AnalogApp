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
        var handler = new CreateFlashCommandHandler(db);

        var command = new CreateFlashCommand("SB-28", "Nikon", "SB-28", null, null, null, null, GuideNumber: 36m, HasTtl: true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.GuideNumber.Should().Be(36m);
        result.HasTtl.Should().BeTrue();
        (await db.Flashes.FindAsync(result.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ExistingFlash_OverwritesGuideNumberAndTtl()
    {
        using var db = TestDbContextFactory.Create();
        var flash = new Flash { Name = "Old flash", Brand = "Generic", Model = "GF-1", GuideNumber = 20m, HasTtl = false };
        db.Flashes.Add(flash);
        await db.SaveChangesAsync();

        var handler = new UpdateFlashCommandHandler(db);
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
        var handler = new UpdateFlashCommandHandler(db);
        var command = new UpdateFlashCommand(Guid.NewGuid(), "X", "X", "X", null, null, null, true, null, null, false);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingFlash_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var flash = new Flash { Name = "V860III", Brand = "Godox", Model = "V860III" };
        db.Flashes.Add(flash);
        await db.SaveChangesAsync();

        var handler = new DeleteFlashCommandHandler(db);
        await handler.Handle(new DeleteFlashCommand(flash.Id), CancellationToken.None);

        (await db.Flashes.FindAsync(flash.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_FlashDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteFlashCommandHandler(db);

        var act = () => handler.Handle(new DeleteFlashCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetFlashes_ExcludesInactiveByDefault()
    {
        using var db = TestDbContextFactory.Create();
        db.Flashes.AddRange(
            new Flash { Name = "Active", Brand = "A", Model = "1", IsActive = true },
            new Flash { Name = "Retired", Brand = "B", Model = "2", IsActive = false });
        await db.SaveChangesAsync();

        var handler = new GetFlashesQueryHandler(db);
        var result = await handler.Handle(new GetFlashesQuery(), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetFlashById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetFlashByIdQueryHandler(db);

        var act = () => handler.Handle(new GetFlashByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
