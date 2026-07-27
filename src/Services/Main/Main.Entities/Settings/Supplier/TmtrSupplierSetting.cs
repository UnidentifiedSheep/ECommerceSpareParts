using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Main.Entities.Settings.Supplier;

public class TmtrSupplierSetting : Setting<TmtrSupplierSettingData>, ISetting<TmtrSupplierSetting>
{
    public TmtrSupplierSetting(string json) : base(SettingName, json) { }

    public TmtrSupplierSetting(TmtrSupplierSettingData data) : base(SettingName, data) { }

    public static string SettingName => "TmtrSupplierSetting";
    public static TmtrSupplierSetting Default => new(new TmtrSupplierSettingData());
}

public record TmtrSupplierSettingData
{
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; init; }

    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; init; }

    [JsonPropertyName("guaranteedDeliveryOffsetDays")]
    public int GuaranteedDeliveryOffsetDays { get; init; } = 1;
    
    [JsonPropertyName("authData")]
    public TmtrAuthData? AuthData { get; init; }
}

public record TmtrAuthData
{
    [JsonPropertyName("login")]
    public required string Login { get; init; }
    
    [JsonPropertyName("encryptedPassword")]
    public required string EncryptedPassword { get; init; }
}