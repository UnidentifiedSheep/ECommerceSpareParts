using System.Reflection;
using Abstractions.Interfaces;
using Application.Common.Interfaces.Repositories;
using BulkValidation.Pgsql.Extensions;
using Domain;
using Domain.CommonEntities.Job;
using Domain.Interfaces;
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
		Type linqRepositoryType,
		Type readRepositoryType,
		Assembly entitiesAssembly)
		where TContext : DbContext
	{
		services.AddScoped<AuditableEntitySaveChangesInterceptor>();
		services.AddScoped<DomainEventFlusherSaveChangesInterceptor>();

		services.AddDbContext<TContext>((serviceProvider, options) =>
		{
			var databaseOptions = serviceProvider
				.GetRequiredService<IOptions<DatabaseOptions>>()
				.Value;

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

				var databaseOptions = serviceProvider
					.GetRequiredService<IOptions<DatabaseOptions>>()
					.Value;

				options.Connection(databaseOptions.ConnectionString);
				options.DatabaseSchemaName = "materialized";

				return options;
			})
			.UseLightweightSessions();

		services.AddScoped(
			typeof(IDocumentRepository<,>),
			typeof(DocumentRepository<,>));

		services.AddScoped(
			typeof(IDocumentReadRepository<,>),
			typeof(DocumentReadRepository<,>));

		AddRepositories(
			services,
			repositoryType,
			linqRepositoryType,
			entitiesAssembly);

		services.AddJobRepositories<TContext>();

		services.AddScoped(
			typeof(IReadRepository<,>),
			readRepositoryType);

		services.AddUnitOfWork<TContext>();

		services.AddScoped<IDbValidator, PgsqlDbValidator<TContext>>();
		services.AddPgsqlDbValidators<TContext>();

		return services;
	}

	private static void AddRepositories(
		IServiceCollection services,
		Type repositoryType,
		Type linqRepositoryType,
		Assembly entitiesAssembly)
	{
		var registered = new HashSet<(Type EntityType, Type KeyType)>();

		foreach (var discoveredType in entitiesAssembly.GetTypes())
		{
			var entityBase = FindEntityBase(discoveredType);

			if (entityBase is null) continue;

			var genericArguments = entityBase.GetGenericArguments();

			var entityType = genericArguments[0];
			var keyType = genericArguments[1];

			if (!registered.Add((entityType, keyType))) continue;

			var linqEntityType = typeof(ILinqEntity<,>)
				.MakeGenericType(entityType, keyType);

			var repositoryDefinition = linqEntityType.IsAssignableFrom(entityType)
				? linqRepositoryType
				: repositoryType;

			var serviceType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
			var implementationType = repositoryDefinition.MakeGenericType(entityType, keyType);

			services.AddScoped(serviceType, implementationType);
		}
	}

	private static Type? FindEntityBase(Type type)
	{
		for (var current = type; current is not null; current = current.BaseType)
		{
			if (!current.IsGenericType) continue;

			if (current.GetGenericTypeDefinition() == typeof(Entity<,>))
				return current;
		}

		return null;
	}

	public static IServiceCollection AddJobRepositories<TContext>(
		this IServiceCollection services)
		where TContext : DbContext
	{
		services.AddScoped<PendingUniqueJobFilter<TContext>>();
		services.AddScoped<IJobRepository, JobRepository<TContext>>();
		services.AddScoped<IRepository<Job, Guid>>(sp => sp.GetRequiredService<IJobRepository>());

		return services;
	}
}
