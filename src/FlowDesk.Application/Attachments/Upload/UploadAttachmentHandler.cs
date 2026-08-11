using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Abstractions.Storage;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.Application.Attachments.Upload;

public sealed class UploadAttachmentHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentStorage _attachmentStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UploadAttachmentCommand> _validator;

    public UploadAttachmentHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentStorage attachmentStorage,
        IUnitOfWork unitOfWork,
        IValidator<UploadAttachmentCommand> validator)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _ticketRepository = ticketRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentStorage = attachmentStorage;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<UploadAttachmentResult> HandleAsync(
        UploadAttachmentCommand command,
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
                    "Customers must be assigned to a company before uploading attachments.");
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
                "The authenticated user cannot upload attachments.");
        }

        Ticket ticket =
            await _ticketRepository.GetByIdAsync(
                command.TicketId,
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

        ticket.EnsureCanReceiveAttachments();

        string originalFileName =
            command.OriginalFileName.Trim();

        string contentType =
            command.ContentType.Trim().ToLowerInvariant();

        string fileExtension =
            Path.GetExtension(originalFileName).ToLowerInvariant();

        command.Content.Position = 0;

        string storedFileName =
            await _attachmentStorage.SaveAsync(
                ticket.Id,
                command.Content,
                fileExtension,
                cancellationToken);

        try
        {
            var attachment = new Attachment(
                ticket.Id,
                currentUser.Id,
                originalFileName,
                storedFileName,
                contentType,
                command.SizeInBytes);

            await _attachmentRepository.AddAsync(
                attachment,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new UploadAttachmentResult(
                attachment.Id,
                attachment.TicketId,
                attachment.UploadedById,
                attachment.OriginalFileName,
                attachment.ContentType,
                attachment.SizeInBytes,
                attachment.CreatedAtUtc);
        }
        catch
        {
            await _attachmentStorage.DeleteAsync(
                ticket.Id,
                storedFileName,
                CancellationToken.None);

            throw;
        }
    }
}
