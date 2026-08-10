using System.Data;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Application.Common.Services.Settings;

public class SettingsService(
    IRepository<Setting, string> repository,
    IApplicationTransactionService transactionService,
    ISettingsContainer settingsContainer,
    ISettingFactory settingFactory
) : ISettingsService
{
    private static readonly TransactionalAttribute TransactionSettings
        = new(
            IsolationLevel.ReadCommitted,
            20,
            3);

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<Setting>.New()
            .Track(false)
            .Build();

        var dbSettings = await repository.ListAsync(criteria, cancellationToken);
        foreach (var setting in dbSettings)
        {
            var typed = settingFactory.Create(setting.Key, setting.Json);
            settingsContainer.Set(typed);
        }
    }

    public async Task SetSetting<T>(
        T value,
        CancellationToken cancellationToken = default
    ) where T : Setting
    {
        await transactionService.ExecuteAsync(
            TransactionSettings,
            async (context, ct) =>
            {
                var criteria = Criteria<Setting>.New()
                    .Where(x => x.Key == value.Key)
                    .ForUpdate()
                    .Track()
                    .Build();

                var existing = await repository.FirstOrDefaultAsync(criteria, ct);
                existing?.SetData(value.Json);

                if (existing == null)
                    await context.UnitOfWork.AddAsync(value, ct);

                await context.UnitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);

        settingsContainer.Set(value);
    }

    public async Task<T> GetOrDefault<T>(CancellationToken cancellationToken = default)
        where T : Setting, ISetting<T>
    {
        if (settingsContainer.TryGet<T>(out var setting)) return setting!;

        var dbSetting = await repository.GetById(T.SettingName, cancellationToken);

        if (dbSetting != null)
        {
            var typed = (T)settingFactory.Create(dbSetting.Key, dbSetting.Json);
            settingsContainer.Set(typed);
            return typed;
        }

        await SetSetting(T.Default, cancellationToken);
        return T.Default;
    }
}
