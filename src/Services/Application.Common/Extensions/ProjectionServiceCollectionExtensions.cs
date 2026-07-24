using System.Reflection;
using Application.Common.Interfaces.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class ProjectionServiceCollectionExtensions
{
    private static readonly Type ProjectionProviderType = typeof(IProjectionProvider<,>);
    private static readonly Type SingletonProviderType = typeof(ISingletonProjectionProvider<,>);
    private static readonly Type ScopedProviderType = typeof(IScopedProjectionProvider<,>);

    public static IServiceCollection RegisterProjectionProviders(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly is required.", nameof(assemblies));

        var distinctAssemblies = assemblies.Distinct().ToArray();
        ValidateProviders(distinctAssemblies);

        services.Scan(scan => scan
            .FromAssemblies(distinctAssemblies)
            .AddClasses(classes => classes.Where(type =>
                Implements(type, SingletonProviderType)))
            .As(GetProjectionInterfaces)
            .WithSingletonLifetime());

        services.Scan(scan => scan
            .FromAssemblies(distinctAssemblies)
            .AddClasses(classes => classes.Where(type =>
                Implements(type, ScopedProviderType)))
            .As(GetProjectionInterfaces)
            .WithScopedLifetime());

        return services;
    }

    private static void ValidateProviders(IEnumerable<Assembly> assemblies)
    {
        var providers = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is { IsAbstract: false, IsClass: true } &&
                Implements(type, ProjectionProviderType))
            .ToList();

        foreach (var provider in providers)
        {
            var lifetimeMarkers = new[]
                {
                    Implements(provider, SingletonProviderType),
                    Implements(provider, ScopedProviderType)
                }
                .Count(value => value);

            if (lifetimeMarkers != 1)
                throw new InvalidOperationException(
                    $"Projection provider {provider.FullName} must implement exactly one lifetime marker.");
        }

        var duplicate = providers
            .SelectMany(provider => GetProjectionInterfaces(provider)
                .Select(service => new { Service = service, Provider = provider }))
            .GroupBy(item => item.Service)
            .FirstOrDefault(group => group.Select(item => item.Provider).Distinct().Count() > 1);

        if (duplicate is null) return;

        var implementations = string.Join(
            ", ",
            duplicate.Select(item => item.Provider.FullName).Distinct());

        throw new InvalidOperationException(
            $"Multiple projection providers are registered for {duplicate.Key}: {implementations}.");
    }

    private static IEnumerable<Type> GetProjectionInterfaces(Type type)
    {
        return type.GetInterfaces().Where(@interface =>
            @interface.IsGenericType &&
            @interface.GetGenericTypeDefinition() == ProjectionProviderType);
    }

    private static bool Implements(Type type, Type genericInterface)
    {
        return type.GetInterfaces().Any(@interface =>
            @interface.IsGenericType &&
            @interface.GetGenericTypeDefinition() == genericInterface);
    }
}
