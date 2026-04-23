using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Settings;
using Domain.CommonEntities;
using Domain.Interfaces;
using MassTransit;

namespace Application.Common.Services.Settings;

public class SettingsService(
    IRepository<Setting, string> repository,
    IUnitOfWork unitOfWork,
    ISettingsContainer settingsContainer,
    IPublishEndpoint publishEndpoint,
    ISettingFactory settingFactory)
    : ISettingsService
{
    private static readonly TransactionalAttribute TransactionSettings 
        = new(IsolationLevel.Snapshot, 20, 3);
    
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
        await unitOfWork.ExecuteWithTransaction(
            settings: TransactionSettings,
            action: async () =>
            {
                await unitOfWork.AddAsync(value, cancellationToken);
                await publishEndpoint.Publish(new SettingChangedEvent
                {
                    Key = value.Key,
                    Value = value.Json,
                    ChangedAt = DateTime.UtcNow,
                }, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken: cancellationToken);
        
        settingsContainer.Set(value);
    }

    public async Task<T> GetOrDefault<T>(CancellationToken cancellationToken = default) where T : Setting, ISetting<T>
    {
        if (settingsContainer.TryGet<T>(out var setting)) return setting!;
        var dbSetting = await repository.GetById(T.SettingName, cancellationToken);
        if (dbSetting == null) return T.Default;
        
        var typed = (T)settingFactory.Create(dbSetting.Key, dbSetting.Json);
        settingsContainer.Set(typed);

        return typed;
    }
}