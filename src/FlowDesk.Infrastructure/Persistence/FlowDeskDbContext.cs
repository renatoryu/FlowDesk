using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FlowDesk.Application.Abstractions.Persistence;

namespace FlowDesk.Infrastructure.Persistence;

public sealed class FlowDeskDbContext : DbContext, IUnitOfWork
{
    public FlowDeskDbContext(
        DbContextOptions<FlowDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FlowDeskDbContext).Assembly);
    }
}
