using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Categories.List;

public sealed class ListCategoriesHandler
{
    private readonly ICategoryRepository _categoryRepository;

    public ListCategoriesHandler(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<ListCategoryResult>> HandleAsync(
        ListCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Category> categories =
            await _categoryRepository.ListActiveAsync(
                cancellationToken);

        return categories
            .Select(category => new ListCategoryResult(
                category.Id,
                category.Name,
                category.Description))
            .ToArray();
    }
}
