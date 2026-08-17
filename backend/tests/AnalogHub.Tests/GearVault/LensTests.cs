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
        var handler = new CreateLensCommandHandler(db);

        var command = new CreateLensCommand(
            "Summicron 50mm", "Leica", "Summicron-M 50mm f/2", null, "Leica M", null, null,
            FocalLengthMinMm: 50m, FocalLengthMaxMm: 50m, MaxAperture: 2.0m, MinAperture: 16.0m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.FocalLengthMinMm.Should().Be(50m);
        result.MaxAperture.Should().Be(2.0m);
        (await db.Lenses.FindAsync(result.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ExistingLens_OverwritesOpticalSpecs()
    {
        using var db = TestDbContextFactory.Create();
        var lens = new Lens { Name = "Kit zoom", Brand = "Nikon", Model = "18-55mm", FocalLengthMinMm = 18m, FocalLengthMaxMm = 55m };
        db.Lenses.Add(lens);
        await db.SaveChangesAsync();

        var handler = new UpdateLensCommandHandler(db);
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
        var handler = new UpdateLensCommandHandler(db);
        var command = new UpdateLensCommand(Guid.NewGuid(), "X", "X", "X", null, null, null, true, null, null, null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingLens_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var lens = new Lens { Name = "50mm", Brand = "Nikon", Model = "50mm f/1.8D" };
        db.Lenses.Add(lens);
        await db.SaveChangesAsync();

        var handler = new DeleteLensCommandHandler(db);
        await handler.Handle(new DeleteLensCommand(lens.Id), CancellationToken.None);

        (await db.Lenses.FindAsync(lens.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_LensDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteLensCommandHandler(db);

        var act = () => handler.Handle(new DeleteLensCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetLenses_OrdersByBrandThenModel()
    {
        using var db = TestDbContextFactory.Create();
        db.Lenses.AddRange(
            new Lens { Name = "B-lens", Brand = "Zeiss", Model = "50mm", IsActive = true },
            new Lens { Name = "A-lens", Brand = "Canon", Model = "50mm", IsActive = true });
        await db.SaveChangesAsync();

        var handler = new GetLensesQueryHandler(db);
        var result = await handler.Handle(new GetLensesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Brand.Should().Be("Canon");
    }

    [Fact]
    public async Task GetLensById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetLensByIdQueryHandler(db);

        var act = () => handler.Handle(new GetLensByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
