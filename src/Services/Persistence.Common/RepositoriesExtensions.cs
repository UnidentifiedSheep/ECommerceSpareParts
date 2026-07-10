using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Common;

public static class RepositoriesExtensions
{
    public static IServiceCollection AddJobRepositories<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        return services.AddScoped<IJobRepository, JobRepository<TContext>>();
    }
}