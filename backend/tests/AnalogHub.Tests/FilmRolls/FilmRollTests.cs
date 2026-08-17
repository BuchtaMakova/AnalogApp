using AnalogHub.Application.Common.Exceptions;
using AnalogHub.Application.FilmRolls.Commands;
using AnalogHub.Application.FilmRolls.Queries;
using AnalogHub.Domain.Entities;
using AnalogHub.Domain.Entities.Gear;
using AnalogHub.Domain.Enums;
using AnalogHub.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AnalogHub.Tests.FilmRolls;

public sealed class FilmRollTests
{
    [Fact]
    public async Task Create_ValidRequest_PersistsFilmRollInLoadedStatus()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateFilmRollCommandHandler(db);

        var command = new CreateFilmRollCommand(
            "Kodak Portra 400", "Kodak", FilmFormat.ThirtyFiveMm, 400, null, 36, null, null, "Street shoot");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(FilmRollStatus.Loaded);
        result.NominalIso.Should().Be(400);
        (await db.FilmRolls.FindAsync(result.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithCameraBody_LinksRollToCameraAndReturnsItsName()
    {
        using var db = TestDbContextFactory.Create();
        var cameraBody = new CameraBody { Name = "Leica M6", Brand = "Leica", Model = "M6" };
        db.CameraBodies.Add(cameraBody);
        await db.SaveChangesAsync();

        var handler = new CreateFilmRollCommandHandler(db);
        var command = new CreateFilmRollCommand(
            "Ektar 100", "Kodak", FilmFormat.ThirtyFiveMm, 100, null, 36, cameraBody.Id, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.CameraBodyId.Should().Be(cameraBody.Id);
        result.CameraBodyName.Should().Be("Leica M6");
    }

    [Fact]
    public async Task Create_CameraBodyDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateFilmRollCommandHandler(db);
        var command = new CreateFilmRollCommand(
            "Roll", "Kodak", FilmFormat.ThirtyFiveMm, 400, null, 36, Guid.NewGuid(), null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ExistingFilmRoll_TransitionsStatusAndSetsLabInfo()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll
        {
            Name = "HP5 Plus", Brand = "Ilford", Format = FilmFormat.ThirtyFiveMm,
            NominalIso = 400, FrameCount = 36, Status = FilmRollStatus.ShotCompleted
        };
        db.FilmRolls.Add(filmRoll);
        await db.SaveChangesAsync();

        var handler = new UpdateFilmRollCommandHandler(db);
        var command = new UpdateFilmRollCommand(
            filmRoll.Id, "HP5 Plus", "Ilford", FilmFormat.ThirtyFiveMm, 400, 800, 36,
            FilmRollStatus.Developed, null, null, null, new DateOnly(2026, 8, 1),
            "The Darkroom Lab", "Pushed one stop", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(FilmRollStatus.Developed);
        result.ExposedAtIso.Should().Be(800);
        result.LabName.Should().Be("The Darkroom Lab");
    }

    [Fact]
    public async Task Update_FilmRollDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new UpdateFilmRollCommandHandler(db);
        var command = new UpdateFilmRollCommand(
            Guid.NewGuid(), "X", "X", FilmFormat.ThirtyFiveMm, 400, null, 36,
            FilmRollStatus.Loaded, null, null, null, null, null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingFilmRoll_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var filmRoll = new FilmRoll { Name = "Roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36 };
        db.FilmRolls.Add(filmRoll);
        await db.SaveChangesAsync();

        var handler = new DeleteFilmRollCommandHandler(db);
        await handler.Handle(new DeleteFilmRollCommand(filmRoll.Id), CancellationToken.None);

        (await db.FilmRolls.FindAsync(filmRoll.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_FilmRollDoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteFilmRollCommandHandler(db);

        var act = () => handler.Handle(new DeleteFilmRollCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetFilmRolls_FiltersByStatusWhenProvided()
    {
        using var db = TestDbContextFactory.Create();
        db.FilmRolls.AddRange(
            new FilmRoll { Name = "Loaded roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36, Status = FilmRollStatus.Loaded },
            new FilmRoll { Name = "Archived roll", Brand = "Kodak", Format = FilmFormat.ThirtyFiveMm, NominalIso = 400, FrameCount = 36, Status = FilmRollStatus.Archived });
        await db.SaveChangesAsync();

        var handler = new GetFilmRollsQueryHandler(db);
        var loadedOnly = await handler.Handle(new GetFilmRollsQuery(FilmRollStatus.Loaded), CancellationToken.None);
        var all = await handler.Handle(new GetFilmRollsQuery(), CancellationToken.None);

        loadedOnly.Should().ContainSingle().Which.Name.Should().Be("Loaded roll");
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilmRollById_DoesNotExist_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetFilmRollByIdQueryHandler(db);

        var act = () => handler.Handle(new GetFilmRollByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
