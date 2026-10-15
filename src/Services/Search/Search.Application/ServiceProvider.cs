using Abstractions;
using Application.Common;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Search.Abstractions.Options;
using Search.Application.Configs;
using Search.Application.Interfaces;
using Search.Application.Services.IndexSynchronizers;
using Search.Entities;

namespace Search.Application;

public static class ServiceProvider
{
	public static IServiceCollection AddApplicationLayer(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		SortByConfig.Configure();
		services.AddOptions<DeletedProductsCleanupOptions>()
			.Bind(configuration.GetSection(DeletedProductsCleanupOptions.SectionName))
			.Validate(settings => settings.RetentionDays is >= 0 and <= 36500, "RetentionDays must be between 0 and 36500.")
			.Validate(settings => settings.BatchSize > 0, "BatchSize must be positive.")
			.ValidateOnStart();
		services.TryAddSingleton(TimeProvider.System);

		services.AddApplicationBase(
			ServicesDefinitions.Search,
			configuration,
			typeof(ServiceProvider).Assembly,
			typeof(ApplicationTransactionBehavior<,>),
			typeof(DbValidationBehavior<,>),
			typeof(CacheBehavior<,>));

		services.RegisterProjectionProviders<ProducerIndexSynchronizer>();
		services.RegisterNamedObject<ILrtNamedObject>(typeof(ServiceProvider).Assembly);
		services.AddSingleton<IIndexSynchronizer<Producer, int>, ProducerIndexSynchronizer>();

		return services;
	}
}
