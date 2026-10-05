using Microsoft.Extensions.DependencyInjection;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Generators;

namespace SchemaGeneration.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSchemaGeneration(this IServiceCollection services)
	{
		services.AddSingleton<ISchemaLocalizer, SchemaLocalizer>();

		services.AddKeyedSingleton<ISchemaGenerator, ReflectionSchemaGenerator>(SchemaGeneratorKind.Raw);
		services.AddKeyedSingleton<ISchemaGenerator>(
			SchemaGeneratorKind.Localized,
			(provider, _) => new LocalizedSchemaGenerator(
				provider.GetRequiredKeyedService<ISchemaGenerator>(SchemaGeneratorKind.Raw),
				provider.GetRequiredService<ISchemaLocalizer>()));

		services.AddSingleton<ISchemaGenerator>(provider =>
			provider.GetRequiredKeyedService<ISchemaGenerator>(SchemaGeneratorKind.Localized));

		return services;
	}
}
