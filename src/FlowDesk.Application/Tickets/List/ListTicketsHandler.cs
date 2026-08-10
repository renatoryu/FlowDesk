using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Tickets.List;

public sealed class ListTicketsHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IValidator<ListTicketsQuery> _validator;

    public ListTicketsHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IValidator<ListTicketsQuery> validator)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _ticketRepository = ticketRepository;
        _validator = validator;
    }

    public async Task<ListTicketsResult> HandleAsync(
        ListTicketsQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            query,
            cancellationToken);

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

        TicketListFilter filter;

        if (currentUser.Role == UserRole.Customer)
        {
            if (currentUser.CompanyId is not Guid companyId)
            {
                throw new ConflictException(
                    "Customers must be assigned to a company before listing tickets.");
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

            filter = new TicketListFilter(
                company.Id,
                currentUser.Id,
                query.CategoryId,
                query.Priority,
                query.Status,
                query.Page,
                query.PageSize);
        }
        else if (currentUser.Role is
                 UserRole.Admin or UserRole.Agent)
        {
            filter = new TicketListFilter(
                null,
                null,
                query.CategoryId,
                query.Priority,
                query.Status,
                query.Page,
                query.PageSize);
        }
        else
        {
            throw new ForbiddenException(
                "The authenticated user cannot list tickets.");
        }

        PagedResult<Ticket> page =
            await _ticketRepository.ListAsync(
                filter,
                cancellationToken);

        TicketListItem[] items =
            page.Items
                .Select(ticket => new TicketListItem(
                    ticket.Id,
                    ticket.CompanyId,
                    ticket.CategoryId,
                    ticket.RequesterId,
                    ticket.Title,
                    ticket.Priority,
                    ticket.Status,
                    ticket.CreatedAtUtc,
                    ticket.UpdatedAtUtc,
                    ticket.StatusChangedAtUtc))
                .ToArray();

        return new ListTicketsResult(
            items,
            page.Page,
            page.PageSize,
            page.TotalCount,
            page.TotalPages);
    }
}
