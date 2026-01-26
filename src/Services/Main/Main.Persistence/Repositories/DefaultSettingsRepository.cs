using Core.StaticFunctions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class DefaultSettingsRepository(DContext context) : IDefaultSettingsRepository
{
    public async Task CreateDefaultSettingsIfNotExist(CancellationToken cancellationToken = default)
    {
        await context.Database.ExecuteSqlRawAsync("""
                                                  INSERT INTO default_settings (key, value) 
                                                  VALUES ('DefaultCurrency', '1'), ('DefaultMarkUp', '25'),
                                                         ('MaximumDaysOfPriceStorage', '30'), ('SelectedMarkupId', '-1'),
                                                         ('PriceGenerationStrategy', 'TakeHighestPrice'),
                                                         ('UseOrderAutoApprovement', 'false')
                                                  ON CONFLICT (key) DO NOTHING;
                                                  """, cancellationToken);
    }

    public async Task<Settings> GetDefaultSettingsAsync(CancellationToken cancellationToken = default)
    {
        var kvp = await context.DefaultSettings.AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);
        var settings = new Settings();
        settings.SetupValuesViaNames(kvp);
        return settings;
    }

    public async Task<DefaultSetting?> GetSettingForUpdateAsync(string key, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.DefaultSettings.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }
}