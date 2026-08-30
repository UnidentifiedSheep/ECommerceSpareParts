using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Entities.Settings;

public class StorageContentSetting : Setting<StorageContentSettingData>, ISetting<StorageContentSetting>
{
	public StorageContentSetting(string json) : base(SettingName, json)
	{
	}

	public StorageContentSetting(StorageContentSettingData data) : base(SettingName, data)
	{
	}

	public static string SettingName => "StorageContentSetting";

	public static StorageContentSetting Default => new(new StorageContentSettingData());
}

public record StorageContentSettingData
{
	[JsonPropertyName("storageContentExtractionPolicy")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.NamedObjectSelector)]
	[SchemaDependsOnEntity("StorageContentExtractPolicy")]
	[SchemaFieldLabel("storage.content.setting.extraction.policy.name")]
	[SchemaFieldDescription("storage.content.setting.extraction.policy.description")]
	public string StorageContentExtractionPolicy { get; init; } = "FifoStorageContentExtractPolicy";
}
