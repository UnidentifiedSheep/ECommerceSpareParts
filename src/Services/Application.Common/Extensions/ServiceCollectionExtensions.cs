using System.Reflection;
using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.History;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Common.Services;
using Application.Common.Services.Events;
using Application.Common.Services.History;
using Application.Common.Services.Persistence;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection RegisterIdCollector(this IServiceCollection collection)
	{
		collection.AddScoped<IIdsCollector, IdsCollector>();
		return collection;
	}

	public static IServiceCollection RegisterCachePolicies(
		this IServiceCollection services,
		Assembly? assembly = null)
	{
		assembly ??= Assembly.GetExecutingAssembly();

		var types = assembly
			.GetTypes()
			.Where(t => t is { IsAbstract: false, IsInterface: false })
			.Select(t => new
			{
				Implementation = t,
				Interfaces = t
					.GetInterfaces()
					.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICachePolicy<>))
			})
			.Where(x => x.Interfaces.Any());

		foreach (var type in types)
		foreach (var @interface in type.Interfaces)
			services.AddScoped(@interface, type.Implementation);

		return services;
	}

	public static IServiceCollection RegisterDbValidations(
		this IServiceCollection services,
		Assembly? assembly = null)
	{
		assembly ??= Assembly.GetExecutingAssembly();
		var validationTypes = assembly
			.GetTypes()
			.Where(t => !t.IsAbstract && !t.IsInterface)
			.Where(t =>
				t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() ==
				typeof(AbstractDbValidation<>));

		foreach (var type in validationTypes)
		{
			var baseType = type.BaseType!;
			services.AddScoped(baseType, type);
		}

		return services;
	}

	public static IServiceCollection RegisterIntegrationEventScope(this IServiceCollection services)
	{
		services.AddScoped<IIntegrationEventScope, IntegrationEventScope>();
		return services;
	}

	public static IServiceCollection RegisterDomainEventScope(this IServiceCollection services)
	{
		services.AddScoped<IDomainEventScope, DomainEventScope>();
		services.AddScoped<IDomainEventExecutor, DomainEventExecutor>();
		services.AddScoped<Func<IDomainEventExecutor>>(provider
			=> provider.GetRequiredService<IDomainEventExecutor>);
		return services;
	}

	public static IServiceCollection RegisterApplicationTransactions(this IServiceCollection services)
	{
		services.AddScoped<IEntityHistoryContext, EntityHistoryContext>();
		services.AddScoped<IRepositoryProvider, RepositoryProvider>();
		services.AddScoped<IApplicationTransactionContext, ApplicationTransactionContext>();
		services.AddScoped<ApplicationTransaction>();
		services.AddScoped<IApplicationTransaction>(provider
			=> provider.GetRequiredService<ApplicationTransaction>());
		services.AddSingleton<IApplicationTransactionFactory, ApplicationTransactionFactory>();
		return services;
	}

	public static IServiceCollection RegisterFluentValidations(
		this IServiceCollection services,
		Assembly? assembly = null)
	{
		assembly ??= Assembly.GetExecutingAssembly();
		services.AddValidatorsFromAssembly(assembly);

		return services;
	}

	public static IServiceCollection AddCqrsMetrics(this IServiceCollection collection)
	{
		collection.AddMetrics();
		collection.AddSingleton<CqrsMetrics>();
		return collection;
	}
}
