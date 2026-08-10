using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Abstractions.Security;

public interface ICurrentUser
{
    Guid UserId { get; }

    UserRole Role { get; }
}
