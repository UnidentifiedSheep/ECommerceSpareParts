using Persistence.Context;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Main.Modules.Jobs.Persistence.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Common;
using Persistence.Extensions;

namespace Main.Modules.Jobs.Persistence;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddJobsPersistence(this IServiceCollection services)
		=> services.AddDbContextModule<JobsDbContextModule>().AddEntities(typeof(Domain.CommonEntities.Job.JobSchedule).Assembly).AddJobRepositories<UnifiedDbContext>();

	public static IServiceCollection AddJobRepositories<TContext>(this IServiceCollection services)
		where TContext : DbContext
	{
		services.AddScoped<PendingUniqueJobFilter<TContext>>();
		services.AddScoped<IJobRepository, JobRepository<TContext>>();
		services.AddScoped<IRepository<Job, Guid>>(sp => sp.GetRequiredService<IJobRepository>());

		return services;
	}
}
