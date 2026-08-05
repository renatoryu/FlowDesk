using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence;

public sealed class FlowDeskDbContext : DbContext, IUnitOfWork
{
    public FlowDeskDbContext(
        DbContextOptions<FlowDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictException(
                "The operation conflicted with another request.",
                exception);
        }
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FlowDeskDbContext).Assembly);
    }
}
