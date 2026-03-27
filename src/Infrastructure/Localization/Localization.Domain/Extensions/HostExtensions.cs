using Microsoft.Extensions.Hosting;

namespace Localization.Domain.Extensions;

public static class HostExtensions
{
    public static async Task<IHost> LoadLocalesFromJson(this IHost host, string path)
    {
        await host.Services.LoadLocalesFromJson(path);
        return host;
    }
}