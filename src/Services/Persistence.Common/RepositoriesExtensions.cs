using System.Reflection;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using BulkValidation.Core.Configuration;
using BulkValidation.Pgsql.Extensions;
using Domain;
using Domain.CommonEntities.Job;
using Domain.Interfaces;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
		params Assembly[] entitiesAssemblies) where TContext : DbContext
		=> services.AddPersistenceBase<TContext>(
			_ => { },
			repositoryType,
			linqRepositoryType,
			readRepositoryType,
			entitiesAssemblies);

	public static IServiceCollection AddPersistenceBase<TContext>(
		this IServiceCollection services,
		Action<ValidationErrors> configureValidation,
		Type repositoryType,
		Type linqRepositoryType,
		Type readRepositoryType,
		params Assembly[] entitiesAssemblies) where TContext : DbContext
	{
		if (!services.Any(descriptor => descriptor.ServiceType == typeof(DbContextOptions<TContext>)))
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

					var databaseOptions =
						serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

					options.Connection(databaseOptions.ConnectionString);
					options.DatabaseSchemaName = "materialized";

					return options;
				})
				.UseLightweightSessions();

			services.AddScoped(typeof(IDocumentRepository<,>), typeof(DocumentRepository<,>));

			services.AddScoped(typeof(IDocumentReadRepository<,>), typeof(DocumentReadRepository<,>));
		}

		AddRepositories(
			services,
			repositoryType,
			linqRepositoryType,
			readRepositoryType,
			entitiesAssemblies);


		services.AddScoped(typeof(IReadRepository<,>), readRepositoryType);

		if (!services.Any(descriptor => descriptor.ServiceType == typeof(IUnitOfWork)))
			services.AddUnitOfWork<TContext>();

		services.AddScoped<IDbValidator, PgsqlDbValidator<TContext>>();
		services.AddPgsqlDbValidators<TContext>(configureValidation);

		return services;
	}

	private static void AddRepositories(
		IServiceCollection services,
		Type repositoryType,
		Type linqRepositoryType,
		Type readRepositoryType,
		Assembly[] entitiesAssemblies)
	{
		var registered = new HashSet<(Type EntityType, Type KeyType)>();

		foreach (var entitiesAssembly in entitiesAssemblies)
		foreach (var discoveredType in entitiesAssembly.GetTypes())
		{
			var entityBase = FindEntityBase(discoveredType);

			if (entityBase is null)
				continue;

			var genericArguments = entityBase.GetGenericArguments();

			var entityType = genericArguments[0];
			var keyType = genericArguments[1];

			if (!registered.Add((entityType, keyType)))
				continue;

			var linqEntityType = typeof(ILinqEntity<,>).MakeGenericType(entityType, keyType);

			var repositoryDefinition = linqEntityType.IsAssignableFrom(entityType)
				? linqRepositoryType
				: repositoryType;

			var serviceType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
			var implementationType = repositoryDefinition.MakeGenericType(entityType, keyType);

			services.AddScoped(serviceType, implementationType);
			services.AddScoped(
				typeof(IReadRepository<,>).MakeGenericType(entityType, keyType),
				readRepositoryType.MakeGenericType(entityType, keyType));
		}
	}

	private static Type? FindEntityBase(Type type)
	{
		for (var current = type; current is not null; current = current.BaseType)
		{
			if (!current.IsGenericType)
				continue;

			if (current.GetGenericTypeDefinition() == typeof(Entity<,>))
				return current;
		}

		return null;
	}
}
