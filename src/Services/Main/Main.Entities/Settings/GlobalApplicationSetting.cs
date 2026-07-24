using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;

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
    [JsonPropertyName("s3ServiceUrl")]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("global.application.setting.s3.service.url.name")]
    [LocalizedJsonFieldDescription("global.application.setting.s3.service.url.description")]
    public string? S3ServiceUrl { get; init; }

    [JsonPropertyName("apiServiceUrl")]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("global.application.setting.api.service.url.name")]
    [LocalizedJsonFieldDescription("global.application.setting.api.service.url.description")]
    public string? ApiServiceUrl { get; init; }

    [JsonPropertyName("appServiceUrl")]
    [InputControl(InputControlType.TextField)]
    [LocalizedJsonFieldName("global.application.setting.app.service.url.name")]
    [LocalizedJsonFieldDescription("global.application.setting.app.service.url.description")]
    public string? AppServiceUrl { get; init; }
}
