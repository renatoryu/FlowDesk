using FlowDesk.Api.Authorization;
using FlowDesk.Api.Contracts.Users;
using FlowDesk.Application.Users.AssignCompany;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = AuthorizationPolicies.UserCompanyWrite)]
public sealed class UsersController : ControllerBase
{
    private readonly AssignUserCompanyHandler _assignUserCompanyHandler;

    public UsersController(
        AssignUserCompanyHandler assignUserCompanyHandler)
    {
        _assignUserCompanyHandler = assignUserCompanyHandler;
    }

    [HttpPut("{id:guid}/company")]
    [ProducesResponseType<AssignUserCompanyResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssignUserCompanyResult>> AssignCompany(
        Guid id,
        AssignUserCompanyRequest request,
        CancellationToken cancellationToken)
    {
        AssignUserCompanyResult result =
            await _assignUserCompanyHandler.HandleAsync(
                new AssignUserCompanyCommand(
                    id,
                    request.CompanyId),
                cancellationToken);

        return Ok(result);
    }
}
