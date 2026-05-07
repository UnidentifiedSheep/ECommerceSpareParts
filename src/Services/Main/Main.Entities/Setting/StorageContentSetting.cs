using Domain.CommonEntities;
using Domain.Interfaces;

namespace Main.Abstractions.Models.Settings;

public class StorageContentSetting : Setting<StorageContentSettingData>, ISetting<StorageContentSetting>
{
    public static string SettingName => "StorageContentSetting";
    public static StorageContentSetting Default { get; } = new(new StorageContentSettingData());
    public StorageContentSetting(string json) : base(SettingName, json) { }
    public StorageContentSetting(StorageContentSettingData data) : base(SettingName, data) { }
}

public record StorageContentSettingData
{
    public string StorageContentExtractionPolicy { get; init; } = "FifoStorageContentExtractPolicy";
}