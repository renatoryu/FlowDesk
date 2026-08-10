using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Application.Tickets.Update;
using FlowDesk.Domain.Common;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Tickets.Update;

public sealed class UpdateTicketHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerUpdatesAndSaves()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Category originalCategory = CreateCategory();

        Category updatedCategory = CreateCategory(
            "Network",
            "Connectivity and network problems.");

        Ticket ticket = CreateTicket(
            company,
            customer,
            originalCategory);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy
        {
            Category = updatedCategory
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        UpdateTicketResult result =
            await handler.HandleAsync(
                new UpdateTicketCommand(
                    ticket.Id,
                    updatedCategory.Id,
                    "  Updated ticket title  ",
                    "  Updated ticket description.  ",
                    TicketPriority.Critical));

        Assert.Equal("Updated ticket title", ticket.Title);
        Assert.Equal(
            "Updated ticket description.",
            ticket.Description);
        Assert.Equal(updatedCategory.Id, ticket.CategoryId);
        Assert.Equal(TicketPriority.Critical, ticket.Priority);

        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.CompanyId, result.CompanyId);
        Assert.Equal(ticket.CategoryId, result.CategoryId);
        Assert.Equal(ticket.RequesterId, result.RequesterId);
        Assert.Equal(ticket.Title, result.Title);
        Assert.Equal(ticket.Description, result.Description);
        Assert.Equal(ticket.Priority, result.Priority);
        Assert.Equal(ticket.Status, result.Status);

        Assert.Equal(1, userRepository.GetByIdCallCount);
        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, categoryRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleUpdatesAnyTicket(
        UserRole role)
    {
        User privilegedUser = CreateUser(role);
        User requester = CreateUser();

        Company company = CreateCompany();
        Category originalCategory = CreateCategory();

        Category updatedCategory = CreateCategory(
            "Hardware",
            "Physical equipment and device problems.");

        Ticket ticket = CreateTicket(
            company,
            requester,
            originalCategory);

        var userRepository = new UserRepositorySpy
        {
            User = privilegedUser
        };

        var companyRepository = new CompanyRepositorySpy();

        var categoryRepository = new CategoryRepositorySpy
        {
            Category = updatedCategory
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                privilegedUser.Id,
                role),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        UpdateTicketResult result =
            await handler.HandleAsync(
                new UpdateTicketCommand(
                    ticket.Id,
                    updatedCategory.Id,
                    "Updated by support",
                    "The support team updated this ticket.",
                    TicketPriority.Medium));

        Assert.Equal("Updated by support", result.Title);
        Assert.Equal(updatedCategory.Id, result.CategoryId);
        Assert.Equal(TicketPriority.Medium, result.Priority);

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        var currentUser = new CurrentUserStub(
            Guid.NewGuid(),
            UserRole.Customer);

        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new UpdateTicketCommand(
                    Guid.Empty,
                    Guid.Empty,
                    string.Empty,
                    string.Empty,
                    (TicketPriority)0)));

        Assert.Equal(0, currentUser.UserIdReadCount);
        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
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

        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                CreateValidCommand(
                    Guid.NewGuid(),
                    Guid.NewGuid())));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithAnotherRequesterTicketThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        User anotherCustomer = CreateUser();
        Category category = CreateCategory();

        Ticket ticket = CreateTicket(
            company,
            anotherCustomer,
            category);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                CreateValidCommand(
                    ticket.Id,
                    category.Id)));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCategoryThrowsConflictException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Category originalCategory = CreateCategory();
        Category inactiveCategory = CreateCategory(
            "Inactive",
            "Inactive category.");
        inactiveCategory.Deactivate();

        Ticket ticket = CreateTicket(
            company,
            customer,
            originalCategory);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy
        {
            Category = inactiveCategory
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                CreateValidCommand(
                    ticket.Id,
                    inactiveCategory.Id)));

        Assert.Equal(1, categoryRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithClosedTicketThrowsDomainRuleException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Category category = CreateCategory();

        Ticket ticket = CreateTicket(
            company,
            customer,
            category);

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

        var categoryRepository = new CategoryRepositorySpy
        {
            Category = category
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        UpdateTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<DomainRuleException>(
            () => handler.HandleAsync(
                CreateValidCommand(
                    ticket.Id,
                    category.Id)));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(1, categoryRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private static UpdateTicketHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ICategoryRepository categoryRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        return new UpdateTicketHandler(
            currentUser,
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork,
            new UpdateTicketCommandValidator());
    }

    private static UpdateTicketCommand CreateValidCommand(
        Guid ticketId,
        Guid categoryId)
    {
        return new UpdateTicketCommand(
            ticketId,
            categoryId,
            "Updated ticket title",
            "Updated ticket description.",
            TicketPriority.High);
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

    private static Category CreateCategory(
        string name = "Access",
        string description =
            "Authentication and permission problems.")
    {
        return new Category(
            name,
            description);
    }

    private static Ticket CreateTicket(
        Company company,
        User requester,
        Category category)
    {
        return new Ticket(
            company.Id,
            category.Id,
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

        public int UserIdReadCount { get; private set; }

        public Guid UserId
        {
            get
            {
                UserIdReadCount++;
                return _userId;
            }
        }

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

    private sealed class CompanyRepositorySpy : ICompanyRepository
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

    private sealed class CategoryRepositorySpy : ICategoryRepository
    {
        public Category? Category { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<Category?> GetByIdAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(Category);
        }

        public Task<IReadOnlyList<Category>> ListActiveAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Category>>(
                Array.Empty<Category>());
        }
    }

    private sealed class TicketRepositorySpy : ITicketRepository
    {
        public Ticket? Ticket { get; init; }

        public int GetForUpdateCallCount { get; private set; }

        public Task<Ticket?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Ticket?>(null);
        }

        public Task<Ticket?> GetForUpdateAsync(
            Guid ticketId,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            GetForUpdateCallCount++;

            return Task.FromResult(Ticket);
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
