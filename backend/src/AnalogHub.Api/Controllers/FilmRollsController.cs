using AnalogHub.Application.FilmRolls.Commands;
using AnalogHub.Application.FilmRolls.Dtos;
using AnalogHub.Application.FilmRolls.Queries;
using AnalogHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/film-rolls")]
public sealed class FilmRollsController : ControllerBase
{
    private readonly ISender _sender;

    public FilmRollsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FilmRollDto>>> GetFilmRolls(
        [FromQuery] FilmRollStatus? status, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetFilmRollsQuery(status), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FilmRollDto>> GetFilmRoll(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetFilmRollByIdQuery(id), cancellationToken));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<FilmRollDto>> CreateFilmRoll(
        [FromBody] CreateFilmRollCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetFilmRoll), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FilmRollDto>> UpdateFilmRoll(
        Guid id, [FromBody] UpdateFilmRollCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFilmRoll(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFilmRollCommand(id), cancellationToken);
        return NoContent();
    }
}
