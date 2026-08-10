using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Comments.List;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Comments.List;

public sealed class ListTicketCommentsHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerReturnsCommentHistory()
    {
        Company company = CreateCompany();
        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Ticket ticket = CreateTicket(company, customer);

        Comment firstComment = new(
            ticket.Id,
            customer.Id,
            "First comment.");

        Comment secondComment = new(
            ticket.Id,
            customer.Id,
            "Second comment.");

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

        var commentRepository = new CommentRepositorySpy
        {
            Comments = new[]
            {
                firstComment,
                secondComment
            }
        };

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        ListTicketCommentsResult result =
            await handler.HandleAsync(
                new ListTicketCommentsQuery(ticket.Id));

        Assert.Collection(
            result.Items,
            first =>
            {
                Assert.Equal(firstComment.Id, first.Id);
                Assert.Equal(
                    "First comment.",
                    first.Content);
            },
            second =>
            {
                Assert.Equal(secondComment.Id, second.Id);
                Assert.Equal(
                    "Second comment.",
                    second.Content);
            });

        Assert.Equal(
            1,
            commentRepository.ListByTicketIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithClosedTicketReturnsHistory()
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

        var commentRepository = new CommentRepositorySpy
        {
            Comments = new[]
            {
                new Comment(
                    ticket.Id,
                    customer.Id,
                    "Previous interaction.")
            }
        };

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        ListTicketCommentsResult result =
            await handler.HandleAsync(
                new ListTicketCommentsQuery(ticket.Id));

        Assert.Single(result.Items);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleReturnsAnyTicketHistory(
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

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(privilegedUser.Id, role),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        ListTicketCommentsResult result =
            await handler.HandleAsync(
                new ListTicketCommentsQuery(ticket.Id));

        Assert.Empty(result.Items);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(
            1,
            commentRepository.ListByTicketIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidQueryDoesNotAccessPersistence()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var commentRepository = new CommentRepositorySpy();

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new ListTicketCommentsQuery(Guid.Empty)));

        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            commentRepository.ListByTicketIdCallCount);
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

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Agent),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new ListTicketCommentsQuery(Guid.NewGuid())));

        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            commentRepository.ListByTicketIdCallCount);
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

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new ListTicketCommentsQuery(Guid.NewGuid())));

        Assert.Equal(0, ticketRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            commentRepository.ListByTicketIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithAnotherCustomerTicketThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        User anotherCustomer = CreateUser();
        Ticket ticket = CreateTicket(company, anotherCustomer);

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

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new ListTicketCommentsQuery(ticket.Id)));

        Assert.Equal(
            0,
            commentRepository.ListByTicketIdCallCount);
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

        ListTicketCommentsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new ListTicketCommentsQuery(Guid.NewGuid())));

        Assert.Equal(1, ticketRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            commentRepository.ListByTicketIdCallCount);
    }

    private static ListTicketCommentsHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        ICommentRepository commentRepository)
    {
        return new ListTicketCommentsHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            commentRepository,
            new ListTicketCommentsQueryValidator());
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
        public IReadOnlyList<Comment> Comments { get; init; } =
            Array.Empty<Comment>();

        public int ListByTicketIdCallCount { get; private set; }

        public Task AddAsync(
            Comment comment,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Comment>> ListByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            ListByTicketIdCallCount++;

            return Task.FromResult(Comments);
        }
    }
}
