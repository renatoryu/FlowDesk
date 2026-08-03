namespace FlowDesk.Application.Abstractions.Security;

public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate();

    string ComputeHash(string token);
}
