using FlowDesk.Application.Authentication.Register;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;

    public AuthController(
        RegisterUserHandler registerUserHandler)
    {
        _registerUserHandler = registerUserHandler;
    }

    [HttpPost("register")]
    [ProducesResponseType<RegisterUserResult>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterUserResult>> Register(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        RegisterUserResult result =
            await _registerUserHandler.HandleAsync(
                command,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}
