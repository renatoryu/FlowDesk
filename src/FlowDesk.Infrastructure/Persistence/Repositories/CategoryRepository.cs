using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository
    : ICategoryRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public CategoryRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Category?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                category => category.Id == categoryId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }
}
