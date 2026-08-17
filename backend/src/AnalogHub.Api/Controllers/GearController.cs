using AnalogHub.Application.GearVault.Commands;
using AnalogHub.Application.GearVault.Dtos;
using AnalogHub.Application.GearVault.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/gear")]
public sealed class GearController : ControllerBase
{
    private readonly ISender _sender;

    public GearController(ISender sender)
    {
        _sender = sender;
    }

    // ---- Camera bodies ----

    [HttpGet("camera-bodies")]
    public async Task<ActionResult<IReadOnlyList<CameraBodyDto>>> GetCameraBodies(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCameraBodiesQuery(includeInactive), cancellationToken));

    [HttpGet("camera-bodies/{id:guid}")]
    public async Task<ActionResult<CameraBodyDto>> GetCameraBody(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCameraBodyByIdQuery(id), cancellationToken));

    [HttpPost("camera-bodies")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<CameraBodyDto>> CreateCameraBody(
        [FromBody] CreateCameraBodyCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCameraBody), new { id = result.Id }, result);
    }

    [HttpPut("camera-bodies/{id:guid}")]
    public async Task<ActionResult<CameraBodyDto>> UpdateCameraBody(
        Guid id, [FromBody] UpdateCameraBodyCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("camera-bodies/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCameraBody(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCameraBodyCommand(id), cancellationToken);
        return NoContent();
    }

    // ---- Lenses ----

    [HttpGet("lenses")]
    public async Task<ActionResult<IReadOnlyList<LensDto>>> GetLenses(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetLensesQuery(includeInactive), cancellationToken));

    [HttpGet("lenses/{id:guid}")]
    public async Task<ActionResult<LensDto>> GetLens(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetLensByIdQuery(id), cancellationToken));

    [HttpPost("lenses")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<LensDto>> CreateLens(
        [FromBody] CreateLensCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetLens), new { id = result.Id }, result);
    }

    [HttpPut("lenses/{id:guid}")]
    public async Task<ActionResult<LensDto>> UpdateLens(
        Guid id, [FromBody] UpdateLensCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("lenses/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteLens(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteLensCommand(id), cancellationToken);
        return NoContent();
    }

    // ---- Flashes ----

    [HttpGet("flashes")]
    public async Task<ActionResult<IReadOnlyList<FlashDto>>> GetFlashes(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetFlashesQuery(includeInactive), cancellationToken));

    [HttpGet("flashes/{id:guid}")]
    public async Task<ActionResult<FlashDto>> GetFlash(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetFlashByIdQuery(id), cancellationToken));

    [HttpPost("flashes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<FlashDto>> CreateFlash(
        [FromBody] CreateFlashCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetFlash), new { id = result.Id }, result);
    }

    [HttpPut("flashes/{id:guid}")]
    public async Task<ActionResult<FlashDto>> UpdateFlash(
        Guid id, [FromBody] UpdateFlashCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("flashes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFlash(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFlashCommand(id), cancellationToken);
        return NoContent();
    }
}
