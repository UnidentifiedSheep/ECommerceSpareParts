using Abstractions.Models.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Tests.Interfaces;
using Tests.Stubs;

namespace Tests.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterTestContexts(this IServiceCollection services)
    {
        var interfaceType = typeof(ITestContext);

        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t)
                        && t is { IsInterface: false, IsAbstract: false });

        foreach (var impl in implementations) services.AddScoped(impl);

        return services;
    }

    public static IServiceCollection AddSystemOptionsForTests(this IServiceCollection services)
    {
        services.RemoveAll<IOptions<SystemOptions>>();
        services.AddSingleton<TestSystemOptionsAccessor>();
        services.AddSingleton<IOptions<SystemOptions>, TestSystemOptions>();
        return services;
    }
}