using System.Text;
using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Abstractions.Storage;
using FlowDesk.Application.Attachments.Upload;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Common;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Attachments.Upload;

public sealed class UploadAttachmentHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerUploadsAttachment()
    {
        Company company = CreateCompany();
        User customer = CreateUser();
        customer.AssignToCompany(company.Id);
        Ticket ticket = CreateTicket(company, customer);

        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();
        var unitOfWork = new UnitOfWorkSpy();

        UploadAttachmentHandler handler = CreateHandler(
            new CurrentUserStub(customer.Id, UserRole.Customer),
            new UserRepositorySpy { User = customer },
            new CompanyRepositorySpy { Company = company },
            new TicketRepositorySpy { Ticket = ticket },
            attachmentRepository,
            storage,
            unitOfWork);

        using MemoryStream content = CreatePdfContent();

        UploadAttachmentResult result =
            await handler.HandleAsync(
                CreateCommand(ticket.Id, content));

        Attachment attachment =
            Assert.IsType<Attachment>(
                attachmentRepository.AddedAttachment);

        Assert.Equal(ticket.Id, attachment.TicketId);
        Assert.Equal(customer.Id, attachment.UploadedById);
        Assert.Equal("evidence.pdf", attachment.OriginalFileName);
        Assert.Equal(storage.StoredFileName, attachment.StoredFileName);
        Assert.Equal(Attachment.PdfContentType, attachment.ContentType);
        Assert.Equal(content.Length, attachment.SizeInBytes);

        Assert.Equal(attachment.Id, result.Id);
        Assert.Equal(attachment.TicketId, result.TicketId);
        Assert.Equal(attachment.UploadedById, result.UploadedById);

        Assert.Equal(1, storage.SaveCallCount);
        Assert.Equal(".pdf", storage.SavedExtension);
        Assert.Equal(0, storage.DeleteCallCount);
        Assert.Equal(1, attachmentRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleUploadsForAnyTicket(
        UserRole role)
    {
        User privilegedUser = CreateUser(role);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);

        var companyRepository =
            new CompanyRepositorySpy();

        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();
        var unitOfWork = new UnitOfWorkSpy();

        UploadAttachmentHandler handler = CreateHandler(
            new CurrentUserStub(privilegedUser.Id, role),
            new UserRepositorySpy { User = privilegedUser },
            companyRepository,
            new TicketRepositorySpy { Ticket = ticket },
            attachmentRepository,
            storage,
            unitOfWork);

        using MemoryStream content = CreatePdfContent();

        await handler.HandleAsync(
            CreateCommand(ticket.Id, content));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, storage.SaveCallCount);
        Assert.Equal(1, attachmentRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotSaveFile()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();
        var unitOfWork = new UnitOfWorkSpy();

        UploadAttachmentHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            attachmentRepository,
            storage,
            unitOfWork);

        using MemoryStream content = CreatePdfContent();

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                CreateCommand(Guid.Empty, content)));

        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        Assert.Equal(0, storage.SaveCallCount);
        Assert.Equal(0, attachmentRepository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithAnotherCustomerTicketThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        User anotherCustomer = CreateUser();
        Ticket ticket = CreateTicket(
            company,
            anotherCustomer);

        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();
        var unitOfWork = new UnitOfWorkSpy();

        UploadAttachmentHandler handler = CreateHandler(
            new CurrentUserStub(customer.Id, UserRole.Customer),
            new UserRepositorySpy { User = customer },
            new CompanyRepositorySpy { Company = company },
            new TicketRepositorySpy { Ticket = ticket },
            attachmentRepository,
            storage,
            unitOfWork);

        using MemoryStream content = CreatePdfContent();

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                CreateCommand(ticket.Id, content)));

        Assert.Equal(0, storage.SaveCallCount);
        Assert.Equal(0, attachmentRepository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithClosedTicketThrowsDomainRuleException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Ticket ticket = CreateTicket(company, customer);
        ticket.ChangeStatus(TicketStatus.InProgress);
        ticket.ChangeStatus(TicketStatus.Resolved);
        ticket.ChangeStatus(TicketStatus.Closed);

        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();
        var unitOfWork = new UnitOfWorkSpy();

        UploadAttachmentHandler handler = CreateHandler(
            new CurrentUserStub(customer.Id, UserRole.Customer),
            new UserRepositorySpy { User = customer },
            new CompanyRepositorySpy { Company = company },
            new TicketRepositorySpy { Ticket = ticket },
            attachmentRepository,
            storage,
            unitOfWork);

        using MemoryStream content = CreatePdfContent();

        await Assert.ThrowsAsync<DomainRuleException>(
            () => handler.HandleAsync(
                CreateCommand(ticket.Id, content)));

        Assert.Equal(0, storage.SaveCallCount);
        Assert.Equal(0, attachmentRepository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWhenDatabaseFailsDeletesStoredFile()
    {
        User admin = CreateUser(UserRole.Admin);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);

        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();

        var unitOfWork = new UnitOfWorkSpy
        {
            ExceptionToThrow =
                new InvalidOperationException(
                    "Database failure.")
        };

        UploadAttachmentHandler handler = CreateHandler(
            new CurrentUserStub(admin.Id, UserRole.Admin),
            new UserRepositorySpy { User = admin },
            new CompanyRepositorySpy(),
            new TicketRepositorySpy { Ticket = ticket },
            attachmentRepository,
            storage,
            unitOfWork);

        using MemoryStream content = CreatePdfContent();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                CreateCommand(ticket.Id, content)));

        Assert.Equal(1, storage.SaveCallCount);
        Assert.Equal(1, attachmentRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);

        Assert.Equal(1, storage.DeleteCallCount);
        Assert.Equal(ticket.Id, storage.DeletedTicketId);
        Assert.Equal(
            storage.StoredFileName,
            storage.DeletedStoredFileName);
    }

    private static UploadAttachmentHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentStorage storage,
        IUnitOfWork unitOfWork)
    {
        return new UploadAttachmentHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            attachmentRepository,
            storage,
            unitOfWork,
            new UploadAttachmentCommandValidator());
    }

    private static UploadAttachmentCommand CreateCommand(
        Guid ticketId,
        MemoryStream content)
    {
        return new UploadAttachmentCommand(
            ticketId,
            "evidence.pdf",
            Attachment.PdfContentType,
            content.Length,
            content);
    }

    private static MemoryStream CreatePdfContent()
    {
        return new MemoryStream(
            Encoding.ASCII.GetBytes(
                "%PDF-1.7\nFlowDesk evidence."),
            writable: false);
    }

    private static User CreateUser(
        UserRole role = UserRole.Customer)
    {
        return new User(
            "Ana Silva",
            $"{Guid.NewGuid():N}@example.com",
            "hashed-password",
            role);
    }

    private static Company CreateCompany()
    {
        return new Company(
            "FlowDesk Tecnologia",
            ValidTaxId,
            "contact@flowdesk.com.br");
    }

    private static Ticket CreateTicket(
        Company company,
        User requester)
    {
        return new Ticket(
            company.Id,
            Guid.NewGuid(),
            requester.Id,
            "Cannot access the system",
            "The user cannot sign in to the internal system.",
            TicketPriority.High);
    }

    private sealed class CurrentUserStub : ICurrentUser
    {
        public CurrentUserStub(
            Guid userId,
            UserRole role)
        {
            UserId = userId;
            Role = role;
        }

        public Guid UserId { get; }

        public UserRole Role { get; }
    }

    private sealed class UserRepositorySpy : IUserRepository
    {
        public User? User { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;
            return Task.FromResult(User);
        }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetForUpdateAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class CompanyRepositorySpy
        : ICompanyRepository
    {
        public Company? Company { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<Company?> GetByIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;
            return Task.FromResult(Company);
        }

        public Task<bool> ExistsByTaxIdAsync(
            string taxId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<Company>> ListAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Company>>(
                Array.Empty<Company>());
        }

        public Task AddAsync(
            Company company,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TicketRepositorySpy : ITicketRepository
    {
        public Ticket? Ticket { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<Ticket?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;
            return Task.FromResult(Ticket);
        }

        public Task<Ticket?> GetForUpdateAsync(
            Guid ticketId,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Ticket?>(null);
        }

        public Task<PagedResult<Ticket>> ListAsync(
            TicketListFilter filter,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new PagedResult<Ticket>(
                    Array.Empty<Ticket>(),
                    1,
                    20,
                    0));
        }

        public Task AddAsync(
            Ticket ticket,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class AttachmentRepositorySpy
        : IAttachmentRepository
    {
        public Attachment? AddedAttachment { get; private set; }

        public int AddCallCount { get; private set; }

        public Task<Attachment?> GetByIdAsync(
            Guid ticketId,
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Attachment?>(null);
        }

        public Task<IReadOnlyList<Attachment>> ListByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Attachment>>(
                Array.Empty<Attachment>());
        }

        public Task AddAsync(
            Attachment attachment,
            CancellationToken cancellationToken = default)
        {
            AddedAttachment = attachment;
            AddCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class AttachmentStorageSpy
        : IAttachmentStorage
    {
        public string StoredFileName { get; } =
            "stored-evidence.pdf";

        public int SaveCallCount { get; private set; }

        public string? SavedExtension { get; private set; }

        public int DeleteCallCount { get; private set; }

        public Guid? DeletedTicketId { get; private set; }

        public string? DeletedStoredFileName { get; private set; }

        public Task<string> SaveAsync(
            Guid ticketId,
            Stream content,
            string fileExtension,
            CancellationToken cancellationToken = default)
        {
            SaveCallCount++;
            SavedExtension = fileExtension;

            return Task.FromResult(StoredFileName);
        }

        public Task<Stream?> OpenReadAsync(
            Guid ticketId,
            string storedFileName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream?>(null);
        }

        public Task DeleteAsync(
            Guid ticketId,
            string storedFileName,
            CancellationToken cancellationToken = default)
        {
            DeleteCallCount++;
            DeletedTicketId = ticketId;
            DeletedStoredFileName = storedFileName;

            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public Exception? ExceptionToThrow { get; init; }

        public int SaveCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCallCount++;

            if (ExceptionToThrow is not null)
            {
                return Task.FromException<int>(
                    ExceptionToThrow);
            }

            return Task.FromResult(1);
        }
    }
}
