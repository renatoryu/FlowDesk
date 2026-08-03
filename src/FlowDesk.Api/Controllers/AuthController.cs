using FlowDesk.Application.Authentication.Login;
using FlowDesk.Application.Authentication.Register;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly LoginUserHandler _loginUserHandler;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginUserHandler loginUserHandler)
    {
        _registerUserHandler = registerUserHandler;
        _loginUserHandler = loginUserHandler;
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

    [HttpPost("login")]
    [ProducesResponseType<LoginUserResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginUserResult>> Login(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        LoginUserResult result =
            await _loginUserHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(result);
    }
}
