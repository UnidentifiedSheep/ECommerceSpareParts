using Abstractions.Interfaces.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterTestContexts(this IServiceCollection services)
    {
        var interfaceType = typeof(ITestContext);

        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t) 
                        && t is { IsInterface: false, IsAbstract: false });

        foreach (var impl in implementations)
            services.AddScoped(impl);

        return services;
    }
}