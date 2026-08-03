namespace FlowDesk.Application.Authentication.Refresh;

public sealed record RefreshSessionResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
