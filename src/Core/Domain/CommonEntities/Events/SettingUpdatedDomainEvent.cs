using Domain.Interfaces.Events;

namespace Domain.CommonEntities.Events;

public sealed record SettingUpdatedDomainEvent(
    string Key,
    string Value,
    DateTime ChangedAt) : IKeyedDomainEvent, IBatchableDomainEvent
{
    public string GetKey() => $"setting-updated:{Key}";
}
