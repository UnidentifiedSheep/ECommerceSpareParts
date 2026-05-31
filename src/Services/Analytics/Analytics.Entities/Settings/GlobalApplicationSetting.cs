using Domain.CommonEntities;
using Domain.Interfaces;

namespace Analytics.Entities.Settings;

public class GlobalApplicationSetting : Setting<GlobalApplicationSettingData>, ISetting<GlobalApplicationSetting>
{
    public GlobalApplicationSetting(string json) : base(SettingName, json)
    {
    }

    public GlobalApplicationSetting(GlobalApplicationSettingData data) : base(SettingName, data)
    {
    }

    public static string SettingName => "GlobalApplicationSetting";

    public static GlobalApplicationSetting Default
        => throw new InvalidOperationException("Global application settings must be initialized");
}

public record GlobalApplicationSettingData
{
    public required Guid SystemId { get; init; }
}