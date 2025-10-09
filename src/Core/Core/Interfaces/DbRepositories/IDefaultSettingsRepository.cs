using Core.Entities;
using Core.Models;

namespace Core.Interfaces.DbRepositories;

public interface IDefaultSettingsRepository
{
    Task CreateDefaultSettingsIfNotExist(CancellationToken cancellationToken = default);
    Task<DefaultSettings> GetDefaultSettingsAsync(CancellationToken cancellationToken = default);

    Task<DefaultSetting?> GetSettingForUpdateAsync(string key, bool track = true,
        CancellationToken cancellationToken = default);
}