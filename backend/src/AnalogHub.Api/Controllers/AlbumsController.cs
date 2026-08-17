using AnalogHub.Application.Albums.Commands;
using AnalogHub.Application.Albums.Dtos;
using AnalogHub.Application.Albums.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/albums")]
public sealed class AlbumsController : ControllerBase
{
    private readonly ISender _sender;

    public AlbumsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AlbumDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlbumDto>>> GetAlbums(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAlbumsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlbumDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlbumDetailDto>> GetAlbum(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetAlbumByIdQuery(id), cancellationToken));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AlbumDto>> CreateAlbum([FromBody] CreateAlbumCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAlbum), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AlbumDto>> UpdateAlbum(Guid id, [FromBody] UpdateAlbumCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id does not match body id.");
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAlbum(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAlbumCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/photos/{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddPhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new AddPhotoToAlbumCommand(id, photoId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemovePhotoFromAlbumCommand(id, photoId), cancellationToken);
        return NoContent();
    }
}
