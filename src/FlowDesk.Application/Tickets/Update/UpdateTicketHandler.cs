using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Tickets.Update;

public sealed class UpdateTicketHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateTicketCommand> _validator;

    public UpdateTicketHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ICategoryRepository categoryRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateTicketCommand> validator)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _categoryRepository = categoryRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<UpdateTicketResult> HandleAsync(
        UpdateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
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
                    "Customers must be assigned to a company before updating tickets.");
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
                    "Inactive companies cannot access tickets.");
            }

            customerCompanyId = company.Id;
        }
        else if (currentUser.Role is not (
                     UserRole.Admin or
                     UserRole.Agent))
        {
            throw new ForbiddenException(
                "The authenticated user cannot update tickets.");
        }

        Ticket ticket =
            await _ticketRepository.GetForUpdateAsync(
                command.Id,
                cancellationToken: cancellationToken)
            ?? throw new NotFoundException(
                "Ticket was not found.");

        if (currentUser.Role == UserRole.Customer &&
            (ticket.RequesterId != currentUser.Id ||
             ticket.CompanyId != customerCompanyId))
        {
            throw new NotFoundException(
                "Ticket was not found.");
        }

        Category category =
            await _categoryRepository.GetByIdAsync(
                command.CategoryId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Category was not found.");

        if (!category.IsActive)
        {
            throw new ConflictException(
                "Inactive categories cannot be assigned to tickets.");
        }

        ticket.UpdateDetails(
            command.Title,
            command.Description,
            category.Id,
            command.Priority);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new UpdateTicketResult(
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
