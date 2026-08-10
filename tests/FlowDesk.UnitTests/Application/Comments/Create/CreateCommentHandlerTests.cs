using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Comments.Create;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Common;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Comments.Create;

public sealed class CreateCommentHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerCreatesComment()
    {
        Company company = CreateCompany();
        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Ticket ticket = CreateTicket(company, customer);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        CreateCommentResult result =
            await handler.HandleAsync(
                new CreateCommentCommand(
                    ticket.Id,
                    "  I need help with this issue.  "));

        Comment comment = Assert.IsType<Comment>(
            commentRepository.AddedComment);

        Assert.Equal(ticket.Id, comment.TicketId);
        Assert.Equal(customer.Id, comment.AuthorId);
        Assert.Equal(
            "I need help with this issue.",
            comment.Content);

        Assert.Equal(comment.Id, result.Id);
        Assert.Equal(comment.TicketId, result.TicketId);
        Assert.Equal(comment.AuthorId, result.AuthorId);
        Assert.Equal(comment.Content, result.Content);
        Assert.Equal(1, commentRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleCreatesCommentForAnyTicket(
        UserRole role)
    {
        User privilegedUser = CreateUser(role);
        Company company = CreateCompany();
        User requester = CreateUser();
        Ticket ticket = CreateTicket(company, requester);

        var userRepository = new UserRepositorySpy
        {
            User = privilegedUser
        };

        var companyRepository = new CompanyRepositorySpy();

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(privilegedUser.Id, role),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await handler.HandleAsync(
            new CreateCommentCommand(
                ticket.Id,
                "Support response."));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, commentRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.Empty,
                    " ")));

        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownUserThrowsUnauthorizedException()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.NewGuid(),
                    "Comment content.")));

        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveUserThrowsUnauthorizedException()
    {
        User customer = CreateUser();
        customer.Deactivate();

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.NewGuid(),
                    "Comment content.")));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithStaleRoleThrowsUnauthorizedException()
    {
        User customer = CreateUser();

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Agent),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.NewGuid(),
                    "Comment content.")));

        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithCustomerWithoutCompanyThrowsConflictException()
    {
        User customer = CreateUser();

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.NewGuid(),
                    "Comment content.")));

        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyThrowsConflictException()
    {
        Company company = CreateCompany();
        company.Deactivate();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.NewGuid(),
                    "Comment content.")));

        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
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

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    ticket.Id,
                    "Comment content.")));

        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownTicketThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    Guid.NewGuid(),
                    "Comment content.")));

        Assert.Equal(1, ticketRepository.GetByIdCallCount);
        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
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

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var commentRepository = new CommentRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCommentHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork);

        await Assert.ThrowsAsync<DomainRuleException>(
            () => handler.HandleAsync(
                new CreateCommentCommand(
                    ticket.Id,
                    "Comment content.")));

        AssertCommentWasNotSaved(
            commentRepository,
            unitOfWork);
    }

    private static CreateCommentHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        return new CreateCommentHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            unitOfWork,
            new CreateCommentCommandValidator());
    }

    private static User CreateUser(
        UserRole role = UserRole.Customer)
    {
        return new User(
            "Ana Silva",
            "ana@example.com",
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

    private static void AssertCommentWasNotSaved(
        CommentRepositorySpy commentRepository,
        UnitOfWorkSpy unitOfWork)
    {
        Assert.Equal(0, commentRepository.AddCallCount);
        Assert.Null(commentRepository.AddedComment);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private sealed class CurrentUserStub : ICurrentUser
    {
        private readonly Guid _userId;
        private readonly UserRole _role;

        public CurrentUserStub(
            Guid userId,
            UserRole role)
        {
            _userId = userId;
            _role = role;
        }

        public Guid UserId => _userId;

        public UserRole Role => _role;
    }

    private sealed class UserRepositorySpy : IUserRepository
    {
        public User? User { get; init; }

        public int GetByIdCallCount { get; private set; }

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

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(User);
        }
    }

    private sealed class CompanyRepositorySpy
        : ICompanyRepository
    {
        public Company? Company { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<bool> ExistsByTaxIdAsync(
            string taxId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<Company?> GetByIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(Company);
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

    private sealed class CommentRepositorySpy
        : ICommentRepository
    {
        public Comment? AddedComment { get; private set; }

        public int AddCallCount { get; private set; }

        public Task AddAsync(
            Comment comment,
            CancellationToken cancellationToken = default)
        {
            AddedComment = comment;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Comment>> ListByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Comment>>(
                Array.Empty<Comment>());
        }
    }

    private sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCallCount++;

            return Task.FromResult(1);
        }
    }
}
