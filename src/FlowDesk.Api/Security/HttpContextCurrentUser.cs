using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Api.Security;

public sealed class HttpContextCurrentUser
    : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            ClaimsPrincipal user =
                GetAuthenticatedUser();

            string? userIdClaim =
                user.FindFirstValue(
                    JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(
                    userIdClaim,
                    out Guid userId) ||
                userId == Guid.Empty)
            {
                throw new UnauthorizedException(
                    "The authenticated user id is missing or invalid.");
            }

            return userId;
        }
    }

    public UserRole Role
    {
        get
        {
            ClaimsPrincipal user =
                GetAuthenticatedUser();

            string? roleClaim =
                user.FindFirstValue("role");

            if (!Enum.TryParse(
                    roleClaim,
                    ignoreCase: false,
                    out UserRole role) ||
                !Enum.IsDefined(role))
            {
                throw new UnauthorizedException(
                    "The authenticated user role is missing or invalid.");
            }

            return role;
        }
    }

    private ClaimsPrincipal GetAuthenticatedUser()
    {
        ClaimsPrincipal? user =
            _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedException(
                "The request is not authenticated.");
        }

        return user;
    }
}
