using Abstractions.Interfaces.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Localization.Extensions;

public static class HostExtensions
{
    public static async Task<IHost> LoadLocalesFromJson(this IHost host, string path)
    {
        using var sp = host.Services.CreateScope();

        var loader = new JsonLocalizerContainerLoader(path);
        IEnumerable<ILocalizerContainer> containers = sp.ServiceProvider.GetServices<ILocalizerContainer>();
        await loader.LoadAsync(containers);
        
        return host;
    }
}