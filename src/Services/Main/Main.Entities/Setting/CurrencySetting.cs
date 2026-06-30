using System.Text.Json.Serialization;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Setting;

public class CurrencySetting : Setting<CurrencySettingData>, ISetting<CurrencySetting>
{
    public CurrencySetting(string json) : base(SettingName, json)
    {
    }

    public CurrencySetting(CurrencySettingData data) : base(SettingName, data)
    {
    }

    public static string SettingName => "CurrencySetting";
    public static CurrencySetting Default => new(new CurrencySettingData());
}

public record CurrencySettingData
{
    [JsonPropertyName("baseCurrencyId")]
    public int BaseCurrencyId { get; init; } = 1;
    
    [JsonPropertyName("rateProvider")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExchangeRateProvider RateProvider { get; init; } = ExchangeRateProvider.Cbr;
}
