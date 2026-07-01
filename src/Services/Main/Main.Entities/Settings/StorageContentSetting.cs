using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Settings;

public class StorageContentSetting : Setting<StorageContentSettingData>, ISetting<StorageContentSetting>
{
    public StorageContentSetting(string json) : base(SettingName, json) { }

    public StorageContentSetting(StorageContentSettingData data) : base(SettingName, data) { }

    public static string SettingName => "StorageContentSetting";
    public static StorageContentSetting Default => new(new StorageContentSettingData());
}

public record StorageContentSettingData
{
    [JsonPropertyName("storageContentExtractionPolicy")]
    [RequiredJsonField]
    [InputControl(InputControlType.NamedObjectSelector)]
    [DependsOnEntity("StorageContentExtractPolicy")]
    [LocalizedJsonFieldName("storage.content.setting.extraction.policy.name")]
    [LocalizedJsonFieldDescription("storage.content.setting.extraction.policy.description")]
    public string StorageContentExtractionPolicy { get; init; } = "FifoStorageContentExtractPolicy";
}