using AnalogHub.Application.Auth.Commands;
using AnalogHub.Application.Auth.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Register(
        [FromBody] RegisterCommand command, CancellationToken cancellationToken)
        => Ok(await _sender.Send(command, cancellationToken));

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResultDto>> Login(
        [FromBody] LoginCommand command, CancellationToken cancellationToken)
        => Ok(await _sender.Send(command, cancellationToken));
}
