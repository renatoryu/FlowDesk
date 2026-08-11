using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Abstractions.Storage;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Attachments.Download;

public sealed class DownloadAttachmentHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentStorage _attachmentStorage;
    private readonly IValidator<DownloadAttachmentQuery> _validator;

    public DownloadAttachmentHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentStorage attachmentStorage,
        IValidator<DownloadAttachmentQuery> validator)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _ticketRepository = ticketRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentStorage = attachmentStorage;
        _validator = validator;
    }

    public async Task<DownloadAttachmentResult> HandleAsync(
        DownloadAttachmentQuery query,
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
                    "Customers must be assigned to a company before downloading ticket attachments.");
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
                    "Inactive companies cannot access ticket attachments.");
            }

            customerCompanyId = company.Id;
        }
        else if (currentUser.Role is not (
                     UserRole.Admin or
                     UserRole.Agent))
        {
            throw new ForbiddenException(
                "The authenticated user cannot download ticket attachments.");
        }

        Ticket ticket =
            await _ticketRepository.GetByIdAsync(
                query.TicketId,
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

        Attachment attachment =
            await _attachmentRepository.GetByIdAsync(
                ticket.Id,
                query.AttachmentId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Attachment was not found.");

        Stream content =
            await _attachmentStorage.OpenReadAsync(
                ticket.Id,
                attachment.StoredFileName,
                cancellationToken)
            ?? throw new NotFoundException(
                "Attachment was not found.");

        return new DownloadAttachmentResult(
            content,
            attachment.ContentType,
            attachment.OriginalFileName);
    }
}
