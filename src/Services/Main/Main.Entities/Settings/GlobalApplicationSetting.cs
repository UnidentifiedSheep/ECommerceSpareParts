using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Entities.Settings;

public class GlobalApplicationSetting : Setting<GlobalApplicationSettingData>,
	ISetting<GlobalApplicationSetting>
{
	public GlobalApplicationSetting(string json) : base(SettingName, json)
	{
	}

	public GlobalApplicationSetting(GlobalApplicationSettingData data) : base(SettingName, data)
	{
	}

	public static string SettingName => "GlobalApplicationSetting";

	public static GlobalApplicationSetting Default => new(new GlobalApplicationSettingData());
}

public record GlobalApplicationSettingData
{
	[JsonPropertyName("apiServiceUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(GlobalApplicationSettingApiServiceUrlNameMessage.Key)]
	[SchemaFieldDescription(GlobalApplicationSettingApiServiceUrlDescriptionMessage.Key)]
	public string? ApiServiceUrl { get; init; }

	[JsonPropertyName("appServiceUrl")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel(GlobalApplicationSettingAppServiceUrlNameMessage.Key)]
	[SchemaFieldDescription(GlobalApplicationSettingAppServiceUrlDescriptionMessage.Key)]
	public string? AppServiceUrl { get; init; }
}
