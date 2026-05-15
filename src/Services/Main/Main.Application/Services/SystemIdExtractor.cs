using Abstractions.Interfaces;
using Application.Common.Interfaces.Settings;
using Main.Entities.Setting;

namespace Main.Application.Services;

public class SystemIdExtractor(
    ISettingsService settingsService) : ISystemIdExtractor
{
    public async Task<Guid> ExtractSystemId(CancellationToken cancellationToken = default)
    {
        return (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;
    }
}