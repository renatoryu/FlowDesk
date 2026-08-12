using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task ApplyDatabaseMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope =
            services.CreateAsyncScope();

        FlowDeskDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<FlowDeskDbContext>();

        await dbContext.Database.MigrateAsync(
            cancellationToken);
    }
}
