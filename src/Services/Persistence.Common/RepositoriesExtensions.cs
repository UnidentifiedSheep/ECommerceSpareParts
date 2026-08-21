using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common.Jobs;

namespace Persistence.Common;

public static class RepositoriesExtensions
{
    public static IServiceCollection AddJobRepositories<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        services.AddScoped<PendingUniqueJobFilter<TContext>>();
        services.AddScoped<IJobRepository, JobRepository<TContext>>();

        return services;
    }
}
