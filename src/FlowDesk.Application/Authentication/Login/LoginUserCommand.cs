namespace FlowDesk.Application.Authentication.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password);
