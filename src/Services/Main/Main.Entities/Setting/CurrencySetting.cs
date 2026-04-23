using Domain.CommonEntities;
using Domain.Interfaces;
using Enums;

namespace Main.Abstractions.Models.Settings;

public class CurrencySetting : Setting<CurrencySettingData>, ISettingKey<CurrencySetting>
{
    public static string SettingName => "CurrencySetting";
    public CurrencySetting(string json) : base(SettingName, json) { }

    public CurrencySetting(CurrencySettingData data) : base(SettingName, data) { }
}

public record CurrencySettingData
{
    public int DefaultCurrencyId { get; init; } = 1;
    public bool AutoUpdateRates { get; init; } = true;
    public ExchangeRateProvider RateProvider { get; init; } = ExchangeRateProvider.Cbr;
}