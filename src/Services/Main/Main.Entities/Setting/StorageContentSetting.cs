using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Main.Entities.Setting;

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
    public string StorageContentExtractionPolicy { get; init; } = "FifoStorageContentExtractPolicy";
}
