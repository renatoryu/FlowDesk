using FlowDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Infrastructure.Persistence.Repositories;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Infrastructure.Security;

namespace FlowDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<FlowDeskDbContext>(
            options => options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<FlowDeskDbContext>());

        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        return services;
    }
}
