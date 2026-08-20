using Microsoft.Extensions.DependencyInjection;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Generators;

namespace SchemaGeneration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchemaGeneration(this IServiceCollection services)
    {
        services.AddScoped<ISchemaLocalizer, SchemaLocalizer>();

        services.AddKeyedSingleton<ISchemaGenerator, ReflectionSchemaGenerator>(SchemaGeneratorKind.Raw);
        services.AddKeyedScoped<ISchemaGenerator>(
            SchemaGeneratorKind.Localized,
            (provider, _) => new LocalizedSchemaGenerator(
                provider.GetRequiredKeyedService<ISchemaGenerator>(SchemaGeneratorKind.Raw),
                provider.GetRequiredService<ISchemaLocalizer>()));

        services.AddScoped<ISchemaGenerator>(provider =>
            provider.GetRequiredKeyedService<ISchemaGenerator>(SchemaGeneratorKind.Localized));

        return services;
    }
}
