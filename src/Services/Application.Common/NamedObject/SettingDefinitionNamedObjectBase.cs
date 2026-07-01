using Application.Common.Abstractions.NamedObjects;
using Application.Common.Interfaces.Settings;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Application.Common.NamedObject;

public abstract class SettingDefinitionNamedObjectBase<TSetting>(
    ISettingsService settingsService
    ) : SettingDefinitionNamedObjectBase 
    where TSetting : Setting, ISetting<TSetting>
{
    protected ISettingsService SettingsService => settingsService;
    public override async Task<Setting> GetSettingAsync(CancellationToken cancellationToken)
        => await settingsService.GetOrDefault<TSetting>(cancellationToken);
}

public abstract class SettingDefinitionNamedObjectBase : LocalizableNameObject
{
    public abstract Type InputSettingType { get; }
    public abstract Type OutputSettingType { get; }

    public abstract Task UpdateSettingAsync(
        string json,
        CancellationToken cancellationToken);
    
    public abstract Task<string> GetOutputJsonAsync(CancellationToken cancellationToken);
    public abstract Task<Setting> GetSettingAsync(CancellationToken cancellationToken);
}
