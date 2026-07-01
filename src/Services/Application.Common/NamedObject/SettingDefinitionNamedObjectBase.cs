using Application.Common.Abstractions.NamedObjects;

namespace Application.Common.NamedObject;

public abstract class SettingDefinitionNamedObjectBase : LocalizableNameObject
{
    public abstract Type InputSettingType { get; }
    public abstract Type OutputSettingType { get; }

    public abstract Task UpdateSettingAsync(
        string json,
        CancellationToken cancellationToken);
    
    public abstract Task<string> GetOutputJsonAsync(CancellationToken cancellationToken);
}
