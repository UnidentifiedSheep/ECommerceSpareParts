using Main.Core.Entities;
using Main.Core.Models;

namespace Main.Core.Interfaces.DbRepositories;

public interface IDefaultSettingsRepository
{
    Task CreateDefaultSettingsIfNotExist(CancellationToken cancellationToken = default);
    Task<Settings> GetDefaultSettingsAsync(CancellationToken cancellationToken = default);

    Task<DefaultSetting?> GetSettingForUpdateAsync(string key, bool track = true,
        CancellationToken cancellationToken = default);
}