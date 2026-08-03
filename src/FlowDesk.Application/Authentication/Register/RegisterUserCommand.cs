namespace FlowDesk.Application.Authentication.Register;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password);
