using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Entities.Settings;

public class GlobalApplicationSetting : Setting<GlobalApplicationSettingData>,
    ISetting<GlobalApplicationSetting>
{
    public GlobalApplicationSetting(string json) : base(SettingName, json) { }

    public GlobalApplicationSetting(GlobalApplicationSettingData data) : base(SettingName, data) { }

    public static string SettingName => "GlobalApplicationSetting";

    public static GlobalApplicationSetting Default => new(new GlobalApplicationSettingData());
}

public record GlobalApplicationSettingData
{
    [JsonPropertyName("apiServiceUrl")]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("global.application.setting.api.service.url.name")]
    [SchemaFieldDescription("global.application.setting.api.service.url.description")]
    public string? ApiServiceUrl { get; init; }

    [JsonPropertyName("appServiceUrl")]
    [SchemaInputControl(InputControlType.TextField)]
    [SchemaFieldLabel("global.application.setting.app.service.url.name")]
    [SchemaFieldDescription("global.application.setting.app.service.url.description")]
    public string? AppServiceUrl { get; init; }
}
