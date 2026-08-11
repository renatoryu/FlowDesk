using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Dashboards.Summary;

public sealed class GetDashboardSummaryHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IDashboardRepository dashboardRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryResult> HandleAsync(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        Guid currentUserId = _currentUser.UserId;
        UserRole tokenRole = _currentUser.Role;

        User currentUser =
            await _userRepository.GetByIdAsync(
                currentUserId,
                cancellationToken)
            ?? throw new UnauthorizedException(
                "The authenticated user was not found.");

        if (!currentUser.IsActive)
        {
            throw new UnauthorizedException(
                "The authenticated user is not active.");
        }

        if (currentUser.Role != tokenRole)
        {
            throw new UnauthorizedException(
                "The authentication session is no longer valid.");
        }

        DashboardTicketFilter filter;

        if (currentUser.Role == UserRole.Customer)
        {
            if (currentUser.CompanyId is not Guid companyId)
            {
                throw new ConflictException(
                    "Customers must be assigned to a company before accessing the dashboard.");
            }

            Company company =
                await _companyRepository.GetByIdAsync(
                    companyId,
                    cancellationToken)
                ?? throw new ConflictException(
                    "The customer's company is unavailable.");

            if (!company.IsActive)
            {
                throw new ConflictException(
                    "Inactive companies cannot be accessed.");
            }

            filter = new DashboardTicketFilter(
                company.Id,
                currentUser.Id);
        }
        else if (currentUser.Role is
                 UserRole.Admin or UserRole.Agent)
        {
            filter = new DashboardTicketFilter(
                null,
                null);
        }
        else
        {
            throw new ForbiddenException(
                "The authenticated user cannot access the dashboard.");
        }

        DashboardTicketCounts counts =
            await _dashboardRepository.GetTicketCountsAsync(
                filter,
                cancellationToken);

        return new DashboardSummaryResult(
            counts.OpenTickets,
            counts.InProgressTickets,
            counts.CompletedTickets);
    }
}
