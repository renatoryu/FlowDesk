namespace FlowDesk.Application.Abstractions.Security;

public sealed record GeneratedRefreshToken(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);
