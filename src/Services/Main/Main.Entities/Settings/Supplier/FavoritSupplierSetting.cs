using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Main.Entities.Settings.Supplier;

public class FavoritSupplierSetting : Setting<FavoritSupplierSettingData>, ISetting<FavoritSupplierSetting>
{
    public FavoritSupplierSetting(string json) : base(SettingName, json) { }

    public FavoritSupplierSetting(FavoritSupplierSettingData data) : base(SettingName, data) { }

    public static string SettingName => "FavoritSupplierSetting";
    public static FavoritSupplierSetting Default => new(new FavoritSupplierSettingData());
}

public record FavoritSupplierSettingData
{
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; init; } = false;

    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; init; }

    [JsonPropertyName("encryptedApiKey")]
    public string? EncryptedApiKey { get; init; }
}