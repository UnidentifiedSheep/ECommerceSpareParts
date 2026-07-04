using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Settings;

public class CurrencySetting : Setting<CurrencySettingData>, ISetting<CurrencySetting>
{
    public CurrencySetting(string json) : base(SettingName, json) { }

    public CurrencySetting(CurrencySettingData data) : base(SettingName, data) { }

    public static string SettingName => "CurrencySetting";
    public static CurrencySetting Default => new(new CurrencySettingData());
}

public record CurrencySettingData
{
    [JsonPropertyName("baseCurrencyId")]
    [RequiredJsonField]
    [InputControl(InputControlType.EntitySelector)]
    [DependsOnEntity("Currency")]
    [LocalizedJsonFieldName("currency.setting.base.currency.name")]
    [LocalizedJsonFieldDescription("currency.setting.base.currency.description")]
    public int BaseCurrencyId { get; init; } = 1;

    [JsonPropertyName("rateProvider")]
    [RequiredJsonField]
    [InputControl(InputControlType.EnumSelector)]
    [DependsOnEntity(nameof(ExchangeRateProvider))]
    [LocalizedJsonFieldName("currency.setting.rate.provider.name")]
    [LocalizedJsonFieldDescription("currency.setting.rate.provider.description")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExchangeRateProvider RateProvider { get; init; } = ExchangeRateProvider.Cbr;
}