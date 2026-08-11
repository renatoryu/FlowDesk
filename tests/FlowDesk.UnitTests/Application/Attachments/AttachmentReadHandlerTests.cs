using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Abstractions.Storage;
using FlowDesk.Application.Attachments.Download;
using FlowDesk.Application.Attachments.List;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.UnitTests.Application.Attachments;

public sealed class AttachmentReadHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task ListWithAdminReturnsAttachmentMetadata()
    {
        User admin = CreateUser(UserRole.Admin);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);
        Attachment attachment = CreateAttachment(ticket, admin);

        var attachmentRepository =
            new AttachmentRepositorySpy
            {
                Attachments = [attachment]
            };

        ListTicketAttachmentsHandler handler =
            CreateListHandler(
                new CurrentUserStub(admin.Id, UserRole.Admin),
                new UserRepositorySpy { User = admin },
                new CompanyRepositorySpy(),
                new TicketRepositorySpy { Ticket = ticket },
                attachmentRepository);

        ListTicketAttachmentsResult result =
            await handler.HandleAsync(
                new ListTicketAttachmentsQuery(ticket.Id));

        AttachmentListItem item =
            Assert.Single(result.Items);

        Assert.Equal(attachment.Id, item.Id);
        Assert.Equal(attachment.TicketId, item.TicketId);
        Assert.Equal(attachment.UploadedById, item.UploadedById);
        Assert.Equal(
            attachment.OriginalFileName,
            item.OriginalFileName);
        Assert.Equal(attachment.ContentType, item.ContentType);
        Assert.Equal(attachment.SizeInBytes, item.SizeInBytes);

        Assert.Equal(1, attachmentRepository.ListCallCount);
        Assert.Equal(0, attachmentRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task ListWithAnotherCustomerTicketThrowsNotFoundException()
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

        ListTicketAttachmentsHandler handler =
            CreateListHandler(
                new CurrentUserStub(
                    customer.Id,
                    UserRole.Customer),
                new UserRepositorySpy { User = customer },
                new CompanyRepositorySpy { Company = company },
                new TicketRepositorySpy { Ticket = ticket },
                attachmentRepository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new ListTicketAttachmentsQuery(ticket.Id)));

        Assert.Equal(0, attachmentRepository.ListCallCount);
    }

    [Fact]
    public async Task DownloadWithAdminReturnsStoredFile()
    {
        User admin = CreateUser(UserRole.Admin);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);
        Attachment attachment = CreateAttachment(ticket, admin);

        var attachmentRepository =
            new AttachmentRepositorySpy
            {
                Attachment = attachment
            };

        using var content =
            new MemoryStream(
                [0x89, 0x50, 0x4E, 0x47],
                writable: false);

        var storage = new AttachmentStorageSpy
        {
            Content = content
        };

        DownloadAttachmentHandler handler =
            CreateDownloadHandler(
                new CurrentUserStub(admin.Id, UserRole.Admin),
                new UserRepositorySpy { User = admin },
                new CompanyRepositorySpy(),
                new TicketRepositorySpy { Ticket = ticket },
                attachmentRepository,
                storage);

        DownloadAttachmentResult result =
            await handler.HandleAsync(
                new DownloadAttachmentQuery(
                    ticket.Id,
                    attachment.Id));

        Assert.Same(content, result.Content);
        Assert.Equal(
            attachment.ContentType,
            result.ContentType);
        Assert.Equal(
            attachment.OriginalFileName,
            result.FileName);

        Assert.Equal(1, attachmentRepository.GetByIdCallCount);
        Assert.Equal(1, storage.OpenReadCallCount);
        Assert.Equal(ticket.Id, storage.OpenedTicketId);
        Assert.Equal(
            attachment.StoredFileName,
            storage.OpenedStoredFileName);
    }

    [Fact]
    public async Task DownloadWithAnotherCustomerTicketThrowsNotFoundException()
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

        DownloadAttachmentHandler handler =
            CreateDownloadHandler(
                new CurrentUserStub(
                    customer.Id,
                    UserRole.Customer),
                new UserRepositorySpy { User = customer },
                new CompanyRepositorySpy { Company = company },
                new TicketRepositorySpy { Ticket = ticket },
                attachmentRepository,
                storage);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new DownloadAttachmentQuery(
                    ticket.Id,
                    Guid.NewGuid())));

        Assert.Equal(0, attachmentRepository.GetByIdCallCount);
        Assert.Equal(0, storage.OpenReadCallCount);
    }

    [Fact]
    public async Task DownloadWithUnknownAttachmentThrowsNotFoundException()
    {
        User admin = CreateUser(UserRole.Admin);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);

        var attachmentRepository =
            new AttachmentRepositorySpy();

        var storage = new AttachmentStorageSpy();

        DownloadAttachmentHandler handler =
            CreateDownloadHandler(
                new CurrentUserStub(admin.Id, UserRole.Admin),
                new UserRepositorySpy { User = admin },
                new CompanyRepositorySpy(),
                new TicketRepositorySpy { Ticket = ticket },
                attachmentRepository,
                storage);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new DownloadAttachmentQuery(
                    ticket.Id,
                    Guid.NewGuid())));

        Assert.Equal(1, attachmentRepository.GetByIdCallCount);
        Assert.Equal(0, storage.OpenReadCallCount);
    }

    [Fact]
    public async Task DownloadWithMissingStoredFileThrowsNotFoundException()
    {
        User admin = CreateUser(UserRole.Admin);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);
        Attachment attachment = CreateAttachment(ticket, admin);

        var attachmentRepository =
            new AttachmentRepositorySpy
            {
                Attachment = attachment
            };

        var storage = new AttachmentStorageSpy();

        DownloadAttachmentHandler handler =
            CreateDownloadHandler(
                new CurrentUserStub(admin.Id, UserRole.Admin),
                new UserRepositorySpy { User = admin },
                new CompanyRepositorySpy(),
                new TicketRepositorySpy { Ticket = ticket },
                attachmentRepository,
                storage);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new DownloadAttachmentQuery(
                    ticket.Id,
                    attachment.Id)));

        Assert.Equal(1, attachmentRepository.GetByIdCallCount);
        Assert.Equal(1, storage.OpenReadCallCount);
    }

    private static ListTicketAttachmentsHandler
        CreateListHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ITicketRepository ticketRepository,
            IAttachmentRepository attachmentRepository)
    {
        return new ListTicketAttachmentsHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            attachmentRepository,
            new ListTicketAttachmentsQueryValidator());
    }

    private static DownloadAttachmentHandler
        CreateDownloadHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ITicketRepository ticketRepository,
            IAttachmentRepository attachmentRepository,
            IAttachmentStorage storage)
    {
        return new DownloadAttachmentHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            attachmentRepository,
            storage,
            new DownloadAttachmentQueryValidator());
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

    private static Attachment CreateAttachment(
        Ticket ticket,
        User uploader)
    {
        return new Attachment(
            ticket.Id,
            uploader.Id,
            "evidence.png",
            "stored-evidence.png",
            Attachment.PngContentType,
            4);
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

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
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

        public Task<Company?> GetByIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
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

        public Task<Ticket?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
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
        public Attachment? Attachment { get; init; }

        public IReadOnlyList<Attachment> Attachments { get; init; } =
            Array.Empty<Attachment>();

        public int GetByIdCallCount { get; private set; }

        public int ListCallCount { get; private set; }

        public Task<Attachment?> GetByIdAsync(
            Guid ticketId,
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;
            return Task.FromResult(Attachment);
        }

        public Task<IReadOnlyList<Attachment>> ListByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            ListCallCount++;
            return Task.FromResult(Attachments);
        }

        public Task AddAsync(
            Attachment attachment,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class AttachmentStorageSpy
        : IAttachmentStorage
    {
        public Stream? Content { get; init; }

        public int OpenReadCallCount { get; private set; }

        public Guid? OpenedTicketId { get; private set; }

        public string? OpenedStoredFileName { get; private set; }

        public Task<string> SaveAsync(
            Guid ticketId,
            Stream content,
            string fileExtension,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("unused");
        }

        public Task<Stream?> OpenReadAsync(
            Guid ticketId,
            string storedFileName,
            CancellationToken cancellationToken = default)
        {
            OpenReadCallCount++;
            OpenedTicketId = ticketId;
            OpenedStoredFileName = storedFileName;

            return Task.FromResult(Content);
        }

        public Task DeleteAsync(
            Guid ticketId,
            string storedFileName,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
