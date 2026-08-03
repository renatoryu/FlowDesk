namespace FlowDesk.Application.Abstractions.Security;

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);
