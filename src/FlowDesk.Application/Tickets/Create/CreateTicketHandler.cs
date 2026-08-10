using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Tickets.Create;

public sealed class CreateTicketHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateTicketCommand> _validator;

    public CreateTicketHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ICategoryRepository categoryRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTicketCommand> validator)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _categoryRepository = categoryRepository;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<CreateTicketResult> HandleAsync(
        CreateTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        Guid currentUserId = _currentUser.UserId;

        User requester =
            await _userRepository.GetByIdAsync(
                currentUserId,
                cancellationToken)
            ?? throw new UnauthorizedException(
                "The authenticated user was not found.");

        if (!requester.IsActive)
        {
            throw new UnauthorizedException(
                "The authenticated user is not active.");
        }

        if (requester.Role != UserRole.Customer)
        {
            throw new ForbiddenException(
                "Only customers can create tickets.");
        }

        if (requester.CompanyId is not Guid companyId)
        {
            throw new ConflictException(
                "Customers must be assigned to a company before creating tickets.");
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
                "Inactive companies cannot receive tickets.");
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
                "Inactive categories cannot receive tickets.");
        }

        var ticket = new Ticket(
            company.Id,
            category.Id,
            requester.Id,
            command.Title,
            command.Description,
            command.Priority);

        await _ticketRepository.AddAsync(
            ticket,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateTicketResult(
            ticket.Id,
            ticket.CompanyId,
            ticket.CategoryId,
            ticket.RequesterId,
            ticket.Title,
            ticket.Description,
            ticket.Priority,
            ticket.Status,
            ticket.CreatedAtUtc,
            ticket.StatusChangedAtUtc);
    }
}
