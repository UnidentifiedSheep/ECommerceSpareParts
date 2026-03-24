using Localization.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Domain.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task<IServiceProvider> LoadLocalesFromJson(this IServiceProvider sp, string path)
    {
        using var scope = sp.CreateScope();

        var loader = new JsonLocalizerContainerLoader(path);
        IEnumerable<ILocalizerContainer> containers = scope.ServiceProvider.GetServices<ILocalizerContainer>();
        await loader.LoadAsync(containers);
        
        return sp;
    }
}