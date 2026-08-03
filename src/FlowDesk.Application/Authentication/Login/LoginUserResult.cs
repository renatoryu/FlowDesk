namespace FlowDesk.Application.Authentication.Login;

public sealed record LoginUserResult(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
