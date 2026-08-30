using Abstractions.Interfaces;
using Application.Common.Interfaces.Repositories;
using BulkValidation.Pgsql.Extensions;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence.Common.Jobs;
using Persistence.DbValidator;
using Persistence.Extensions;
using Persistence.Interceptors;
using Persistence.Repository.Document;

namespace Persistence.Common;

public static class RepositoriesExtensions
{
	public static IServiceCollection AddPersistenceBase<TContext>(
		this IServiceCollection services,
		Type repositoryType,
		Type readRepositoryType) where TContext : DbContext
	{
		services.AddScoped<AuditableEntitySaveChangesInterceptor>();
		services.AddScoped<DomainEventFlusherSaveChangesInterceptor>();

		services.AddDbContext<TContext>((serviceProvider, options) =>
		{
			var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

			options.UseNpgsql(databaseOptions.ConnectionString);
			options.AddInterceptors(
				serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
			options.AddInterceptors(
				serviceProvider.GetRequiredService<DomainEventFlusherSaveChangesInterceptor>());
		});

		services
			.AddMarten(serviceProvider =>
			{
				var options = new StoreOptions();

				var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

				options.Connection(databaseOptions.ConnectionString);

				options.DatabaseSchemaName = "materialized";
				return options;
			})
			.UseLightweightSessions();

		services.AddScoped(typeof(IDocumentRepository<,>), typeof(DocumentRepository<,>));
		services.AddScoped(typeof(IDocumentReadRepository<,>), typeof(DocumentReadRepository<,>));

		services.AddScoped(typeof(IRepository<,>), repositoryType);
		services.AddScoped(typeof(IReadRepository<,>), readRepositoryType);

		services.AddJobRepositories<TContext>();
		services.AddUnitOfWork<TContext>();

		services.AddScoped<IDbValidator, PgsqlDbValidator<TContext>>();
		services.AddPgsqlDbValidators<TContext>();

		return services;
	}

	public static IServiceCollection AddJobRepositories<TContext>(this IServiceCollection services)
		where TContext : DbContext
	{
		services.AddScoped<PendingUniqueJobFilter<TContext>>();
		services.AddScoped<IJobRepository, JobRepository<TContext>>();

		return services;
	}
}
