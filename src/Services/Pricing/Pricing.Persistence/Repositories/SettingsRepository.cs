using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using Pricing.Abstractions.Interfaces.DbRepositories;
using Pricing.Entities;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence.Repositories;

public class SettingsRepository(DContext context) : ISettingsRepository
{
    public async Task<List<Setting>> GetSettings(CancellationToken ct = default)
    {
        return await context.Settings.ToListAsync(ct);
    }

    public async Task<Setting?> GetSetting(string key, bool track = true, CancellationToken ct = default)
    {
        return await context.Settings.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Key == key, ct);
    }
}