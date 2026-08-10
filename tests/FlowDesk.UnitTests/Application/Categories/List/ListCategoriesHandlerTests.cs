using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Categories.List;
using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Application.Categories.List;

public sealed class ListCategoriesHandlerTests
{
    [Fact]
    public async Task HandleAsyncMapsActiveCategories()
    {
        Category access = new Category(
            "Access",
            "Authentication problems.");

        Category software = new Category(
            "Software",
            "Application problems.");

        var repository = new CategoryRepositoryStub(
            new[] { access, software });

        var handler = new ListCategoriesHandler(
            repository);

        IReadOnlyList<ListCategoryResult> result =
            await handler.HandleAsync(
                new ListCategoriesQuery());

        Assert.Equal(1, repository.ListActiveCallCount);
        Assert.Equal(2, result.Count);

        Assert.Equal(access.Id, result[0].Id);
        Assert.Equal(access.Name, result[0].Name);
        Assert.Equal(
            access.Description,
            result[0].Description);

        Assert.Equal(software.Id, result[1].Id);
        Assert.Equal(software.Name, result[1].Name);
    }

    [Fact]
    public async Task HandleAsyncWithNoCategoriesReturnsEmptyList()
    {
        var repository = new CategoryRepositoryStub(
            Array.Empty<Category>());

        var handler = new ListCategoriesHandler(
            repository);

        IReadOnlyList<ListCategoryResult> result =
            await handler.HandleAsync(
                new ListCategoriesQuery());

        Assert.Equal(1, repository.ListActiveCallCount);
        Assert.Empty(result);
    }

    private sealed class CategoryRepositoryStub
        : ICategoryRepository
    {
        private readonly IReadOnlyList<Category> _categories;

        public CategoryRepositoryStub(
            IReadOnlyList<Category> categories)
        {
            _categories = categories;
        }

        public int ListActiveCallCount { get; private set; }

        public Task<Category?> GetByIdAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Category?>(null);
        }

        public Task<IReadOnlyList<Category>> ListActiveAsync(
            CancellationToken cancellationToken = default)
        {
            ListActiveCallCount++;

            return Task.FromResult(_categories);
        }
    }
}
