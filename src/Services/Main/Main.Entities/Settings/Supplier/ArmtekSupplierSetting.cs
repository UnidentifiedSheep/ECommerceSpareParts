using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Main.Entities.Settings.Supplier;

public class ArmtekSupplierSetting : Setting<ArmtekSupplierSettingData>, ISetting<ArmtekSupplierSetting>
{
    public ArmtekSupplierSetting(string json) : base(SettingName, json)
    {
    }

    public ArmtekSupplierSetting(ArmtekSupplierSettingData data) : base(SettingName, data)
    {
    }

    public static string SettingName => "ArmtekSupplierSetting";
    public static ArmtekSupplierSetting Default => new(new ArmtekSupplierSettingData());
}

public record ArmtekSupplierSettingData
{
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; init; } = false;
    
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; init; }
}
