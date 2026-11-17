using System.Reflection;
using Abstractions.Interfaces;
using Application.Common.Backplane;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchemaGeneration.Extensions;
using ZiggyCreatures.Caching.Fusion.Backplane;
using ProjectJsonOptions = Abstractions.Models.Options.ProjectJsonOptions;

namespace Application.Common;

public static class ServiceProvider
{
	public static IServiceCollection AddApplicationBase(
		this IServiceCollection services,
		IServiceDefinition serviceDefinition,
		IConfiguration? configuration,
		Assembly? assembly = null,
		params Type[] behaviorsToExclude)
	{
		assembly ??= Assembly.GetExecutingAssembly();
		services
			.AddSchemaGeneration()
			.AddCqrsMetrics()
			.RegisterIdCollector()
			.RegisterIntegrationEventScope()
			.RegisterDomainEventScope()
			.RegisterCachePolicies(assembly)
			.RegisterDbValidations(assembly)
			.RegisterFluentValidations(assembly);

		services.AddSingleton<IBackplaneDispatcher, BackplaneDispatcher>();
		services.AddSingleton<IFusionCacheBackplane, MassTransitBackplane>();
		services.TryAddSingleton<ProjectJsonOptions>();
		services.TryAddSingleton<IJsonSerializer, ProjectJsonSerializer>();
		services.AddSingleton(serviceDefinition);

		var hs = behaviorsToExclude.ToHashSet();
		services.AddMediatR(config =>
		{
			var licenseKey = configuration?.GetValue<string>("MediatR:LicenseKey");
			if (!string.IsNullOrWhiteSpace(licenseKey))
				config.LicenseKey = licenseKey;

			config.RegisterServicesFromAssembly(assembly);
			config
				.RegisterIfNotExcluded(hs, typeof(MetricsBehavior<,>))
				.RegisterIfNotExcluded(hs, typeof(ValidationBehavior<,>))
				.RegisterIfNotExcluded(
					hs,
					typeof(DbValidationBehavior<,>),
					ServiceLifetime.Scoped)
				.RegisterIfNotExcluded(hs, typeof(CacheBehavior<,>))
				.RegisterIfNotExcluded(
					hs,
					typeof(ApplicationTransactionBehavior<,>),
					ServiceLifetime.Scoped);
		});

		return services;
	}

	private static MediatRServiceConfiguration RegisterIfNotExcluded(
		this MediatRServiceConfiguration serviceConfiguration,
		HashSet<Type> excludedTypes,
		Type openBehaviorType,
		ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
	{
		if (excludedTypes.Contains(openBehaviorType))
			return serviceConfiguration;
		serviceConfiguration.AddOpenBehavior(openBehaviorType, serviceLifetime);
		return serviceConfiguration;
	}
}
