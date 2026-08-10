using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Tickets.GetById;

public sealed class GetTicketByIdHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IValidator<GetTicketByIdQuery> _validator;

    public GetTicketByIdHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IValidator<GetTicketByIdQuery> validator)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _ticketRepository = ticketRepository;
        _validator = validator;
    }

    public async Task<GetTicketByIdResult> HandleAsync(
        GetTicketByIdQuery query,
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

        Guid? customerCompanyId = null;

        if (currentUser.Role == UserRole.Customer)
        {
            if (currentUser.CompanyId is not Guid companyId)
            {
                throw new ConflictException(
                    "Customers must be assigned to a company before accessing tickets.");
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

            customerCompanyId = company.Id;
        }
        else if (currentUser.Role is not (
                     UserRole.Admin or
                     UserRole.Agent))
        {
            throw new ForbiddenException(
                "The authenticated user cannot access tickets.");
        }

        Ticket ticket =
            await _ticketRepository.GetByIdAsync(
                query.Id,
                cancellationToken)
            ?? throw new NotFoundException(
                "Ticket was not found.");

        if (currentUser.Role == UserRole.Customer &&
            (ticket.RequesterId != currentUser.Id ||
             ticket.CompanyId != customerCompanyId))
        {
            throw new NotFoundException(
                "Ticket was not found.");
        }

        return new GetTicketByIdResult(
            ticket.Id,
            ticket.CompanyId,
            ticket.CategoryId,
            ticket.RequesterId,
            ticket.Title,
            ticket.Description,
            ticket.Priority,
            ticket.Status,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.StatusChangedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc);
    }
}
