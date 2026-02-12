namespace Contracts.Settings;

public record SettingChangedEvent
{
    public string Key { get; init; } = null!;
    public string? Value { get; init; }
    public DateTime ChangedAt { get; init; }
}

