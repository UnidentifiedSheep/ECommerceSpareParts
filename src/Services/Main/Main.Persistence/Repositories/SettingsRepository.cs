using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class SettingsRepository(DContext context) : ISettingsRepository
{
    public async Task<List<DefaultSetting>> GetSettings(CancellationToken ct = default)
    {
        return await context.DefaultSettings.ToListAsync(ct);
    }

    public async Task<DefaultSetting?> GetSetting(string key, bool track = true, CancellationToken ct = default)
    {
        return await context.DefaultSettings.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Key == key, ct);
    }
}