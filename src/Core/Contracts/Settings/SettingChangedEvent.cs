namespace Contracts.Settings;

public record SettingChangedEvent
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public required DateTime ChangedAt { get; init; }
}