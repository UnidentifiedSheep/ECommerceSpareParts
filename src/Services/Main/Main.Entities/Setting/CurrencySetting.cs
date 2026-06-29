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
    public static CurrencySetting Default { get; } = new(new CurrencySettingData());
}

public record CurrencySettingData
{
    public int BaseCurrencyId { get; init; } = 1;
    public ExchangeRateProvider RateProvider { get; init; } = ExchangeRateProvider.Cbr;
}