namespace FlowDesk.Application.Authentication.Register;

public sealed record RegisterUserResult(
    Guid Id,
    string FullName,
    string Email,
    string Role);
