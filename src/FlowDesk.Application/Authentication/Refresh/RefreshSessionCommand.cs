namespace FlowDesk.Application.Authentication.Refresh;

public sealed record RefreshSessionCommand(
    string RefreshToken);
