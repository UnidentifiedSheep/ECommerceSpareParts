using Pricing.Entities;

namespace Pricing.Abstractions.Interfaces.DbRepositories;

public interface ISettingsRepository
{
    Task<List<Setting>> GetSettings(CancellationToken ct = default);
    Task<Setting?> GetSetting(string key, bool track = true, CancellationToken ct = default);
}