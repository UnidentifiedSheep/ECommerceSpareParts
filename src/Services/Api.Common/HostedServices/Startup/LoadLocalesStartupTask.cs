using System.Reflection;
using Api.Common.Extensions;
using Application.Common.Interfaces;
using Localization.Abstractions.Interfaces;
using Localization.Domain;
using Localization.Domain.Extensions;

namespace Api.Common.HostedServices.Startup;

public class LoadLocalesStartupTask(
    IEnumerable<ILocalizerContainer> containers) : IStartupTask
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var path = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var loader = new JsonLocalizerContainerLoader(path);
        await loader.LoadAsync(containers);
    }
}