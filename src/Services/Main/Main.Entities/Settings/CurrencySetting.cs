using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;
using SchemaGeneration.Abstractions.Enums;

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
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.EntitySelector)]
    [SchemaDependsOnEntity("Currency")]
    [SchemaFieldLabel("currency.setting.base.currency.name")]
    [SchemaFieldDescription("currency.setting.base.currency.description")]
    public int BaseCurrencyId { get; init; } = 1;

    [JsonPropertyName("rateProvider")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.EnumSelector)]
    [SchemaDependsOnEntity(nameof(ExchangeRateProvider))]
    [SchemaFieldLabel("currency.setting.rate.provider.name")]
    [SchemaFieldDescription("currency.setting.rate.provider.description")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExchangeRateProvider RateProvider { get; init; } = ExchangeRateProvider.Cbr;
}
