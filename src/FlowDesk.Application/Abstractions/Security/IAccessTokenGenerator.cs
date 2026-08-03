using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Security;

public interface IAccessTokenGenerator
{
    AccessTokenResult Generate(User user);
}
