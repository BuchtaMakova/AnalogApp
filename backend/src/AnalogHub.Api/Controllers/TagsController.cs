using AnalogHub.Application.Tags.Dtos;
using AnalogHub.Application.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagsController : ControllerBase
{
    private readonly ISender _sender;

    public TagsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetTags(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetTagsQuery(), cancellationToken));
}
