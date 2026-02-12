using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface ISettingsRepository
{
    Task<List<DefaultSetting>> GetSettings(CancellationToken ct = default);
    Task<DefaultSetting?> GetSetting(string key, bool track = true, CancellationToken ct = default);
}