using FlowDesk.Api.Authorization;
using FlowDesk.Application.Dashboards.Summary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = AuthorizationPolicies.DashboardRead)]
public sealed class DashboardController : ControllerBase
{
    private readonly GetDashboardSummaryHandler _getDashboardSummaryHandler;

    public DashboardController(
        GetDashboardSummaryHandler getDashboardSummaryHandler)
    {
        _getDashboardSummaryHandler = getDashboardSummaryHandler;
    }

    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardSummaryResult>> GetSummary(
        CancellationToken cancellationToken)
    {
        DashboardSummaryResult result =
            await _getDashboardSummaryHandler.HandleAsync(
                new GetDashboardSummaryQuery(),
                cancellationToken);

        return Ok(result);
    }
}
